// --- DifficultyManager.cs (Final Menu Version) ---
using UnityEngine;
using TMPro; // Required for TextMeshProUGUI

public class DifficultyManager : MonoBehaviour
{
    // Usuni�to Singleton, poniewa� ten skrypt b�dzie istnia� tylko w scenie menu
    // i nie musi przetrwa� zmiany sceny.

    [Header("UI Display")]
    [Tooltip("Text element to display the current difficulty.")]
    [SerializeField] private TextMeshProUGUI currentDifficultyText;

    private const string DifficultyKey = "SelectedDifficulty";

    private void Start()
    {
        // --- ZMIANA ---
        // Usun�li�my logik�, kt�ra aktualizowa�a tekst przy starcie.
        // Teraz tekst na pocz�tku b�dzie taki, jaki ustawi�e� w edytorze.
        // Mo�esz go zostawi� pustym.
        if (currentDifficultyText != null)
        {
            currentDifficultyText.text = ""; // Opcjonalnie: wyczy�� tekst na starcie
        }
    }

    // --- Publiczne metody dla przycisk�w UI ---

    public void SetEasyMode()
    {
        // 1. Zapisz wyb�r, aby scena z gr� mog�a go odczyta�
        PlayerPrefs.SetInt(DifficultyKey, 0);
        PlayerPrefs.Save();

        // 2. Zaktualizuj tekst w menu, aby pokaza� wyb�r gracza
        UpdateDifficultyText(0);
        Debug.Log("Difficulty set to EASY and saved.");
    }

    public void SetMediumMode()
    {
        PlayerPrefs.SetInt(DifficultyKey, 1);
        PlayerPrefs.Save();
        UpdateDifficultyText(1);
        Debug.Log("Difficulty set to MEDIUM and saved.");
    }

    public void SetHardMode()
    {
        PlayerPrefs.SetInt(DifficultyKey, 2);
        PlayerPrefs.Save();
        UpdateDifficultyText(2);
        Debug.Log("Difficulty set to HARD and saved.");
    }

    // Ta prywatna metoda aktualizuje tekst i jego kolor w UI menu.
    private void UpdateDifficultyText(int level)
    {
        if (currentDifficultyText == null)
        {
            Debug.LogWarning("CurrentDifficultyText is not assigned in the Inspector. Cannot display difficulty.");
            return;
        }

        switch (level)
        {
            case 0: // Easy
                currentDifficultyText.text = "Currently Selected Difficulty: <color=green>EASY</color>";
                break;
            case 1: // Medium
                currentDifficultyText.text = "Currently Selected Difficulty: <color=yellow>MEDIUM</color>";
                break;
            case 2: // Hard
                currentDifficultyText.text = "Currently Selected Difficulty: <color=red>HARD</color>";
                break;
        }
    }
}