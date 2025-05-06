using UnityEngine;
using UnityEngine.UI;

public class ChargingStation : MonoBehaviour
{

    public Slider chargeBar;         // Reference to the UI bar
    private bool playerInRange = false;
    
    [SerializeField] public float maxCharge = 100f;   // Max charge value
    void Update()
    {
        // Press E to charge when player is near
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            chargeBar.value = maxCharge;
            Debug.Log("Charge filled!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
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


