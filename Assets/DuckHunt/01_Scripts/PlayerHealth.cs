using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Gestiona la vida del jugador, daño recibido y reinicio/retorno al Menú Principal al morir.
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    [Header("Configuración de Vida")]
    [Tooltip("Vida máxima del jugador")]
    public int maxHealth = 3;

    [Tooltip("Vida actual")]
    public int currentHealth;

    [Header("Escenas")]
    [Tooltip("Nombre de la escena a cargar al morir (Menú Principal)")]
    public string menuSceneName = "MainMenu";

    [Header("Feedback Visual")]
    [Tooltip("Tiempo de invulnerabilidad tras recibir daño")]
    public float invulnerabilityDuration = 1.0f;

    private bool isDead = false;
    private bool isInvulnerable = false;
    private Canvas damageCanvas;
    private Image damageOverlay;

    void Awake()
    {
        currentHealth = maxHealth;
        CrearDamageOverlay();
    }

    void CrearDamageOverlay()
    {
        Camera cam = GetComponentInChildren<Camera>();
        if (cam == null) cam = Camera.main;

        if (cam != null)
        {
            GameObject canvasObj = new GameObject("DamageEffect_Canvas");
            canvasObj.transform.SetParent(cam.transform, false);
            canvasObj.transform.localPosition = new Vector3(0, 0, 0.35f);
            canvasObj.transform.localRotation = Quaternion.identity;

            damageCanvas = canvasObj.AddComponent<Canvas>();
            damageCanvas.renderMode = RenderMode.WorldSpace;
            damageCanvas.worldCamera = cam;

            RectTransform rect = canvasObj.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(1f, 1f);

            GameObject imgObj = new GameObject("DamageFlash");
            imgObj.transform.SetParent(canvasObj.transform, false);
            RectTransform imgRect = imgObj.AddComponent<RectTransform>();
            imgRect.anchorMin = Vector2.zero;
            imgRect.anchorMax = Vector2.one;
            imgRect.sizeDelta = Vector2.zero;

            damageOverlay = imgObj.AddComponent<Image>();
            damageOverlay.color = new Color(0.85f, 0.05f, 0.05f, 0f);
            damageOverlay.raycastTarget = false;
        }
    }

    void Start()
    {
        if (PlayerHUD.Instance != null)
        {
            PlayerHUD.Instance.UpdateHealth(currentHealth, maxHealth);
        }
    }

    /// <summary>
    /// Aplica daño al jugador. Si tiene un escudo activo, lo consume y bloquea el daño.
    /// Si la vida llega a 0, activa la muerte y regresa al menú.
    /// </summary>
    public void TakeDamage(int damage = 1)
    {
        if (isDead || isInvulnerable) return;

        // Si el jugador tiene un Escudo activo, absorbe el daño por completo
        if (PowerUpManager.Instance != null && PowerUpManager.Instance.IntentarConsumirEscudo())
        {
            StartCoroutine(ShieldBlockFeedbackRoutine());
            return;
        }

        currentHealth = Mathf.Max(0, currentHealth - damage);
        Debug.Log($"[PlayerHealth] ¡Jugador herido! Vida restante: {currentHealth}/{maxHealth}");

        if (PlayerHUD.Instance != null)
        {
            PlayerHUD.Instance.UpdateHealth(currentHealth, maxHealth);
        }

        StartCoroutine(DamageFeedbackRoutine());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private IEnumerator ShieldBlockFeedbackRoutine()
    {
        isInvulnerable = true;

        if (damageOverlay != null)
        {
            damageOverlay.color = new Color(0.1f, 0.75f, 1.0f, 0.5f); // Destello azul/cian de escudo
            float elapsed = 0f;
            float duration = 0.45f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(0.5f, 0f, elapsed / duration);
                damageOverlay.color = new Color(0.1f, 0.75f, 1.0f, alpha);
                yield return null;
            }

            damageOverlay.color = new Color(0.1f, 0.75f, 1.0f, 0f);
        }

        yield return new WaitForSeconds(0.4f);
        isInvulnerable = false;
    }

    private IEnumerator DamageFeedbackRoutine()
    {
        isInvulnerable = true;

        if (damageOverlay != null)
        {
            damageOverlay.color = new Color(0.85f, 0.05f, 0.05f, 0.45f);
            float elapsed = 0f;
            float duration = 0.4f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(0.45f, 0f, elapsed / duration);
                damageOverlay.color = new Color(0.85f, 0.05f, 0.05f, alpha);
                yield return null;
            }

            damageOverlay.color = new Color(0.85f, 0.05f, 0.05f, 0f);
        }

        yield return new WaitForSeconds(invulnerabilityDuration - 0.4f);
        isInvulnerable = false;
    }

    /// <summary>
    /// Gestiona la muerte del jugador y carga la escena del menú principal.
    /// </summary>
    public void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log("[PlayerHealth] ¡Jugador ha muerto! Volviendo al Menú Principal...");

        var mover = GetComponent<VRAutoForwardMover>();
        if (mover != null) mover.avanzar = false;

        StartCoroutine(VolverAlMenuRutina());
    }

    private IEnumerator VolverAlMenuRutina()
    {
        if (damageOverlay != null)
        {
            damageOverlay.color = new Color(0.6f, 0.0f, 0.0f, 0.75f);
        }

        yield return new WaitForSeconds(1.2f);

        if (!string.IsNullOrEmpty(menuSceneName))
        {
            SceneManager.LoadScene(menuSceneName);
        }
        else
        {
            SceneManager.LoadScene(0);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy") || other.GetComponent<EnemyTarget>() != null)
        {
            TakeDamage(1);
            var enemy = other.GetComponent<EnemyTarget>();
            if (enemy != null)
            {
                enemy.OnHit();
            }
        }
    }
}
