// --- CloseTutorial.cs ---
using UnityEngine;
using System.Collections;

public class CloseTutorial : MonoBehaviour
{
    public FlickeringLight flickeringLight;
    // We no longer need a direct reference to the SubtitleManager here
    public string[] followUpSubtitles;
    public float delayBetweenSubtitles = 3f; // This is now used differently

    public void CloseUi()
    {
        PlayerMovement playerMovement = FindFirstObjectByType<PlayerMovement>();
        if (playerMovement != null)
        {
            // --- FIXED ---
            // Replaced 'Intutorial = false' with the new method to enable movement.
            playerMovement.EnableMovement();
            // -------------
        }

        Cursor.visible = false;
        // Changed to Confined so the cursor doesn't get completely lost if needed later
        Cursor.lockState = CursorLockMode.Locked;

        if (flickeringLight != null)
        {
            flickeringLight.StartFlicker();
        }

        // --- IMPROVED ---
        // Start the subtitles using the SubtitleManager Singleton
        if (SubtitleManager.Instance != null && followUpSubtitles.Length > 0)
        {
            // The coroutine is now started on this object, but calls the Singleton
            StartCoroutine(ShowFollowUpSubtitles());
        }
        // ----------------

        // Destroy the UI panel GameObject this script is attached to.
        Destroy(gameObject);
    }

    private IEnumerator ShowFollowUpSubtitles()
    {
        // This coroutine will now queue up all subtitles, respecting the new manager's logic
        foreach (string subtitle in followUpSubtitles)
        {
            SubtitleManager.Instance.ShowSubtitle(subtitle);
            // We no longer need to wait here, the SubtitleManager handles the "Press Enter" flow.
        }
        yield break; // Coroutine finishes after queueing all subtitles.
    }
}