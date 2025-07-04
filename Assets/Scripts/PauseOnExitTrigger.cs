using static GameMenu;
using UnityEngine;

public class PauseOnExitTrigger : MonoBehaviour
{
    public string playerTag = "Player";
    [SerializeField] private GameMenu gameMenu;
    [SerializeField] private GameObject warningUI;
    [SerializeField] private float startDelay = 3f; // Time in seconds to delay at song start

    private Collider triggerCollider;
    private GameObject player;
    private bool warningShown = false;
    private bool wasInside = false;
    private float timeSinceStart = 0f;
    private bool delayPassed = false;

    void Start()
    {
        triggerCollider = GetComponent<Collider>();
        player = GameObject.FindGameObjectWithTag(playerTag);

        timeSinceStart = 0f;
        delayPassed = false;
    }

    void Update()
    {
        if (!delayPassed)
        {
            timeSinceStart += Time.unscaledDeltaTime; // Use unscaled time in case game starts paused
            if (timeSinceStart >= startDelay)
            {
                delayPassed = true;
            }
            else
            {
                return; // Don't run logic yet
            }
        }

        if (player == null || triggerCollider == null) return;

        bool isInside = triggerCollider.bounds.Contains(player.transform.position);

        if (isInside && !wasInside && warningShown)
        {
            if (warningUI != null)
                warningUI.SetActive(false);

            gameMenu.ShowMenu(MenuType.Pause);
            gameMenu.disableInput = false;
            warningShown = false;
            Debug.Log("Player returned, showing pause menu.");
        }
        else if (!isInside && wasInside && !warningShown)
        {
            ShowWarning();
        }

        wasInside = isInside;
    }

    private void ShowWarning()
    {
        if (warningUI != null)
            warningUI.SetActive(true);
        gameMenu.disableInput = true;
        Time.timeScale = 0f;
        warningShown = true;
        Debug.Log("Warning shown. Game paused.");
    }
}
