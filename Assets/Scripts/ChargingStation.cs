using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections;

public class ChargingStation : MonoBehaviour
{
    [SerializeField] private EnergyController playerEnergy;
    [SerializeField] GameObject playerCube;
    [SerializeField] private float energyReward = 25f;
    [SerializeField] public float chargeAmount = 100f; 
    [SerializeField] private float chargeSpeed = 10f;

    private bool playerInRange = false;
    private GameObject _placedObject;
    private bool isObjectPlaced = false;

    [SerializeField] private Transform spawnPoint;
    //[SerializeField] private GameObject chargeablePrefab;
    [SerializeField] private InputActionReference interactAction;

    private Transform originalParent;
    [SerializeField] private Transform holdPoint; 
    private Vector3 originalPosition;
    private Quaternion originalRotation;

    private void OnEnable()
    {
        if(interactAction == null || interactAction.action == null)
        {
            Debug.LogError("Interaction Action is not assigned");
            return;
        }
        interactAction.action.performed += OnInteract;
        interactAction.action.Enable();
    }

    private void OnDisable()
    {
        if (interactAction != null && interactAction.action != null)
        {
            interactAction.action.performed -= OnInteract;
            interactAction.action.Disable();
        }
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
        if (_placedObject == null)
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

        if ( playerCube == null)
        {
            Debug.LogError("Player cube not assigned.");
            return;
        }
        {
            
        }

        //originalParent = playerCube.transform.parent;

        playerCube.transform.SetParent(spawnPoint);
        playerCube.transform.localPosition = Vector3.zero;
        playerCube.transform.localRotation = Quaternion.identity;

        _placedObject = playerCube;
        isObjectPlaced = true;

        Debug.Log("Placed object: " + _placedObject);
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
        if (_placedObject != null && playerEnergy.Percent >= chargeAmount)
        {
            Debug.Log("Placed object: " + _placedObject);
            Debug.Log("Destroying placed object...");

            _placedObject.transform.SetParent(holdPoint);
            _placedObject.transform.localPosition = new Vector3(0.671000004f, -0.247999996f, 0.0189999994f);
            _placedObject.transform.localRotation = Quaternion.identity;

            //Destroy(_placedObject);

            _placedObject = null;
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




