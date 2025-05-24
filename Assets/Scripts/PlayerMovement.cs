using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 16f;
    [SerializeField] private float sprintSpeed = 24f;
    [SerializeField] private Stamina staminaSystem;

    [SerializeField] private GameObject inventoryObject;
    [SerializeField] private GameObject cameraObject;

    // **DODANE**:
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float gravity = -9.81f;
    private bool inInventory = false;
    private float verticalVelocity;
    private bool inTutorial = false;
    public bool Intutorial { set { inTutorial = value; } }

    private CharacterController characterController;
    private new Camera camera;

    private void Start()
    {
        characterController = GetComponent<CharacterController>();

        // Pobierz Stamina z tego obiektu, jeśli nie w Inspectorze
        if (staminaSystem == null)
            staminaSystem = GetComponent<Stamina>();

        // Przypisz kamerę z pola lub domyślną
        if (cameraObject != null)
            camera = cameraObject.GetComponent<Camera>();
        else
            camera = Camera.main;
    }

    private void Update()
    {

        if (!inInventory && !inTutorial)
        {

            // --- Skok i grawitacja (tak jak było) ---
            if (characterController.isGrounded && verticalVelocity < 0f)
                verticalVelocity = -2f;
            if (Input.GetButtonDown("Jump") && characterController.isGrounded)
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            verticalVelocity += gravity * Time.deltaTime;

            // --- Ruch + sprint + stamina ---
            Vector3 input = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
            Vector3 forward = camera.transform.forward; forward.y = 0;
            Vector3 right = camera.transform.right; right.y = 0;
            Vector3 direction = forward * input.z + right * input.x;

            bool isSprinting = Input.GetKey(KeyCode.LeftShift)
                                && staminaSystem != null
                                && staminaSystem.HasStamina();

            float speed = isSprinting ? sprintSpeed : moveSpeed;

            Vector3 move = direction * speed;
            move.y = verticalVelocity;
            characterController.Move(move * Time.deltaTime);
            // --------------------------------------------
        }

        // ... reszta Twojego kodu (inwentaryzacja, interakcje) bez zmian ...
    }
}
