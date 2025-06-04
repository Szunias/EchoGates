using UnityEngine;
using System.Linq;    // Required for checking all totems easily
using System.Collections; // Required for Coroutines

public class SpawnTeleport : MonoBehaviour
{
    [Header("Teleport Settings")]
    [Tooltip("Where the player should appear after teleporting")]
    [SerializeField] private Transform destination;

    [Tooltip("List of totems that must be lit up for this teleport to activate. If empty, no totems are required.")]
    [SerializeField] private TutorialTotem[] totemsRequiredForTeleport;

    [Header("Door Cube Settings")]
    [Tooltip("The GameObject representing the door cube that will rotate (this should be the Hinge/Pivot object if you set one up).")]
    [SerializeField] private GameObject doorCube; // This should be the Hinge/Pivot

    [Tooltip("The rotation to apply to the door cube to open it (e.g., (0, 90, 0) for 90 degrees on Y axis).")]
    [SerializeField] private Vector3 doorOpenRotationAngle = new Vector3(0, 90, 0);

    [Tooltip("How long the door cube rotation should take.")]
    [SerializeField] private float doorRotationDuration = 1.0f;

    [Header("Door Sound Settings")] // NEW SECTION
    [Tooltip("The sound to play when the door starts opening.")]
    [SerializeField] private AudioClip doorOpenSound; // NEW: AudioClip for the sound

    [Tooltip("The AudioSource component that will play the door open sound. Can be on the door, teleport, or a sound manager.")]
    [SerializeField] private AudioSource doorAudioSource; // NEW: AudioSource to play the sound

    private bool conditionsMetForOpening = false;
    private bool isDoorCubeOpen = false;

    private void Update()
    {
        if (isDoorCubeOpen)
        {
            return;
        }

        if (!CubePickup.hasCube)
        {
            conditionsMetForOpening = false;
            return;
        }

        if (totemsRequiredForTeleport != null && totemsRequiredForTeleport.Length > 0)
        {
            if (totemsRequiredForTeleport.Any(totem => totem == null))
            {
                if (!conditionsMetForOpening)
                    Debug.LogError("Teleport/Door inactive: One or more totems in 'Totems Required For Teleport' are not assigned!", this.gameObject);
                conditionsMetForOpening = false;
                return;
            }

            if (!totemsRequiredForTeleport.All(totem => totem.IsLit))
            {
                conditionsMetForOpening = false;
                return;
            }
        }

        conditionsMetForOpening = true;

        if (doorCube != null && !isDoorCubeOpen) // isDoorCubeOpen check ensures this block runs once
        {
            Debug.Log("Conditions met. Opening door cube and playing sound...");

            // --- PLAY DOOR OPEN SOUND ---
            if (doorAudioSource != null && doorOpenSound != null)
            {
                doorAudioSource.PlayOneShot(doorOpenSound); // Play the sound
            }
            else
            {
                if (doorOpenSound != null && doorAudioSource == null)
                {
                    Debug.LogWarning("Door Open Sound is assigned, but Door Audio Source is not! Cannot play sound on " + gameObject.name);
                }
                // If doorOpenSound is null, no warning is needed, it's optional.
            }
            // ----------------------------

            StartCoroutine(RotateDoorCubeCoroutine());
            isDoorCubeOpen = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        if (!conditionsMetForOpening)
        {
            if (!CubePickup.hasCube)
            {
                Debug.Log("Teleport inactive: Cube not picked up yet.");
            }
            else
            {
                Debug.Log("Teleport inactive: Not all required totems are lit (or check Inspector setup).");
            }
            return;
        }

        Debug.Log("Player entered teleport trigger, and all conditions are met. Teleporting...");

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