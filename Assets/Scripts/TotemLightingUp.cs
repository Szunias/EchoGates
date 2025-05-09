using System.Collections;
using UnityEngine;

public class TotemLightingUp : MonoBehaviour
{
    [Header("Totem Settings")]
    [Tooltip("Amount of time for each level to light up")] [SerializeField] public float totemTime = 0.5f; 
    [Tooltip("Material used for the totem to light up")] [SerializeField] private Material lightUpMaterial;

    [Header("Totem Game Objects")]
    [SerializeField] private GameObject level1;
    [SerializeField] private GameObject level2;
    [SerializeField] private GameObject level3;
    [SerializeField] private GameObject level4;

    private bool isRunning = true;
    private bool isLit = false;
    private int layers = 4;

    public void Update()
    {
           if (isLit)
        {
            if (isRunning)
            {
                if (layers == 4)
                {
                    level4.GetComponent<MeshRenderer>().material = lightUpMaterial;
                }
                else if (layers == 3)
                {
                    level3.GetComponent<MeshRenderer>().material = lightUpMaterial;

                }
                else if (layers == 2)
                {
                    level2.GetComponent<MeshRenderer>().material = lightUpMaterial;
                }
                else
                {
                    level1.GetComponent<MeshRenderer>().material = lightUpMaterial;
                }
                layers--;
                StartCoroutine(StartCooldown());
            }
        }
    }

    public void LightUp()
    {
        isLit = true;
    }

    IEnumerator StartCooldown()
    {
        isRunning = false;

        yield return new WaitForSeconds(totemTime);

        isRunning = true;

    }
}
