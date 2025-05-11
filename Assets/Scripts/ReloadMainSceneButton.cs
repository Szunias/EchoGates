using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Podłącz do obiektu przycisku.  <br/>
/// 1.  Gdy menu się aktywuje, pokazuje kursor. <br/>
/// 2.  Po kliknięciu ładuje wskazaną scenę, ponownie chowa i blokuje kursor
///     (CubeLight i tak zrobi to w Start, ale tu masz pewność nawet
///      gdyby scenę wczytywał kto inny).
/// </summary>
[RequireComponent(typeof(Button))]
public class ReloadMainSceneButton : MonoBehaviour
{
    [Tooltip("Scena do ponownego wczytania")]
    [SerializeField] private string sceneName = "MainScene";

    private Button _btn;

    /* --------------------  UI lifecycle  -------------------- */
    private void OnEnable()          // gdy menu / przycisk staje się widoczny
    {
        // Pokaż kursor, odblokuj kamerę
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Zarejestruj klik
        _btn = GetComponent<Button>();
        _btn.onClick.AddListener(ReloadScene);
    }

    private void OnDisable()         // porządek przy chowaniu menu
    {
        if (_btn != null)
            _btn.onClick.RemoveListener(ReloadScene);
    }

    /* --------------------  ACTION  -------------------- */
    private void ReloadScene()
    {
        // Schowaj kursor natychmiast (opcjonalne, dodatkowe zabezpieczenie)
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        SceneManager.LoadScene(sceneName);
    }
}
