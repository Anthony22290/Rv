using System.Collections;
using UnityEngine;

/// <summary>
/// Genera Power-Ups de forma periódica por delante del avance del jugador en VR.
/// </summary>
public class PowerUpSpawner : MonoBehaviour
{
    [Header("Configuración de Intervalo")]
    public float minSpawnInterval = 12f;
    public float maxSpawnInterval = 22f;
    public bool autoSpawn = true;

    [Header("Posición de Spawn Relativa al Jugador")]
    public float distanceAheadMin = 14f;
    public float distanceAheadMax = 24f;
    public float sideDistanceMin = -6f;
    public float sideDistanceMax = 6f;
    public float minHeight = 2.0f;
    public float maxHeight = 4.5f;

    [Header("Referencias")]
    public Transform playerTransform;

    private Coroutine spawnRoutine;

    void Start()
    {
        if (playerTransform == null)
        {
            var mover = FindAnyObjectByType<VRAutoForwardMover>();
            if (mover != null) playerTransform = mover.transform;
            else if (Camera.main != null) playerTransform = Camera.main.transform;
            else playerTransform = transform;
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
        // Primer powerup rápido tras comenzar la partida
        yield return new WaitForSeconds(Random.Range(5f, 10f));

        while (true)
        {
            SpawnRandomPowerUp();
            yield return new WaitForSeconds(Random.Range(minSpawnInterval, maxSpawnInterval));
        }
    }

    public GameObject SpawnRandomPowerUp()
    {
        Vector3 playerPos = (playerTransform != null) ? playerTransform.position : transform.position;

        float distanceAhead = Random.Range(distanceAheadMin, distanceAheadMax);
        float spawnZ = playerPos.z + distanceAhead;
        float spawnX = playerPos.x + Random.Range(sideDistanceMin, sideDistanceMax);
        float spawnY = playerPos.y + Random.Range(minHeight, maxHeight);

        Vector3 spawnPos = new Vector3(spawnX, spawnY, spawnZ);

        GameObject powerUpObj = new GameObject("PowerUp_Collectible");
        powerUpObj.transform.position = spawnPos;

        SphereCollider collider = powerUpObj.AddComponent<SphereCollider>();
        collider.radius = 0.9f;
        collider.isTrigger = true;

        PowerUpItem item = powerUpObj.AddComponent<PowerUpItem>();

        // Determinar tipo de PowerUp
        PlayerHealth health = Object.FindAnyObjectByType<PlayerHealth>();
        PowerUpType selectedType;

        if (health != null && health.currentHealth < 2 && Random.value < 0.5f)
        {
            selectedType = PowerUpType.ExtraLife;
        }
        else
        {
            float rand = Random.value;
            if (rand < 0.35f) selectedType = PowerUpType.ExtraLife;
            else if (rand < 0.70f) selectedType = PowerUpType.Shield;
            else selectedType = PowerUpType.DoublePoints;
        }

        item.powerUpType = selectedType;

        Debug.Log($"[PowerUpSpawner] 🎁 Spawned Power-Up '{selectedType}' en pos {spawnPos}");
        return powerUpObj;
    }
}
