using UnityEngine;

public class TutorialTotem : MonoBehaviour
{
    [SerializeField] private Material emissiveMaterial;

    private Renderer rend;
    private Material originalMaterial;

    private bool isLitUp = false;

    void Start()
    {
        rend = GetComponent<Renderer>();
        originalMaterial = rend.material;
    }

    public void LightUp()
    {
        if (emissiveMaterial != null)
        {
            rend.material = emissiveMaterial;
            DynamicGI.SetEmissive(rend, emissiveMaterial.GetColor("_EmissionColor"));
        }

        isLitUp = true;
    }
}
