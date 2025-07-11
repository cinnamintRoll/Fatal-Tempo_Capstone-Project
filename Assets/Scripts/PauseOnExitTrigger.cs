using static GameMenu;
using UnityEngine;

public class PauseOnExitTrigger : MonoBehaviour
{
    public string playerTag = "Player";
    [SerializeField] private GameMenu gameMenu;
    [SerializeField] private GameObject warningUI;
    [SerializeField] private float startDelay = 3f;

    private Collider triggerCollider;
    private GameObject player;
    private bool warningActive = false;
    private float timer;

    private void Start()
    {
        triggerCollider = GetComponent<Collider>();
        player = GameObject.FindGameObjectWithTag(playerTag);
        timer = 0f;
    }

    private void Update()
    {
        // Wait for the start delay before checking trigger logic
        if (timer < startDelay)
        {
            timer += Time.unscaledDeltaTime;
            return;
        }

        if (player == null || triggerCollider == null) return;

        bool playerInside = triggerCollider.bounds.Contains(player.transform.position);

        if (!playerInside && !warningActive)
        {
            ShowWarning();
        }
        else if (playerInside && warningActive)
        {
            HideWarningAndPause();
        }
    }

    private void ShowWarning()
    {
        if (warningUI != null)
            warningUI.SetActive(true);

        gameMenu.disableInput = true;
        Time.timeScale = 0f;
        warningActive = true;
        Debug.Log("Player exited. Warning shown and game paused.");
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
