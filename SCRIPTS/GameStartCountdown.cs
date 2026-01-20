using System.Collections;
using UnityEngine;
using TMPro;

/// Handles the game start countdown:
/// - Displays a countdown UI (3…2…1…GO!)
/// - Freezes the game during the countdown
/// - Disables player input and gameplay scripts
/// - Uses real-time waiting (unaffected by Time.timeScale)
public class GameStartCountdown : MonoBehaviour
{
    // Countdown UI
    [Header("UI")]
    [SerializeField] GameObject countdownPanel; // Countdown panel
    [SerializeField] TMP_Text countdownText;    // Countdown text (TMP)

    // Countdown settings
    [Header("Countdown")]
    [SerializeField] int startFrom = 3; // Starting number for the countdown

    // Scripts to disable during countdown
    [Header("Disable While Counting Down (drag scripts here)")]
    [SerializeField] MonoBehaviour[] disableDuringCountdown;

    // Initialization
    void Start()
    {
        StartCoroutine(CountdownRoutine());
    }

    // Countdown sequence
    IEnumerator CountdownRoutine()
    {
        // Ensure countdown UI is visible
        if (countdownPanel)
            countdownPanel.SetActive(true);

        // Disable player input (weapon, movement, camera, etc.)
        SetDisabledScripts(true);

        // Freeze the entire game (enemies, physics, timers...)
        Time.timeScale = 0f;

        // Lock cursor in FPS mode during the countdown
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Countdown using real-time seconds (not affected by timeScale)
        for (int i = startFrom; i >= 1; i--)
        {
            if (countdownText)
                countdownText.text = i.ToString();

            yield return new WaitForSecondsRealtime(1f);
        }

        // Display "GO!"
        if (countdownText)
            countdownText.text = "GO!";

        yield return new WaitForSecondsRealtime(0.6f);

        // Start the game
        if (countdownPanel)
            countdownPanel.SetActive(false);

        // Resume game time and input
        Time.timeScale = 1f;
        SetDisabledScripts(false);

        // Ensure cursor remains locked for gameplay
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    /// Enables or disables selected scripts during the countdown.
    void SetDisabledScripts(bool disabled)
    {
        if (disableDuringCountdown == null) return;

        for (int i = 0; i < disableDuringCountdown.Length; i++)
        {
            if (disableDuringCountdown[i] != null)
                disableDuringCountdown[i].enabled = !disabled;
        }
    }
}