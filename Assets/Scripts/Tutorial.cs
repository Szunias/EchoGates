using System.Collections;
using UnityEngine;

public class Tutorial : MonoBehaviour
{
    // --- SINGLETON PATTERN ---
    public static Tutorial Instance { get; private set; }

    // --- STATIC FLAG to remember state between scene loads ---
    private static bool tutorialHasBeenPlayed = false;

    // We no longer need a direct reference to the SubtitleManager here,
    // as we will use the Singleton instance: SubtitleManager.Instance

    private Coroutine tutorialCoroutine; // A reference to our coroutine so we can stop it

    private void Awake()
    {
        // Singleton Setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    void Start()
    {
        // --- CHECK if the tutorial has already been played ---
        // If so, don't start it again.
        if (tutorialHasBeenPlayed)
        {
            Debug.Log("Tutorial has already been played in this session. Skipping.");
            gameObject.SetActive(false); // Deactivate this object to prevent further logic
            return;
        }

        // If not, start the tutorial sequence
        tutorialCoroutine = StartCoroutine(PlayTutorialSequence());
    }

    /// <summary>
    /// Public method to stop the tutorial from other scripts.
    /// </summary>
    public void StopTutorial()
    {
        if (tutorialCoroutine != null)
        {
            StopCoroutine(tutorialCoroutine);
            tutorialCoroutine = null; // Clear the reference
            Debug.Log("Tutorial coroutine stopped.");

            // Also ensure the subtitles are cleared immediately
            if (SubtitleManager.Instance != null)
            {
                SubtitleManager.Instance.HideAndClear();
            }

            // Mark the tutorial as "finished", even if it was interrupted
            tutorialHasBeenPlayed = true;
        }
    }

    private IEnumerator PlayTutorialSequence()
    {
        // Check if the SubtitleManager exists
        if (SubtitleManager.Instance == null)
        {
            Debug.LogError("Cannot play tutorial sequence, SubtitleManager.Instance is null.");
            yield break;
        }

        Debug.Log("Starting tutorial sequence...");

        // Wait a moment for the player to look around
        yield return new WaitForSeconds(2f);

        // Add subtitles to the queue. The manager will handle timings.
        SubtitleManager.Instance.ShowSubtitle("What is that on the table?");

        // We can add the next one immediately to the queue
        SubtitleManager.Instance.ShowSubtitle("Is that... Captain Sniffles?");

        // After adding all subtitles to the queue, wait for them to finish displaying
        // before marking the tutorial as complete.
        // This calculates a rough duration.
        float waitTime = (SubtitleManager.Instance.defaultDisplayTime + SubtitleManager.Instance.delayBetweenSubtitles) * 2;
        yield return new WaitForSeconds(waitTime);


        // Mark the tutorial as played once the sequence is fully complete
        tutorialHasBeenPlayed = true;
        Debug.Log("Tutorial sequence finished and marked as played.");
        tutorialCoroutine = null; // Clear the reference after completion
    }
}