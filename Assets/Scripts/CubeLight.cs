using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Light), typeof(LineRenderer))]
public class CubeLight : MonoBehaviour
{
    [SerializeField] private Material beamMaterial;

    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform beamOrigin;

    [Header("Energy Settings")]
    [SerializeField] private EnergySettings energy;

    [Header("Light Settings")]
    [SerializeField] private LightSettings lightSettings;

    [Header("Beam Settings")]
    [SerializeField] private BeamSettings beamSettings;

    private Light pointLight;
    private LineRenderer beamLine;

    private float lightTimer = 0f;
    private float beamCooldownTimer = 0f;
    private bool isLightActive = false;

    void Start()
    {
        pointLight = GetComponent<Light>();
        pointLight.type = LightType.Point;
        pointLight.enabled = false;

        beamLine = GetComponent<LineRenderer>();
        beamLine.material = beamMaterial;       
        beamLine.enabled = false;

        beamLine.startWidth = 0.15f;  // Make the start of the beam thinner
        beamLine.endWidth = 0.15f;    // Make the end of the beam thinner 
    }

    void Update()
    {
        HandleLightInput();
        HandleBeamInput();
        UpdateLightTimer();
    }

    private void HandleLightInput()
    {
        if (Input.GetMouseButton(1) && energy.Percent > 0f)
        {
            if (!pointLight.enabled)
                pointLight.enabled = true;

            // Drain energy over time
            energy.Percent -= lightSettings.energyPerSecond * Time.deltaTime;
            energy.Percent = Mathf.Max(0f, energy.Percent);

            float energyFactor = energy.Percent / 100f;
            pointLight.range = lightSettings.maxRange * energyFactor;
            pointLight.intensity = lightSettings.maxIntensity * energyFactor;

            // Auto-disable if out of energy
            if (energy.Percent <= 0f)
            {
                pointLight.enabled = false;
            }
        }
        else
        {
            if (pointLight.enabled)
                pointLight.enabled = false;
        }
    }

    private void HandleBeamInput()
    {
        beamCooldownTimer += Time.deltaTime;

        if (Input.GetButtonDown("Fire1") && beamCooldownTimer >= beamSettings.fireRate)
        {
            if (energy.Percent < beamSettings.energyCost)
                return; // Not enough energy, don't fire

            beamCooldownTimer = 0f;
            energy.Percent -= beamSettings.energyCost;

            FireBeam();
        }
    }

    private void FireBeam()
    {
        Vector3 origin = beamOrigin.position;
        Vector3 direction = playerCamera.transform.forward;

        beamLine.SetPosition(0, origin);

        if (Physics.Raycast(origin, direction, out RaycastHit hit, beamSettings.range))
        {
            beamLine.SetPosition(1, hit.point);

            if (hit.transform.CompareTag("Enemy"))
            {
                Destroy(hit.transform.gameObject);
            }
        }
        else
        {
            beamLine.SetPosition(1, origin + direction * beamSettings.range);
        }

        StartCoroutine(ShootBeam());
    }

    private IEnumerator ShootBeam()
    {
        beamLine.enabled = true;
        yield return new WaitForSeconds(beamSettings.duration);
        beamLine.enabled = false;
    }

    private void UpdateLightTimer()
    {
        if (isLightActive)
        {
            lightTimer -= Time.deltaTime;

            if (lightTimer <= 0f)
            {
                pointLight.enabled = false;
                isLightActive = false;
            }
        }
    }

    // -------------------- Struct Definitions --------------------

    [System.Serializable]
    private struct EnergySettings
    {
        [Range(0, 100)] public float Percent;
    }

    [System.Serializable]
    private struct LightSettings
    {
        public float maxRange;
        public float maxIntensity;
        public float energyPerSecond;
    }

    [System.Serializable]
    private struct BeamSettings
    {
        public float range;
        public float fireRate;
        public float duration;
        public float energyCost;
    }
}
