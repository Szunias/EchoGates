using UnityEngine;
using System; // Required for System.Action

public class TotemLightingUp : MonoBehaviour
{
    [Header("Totem Settings")]
    [SerializeField] private Material lightUpMaterial;
    [Tooltip("The light component on or child of this totem that should turn on.")]
    [SerializeField] private Light totemLightSource;


    public static bool cageIsGone = false; // Static flag to indicate if the cage is gone
    private bool isLit = false;

    [Header("Cage Settings")]
    [Tooltip("Reference to the cage GameObject that should be destroyed.")]
    [SerializeField] private GameObject cageObject;

    // Static counter for all totems
    private static int litTotemsCount = 0;
    // Ensure this matches the actual number of totems player needs to light up in the scene
    [Tooltip("Total number of totems required to destroy the cage.")]
    [SerializeField] private int requiredTotemsToWin = 5; // Made this instance-configurable but will use the value from the first totem for static comparison

    private static int staticRequiredTotems = 5; // Default, will be set by the first totem
    private static GameObject sharedCageInstance; // To ensure only one cage is controlled

    // Event to notify when the number of lit totems changes
    public static event Action OnLitTotemCountChanged;

    private AudioSource source;

    void Awake()
    {
        // If this is the first totem initializing, set the static required count
        // This assumes all totems in a level should contribute to the same goal/count.
        // A better way might be a central GameManager defining REQUIRED_TOTEMS_TO_WIN.
        if (litTotemsCount == 0 && !Application.isPlaying)
        { // Attempt to set during edit mode or first load
          // This logic for staticRequiredTotems might be tricky if not managed by a GameManager.
          // For now, let's assume requiredTotemsToWin on one of the prefabs/instances is the master.
        }
        staticRequiredTotems = requiredTotemsToWin; // Each totem can report its requirement, the static one takes one.
                                                    // Consider setting this from a GameManager.

        source = GetComponent<AudioSource>();

    }

    private void Start()
    {
        // Assign the shared cage instance from the first totem that has it assigned
        if (sharedCageInstance == null && cageObject != null)
        {
            sharedCageInstance = cageObject;
        }

        if (totemLightSource == null)
        {
            totemLightSource = GetComponentInChildren<Light>(); // Try to find light in children
        }
        if (totemLightSource != null)
        {
            totemLightSource.enabled = isLit; // Ensure light state matches isLit on start
        }
        // Ensure UI is updated on start, especially after a scene reload where static counts might persist
        // if not reset properly by a game manager.
        OnLitTotemCountChanged?.Invoke();
        source.volume = 0.0f;
    }


    public void LightUp()
    {
        if (isLit)
            return;

        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null && lightUpMaterial != null)
        {
            meshRenderer.material = lightUpMaterial;
        }

        if (totemLightSource != null)
        {
            totemLightSource.enabled = true;
        }

        isLit = true;
        litTotemsCount++;
        source.volume = 1.0f;

        Debug.Log($"Totem lit! Current count: {litTotemsCount}/{staticRequiredTotems}");

        OnLitTotemCountChanged?.Invoke();

        if (litTotemsCount >= staticRequiredTotems)
        {
            if (sharedCageInstance != null)
            {
                Destroy(sharedCageInstance);
                Debug.Log("All totems lit! Cage destroyed!");
            }
            else
            {
                Debug.LogWarning("All totems lit, but no 'sharedCageInstance' was assigned to destroy.");
            }
            cageIsGone = true;
        }
    }

    // Example activation methods (collision/trigger)
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Bullet"))
        {
            LightUp();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bullet"))
        {
            LightUp();
        }
    }

    public static int GetLitTotemsCount()
    {
        return litTotemsCount;
    }

    public static int GetRequiredTotems()
    {
        // Use the statically set required count
        return staticRequiredTotems;
    }

    // This method MUST be called by your game/level manager when the game restarts.
    public static void ResetTotemProgress()
    {
        litTotemsCount = 0;
        cageIsGone = false;
        // Note: Individual 'isLit' states on totem instances are not reset here.
        // If totems are persistent objects that aren't destroyed/reloaded,
        // they would also need an instance method to reset their visual state.
        // However, if the scene reloads, their 'isLit' will be false on Start.
        Debug.Log("Totem progress has been reset.");
        OnLitTotemCountChanged?.Invoke(); // Notify UI
    }
}
