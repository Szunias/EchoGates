using UnityEngine;
using System.Linq; // Required for using All()

public class TotemDoor : MonoBehaviour
{
    [Tooltip("List of totems that must be activated to open this door.")]
    [SerializeField] private TutorialTotem[] totemsRequired;

    [Tooltip("The door GameObject that will be deactivated/opened. You can also assign an Animator here.")]
    [SerializeField] private GameObject doorGameObject;
    // Optionally, if using an Animator to open the door:
    // [SerializeField] private Animator doorAnimator;
    // [SerializeField] private string openAnimationTrigger = "Open";

    private bool areAllTotemsLit = false;
    private bool isOpen = false;

    void Update()
    {
        // If the door is already open, do nothing more
        if (isOpen)
        {
            return;
        }

        // Check if all required totems have been assigned
        if (totemsRequired == null || totemsRequired.Length == 0)
        {
            Debug.LogWarning("No totems assigned to the door!", this.gameObject);
            // You can decide if the door should be open by default or remain locked
            // For example, to keep it locked:
            // return;
            // To have it open (if there are no requirements):
            // OpenDoor();
            // return;
            // For safety, let's assume it stays locked if unconfigured
            return;
        }

        // Check the status of all totems
        CheckTotemStatus();

        if (areAllTotemsLit && !isOpen)
        {
            OpenDoor();
        }
    }

    void CheckTotemStatus()
    {
        // If any of the totems are not assigned in the inspector, log an error and do not proceed
        if (totemsRequired.Any(totem => totem == null))
        {
            Debug.LogError("One or more required totems are not assigned in the Inspector!", this.gameObject);
            areAllTotemsLit = false; // Ensure it's false if there's a configuration error
            return;
        }

        // Check if all totems in the list have their IsLit property set to true
        // We use Linq.All() to check this condition
        areAllTotemsLit = totemsRequired.All(totem => totem.IsLit);
    }

    void OpenDoor()
    {
        isOpen = true;
        Debug.Log("All totems activated! Opening door...");

        if (doorGameObject != null)
        {
            // Example: deactivate the door object to "open" it
            doorGameObject.SetActive(false);
            Debug.Log($"Door {doorGameObject.name} has been opened (deactivated).");
        }
        // Optionally, if using an animator:
        // if (doorAnimator != null)
        // {
        //     doorAnimator.SetTrigger(openAnimationTrigger);
        //     Debug.Log($"Door opening animation '{openAnimationTrigger}' triggered.");
        // }
        else if (doorGameObject == null) // Only log warning if doorGameObject is also null (and not using animator alternative)
        {
            Debug.LogWarning("Door GameObject (doorGameObject) or Animator not assigned!", this.gameObject);
        }
    }

    // Optional: You can add a visual aid in the editor to show connections to totems
    private void OnDrawGizmosSelected()
    {
        if (totemsRequired == null || totemsRequired.Length == 0) return;

        Gizmos.color = Color.yellow;
        foreach (TutorialTotem totem in totemsRequired)
        {
            if (totem != null)
            {
                Gizmos.DrawLine(transform.position, totem.transform.position);
            }
        }
    }
}