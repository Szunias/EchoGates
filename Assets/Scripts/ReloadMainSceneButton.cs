using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Attach to a Button GameObject. <br/>
/// 1. When the menu (or button) activates, it shows the cursor. <br/>
/// 2. When clicked, it loads the specified scene and hides/locks the cursor again.
/// </summary>
[RequireComponent(typeof(Button))]
public class ReloadMainSceneButton : MonoBehaviour
{
    [Tooltip("The scene to reload (e.g., your main game scene)")]
    [SerializeField] private string sceneName = "MainScene"; // Make sure this matches your scene name

    private Button _btn;

    /* --------------------  UI Lifecycle  -------------------- */
    private void OnEnable()         // When the menu / button becomes visible
    {
        // Show cursor, unlock camera (or allow interaction)
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Register the click listener
        _btn = GetComponent<Button>();
        _btn.onClick.AddListener(ReloadScene);
    }

    private void OnDisable()        // Cleanup when hiding the menu
    {
        // Unregister the click listener to prevent potential issues
        if (_btn != null)
            _btn.onClick.RemoveListener(ReloadScene);
    }

    /* --------------------  ACTION  -------------------- */
    private void ReloadScene()
    {
        // --- RESET TOTEM PROGRESS ---
        // This is the crucial line: Call the static reset method before loading.
        TotemLightingUp.ResetTotemProgress();
        Debug.Log("Totem progress reset initiated before scene reload.");
        // -----------------------------

        // Hide cursor immediately (optional, as a safeguard)
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Reload the scene
        SceneManager.LoadScene(sceneName);
    }
}
