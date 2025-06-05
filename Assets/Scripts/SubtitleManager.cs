using UnityEngine;
using System.Collections;
using System.Collections.Generic; // Required for using the Queue
using TMPro;
using UnityEngine.UI;

public class SubtitleManager : MonoBehaviour
{
    // --- SINGLETON PATTERN ---
    // This allows easy access to the manager from anywhere in the code
    // by writing: SubtitleManager.Instance.ShowSubtitle("...")
    public static SubtitleManager Instance { get; private set; }

    [Header("UI References")]
    public TextMeshProUGUI subtitleText;
    public Image backgroundImage;

    [Header("Timing Settings")]
    public float defaultDisplayTime = 4f; // Default duration for a subtitle to be on screen
    public float delayBetweenSubtitles = 0.5f; // A short pause between consecutive subtitles in the queue

    // --- QUEUE SYSTEM ---
    private Queue<SubtitleRequest> subtitleQueue; // A queue that holds subtitle requests
    private Coroutine displayCoroutine;           // A reference to the main coroutine that manages the queue

    // A small struct to hold the text and duration for each subtitle request
    private struct SubtitleRequest
    {
        public string Text;
        public float Duration;
    }

    private void Awake()
    {
        // --- Singleton Setup ---
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // If another instance already exists, destroy this one.
        }
        else
        {
            Instance = this;
            // Optional: DontDestroyOnLoad(gameObject); // If the manager should persist across scene loads
        }

        subtitleQueue = new Queue<SubtitleRequest>();
        // Hide UI elements on start
        if (subtitleText != null) subtitleText.gameObject.SetActive(false);
        if (backgroundImage != null) backgroundImage.gameObject.SetActive(false);
    }

    private void Start()
    {
        // Start the main loop that will manage the queue
        displayCoroutine = StartCoroutine(SubtitleDisplayLoop());
    }

    /// <summary>
    /// Public method to add a new subtitle to the queue.
    /// </summary>
    /// <param name="text">The text to display.</param>
    /// <param name="duration">Optional display duration. If -1, the default time will be used.</param>
    public void ShowSubtitle(string text, float duration = -1f)
    {
        if (string.IsNullOrEmpty(text)) return;

        // Create a new subtitle request and add it to the queue
        SubtitleRequest newRequest = new SubtitleRequest
        {
            Text = text,
            Duration = duration > 0 ? duration : defaultDisplayTime
        };
        subtitleQueue.Enqueue(newRequest);
    }

    /// <summary>
    /// Public method to immediately hide the current subtitle and clear the entire queue.
    /// </summary>
    public void HideAndClear()
    {
        Debug.Log("SubtitleManager: Clearing queue and hiding subtitles.");
        subtitleQueue.Clear(); // Clear all pending subtitles

        // Stop and restart the main loop to interrupt the current display
        if (displayCoroutine != null)
        {
            StopCoroutine(displayCoroutine);
        }
        displayCoroutine = StartCoroutine(SubtitleDisplayLoop());

        // Immediately hide the UI elements
        if (subtitleText != null) subtitleText.gameObject.SetActive(false);
        if (backgroundImage != null) backgroundImage.gameObject.SetActive(false);
    }

    /// <summary>
    /// The main coroutine loop that constantly checks the queue and displays subtitles.
    /// </summary>
    private IEnumerator SubtitleDisplayLoop()
    {
        while (true) // This loop runs continuously in the background
        {
            // If there are any subtitles in the queue...
            if (subtitleQueue.Count > 0)
            {
                // ...get the next one from the queue.
                SubtitleRequest request = subtitleQueue.Dequeue();

                // --- Show Subtitle ---
                subtitleText.text = request.Text;
                subtitleText.gameObject.SetActive(true);
                if (backgroundImage != null)
                {
                    backgroundImage.gameObject.SetActive(true);
                }

                // Wait for the specified duration
                yield return new WaitForSeconds(request.Duration);

                // --- Hide Subtitle ---
                subtitleText.gameObject.SetActive(false);
                if (backgroundImage != null)
                {
                    backgroundImage.gameObject.SetActive(false);
                }

                // Wait for a short moment before displaying the next subtitle
                yield return new WaitForSeconds(delayBetweenSubtitles);
            }
            else
            {
                // If the queue is empty, just wait for the next frame
                yield return null;
            }
        }
    }
}