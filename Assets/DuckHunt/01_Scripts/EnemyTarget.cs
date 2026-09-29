using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class EnemyTarget : MonoBehaviour
{
    [Header("Configuración")]
    public EnemyDataSO enemyData;
    public bool isDynamicallyAggressive = false;

    [Header("Efectos")]
    public GameObject explosionPrefab;
    public GameObject featherEmitterPrefab;

    private Vector3 moveDirection = Vector3.right;
    private float spawnTime;
    private Vector3 initialPosition;
    private Vector3 basePosition;
    private bool isDead = false;
    private Rigidbody rb;
    private Transform playerTransform;
    private Animator birdAnimator;
    private AudioSource audioSource;
    private GameObject currentModelInstance;

    void Awake()
    {
        if (!gameObject.CompareTag("Enemy"))
        {
            gameObject.tag = "Enemy";
        }
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        rb.isKinematic = true;
        rb.useGravity = false;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.spatialBlend = 0.85f;
        audioSource.minDistance = 3f;
        audioSource.maxDistance = 35f;
        audioSource.playOnAwake = false;
        audioSource.rolloffMode = AudioRolloffMode.Linear;
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

        // Si se especificó un modelo de Living Birds
        if (data.birdPrefab != null)
        {
            // Ocultar o eliminar modelos hijos anteriores
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Transform child = transform.GetChild(i);
                if (child.name.StartsWith("Model") || child.name.StartsWith("tripo_") || child.name.StartsWith("lb_"))
                {
                    Destroy(child.gameObject);
                }
            }

            currentModelInstance = Instantiate(data.birdPrefab, transform);
            currentModelInstance.transform.localPosition = Vector3.zero;
            currentModelInstance.transform.localRotation = Quaternion.identity;
            currentModelInstance.transform.localScale = Vector3.one;

            // Desactivar lb_Bird y colliders en el prefab hijo para que EnemyTarget maneje la física y colisiones
            var lbBird = currentModelInstance.GetComponent<lb_Bird>();
            if (lbBird != null) Destroy(lbBird);

            var childColliders = currentModelInstance.GetComponentsInChildren<Collider>();
            foreach (var col in childColliders)
            {
                if (col.gameObject != gameObject)
                {
                    Destroy(col);
                }
            }

            birdAnimator = currentModelInstance.GetComponentInChildren<Animator>();
            if (birdAnimator != null)
            {
                birdAnimator.applyRootMotion = false;
                birdAnimator.SetBool("flying", true);
                birdAnimator.speed = 1.25f;
            }
        }
        else
        {
            birdAnimator = GetComponentInChildren<Animator>();
            if (birdAnimator != null)
            {
                birdAnimator.applyRootMotion = false;
                birdAnimator.SetBool("flying", true);
            }
        }

        // Aplicar tintado de color si es necesario
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (var r in renderers)
        {
            if (r != null)
            {
                if (data.duckMaterial != null)
                {
                    r.material = data.duckMaterial;
                }

                if (data.bodyColor != Color.white && r.material != null)
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

        // Ajustar Collider para asegurar buen hitbox
        BoxCollider boxCol = GetComponent<BoxCollider>();
        if (boxCol != null)
        {
            boxCol.isTrigger = true;
            boxCol.size = new Vector3(1.2f, 0.9f, 1.2f);
            boxCol.center = new Vector3(0, 0.2f, 0);
        }

        // Reproducir sonido de aleteo y canto característico
        if (audioSource != null)
        {
            if (data.birdFlySound != null)
            {
                audioSource.PlayOneShot(data.birdFlySound, 0.6f);
            }
            else
            {
                audioSource.PlayOneShot(SonidosArmas.SonidoPajaroAleteo, 0.5f);
            }

            if (data.birdChirpSound != null)
            {
                StartCoroutine(CantoPeriodicoRutina(data.birdChirpSound));
            }
        }

        Destroy(gameObject, data.maxLifeTime);
    }

    private IEnumerator CantoPeriodicoRutina(AudioClip chirp)
    {
        yield return new WaitForSeconds(Random.Range(0.2f, 0.8f));
        while (!isDead && chirp != null && audioSource != null)
        {
            audioSource.PlayOneShot(chirp, 0.7f);
            yield return new WaitForSeconds(Random.Range(2.5f, 4.5f));
        }
    }

    void Update()
    {
        if (isDead) return;

        float elapsedTime = Time.time - spawnTime;
        float speed = (enemyData != null) ? enemyData.moveSpeed : 4.5f;

        if ((isDynamicallyAggressive || (enemyData != null && enemyData.isAggressive)) && playerTransform != null)
        {
            if (Vector3.Distance(basePosition, playerTransform.position) > 2.5f)
            {
                Vector3 headPos = playerTransform.position;
                headPos.y += 0.5f;
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

            if (birdAnimator != null)
            {
                birdAnimator.SetFloat("flyingDirectionX", tiltZ / 8f);
            }
        }

        // Colisión por proximidad con el jugador
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
            if (featherEmitterPrefab == null)
            {
                featherEmitterPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/DuckHunt/02_Sprites/living birds/resources/featherEmitter.prefab");
            }
#endif
            if (featherEmitterPrefab != null)
            {
                GameObject feathers = Instantiate(featherEmitterPrefab, transform.position, Quaternion.identity);
                Destroy(feathers, 3f);
            }

            if (explosionPrefab != null)
            {
                GameObject exp = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
                Destroy(exp, 3f);
            }

            if (enemyData != null && enemyData.birdHitSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(enemyData.birdHitSound, 0.9f);
            }
        }

        if (birdAnimator != null)
        {
            birdAnimator.SetBool("flying", false);
            birdAnimator.SetTrigger("die");
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

        Destroy(gameObject, 1.8f);
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
