// --- Tutorial.cs (Final Version with Advanced Skip UI) ---
using System.Collections;
using UnityEngine;
using UnityEngine.UI; // Required for Slider

public class Tutorial : MonoBehaviour
{
    public static Tutorial Instance { get; private set; }
    private static bool tutorialHasBeenPlayed = false;

    [Header("Skip Settings")]
    [Tooltip("The key to hold down to skip the tutorial.")]
    [SerializeField] private KeyCode skipKey = KeyCode.Tab;
    [Tooltip("How long the player must hold the key to skip (in seconds).")]
    [SerializeField] private float holdToSkipDuration = 1.5f;
    [Tooltip("The fade duration for the skip UI.")]
    [SerializeField] private float fadeDuration = 0.5f;

    [Header("UI References")]
    [Tooltip("The CanvasGroup containing all skip UI elements (text and slider).")]
    [SerializeField] private CanvasGroup skipUIGroup;
    [Tooltip("The Slider component used as a progress indicator for skipping.")]
    [SerializeField] private Slider skipSlider;

    private Coroutine tutorialCoroutine;
    private float skipHoldTimer = 0f;
    private bool isRunning = false; // Flag to know if the tutorial is currently active

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); }
        else { Instance = this; }
    }

    void Start()
    {
        if (tutorialHasBeenPlayed)
        {
            gameObject.SetActive(false);
            return;
        }

        // Ensure UI is hidden and configured correctly on start
        if (skipUIGroup != null)
        {
            skipUIGroup.alpha = 0;
            skipUIGroup.interactable = false;
        }
        if (skipSlider != null)
        {
            skipSlider.minValue = 0;
            skipSlider.maxValue = holdToSkipDuration;
            skipSlider.value = 0;
        }

        tutorialCoroutine = StartCoroutine(PlayTutorialSequence());
    }

    void Update()
    {
        // The skip logic only runs if the tutorial is active
        if (isRunning)
        {
            if (Input.GetKey(skipKey))
            {
                skipHoldTimer += Time.deltaTime;
                if (skipSlider != null)
                {
                    skipSlider.value = skipHoldTimer;
                }

                if (skipHoldTimer >= holdToSkipDuration)
                {
                    Debug.Log("Tutorial skipped by player.");
                    StopTutorial();
                }
            }
            else if (Input.GetKeyUp(skipKey))
            {
                // Reset timer and slider if key is released early
                skipHoldTimer = 0f;
                if (skipSlider != null)
                {
                    skipSlider.value = 0;
                }
            }
        }
    }

    public void StopTutorial()
    {
        if (!isRunning) return; // Prevent multiple calls

        isRunning = false; // Mark tutorial as no longer running
        if (tutorialCoroutine != null)
        {
            StopCoroutine(tutorialCoroutine);
            tutorialCoroutine = null;
        }
        Debug.Log("Tutorial stopped.");

        StartCoroutine(FadeUI(false)); // Fade out the skip UI

        if (SubtitleManager.Instance != null) { SubtitleManager.Instance.HideAndClear(); }
        tutorialHasBeenPlayed = true;
    }

    private IEnumerator PlayTutorialSequence()
    {
        isRunning = true;
        StartCoroutine(FadeUI(true)); // Fade in the skip UI

        if (SubtitleManager.Instance == null)
        {
            Debug.LogError("Cannot play tutorial sequence, SubtitleManager.Instance is null.");
            isRunning = false;
            yield break;
        }

        Debug.Log("Starting tutorial sequence...");
        // This delay is now part of the tutorial itself, not a Start() delay
        yield return new WaitForSeconds(2f);

        SubtitleManager.Instance.ShowSubtitle("What is that on the table?");
        SubtitleManager.Instance.ShowSubtitle("Is that... Captain Sniffles?");

        yield return new WaitUntil(() => SubtitleManager.Instance.IsQueueEmpty && !SubtitleManager.Instance.IsDisplaying);

        // When subtitles are done, mark as finished and fade out the skip UI
        isRunning = false;
        tutorialHasBeenPlayed = true;
        Debug.Log("Tutorial sequence finished and marked as played.");
        StartCoroutine(FadeUI(false));
        tutorialCoroutine = null;
    }

    private IEnumerator FadeUI(bool fadeIn)
    {
        if (skipUIGroup == null) yield break;

        float targetAlpha = fadeIn ? 1f : 0f;
        float startAlpha = skipUIGroup.alpha;
        float elapsedTime = 0f;

        // Enable/disable interaction at the start of the fade
        skipUIGroup.interactable = fadeIn;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            skipUIGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / fadeDuration);
            yield return null;
        }

        skipUIGroup.alpha = targetAlpha; // Ensure it reaches the final value
    }
}