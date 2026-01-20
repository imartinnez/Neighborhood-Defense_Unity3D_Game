using UnityEngine;

/// Simple cursor lock/unlock controller:
/// - On Start: locks and hides the cursor (FPS style).
/// - Press ESC: unlocks and shows the cursor (useful for UI).
/// - Left click: locks and hides the cursor again to resume aiming.
public class CursorLock : MonoBehaviour
{
    void Start()
    {
        // Lock the cursor to the center of the screen and hide it.
        // This is typical for first-person shooters.
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // ESC unlocks the cursor so the player can interact with UI.
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // Left mouse click locks the cursor again to resume aiming/shooting.
        if (Input.GetMouseButtonDown(0))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
