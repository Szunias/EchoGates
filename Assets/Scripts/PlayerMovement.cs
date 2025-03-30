using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 16;

    private Vector3 spawnPoint;
    private CharacterController characterController;
    private new Camera camera;

    private void Start()
    {
        characterController = GetComponent<CharacterController>();
        spawnPoint = transform.position;
        camera = Camera.main;
    }

    private void Update()
    {

        Vector3 input = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));

        Vector3 forward = camera.transform.forward;
        Vector3 right = camera.transform.right;

        forward.y = 0f;
        right.y = 0f;

        Vector3 direction = forward * input.z + right * input.x;


        characterController.Move(direction * moveSpeed * Time.deltaTime);
    }

    public void ResetPosition()
    {
        characterController.enabled = false;
        transform.position = spawnPoint;
        characterController.enabled = true;
    }
}
