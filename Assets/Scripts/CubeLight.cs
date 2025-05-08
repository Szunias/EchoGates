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
    [SerializeField] private float maxLightRange = 6f;
    [SerializeField] private float maxIntensity = 2f;
    [SerializeField] private float energyPerSecond = 5f;

    [Header("Beam Settings")]
    [SerializeField] private float beamRange = 50f;
    [SerializeField] private float fireRate = 0.2f;
    [SerializeField] private float beamDuration = 0.05f;
    [SerializeField] private float beamEnergyCost = 15f;
    [SerializeField] private Material beamMaterial;

    private Light pointLight;
    private LineRenderer beamLine;

    private float beamCooldownTimer = 0f;

    void Start()
    {
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

            float energyFactor = energy.Percent / energy.MaxEnergy;
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

    private void FireBeam()
    {
        Vector3 origin = beamOrigin.position;
        Vector3 direction = playerCamera.transform.forward;

        beamLine.SetPosition(0, origin);

        if (Physics.Raycast(origin, direction, out RaycastHit hit, beamRange))
        {
            beamLine.SetPosition(1, hit.point);

            if (hit.transform.CompareTag("Enemy"))
            {
                Destroy(hit.transform.gameObject);
            }
        }
        else
        {
            beamLine.SetPosition(1, origin + direction * beamRange);
        }

        StartCoroutine(ShootBeam());
    }

    private IEnumerator ShootBeam()
    {
        beamLine.enabled = true;
        yield return new WaitForSeconds(beamDuration);
        beamLine.enabled = false;
    }
}
