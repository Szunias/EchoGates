using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TutorialTrigger : MonoBehaviour
{
    [Tooltip("Tekst, który ma się pojawić")]
    [TextArea(2, 5)]
    [SerializeField] private string message = "Domyślny tekst tutoriala";

    private bool _triggered = false;
    private Collider _col;

    private void Awake()
    {
        _col = GetComponent<Collider>();
        if (!_col.isTrigger)
        {
            _col.isTrigger = true; // wymuszamy dla pewności
            Debug.LogWarning($"{name}: Collider ustawiony na Is Trigger = true");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        /* 1 . Wychodzimy natychmiast, jeżeli już odpalone */
        if (_triggered) return;

        /* 2 . Sprawdzamy, czy collider należy do gracza
              ─ bierzemy ROOT z Rigidbody, żeby uniknąć multiplikacji     */
        Transform root = other.attachedRigidbody ?
                         other.attachedRigidbody.transform : other.transform;

        if (!root.CompareTag("Player")) return;

        /* 3 . Od TEJ chwili nic już nie przejdzie */
        _triggered = true;
        _col.enabled = false;          // wyłączamy trigger

        MessageUIManager.Instance.Show(message);
    }
}
