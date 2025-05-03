using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemySpawner : MonoBehaviour
{
    [Header("What to spawn")]
    [SerializeField] private GameObject enemyPrefab;

    [Header("Where")]
    [SerializeField] private Vector2 spawnBox = new Vector2(10f, 10f);   // width (X) × depth (Z)
    [SerializeField] private bool snapToNavMesh = true;

    [Header("When")]
    [SerializeField] private float spawnInterval = 5f;                   // seconds (fixed)
    [SerializeField] private bool randomizeInterval = false;
    [SerializeField] private Vector2 intervalRange = new Vector2(3f, 8f);

    [Header("How many")]
    [SerializeField] private int maxAlive = 5;

    private readonly List<GameObject> alive = new();                     // track current enemies

    private void Start()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError($"EnemySpawner on {name}: no enemyPrefab assigned!");
            enabled = false;
            return;
        }

        StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        yield return new WaitForSeconds(1f);                              // small delay at start

        while (true)
        {
            CleanupList();

            if (alive.Count < maxAlive)
                TrySpawnEnemy();

            float wait = randomizeInterval ? Random.Range(intervalRange.x, intervalRange.y)
                                            : spawnInterval;
            yield return new WaitForSeconds(wait);
        }
    }

    private void TrySpawnEnemy()
    {
        //–––– safeguard – make sure we still have a valid prefab ––––––––––––––––  // added
        if (enemyPrefab == null)
        {
            Debug.LogWarning($"{name}: Enemy prefab reference is missing!");
            return;
        }
        //–––––––––––––––––––––––––––––––––––––––––––––––––––––––––––––––––––––––––

        Vector3 local = new Vector3(Random.Range(-spawnBox.x * 0.5f, spawnBox.x * 0.5f),
                                    0f,
                                    Random.Range(-spawnBox.y * 0.5f, spawnBox.y * 0.5f));

        Vector3 world = transform.TransformPoint(local);

        if (snapToNavMesh && NavMesh.SamplePosition(world, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            world = hit.position;

        GameObject go = Instantiate(enemyPrefab, world, Quaternion.identity, transform);
        alive.Add(go);
    }

    private void CleanupList()
    {
        for (int i = alive.Count - 1; i >= 0; i--)
            if (alive[i] == null) alive.RemoveAt(i);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.3f, 0f, 0.25f);
        Matrix4x4 prev = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
        Gizmos.DrawCube(Vector3.zero, new Vector3(spawnBox.x, 0.1f, spawnBox.y));
        Gizmos.matrix = prev;
    }
#endif
}
