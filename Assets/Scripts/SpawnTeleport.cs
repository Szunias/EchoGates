using UnityEngine;

public class SpawnTeleport : MonoBehaviour  
{
    [Tooltip("Where the player should appear in the forest")]
    [SerializeField] private Transform destination;

    private void OnTriggerEnter(Collider other)
    {
        // Only react to the player
        if (!other.CompareTag("Player")) return;

        // If the player uses a CharacterController, briefly disable it
        var controller = other.GetComponent<CharacterController>();
        if (controller != null) controller.enabled = false;

        // Snap position + rotation
        other.transform.SetPositionAndRotation(destination.position, destination.rotation);

        if (controller != null) controller.enabled = true;
    }
}
