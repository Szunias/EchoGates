using UnityEngine;
using UnityEngine.InputSystem;

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
    
}

