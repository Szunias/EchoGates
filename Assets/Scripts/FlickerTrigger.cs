using UnityEngine;

public class Flicker : MonoBehaviour
{
    public FlickeringLight flickeringLight; // Drag in the Inspector

    void Start()
    {
        if (flickeringLight != null)
        {
            flickeringLight.StartFlicker();
        }
    }
}
