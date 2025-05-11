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
        bool rightMouseDown = Input.GetMouseButton(1);
        bool hasEnoughEnergy = energy.HasEnergy(0.1f);
        // Debug.Log($"Right Mouse: {rightMouseDown}, Has Energy: {hasEnoughEnergy}, Current Energy: {energy.Percent}");

        if (rightMouseDown && hasEnoughEnergy)
        {
            if (!pointLight.enabled)
            {
                pointLight.enabled = true;
                Debug.Log("Point light ENABLED");
            }

            energy.ConsumeEnergy(energyPerSecond * Time.deltaTime);

            float energyFactor = (energy.Percent + 25) / energy.MaxEnergy;
            pointLight.range = maxLightRange * energyFactor;
            pointLight.intensity = maxIntensity * energyFactor;
            // Debug.Log($"EnergyFactor: {energyFactor}, Range: {pointLight.range}, Intensity: {pointLight.intensity}");

            if (!energy.HasEnergy(0.1f)) // Ponownie sprawdŸ po zu¿yciu
            {
                if (pointLight.enabled) // Wy³¹cz tylko jeœli by³o w³¹czone
                {
                    pointLight.enabled = false;
                    Debug.Log("Point light DISABLED (energy depleted during use)");
                }
            }
        }
        else
        {
            if (pointLight.enabled)
            {
                pointLight.enabled = false;
                Debug.Log("Point light DISABLED (mouse up or no initial energy)");
            }
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
