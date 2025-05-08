using UnityEngine;

public class TotemLightingUp : RaycastBeam
{
    [Header("Totem Settings")]
    [SerializeField] public bool beamHit = true; //needs to be changed when raycast is hit
    [Tooltip("Amount of time for each level to light up")] [SerializeField] public float totemTime = 0.5f; 
    [Tooltip("Material used for the totem to light up")] [SerializeField] private Material lightUpMaterial;

    private int layers = 4;

    void Update()
    {
        //beam hits object
        if (beamHit)
        {
            float startTotemTime = totemTime;
            while (layers != 0)
            {
                startTotemTime -= Time.deltaTime;
                if (totemTime < 0)
                {
                    GameObject lightUp = GameObject.Find("Level " + layers);
                    lightUp.GetComponent<Renderer>().material = lightUpMaterial;
                    layers--;   
                    startTotemTime = totemTime;
                }
            }
        }

    }
}
