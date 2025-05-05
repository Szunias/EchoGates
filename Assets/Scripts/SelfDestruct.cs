using UnityEngine;

public class SelfDestruct : MonoBehaviour
{
    [Tooltip("Lifetime")]
    public float lifetime = 6f;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }
}
