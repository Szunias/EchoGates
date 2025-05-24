using Unity.VisualScripting;
using UnityEngine;

public class CloseTutorial : MonoBehaviour
{
    public void CloseUi()
    {
        FindFirstObjectByType<PlayerMovement>().Intutorial = false;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;
        Destroy(gameObject);
    }
}
