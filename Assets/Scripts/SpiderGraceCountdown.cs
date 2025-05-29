using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>Wyświetla odliczanie (mm:ss) do końca grace-period pająka.</summary>
public class SpiderGraceCountdown : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Pająk z gracePeriod. Puste = pierwszy spiderAI w scenie.")]
    [SerializeField] private spiderAI spider;

    [Tooltip("Pole TextMeshProUGUI lub UI.Text. Puste = komponent na tym obiekcie.")]
    [SerializeField] private Component textField;

    private float graceDuration;
    private float startTime;
    private bool running = false;

    void OnEnable()
    {
        // zawsze wyczyść tekst startowy (np. “Countdown” wpisany w Inspectorze)
        ResetText();
    }

    void Awake()
    {
        // ---- znajdź pająka ----
        if (spider == null)
            spider = Object.FindAnyObjectByType<spiderAI>();

        if (spider == null)
        {
            Debug.LogWarning($"{name}/{GetType().Name}: nie znaleziono spiderAI.");
            // możemy kontynuować, ale licznik będzie pusty
            return;
        }

        // ---- znajdź pole tekstowe ----
        if (textField == null)
            textField = GetComponent<TMP_Text>() ?? (Component)GetComponent<Text>();

        if (textField == null)
        {
            Debug.LogWarning($"{name}/{GetType().Name}: brak komponentu Text/TMP_Text.");
            return;
        }

        graceDuration = spider.gracePeriod;
        startTime = Time.time;
        running = true;
    }

    void Update()
    {
        if (!running) return;

        float remaining = Mathf.Max(0f, graceDuration - (Time.time - startTime));
        int totalSec = Mathf.CeilToInt(remaining);
        int minutes = totalSec / 60;
        int seconds = totalSec % 60;
        string txt = $"{minutes:00}:{seconds:00}";

        WriteText(txt);

        if (remaining <= 0.01f)
        {
            ResetText();      // znikamy po odliczeniu
            running = false;
        }
    }

    private void WriteText(string msg)
    {
        if (textField is TMP_Text tmp)
            tmp.text = msg;
        else if (textField is Text ui)
            ui.text = msg;
    }

    private void ResetText() => WriteText(string.Empty);
}
