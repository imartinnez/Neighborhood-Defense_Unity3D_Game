using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// Manages all Heads-Up Display (HUD) elements:
/// - Player health bar and text
/// - Round timer, round number, and enemy counter
/// - Ammo display
/// - Game Over and Win screens
/// - Input blocking and cursor control during end screens
public class HUDManager : MonoBehaviour
{
    // Singleton instance
    public static HUDManager Instance { get; private set; }

    // Player health UI
    [Header("Player Health")]
    public Image healthBarFill; // Health bar fill image
    public Text healthText;     // Numeric health text (e.g. 75/100)

    // Round information UI
    [Header("Round Info")]
    public Text timerText;     // Round timer (MM:SS)
    public Text enemiesText;   // Remaining enemies
    public Text roundText;     // Current round number

    // Ammo UI
    [Header("Ammo")]
    public Text ammoText; // Ammo display (e.g. "10/30" or "Reloading...")

    // End screen panels
    [Header("Panels")]
    public GameObject gameOverPanel; // Game Over screen
    public GameObject winPanel;      // Win screen

    // Scene configuration
    [Header("Scenes")]
    [SerializeField] string mainMenuSceneName = "MainMenu";

    // Scripts to disable while end screen is active
    [Header("Disable Input While End Screen (drag scripts here)")]
    [SerializeField] MonoBehaviour[] disableWhileEndScreen;

    // Initialization
    void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        // Ensure a safe state when entering the Game scene
        Time.timeScale = 1f;

        if (gameOverPanel)
            gameOverPanel.SetActive(false);

        if (winPanel)
            winPanel.SetActive(false);
    }

    // --- HEALTH UI ---

    /// Updates the player's health bar and text.
    public void UpdateHealth(float current, float max)
    {
        float ratio = current / max;

        if (healthBarFill)
            healthBarFill.fillAmount = ratio;

        if (healthText)
            healthText.text = $"{current}/{max}";

        // Dynamic color change (green → red)
        if (healthBarFill)
            healthBarFill.color = Color.Lerp(Color.red, Color.green, ratio);
    }

    // --- TIMER UI ---

    /// Updates the round timer and changes color when time is low.
    public void UpdateTimer(float timeRemaining)
    {
        int minutes = Mathf.FloorToInt(timeRemaining / 60f);
        int seconds = Mathf.FloorToInt(timeRemaining % 60f);

        if (timerText)
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

        // Visual warning when time is running out
        if (timerText)
            timerText.color = timeRemaining <= 10f ? Color.red : Color.white;
    }

    // --- ROUND & ENEMY UI ---

    /// Updates the round number and remaining enemies.
    public void UpdateRoundInfo(int round, int totalRounds, int enemiesLeft)
    {
        if (roundText)
            roundText.text = $"Round: {round} / {totalRounds}";

        if (enemiesText)
            enemiesText.text = $"Enemies: {enemiesLeft}";
    }

    // --- AMMO UI ---

    /// Updates the ammo display.
    public void UpdateAmmo(int current, int max, bool reloading)
    {
        if (!ammoText) return;

        ammoText.text = reloading
            ? "Reloading..."
            : $"{current}/{max}";
    }

    // --- END SCREENS ---

    /// Displays the Game Over screen.
    public void ShowGameOver()
    {
        if (gameOverPanel)
            gameOverPanel.SetActive(true);

        EnterEndScreenState();
    }

    /// Displays the Win screen.
    public void ShowWin()
    {
        if (winPanel)
            winPanel.SetActive(true);

        EnterEndScreenState();
    }

    /// Enters the end screen state:
    /// - Freezes the game
    /// - Unlocks the cursor
    /// - Disables player input
    void EnterEndScreenState()
    {
        // Freeze game time
        Time.timeScale = 0f;

        // Unlock cursor for UI interaction
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Disable gameplay input
        SetInputDisabled(true);
    }

    /// Enables or disables selected scripts during end screens.
    void SetInputDisabled(bool disabled)
    {
        if (disableWhileEndScreen == null) return;

        for (int i = 0; i < disableWhileEndScreen.Length; i++)
        {
            if (disableWhileEndScreen[i] != null)
                disableWhileEndScreen[i].enabled = !disabled;
        }
    }

    // --- BUTTON ACTIONS ---

    /// Restarts the current level.
    public void Retry()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /// Returns to the main menu.
    public void BackToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}