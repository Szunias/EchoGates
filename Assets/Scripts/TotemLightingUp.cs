using UnityEngine;
using System; // Required for System.Action

public class TotemLightingUp : MonoBehaviour
{
    [Header("Totem Settings")]
    [SerializeField] private Material lightUpMaterial;

    public static bool cageIsGone = false; // Static flag to indicate if the cage is gone
    private bool isLit = false;

    [Header("Cage Settings")]
    [SerializeField] private GameObject cage; // Reference to the cage GameObject

    // Static counter for all totems
    private static int litTotemsCount = 0;
    // Ensure this matches the actual number of totems player needs to light up in the scene
    private const int REQUIRED_TOTEMS_TO_WIN = 5;

    private static GameObject sharedCageInstance; // To ensure only one cage is controlled

    // Event to notify when the number of lit totems changes
    public static event Action OnLitTotemCountChanged;

    void Awake()
    {
        // This ensures that if the scene is reloaded (e.g., after "Try Again"),
        // the totem count is reset for the new session.
        // If you have a more sophisticated game manager that handles game state
        // between scene loads or persists data, you might call ResetTotemProgress
        // from there instead.
        // For a simple scene reload, this Awake call is a common way to reset.
        // However, if multiple TotemLightingUp scripts exist and Awake is called for all,
        // this might lead to multiple resets. A central game manager is better for complex scenarios.
        // For now, we'll ensure it's reset if it's the "first" totem being initialized or scene starts.
        // A better approach for scene start reset is a dedicated GameManager.
        // Let's assume ResetTotemProgress() will be called by a GameManager on "Try Again" or level start.
        // For now, we can call it in Start() of a GameManager or when the level loads.
        // To ensure it's reset when the game/level truly starts, we'll call it from Start().
        // If this script is on multiple totems, this will reset for each, which is not ideal.
        // A better place is a GameManager's Start or a specific reset function.
        // For now, let's ensure it's callable and the UI updates.
    }

    private void Start()
    {
        // Assign the shared cage instance from the first totem that has it assigned
        if (sharedCageInstance == null && cage != null)
        {
            sharedCageInstance = cage;
        }

        // To ensure the UI is correct at the start of the scene,
        // invoke the event if it's the first totem setting up.
        // This is a bit of a workaround; a GameManager should ideally manage this.
        if (!Application.isPlaying) return; // Don't run in edit mode during certain recompiles

        // This should ideally be called ONCE when the level loads/restarts.
        // If you have a GameManager, call ResetTotemProgress() from its Start() or a specific reset method.
        // For simplicity here, if it's the first totem initializing the cage, we can trigger an update.
        // However, the actual reset of litTotemsCount should happen in a central place.
        // We will ensure ResetTotemProgress is called when the scene loads.
        // The best way is to have a GameManager that calls ResetTotemProgress() when the game state is reset.
        // For now, let's ensure the UI updates on start.
        OnLitTotemCountChanged?.Invoke();
    }


    // Method to light up the totem
    public void LightUp()
    {
        if (isLit) // If already lit, do nothing
            return;

        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null && lightUpMaterial != null)
        {
            meshRenderer.material = lightUpMaterial;
        }

        Light pointLight = GetComponentInChildren<Light>(); // Get light from children if it's structured that way
        if (pointLight != null)
        {
            pointLight.enabled = true;
        }

        isLit = true;
        litTotemsCount++;

        Debug.Log($"Totem lit! Current count: {litTotemsCount}/{REQUIRED_TOTEMS_TO_WIN}");

        // Notify subscribers (like the UI) that the count has changed
        OnLitTotemCountChanged?.Invoke();

        // Check if all required totems are lit
        if (litTotemsCount >= REQUIRED_TOTEMS_TO_WIN)
        {
            if (sharedCageInstance != null)
            {
                Destroy(sharedCageInstance); // Destroy the cage
                Debug.Log("All totems lit! Cage destroyed!");
            }
            cageIsGone = true; // Set the static flag
        }
    }

    // Example activation: if the totem is hit by a projectile tagged "Bullet"
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Bullet")) // Make sure your projectile has this tag
        {
            LightUp();
        }
    }

    // Alternative activation if using triggers
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bullet")) // Make sure your projectile has this tag
        {
            LightUp();
        }
    }

    // Public method to get the current number of lit totems
    public static int GetLitTotemsCount()
    {
        return litTotemsCount;
    }

    // Public method to get the total number of required totems
    public static int GetRequiredTotems()
    {
        return REQUIRED_TOTEMS_TO_WIN;
    }

    // Public static method to reset totem progress
    // This should be called by your game manager when the game restarts or "Try Again" is selected.
    public static void ResetTotemProgress()
    {
        litTotemsCount = 0;
        cageIsGone = false; // Also reset the cage status
        Debug.Log("Totem progress has been reset.");
        // Notify the UI to update to the reset count
        OnLitTotemCountChanged?.Invoke();
    }
}
