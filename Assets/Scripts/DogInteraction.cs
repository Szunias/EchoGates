using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; // Required for TextMeshPro

public class DogInteraction : MonoBehaviour
{
    [Header("Scene Settings")]
    [Tooltip("The name of the scene to load when the game is won.")]
    [SerializeField] private string winSceneName = "GameWon";

    [Header("Interaction Settings")]
    [Tooltip("The key to press for interaction.")]
    [SerializeField] private KeyCode interactionKey = KeyCode.E;
    [Tooltip("Message displayed when interaction is possible.")]
    [SerializeField] private string interactionPromptMessage = "Press E to Interact with Dog";

    [Header("Game Object References")]
    [Tooltip("The 'Beam' GameObject to activate when all totems are collected and cage is gone.")]
    [SerializeField] private GameObject beamObject; // Assign your Beam GameObject here

    [Header("UI References")]
    [Tooltip("The TextMeshProUGUI element to display the interaction prompt.")]
    [SerializeField] private TextMeshProUGUI promptText; // Assign your prompt TextMeshProUGUI element here

    private bool isPlayerInRange = false;

    private void Start()
    {
        // Ensure the prompt text is hidden at the start
        if (promptText != null)
        {
            promptText.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("DogInteraction: PromptText is not assigned. UI prompt will not be displayed.", this.gameObject);
        }

        // Ensure the beam object is hidden at the start
        if (beamObject != null)
        {
            beamObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("DogInteraction: BeamObject is not assigned. Beam visibility cannot be controlled.", this.gameObject);
        }
    }

    void Update()
    {
        // Control beam visibility
        if (beamObject != null)
        {
            // Show beam if cage is gone and beam is not already active
            if (TotemLightingUp.cageIsGone && !beamObject.activeSelf)
            {
                beamObject.SetActive(true);
                Debug.Log("DogInteraction: All totems collected, cage is gone. Activating beam.");
            }
            // Hide beam if cage is not gone and beam is active (e.g., if state resets)
            else if (!TotemLightingUp.cageIsGone && beamObject.activeSelf)
            {
                beamObject.SetActive(false);
                Debug.Log("DogInteraction: Cage is not gone. Deactivating beam.");
            }
        }

        // Handle interaction prompt and win condition
        if (isPlayerInRange && TotemLightingUp.cageIsGone)
        {
            if (promptText != null && !promptText.gameObject.activeSelf)
            {
                promptText.text = interactionPromptMessage;
                promptText.gameObject.SetActive(true);
            }

            if (Input.GetKeyDown(interactionKey))
            {
                TriggerWin();
            }
        }
        else
        {
            if (promptText != null && promptText.gameObject.activeSelf)
            {
                promptText.gameObject.SetActive(false);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            Debug.Log("Player entered dog interaction range.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            Debug.Log("Player exited dog interaction range.");
            if (promptText != null && promptText.gameObject.activeSelf)
            {
                promptText.gameObject.SetActive(false); // Hide prompt when player exits
            }
        }
    }

    private void TriggerWin()
    {
        Debug.Log($"Interaction key '{interactionKey}' pressed near the dog! Loading win scene: {winSceneName}");
        if (!string.IsNullOrEmpty(winSceneName))
        {
            SceneManager.LoadScene(winSceneName);
        }
        else
        {
            Debug.LogError("DogInteraction: Win scene name is not set or is empty!", this.gameObject);
        }
    }
}
