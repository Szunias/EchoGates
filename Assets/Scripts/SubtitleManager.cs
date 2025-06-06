// --- SubtitleManager.cs (TextMeshPro Version) ---
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI; // Still needed for the legacy Image if you keep it, but not for the new fields.

public class SubtitleManager : MonoBehaviour
{
    public static SubtitleManager Instance { get; private set; }

    [Header("UI References (TextMeshPro)")]
    public TextMeshProUGUI subtitleText;

    // CHANGED: Replaced Image with TextMeshProUGUI
    [Tooltip("Optional TextMeshPro object used as a background panel.")]
    public TextMeshProUGUI backgroundText;

    // CHANGED: Replaced Image with TextMeshProUGUI
    [Tooltip("The TextMeshPro object that displays the 'Press Enter' prompt.")]
    public TextMeshProUGUI continuePromptText;

    [Header("Dependencies")]
    [Tooltip("A reference to the player's movement script to disable/enable it.")]
    public PlayerMovement playerMovement;

    private Queue<string> subtitleQueue = new Queue<string>();
    private bool isWaitingForInput = false;

    public bool IsQueueEmpty => subtitleQueue.Count == 0;
    public bool IsDisplaying { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); }
        else { Instance = this; }

        HideAllUI();
    }

    private void Start()
    {
        StartCoroutine(SubtitleDisplayLoop());
    }

    private void Update()
    {
        if (isWaitingForInput && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
            isWaitingForInput = false;
        }
    }

    public void ShowSubtitle(string text)
    {
        if (string.IsNullOrEmpty(text)) return;
        subtitleQueue.Enqueue(text);
    }

    public void HideAndClear()
    {
        subtitleQueue.Clear();
        isWaitingForInput = false;
        HideAllUI();
        if (playerMovement != null) { playerMovement.EnableMovement(); }
        IsDisplaying = false;
    }

    private IEnumerator SubtitleDisplayLoop()
    {
        while (true)
        {
            yield return new WaitUntil(() => subtitleQueue.Count > 0);

            IsDisplaying = true;
            if (playerMovement != null) { playerMovement.DisableMovement(); }

            while (subtitleQueue.Count > 0)
            {
                string textToShow = subtitleQueue.Dequeue();

                // Show UI elements
                subtitleText.text = textToShow;
                subtitleText.gameObject.SetActive(true);

                // CHANGED: Now interacts with backgroundText instead of backgroundImage
                if (backgroundText != null) backgroundText.gameObject.SetActive(true);
                // CHANGED: Now interacts with continuePromptText instead of continuePromptImage
                if (continuePromptText != null) continuePromptText.gameObject.SetActive(true);

                isWaitingForInput = true;
                yield return new WaitUntil(() => !isWaitingForInput);

                // CHANGED: Hides the text prompt after input
                if (continuePromptText != null) continuePromptText.gameObject.SetActive(false);
            }

            IsDisplaying = false;
            HideAllUI();
            if (playerMovement != null) { playerMovement.EnableMovement(); }
        }
    }

    private void HideAllUI()
    {
        if (subtitleText != null) subtitleText.gameObject.SetActive(false);
        // CHANGED: Hides the text background
        if (backgroundText != null) backgroundText.gameObject.SetActive(false);
        // CHANGED: Hides the text prompt
        if (continuePromptText != null) continuePromptText.gameObject.SetActive(false);
    }
}