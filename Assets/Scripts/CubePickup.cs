using UnityEngine;

public class CubePickup : MonoBehaviour
{
    [Header("Pickup Settings")]
    [SerializeField] private float pickupRange = 3f;
    [SerializeField] private KeyCode pickupKey = KeyCode.E;
    [SerializeField] private string pickupPrompt = "Press E to pick up";

    [Header("CubeLight Reference")]
    [SerializeField] private GameObject existingCubeLight; // Przeci¹gnij tutaj CubeLight z hierarchy

    private bool playerInRange = false;
    private GameObject player;
    public FlickeringLight flickeringLight; // Drag in the Inspector
    public GameObject myCanvas;

    // --- NOWA ZMIENNA STATYCZNA ---
    public static bool hasCube = false;
    // -----------------------------

    void Start()
    {
        // Resetuj stan przy starcie sceny (jeœli to konieczne, np. przy ponownym ³adowaniu poziomu)
        // Jeœli chcesz, aby stan by³ zachowany miêdzy scenami, musia³byœ u¿yæ np. DontDestroyOnLoad dla obiektu zarz¹dzaj¹cego stanem gry.
        // Dla prostoty tego przyk³adu, resetujemy go tutaj.
        // Jeœli masz mened¿era gry, lepiej zarz¹dzaæ tym stanem tam.
        hasCube = false; // Upewnij siê, ¿e na starcie gracz nie ma kostki

        player = GameObject.FindGameObjectWithTag("Player");

        if (existingCubeLight == null)
        {
            existingCubeLight = GameObject.Find("CubeLight");
        }

        if (existingCubeLight != null)
        {
            existingCubeLight.SetActive(false);
        }
    }

    void Update()
    {
        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.transform.position);
            playerInRange = distance <= pickupRange;
        }

        if (playerInRange && Input.GetKeyDown(pickupKey))
        {
            PickUp();
            if (myCanvas != null) // Dodano sprawdzenie czy myCanvas jest przypisany
            {
                myCanvas.SetActive(true);
            }
        }
    }

    private void PickUp()
    {
        if (existingCubeLight != null)
        {
            existingCubeLight.SetActive(true);
            PlayerMovement playerMovement = FindFirstObjectByType<PlayerMovement>();
            if (playerMovement != null) // Dodano sprawdzenie
            {
                playerMovement.Intutorial = true;
            }
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            Debug.Log("CubeLight activated!");
        }
        else
        {
            Debug.LogError("CubeLight reference not set!");
        }

        // --- USTAW FLAGÊ ---
        hasCube = true;
        Debug.Log("Cube has been picked up! Teleport is now potentially active.");
        // -------------------

        Destroy(gameObject);
    }

    void OnGUI()
    {
        if (playerInRange)
        {
            GUI.Label(new Rect(Screen.width / 2 - 100, Screen.height / 2 + 50, 200, 30),
                      pickupPrompt, new GUIStyle()
                      {
                          alignment = TextAnchor.MiddleCenter,
                          fontSize = 16,
                          normal = new GUIStyleState() { textColor = Color.white }
                      });
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, pickupRange);
    }
}