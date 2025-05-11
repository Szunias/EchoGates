using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TotemLightingUp : MonoBehaviour
{
    /* --------------------  INSPECTOR -------------------- */
    [Header("Totem Settings")]
    [SerializeField] private float totemTime = 0.5f;          // czas pomiędzy warstwami
    [SerializeField] private Material lightUpMaterial;

    [Header("Totem Game Objects")]
    [SerializeField] private GameObject level1;
    [SerializeField] private GameObject level2;
    [SerializeField] private GameObject level3;
    [SerializeField] private GameObject level4;

    [Header("Win Scene")]
    [Tooltip("Scena wczytywana po zapaleniu wszystkich totemów")]
    [SerializeField] private string winSceneName = "GameWon";

    /* --------------------  RUNTIME -------------------- */
    private int currentLayer = 0;
    private bool isCoolingDown = false;
    private bool fullyLit = false;

    /* ---- statyczne śledzenie postępu gry ---- */
    private static int totalTotems = -1;   // ustawiane raz w Awake()
    private static int litTotems = 0;    // ile totemów już w pełni zapalonych

    /* =================================================== */
    void Awake()
    {
        // liczymy totemy tylko raz (przy pierwszym Awake)
        if (totalTotems < 0)
            totalTotems = FindObjectsOfType<TotemLightingUp>().Length;
    }

    /* =================================================== */
    /// <summary>Wywołaj tę metodę, aby zapalić kolejną warstwę.</summary>
    public void LightUp()
    {
        if (isCoolingDown || currentLayer >= 4)
            return;

        StartCoroutine(LightNextLayer());
    }

    private IEnumerator LightNextLayer()
    {
        isCoolingDown = true;

        /* ---------- zapalamy odpowiedni poziom ---------- */
        if (currentLayer == 0)
        {
            level1.GetComponent<MeshRenderer>().material = lightUpMaterial;
            EnableLight(level1);
        }
        else if (currentLayer == 1)
        {
            level2.GetComponent<MeshRenderer>().material = lightUpMaterial;
            EnableLight(level2);
        }
        else if (currentLayer == 2)
        {
            level3.GetComponent<MeshRenderer>().material = lightUpMaterial;
            EnableLight(level3);
        }
        else if (currentLayer == 3)
        {
            level4.GetComponent<MeshRenderer>().material = lightUpMaterial;
            EnableLight(level4);
        }

        currentLayer++;

        /* ---------- totem w pełni zapalony? ---------- */
        if (currentLayer >= 4 && !fullyLit)
            OnTotemFullyLit();

        yield return new WaitForSeconds(totemTime);
        isCoolingDown = false;
    }

    /* =================================================== */
    private void EnableLight(GameObject levelObj)
    {
        Light l = levelObj.GetComponent<Light>();
        if (l != null) l.enabled = true;
    }

    private void OnTotemFullyLit()
    {
        fullyLit = true;
        litTotems++;

        if (litTotems >= totalTotems)
        {
            // wszystkie totemy gotowe → wygrana
            SceneManager.LoadScene(winSceneName);
        }
    }
}
