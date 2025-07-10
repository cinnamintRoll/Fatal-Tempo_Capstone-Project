using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class EnemyLaserShooter : MonoBehaviour
{
    [SerializeField] private Transform player; // Reference to the player
    [SerializeField] private Transform gunMuzzle; // Position where the laser starts
    [SerializeField] private LineRenderer laser; // Laser LineRenderer
    [SerializeField] private float laserRange = 50f; // Maximum range of the laser
    [SerializeField] private float playerDamage = 10f; // Damage to apply to player
    [SerializeField] private AudioClip deflectionSound; // Sound to play on deflection
    [SerializeField] private AudioClip chargeSound; // Sound to play while charging
    [SerializeField] private AudioClip shootSound; // Sound to play when shooting
    [SerializeField] private AudioSource audioSource; // AudioSource component reference
    [SerializeField] private GameObject visuals;
    [SerializeField] private float offsetAmount = 0.5f; // Set the offset distance
    [SerializeField] private Color laserColorCharged = Color.red; // Color of the charged laser
    [SerializeField] private Color laserColorNormal = Color.green; // Color of the normal laser
    [SerializeField] private Color shootBeamColor = Color.white; // Color of the beam when shooting
    [SerializeField] private Vector3 aimRotationOffset; // Rotation offset for aiming at the player
    [SerializeField] private float DeflectSpeedThreshold = 3f;
    private bool isDeflected = false;
    [SerializeField] private float initialLaserWidth = 0.05f; // Initial width of the laser
    [SerializeField] private float chargeLaserWidth = 0.2f; // Maximum width during the charge-up phase
    [SerializeField] private float shootLaserWidth = 0.4f; // Width of the white beam during shooting
    [SerializeField] private float bigBeamDuration = 0.2f; // Duration of the big beam effect
    [SerializeField] private float aimDelay = 1f;
    [SerializeField] private LayerMask deflectLayer;
    private bool isShooting = false;
    private Vector3 laserOffset; 
    [SerializeField] private EnemyAI AIScript;
    [SerializeField] private int beatsBetweenShoots = 3;
    private bool isCharging = false;
    [SerializeField] private GameObject BulletCollider;
    [SerializeField] private bool loopShooting = true;
    private bool hasLooped = false;
    private bool shouldStop = false;
    [Header("Aiming Options")]
    [SerializeField] private bool rotateModelHorizontally = true;
    [SerializeField] private bool rotateModelVertically = false;

    public UnityEvent OnDeflect;
    private MusicManager musicManager;

    void Start()
    {
        musicManager = MusicManager.Instance;

        if (!player)
        {
            SetPlayer(Camera.main.transform.gameObject);
        }

        laser.startColor = laserColorNormal; 
        laser.endColor = laserColorNormal;

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

        if (BulletCollider == null)
        {
            OnBulletColliderDestroyed();
        }
    }


    private IEnumerator StartShootingRoutine()
    {
        yield return new WaitForSeconds(aimDelay); 
        while (true)
        {
            if (Time.timeScale == 0)
            {
                yield return null; 
                continue; 
            }

            AimAtPlayer();
            UpdateLaser();



            if (!isShooting)
            {
                if (!loopShooting && hasLooped)
                    yield break;

                yield return StartCoroutine(ChargeAndShootLaser());
            }


            yield return null;
        }
    }

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
                if (horizontalDirection == Vector3.zero) return; 

                Quaternion lookRotation = Quaternion.LookRotation(horizontalDirection);
                Quaternion desiredHorizontalRotation = Quaternion.Euler(0, lookRotation.eulerAngles.y + aimRotationOffset.y, 0);
                transform.rotation = Quaternion.Slerp(currentRotation, desiredHorizontalRotation, Time.deltaTime * 5f); 
            }
        }
    }
    void UpdateLaser()
    {
        if (gunMuzzle && player)
        {
            laser.SetPosition(0, gunMuzzle.position);
            Vector3 laserDirection = (player.position - gunMuzzle.position).normalized;

            Quaternion offsetRotation = Quaternion.Euler(aimRotationOffset);
            laserDirection = offsetRotation * laserDirection;

            laser.SetPosition(1, gunMuzzle.position + laserDirection * laserRange);
        }
    }

    private IEnumerator ChargeAndShootLaser()
    {
        if (isDeflected) yield break;

        isShooting = true;
        isCharging = true;
        int chargeBeats = beatsBetweenShoots;
        int beatCount = 0;
        bool canDeflectWindow = false;
        bool finalPulseStarted = false;

        audioSource.clip = chargeSound;
        audioSource.Play();

        void OnShootBeat()
        {
            if (isDeflected || laser == null) return;

            beatCount++;

            if (beatCount >= chargeBeats)
            {
                if (!finalPulseStarted)
                {
                    finalPulseStarted = true;
                    StartCoroutine(FinalPulseAndShoot(() =>
                    {
                        canDeflectWindow = false;
                        OnShootEnd();
                    }));
                }
            }
            else
            {
                float progress = (float)beatCount / (chargeBeats - 1);
                progress = Mathf.Clamp01(progress); 

                Color interpolatedPulseColor = Color.Lerp(laserColorNormal, laserColorCharged, progress);

                StartCoroutine(PulseLaserWidth(
                    chargeLaserWidth,
                    initialLaserWidth,
                    interpolatedPulseColor,
                    laserColorNormal,
                    0.25f));
            }
        }

        musicManager.OnIntervalPassed.AddListener(OnShootBeat);

        while (!finalPulseStarted)
        {
            AimAtPlayer();
            UpdateLaser();
            yield return null;
        }

        musicManager.OnIntervalPassed.RemoveListener(OnShootBeat);

        isCharging = false;
        isShooting = false;
        hasLooped = true;
    }



    private IEnumerator PulseLaserWidth(float pulseOutWidth, float returnWidth, Color pulseColor, Color baseColor, float duration)
    {
        float halfTime = duration / 2f;
        float t = 0f;
        while (t < halfTime)
        {
            float lerp = t / halfTime;
            float width = Mathf.Lerp(initialLaserWidth, pulseOutWidth, lerp);
            Color color = Color.Lerp(baseColor, pulseColor, lerp);
            ApplyLaserVisuals(width, color);
            t += Time.deltaTime;
            yield return null;
        }

        t = 0f;
        while (t < halfTime)
        {
            float lerp = t / halfTime;
            float width = Mathf.Lerp(pulseOutWidth, returnWidth, lerp);
            Color color = Color.Lerp(pulseColor, baseColor, lerp);
            ApplyLaserVisuals(width, color);
            t += Time.deltaTime;
            yield return null;
        }
    }

    private void ApplyLaserVisuals(float width, Color color)
    {
        if (laser != null)
        {
            laser.startWidth = width;
            laser.endWidth = width;
            laser.startColor = color;
            laser.endColor = color;
        }
    }


    public void StartShooting()
    {
        Debug.LogWarning("Beam start shooting");
        StartCoroutine(StartShootingRoutine());
    }

    void ShootPlayer()
    {
        PlayerHealth.Instance.TakeDamage();
    }

    public void DeflectShot(Vector3 deflectPoint)
    {
        
        isDeflected = true;
        OnDeflect.Invoke();
        laser.enabled = false;
        AudioSource.PlayClipAtPoint(deflectionSound, deflectPoint);
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

    public void OnBulletColliderDestroyed()
    {
        shouldStop = true;
        StopAllCoroutines();
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

    private IEnumerator FinalPulseAndShoot(System.Action onFinish)
    {
        float expandTime = 0.1f;
        float holdTime = bigBeamDuration;
        float contractTime = 0.1f;
        float elapsed;

        audioSource.PlayOneShot(shootSound);

        elapsed = 0f;
        while (elapsed < expandTime)
        {
            if (CheckDeflectionDuringFinalPhase()) yield break;

            float t = elapsed / expandTime;
            ApplyLaserVisuals(Mathf.Lerp(chargeLaserWidth, shootLaserWidth, t), shootBeamColor);
            elapsed += Time.deltaTime;
            yield return null;
        }

        float holdEndTime = Time.time + holdTime;
        while (Time.time < holdEndTime)
        {
            if (CheckDeflectionDuringFinalPhase()) yield break;

            ApplyLaserVisuals(shootLaserWidth, shootBeamColor);
            yield return null;
        }

        elapsed = 0f;
        float startWidth = shootLaserWidth;
        float endWidth = loopShooting ? chargeLaserWidth : 0f;

        if (!isDeflected)
        {
            ShootPlayer();
        }
        while (elapsed < contractTime)
        {
            float t = elapsed / contractTime;
            ApplyLaserVisuals(Mathf.Lerp(startWidth, endWidth, t), shootBeamColor);
            elapsed += Time.deltaTime;
            yield return null;
        }

        ApplyLaserVisuals(initialLaserWidth, laserColorNormal);

        onFinish?.Invoke();
    }

    private bool CheckDeflectionDuringFinalPhase()
    {
        if (isDeflected || shouldStop || gunMuzzle == null || player == null) return false;

        Vector3 origin = gunMuzzle.position;
        Vector3 laserDirection = (player.position - gunMuzzle.position).normalized;
        laserDirection = Quaternion.Euler(aimRotationOffset) * laserDirection;

        if (Physics.SphereCast(origin, 0.05f, laserDirection, out RaycastHit hit, laserRange, deflectLayer))
        {
            SliceObject sliceObj = hit.collider?.GetComponent<SliceObject>();
            if (sliceObj != null)
            {
                float speed = sliceObj.GetSwordVelocity().magnitude;
                if (speed >= DeflectSpeedThreshold)
                {
                    DeflectShot(hit.point);
                    isDeflected = true;
                    return true;
                }
            }
        }

        return false;
    }


    private void OnShootEnd()
    {
        if (!loopShooting)
        {
            laser.enabled = false;
            isCharging = false;
            isShooting = false;
            hasLooped = true;
            shouldStop = true;
        }
    }

}