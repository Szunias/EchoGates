using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections;

public class ChargingStation : MonoBehaviour
{
    [SerializeField] private EnergyController playerEnergy;
    [SerializeField] private float energyReward = 25f;
    [SerializeField] public float chargeAmount = 100f; 
    [SerializeField] private float chargeSpeed = 10f;

    private bool playerInRange = false;
    private GameObject placedObject;
    private bool isObjectPlaced = false;

    [SerializeField] private Transform spawnPoint;
    [SerializeField] private GameObject chargeablePrefab;
    [SerializeField] private InputActionReference interactAction;

    private void OnEnable()
    {
        interactAction.action.performed += OnInteract;
        interactAction.action.Enable();
    }

    private void OnDisable()
    {
        interactAction.action.performed -= OnInteract;
        interactAction.action.Disable();
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        if (playerInRange)
        {
            HandleInteraction();
        }
    }

    private void HandleInteraction()
    {
        if (placedObject == null)
        {
            SpawnObject();
        }
        else if (isObjectPlaced && playerEnergy.Percent >= playerEnergy.MaxEnergy)
        {
            RetrieveObject();
        }
        else
        {
            Debug.Log("Charging in progress...");
        }
    }

    private void SpawnObject()
    {
        if ( playerEnergy.Percent >= playerEnergy.MaxEnergy)
        {
            Debug.Log("Energy already full. No need to charge. ");
            return;
        }

        placedObject = Instantiate(chargeablePrefab, spawnPoint.position, spawnPoint.rotation);
        isObjectPlaced = true;

        Debug.Log("Placed object: " + placedObject);
        Debug.Log("Object placed on charging station.");

        StartCoroutine(ChargeObject());
    }

    private IEnumerator ChargeObject()
    {
        while (playerEnergy.Percent < playerEnergy.MaxEnergy && isObjectPlaced)
        {
            playerEnergy.AddEnergy (Time.deltaTime * chargeSpeed);
            yield return null;
        }

        if (playerEnergy.Percent >= playerEnergy.MaxEnergy)
        {
            Debug.Log("Charging complete.");
        }
    }

    private void RetrieveObject()
    {
        if (placedObject != null && playerEnergy.Percent >= chargeAmount)
        {
            Debug.Log("Placed object: " + placedObject);
            Debug.Log("Destroying placed object...");

            Destroy(placedObject);

            placedObject = null;
            isObjectPlaced = false;
            playerEnergy.AddEnergy(energyReward);
            Debug.Log("Energy added to player. ");
        }
        else
        {
            Debug.Log("Object is not fully charged.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            Debug.Log("Player is in range to interact.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            Debug.Log("Player is out of range.");
        }
    }
}




