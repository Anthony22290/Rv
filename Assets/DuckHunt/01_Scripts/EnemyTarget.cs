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

    void Update()
    {
        if (isDead) return;

        float elapsedTime = Time.time - spawnTime;
        float speed = (enemyData != null) ? enemyData.moveSpeed : 4f;

        Vector3 currentPos = initialPosition + (moveDirection * (speed * elapsedTime));

        float verticalOffset = 0f;
        if (enemyData != null)
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

        currentPos.y += verticalOffset;
        transform.position = currentPos;

        // Orientar el pato hacia donde vuela con una ligera inclinación dinámica
        if (moveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            // Pequeño bamboleo de vuelo
            float tiltZ = Mathf.Sin(elapsedTime * 6f) * 8f;
            transform.rotation = targetRotation * Quaternion.Euler(0, 0, tiltZ);
        }
    }

    public void OnHit()
    {
        if (isDead) return;
        isDead = true;

        int points = (enemyData != null) ? enemyData.scorePoints : 100;
        Debug.Log($"[DuckHunt] ¡Pato abatido! ({enemyData?.enemyName}) +{points} puntos");

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
        if (other.CompareTag("Bala") || other.GetComponent<Bala>() != null)
        {
            OnHit();
        }
    }
}
