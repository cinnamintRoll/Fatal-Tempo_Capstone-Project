using static GameMenu;
using UnityEngine;

public class PauseOnExitTrigger : MonoBehaviour
{
    public string playerTag = "Player";
    [SerializeField] private GameMenu gameMenu;
    [SerializeField] private GameObject warningUI;
    [SerializeField] private float startDelay = 3f;
    [SerializeField] private float exitTimeThreshold = 1f;

    private Collider triggerCollider;
    private GameObject player;
    private bool warningActive = false;
    private float timer;
    private float outsideTimer = 0f;

    private void Start()
    {
        triggerCollider = GetComponent<Collider>();
        player = GameObject.FindGameObjectWithTag(playerTag);
        timer = 0f;
    }

    private void Update()
    {
        if (timer < startDelay)
        {
            timer += Time.unscaledDeltaTime;
            return;
        }

        if (player == null || triggerCollider == null) return;

        bool playerInside = triggerCollider.bounds.Contains(player.transform.position);

        if (!playerInside)
        {
            outsideTimer += Time.unscaledDeltaTime;

            if (outsideTimer >= exitTimeThreshold && !warningActive)
            {
                ShowWarning();
            }
        }
        else
        {
            outsideTimer = 0f;

            if (warningActive)
            {
                HideWarningAndPause();
            }
        }
    }

    private void ShowWarning()
    {
        if (warningUI != null)
            warningUI.SetActive(true);

        gameMenu.disableInput = true;
        Time.timeScale = 0f;
        warningActive = true;
        Debug.Log("Player exited for 1 second. Warning shown and game paused.");
    }

    private void HideWarningAndPause()
    {
        if (warningUI != null)
            warningUI.SetActive(false);

        gameMenu.ShowMenu(MenuType.Pause);
        gameMenu.disableInput = false;
        warningActive = false;
        Debug.Log("Player returned. Warning hidden, showing pause menu.");
    }
}
