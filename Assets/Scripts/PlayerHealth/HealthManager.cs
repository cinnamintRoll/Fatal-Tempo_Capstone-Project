using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class PlayerHealth : MonoBehaviour
{
    public static PlayerHealth Instance { get; private set; }
    [SerializeField] private int maxLives = 3;
    [SerializeField] private int currentLives;
    [SerializeField] private Slider[] lifeSliders;  // Two sliders representing the lives
    [SerializeField] private TMP_Text killsRemainingText;

    [SerializeField] private int killsToRestoreLife = 5;  // Number of kills required to restore one life
    private int currentKillCount;
    [SerializeField] private GameMenu gameMenu;
    private float currentHealth;

    [SerializeField] private Animator DamageAnimator;

    // Audio handling with a single AudioSource and two clips
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip damageClip;
    [SerializeField] private AudioClip healClip;

    [SerializeField] private UnityEvent OnDeath;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;  // Set this as the Singleton instance
        }
        else
        {
            Destroy(gameObject);  // Ensure there's only one instance
        }
    }

    void Start()
    {
        currentLives = maxLives;
        UpdateSliders();
        UpdateKillCountText();
    }

    void UpdateSliders()
    {
        for (int i = 0; i < lifeSliders.Length; i++)
        {
            if (i < currentLives - 1)
            {
                lifeSliders[i].value = 1;  // Full life
            }
            else if (i == currentLives - 1)
            {
                lifeSliders[i].value = currentHealth;  // Current life fills based on health
            }
            else
            {
                lifeSliders[i].value = 0;  // No life
            }
        }
    }

    void UpdateKillCountText()
    {
        killsRemainingText.text = $"{killsToRestoreLife - currentKillCount}";
    }

    // Damage instantly removes a full life
    public void TakeDamage()
    {
        if (currentLives > 0)
        {
            currentLives--;
            currentHealth = 0;  // Instant full bar removal on damage
            currentKillCount = 0;  // Reset kill progress on damage
        }

        if (currentLives <= 0)
        {
            // Handle death logic
            Debug.Log("Player is dead");
            PlayerDeath();
        }

        DamageAnimator.SetTrigger("TakeDamage");

        // Play damage sound effect
        audioSource.PlayOneShot(damageClip);

        UpdateKillCountText();
        UpdateSliders();
    }

    // Killing enemies slowly fills up the current life
    public void KillEnemy()
    {
        if (currentLives < maxLives)
        {
            currentKillCount++;
            UpdateKillCountText();

            // Gradually fill up the current health bar based on kills if we're working on restoring a life
            if (currentKillCount < killsToRestoreLife)
            {
                currentHealth = (float)currentKillCount / killsToRestoreLife;
            }
            else
            {
                RestoreLife();  // Restore a full life once enough kills are made
            }

            UpdateSliders();
        }
    }

    // Restores one full life when enough enemies are killed
    void RestoreLife()
    {
        currentLives++;
        currentKillCount = 0;
        currentHealth = 0;  // Set the newly restored life to full health
        UpdateSliders();
        UpdateKillCountText();

        // Play healing sound effect
        audioSource.PlayOneShot(healClip);
    }

    // Debug methods for the Inspector buttons
    public void DebugTakeDamage()
    {
        TakeDamage();  // Arbitrary damage for full bar removal
    }

    public void DebugKillEnemy()
    {
        KillEnemy();  // Simulate killing an enemy
    }

    public void PlayerDeath()
    {
        gameMenu.TriggerDeathMenu();
        OnDeath.Invoke();
    }
}
