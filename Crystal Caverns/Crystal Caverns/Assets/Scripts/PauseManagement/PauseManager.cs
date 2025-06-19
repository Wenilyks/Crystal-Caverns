using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    [Header("Menu Panels")]
    public GameObject pauseMenuPanel; // This contains both MainPausePanel and SettingsPanel
    public GameObject backgroundBlur;
    public GameObject mainPausePanel; // Left side panel with main buttons
    public GameObject settingsPanel;  // Right side panel with settings

    [Header("Main Menu Buttons")]
    public Button resumeButton;
    public Button settingsButton;
    public Button quitButton;

    [Header("Settings UI")]
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;
    public Toggle musicToggle;
    public Toggle sfxToggle;
    public Button backFromSettingsButton;

    [Header("Visual Effects")]
    public CanvasGroup pauseMenuCanvasGroup;
    public CanvasGroup mainPauseCanvasGroup;
    public CanvasGroup settingsCanvasGroup;
    public float fadeSpeed = 5f;

    private bool isPaused = false;
    private bool settingsOpen = false;
    private PlayerController playerController;
    private Rigidbody2D playerRb;
    private Vector2 pausedVelocity;

    private void Start()
    {
        InitializeUI();
        FindPlayerComponents();
        SetupAudioSliders();

        pauseMenuPanel.SetActive(false);
        mainPausePanel.SetActive(false);
        settingsPanel.SetActive(false);
        if (backgroundBlur) backgroundBlur.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (settingsOpen)
            {
                // Close settings but keep pause menu open
                CloseSettings();
            }
            else if (isPaused)
            {
                // Resume game
                ResumeGame();
            }
            else
            {
                // Pause game
                PauseGame();
            }
        }
    }

    private void InitializeUI()
    {
        resumeButton.onClick.AddListener(() => { PlayButtonSound(); ResumeGame(); });
        settingsButton.onClick.AddListener(() => { PlayButtonSound(); OpenSettings(); });
        quitButton.onClick.AddListener(() => { PlayButtonSound(); QuitToMainMenu(); });

        backFromSettingsButton.onClick.AddListener(() => { PlayButtonSound(); CloseSettings(); });

        musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxVolumeSlider.onValueChanged.AddListener(SetSfxVolume);
    }

    private void FindPlayerComponents()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerController = player.GetComponent<PlayerController>();
            playerRb = player.GetComponent<Rigidbody2D>();
        }
    }

    private void SetupAudioSliders()
    {
        // Set slider values without triggering events first
        musicVolumeSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat("MusicVolume", 0.7f));
        sfxVolumeSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat("SfxVolume", 0.8f));

        // Apply current values to audio system
        SetMusicVolume(musicVolumeSlider.value);
        SetSfxVolume(sfxVolumeSlider.value);
    }

    public void PauseGame()
    {
        isPaused = true;

        // Pause player
        PausePlayer();

        // Pause all audio
        PauseAllAudio();

        // Show pause menu with main panel
        StartCoroutine(ShowPauseMenuWithFade());

        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        isPaused = false;
        settingsOpen = false;

        // Resume player
        ResumePlayer();

        // Resume audio
        ResumeAllAudio();

        Time.timeScale = 1f;

        // Hide entire pause menu
        StartCoroutine(HidePauseMenuWithFade());
    }

    public void OpenSettings()
    {
        if (!settingsOpen)
        {
            settingsOpen = true;
            StartCoroutine(ShowSettingsPanel());
        }
    }

    public void CloseSettings()
    {
        if (settingsOpen)
        {
            settingsOpen = false;
            StartCoroutine(HideSettingsPanel());
        }
    }

    public void QuitToMainMenu()
    {
        Time.timeScale = 1f;
        SceneLoader.Instance.LoadScene("MainMenu");
    }

    private void PausePlayer()
    {
        if (playerController != null)
        {
            playerController.enabled = false;
        }

        if (playerRb != null)
        {
            pausedVelocity = playerRb.velocity;
            playerRb.velocity = Vector2.zero;
            playerRb.bodyType = RigidbodyType2D.Kinematic;
        }
    }

    private void ResumePlayer()
    {
        if (playerController != null)
        {
            playerController.enabled = true;
        }
        if (playerRb != null)
        {
            playerRb.bodyType = RigidbodyType2D.Dynamic;
            playerRb.velocity = pausedVelocity;
        }
    }

    private void PauseAllAudio()
    {
        AudioManager.Instance.StopAllSFX();
        AudioManager.Instance.StopMusic();
    }

    private void ResumeAllAudio()
    {
        AudioManager.Instance.ResumeMusic();
    }

    public void SetMusicVolume(float volume)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(volume);
        }
        PlayerPrefs.SetFloat("MusicVolume", volume);
        PlayerPrefs.Save(); // Force save immediately
    }

    public void SetSfxVolume(float volume)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSfxVolume(volume);
            // Play a test sound to hear the volume change
            AudioManager.Instance.PlaySFX("Button click");
        }
        PlayerPrefs.SetFloat("SfxVolume", volume);
        PlayerPrefs.Save(); // Force save immediately
    }

    private void PlayButtonSound()
    {
        AudioManager.Instance.PlaySFX("Button click");
    }

    private IEnumerator ShowPauseMenuWithFade()
    {
        // Show the overall pause menu panel and background
        pauseMenuPanel.SetActive(true);
        mainPausePanel.SetActive(true);
        if (backgroundBlur) backgroundBlur.SetActive(true);

        // Initialize canvas groups
        pauseMenuCanvasGroup.alpha = 0f;
        pauseMenuCanvasGroup.interactable = false;

        mainPauseCanvasGroup.alpha = 0f;
        mainPauseCanvasGroup.interactable = false;

        // Fade in the main pause menu
        while (pauseMenuCanvasGroup.alpha < 1f || mainPauseCanvasGroup.alpha < 1f)
        {
            float deltaTime = Time.unscaledDeltaTime * fadeSpeed;
            pauseMenuCanvasGroup.alpha += deltaTime;
            mainPauseCanvasGroup.alpha += deltaTime;
            yield return null;
        }

        pauseMenuCanvasGroup.alpha = 1f;
        mainPauseCanvasGroup.alpha = 1f;
        mainPauseCanvasGroup.interactable = true;
        pauseMenuCanvasGroup.interactable = true;
    }

    private IEnumerator HidePauseMenuWithFade()
    {
        pauseMenuCanvasGroup.interactable = false;
        mainPauseCanvasGroup.interactable = false;
        if (settingsCanvasGroup) settingsCanvasGroup.interactable = false;

        // Fade out all panels
        while (pauseMenuCanvasGroup.alpha > 0f)
        {
            float deltaTime = Time.unscaledDeltaTime * fadeSpeed;
            pauseMenuCanvasGroup.alpha -= deltaTime;
            mainPauseCanvasGroup.alpha -= deltaTime;
            if (settingsCanvasGroup) settingsCanvasGroup.alpha -= deltaTime;
            yield return null;
        }

        pauseMenuCanvasGroup.alpha = 0f;
        mainPauseCanvasGroup.alpha = 0f;
        if (settingsCanvasGroup) settingsCanvasGroup.alpha = 0f;

        // Hide all panels
        pauseMenuPanel.SetActive(false);
        mainPausePanel.SetActive(false);
        settingsPanel.SetActive(false);
        if (backgroundBlur) backgroundBlur.SetActive(false);
    }

    private IEnumerator ShowSettingsPanel()
    {
        settingsPanel.SetActive(true);
        settingsCanvasGroup.alpha = 0f;
        settingsCanvasGroup.interactable = false;

        // Fade in settings panel
        while (settingsCanvasGroup.alpha < 1f)
        {
            settingsCanvasGroup.alpha += Time.unscaledDeltaTime * fadeSpeed;
            yield return null;
        }

        settingsCanvasGroup.alpha = 1f;
        settingsCanvasGroup.interactable = true;
    }

    private IEnumerator HideSettingsPanel()
    {
        settingsCanvasGroup.interactable = false;

        // Fade out settings panel
        while (settingsCanvasGroup.alpha > 0f)
        {
            settingsCanvasGroup.alpha -= Time.unscaledDeltaTime * fadeSpeed;
            yield return null;
        }

        settingsCanvasGroup.alpha = 0f;
        settingsPanel.SetActive(false);
    }
}