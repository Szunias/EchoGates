using Unity.VisualScripting;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Light))]
[RequireComponent(typeof(LineRenderer))] 
public class CubeLight : MonoBehaviour
{
    public Camera playerCamera;
    public Transform beamOrigin;
    public int MouseButton = 1; //for the right mouse button

    [Header("Energy Settings")]
    [Range(0, 100)]
    public float EnergyPercent = 100f; // Total energy left (0–100%)
    public float EnergyDrainPerUse = 10f; // Energy used per cast

    [Header("Light Settings")]
    public float MaxRange = 6f; // Light radius at 100%
    public float MaxIntensity = 2f; // Brightness at 100%
    public float LightDuration = 4f; // Light stays on for this duration

    [Header("Beam Settings")]
    [Tooltip("Range of the beam")][SerializeField] public float soulRange = 50f;
    [Tooltip("Time between beam shots")][SerializeField] public float fireRate = 0.2f;
    [Tooltip("How long the soul activates its beam")][SerializeField] public float beamDuration = 0.05f; 

    private Light LightCube;
    private float LightTimer = 0f;
    private bool IsCasting = false;

    LineRenderer beamLine;
    float fireTimer;

    void Start()
    {
        beamLine = GetComponent<LineRenderer>();
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

        fireTimer += Time.deltaTime;
        if (Input.GetButtonDown("Fire1") && fireTimer > fireRate) //Fire1 is equal to right click
        {
            fireTimer = 0;
            beamLine.SetPosition(0, beamOrigin.position);
            Vector3 rayOrigin = playerCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0));
            RaycastHit hit;
            if (Physics.Raycast(rayOrigin, playerCamera.transform.forward, out hit, soulRange))
            {
                if (hit.transform.gameObject.name != "Cube") //Cube is the floor, should be changed accordingly
                {
                    beamLine.SetPosition(1, hit.point);
                    Destroy(hit.transform.gameObject); //can be changed to whatever we want to do when beam hits
                }
            }
            else
            {
                beamLine.SetPosition(1, rayOrigin + (playerCamera.transform.forward * soulRange));
            }
            StartCoroutine(ShootBeam());
        }

        IEnumerator ShootBeam()
        {
            beamLine.enabled = true;
            yield return new WaitForSeconds(beamDuration);
            beamLine.enabled = false;
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
