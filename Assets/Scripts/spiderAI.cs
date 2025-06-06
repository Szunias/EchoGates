// --- spiderAI.cs (Correct, Complete, and Final Version) ---
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

// A struct to hold settings, kept inside spiderAI for organization
[System.Serializable]
public struct DifficultySettings
{
    public float walkSpeed;
    public float chaseSpeed;
    public float idleMin;
    public float idleMax;
    public float chaseMaxDuration;
    public float sightRange;
    public float hearingRange;
    public float teleportCooldown;
    public float preTeleportDuration;
    public float fogDensity;
}

[RequireComponent(typeof(NavMeshAgent))]
public class spiderAI : MonoBehaviour
{
    public enum State { Patrol, Idle, Chase, PreTeleport }

    [Header("Agent & Patrol Points")]
    public NavMeshAgent agent;
    public List<Transform> patrolPoints = new List<Transform>();

    // These default values will be overwritten by difficulty settings on Start
    [Header("Speeds & Timers")]
    public float walkSpeed = 10f;
    public float chaseSpeed = 30f;
    public float idleMin = 2.0f;
    public float idleMax = 4.0f;
    public float chaseMaxDuration = 6f;

    [Header("Detection Settings")]
    public float sightRange = 75f;
    public float hearingRange = 30f;
    public Vector3 eyeOffset = new Vector3(0, 1, 0);

    [Header("Teleportation")]
    public float teleportCooldown = 15f;
    public float teleportMinDist = 4f;
    public float teleportMaxDist = 10f;
    public float preTeleportDuration = 2.0f;

    [Header("Startup")]
    [Tooltip("Time after scene start before the spider becomes active")]
    public float gracePeriod = 10f;

    [Header("References")]
    public Transform player;
    public string deathScene = "GameOver";
    public Light spotLight;

    [Header("Audio Sources")]
    public AudioSource[] preTeleportWarningSoundSources;
    public AudioSource teleportExecutionSoundSource;

    // Internal state variables
    private bool isStunned = false;
    private float stunEndTime = 0f;
    private State currentState = State.Idle;
    private int currentPatrolIndex = -1;
    private float stateTimer;
    private float chaseTimer;
    private float teleportTimer;
    private Color originalSpotlightColor;
    private bool isDead = false;
    private float graceEndTime;
    private bool graceDone = false;
    public bool IsStunned => isStunned;

    void Awake()
    {
        // Basic reference checks
        if (agent == null) Debug.LogError("spiderAI: NavMeshAgent missing");
        if (player == null) Debug.LogError("spiderAI: Player reference missing");
        if (patrolPoints.Count == 0) Debug.LogWarning("spiderAI: patrolPoints empty. Spider might only stay Idle or Chase.");
        if (spotLight == null) Debug.LogWarning("spiderAI: SpotLight reference missing.");
    }

    void Start()
    {
        // Apply difficulty settings from PlayerPrefs when the scene starts
        ApplyDifficultySettings();

        graceEndTime = Time.time + gracePeriod;
        agent.isStopped = true;

        if (spotLight != null)
        {
            originalSpotlightColor = spotLight.color;
        }

        EnterIdle();
    }

    // This method reads the saved difficulty and configures the spider and environment
    private void ApplyDifficultySettings()
    {
        string difficultyKey = "SelectedDifficulty";
        int difficultyLevel = PlayerPrefs.GetInt(difficultyKey, 1); // Default to Medium

        DifficultySettings settings;
        string modeName = "MEDIUM";

        switch (difficultyLevel)
        {
            case 0: // Easy
                settings = new DifficultySettings
                {
                    walkSpeed = 7f,
                    chaseSpeed = 20f,
                    idleMin = 3.0f,
                    idleMax = 5.0f,
                    chaseMaxDuration = 4f,
                    sightRange = 70f,
                    hearingRange = 20f,
                    teleportCooldown = 20f,
                    preTeleportDuration = 1.5f,
                    fogDensity = 0.035f
                };
                modeName = "EASY";
                break;

            case 2: // Hard
                settings = new DifficultySettings
                {
                    walkSpeed = 29f,
                    chaseSpeed = 40f,
                    idleMin = 0.9f,
                    idleMax = 2.6f,
                    chaseMaxDuration = 8f,
                    sightRange = 110f,
                    hearingRange = 40f,
                    teleportCooldown = 10f,
                    preTeleportDuration = 3.0f,
                    fogDensity = 0.125f
                };
                modeName = "HARD";
                break;

            case 1: // Medium (Default)
            default:
                settings = new DifficultySettings
                {
                    walkSpeed = 10f,
                    chaseSpeed = 30f,
                    idleMin = 2.0f,
                    idleMax = 4.0f,
                    chaseMaxDuration = 6f,
                    sightRange = 785f,
                    hearingRange = 30f,
                    teleportCooldown = 15f,
                    preTeleportDuration = 2.0f,
                    fogDensity = 0.085f
                };
                modeName = "MEDIUM";
                break;
        }

        Debug.Log($"<color=cyan>SPIDER AI: Applying {modeName} settings.</color>");

        // Apply settings to this script's variables
        this.walkSpeed = settings.walkSpeed;
        this.chaseSpeed = settings.chaseSpeed;
        this.idleMin = settings.idleMin;
        this.idleMax = settings.idleMax;
        this.chaseMaxDuration = settings.chaseMaxDuration;
        this.sightRange = settings.sightRange;
        this.hearingRange = settings.hearingRange;
        this.teleportCooldown = settings.teleportCooldown;
        this.preTeleportDuration = settings.preTeleportDuration;

        // Apply global rendering settings
        RenderSettings.fog = true;
        RenderSettings.fogDensity = settings.fogDensity;

        Debug.Log($"<color=purple>FOG SETTING: Fog density set to {RenderSettings.fogDensity}</color>");
    }

    // --- ALL THE CORE AI LOGIC THAT WAS MISSING ---

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

    public void Stun(float duration)
    {
        if (duration <= 0f || isStunned) return;
        isStunned = true;
        stunEndTime = Mathf.Max(stunEndTime, Time.time + duration);
        agent.isStopped = true;
        if (spotLight != null) { spotLight.color = Color.green; }
    }

    bool CanDetectPlayer()
    {
        if (player == null) return false;
        float dist = Vector3.Distance(transform.position, player.position);
        if (dist <= hearingRange) return true;
        if (dist <= sightRange) return true;
        return false;
    }

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
        if (preTeleportWarningSoundSources != null && preTeleportWarningSoundSources.Length > 0)
        {
            int randomIndex = Random.Range(0, preTeleportWarningSoundSources.Length);
            AudioSource sourceToPlay = preTeleportWarningSoundSources[randomIndex];
            if (sourceToPlay != null) { sourceToPlay.Play(); }
        }
        if (spotLight != null) { spotLight.color = Color.yellow; }
    }

    void UpdatePreTeleport()
    {
        if (Time.time >= stateTimer) { ExecuteTeleportSequence(); }
    }

    void ExecuteTeleportSequence()
    {
        if (player == null) { if (spotLight != null) spotLight.color = originalSpotlightColor; EnterIdle(); return; }
        Vector3 dir = Random.insideUnitSphere;
        dir.y = 0;
        dir.Normalize();
        float dist = Random.Range(teleportMinDist, teleportMaxDist);
        Vector3 targetPos = player.position + dir * dist;
        if (NavMesh.SamplePosition(targetPos, out NavMeshHit navHit, teleportMaxDist, NavMesh.AllAreas))
        {
            agent.Warp(navHit.position);
            if (teleportExecutionSoundSource != null) { teleportExecutionSoundSource.Play(); }
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
            SceneManager.LoadScene(deathScene);
        }
    }

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