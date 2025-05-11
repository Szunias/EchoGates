using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(NavMeshAgent))]
public class spiderAI : MonoBehaviour
{
    public enum State { Patrol, Idle, Chase, Teleport }

    /* ---------- Inspector ---------- */
    [Header("Agent & Patrol Points")] public NavMeshAgent agent;
    public List<Transform> patrolPoints = new List<Transform>();

    [Header("Speeds & Timers")] public float walkSpeed = 3.5f;
    public float chaseSpeed = 6f;
    public float idleMin = 1.5f;
    public float idleMax = 3.5f;
    public float chaseMaxDuration = 8f;

    [Header("Detection Settings")] public float sightRange = 30f;
    public float hearingRange = 7f;
    public Vector3 eyeOffset = new Vector3(0, 1, 0);

    [Header("Teleportation")] public float teleportCooldown = 6f;
    public float teleportMinDist = 4f;
    public float teleportMaxDist = 10f;

    [Header("Startup")]                 // <‑‑‑ NOWE
    [Tooltip("Czas po starcie sceny, zanim pająk zacznie działać")]
    public float gracePeriod = 10f;

    [Header("References")] public Transform player;
    public string deathScene = "GameOver";

    /* ---------- STUN ---------- */
    private bool isStunned = false;
    private float stunEndTime = 0f;

    /* ---------- Internals ---------- */
    private State currentState = State.Idle;
    private int currentPatrolIndex = -1;
    private float stateTimer;
    private float chaseTimer;
    private float teleportTimer;

    /* ---------- Grace period ---------- */
    private float graceEndTime;   // kiedy kończy się czas ochronny
    private bool graceDone = false;

    /* ====================================================================== */
    /*                           LIFECYCLE                                    */
    /* ====================================================================== */
    void Awake()
    {
        if (agent == null) Debug.LogError("spiderAI: NavMeshAgent missing");
        if (player == null) Debug.LogError("spiderAI: Player reference missing");
        if (patrolPoints.Count == 0) Debug.LogError("spiderAI: patrolPoints empty");
    }

    void Start()
    {
        agent.speed = walkSpeed;

        /* --- rozpoczynamy okres ochronny --- */
        graceEndTime = Time.time + gracePeriod;
        agent.isStopped = true;

        EnterIdle(); // nic nie robi, ale ustawia timery
    }

    void Update()
    {
        /* ---------- GRACE PERIOD ---------- */
        if (!graceDone)
        {
            if (Time.time < graceEndTime)
                return;                    // przez pierwsze X sekund AI śpi

            graceDone = true;          // koniec ochrony
            agent.isStopped = false;
            EnterIdle();                   // zresetuj timery i zacznij normalnie
        }

        /* --------------- STUN HANDLING --------------- */
        if (isStunned)
        {
            if (Time.time >= stunEndTime)
            {
                isStunned = false;
                agent.isStopped = false;
            }
            else
            {
                return; // gdy ogłuszony, pomijamy logikę
            }
        }

        teleportTimer += Time.deltaTime;

        /* ----------- NORMAL AI LOGIC BELOW ----------- */
        if (currentState != State.Chase && CanDetectPlayer())
            EnterChase();

        switch (currentState)
        {
            case State.Patrol: UpdatePatrol(); break;
            case State.Idle: UpdateIdle(); break;
            case State.Chase: UpdateChase(); break;
            case State.Teleport: UpdateTeleport(); break;
        }
    }

    /* ====================================================================== */
    /*                             STUN API                                   */
    /* ====================================================================== */
    public void Stun(float duration)
    {
        if (duration <= 0f) return;

        isStunned = true;
        stunEndTime = Mathf.Max(stunEndTime, Time.time + duration);
        agent.isStopped = true;
    }

    /* ====================================================================== */
    /*                     CAN DETECT PLAYER (bez zmian)                       */
    /* ====================================================================== */
    bool CanDetectPlayer()
    {
        if (player == null) return false;

        float dist = Vector3.Distance(transform.position, player.position);

        /* ---------- SŁUCH ---------- */
        if (dist <= hearingRange) return true;

        /* ---------- WZROK ---------- */
        if (dist <= sightRange)
        {
            Vector3 origin = transform.position + eyeOffset;
            Vector3 target = player.position + eyeOffset;
            Vector3 dir = (target - origin).normalized;

            if (Physics.Raycast(origin, dir, out RaycastHit hit, sightRange))
            {
                if (hit.transform == player || hit.transform.root == player)
                    return true;
            }
        }
        return false;
    }

    /* ====================================================================== */
    /*                        IDLE / PATROL / CHASE ...                       */
    /* ====================================================================== */
    void EnterIdle()
    {
        currentState = State.Idle;
        agent.isStopped = true;
        stateTimer = Time.time + Random.Range(idleMin, idleMax);
    }

    void UpdateIdle()
    {
        if (Time.time >= stateTimer)
        {
            int nextIndex = currentPatrolIndex;
            if (patrolPoints.Count > 1)
            {
                while (nextIndex == currentPatrolIndex)
                    nextIndex = Random.Range(0, patrolPoints.Count);
            }
            else nextIndex = 0;

            currentPatrolIndex = nextIndex;

            agent.isStopped = false;
            agent.speed = walkSpeed;
            agent.SetDestination(patrolPoints[currentPatrolIndex].position);
            currentState = State.Patrol;
        }
    }

    void UpdatePatrol()
    {
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            EnterIdle();
    }

    void EnterChase()
    {
        currentState = State.Chase;
        chaseTimer = 0f;
        agent.isStopped = false;
        agent.speed = chaseSpeed;
    }

    void UpdateChase()
    {
        chaseTimer += Time.deltaTime;
        agent.SetDestination(player.position);

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            SceneManager.LoadScene(deathScene);
            return;
        }

        if (!CanDetectPlayer())
        {
            if (teleportTimer >= teleportCooldown)
                currentState = State.Teleport;
            else if (chaseTimer >= chaseMaxDuration)
            {
                agent.speed = walkSpeed;
                teleportTimer = 0f;
                EnterIdle();
            }
        }
    }

    void UpdateTeleport()
    {
        if (player == null)
        {
            agent.speed = walkSpeed;
            EnterIdle();
            return;
        }
        Vector3 dir = Random.insideUnitSphere; dir.y = 0; dir.Normalize();
        float dist = Random.Range(teleportMinDist, teleportMaxDist);
        Vector3 target = player.position + dir * dist;

        if (NavMesh.SamplePosition(target, out NavMeshHit navHit, teleportMaxDist, NavMesh.AllAreas))
            agent.Warp(navHit.position);

        teleportTimer = 0f;
        currentState = State.Chase;
    }

    /* ====================================================================== */
    /*                            GIZMOS & TRIGGER                            */
    /* ====================================================================== */
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + eyeOffset, sightRange);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position + eyeOffset, hearingRange);
    }

    void OnTriggerEnter(Collider other)
    {
        if (currentState == State.Chase && other.transform == player)
            SceneManager.LoadScene(deathScene);
    }
}
