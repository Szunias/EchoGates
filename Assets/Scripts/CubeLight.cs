using UnityEngine;
using System.Collections;
using System.Runtime.CompilerServices;

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

    [Header("Tutorial Subtitles")]
    [SerializeField] private SubtitleManager subtitleManager;
    [SerializeField] private string[] rightClickSubtitles;
    [SerializeField] private float subtitleDelay = 3f;

    [Header("Audio Sources")] // <-- NOWE: Sekcja na źródła dźwięku
    [SerializeField] private AudioSource audioSourceLightLoop; // <-- NOWE: Przeciągnij tu AudioSource dla dźwięku świecenia (RMB)
    [SerializeField] private AudioSource audioSourceBeamShot;  // <-- NOWE: Przeciągnij tu AudioSource dla dźwięku strzału (LMB)

    /* --------------------  RUNTIME -------------------- */
    private Light pointLight;
    private LineRenderer beamLine;
    private float beamCooldown = 0f;

    private Renderer cubeRenderer;
    private Material runtimeMat;
    private readonly int emissionID = Shader.PropertyToID("_EmissionColor");

    private bool beamFlashActive = false;
    private bool rightClickTriggered = false;

    // Usunięto stare referencje do audio, ponieważ teraz używamy publicznych pól powyżej

    /* =================================================== */
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

        cubeRenderer = GetComponent<Renderer>();
        runtimeMat = cubeRenderer.material;
        runtimeMat.EnableKeyword("_EMISSION");
        SetEmission(false);

        // Usunięto GetComponent<AudioSource>(), ponieważ teraz przypisujemy źródła w Inspektorze
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

        if (!rightClickTriggered && Input.GetMouseButtonDown(1))
        {
            rightClickTriggered = true;
            if (subtitleManager != null && rightClickSubtitles.Length > 0)
            {
                StartCoroutine(ShowRightClickSubtitles());
            }
        }

        if (rmbHeld && enoughEnergy)
        {
            if (!pointLight.enabled)
            {
                pointLight.enabled = true;
                // <-- NOWE: Odtwórz zapętlony dźwięk świecenia
                if (audioSourceLightLoop != null)
                {
                    audioSourceLightLoop.Play();
                }
            }

            energy.ConsumeEnergy(energyPerSec * Time.deltaTime);

            float factor = (energy.Percent + 25f) / energy.MaxEnergy;
            pointLight.range = maxLightRange * factor;
            pointLight.intensity = maxIntensity * factor;

            float stunRadius = useManualStunRadius ? manualStunRadius : pointLight.range;
            StunSpidersInArea(stunRadius, 3f);

            SetEmission(true);

            if (!energy.HasEnergy(0.1f))
            {
                pointLight.enabled = false;
                // <-- NOWE: Zatrzymaj dźwięk świecenia, gdy zabraknie energii
                if (audioSourceLightLoop != null)
                {
                    audioSourceLightLoop.Stop();
                }
            }
        }
        else
        {
            if (pointLight.enabled)
            {
                pointLight.enabled = false;
                // <-- NOWE: Zatrzymaj dźwięk świecenia, gdy puścisz przycisk
                if (audioSourceLightLoop != null)
                {
                    audioSourceLightLoop.Stop();
                }
            }

            if (!beamFlashActive)
            {
                SetEmission(false);
            }
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
        // <-- NOWE: Odtwórz pojedynczy dźwięk strzału
        if (audioSourceBeamShot != null)
        {
            // Można dodać losową zmianę tonu dla urozmaicenia, jeśli chcesz
            // audioSourceBeamShot.pitch = Random.Range(0.9f, 1.1f);
            audioSourceBeamShot.Play();
        }

        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f));
        beamLine.SetPosition(0, beamOrigin.position);
        Vector3 hitPoint;

        // Usunięto starą logikę odtwarzania dźwięku

        if (Physics.Raycast(ray, out RaycastHit hit, beamRange))
        {
            hitPoint = hit.point;
            if (hit.transform.TryGetComponent<spiderAI>(out var spider))
                spider.Stun(5f);
            if (hit.transform.TryGetComponent<TotemLightingUp>(out var totem))
                totem.LightUp();
            if (hit.transform.TryGetComponent<TutorialTotem>(out var tutorialTotem))
            {
                tutorialTotem.LightUp();
            }
        }
        else
        {
            hitPoint = ray.origin + ray.direction * beamRange;
        }

        beamLine.SetPosition(1, hitPoint);
        StartCoroutine(FlashEmission());
        StartCoroutine(ShootBeam());
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
        beamFlashActive = true;
        SetEmission(true);
        yield return new WaitForSeconds(beamDuration);
        beamFlashActive = false;
        if (!Input.GetMouseButton(1))
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
        float radius = useManualStunRadius ? manualStunRadius : (Application.isPlaying && pointLight != null ? pointLight.range : maxLightRange);
        Gizmos.color = Color.yellow;
        Transform center = beamOrigin != null ? beamOrigin : transform;
        Gizmos.DrawWireSphere(center.position, radius);
    }

    private IEnumerator ShowRightClickSubtitles()
    {
        foreach (var line in rightClickSubtitles)
        {
            subtitleManager.ShowSubtitle(line);
            yield return new WaitForSeconds(subtitleDelay);
        }
    }
}