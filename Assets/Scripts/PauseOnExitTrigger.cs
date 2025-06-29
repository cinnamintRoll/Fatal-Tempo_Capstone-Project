using static GameMenu;
using UnityEngine;

public class PauseOnExitTrigger : MonoBehaviour
{
    public string playerTag = "Player";
    [SerializeField] private GameMenu gameMenu;
    [SerializeField] private GameObject warningUI;

    private Collider triggerCollider;
    private GameObject player;
    private bool warningShown = false;
    private bool wasInside = false;

    void Start()
    {
        triggerCollider = GetComponent<Collider>();
        player = GameObject.FindGameObjectWithTag(playerTag);
    }

    void Update()
    {
        if (player == null || triggerCollider == null) return;

        bool isInside = triggerCollider.bounds.Contains(player.transform.position);

        if (isInside && !wasInside && warningShown)
        {
            // Player just re-entered
            if (warningUI != null)
                warningUI.SetActive(false);

            gameMenu.ShowMenu(MenuType.Pause);
            gameMenu.disableInput = false;
            warningShown = false;
            Debug.Log("Player returned, showing pause menu.");
        }
        else if (!isInside && wasInside && !warningShown)
        {
            // Player just exited
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
