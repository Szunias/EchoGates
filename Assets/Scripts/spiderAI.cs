using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(NavMeshAgent))]
// [RequireComponent(typeof(AudioSource))] // Już nie jest potrzebne, bo będziemy przypisywać źródła ręcznie
public class spiderAI : MonoBehaviour
{
    public enum State { Patrol, Idle, Chase, PreTeleport }

    /* ---------- Inspector ---------- */
    [Header("Agent & Patrol Points")]
    public NavMeshAgent agent;
    public List<Transform> patrolPoints = new List<Transform>();

    [Header("Speeds & Timers")]
    public float walkSpeed = 3.5f;
    public float chaseSpeed = 6f;
    public float idleMin = 1.5f;
    public float idleMax = 3.5f;
    public float chaseMaxDuration = 8f;

    [Header("Detection Settings")]
    public float sightRange = 30f;
    public float hearingRange = 7f;
    public Vector3 eyeOffset = new Vector3(0, 1, 0);

    [Header("Teleportation")]
    public float teleportCooldown = 6f;
    public float teleportMinDist = 4f;
    public float teleportMaxDist = 10f;
    public float preTeleportDuration = 1.0f;

    [Header("Startup")]
    [Tooltip("Czas po starcie sceny, zanim pająk zacznie działać")]
    public float gracePeriod = 10f;

    [Header("References")]
    public Transform player;
    public string deathScene = "GameOver";
    public Light spotLight;

    [Header("Audio Sources (Nowy Sposób)")]
    // ZMIANA: Zamiast AudioClip, teraz referencje do całych komponentów AudioSource
    public AudioSource[] preTeleportWarningSoundSources; // Tablica na źródła dźwięków ostrzegawczych
    public AudioSource teleportExecutionSoundSource; // Pojedyncze źródło dla dźwięku wykonania teleportu

    /* ---------- STUN ---------- */
    private bool isStunned = false;
    private float stunEndTime = 0f;

    /* ---------- Internals ---------- */
    private State currentState = State.Idle;
    private int currentPatrolIndex = -1;
    private float stateTimer;
    private float chaseTimer;
    private float teleportTimer;
    private Color originalSpotlightColor;
    private bool isDead = false;
    // Nie potrzebujemy już jednej, głównej referencji do audioSource, bo używamy dedykowanych

    /* ---------- Grace period ---------- */
    private float graceEndTime;
    private bool graceDone = false;
    public bool IsStunned => isStunned;

    /* ====================================================================== */
    /* LIFECYCLE                                */
    /* ====================================================================== */
    void Awake()
    {
        if (agent == null) Debug.LogError("spiderAI: NavMeshAgent missing");
        if (player == null) Debug.LogError("spiderAI: Player reference missing");
        if (patrolPoints.Count == 0 && currentState != State.Idle) Debug.LogWarning("spiderAI: patrolPoints empty, spider might only stay Idle or Chase.");
        if (spotLight == null) Debug.LogWarning("spiderAI: SpotLight reference missing. Przypisz Spotlight w Inspektorze.");
    }

    void Start()
    {
        graceEndTime = Time.time + gracePeriod;
        agent.isStopped = true;

        if (spotLight != null)
        {
            originalSpotlightColor = spotLight.color;
        }

        EnterIdle();
    }

    void Update()
    {
        if (isDead) return;

        if (!graceDone)
        {
            if (Time.time < graceEndTime)
                return;

            graceDone = true;
            agent.isStopped = false;
            EnterIdle();
        }

        if (isStunned)
        {
            if (Time.time >= stunEndTime)
            {
                isStunned = false;
                agent.isStopped = false;
                if (spotLight != null)
                {
                    if (currentState != State.PreTeleport) { spotLight.color = originalSpotlightColor; }
                    else { spotLight.color = Color.yellow; }
                }
                switch (currentState)
                {
                    case State.Patrol: case State.Idle: agent.speed = walkSpeed; break;
                    case State.Chase: agent.speed = chaseSpeed; break;
                    case State.PreTeleport: agent.isStopped = true; break;
                }
            }
            else
            {
                agent.isStopped = true;
                return;
            }
        }

        teleportTimer += Time.deltaTime;

        if (currentState != State.Chase && currentState != State.PreTeleport && CanDetectPlayer())
        {
            EnterChase();
        }

        switch (currentState)
        {
            case State.Patrol: UpdatePatrol(); break;
            case State.Idle: UpdateIdle(); break;
            case State.Chase: UpdateChase(); break;
            case State.PreTeleport: UpdatePreTeleport(); break;
        }
    }

    /* ====================================================================== */
    /* STUN API ... (bez zmian)                                                */
    /* ====================================================================== */
    public void Stun(float duration)
    {
        if (duration <= 0f || isStunned) return;
        isStunned = true;
        stunEndTime = Mathf.Max(stunEndTime, Time.time + duration);
        agent.isStopped = true;
        if (spotLight != null) { spotLight.color = Color.green; }
    }

    /* ====================================================================== */
    /* CAN DETECT PLAYER ... (bez zmian)                                       */
    /* ====================================================================== */
    bool CanDetectPlayer()
    {
        if (player == null) return false;
        float dist = Vector3.Distance(transform.position, player.position);
        if (dist <= hearingRange) return true;
        if (dist <= sightRange) return true;
        return false;
    }

    /* ====================================================================== */
    /* IDLE / PATROL / CHASE ... (bez zmian)                                   */
    /* ====================================================================== */
    void EnterIdle()
    {
        currentState = State.Idle;
        agent.speed = walkSpeed;
        agent.isStopped = true;
        stateTimer = Time.time + Random.Range(idleMin, idleMax);
        if (spotLight != null && !isStunned) { spotLight.color = originalSpotlightColor; }
    }

    void UpdateIdle()
    {
        if (Time.time >= stateTimer)
        {
            if (patrolPoints.Count > 0)
            {
                int nextIndex = currentPatrolIndex;
                if (patrolPoints.Count > 1) { while (nextIndex == currentPatrolIndex) nextIndex = Random.Range(0, patrolPoints.Count); }
                else { nextIndex = 0; }
                currentPatrolIndex = nextIndex;
                agent.isStopped = false;
                agent.SetDestination(patrolPoints[currentPatrolIndex].position);
                currentState = State.Patrol;
            }
        }
    }

    void UpdatePatrol() { if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance) EnterIdle(); }

    void EnterChase()
    {
        currentState = State.Chase;
        chaseTimer = 0f;
        agent.speed = chaseSpeed;
        agent.isStopped = false;
        if (spotLight != null && !isStunned) { spotLight.color = originalSpotlightColor; }
    }

    void UpdateChase()
    {
        chaseTimer += Time.deltaTime;
        if (player != null)
        {
            agent.SetDestination(player.position);
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance) { HandlePlayerCaught(); return; }
        }
        else { EnterIdle(); return; }

        if (!CanDetectPlayer())
        {
            if (teleportTimer >= teleportCooldown) { EnterPreTeleport(); }
            else if (chaseTimer >= chaseMaxDuration) { teleportTimer = 0f; EnterIdle(); }
        }
    }

    void EnterPreTeleport()
    {
        currentState = State.PreTeleport;
        agent.isStopped = true;
        stateTimer = Time.time + preTeleportDuration;

        // ZMIANA: Odtwórz dźwięk z losowego AudioSource z tablicy
        if (preTeleportWarningSoundSources != null && preTeleportWarningSoundSources.Length > 0)
        {
            int randomIndex = Random.Range(0, preTeleportWarningSoundSources.Length);
            AudioSource sourceToPlay = preTeleportWarningSoundSources[randomIndex];
            if (sourceToPlay != null)
            {
                sourceToPlay.Play(); // Używamy .Play() na komponencie AudioSource
                Debug.Log("Playing pre-teleport warning sound from source: " + sourceToPlay.gameObject.name);
            }
            else
            {
                Debug.LogWarning("spiderAI: Randomly selected AudioSource for warning sound is null at index " + randomIndex + ".");
            }
        }
        else
        {
            Debug.LogWarning("spiderAI: PreTeleportWarningSoundSources array is not assigned or is empty in Inspector.");
        }

        if (spotLight != null) { spotLight.color = Color.yellow; }
    }

    void UpdatePreTeleport()
    {
        if (Time.time >= stateTimer)
        {
            ExecuteTeleportSequence();
        }
    }

    void ExecuteTeleportSequence()
    {
        if (player == null)
        {
            if (spotLight != null) spotLight.color = originalSpotlightColor;
            EnterIdle();
            return;
        }

        Vector3 dir = Random.insideUnitSphere;
        dir.y = 0;
        dir.Normalize();
        float dist = Random.Range(teleportMinDist, teleportMaxDist);
        Vector3 targetPos = player.position + dir * dist;

        if (NavMesh.SamplePosition(targetPos, out NavMeshHit navHit, teleportMaxDist, NavMesh.AllAreas))
        {
            agent.Warp(navHit.position);

            // ZMIANA: Odtwórz dźwięk z dedykowanego AudioSource
            if (teleportExecutionSoundSource != null)
            {
                teleportExecutionSoundSource.Play(); // Używamy .Play() na komponencie AudioSource
                Debug.Log("Playing teleport EXECUTION sound from source: " + teleportExecutionSoundSource.gameObject.name);
            }
            else
            {
                Debug.LogWarning("spiderAI: TeleportExecutionSoundSource is not assigned in Inspector.");
            }
        }

        if (spotLight != null) { spotLight.color = originalSpotlightColor; }
        teleportTimer = 0f;
        agent.isStopped = false;
        EnterChase();
    }

    void HandlePlayerCaught()
    {
        if (!isDead)
        {
            isDead = true;
            if (agent != null) agent.isStopped = true;
            this.enabled = false;
            Debug.Log("Player caught! Loading death scene: " + deathScene);
            SceneManager.LoadScene(deathScene);
        }
    }

    /* ====================================================================== */
    /* GIZMOS & TRIGGER (bez zmian)                                          */
    /* ====================================================================== */
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + eyeOffset, sightRange);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, hearingRange);
        if (player != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(player.position, teleportMinDist);
            Gizmos.DrawWireSphere(player.position, teleportMaxDist);
        }
        if (currentState == State.PreTeleport)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(transform.position + eyeOffset, 0.5f);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (player != null && other.transform == player && !isDead)
        {
            if (currentState == State.Chase || Vector3.Distance(transform.position, player.position) < agent.stoppingDistance * 1.5f)
            {
                HandlePlayerCaught();
            }
        }
    }
}