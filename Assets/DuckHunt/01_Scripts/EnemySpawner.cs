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

        // Obtener posición de referencia del jugador
        Vector3 playerPos = (playerTransform != null) ? playerTransform.position : transform.position;

        // Calcular punto de aparición por delante del jugador en el eje del camino
        float distanceAhead = Random.Range(spawnDistanceAheadMin, spawnDistanceAheadMax);
        float spawnZ = playerPos.z + distanceAhead;
        float spawnY = playerPos.y + Random.Range(minSpawnHeight, maxSpawnHeight);

        // Decidir si aparece a la izquierda o a la derecha
        bool spawnOnLeft = Random.value > 0.5f;
        float spawnX = spawnOnLeft
            ? (playerPos.x - spawnSideDistance)
            : (playerPos.x + spawnSideDistance);

        Vector3 spawnPosition = new Vector3(spawnX, spawnY, spawnZ);

        // Dirección de vuelo: cruzar hacia el lado opuesto
        Vector3 flightDirection = spawnOnLeft ? Vector3.right : Vector3.left;

        // Añadir una ligera variación en profundidad (Z) y altura (Y) para vuelos más naturales
        flightDirection.z = Random.Range(-0.25f, 0.25f);
        flightDirection.y = Random.Range(-0.1f, 0.25f);
        flightDirection.Normalize();

        GameObject newEnemy = Instantiate(enemyBasePrefab, spawnPosition, Quaternion.identity);

        EnemyTarget targetComp = newEnemy.GetComponent<EnemyTarget>();
        if (targetComp == null)
        {
            targetComp = newEnemy.AddComponent<EnemyTarget>();
        }

        // Asignar Scriptable Object aleatorio
        if (enemyTypes != null && enemyTypes.Length > 0)
        {
            EnemyDataSO selectedData = enemyTypes[Random.Range(0, enemyTypes.Length)];
            targetComp.Setup(selectedData, flightDirection);
        }
        else
        {
            targetComp.Setup(null, flightDirection);
        }

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
