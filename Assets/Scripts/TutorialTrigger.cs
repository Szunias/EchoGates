using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TutorialTrigger : MonoBehaviour
{
    [Tooltip("Wpisz tu w Inspectorze treœæ, jak¹ gracz ma zobaczyæ")]
    [TextArea(2, 5)]
    [SerializeField] private string message = "Domyœlny tekst tutoriala";

    private bool _triggered = false;
    private Collider _col;

    private void Awake()
    {
        _col = GetComponent<Collider>();
        if (!_col.isTrigger)
            Debug.LogWarning($"{name}: Collider musi mieæ Is Trigger = true!");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_triggered) return;
        if (!other.CompareTag("Player")) return;

        _triggered = true;
        _col.enabled = false;

        // pokazujemy tylko tekst – obrazek jest ju¿ przypisany w UI managerze
        MessageUIManager.Instance.Show(message);
    }
}
