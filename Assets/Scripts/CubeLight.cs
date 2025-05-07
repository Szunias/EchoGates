using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Light))]
public class CubeLight : MonoBehaviour
{
    public int MouseButton = 1; //for the right mouse button

    [Header("Energy Settings")]
    [Range(0, 100)]
    public float EnergyPercent = 100f; // Total energy left (0–100%)
    public float EnergyDrainPerUse = 10f; // Energy used per cast

    [Header("Light Settings")]
    public float MaxRange = 6f; // Light radius at 100%
    public float MaxIntensity = 2f; // Brightness at 100%
    public float LightDuration = 4f; // Light stays on for this duration

    private Light LightCube;
    private float LightTimer = 0f;
    private bool IsCasting = false;

    void Start()
    {
        LightCube = GetComponent<Light>();
        LightCube.type = LightType.Point;
        LightCube.enabled = false;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(MouseButton))
        {
            if (!IsCasting && EnergyPercent >= EnergyDrainPerUse)
            {
                IsCasting = true;
                LightTimer = LightDuration;

                EnergyPercent -= EnergyDrainPerUse;
                float energyFactor = EnergyPercent / 100f;

                LightCube.range = MaxRange * energyFactor; 
                LightCube.intensity = MaxIntensity * energyFactor;
                LightCube.enabled = true;
            }
        }

        // Timer countdown
        if (IsCasting)
        {
            LightTimer -= Time.deltaTime;

            if (LightTimer <= 0f)
            {
                LightCube.enabled = false;
                IsCasting = false;
            }
        }
    }
}
