using System.Collections;
using UnityEngine;

/// <summary>
/// Genera pajaros (Living Birds) y asegura la presencia del generador de Power-Ups a lo largo del recorrido.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Prefab del enemigo base (Pato3D).")]
    public GameObject enemyBasePrefab;

    [Tooltip("Tipos de pajaros configurados con Scriptable Objects (Living Birds).")]
    public EnemyDataSO[] enemyTypes;

    [Tooltip("Transform del jugador o camara VR. Si se deja vacio, se detecta automaticamente.")]
    public Transform playerTransform;

    [Header("Zona de Spawn Dinamica (Relativa al Jugador)")]
    [Tooltip("Distancia minima por delante del jugador donde aparecen los pajaros")]
    public float spawnDistanceAheadMin = 14f;

    [Tooltip("Distancia maxima por delante del jugador")]
    public float spawnDistanceAheadMax = 24f;

    [Tooltip("Distancia horizontal a los lados del camino donde nacen")]
    public float spawnSideDistance = 12f;

    [Tooltip("Altura minima de vuelo")]
    public float minSpawnHeight = 1.8f;

    [Tooltip("Altura maxima de vuelo")]
    public float maxSpawnHeight = 5.5f;

    [Header("Frecuencia de Aparicion (Mayor densidad)")]
    public float spawnIntervalMin = 0.7f;
    public float spawnIntervalMax = 1.5f;
    public bool autoSpawn = true;

    [Header("Power-Ups")]
    public bool spawnPowerUps = true;

    private Coroutine spawnRoutine;

    void Start()
    {
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

        if (spawnPowerUps && FindAnyObjectByType<PowerUpSpawner>() == null)
        {
            GameObject powerUpSpawnerObj = new GameObject("PowerUpSpawner_Auto");
            var powerSpawner = powerUpSpawnerObj.AddComponent<PowerUpSpawner>();
            powerSpawner.playerTransform = playerTransform;
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
            
            // Spawn regular de 1 o 2 pájaros a la vez
            SpawnEnemyAheadOfPlayer();
            if (Random.value < 0.45f)
            {
                SpawnEnemyAheadOfPlayer();
            }
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
        bool shouldAttack = (selectedData != null && selectedData.isAggressive) || (Random.value > 0.70f);
        if (shouldAttack)
        {
            Vector3 targetHead = playerPos;
            targetHead.y += 1.0f;
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
