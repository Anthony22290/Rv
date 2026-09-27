using UnityEngine;

[RequireComponent(typeof(Collider))]
public class EnemyTarget : MonoBehaviour
{
    [Header("Configuración")]
    public EnemyDataSO enemyData;

    private Vector3 moveDirection = Vector3.right;
    private float spawnTime;
    private Vector3 initialPosition;
    private bool isDead = false;
    private Renderer[] renderers;
    private Rigidbody rb;
    private Transform playerTransform;

    void Awake()
    {
        if (!gameObject.CompareTag("Enemy"))
        {
            gameObject.tag = "Enemy";
        }
        rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        spawnTime = Time.time;
        initialPosition = transform.position;

        if (Camera.main != null)
        {
            playerTransform = Camera.main.transform;
        }

        if (enemyData != null)
        {
            ApplyData(enemyData);
        }
    }

    public void Setup(EnemyDataSO data, Vector3 direction)
    {
        enemyData = data;
        moveDirection = direction.normalized;
        initialPosition = transform.position;
        spawnTime = Time.time;
        ApplyData(data);
    }

    public void ApplyData(EnemyDataSO data)
    {
        if (data == null) return;

        transform.localScale = data.scale;

        // Aplicar tinte al material sin borrar la textura base
        renderers = GetComponentsInChildren<Renderer>();
        foreach (var r in renderers)
        {
            if (r != null && r.material != null)
            {
                if (r.material.HasProperty("_BaseColor"))
                {
                    r.material.SetColor("_BaseColor", data.bodyColor);
                }
                else
                {
                    r.material.color = data.bodyColor;
                }
            }
        }

        // Destruir por escape si pasa el tiempo límite
        Destroy(gameObject, data.maxLifeTime);
    }

    private float accumulatedTime = 0f;

    void Update()
    {
        if (isDead) return;

        float speedMultiplier = (PowerUpManager.Instance != null) ? PowerUpManager.Instance.CurrentEnemySpeedMultiplier : 1.0f;
        accumulatedTime += Time.deltaTime * speedMultiplier;
        float speed = (enemyData != null) ? enemyData.moveSpeed : 4f;

        Vector3 currentPos = initialPosition + (moveDirection * (speed * accumulatedTime));

        float verticalOffset = 0f;
        if (enemyData != null)
        {
            switch (enemyData.movementPattern)
            {
                case EnemyMovementPattern.SineWave:
                    verticalOffset = Mathf.Sin(accumulatedTime * enemyData.waveFrequency) * enemyData.waveAmplitude;
                    break;

                case EnemyMovementPattern.ArcFly:
                    verticalOffset = Mathf.Sin((accumulatedTime / enemyData.maxLifeTime) * Mathf.PI) * enemyData.waveAmplitude;
                    break;

                case EnemyMovementPattern.StraightLine:
                default:
                    verticalOffset = 0f;
                    break;
            }
        }

        currentPos.y += verticalOffset;
        transform.position = currentPos;

        // Orientar el pato hacia donde vuela con una ligera inclinación dinámica
        if (moveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            float tiltZ = Mathf.Sin(accumulatedTime * 6f) * 8f;
            transform.rotation = targetRotation * Quaternion.Euler(0, 0, tiltZ);
        }

        // Detectar si impacta cerca del jugador / cámara
        if (playerTransform != null && Vector3.Distance(transform.position, playerTransform.position) < 1.4f)
        {
            var health = playerTransform.GetComponentInParent<PlayerHealth>() ?? Object.FindAnyObjectByType<PlayerHealth>();
            if (health != null)
            {
                health.TakeDamage(1);
            }
            OnHit();
        }
    }

    public void OnHit()
    {
        if (isDead) return;
        isDead = true;

        int multiplier = (PowerUpManager.Instance != null) ? PowerUpManager.Instance.CurrentScoreMultiplier : 1;
        int basePoints = (enemyData != null) ? enemyData.scorePoints : 100;
        int points = basePoints * multiplier;

        Debug.Log($"[DuckHunt] 🎯 ¡Pato abatido! ({enemyData?.enemyName}) +{points} puntos {(multiplier > 1 ? "(¡DOBLE PUNTUACIÓN!)" : "")}");

        // Sumar puntos en el HUD
        PlayerHUD.AddScore(points);

        // Intentar soltar Power-Up al jugador en su mano izquierda
        if (PowerUpManager.Instance != null)
        {
            PowerUpManager.Instance.TryDropPowerUp(transform.position);
        }

        // Activar física de caída con rotación
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.linearVelocity = new Vector3(moveDirection.x * 1.5f, 1f, moveDirection.z * 1.5f);
            rb.angularVelocity = new Vector3(Random.Range(-5f, 5f), Random.Range(-5f, 5f), Random.Range(-5f, 5f));
        }

        // Desactivar el collider para no recibir más impactos
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        Destroy(gameObject, 1.5f);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bala") || 
            other.GetComponent<Bala>() != null ||
            other.name.Contains("Bala") ||
            other.name.Contains("bala") ||
            other.name.Contains("misil") ||
            other.name.Contains("bullet") ||
            other.name.Contains("Pellet"))
        {
            OnHit();
        }
        else
        {
            var playerHealth = other.GetComponentInParent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(1);
                OnHit();
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        OnTriggerEnter(collision.collider);
    }
}
