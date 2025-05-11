using UnityEngine;
using UnityEngine.SceneManagement;

public class TotemLightingUp : MonoBehaviour
{
    [Header("Totem Settings")]
    [SerializeField] private Material lightUpMaterial;
    [SerializeField] private string winSceneName = "GameWon";

    private bool isLit = false;

    // Statyczny licznik dla wszystkich totemów
    private static int litTotems = 0;
    private const int REQUIRED_TOTEMS = 5;

    // Metoda do zapalenia totemu
    public void LightUp()
    {
        // Jeśli totem jest już zapalony, nic nie rób
        if (isLit)
            return;

        // Zmień materiał totemu
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.material = lightUpMaterial;
        }

        // Włącz światło, jeśli istnieje
        Light light = GetComponent<Light>();
        if (light != null)
        {
            light.enabled = true;
        }

        // Oznacz totem jako zapalony
        isLit = true;
        litTotems++;

        Debug.Log($"Totem zapalony! Aktualna liczba: {litTotems}/{REQUIRED_TOTEMS}");

        // Sprawdź, czy wszystkie totemy są zapalone
        if (litTotems >= REQUIRED_TOTEMS)
        {
            Debug.Log("Wszystkie totemy zapalone! Wczytywanie sceny: " + winSceneName);
            SceneManager.LoadScene(winSceneName);
        }
    }

    // Możesz dodać tę metodę, jeśli totem jest aktywowany przez strzał
    // Przykład: Totem reaguje na kolizję pocisku z tagiem "Bullet"
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Bullet"))
        {
            LightUp();
        }
    }

    // Alternatywna metoda, jeśli używasz triggerów
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bullet"))
        {
            LightUp();
        }
    }

    // Metoda do resetowania stanu totemów (opcjonalna)
    public static void ResetTotemProgress()
    {
        litTotems = 0;
        Debug.Log("Zresetowano postęp totemów");
    }
}