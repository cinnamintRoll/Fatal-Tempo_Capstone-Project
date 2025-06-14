using BNG;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

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
    [SerializeField] private AudioSource audioSource;
    private float originalFixedDelta;
    [SerializeField] private UIPointer pointer;
    [SerializeField] private float heightOffset;
    [SerializeField] private VideoPlayer player;
    [SerializeField] ScreenFader screenFader;
    [Tooltip("If true, will set Time.fixedDeltaTime to the device refresh rate")]
    public bool SetFixedDelta = false;

    public enum MenuType { None, Pause, Death }
    private MenuType activeMenu = MenuType.None;

    // Unity Events for showing and hiding menus
    public UnityEvent onShowPauseMenu;
    public UnityEvent onHidePauseMenu;
    public UnityEvent onShowDeathMenu;
    public UnityEvent onHideDeathMenu;

    private void Start()
    {
        if (SetFixedDelta)
        {
            Time.fixedDeltaTime = (Time.timeScale / UnityEngine.XR.XRDevice.refreshRate);
        }
        audioSource.ignoreListenerPause = true;
        playerHead = Camera.main.transform;

        // Ensure menus are initially disabled
        pauseMenuUI.SetActive(false);
        deathMenuUI.SetActive(false);

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
        originalFixedDelta = Time.fixedDeltaTime;

        if (player != null)
        {
            player.loopPointReached += OnDeathVideoEnd;
        }
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

    private void OnDeathVideoEnd(VideoPlayer vp)
    {
        StartCoroutine(WaitAndReturnToMainMenu());
    }

    private IEnumerator WaitAndReturnToMainMenu()
    {
        screenFader.DoFadeIn();

        yield return new WaitForSecondsRealtime(2f); // Waits 2 seconds in real time even if timeScale is 0

        ReturnToMainMenu();
    }

    private void OnDestroy()
    {
        if (player != null)
        {
            player.loopPointReached -= OnDeathVideoEnd;
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
                onShowPauseMenu?.Invoke();  // Trigger event when showing the pause menu
                break;

            case MenuType.Death:
                deathMenuUI.SetActive(true);
                MoveMenuToPlayer(deathMenuUI);
                PlayAudio(deathClip);
                activeMenu = MenuType.Death;
                onShowDeathMenu?.Invoke();  // Trigger event when showing the death menu

                if (player != null)
                {
                    player.Play();
                }
                break;
        }
        if (pointer != null)
        {
            pointer.HidePointerIfNoObjectsFound = false;
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
            onHidePauseMenu?.Invoke();  // Trigger event when hiding the pause menu
        }
        else if (activeMenu == MenuType.Death)
        {
            deathMenuUI.SetActive(false);
            onHideDeathMenu?.Invoke();  // Trigger event when hiding the death menu
        }
        if (pointer != null)
        {
            pointer.HidePointerIfNoObjectsFound = true;
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
        SceneManager.LoadScene("Level Select");
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
