using UnityEngine;

public class LightOnClick : MonoBehaviour 
{
    public Material glowMaterial;      // Emissive material with bloom
    public Material defaultMaterial;   // Non-glowing material
    public float glowDuration = 2f;    // How long it glows
    private bool isGlowing = false;
    private Renderer rend;

    void Start()
    {
        rend = GetComponent<Renderer>();
        rend.material = defaultMaterial;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1) && !isGlowing) // Right mouse button
        {
            StartCoroutine(StartGlow());
        }
    }

    System.Collections.IEnumerator StartGlow()
    {
        isGlowing = true;
        rend.material = glowMaterial;

        yield return new WaitForSeconds(glowDuration);

        rend.material = defaultMaterial;
        isGlowing = false;
    } 
}
