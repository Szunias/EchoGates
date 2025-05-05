using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 16;
    [SerializeField] private GameObject inventoryObject;
    [SerializeField] private GameObject cameraObject;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip inventoryOpenClip;
    [SerializeField] private AudioClip inventoryCloseClip;

    // --- Jump variables (added) ------------------------------------------------
    [Header("Jump Settings")]
    [SerializeField] private float jumpHeight = 2f;          // How high the player jumps
    [SerializeField] private float gravity = -9.81f;         // Gravity acceleration (negative)
    private float verticalVelocity;                          // Current Y velocity
    // --------------------------------------------------------------------------

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

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (!audioSource) audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (!inInventory)
        {
            // --- Jump logic (added) -------------------------------------------
            if (characterController.isGrounded && verticalVelocity < 0f)
                verticalVelocity = -2f; // small push to keep grounded

            if (Input.GetButtonDown("Jump") && characterController.isGrounded)
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity); // initial jump impulse

            verticalVelocity += gravity * Time.deltaTime; // apply gravity
            // ------------------------------------------------------------------

            Vector3 input = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));

            Vector3 forward = camera.transform.forward;
            Vector3 right = camera.transform.right;

            forward.y = 0f;
            right.y = 0f;

            Vector3 direction = forward * input.z + right * input.x;

            Vector3 move = direction * moveSpeed;   // horizontal movement
            move.y = verticalVelocity;               // vertical movement added

            characterController.Move(move * Time.deltaTime);
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

            if (audioSource)
            {
                AudioClip clip = inInventory ? inventoryOpenClip : inventoryCloseClip;
                if (clip) audioSource.PlayOneShot(clip);
            }

            Cursor.lockState = inInventory ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = inInventory;

            if (inInventory)
            {
                InventoryManager.Instance.ListItems(); 
            }
        }
    }

    public void Interaction(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            LayerMask layerMask = LayerMask.GetMask("PickUp");
            if (Physics.Raycast(camera.transform.position, camera.transform.forward, out RaycastHit hit, maxDistance, layerMask))
            {
                GameObject hitObject = hit.collider.gameObject;
                hitObject.GetComponent<ItemPickup>().Pickup();
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
