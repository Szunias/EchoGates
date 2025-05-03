using UnityEngine;

public class PlayerMeleeAttack : MonoBehaviour
{
    [Header("Combat")]
    [SerializeField] private float attackRange = 5f;           // Melee reach
    [SerializeField] private LayerMask enemyMask = ~0;         // Layers to hit

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;          // Source that plays the swing
    [SerializeField] private AudioClip swingClip;             // Sound of the attack

    [Header("References")]
    [SerializeField] private Transform attackOrigin;           // If left empty, main or first camera is used

    private void Start()
    {
        if (!attackOrigin)
        {
            Camera cam = Camera.main ? Camera.main : FindObjectOfType<Camera>();
            if (cam) attackOrigin = cam.transform;
            else Debug.LogError("PlayerMeleeAttack: No camera found. Assign Attack Origin in the Inspector.");
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
            AttemptAttack();
    }

    private void AttemptAttack()
    {
        // play swing immediately—even if we miss
        if (audioSource && swingClip)
            audioSource.PlayOneShot(swingClip);

        if (!attackOrigin) return;

        Ray ray = new Ray(attackOrigin.position, attackOrigin.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, attackRange, enemyMask))
        {
            var enemy = hit.collider.GetComponentInParent<EnemyHealth>();
            if (enemy)
            {
                enemy.TakeHit();
                Debug.Log($"Hit enemy: {enemy.name}");
            }
            else
            {
                Debug.Log("Hit something, but it is not an enemy.");
            }
        }
        else
        {
            Debug.Log("Miss");
        }
    }
}
