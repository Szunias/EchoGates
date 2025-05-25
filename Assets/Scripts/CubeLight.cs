using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Light), typeof(LineRenderer), typeof(Renderer))]
public class CubeLight : MonoBehaviour
{
    /* --------------------  INSPECTOR -------------------- */
    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform beamOrigin;
    [SerializeField] private EnergyController energy;

    [Header("Light Settings")]
    [SerializeField] private float maxLightRange = 15f;
    [SerializeField] private float maxIntensity = 100f;
    [SerializeField] private float energyPerSec = 2f;

    [Header("Beam Settings (LMB)")]
    [Tooltip("Maksymalny zasięg promienia – edytowalny")]
    [SerializeField] private float beamRange = 25f;
    [SerializeField] private float fireRate = 0.5f;
    [SerializeField] private float beamDuration = 0.5f;
    [SerializeField] private float beamEnergyCost = 10f;
    [SerializeField] private Material beamMaterial;

    [Header("Stun Settings (PPM)")]
    [Tooltip("Jeśli OFF – użyj pointLight.range")]
    [SerializeField] private bool useManualStunRadius = false;
    [SerializeField] private float manualStunRadius = 7f;
    [SerializeField] private bool debugStunSphere = true;

    [Header("Cube Emission")]
    [SerializeField] private Color emissionColor = Color.cyan;
    [SerializeField] private float emissionIntensity = 2f;

    /* --------------------  RUNTIME -------------------- */
    private Light pointLight;
    private LineRenderer beamLine;
    private float beamCooldown = 0f;

    private Renderer cubeRenderer;
    private Material runtimeMat;
    private readonly int emissionID = Shader.PropertyToID("_EmissionColor");

    /*  NEW: blokada gaśnięcia emisji w trakcie błysku LMB  */
    private bool beamFlashActive = false;

    /* =================================================== */
    void Start()
    {
        // Usunięte lub zakomentowane linie:
        // Cursor.lockState = CursorLockMode.Locked;
        // Cursor.visible = false;

        pointLight = GetComponent<Light>();
        pointLight.type = LightType.Point;
        pointLight.enabled = false;

        beamLine = GetComponent<LineRenderer>();
        beamLine.material = beamMaterial;
        beamLine.startWidth = 0.15f;
        beamLine.endWidth = 0.15f;
        beamLine.enabled = false;

        cubeRenderer = GetComponent<Renderer>();
        runtimeMat = cubeRenderer.material;
        runtimeMat.EnableKeyword("_EMISSION");
        SetEmission(false);
    }

    void Update()
    {
        HandlePointLight();  // PPM
        HandleBeam();        // LMB
    }

    /* ==================  PPM  ================== */
    private void HandlePointLight()
    {
        bool rmbHeld = Input.GetMouseButton(1);
        bool enoughEnergy = energy.HasEnergy(0.1f);

        if (rmbHeld && enoughEnergy)
        {
            if (!pointLight.enabled) pointLight.enabled = true;

            energy.ConsumeEnergy(energyPerSec * Time.deltaTime);

            float factor = (energy.Percent + 25f) / energy.MaxEnergy;
            pointLight.range = maxLightRange * factor;
            pointLight.intensity = maxIntensity * factor;

            float stunRadius = useManualStunRadius ? manualStunRadius : pointLight.range;
            StunSpidersInArea(stunRadius, 3f);

            SetEmission(true);                     // świeć w trakcie PPM

            if (!energy.HasEnergy(0.1f))
                pointLight.enabled = false;
        }
        else
        {
            if (pointLight.enabled) pointLight.enabled = false;

            /* --- gaś tylko, jeżeli NIE trwa błysk z LMB --- */
            if (!beamFlashActive)
                SetEmission(false);
        }
    }

    /* ==================  LPM  ================== */
    private void HandleBeam()
    {
        beamCooldown += Time.deltaTime;

        if (Input.GetButtonDown("Fire1") && beamCooldown >= fireRate)
        {
            if (!energy.ConsumeEnergy(beamEnergyCost)) return;

            beamCooldown = 0f;
            FireBeam();
        }
    }

    private void FireBeam()
    {
        Ray ray = playerCamera.ScreenPointToRay(
            new Vector3(Screen.width / 2f, Screen.height / 2f));

        beamLine.SetPosition(0, beamOrigin.position);
        Vector3 hitPoint;

        if (Physics.Raycast(ray, out RaycastHit hit, beamRange))
        {
            hitPoint = hit.point;

            if (hit.transform.TryGetComponent<spiderAI>(out var spider))
                spider.Stun(5f);

            if (hit.transform.TryGetComponent<TotemLightingUp>(out var totem))
                totem.LightUp();
        }
        else
        {
            hitPoint = ray.origin + ray.direction * beamRange;
        }

        beamLine.SetPosition(1, hitPoint);

        StartCoroutine(FlashEmission());     // błysk sześcianu
        StartCoroutine(ShootBeam());         // linia laseru
    }

    private IEnumerator ShootBeam()
    {
        beamLine.enabled = true;
        yield return new WaitForSeconds(beamDuration);
        beamLine.enabled = false;
    }

    /* ==================  EMISSION HELPERS  ================== */
    private void SetEmission(bool on)
    {
        if (on)
            runtimeMat.SetColor(emissionID, emissionColor * emissionIntensity);
        else
            runtimeMat.SetColor(emissionID, Color.black);
    }

    private IEnumerator FlashEmission()
    {
        beamFlashActive = true;      // blokujemy gaszenie
        SetEmission(true);

        yield return new WaitForSeconds(beamDuration);

        beamFlashActive = false;
        if (!Input.GetMouseButton(1))    // jeśli PPM nieprzytrzymany, gaś
            SetEmission(false);
    }

    /* ==================  OTHER HELPERS  ================== */
    private void StunSpidersInArea(float radius, float duration)
    {
        Collider[] cols = Physics.OverlapSphere(beamOrigin.position, radius);
        foreach (var col in cols)
        {
            if (col.TryGetComponent<spiderAI>(out var spider))
                spider.Stun(duration);
        }
    }

    /* ==================  DEBUG ================== */
    private void OnDrawGizmosSelected()
    {
        if (!debugStunSphere) return;

        float radius = useManualStunRadius
            ? manualStunRadius
            : (Application.isPlaying && pointLight != null ? pointLight.range : maxLightRange);

        Gizmos.color = Color.yellow;
        Transform center = beamOrigin != null ? beamOrigin : transform;
        Gizmos.DrawWireSphere(center.position, radius);
    }
}