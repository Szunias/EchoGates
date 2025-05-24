using Unity.VisualScripting;
using UnityEngine;

public class CloseTutorial : MonoBehaviour
{
    public FlickeringLight flickeringLight; // Drag in the Inspector
    public void CloseUi()
    {
        FindFirstObjectByType<PlayerMovement>().Intutorial = false;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;
        if (flickeringLight != null)
        {
            flickeringLight.StartFlicker();
        }
        Destroy(gameObject);
       
    }
    
}
