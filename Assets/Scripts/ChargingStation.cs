using UnityEngine;
using UnityEngine.UI;

public class ChargingStation : MonoBehaviour
{

    public Slider chargeBar;         // Reference to the UI bar

    private bool playerInRange = false;

    [Range(0, 100)]
    [SerializeField] public float chargeAmount = 100f;   // Max charge value

    [SerializeField] private Transform spawnPoint;
    [SerializeField] private GameObject chargeablePrefab;

    void Update()
    {
        // Press E to charge when player is near
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            GameObject placedObject = Instantiate(chargeablePrefab, spawnPoint.position, spawnPoint.rotation);
            Debug.Log("Placed object on station.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInRange = true;

        if (other.CompareTag("Chargeable"))
        {
            chargeBar.value = chargeAmount;
            Debug.Log("Charging started.");
        }

    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }
}



