using UnityEngine;

public class SpawnTeleport : MonoBehaviour
{
    [Tooltip("Where the player should appear in the forest")]
    [SerializeField] private Transform destination;

    private void OnTriggerEnter(Collider other)
    {
        // --- SPRAWDè CZY GRACZ MA KOSTK  ---
        if (!CubePickup.hasCube)
        {
            Debug.Log("Teleport inactive: Cube not picked up yet.");
            return; // Nie rÛb nic, jeúli kostka nie zosta≥a zebrana
        }
        // ------------------------------------

        // Only react to the player
        if (!other.CompareTag("Player")) return;

        Debug.Log("Player with cube entered teleport trigger. Teleporting...");

        // If the player uses a CharacterController, briefly disable it
        var controller = other.GetComponent<CharacterController>();
        if (controller != null) controller.enabled = false;

        // Snap position + rotation
        if (destination != null) // Dodano sprawdzenie czy destination jest przypisane
        {
            other.transform.SetPositionAndRotation(destination.position, destination.rotation);
        }
        else
        {
            Debug.LogError("Destination for teleport is not set!", this.gameObject);
        }


        if (controller != null) controller.enabled = true;
    }
}
