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
        // initialize
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
        // drain while holding Shift and having stamina
        bool isSprinting = Input.GetKey(KeyCode.LeftShift) && currentStamina > 0f;
        if (isSprinting)
        {
            currentStamina -= drainRate * Time.deltaTime;
            currentStamina = Mathf.Max(currentStamina, 0f);
        }
        else
        {
            currentStamina += regenRate * Time.deltaTime;
            currentStamina = Mathf.Min(currentStamina, maxStamina);
        }

        // update UI
        if (staminaSlider != null)
            staminaSlider.value = currentStamina;
    }

    /// <summary>
    /// Returns true if there is any stamina remaining.
    /// </summary>
    public bool HasStamina()
    {
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
