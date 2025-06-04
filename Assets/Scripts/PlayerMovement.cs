using System.Runtime.CompilerServices;

using UnityEngine;
// using UnityEngine.InputSystem; // This was in your script, but not used by the GetAxis calls. Remove if not using Input System package actions.


public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 16f;
    [SerializeField] private float sprintSpeed = 24f;
    [SerializeField] private Stamina staminaSystem;

    [SerializeField] private GameObject inventoryObject;
    [SerializeField] private GameObject cameraObject;

    // Added by user:
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float gravity = -9.81f;
    private bool inInventory = false;
    private float verticalVelocity;
    private bool inTutorial = false; // Backing field for Intutorial property

    // Property as provided by user (lowercase 't')
    public bool Intutorial { set { inTutorial = value; } }

    private CharacterController characterController;
    private Camera camera; // Using 'new' to hide any potential inherited member

    // Audio
    private AudioSource source;
    private Vector3 previousPosition;   
    private float totalDistance = 0;
    [Tooltip("Sound effects for wood")][SerializeField] private GameObject woodSteps;
    [Tooltip("Sound effects for grass")][SerializeField] private GameObject grassSteps;
    [Tooltip("Sound effects for carpet")][SerializeField] private GameObject carpetSteps;

    private void Start()
    {
        characterController = GetComponent<CharacterController>();

        // Get Stamina component from this GameObject if not assigned in Inspector
        if (staminaSystem == null)
        {
            staminaSystem = GetComponent<Stamina>();
            if (staminaSystem == null)
            {
                Debug.LogError("PlayerMovement: Stamina system not found on the player or not assigned.", this.gameObject);
            }
        }

        // Assign camera from the specified cameraObject or use the main camera as a fallback
        if (cameraObject != null)
        {
            camera = cameraObject.GetComponent<Camera>();
            if (camera == null)
            {
                Debug.LogError("PlayerMovement: Camera component not found on the assigned cameraObject. Falling back to Camera.main.", this.gameObject);
                camera = Camera.main;
            }
        }
        else
        {
            camera = Camera.main;
        }

        if (camera == null)
        {
            Debug.LogError("PlayerMovement: No camera found (neither assigned nor Camera.main). Movement might not work as expected.", this.gameObject);
        }

        source = GetComponent<AudioSource>();
    }

    private void Update()
    {
        Vector3 currentPosition = transform.localPosition;
        float distance = Mathf.Abs(Vector3.Magnitude(currentPosition - previousPosition));
        float pitch = GetPitchFromDistance(distance);
        source.pitch = pitch;
        totalDistance = totalDistance + distance;

        // Audio
        if (totalDistance >= 8.4f)
        {
            Vector3 rayOrigin = transform.position;
            RaycastHit hit;
            if (Physics.Raycast(rayOrigin, Vector3.down, out hit, 6f))
            {
                if (LayerMask.LayerToName(hit.collider.gameObject.layer) == "Wood")
                {
                    GameObject woodenFootsteps = Instantiate(woodSteps);
                    woodenFootsteps.transform.position = transform.position;
                    totalDistance = 0;
                }
                else if (LayerMask.LayerToName(hit.collider.gameObject.layer) == "Ground")
                {
                    GameObject grassFootsteps = Instantiate(grassSteps);
                    grassFootsteps.transform.position = transform.position;
                    totalDistance = 0;
                }
                else if (LayerMask.LayerToName(hit.collider.gameObject.layer) == "Carpet")
                {
                    GameObject carpetFootsteps = Instantiate(carpetSteps);
                    carpetFootsteps.transform.position = transform.position;
                    totalDistance = 0;
                }
                else //failsafe
                {
                    GameObject grassFootsteps = Instantiate(grassSteps);
                    grassFootsteps.transform.position = transform.position;
                    totalDistance = 0;
                }
            }
        }
        previousPosition = currentPosition;

        // Only allow movement if not in inventory and not in the tutorial state
        if (!inInventory && !inTutorial) // Check the backing field directly for read access
        {
            // --- Jump and gravity ---
            if (characterController.isGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = -2f; // Small downward force when grounded
            }

            if (Input.GetButtonDown("Jump") && characterController.isGrounded)
            {
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
            verticalVelocity += gravity * Time.deltaTime; // Apply gravity

            // --- Movement + sprint + stamina ---
            float horizontalInput = Input.GetAxis("Horizontal");
            float verticalInput = Input.GetAxis("Vertical");
            Vector3 input = new Vector3(horizontalInput, 0f, verticalInput);

            // Calculate movement direction relative to camera
            Vector3 forward = Vector3.zero;
            Vector3 right = Vector3.zero;

            if (camera != null)
            {
                forward = camera.transform.forward;
                right = camera.transform.right;
            }
            forward.y = 0; // Keep movement planar
            right.y = 0;   // Keep movement planar
            // Normalizing camera vectors can be good practice if camera has non-unit scale or unusual pitch.
            // forward.Normalize();
            // right.Normalize();

            // Original direction calculation from your script
            Vector3 direction = forward * input.z + right * input.x;

            // Determine if sprinting
            // Player must be trying to move for sprint speed to apply (input.sqrMagnitude check)
            bool isActuallyMoving = input.sqrMagnitude > 0.01f;
            bool canSprint = Input.GetKey(KeyCode.LeftShift)
                                && staminaSystem != null
                                && staminaSystem.HasStamina()
                                && isActuallyMoving; // Only allow sprint if actually moving

            float speed = canSprint ? sprintSpeed : moveSpeed;

            // If not normalizing 'direction' before multiplying by speed, diagonal movement will be faster.
            // To ensure consistent speed in all directions, normalize 'direction' if input is significant:
            // if (isActuallyMoving) direction.Normalize();
            Vector3 move = direction.normalized * speed; // Normalized direction for consistent speed
            if (!isActuallyMoving) move = Vector3.zero; // Ensure no movement if no input, even if direction was calculated from tiny joystick drift

            move.y = verticalVelocity; // Add vertical movement (jump/gravity)

            characterController.Move(move * Time.deltaTime);
            // --------------------------------------------
        }
    }

    private float GetPitchFromDistance(float distance)
    {
        return Mathf.Clamp(1f + (distance / 5f), 0.5f, 2f);
    }
}
