using UnityEngine;

/// <summary>
/// Pokazuje / chowa obiekt‑znacznik w zależności od tego,
/// czy pająk jest ogłuszony.
/// </summary>
public class SpiderStunIndicator : MonoBehaviour
{
    [Tooltip("Referencja do skryptu spiderAI (jeśli puste – weź z tego samego obiektu)")]
    [SerializeField] private spiderAI spider;

    [Tooltip("Obiekt, który ma się pojawiać podczas stun (gwiazdka, sprite, PS). " +
             "Ustaw go na disabled w Inspectorze!")]
    [SerializeField] private GameObject indicator;

    void Awake()
    {
        if (spider == null)
            spider = GetComponent<spiderAI>();

        if (indicator == null)
            Debug.LogWarning($"{name}: nie podpięto pola <indicator> – nic nie pokażę.");
    }

    void Update()
    {
        if (spider == null || indicator == null) return;

        // aktywujemy tylko wtedy, gdy faktycznie stun
        bool shouldShow = spider.IsStunned;
        if (indicator.activeSelf != shouldShow)
            indicator.SetActive(shouldShow);
    }
}
