using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Light), typeof(LineRenderer))]
public class CubeLight : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform beamOrigin;
    [SerializeField] private EnergyController energy;

    [Header("Light Settings")]
    [SerializeField] private float maxLightRange = 15f;
    [SerializeField] private float maxIntensity = 100f;
    [SerializeField] private float energyPerSecond = 2f;

    [Header("Beam Settings")]
    [SerializeField] private float beamRange = 100f;
    [SerializeField] private float fireRate = 0.5f;
    [SerializeField] private float beamDuration = 0.5f;
    [SerializeField] private float beamEnergyCost = 10f;
    [SerializeField] private Material beamMaterial;

    private Light pointLight;
    private LineRenderer beamLine;

    private float beamCooldownTimer = 0f;

    void Start()
    {
        //to hide the cursor and lock it to the center of the screen (where the crosshair is)
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        pointLight = GetComponent<Light>();
        pointLight.type = LightType.Point;
        pointLight.enabled = false;

        beamLine = GetComponent<LineRenderer>();
        beamLine.material = beamMaterial;
        beamLine.startWidth = 0.15f;
        beamLine.endWidth = 0.15f;
        beamLine.enabled = false;
    }

    void Update()
    {
        HandleLightInput();
        HandleBeamInput();
    }

    private void HandleLightInput()
    {
        if (Input.GetMouseButton(1) && energy.HasEnergy(0.1f))
        {
            if (!pointLight.enabled)
                pointLight.enabled = true;

            energy.ConsumeEnergy(energyPerSecond * Time.deltaTime);

            float energyFactor = (energy.Percent + 25) / energy.MaxEnergy; //the +25 is so that  the pointlight doesn't get too small when energy is low
            pointLight.range = maxLightRange * energyFactor;
            pointLight.intensity = maxIntensity * energyFactor;

            if (!energy.HasEnergy(0.1f))
                pointLight.enabled = false;
        }
        else
        {
            if (pointLight.enabled)
                pointLight.enabled = false;
        }
    }


    //Maybe you can put this on the motherfaka with a new script (that you need to create)...
    private void HandleBeamInput()
    {
        beamCooldownTimer += Time.deltaTime;

        if (Input.GetButtonDown("Fire1") && beamCooldownTimer >= fireRate)
        {
            if (!energy.ConsumeEnergy(beamEnergyCost))
                return;

            beamCooldownTimer = 0f;
            FireBeam();
        }
    }
    //... and also this
    private void FireBeam()
    {
        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
        beamLine.SetPosition(0, beamOrigin.position);

        Vector3 hitPoint;

        if (Physics.Raycast(ray, out RaycastHit hit, beamRange))
        {
            hitPoint = hit.point;

            // Debugging hit information
            Debug.Log("Raycast hit: " + hit.transform.name);  // Log the name of the object hit

            // Check if we hit the totem (with TotemLightingUp script)
            TotemLightingUp totem = hit.transform.GetComponent<TotemLightingUp>();
            if (totem != null)
            {
                Debug.Log("Totem hit, calling LightUp()");  // Log when Totem is hit
                totem.LightUp();
            }
        }
        else
        {
            hitPoint = ray.origin + ray.direction * beamRange;
        }

        beamLine.SetPosition(1, hitPoint);
        StartCoroutine(ShootBeam());
    }

    private IEnumerator ShootBeam()
    {
        beamLine.enabled = true;
        yield return new WaitForSeconds(beamDuration);
        beamLine.enabled = false;
    }
}
