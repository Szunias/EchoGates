using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(NavMeshAgent))]
public class spiderAI : MonoBehaviour
{
    public enum State { Patrol, Idle, Chase, Teleport }

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

    [Header("References")]
    public Transform player;
    public string deathScene = "GameOver";

    private State currentState = State.Idle;
    private int currentPatrolIndex = -1;
    private float stateTimer;
    private float chaseTimer;
    private float teleportTimer;

    void Awake()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (player == null) Debug.LogError("spiderAI: Player Transform is not assigned.");
        if (patrolPoints == null || patrolPoints.Count == 0) Debug.LogError("spiderAI: No patrol points assigned.");
    }

    void Start()
    {
        agent.speed = walkSpeed;
        EnterIdle();
    }

    void Update()
    {
        teleportTimer += Time.deltaTime;

        // Always check detection except during Chase
        if (currentState != State.Chase && CanDetectPlayer())
            EnterChase();

        switch (currentState)
        {
            case State.Patrol:
                UpdatePatrol();
                break;
            case State.Idle:
                UpdateIdle();
                break;
            case State.Chase:
                UpdateChase();
                break;
            case State.Teleport:
                UpdateTeleport();
                break;
        }
    }

    // DETECTION: hearing or sight + path reachability
    bool CanDetectPlayer()
    {
        if (player == null) return false;
        float dist = Vector3.Distance(transform.position, player.position);
        // Hearing
        if (dist <= hearingRange)
            return true;

        // Sight sphere overlap
        if (dist <= sightRange)
        {
            Collider[] hits = Physics.OverlapSphere(transform.position + eyeOffset, sightRange);
            foreach (var col in hits)
            {
                if (col.transform == player)
                {
                    // Check NavMesh path reachability
                    NavMeshPath path = new NavMeshPath();
                    agent.CalculatePath(player.position, path);
                    if (path.status == NavMeshPathStatus.PathComplete)
                        return true;
                }
            }
        }
        return false;
    }

    // IDLE state
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
            // Choose random patrol point
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

    // ENTER CHASE
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

        // Catch
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            SceneManager.LoadScene(deathScene);
            return;
        }

        // Lost detection
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

    // TELEPORT
    void UpdateTeleport()
    {
        if (player == null)
        {
            agent.speed = walkSpeed;
            EnterIdle();
            return;
        }
        Vector3 dir = Random.insideUnitSphere;
        dir.y = 0;
        dir.Normalize();
        float dist = Random.Range(teleportMinDist, teleportMaxDist);
        Vector3 target = player.position + dir * dist;
        NavMeshHit navHit;
        if (NavMesh.SamplePosition(target, out navHit, teleportMaxDist, NavMesh.AllAreas))
            agent.Warp(navHit.position);

        teleportTimer = 0f;
        currentState = State.Chase;
    }

    // DRAW detection sphere in editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + eyeOffset, sightRange);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position + eyeOffset, hearingRange);
    }
    void OnTriggerEnter(Collider other)
    {
        if (currentState == State.Chase && other.transform == player) // Sprawdü, czy trigger jest z graczem i czy pajπk jest w stanie Chase
        {
            Debug.Log("Spider triggered with player!");
            SceneManager.LoadScene(deathScene);
        }
    }
}