﻿using UnityEngine;
using UnityEngine.InputSystem;   // ✨ New Input System

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
    [SerializeField] private InputActionReference attackAction;

    private void OnEnable()
    {
        if (attackAction) attackAction.action.Enable();
    }

    private void OnDisable()
    {
        if (attackAction) attackAction.action.Disable();
    }

    private void Start()
    {
        // fallback – jeśli nie ustawiono w Inspectorze, weź pierwszą kamerę w scenie
        if (attackOrigin == null)
        {
            Camera cam = Camera.main;
            if (cam == null)
                cam = Object.FindAnyObjectByType<Camera>();
            if (cam != null)
                attackOrigin = cam.transform;
            else
                Debug.LogError("PlayerMeleeAttack: No camera found. Assign Attack Origin in the Inspector.");
        }
    }

    private void Update()
    {
        if (attackAction?.action.WasPressedThisFrame() == true)
            AttemptAttack();
    }

    private void AttemptAttack()
    {
        if (audioSource && swingClip)
            audioSource.PlayOneShot(swingClip);

        if (attackOrigin == null) return;

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
