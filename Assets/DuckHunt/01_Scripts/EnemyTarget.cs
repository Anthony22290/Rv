using UnityEngine;

[RequireComponent(typeof(Collider))]
public class EnemyTarget : MonoBehaviour
{
    [Header("Configuracion")]
    public EnemyDataSO enemyData;
    public bool isDynamicallyAggressive = false;
    [Header("Efectos")]
    public GameObject explosionPrefab;

    private Vector3 moveDirection = Vector3.right;
    private float spawnTime;
    private Vector3 initialPosition;
    private Vector3 basePosition;
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
        basePosition = transform.position;

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
        basePosition = transform.position;
        spawnTime = Time.time;
        ApplyData(data);
    }

    public void ApplyData(EnemyDataSO data)
    {
        if (data == null) return;

        transform.localScale = data.scale;

        renderers = GetComponentsInChildren<Renderer>();
        foreach (var r in renderers)
        {
            if (r != null)
            {
                if (data.duckMaterial != null)
                {
                    r.material = data.duckMaterial;
                }

                if (r.material != null)
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
        }

        Destroy(gameObject, data.maxLifeTime);
    }

    void Update()
    {
        if (isDead) return;

        float elapsedTime = Time.time - spawnTime;
        float speed = (enemyData != null) ? enemyData.moveSpeed : 4f;

        if ((isDynamicallyAggressive || (enemyData != null && enemyData.isAggressive)) && playerTransform != null)
        {
            if (Vector3.Distance(basePosition, playerTransform.position) > 4.0f)
            {
                Vector3 headPos = playerTransform.position;
                headPos.y += 0.8f;
                Vector3 dir = (headPos - basePosition).normalized;
                moveDirection = Vector3.Lerp(moveDirection, dir, Time.deltaTime * 3.5f).normalized;
            }
        }

        basePosition += moveDirection * speed * Time.deltaTime;

        float verticalOffset = 0f;
        if (enemyData != null && !isDynamicallyAggressive && !enemyData.isAggressive)
        {
            switch (enemyData.movementPattern)
            {
                case EnemyMovementPattern.SineWave:
                    verticalOffset = Mathf.Sin(elapsedTime * enemyData.waveFrequency) * enemyData.waveAmplitude;
                    break;
                case EnemyMovementPattern.ArcFly:
                    verticalOffset = Mathf.Sin((elapsedTime / enemyData.maxLifeTime) * Mathf.PI) * enemyData.waveAmplitude;
                    break;
                case EnemyMovementPattern.StraightLine:
                default:
                    verticalOffset = 0f;
                    break;
            }
        }

        transform.position = basePosition + new Vector3(0, verticalOffset, 0);

        if (moveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            float tiltZ = Mathf.Sin(elapsedTime * 6f) * 8f;
            transform.rotation = targetRotation * Quaternion.Euler(0, 0, tiltZ);
        }

        if (playerTransform != null && Vector3.Distance(transform.position, playerTransform.position) < 1.4f)
        {
            var health = playerTransform.GetComponentInParent<PlayerHealth>() ?? Object.FindAnyObjectByType<PlayerHealth>();
            if (health != null) health.TakeDamage(1);
            OnHit(false);
        }
    }

    public void OnHit(bool killedByPlayer = true)
    {
        if (isDead) return;
        isDead = true;

        if (killedByPlayer)
        {
            int points = (enemyData != null) ? enemyData.scorePoints : 100;
            PlayerHUD.AddScore(points);

#if UNITY_EDITOR
            if (explosionPrefab == null)
            {
                explosionPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/DuckHunt/03_Prefabs/_Explosion (BASE).prefab");
            }
#endif
            if (explosionPrefab != null)
            {
                GameObject exp = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
                Destroy(exp, 4f); // Destruir la explosion despues de 4 seg
            }
        }

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.linearVelocity = new Vector3(moveDirection.x * 1.5f, 1f, moveDirection.z * 1.5f);
            rb.angularVelocity = new Vector3(Random.Range(-5f, 5f), Random.Range(-5f, 5f), Random.Range(-5f, 5f));
        }

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
            OnHit(false);
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        OnTriggerEnter(collision.collider);
    }
}



 

 
