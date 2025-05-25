using UnityEngine;
using UnityEngine.UI;

public class Stamina : MonoBehaviour
{
    [Header("Stamina Settings")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float drainRate = 20f;  // points per second while sprinting
    [SerializeField] private float regenRate = 10f;  // points per second while not sprinting

    [Header("UI References")]
    [SerializeField] private Slider staminaSlider;

    private float currentStamina;

    void Awake()
    {
        // Initialize
        currentStamina = maxStamina;

        if (staminaSlider != null)
        {
            staminaSlider.maxValue = maxStamina;
            staminaSlider.value = currentStamina;
        }
        else
        {
            Debug.LogError("Stamina: Please assign a UI Slider in the Inspector.");
        }
    }

    void Update()
    {
        // Check for movement input from the player
        // Using a small threshold to account for analog stick dead zones or minor floating point inaccuracies
        bool isTryingToMove = Mathf.Abs(Input.GetAxis("Horizontal")) > 0.01f || Mathf.Abs(Input.GetAxis("Vertical")) > 0.01f;
        bool wantsToSprint = Input.GetKey(KeyCode.LeftShift);

        if (wantsToSprint && isTryingToMove) // Player is attempting to sprint (holding key AND moving)
        {
            if (currentStamina > 0f)
            {
                // Drain stamina if the player wants to sprint, is trying to move, and has stamina
                currentStamina -= drainRate * Time.deltaTime;
                currentStamina = Mathf.Max(currentStamina, 0f); // Clamp stamina at 0
            }
            // If currentStamina is 0f here, and player is still trying to sprint/move,
            // stamina will NOT regenerate in this block. It will remain 0.
        }
        else // Player is NOT attempting to sprint (either not holding sprint key OR not moving OR out of stamina and still trying)
        {
            // Regenerate stamina if it's not already full
            if (currentStamina < maxStamina)
            {
                currentStamina += regenRate * Time.deltaTime;
                currentStamina = Mathf.Min(currentStamina, maxStamina); // Clamp stamina at maxStamina
            }
        }

        // Update UI
        if (staminaSlider != null)
        {
            staminaSlider.value = currentStamina;
        }
    }

    /// <summary>
    /// Returns true if there is any stamina remaining.
    /// </summary>
    public bool HasStamina()
    {
        // This will correctly return false if currentStamina is 0
        return currentStamina > 0f;
    }

    /// <summary>
    /// Optional getter for current stamina.
    /// </summary>
    public float GetCurrentStamina()
    {
        return currentStamina;
    }

    /// <summary>
    /// Optional getter for max stamina.
    /// </summary>
    public float GetMaxStamina()
    {
        return maxStamina;
    }
}
