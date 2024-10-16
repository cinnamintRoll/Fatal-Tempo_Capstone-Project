using UnityEngine;
using System.Collections;
using BNG;
using UnityEngine.Events;
using TMPro;

public class AutoAimGun : MonoBehaviour
{
    // Firing modes
    public enum FiringMode { SemiAuto, FullAuto }
    public FiringMode currentFiringMode = FiringMode.SemiAuto;

    // Hand side (Left or Right)

    public BNG.ControllerHand HandSide = ControllerHand.Right;

    // Public variables for customization
    public float aimRange = 50f;
    public float aimAssistAngle = 15f;
    public float shootRange = 100f;
    public LayerMask enemyLayer;
    public Transform gunBarrel;
    public ParticleSystem muzzleFlash;
    public AudioSource muzzleSound;
    public AudioClip GunShotSound;
    public float GunShotVolume = 1f;
    public AudioClip EmptySound;
    public float EmptyVolume = 1f;
    public AudioSource ReloadSound;
    public AudioClip GunReloadSound;
    public float ReloadVolume = 1f;
    public int InternalAmmo = 30;
    public int MaxInternalAmmo = 30;
    public float RecoilDuration = 0.1f;
    public Vector3 recoilAmount = new Vector3(0.05f, 0.05f, -0.1f);
    public Vector3 recoilTiltAmount = new Vector3(-5f, 2f, 0); // Tilt rotation for recoil (x for pitch, y for yaw, z for roll)
    private Quaternion originalRotation;
    public float triggerThreshold = 0.5f;
    private Transform closestEnemy;
    private bool canFire = true; // Semi-auto control
    private InputBridge input;
    [SerializeField] private float bulletDamage = 1f;
    public float fullAutoFireRate = 0.2f;
    private float nextFireTime = 0f;
    public float reloadAngleThreshold = 60f;
    public float reloadCooldown = 2f;
    private float nextReloadTime = 0f;
    private Vector3 originalPosition;
    private Coroutine recoilCoroutine;
    private bool hasReloaded = false;
    [SerializeField] private TMP_Text ammoText;
    public LineRenderer bulletTrailPrefab;
    public float trailDuration = 0.5f;
    [SerializeField] private Animator gunAnimator;

    public UnityEvent onShootEvent;
    public UnityEvent onReloadEvent;

    private PlayerHealth PlayerHealth;
    // Animator component

    private void Awake()
    {
        input = InputBridge.Instance;
        originalPosition = transform.localPosition;
        originalRotation = transform.localRotation; // Store the original rotation
        UpdateAmmoCounter();
        PlayerHealth = PlayerHealth.Instance;
    }

    void Update()
    {
        if (currentFiringMode == FiringMode.SemiAuto)
        {
            HandleSemiAutoFire();
        }
        else if (currentFiringMode == FiringMode.FullAuto)
        {
            HandleFullAutoFire();
        }

        // Check for reloading by pointing gun up or down
        CheckForReload();
    }

    bool IsTriggerPressed()
    {
        // Detect trigger press based on current hand side
        if (HandSide == ControllerHand.Right)
        {
            return InputBridge.Instance.RightTrigger > triggerThreshold;
        }
        else
        {
            return InputBridge.Instance.LeftTrigger > triggerThreshold;
        }
    }

    void HandleSemiAutoFire()
    {
        if (IsTriggerPressed() && canFire)
        {
            FindClosestEnemy();
            Fire();
            canFire = false;
        } 
        else if (!IsTriggerPressed())
        {
            canFire = true;
        }
    }

    void HandleFullAutoFire()
    {
        if (IsTriggerPressed() && Time.time >= nextFireTime)
        {
            FindClosestEnemy();
            Fire();
            nextFireTime = Time.time + fullAutoFireRate;
        }
    }

    void CheckForReload()
    {
        float angleUp = Vector3.Angle(gunBarrel.forward, Vector3.up);
        float angleDown = Vector3.Angle(gunBarrel.forward, Vector3.down);

        if ((angleUp <= reloadAngleThreshold || angleDown <= reloadAngleThreshold) && !hasReloaded && Time.time >= nextReloadTime)
        {
            Reload();
            hasReloaded = true;
            nextReloadTime = Time.time + reloadCooldown;
        }
        else if (angleUp > reloadAngleThreshold && angleDown > reloadAngleThreshold)
        {
            hasReloaded = false;
        }
    }

    void FindClosestEnemy()
    {
        closestEnemy = null;
        Collider[] enemiesInRange = Physics.OverlapSphere(gunBarrel.position, aimRange, enemyLayer);
        float closestAngle = Mathf.Infinity;

        foreach (var enemy in enemiesInRange)
        {
            Vector3 directionToEnemy = enemy.transform.position - gunBarrel.position;
            float angle = Vector3.Angle(gunBarrel.forward, directionToEnemy);

            if (angle < aimAssistAngle && angle < closestAngle)
            {
                closestAngle = angle;
                closestEnemy = enemy.transform;
            }
        }
    }

    void Fire()
    {
        if(InternalAmmo <= 3)
        {
            muzzleSound.PlayOneShot(EmptySound, EmptyVolume);
        }
        if (InternalAmmo <= 0)
        {
            
            if (gunAnimator)
            {
                gunAnimator.SetBool("Empty",true); // Play empty animation if no ammo
                gunAnimator.SetTrigger("Shoot");
            }
            return;
        }

        // Trigger the shoot animation
        gunAnimator.SetTrigger("Shoot");

        Vector3 shootDirection = (closestEnemy != null)
            ? (closestEnemy.position - gunBarrel.position).normalized
            : gunBarrel.forward;

        RaycastHit hit;
        Vector3 hitPoint = gunBarrel.position + shootDirection * shootRange;

        if (Physics.Raycast(gunBarrel.position, shootDirection, out hit, shootRange))
        {
            if (((1 << hit.collider.gameObject.layer) & enemyLayer) != 0)
            {
                Damageable damageEnemy = hit.collider.gameObject.GetComponent<Damageable>();
                if (damageEnemy != null)
                {
                    damageEnemy.DealDamage(bulletDamage);
                    
                }
                if (PlayerHealth)
                    PlayerHealth.KillEnemy();
            }
            hitPoint = hit.point;
        }

        CreateBulletTrail(gunBarrel.position, hitPoint);

        if (muzzleFlash != null)
        {
            muzzleFlash.Play();
        }

        if (GunShotSound != null && muzzleSound != null)
        {
            muzzleSound.PlayOneShot(GunShotSound, GunShotVolume);
        }

        ApplyRecoil();

        if (onShootEvent != null)
        {
            onShootEvent.Invoke();
        }

        InternalAmmo--;
        UpdateAmmoCounter();
        if (InternalAmmo <= 0)
        {
            if (gunAnimator)
            {
                gunAnimator.SetBool("Empty", true); // Set empty animation when ammo is depleted
            }
        }
    }

    void CreateBulletTrail(Vector3 startPoint, Vector3 endPoint)
    {
        LineRenderer bulletTrail = Instantiate(bulletTrailPrefab);
        bulletTrail.useWorldSpace = true;
        bulletTrail.SetPosition(0, startPoint);
        bulletTrail.SetPosition(1, endPoint);
        StartCoroutine(FadeBulletTrail(bulletTrail));
    }

    IEnumerator FadeBulletTrail(LineRenderer bulletTrail)
    {
        float duration = 0.5f;
        float startWidth = bulletTrail.startWidth;
        float endWidth = bulletTrail.endWidth;

        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            float lerpFactor = t / duration;
            bulletTrail.startWidth = Mathf.Lerp(startWidth, 0f, lerpFactor);
            bulletTrail.endWidth = Mathf.Lerp(endWidth, 0f, lerpFactor);
            yield return null;
        }

        Destroy(bulletTrail.gameObject);
    }

    private void ApplyRecoil()
    {
        if (recoilCoroutine != null)
        {
            StopCoroutine(recoilCoroutine);
        }

        recoilCoroutine = StartCoroutine(RecoilAnimation());

        input.VibrateController(0.5f, 1f, 0.05f, HandSide);
    }

    private IEnumerator RecoilAnimation()
    {
        Vector3 recoilPosition = originalPosition + recoilAmount;
        Quaternion recoilRotation = originalRotation * Quaternion.Euler(recoilTiltAmount); // Calculate the tilt rotation

        float recoilTime = 0;
        while (recoilTime < RecoilDuration)
        {
            recoilTime += Time.deltaTime;
            transform.localPosition = Vector3.Lerp(originalPosition, recoilPosition, recoilTime / RecoilDuration);
            transform.localRotation = Quaternion.Lerp(originalRotation, recoilRotation, recoilTime / RecoilDuration); // Tilt the gun during recoil
            yield return null;
        }

        recoilTime = 0;
        while (recoilTime < RecoilDuration)
        {
            recoilTime += Time.deltaTime;
            transform.localPosition = Vector3.Lerp(recoilPosition, originalPosition, recoilTime / RecoilDuration);
            transform.localRotation = Quaternion.Lerp(recoilRotation, originalRotation, recoilTime / RecoilDuration); // Return the gun back to its original rotation
            yield return null;
        }

        transform.localPosition = originalPosition;
        transform.localRotation = originalRotation; // Ensure the gun returns to its exact original rotation
    }

    public void Reload()
    {
        if (InternalAmmo != MaxInternalAmmo)
        {
            if (gunAnimator)
            {
                gunAnimator.SetBool("Empty", false);
            }
            InternalAmmo = MaxInternalAmmo;
            UpdateAmmoCounter();
            if (GunReloadSound != null && ReloadSound != null)
            {
                ReloadSound.PlayOneShot(GunReloadSound, ReloadVolume);
            }

            if (onReloadEvent != null)
            {
                onReloadEvent.Invoke();
            }
        }
    }

    public void UpdateAmmoCounter()
    {
        if (ammoText != null)
        {
            ammoText.text = $"{InternalAmmo}";
        }
    }
}
