using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEditor.Progress;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 16;
    [SerializeField] private GameObject inventoryObject;
    [SerializeField] private GameObject cameraObject;

    private Vector3 spawnPoint;
    private CharacterController characterController;
    private new Camera camera;
    private bool inInventory;
    private Canvas inventoryCanvas;
    private CameraMovement cameraComponent;

    private const float maxDistance = 2.5f;

    private void Start()
    {
        characterController = GetComponent<CharacterController>();
        spawnPoint = transform.position;
        camera = Camera.main;

        inventoryCanvas = inventoryObject.GetComponent<Canvas>();
        inventoryCanvas.enabled = false;

        cameraComponent = cameraObject.GetComponent<CameraMovement>();
    }

    private void Update()
    {
        if (!inInventory) 
        {
            Vector3 input = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));

            Vector3 forward = camera.transform.forward;
            Vector3 right = camera.transform.right;

            forward.y = 0f;
            right.y = 0f;

            Vector3 direction = forward * input.z + right * input.x;


            characterController.Move(direction * moveSpeed * Time.deltaTime);
        }
        
    }

    public void ResetPosition()
    {
        characterController.enabled = false;
        transform.position = spawnPoint;
        characterController.enabled = true;
    }

    public void OpenInventory(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            inInventory = !inInventory;

            inventoryCanvas.enabled = inInventory;

            cameraComponent.EnableRotation(!inInventory);
        }
    }
    
    public void Interaction(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            LayerMask layerMask = LayerMask.GetMask("PickUp");
            RaycastHit hit;
            if (Physics.Raycast(camera.transform.position, camera.transform.forward, out hit, maxDistance, layerMask))
            {
                GameObject hitObject = hit.collider.gameObject;
                hitObject.GetComponent<ItemPickup>().Pickup();

                //Debug.DrawRay(camera.transform.position, camera.transform.forward * hit.distance, Color.magenta,2f);
                Debug.Log("Hit");
            }
            else
            {
                Debug.DrawRay(camera.transform.position, camera.transform.forward * 2.5f, Color.blue, 2f);
                Debug.Log("Not hit");
            }

        }
    }
}

