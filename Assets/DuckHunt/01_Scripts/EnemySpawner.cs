using System.Collections;
using UnityEngine;

/// <summary>
/// Genera patos de forma dinámica a lo largo del recorrido en función de la posición actual del jugador.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Prefab del enemigo (Pato3D).")]
    public GameObject enemyBasePrefab;

    [Tooltip("Tipos de patos configurados con Scriptable Objects.")]
    public EnemyDataSO[] enemyTypes;

    [Tooltip("Transform del jugador o cámara VR. Si se deja vacío, se detecta automáticamente.")]
    public Transform playerTransform;

    [Header("Zona de Spawn Dinámica (Relativa al Jugador)")]
    [Tooltip("Distancia mínima por delante del jugador donde aparecen los patos")]
    public float spawnDistanceAheadMin = 14f;

    [Tooltip("Distancia máxima por delante del jugador")]
    public float spawnDistanceAheadMax = 22f;

    [Tooltip("Distancia horizontal a los lados del camino donde nacen")]
    public float spawnSideDistance = 12f;

    [Tooltip("Altura mínima de vuelo")]
    public float minSpawnHeight = 2.0f;

    [Tooltip("Altura máxima de vuelo")]
    public float maxSpawnHeight = 5.5f;

    [Header("Frecuencia de Aparición")]
    public float spawnIntervalMin = 1.8f;
    public float spawnIntervalMax = 3.5f;
    public bool autoSpawn = true;

    private Coroutine spawnRoutine;

    void Start()
    {
        // Buscar al jugador automáticamente si no fue asignado en el inspector
        if (playerTransform == null)
        {
            var mover = FindAnyObjectByType<VRAutoForwardMover>();
            if (mover != null)
            {
                playerTransform = mover.transform;
            }
            else if (Camera.main != null)
            {
                playerTransform = Camera.main.transform;
            }
            else
            {
                playerTransform = transform;
            }
        }

        if (autoSpawn)
        {
            StartSpawning();
        }
    }

    public void StartSpawning()
    {
        if (spawnRoutine == null)
        {
            spawnRoutine = StartCoroutine(SpawnLoop());
        }
    }

    public void StopSpawning()
    {
        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(spawnIntervalMin, spawnIntervalMax));
            SpawnEnemyAheadOfPlayer();
        }
    }

    public GameObject SpawnEnemyAheadOfPlayer()
    {
        if (enemyBasePrefab == null)
        {
            Debug.LogWarning("[EnemySpawner] Falta asignar enemyBasePrefab.");
            return null;
        }

        Vector3 playerPos = (playerTransform != null) ? playerTransform.position : transform.position;

        float distanceAhead = Random.Range(spawnDistanceAheadMin, spawnDistanceAheadMax);
        float spawnZ = playerPos.z + distanceAhead;
        float spawnY = playerPos.y + Random.Range(minSpawnHeight, maxSpawnHeight);

        bool spawnOnLeft = Random.value > 0.5f;
        float spawnX = spawnOnLeft
            ? (playerPos.x - spawnSideDistance)
            : (playerPos.x + spawnSideDistance);

        Vector3 spawnPosition = new Vector3(spawnX, spawnY, spawnZ);

        EnemyDataSO selectedData = null;
        if (enemyTypes != null && enemyTypes.Length > 0)
        {
            selectedData = enemyTypes[Random.Range(0, enemyTypes.Length)];
        }

        Vector3 flightDirection;
        bool shouldAttack = (selectedData != null && selectedData.isAggressive) || (Random.value > 0.6f); // 40% de patos atacan autom�ticamente
        if (shouldAttack)
        {
            Vector3 targetHead = playerPos;
            targetHead.y += 1.6f;
            flightDirection = (targetHead - spawnPosition).normalized;
        }
        else
        {
            flightDirection = spawnOnLeft ? Vector3.right : Vector3.left;
            flightDirection.z = Random.Range(-0.25f, 0.25f);
            flightDirection.y = Random.Range(-0.1f, 0.25f);
            flightDirection.Normalize();
        }

        GameObject newEnemy = Instantiate(enemyBasePrefab, spawnPosition, Quaternion.identity);

        EnemyTarget targetComp = newEnemy.GetComponent<EnemyTarget>();
        if (targetComp == null)
        {
            targetComp = newEnemy.AddComponent<EnemyTarget>();
        }

        targetComp.Setup(selectedData, flightDirection);
        targetComp.isDynamicallyAggressive = shouldAttack;

        return newEnemy;
    }
    void OnDrawGizmosSelected()
    {
        Vector3 playerPos = (playerTransform != null) ? playerTransform.position : transform.position;
        Gizmos.color = Color.yellow;
        Vector3 boxCenter = new Vector3(playerPos.x, playerPos.y + (minSpawnHeight + maxSpawnHeight) * 0.5f, playerPos.z + (spawnDistanceAheadMin + spawnDistanceAheadMax) * 0.5f);
        Vector3 boxSize = new Vector3(spawnSideDistance * 2f, maxSpawnHeight - minSpawnHeight, spawnDistanceAheadMax - spawnDistanceAheadMin);
        Gizmos.DrawWireCube(boxCenter, boxSize);
    }
}
 


