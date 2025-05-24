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


    void Start()
    {
        // ZnajdŸ gracza
        player = GameObject.FindGameObjectWithTag("Player");

        // Jeœli nie przypisano rêcznie, spróbuj znaleŸæ
        if (existingCubeLight == null)
        {
            existingCubeLight = GameObject.Find("CubeLight"); // Lub inna nazwa
        }

        // Upewnij siê ¿e CubeLight jest wy³¹czony na start
        if (existingCubeLight != null)
        {
            existingCubeLight.SetActive(false);
        }

        //myCanvas.SetActive(false);
    }

    void Update()
    {
        // SprawdŸ odleg³oœæ do gracza
        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.transform.position);
            playerInRange = distance <= pickupRange;
        }

        // Obs³uga podnoszenia
        if (playerInRange && Input.GetKeyDown(pickupKey))
        {
            PickUp();
            myCanvas.SetActive(true);
        }
    }

    private void PickUp()
    {
        // Aktywuj istniej¹cy CubeLight
        if (existingCubeLight != null)
        {
            existingCubeLight.SetActive(true);
            FindFirstObjectByType<PlayerMovement>().Intutorial = true;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            Debug.Log("CubeLight activated!");
        }
        else
        {
            Debug.LogError("CubeLight reference not set!");
        }

        // Zniszcz ten cube
        Destroy(gameObject);
        if (flickeringLight != null)
        {
            flickeringLight.StartFlicker();
        }
    }

    void OnGUI()
    {
        if (playerInRange)
        {
            // Poka¿ prompt
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
        // Poka¿ zasiêg podnoszenia
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, pickupRange);
    }
    
}