using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Bala : MonoBehaviour
{
    [Header("Configuracion del Proyectil")]
    public float velocidad = 45f;
    public float tiempoVida = 3.5f;

    [Header("Efectos Visuales")]
    public Color trailColor = new Color(1f, 0.75f, 0.1f, 1f);
    public AudioClip sonidoImpacto;

    private Vector3 previousPosition;
    private TrailRenderer trail;

    void Awake()
    {
        Collider col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;

        trail = GetComponent<TrailRenderer>();
        if (trail == null)
        {
            trail = gameObject.AddComponent<TrailRenderer>();
            trail.time = 0.25f;
            trail.startWidth = 0.12f;
            trail.endWidth = 0.01f;

            Material trailMat = Resources.Load<Material>("M_BulletGlow");
            if (trailMat == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color");
                trailMat = new Material(shader);
                trailMat.SetColor("_BaseColor", trailColor);
            }
            trail.material = trailMat;

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
        Vector3 moveDelta = newPosition - previousPosition;
        float distance = moveDelta.magnitude;

        if (distance > 0.001f)
        {
            if (Physics.Raycast(previousPosition, moveDelta.normalized, out RaycastHit hit, distance))
            {
                EnemyTarget enemy = hit.collider.GetComponentInParent<EnemyTarget>() ?? hit.collider.GetComponent<EnemyTarget>();
                if (enemy != null || hit.collider.name.ToLower().Contains("pato") || hit.collider.name.ToLower().Contains("bird"))
                {
                    if (sonidoImpacto != null) AudioSource.PlayClipAtPoint(sonidoImpacto, hit.point, 1.0f);
                    if (enemy != null) enemy.OnHit();
                    else Destroy(hit.collider.gameObject);
                    Destroy(gameObject);
                    return;
                }

                PowerUpItem powerUp = hit.collider.GetComponentInParent<PowerUpItem>() ?? hit.collider.GetComponent<PowerUpItem>();
                if (powerUp != null || hit.collider.name.ToLower().Contains("powerup"))
                {
                    if (powerUp != null) powerUp.Collect();
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
        EnemyTarget enemy = other.GetComponentInParent<EnemyTarget>() ?? other.GetComponent<EnemyTarget>();
        if (enemy != null || other.name.ToLower().Contains("pato") || other.name.ToLower().Contains("bird"))
        {
            if (sonidoImpacto != null) AudioSource.PlayClipAtPoint(sonidoImpacto, transform.position, 1.0f);
            if (enemy != null) enemy.OnHit();
            else Destroy(other.gameObject);
            Destroy(gameObject);
            return;
        }

        PowerUpItem powerUp = other.GetComponentInParent<PowerUpItem>() ?? other.GetComponent<PowerUpItem>();
        if (powerUp != null || other.name.ToLower().Contains("powerup"))
        {
            if (powerUp != null) powerUp.Collect();
            Destroy(gameObject);
            return;
        }
    }
}
