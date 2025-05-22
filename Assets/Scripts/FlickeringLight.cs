using UnityEngine;
using System.Collections;

public class FlickeringLight : MonoBehaviour
{
    public Light lightSource;
    public Renderer targetRenderer; // The object with the emissive material
    public float flickerDuration = 30f;
    public float minFlickerDelay = 0.05f;
    public float maxFlickerDelay = 0.2f;

    private bool isFlickering = false;
    private Material mat;

    public void StartFlicker()
    {
        if (!isFlickering)
            StartCoroutine(FlickerRoutine());
    }

    private IEnumerator FlickerRoutine()
    {
        isFlickering = true;
        float elapsedTime = 0f;
        mat = targetRenderer.material;

        while (elapsedTime < flickerDuration)
        {
            bool flickerOn = Random.value > 0.5f;

            // Toggle the light
            lightSource.enabled = flickerOn;

            // Toggle material emission
            if (flickerOn)
                mat.EnableKeyword("_EMISSION");
            else
                mat.DisableKeyword("_EMISSION");

            float waitTime = Random.Range(minFlickerDelay, maxFlickerDelay);
            yield return new WaitForSeconds(waitTime);
            elapsedTime += waitTime;
        }

        // Turn everything off at the end
        lightSource.enabled = false;
        mat.DisableKeyword("_EMISSION");
        isFlickering = false;
    }

    void Start()
    {
        StartFlicker(); // Optional: remove if triggering from another script
    }
}
