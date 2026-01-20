using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// Controls the main menu flow:
/// - Scene loading
/// - Panel switching (Main / Instructions)
/// - Cursor and time state management
/// - Input blocking on startup to prevent accidental clicks
/// - Safe UI activation after window focus
public class MenuController : MonoBehaviour
{
    // Scene configuration
    [Header("Scenes")]
    [SerializeField] string gameSceneName = "Game"; // Gameplay scene name

    // Menu panels
    [Header("Panels")]
    [SerializeField] GameObject mainPanel;          // Main menu panel
    [SerializeField] GameObject instructionsPanel;  // Instructions panel

    // Optional UI references
    [Header("UI (optional)")]
    [SerializeField] GraphicRaycaster raycaster; // Assign the Canvas raycaster if desired

    // Initialization (coroutine to safely block input on startup)
    IEnumerator Start()
    {
        // Ensure a safe default state
        Time.timeScale = 1f;
        ShowMain();

        // Unlock and show cursor for menu interaction
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Auto-find raycaster if not assigned
        if (!raycaster)
            raycaster = FindFirstObjectByType<GraphicRaycaster>();

        // BLOCK UI clicks on startup
        // (important in Editor and in builds when the window gains focus)
        if (raycaster)
            raycaster.enabled = false;

        // Wait two frames (Play click / window focus often happens here)
        yield return null;
        yield return null;

        // Wait until the user releases all mouse buttons
        while (Input.GetMouseButton(0) || Input.GetMouseButton(1) || Input.GetMouseButton(2))
            yield return null;

        // Clear input state and UI selection
        Input.ResetInputAxes();
        if (EventSystem.current)
            EventSystem.current.SetSelectedGameObject(null);

        // Re-enable UI interaction
        if (raycaster)
            raycaster.enabled = true;
    }

    /// Starts the game.
    public void Play()
    {
        StartCoroutine(LoadGameRoutine());
    }

    /// Safely loads the gameplay scene.
    /// Prevents the Play button click from leaking into the next scene.
    IEnumerator LoadGameRoutine()
    {
        Time.timeScale = 1f;

        // Clear input to avoid carry-over
        Input.ResetInputAxes();
        yield return null;

        // Wait until the Play button click is released
        while (Input.GetMouseButton(0))
            yield return null;

        // Load the gameplay scene
        SceneManager.LoadScene(gameSceneName);
    }

    /// Shows the instructions panel.
    public void ShowInstructions()
    {
        if (mainPanel)
            mainPanel.SetActive(false);

        if (instructionsPanel)
            instructionsPanel.SetActive(true);
    }

    /// Returns to the main menu panel.
    public void Back()
    {
        ShowMain();
    }

    /// Shows the main menu and hides other panels.
    void ShowMain()
    {
        if (mainPanel)
            mainPanel.SetActive(true);

        if (instructionsPanel)
            instructionsPanel.SetActive(false);
    }

    /// Quits the application.
    /// (In the Unity Editor, this only logs a message.)
    public void Quit()
    {
#if UNITY_EDITOR
        Debug.Log("Quit (Editor): application will not close.");
#else
        Application.Quit();
#endif
    }
}