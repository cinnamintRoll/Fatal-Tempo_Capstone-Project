using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class EnemyLaserShooter : MonoBehaviour
{
    [SerializeField] private Transform player; // Reference to the player
    [SerializeField] private Transform gunMuzzle; // Position where the laser starts (muzzle of the gun)
    [SerializeField] private LineRenderer laser; // Laser LineRenderer
    [SerializeField] private float laserRange = 50f; // Maximum range of the laser
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
    [SerializeField] private EnemyAI AIScript;
    // Adjustable Beats for Charging and Firing
    [SerializeField] private int beatsBetweenShoots = 3; // Adjustable beats to shoot
    private int currentBeat = 0; // Track the current beat for the laser charge cycle
    private bool isCharging = false;
    [SerializeField] private GameObject BulletCollider;
    [SerializeField] private bool loopShooting = true;
    private bool hasLooped = false;
    private bool shouldStop = false;
    [Header("Aiming Options")]
    [SerializeField] private bool rotateModelHorizontally = true;
    [SerializeField] private bool rotateModelVertically = false;

    public UnityEvent OnDeflect;
    // Reference to the MusicManager
    private MusicManager musicManager;

    void Start()
    {
        musicManager = MusicManager.Instance;

        if (!player)
        {
            SetPlayer(Camera.main.transform.gameObject);
        }

        // Initialize the laser appearance
        laser.startColor = laserColorNormal; // Set the initial color of the laser
        laser.endColor = laserColorNormal;

        // Start the shooting routine
        //StartCoroutine(StartShootingRoutine());
    }

    void SetPlayer(GameObject playerObject)
    {
        player = playerObject.transform;
    }

    private void Update()
    {
        if (visuals == null)
        {
            DestroyObject();
        }

        if (!shouldStop && BulletCollider == null)
        {
            OnBulletColliderDestroyed();
        }

        if (isCharging)
        {
            CheckForDeflection();       
        }
    }


    private IEnumerator StartShootingRoutine()
    {
        yield return new WaitForSeconds(aimDelay); // Wait for the delay before starting
        while (true) // Continuously aim and shoot
        {
            if (Time.timeScale == 0)
            {
                yield return null; // Wait until the game resumes
                continue; // Skip this iteration
            }

            AimAtPlayer();
            UpdateLaser();
            CheckForDeflection();



            if (!isShooting)
            {
                if (!loopShooting && hasLooped)
                    yield break;

                yield return StartCoroutine(ChargeAndShootLaser());
            }


            yield return null;
        }
    }

    // Aim the enemy towards the player with an offset
    void AimAtPlayer()
    {
        if (player != null)
        {

            Vector3 targetDirection = (player.position - transform.position).normalized;
            Quaternion currentRotation = transform.rotation;

            if (rotateModelHorizontally && rotateModelVertically)
            {
                // Rotate both horizontally and vertically
                Quaternion lookRotation = Quaternion.LookRotation(targetDirection);
                transform.rotation = Quaternion.Slerp(currentRotation, lookRotation * Quaternion.Euler(aimRotationOffset), Time.deltaTime * 5f); // Smooth rotation
            }
            else if (rotateModelHorizontally)
            {
                // Only rotate horizontally
                Vector3 horizontalDirection = new Vector3(targetDirection.x, 0, targetDirection.z).normalized;
                if (horizontalDirection == Vector3.zero) return; // Prevent Quaternion.LookRotation from throwing error with zero vector

                Quaternion lookRotation = Quaternion.LookRotation(horizontalDirection);
                Quaternion desiredHorizontalRotation = Quaternion.Euler(0, lookRotation.eulerAngles.y + aimRotationOffset.y, 0);
                transform.rotation = Quaternion.Slerp(currentRotation, desiredHorizontalRotation, Time.deltaTime * 5f); // Smooth rotation
            }
            // If neither model rotation option is selected, the model's rotation remains unchanged
        }
    }

    // Update the laser to point towards the player
    void UpdateLaser()
    {
        if (gunMuzzle && player)
        {
            laser.SetPosition(0, gunMuzzle.position);
            Vector3 laserDirection = (player.position - gunMuzzle.position).normalized;

            // Apply the aimRotationOffset directly to the laser's direction
            Quaternion offsetRotation = Quaternion.Euler(aimRotationOffset);
            laserDirection = offsetRotation * laserDirection;

            laser.SetPosition(1, gunMuzzle.position + laserDirection * laserRange);
        }
    }

    // Coroutine to charge and shoot the laser
    private IEnumerator ChargeAndShootLaser()
    {
        if (!isDeflected)
        {
            isShooting = true;
            isCharging = true;

            int chargeBeats = beatsBetweenShoots;
            int beatCount = 0;

            audioSource.clip = chargeSound;
            audioSource.Play();

            void OnShootBeat()
            {
                if (isDeflected) return; // Skip if deflected mid-charge

                beatCount++;
                float t = (float)beatCount / chargeBeats;
                float targetWidth = Mathf.Lerp(initialLaserWidth, chargeLaserWidth, t);
                Color targetColor = Color.Lerp(laserColorNormal, laserColorCharged, t);
                if (laser == null) return;
                laser.startWidth = targetWidth;
                laser.endWidth = targetWidth;
                laser.startColor = targetColor;
                laser.endColor = targetColor;
            }

            musicManager.OnIntervalPassed.AddListener(OnShootBeat);

            while (beatCount < chargeBeats)
            {
                if (isDeflected)
                {
                    musicManager.OnIntervalPassed.RemoveListener(OnShootBeat);
                    yield break; // Stop coroutine if deflected mid-charge
                }

                AimAtPlayer();
                UpdateLaser();
                CheckForDeflection();

                yield return null;
            }

            musicManager.OnIntervalPassed.RemoveListener(OnShootBeat);

            // 🛑 Final check before shooting
            if (isDeflected)
                yield break;

            // Proceed to shoot
            audioSource.PlayOneShot(shootSound);
            ShootPlayer();

            yield return StartCoroutine(AnimateShootBeam());

            if (loopShooting)
            {
                laser.startWidth = initialLaserWidth;
                laser.endWidth = initialLaserWidth;
            }
            else
            {
                laser.startWidth = 0f;
                laser.endWidth = 0f;
            }

            laser.startColor = laserColorNormal;
            laser.endColor = laserColorNormal;

            isCharging = false;
            isShooting = false;
            hasLooped = true;
        }
    }



    public void StartShooting()
    {
        Debug.LogWarning("Beam start shooting");
        StartCoroutine(StartShootingRoutine());
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
        OnDeflect.Invoke();
        laser.enabled = false;
        AudioSource.PlayClipAtPoint(deflectionSound, deflectPoint);
        /*if (AIScript != null)
        {
            AIScript.TransitionToState(EnemyAI.EnemyState.Death);
        }
        else
        {
            Destroy(visuals);
            
            Destroy(gameObject, 0.5f);
        }*/
    }

    public void OnBulletColliderDestroyed()
    {
        shouldStop = true;
        StopAllCoroutines(); // Stop everything gracefully
        laser.enabled = false;

        if (AIScript != null)
        {
            AIScript.TransitionToState(EnemyAI.EnemyState.Death);
        }
        else
        {
            Destroy(visuals);
            Destroy(gameObject, 0.5f);
        }
    }


    public void DestroyObject()
    {
        laser.enabled = false;
        Destroy(gameObject, 0.2f);
    }

    // Coroutine to animate the laser beam shooting effect
    IEnumerator AnimateShootBeam()
    {
        Debug.LogWarning("Beam start shooting");
        float expandTime = 0.1f;
        float contractTime = 0.1f;

        // Expand phase
        float elapsedTime = 0f;
        while (elapsedTime < expandTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / expandTime;
            float currentWidth = Mathf.Lerp(chargeLaserWidth, shootLaserWidth, t);
            laser.startWidth = currentWidth;
            laser.endWidth = currentWidth;
            laser.startColor = shootBeamColor;
            laser.endColor = shootBeamColor;
            yield return null;
        }

        yield return new WaitForSeconds(bigBeamDuration);

        Debug.LogWarning("BeamBeingShot");
        // Contract phase
        elapsedTime = 0f;
        while (elapsedTime < contractTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / contractTime;
            float currentWidth = Mathf.Lerp(shootLaserWidth, chargeLaserWidth, t);
            laser.startWidth = currentWidth;
            laser.endWidth = currentWidth;
            yield return null;
        }
    }

    // Function to check for deflection
    private void CheckForDeflection()
    {
        if (gunMuzzle == null || player == null) return;

        Vector3 origin = gunMuzzle.position;

        // Get direction with aim offset (same as laser visuals)
        Vector3 laserDirection = (player.position - gunMuzzle.position).normalized;
        Quaternion offsetRotation = Quaternion.Euler(aimRotationOffset);
        laserDirection = offsetRotation * laserDirection;

        RaycastHit hit;
        bool hitDeflectable = Physics.SphereCast(origin, 0.05f, laserDirection, out hit, laserRange, deflectLayer);

        if (hitDeflectable && !isDeflected)
        {
            DeflectShot(hit.point);
            isDeflected = true;
        }
    }

}