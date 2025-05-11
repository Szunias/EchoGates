using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class MessageUIManager : MonoBehaviour
{
    public static MessageUIManager Instance;

    [Header("UI References")]
    [SerializeField] private GameObject panel;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private Button continueButton;

    [Header("HUD & Inventory Panels")]
    [Tooltip("Panel HUD to disable during tutorial")]
    [SerializeField] private GameObject hudPanel;
    [Tooltip("Panel Inventory UI to disable during tutorial")]
    [SerializeField] private GameObject inventoryPanel;

    [Header("Gameplay to disable")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private CameraMovement cameraMovement;
    [SerializeField] private PlayerInput playerInput;  // jeœli u¿ywasz
    [SerializeField] private EnemyAI enemy;
    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;

        panel.SetActive(false);
        continueButton.onClick.AddListener(Hide);

        // Ensure HUD and Inventory are active on start
        if (hudPanel != null) hudPanel.SetActive(true);
        if (inventoryPanel != null) inventoryPanel.SetActive(true);

        // Hide cursor initially
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    /// <summary>
    /// Displays the tutorial message, pauses the game, disables gameplay and UI layers.
    /// </summary>
    public void Show(string text)
    {
        messageText.text = text;
        panel.SetActive(true);

        // Pause game time
        Time.timeScale = 0f;

        // Disable input and movement scripts
        if (playerInput != null) playerInput.enabled = false;
        if (playerMovement != null) playerMovement.enabled = false;
        if (cameraMovement != null) cameraMovement.enabled = false;
        if (enemy != null) enemy.enabled = false;

        // Disable HUD and Inventory panels
        if (hudPanel != null) hudPanel.SetActive(false);
        if (inventoryPanel != null) inventoryPanel.SetActive(false);

        // Show and unlock cursor for UI
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    /// <summary>
    /// Hides the tutorial, resumes the game, re-enables gameplay and UI.
    /// </summary>
    private void Hide()
    {
        panel.SetActive(false);

        // Resume game time
        Time.timeScale = 1f;

        // Re-enable input and movement scripts
        if (playerInput != null) playerInput.enabled = true;
        if (playerMovement != null) playerMovement.enabled = true;
        if (cameraMovement != null) cameraMovement.enabled = true;
        if (enemy != null) enemy.enabled = true;

        // Re-enable HUD and Inventory panels
        if (hudPanel != null) hudPanel.SetActive(true);
        if (inventoryPanel != null) inventoryPanel.SetActive(true);

        // Hide and lock cursor
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}