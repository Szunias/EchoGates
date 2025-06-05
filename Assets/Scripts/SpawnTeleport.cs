using UnityEngine;
using System.Linq;
using System.Collections;

public class SpawnTeleport : MonoBehaviour
{
    [Header("Teleport Settings")]
    [Tooltip("Where the player should appear after teleporting")]
    [SerializeField] private Transform destination;

    [Tooltip("List of totems that must be lit up for this teleport to activate. If empty, no totems are required.")]
    [SerializeField] private TutorialTotem[] totemsRequiredForTeleport;

    [Header("Door Cube Settings")]
    [Tooltip("The GameObject representing the door cube that will rotate (this should be the Hinge/Pivot object if you set one up).")]
    [SerializeField] private GameObject doorCube;

    [Tooltip("The rotation to apply to the door cube to open it (e.g., (0, 90, 0) for 90 degrees on Y axis).")]
    [SerializeField] private Vector3 doorOpenRotationAngle = new Vector3(0, 90, 0);

    [Tooltip("How long the door cube rotation should take.")]
    [SerializeField] private float doorRotationDuration = 1.0f;

    [Header("Door Sound Settings")]
    [Tooltip("The sound to play when the door starts opening.")]
    [SerializeField] private AudioClip doorOpenSound;

    [Tooltip("The AudioSource component that will play the door open sound.")]
    [SerializeField] private AudioSource doorAudioSource;

    private bool conditionsMetForOpening = false;
    private bool isDoorCubeOpen = false;

    private void Update()
    {
        if (isDoorCubeOpen || !CubePickup.hasCube)
        {
            return;
        }

        bool allTotemsLit = true;
        if (totemsRequiredForTeleport != null && totemsRequiredForTeleport.Length > 0)
        {
            // Check for unassigned totems first
            if (totemsRequiredForTeleport.Any(totem => totem == null))
            {
                Debug.LogError("Teleport/Door inactive: One or more totems are not assigned!", this.gameObject);
                return;
            }
            // Check if all assigned totems are lit
            if (!totemsRequiredForTeleport.All(totem => totem.IsLit))
            {
                allTotemsLit = false;
            }
        }

        // If conditions were not met before, but are now met
        if (!conditionsMetForOpening && allTotemsLit)
        {
            conditionsMetForOpening = true;
            Debug.Log("Conditions met. Opening door cube and playing sound...");

            // Play sound
            if (doorAudioSource != null && doorOpenSound != null)
            {
                doorAudioSource.PlayOneShot(doorOpenSound);
            }

            // Start rotating door
            if (doorCube != null)
            {
                StartCoroutine(RotateDoorCubeCoroutine());
                isDoorCubeOpen = true;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") || !conditionsMetForOpening)
        {
            return;
        }

        Debug.Log("Player entered teleport trigger, and all conditions are met. Teleporting...");

        // --- HIDE SUBTITLES AND STOP TUTORIAL ---
        if (SubtitleManager.Instance != null)
        {
            SubtitleManager.Instance.HideAndClear();
        }
        if (Tutorial.Instance != null)
        {
            Tutorial.Instance.StopTutorial();
        }
        // ------------------------------------------

        var controller = other.GetComponent<CharacterController>();
        if (controller != null)
        {
            controller.enabled = false;
        }

        if (destination != null)
        {
            other.transform.SetPositionAndRotation(destination.position, destination.rotation);
        }
        else
        {
            Debug.LogError("Destination for teleport is not set!", this.gameObject);
        }

        if (controller != null)
        {
            controller.enabled = true;
        }
    }

    private IEnumerator RotateDoorCubeCoroutine()
    {
        if (doorCube == null)
        {
            Debug.LogWarning("Door Cube is not assigned. Cannot rotate.", this.gameObject);
            yield break;
        }

        Quaternion initialRotation = doorCube.transform.rotation;
        Quaternion targetRotation = initialRotation * Quaternion.Euler(doorOpenRotationAngle);
        float elapsedTime = 0f;

        Debug.Log($"Door cube rotating from {initialRotation.eulerAngles} to {targetRotation.eulerAngles} over {doorRotationDuration}s");

        while (elapsedTime < doorRotationDuration)
        {
            doorCube.transform.rotation = Quaternion.Slerp(initialRotation, targetRotation, elapsedTime / doorRotationDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        doorCube.transform.rotation = targetRotation;
        Debug.Log("Door cube has finished rotating.");
    }
}