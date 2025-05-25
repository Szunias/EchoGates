using UnityEngine;
using TMPro; // Required for TextMeshPro

public class TotemDisplayUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("The TextMeshProUGUI element to display the totem status.")]
    [SerializeField] private TextMeshProUGUI totemStatusText;

    private void OnEnable()
    {
        // Subscribe to the event when this object is enabled
        TotemLightingUp.OnLitTotemCountChanged += UpdateTotemDisplay;
        // Immediately update the display when enabled, in case the event was missed or for initial setup
        UpdateTotemDisplay();
    }

    private void OnDisable()
    {
        // Unsubscribe from the event when this object is disabled to prevent errors
        TotemLightingUp.OnLitTotemCountChanged -= UpdateTotemDisplay;
    }

    private void Start()
    {
        if (totemStatusText == null)
        {
            Debug.LogError("TotemDisplayUI: TotemStatusText is not assigned in the Inspector! UI will not update.", this.gameObject);
            enabled = false; // Disable this script if the text element is missing
            return;
        }
        // Initial update when the game starts or UI becomes active
        UpdateTotemDisplay();
    }

    private void UpdateTotemDisplay()
    {
        if (totemStatusText != null)
        {
            int litCount = TotemLightingUp.GetLitTotemsCount();
            int requiredCount = TotemLightingUp.GetRequiredTotems();
            totemStatusText.text = $"Totems Lit: {litCount}/{requiredCount}";
        }
    }
}
