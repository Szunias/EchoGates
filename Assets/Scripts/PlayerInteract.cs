using UnityEngine;

public class InteractionNpc : MonoBehaviour 
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            float InteractRange = 2f;
            Physics.OverlapSphere(transform.position, InteractRange);
            Collider[] colliderArray = Physics.OverlapSphere(transform.position, InteractRange);
            foreach (Collider collider in colliderArray)
            {
                if (collider.TryGetComponent(out NpcInteractable npcInteractable))
                {
                    npcInteractable.Interact(); 
                }
            }
        }
    }
}
