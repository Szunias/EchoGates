using UnityEngine;
using UnityEngine.InputSystem;   // ✨ New Input System

public class PlayerMeleeAttack : MonoBehaviour
{
    [Header("Combat")]
    [SerializeField] private float attackRange = 5f;
    [SerializeField] private LayerMask enemyMask = ~0;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip swingClip;

    [Header("References")]
    [SerializeField] private Transform attackOrigin;

    [Header("Input")]
    [Tooltip("Drag your “Attack” InputActionReference here")]
    [SerializeField] private InputActionReference attackAction;   // <-- reference to the action

    private void OnEnable()
    {
        // make sure the action is enabled when this component is active
        if (attackAction) attackAction.action.Enable();
    }

    private void OnDisable()
    {
        if (attackAction) attackAction.action.Disable();
    }

    private void Start()
    {
        // fallback ‑ if no attackOrigin was set in the Inspector, grab the main camera
        if (!attackOrigin)
        {
            Camera cam = Camera.main ? Camera.main : FindObjectOfType<Camera>();
            if (cam) attackOrigin = cam.transform;
            else Debug.LogError("PlayerMeleeAttack: No camera found. Assign Attack Origin in the Inspector.");
        }
    }

    private void Update()
    {
        // InputSystem polling: true in the frame the button/trigger was pressed
        if (attackAction && attackAction.action.WasPressedThisFrame())
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
