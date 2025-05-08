using UnityEngine;

public class ChargingStation : MonoBehaviour
{
    private bool playerInRange = false;
    private EnergyController playerEnergy;

    [SerializeField] private float rechargeAmount = 100f;

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.Q))
        {
            if (playerEnergy != null)
            {
                playerEnergy.AddEnergy(rechargeAmount);
                Debug.Log("Energy fully recharged!");
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            playerEnergy = other.GetComponent<EnergyController>();
            Debug.Log("Player in range of charging station.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            playerEnergy = null;
            Debug.Log("Player left charging station.");
        }
    }
}



