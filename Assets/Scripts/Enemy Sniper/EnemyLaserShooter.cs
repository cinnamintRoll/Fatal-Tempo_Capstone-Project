using System.Collections;
using UnityEngine;

public class EnemyLaserShooter : MonoBehaviour
{
    [SerializeField] private Transform player; // Reference to the player
    [SerializeField] private Transform gunMuzzle; // Position where the laser starts (muzzle of the gun)
    [SerializeField] private LineRenderer laser; // Laser LineRenderer
    [SerializeField] private float laserRange = 50f; // Maximum range of the laser
    [SerializeField] private float aimDuration = 2f; // Time before shooting
    [SerializeField] private float playerDamage = 10f; // Damage to apply to player
    [SerializeField] private AudioClip deflectionSound; // Sound to play on deflection
    [SerializeField] private AudioClip chargeSound; // Sound to play while charging
    [SerializeField] private AudioClip shootSound; // Sound to play when shooting
    [SerializeField] private AudioSource audioSource; // AudioSource component reference
    [SerializeField] private GameObject visuals;
    [SerializeField] private float offsetAmount = 0.5f; // Set the offset distance (adjust this value as needed)
    [SerializeField] private Color laserColorCharged = Color.red; // Color of the charged laser
    [SerializeField] private Color laserColorNormal = Color.green; // Color of the normal laser
    [SerializeField] private Color shootBeamColor = Color.white; // Color of the beam when shooting
    [SerializeField] private Vector3 aimRotationOffset; // Rotation offset for aiming at the player
    private bool isDeflected = false;
    [SerializeField] private float initialLaserWidth = 0.05f; // Initial width of the laser
    [SerializeField] private float chargeLaserWidth = 0.2f; // Maximum width during the charge-up phase
    [SerializeField] private float shootLaserWidth = 0.4f; // Width of the white beam during shooting
    [SerializeField] private float bigBeamDuration = 0.2f; // Duration of the big beam effect
    [SerializeField] private float aimDelay = 1f;
    [SerializeField] private LayerMask deflectLayer;
    private bool isShooting = false;
    private Vector3 laserOffset; // Fixed laser offset

    // Variables for the capsule cast
    [SerializeField] private float capsuleRadius = 0.5f; // Radius of the capsule
    [SerializeField] private float capsuleHeight = 2f; // Height of the capsule

    void Start()
    {
        if (!player)
        {
            SetPlayer(Camera.main.transform.gameObject);
        }
        // Initialize the laser appearance
        laser.startColor = laserColorNormal; // Set the initial color of the laser
        laser.endColor = laserColorNormal;
    }

    private void Update()
    {
        if (visuals == null)
        {
            DestroyObject();
        }
    }

    public void StartShooting()
    {
        if (this)
            StartCoroutine(StartAimingAfterDelay());
    }

    void SetPlayer(GameObject playerObject)
    {
        player = playerObject.transform;
    }

    private IEnumerator StartAimingAfterDelay()
    {
        yield return new WaitForSeconds(aimDelay); // Wait for the delay
        StartCoroutine(AimAndShoot()); // Start aiming and shooting after the delay
    }

    private IEnumerator AimAndShoot()
    {
        while (true) // Continuously aim and shoot
        {
            if (Time.timeScale == 0)
            {
                yield return null; // Wait until the game resumes
                continue; // Skip this iteration
            }

            AimAtPlayer();
            UpdateLaser();
            CheckForDeflection(); // Check for deflection using capsule cast
            if (!isShooting && !isDeflected)
            {
                StartCoroutine(ShootAfterDelay());
            }
            yield return null; // Wait for the next frame
        }
    }

    // Aim the enemy towards the player with an offset
    void AimAtPlayer()
    {
        if (player != null)
        {
            Vector3 direction = (player.position - transform.position).normalized;

            // Rotate the direction by the specified offset
            Quaternion offsetRotation = Quaternion.Euler(aimRotationOffset);
            direction = offsetRotation * direction;

            // Rotate the enemy object to look at the modified direction
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = lookRotation; // Snap to the new rotation immediately
        }
    }

    // Update the laser to point towards the player
    void UpdateLaser()
    {
        if (gunMuzzle)
        {
            laser.SetPosition(0, gunMuzzle.position); // Set the start position of the laser to the gun muzzle

            // Laser direction aiming straight forward from the gun muzzle
            Vector3 laserDirection = gunMuzzle.forward; // Use the forward direction
            laser.SetPosition(1, gunMuzzle.position + laserDirection * laserRange);
        }
    }

    // Coroutine to shoot after a delay
    IEnumerator ShootAfterDelay()
    {
        isShooting = true;
        if (!isDeflected)
        {
            // Start playing the charging sound on loop
            audioSource.clip = chargeSound;
            audioSource.loop = true;
            audioSource.Play();

            // Charge up effect
            float chargeElapsed = 0f;
            while (chargeElapsed < aimDuration)
            {
                chargeElapsed += Time.deltaTime;
                float t = chargeElapsed / aimDuration;

                // Adjust laser color based on charge
                laser.startColor = Color.Lerp(laserColorNormal, laserColorCharged, t);
                laser.endColor = Color.Lerp(laserColorNormal, laserColorCharged, t);

                // Gradually increase the laser width during the charge-up phase
                float currentWidth = Mathf.Lerp(initialLaserWidth, chargeLaserWidth, t);
                laser.startWidth = currentWidth;
                laser.endWidth = currentWidth;

                yield return null; // Wait for the next frame
            }

            // Stop the charging sound before shooting
            if (!isDeflected)
            {
                audioSource.PlayOneShot(shootSound);
                ShootPlayer();
            }

            // White shooting beam with animated expansion and contraction
            yield return StartCoroutine(AnimateShootBeam());

            // Now shoot the laser (apply damage or any action here)

            // Reset laser width and color back to normal after shooting
            laser.startWidth = initialLaserWidth;
            laser.endWidth = initialLaserWidth;
            laser.startColor = laserColorNormal;
            laser.endColor = laserColorNormal;

            isShooting = false;
        }
        else if (isDeflected)
        {
            audioSource.Stop();
        }
    }

    // Function to apply damage to the player
    void ShootPlayer()
    {
        PlayerHealth.Instance.TakeDamage();
    }

    // Method to deflect the shot
    public void DeflectShot(Vector3 deflectPoint)
    {
        isDeflected = true;
        Debug.Log("Shot deflected! Enemy killed.");
        laser.enabled = false;
        Destroy(visuals);

        // Play deflection sound
        AudioSource.PlayClipAtPoint(deflectionSound, deflectPoint);

        // Destroy the enemy object after a short delay for sound to play
        Destroy(gameObject, 0.5f);
    }

    private void DestroyObject()
    {
        Destroy(gameObject, 0.5f);
    }

    // Coroutine to animate the laser beam shooting effect
    IEnumerator AnimateShootBeam()
    {
        float expandTime = 0.1f; // Time to expand the beam
        float contractTime = 0.1f; // Time to contract the beam

        // Expand phase
        float elapsedTime = 0f;
        while (elapsedTime < expandTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / expandTime;

            // Increase laser width to shootLaserWidth
            float currentWidth = Mathf.Lerp(chargeLaserWidth, shootLaserWidth, t);
            laser.startWidth = currentWidth;
            laser.endWidth = currentWidth;

            laser.startColor = shootBeamColor; // Set the beam color to white
            laser.endColor = shootBeamColor;

            yield return null;
        }

        yield return new WaitForSeconds(bigBeamDuration); // Hold the big beam for a short duration

        // Contract phase
        elapsedTime = 0f;
        while (elapsedTime < contractTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / contractTime;

            // Reduce laser width back to chargeLaserWidth
            float currentWidth = Mathf.Lerp(shootLaserWidth, chargeLaserWidth, t);
            laser.startWidth = currentWidth;
            laser.endWidth = currentWidth;

            yield return null;
        }
    }

    // Function to check for deflection using a capsule cast
    private void CheckForDeflection()
    {
        Vector3 capsuleStart = gunMuzzle.position;
        Vector3 capsuleEnd = gunMuzzle.position + gunMuzzle.forward * capsuleHeight;

        // Perform the capsule cast
        RaycastHit hit;
        bool hitDeflectable = Physics.CapsuleCast(capsuleStart, capsuleEnd, capsuleRadius, gunMuzzle.forward, out hit, laserRange, deflectLayer);

        // Check if a deflectable object was hit
        if (hitDeflectable)
        {
            if (!isDeflected)
            {
                DeflectShot(hit.point);
                isDeflected = true;
            }
        }
    }
}
