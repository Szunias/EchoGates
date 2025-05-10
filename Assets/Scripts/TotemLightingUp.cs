using System.Collections;  // Ensure this is here for IEnumerator and coroutines
using UnityEngine;

public class TotemLightingUp : MonoBehaviour
{
    [Header("Totem Settings")]
    [SerializeField] private float totemTime = 0.5f; // Time to light up each layer
    [SerializeField] private Material lightUpMaterial;

    [Header("Totem Game Objects")]
    [SerializeField] private GameObject level1;
    [SerializeField] private GameObject level2;
    [SerializeField] private GameObject level3;
    [SerializeField] private GameObject level4;

    private int currentLayer = 0;
    private bool isCoolingDown = false;

    // This method will be called to light up the totem
    public void LightUp()
    {
        if (isCoolingDown || currentLayer >= 4)
            return; // If it's cooling down or all layers are lit, stop.

        StartCoroutine(LightNextLayer()); // Start coroutine to light up the next layer
    }

    // Coroutine to light up one layer of the totem
    private IEnumerator LightNextLayer()
    {
        isCoolingDown = true;

        // Lighting up the appropriate layer based on currentLayer
        if (currentLayer == 0)
        {
            level1.GetComponent<MeshRenderer>().material = lightUpMaterial;
            Debug.Log("Lit level 1");
        }
        else if (currentLayer == 1)
        {
            level2.GetComponent<MeshRenderer>().material = lightUpMaterial;
            Debug.Log("Lit level 2");
        }
        else if (currentLayer == 2)
        {
            level3.GetComponent<MeshRenderer>().material = lightUpMaterial;
            Debug.Log("Lit level 3");
        }
        else if (currentLayer == 3)
        {
            level4.GetComponent<MeshRenderer>().material = lightUpMaterial;
            Debug.Log("Lit level 4");
        }

        currentLayer++;  // Move to the next layer
        yield return new WaitForSeconds(totemTime);  // Wait for the specified time before lighting up next layer

        isCoolingDown = false; // Cooldown finished, ready to light the next layer
    }
}
