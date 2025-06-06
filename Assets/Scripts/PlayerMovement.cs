
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 16f;
    [SerializeField] private float sprintSpeed = 24f;
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float gravity = -9.81f;

    [Header("System References")]
    [SerializeField] private Stamina staminaSystem;
    [SerializeField] private GameObject cameraObject;
    // Usunięto pole: [SerializeField] private GameObject inventoryObject;

    [Header("Footstep Audio")]
    [Tooltip("Sound effects for wood")][SerializeField] private GameObject woodSteps;
    [Tooltip("Sound effects for grass")][SerializeField] private GameObject grassSteps;
    [Tooltip("Sound effects for carpet")][SerializeField] private GameObject carpetSteps;

    // --- State Control ---
    private bool movementDisabled = false; // Flaga do blokowania ruchu z innych skryptów (np. SubtitleManager)

    // --- Private Components & Variables ---
    private CharacterController characterController;
    private new Camera camera;
    private AudioSource source;
    private Vector3 verticalVelocity;
    private Vector3 previousPosition;
    private float totalDistance = 0;

    private void Start()
    {
        characterController = GetComponent<CharacterController>();
        source = GetComponent<AudioSource>();

        if (staminaSystem == null) { staminaSystem = GetComponent<Stamina>(); }
        if (staminaSystem == null) { Debug.LogError("PlayerMovement: Stamina system not found or assigned.", this); }

        if (cameraObject != null) { camera = cameraObject.GetComponent<Camera>(); }
        if (camera == null) { camera = Camera.main; }
        if (camera == null) { Debug.LogError("PlayerMovement: No camera found. Movement will be world-relative.", this); }

        previousPosition = transform.position;
    }

    private void Update()
    {
        // --- MASTER MOVEMENT CHECK ---
        // Usunięto sprawdzanie 'inInventory'. Teraz tylko 'movementDisabled' może zablokować ruch.
        if (movementDisabled)
        {
            // Możesz zostawić ten log do testów lub go usunąć
            // Debug.LogWarning($"Movement BLOCKED. Reason: movementDisabled={movementDisabled}");
            return;
        }

        HandleMovement();
        HandleFootsteps();
    }

    private void HandleMovement()
    {
        // --- Jump and gravity ---
        if (characterController.isGrounded && verticalVelocity.y < 0f)
        {
            verticalVelocity.y = -2f;
        }

        if (Input.GetButtonDown("Jump") && characterController.isGrounded)
        {
            verticalVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
        verticalVelocity.y += gravity * Time.deltaTime;

        // --- Horizontal Movement ---
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        Vector3 input = new Vector3(horizontalInput, 0f, verticalInput);

        Vector3 forward = camera != null ? camera.transform.forward : Vector3.forward;
        Vector3 right = camera != null ? camera.transform.right : Vector3.right;
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        Vector3 direction = forward * input.z + right * input.x;

        // --- Sprinting ---
        bool isMoving = direction.sqrMagnitude > 0.01f;
        bool canSprint = Input.GetKey(KeyCode.LeftShift) && staminaSystem != null && staminaSystem.HasStamina() && isMoving;
        float currentSpeed = canSprint ? sprintSpeed : moveSpeed;

        // --- Apply Movement ---
        Vector3 move = direction.normalized * currentSpeed;
        move.y = verticalVelocity.y;
        characterController.Move(move * Time.deltaTime);
    }

    private void HandleFootsteps()
    {
        Vector3 currentPosition = transform.position;
        float distance = Vector3.Distance(currentPosition, previousPosition);
        totalDistance += distance;

        if (totalDistance >= 8.4f)
        {
            totalDistance = 0;
            RaycastHit hit;
            if (Physics.Raycast(transform.position, Vector3.down, out hit, 6f))
            {
                GameObject stepPrefab = null;
                string layerName = LayerMask.LayerToName(hit.collider.gameObject.layer);

                if (layerName == "Wood") stepPrefab = woodSteps;
                else if (layerName == "Carpet") stepPrefab = carpetSteps;
                else stepPrefab = grassSteps; // Domyślnie trawa

                if (stepPrefab != null)
                {
                    Instantiate(stepPrefab, transform.position, Quaternion.identity);
                }
            }
        }
        previousPosition = currentPosition;
    }

    // --- PUBLIC METHODS FOR EXTERNAL CONTROL ---

    /// <summary>
    /// Disables player movement. Called by other scripts like SubtitleManager.
    /// </summary>
    public void DisableMovement()
    {
        movementDisabled = true;
        Debug.Log("<color=red>PlayerMovement: Movement DISABLED by an external script.</color>");
    }

    /// <summary>
    /// Enables player movement. Called by other scripts like SubtitleManager.
    /// </summary>
    public void EnableMovement()
    {
        movementDisabled = false;
        Debug.Log("<color=green>PlayerMovement: Movement ENABLED by an external script.</color>");
    }
}