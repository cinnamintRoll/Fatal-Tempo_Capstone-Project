using BNG;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMenu : MonoBehaviour
{
    public GameObject pauseMenuUI;  // Assign your pause menu UI object
    public GameObject deathMenuUI;  // Assign your death menu UI object
    public float distanceFromPlayer = 2f;  // Distance the menu should appear in front of the player

    public AudioClip pauseClip;     // Audio clip for pausing
    public AudioClip resumeClip;    // Audio clip for resuming
    public AudioClip deathClip;     // Audio clip for death

    private Transform playerHead;  // Reference to the player's head (e.g., VR camera)
    private bool isPaused = false; // Tracks if the game is paused
    private AudioSource audioSource;
    private float originalFixedDelta;
    [SerializeField] private float heightOffset;

    [Tooltip("If true, will set Time.fixedDeltaTime to the device refresh rate")]
    public bool SetFixedDelta = false;

    public enum MenuType { None, Pause, Death }
    private MenuType activeMenu = MenuType.None;

    private void Start()
    {
        if (SetFixedDelta)
        {
            Time.fixedDeltaTime = (Time.timeScale / UnityEngine.XR.XRDevice.refreshRate);
        }

        playerHead = Camera.main.transform;

        // Ensure menus are initially disabled
        pauseMenuUI.SetActive(false);
        deathMenuUI.SetActive(false);

        audioSource = GetComponent<AudioSource>();
        originalFixedDelta = Time.fixedDeltaTime;
    }

    void Update()
    {
        // Check if the pause button (custom input) is pressed
        if (InputBridge.Instance.AButtonDown)
        {
            if (activeMenu == MenuType.Pause)
            {
                ResumeGame();
            }
            else if (activeMenu == MenuType.None)
            {
                ShowMenu(MenuType.Pause);
            }
        }
    }

    public void ShowMenu(MenuType menuType)
    {
        switch (menuType)
        {
            case MenuType.Pause:
                pauseMenuUI.SetActive(true);
                MoveMenuToPlayer(pauseMenuUI);
                PlayAudio(pauseClip);
                activeMenu = MenuType.Pause;
                break;

            case MenuType.Death:
                deathMenuUI.SetActive(true);
                MoveMenuToPlayer(deathMenuUI);
                PlayAudio(deathClip);
                activeMenu = MenuType.Death;
                break;
        }
        Time.timeScale = 0;
        Time.fixedDeltaTime = originalFixedDelta * Time.timeScale;
    }

    public void ResumeGame()
    {
        // Disable the current menu
        if (activeMenu == MenuType.Pause)
        {
            pauseMenuUI.SetActive(false);
        }
        else if (activeMenu == MenuType.Death)
        {
            deathMenuUI.SetActive(false);
        }

        // Resume the game
        Time.timeScale = 1;
        Time.fixedDeltaTime = originalFixedDelta;

        PlayAudio(resumeClip);

        activeMenu = MenuType.None;
    }

    public void RestartLevel()
    {
        Time.timeScale = 1;
        Time.fixedDeltaTime = originalFixedDelta;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1;
        Time.fixedDeltaTime = originalFixedDelta;
        SceneManager.LoadScene("Main Menu");
    }

    public void TriggerDeathMenu()
    {
        ShowMenu(MenuType.Death);
    }

    void MoveMenuToPlayer(GameObject menuUI)
    {
        // Move the menu to a position in front of the player's head
        Vector3 targetPosition = playerHead.position + playerHead.forward * distanceFromPlayer;

        Quaternion headRotation = Quaternion.Euler(0, playerHead.eulerAngles.y, 0);

        menuUI.transform.position = new Vector3(targetPosition.x, playerHead.position.y + heightOffset, targetPosition.z);
        menuUI.transform.rotation = headRotation;
    }

    void PlayAudio(AudioClip clip)
    {
        if (clip != null)
        {
            audioSource.clip = clip;
            audioSource.Play();
        }
    }
}
