using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

/// <summary>
/// Pokazuje ekran sterowania na starcie i blokuje gracza
/// aż do pierwszego wciśnięcia dowolnego przycisku.
/// </summary>
public class ControlsOverlay : MonoBehaviour
{
    [Header("UI root (Panel)")]
    [SerializeField] private GameObject overlayRoot;

    [Header("Co wyłączyć podczas overlayu")]
    [SerializeField] private MonoBehaviour[] componentsToDisable;   // ← wrzuć PlayerMovement (+ ew. inne)

    private bool _dismissed;

    private void Awake()
    {
        // fallback, gdyby ktoś nie podpiął w inspektorze
        if (!overlayRoot) overlayRoot = gameObject;

        // dezaktywuj sterowanie/wzrok
        foreach (var c in componentsToDisable)
            if (c) c.enabled = false;

        // pokaż panel + odblokuj kursor + pauza
        overlayRoot.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0f;

        // czekamy na dowolne wejście z nowego Input Systemu
        InputSystem.onAnyButtonPress.CallOnce(_ => HideOverlay());
    }

    private void HideOverlay()
    {
        if (_dismissed) return;
        _dismissed = true;

        // przywróć sterowanie
        foreach (var c in componentsToDisable)
            if (c) c.enabled = true;

        overlayRoot.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Time.timeScale = 1f;
    }
}
