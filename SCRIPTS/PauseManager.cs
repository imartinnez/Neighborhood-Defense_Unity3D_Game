using UnityEngine;
using UnityEngine.SceneManagement;

/// Handles the pause menu system:
/// - Toggles pause state with Escape
/// - Freezes and restores game time
/// - Shows / hides pause UI
/// - Manages cursor lock and visibility
/// - Disables selected gameplay scripts while paused
/// - Allows returning to the main menu
public class PauseMenu : MonoBehaviour
{
    // Global pause state
    public static bool IsPaused { get; private set; }

    // Pause UI
    [Header("UI")]
    [SerializeField] GameObject pausePanel; // Pause menu panel (initially disabled)

    // Scripts to disable while paused
    [Header("Disable While Paused (drag scripts here)")]
    [SerializeField] MonoBehaviour[] disableWhilePaused;

    // Scene configuration
    [Header("Scenes")]
    [SerializeField] string mainMenuSceneName = "MainMenu";

    // Initialization
    void Start()
    {
        // Ensure a correct state when entering the scene
        ResumeInternal();
    }

    // Input handling
    void Update()
    {
        // Toggle pause with Escape key
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (IsPaused)
                Resume();
            else
                Pause();
        }
    }

    /// Pauses the game and opens the pause menu.
    public void Pause()
    {
        if (IsPaused) return;
        IsPaused = true;

        // Show pause UI
        if (pausePanel)
            pausePanel.SetActive(true);

        // Freeze game time
        Time.timeScale = 0f;

        // Unlock cursor for UI interaction
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Disable gameplay scripts
        SetDisabledScripts(true);
    }

    /// Resumes the game from the paused state.
    public void Resume()
    {
        if (!IsPaused) return;
        ResumeInternal();
    }

    /// Restores gameplay state after pause.
    void ResumeInternal()
    {
        IsPaused = false;

        // Hide pause UI
        if (pausePanel)
            pausePanel.SetActive(false);

        // Restore time
        Time.timeScale = 1f;

        // Lock cursor back to FPS mode
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Re-enable gameplay scripts
        SetDisabledScripts(false);
    }

    /// Loads the main menu scene.
    /// Always clears pause state before switching scenes.
    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        IsPaused = false;

        SceneManager.LoadScene(mainMenuSceneName);
    }

    /// Enables or disables selected scripts based on pause state.
    void SetDisabledScripts(bool paused)
    {
        if (disableWhilePaused == null) return;

        for (int i = 0; i < disableWhilePaused.Length; i++)
        {
            if (disableWhilePaused[i] != null)
                disableWhilePaused[i].enabled = !paused;
        }
    }
}