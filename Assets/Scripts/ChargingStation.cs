using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ChargingStation : MonoBehaviour
{

    public Slider chargeBar;  // Reference to the UI bar

    private bool playerInRange = false;

    private GameObject placedObject;

    [Range(0, 100)]
    [SerializeField] public float chargeAmount = 100f;  // Max charge value

    [SerializeField] private Transform spawnPoint;
    [SerializeField] private GameObject chargeablePrefab;
    [SerializeField] private float chargeSpeed = 10f;

    private bool isObjectPlaced = false;

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
            if (!isObjectPlaced)
            {
                SpawnObject();
            }
            else if (chargeBar.value >= chargeAmount)
            {
                RetrieveObject();
            }
        }
    }

    private void SpawnObject()
    {

        placedObject = Instantiate(chargeablePrefab, spawnPoint.position, spawnPoint.rotation);
        chargeBar.value = 0f;
        chargeBar.maxValue = chargeAmount;

        Debug.Log("Placed object: " + placedObject);
        Debug.Log("Object placed on charging station.");

        isObjectPlaced = true;
        StartCoroutine(ChargeObject());
    }

 

    private IEnumerator ChargeObject()
    {
        while (chargeBar.value < chargeAmount && isObjectPlaced)
        {
            chargeBar.value += Time.deltaTime * chargeSpeed;
            yield return null;
        } 
    }

   private void RetrieveObject()
    {
        if (placedObject != null && chargeBar.value >= chargeAmount)
        {
            Debug.Log("Placed object: " + placedObject);
            Debug.Log("Destroying placed object...");

            Destroy(placedObject);

            placedObject = null;
            chargeBar.value = 0;
            isObjectPlaced = false;

            Debug.Log("Object collected and reset.");
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



