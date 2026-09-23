using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Bala : MonoBehaviour
{
    [Header("Configuración del Proyectil")]
    public float velocidad = 45f;
    public float tiempoVida = 3.5f;

    [Header("Efectos Visuales")]
    public Color trailColor = new Color(1f, 0.75f, 0.1f, 1f); // Amarillo / Naranja brillante

    private Vector3 previousPosition;
    private TrailRenderer trail;

    void Awake()
    {
        // Asegurar que el collider sea trigger
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }

        // Configurar o añadir TrailRenderer para que la estela del disparo sea claramente visible
        trail = GetComponent<TrailRenderer>();
        if (trail == null)
        {
            trail = gameObject.AddComponent<TrailRenderer>();
            trail.time = 0.25f;
            trail.startWidth = 0.12f;
            trail.endWidth = 0.01f;

            // Material Unlit brillante
            Material trailMat = Resources.Load<Material>("M_BulletGlow");
            if (trailMat == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
                if (shader == null) shader = Shader.Find("Unlit/Color");
                trailMat = new Material(shader);
                trailMat.SetColor("_BaseColor", trailColor);
            }
            trail.material = trailMat;

            // Gradiente de color en la estela
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(trailColor, 0.0f), new GradientColorKey(new Color(1f, 0.3f, 0f), 1.0f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) }
            );
            trail.colorGradient = gradient;
        }
    }

    void Start()
    {
        previousPosition = transform.position;
        Destroy(gameObject, tiempoVida);
    }

    void Update()
    {
        Vector3 newPosition = transform.position + (transform.forward * (velocidad * Time.deltaTime));

        // Raycast continuo entre frames para evitar que la bala atraviese objetos rápidos
        Vector3 moveDelta = newPosition - previousPosition;
        float distance = moveDelta.magnitude;

        if (distance > 0.001f)
        {
            if (Physics.Raycast(previousPosition, moveDelta.normalized, out RaycastHit hit, distance))
            {
                if (hit.collider.CompareTag("Enemy") || hit.collider.GetComponent<EnemyTarget>() != null)
                {
                    EnemyTarget enemy = hit.collider.GetComponent<EnemyTarget>();
                    if (enemy != null)
                    {
                        enemy.OnHit();
                    }
                    else
                    {
                        Destroy(hit.collider.gameObject);
                    }

                    Destroy(gameObject);
                    return;
                }
            }
        }

        previousPosition = transform.position;
        transform.position = newPosition;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            EnemyTarget enemy = other.GetComponent<EnemyTarget>();
            if (enemy != null)
            {
                enemy.OnHit();
            }
            else
            {
                Destroy(other.gameObject);
            }

            Destroy(gameObject);
        }
    }
}