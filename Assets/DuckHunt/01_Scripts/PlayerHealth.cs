using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class PlayerHealth : MonoBehaviour
{
    public static PlayerHealth Instance { get; private set; }

    [Header("Configuracion de Vida")]
    public int maxHealth = 3;
    public int currentHealth;

    [Header("Escenas")]
    public string menuSceneName = "MainMenu";

    [Header("Feedback Visual")]
    public float invulnerabilityDuration = 1.0f;

    [Header("Escudo Protector (Power-Up)")]
    public bool isShieldActive = false;
    public float shieldTimeRemaining = 0f;

    private bool isDead = false;
    private bool isInvulnerable = false;
    private Canvas damageCanvas;
    private Image damageOverlay;
    private GameObject shieldVisualObj;
    private Renderer shieldRenderer;

    void Awake()
    {
        Instance = this;
        currentHealth = maxHealth;
        CrearDamageOverlay();
        CrearVisualEscudo();
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    void Start()
    {
        if (PlayerHUD.Instance != null)
        {
            PlayerHUD.Instance.UpdateHealth(currentHealth, maxHealth);
        }
    }

    void Update()
    {
        if (isShieldActive)
        {
            shieldTimeRemaining -= Time.deltaTime;
            PlayerHUD.SetShieldStatus(shieldTimeRemaining);

            if (shieldVisualObj != null)
            {
                shieldVisualObj.transform.Rotate(Vector3.up, 30f * Time.deltaTime, Space.World);
                if (shieldRenderer != null && shieldRenderer.material != null)
                {
                    float pulse = 0.35f + Mathf.Sin(Time.time * 6f) * 0.15f;
                    Color shieldCol = new Color(0f, 0.75f, 1f, pulse);
                    if (shieldRenderer.material.HasProperty("_BaseColor"))
                        shieldRenderer.material.SetColor("_BaseColor", shieldCol);
                    else
                        shieldRenderer.material.color = shieldCol;
                }
            }

            if (shieldTimeRemaining <= 0f)
            {
                DesactivarEscudo();
            }
        }
    }

    public void Heal(int amount = 1)
    {
        if (isDead) return;

        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        Debug.Log($"[PlayerHealth] Curado +{amount} HP | Vida actual: {currentHealth}/{maxHealth}");

        if (PlayerHUD.Instance != null)
        {
            PlayerHUD.Instance.UpdateHealth(currentHealth, maxHealth);
        }

        StartCoroutine(HealFeedbackRoutine());
    }

    public void ActivateShield(float duration = 12f)
    {
        if (isDead) return;

        isShieldActive = true;
        shieldTimeRemaining = Mathf.Max(shieldTimeRemaining, duration);
        PlayerHUD.SetShieldStatus(shieldTimeRemaining);

        if (shieldVisualObj != null)
        {
            shieldVisualObj.SetActive(true);
        }

        Debug.Log($"[PlayerHealth] ESCUDO ACTIVADO | Duracion: {duration}s");
    }

    public void DesactivarEscudo()
    {
        isShieldActive = false;
        shieldTimeRemaining = 0f;
        PlayerHUD.SetShieldStatus(0f);

        if (shieldVisualObj != null)
        {
            shieldVisualObj.SetActive(false);
        }
    }

    public void TakeDamage(int damage = 1)
    {
        if (isDead || isInvulnerable) return;

        if (isShieldActive)
        {
            Debug.Log("[PlayerHealth] El impacto fue absorbido por el ESCUDO.");
            AudioSource.PlayClipAtPoint(SonidosArmas.SonidoShieldAbsorb, transform.position, 1.0f);
            StartCoroutine(ShieldDeflectFeedbackRoutine());
            return;
        }

        currentHealth = Mathf.Max(0, currentHealth - damage);

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

    private IEnumerator ShieldDeflectFeedbackRoutine()
    {
        if (shieldRenderer != null && shieldRenderer.material != null)
        {
            Color flashCol = new Color(0.3f, 1f, 1f, 0.85f);
            if (shieldRenderer.material.HasProperty("_BaseColor"))
                shieldRenderer.material.SetColor("_BaseColor", flashCol);
            else
                shieldRenderer.material.color = flashCol;

            yield return new WaitForSeconds(0.2f);
        }
    }

    private IEnumerator HealFeedbackRoutine()
    {
        if (damageOverlay != null)
        {
            damageOverlay.color = new Color(0.1f, 0.9f, 0.2f, 0.35f);
            float elapsed = 0f;
            float duration = 0.4f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(0.35f, 0f, elapsed / duration);
                damageOverlay.color = new Color(0.1f, 0.9f, 0.2f, alpha);
                yield return null;
            }

            damageOverlay.color = new Color(0f, 0f, 0f, 0f);
        }
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

    public void Die()
    {
        if (isDead) return;
        isDead = true;

        var mover = GetComponent<VRAutoForwardMover>();
        if (mover != null) mover.avanzar = false;

        StartCoroutine(VolverAlMenuRutina());
    }

    private IEnumerator VolverAlMenuRutina()
    {
        // Detener musica de fondo
        GameObject bgMusic = GameObject.Find("MusicaDeFondo");
        if (bgMusic != null) Destroy(bgMusic);

        // Reproducir musica de derrota
        AudioClip pierdesClip = Resources.Load<AudioClip>("Musica/pierdes");
        if (pierdesClip != null)
        {
            AudioSource src = gameObject.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.spatialBlend = 0f;
            src.PlayOneShot(pierdesClip, 1.0f);
        }

        if (damageOverlay != null)
        {
            damageOverlay.color = new Color(0.8f, 0.0f, 0.0f, 0.85f);
        }

        Camera cam = Camera.main;
        if (cam != null)
        {
            Vector3 centerPos = cam.transform.position + cam.transform.forward * 3f;
            Quaternion rotation = Quaternion.LookRotation(cam.transform.forward);

            GameObject titleObj = new GameObject("GameOverTitle");
            titleObj.transform.position = centerPos + Vector3.up * 0.8f;
            titleObj.transform.rotation = rotation;
            var tmpTitle = titleObj.AddComponent<TMPro.TextMeshPro>();
            tmpTitle.text = "HAS PERDIDO\n<size=35%><color=white>Apunta y haz clic o dispara para elegir</color></size>";
            tmpTitle.fontSize = 7f;
            tmpTitle.alignment = TMPro.TextAlignmentOptions.Center;
            tmpTitle.color = Color.red;

            CrearBotonVR("VOLVER A JUGAR", centerPos + Vector3.up * 0.1f, rotation, true);
            CrearBotonVR("IR AL MENU", centerPos - Vector3.up * 0.5f, rotation, false);
        }

        yield return null;
    }

    private void CrearBotonVR(string texto, Vector3 pos, Quaternion rot, bool esReinicio)
    {
        GameObject btnObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        btnObj.name = texto;
        btnObj.transform.position = pos;
        btnObj.transform.rotation = rot;
        btnObj.transform.localScale = new Vector3(2.5f, 0.45f, 0.1f);
        
        btnObj.GetComponent<Renderer>().material.color = new Color(0.15f, 0.15f, 0.15f, 0.9f);
        
        var collider = btnObj.GetComponent<BoxCollider>();
        collider.isTrigger = false;
        
        var interactable = btnObj.AddComponent<XRSimpleInteractable>();
        interactable.selectEntered.AddListener((args) => 
        {
            PlayerHUD.ResetScore();
            if (esReinicio) SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            else SceneManager.LoadScene(!string.IsNullOrEmpty(menuSceneName) ? menuSceneName : "MainMenu");
        });

        var action = btnObj.AddComponent<GameOverAction>();
        action.isRestart = esReinicio;
        action.menuSceneName = menuSceneName;

        GameObject textObj = new GameObject("Texto");
        textObj.transform.SetParent(btnObj.transform);
        textObj.transform.localPosition = new Vector3(0, 0, -0.6f);
        textObj.transform.localRotation = Quaternion.identity;
        
        var tmp = textObj.AddComponent<TMPro.TextMeshPro>();
        tmp.text = texto;
        tmp.fontSize = 2f;
        tmp.alignment = TMPro.TextAlignmentOptions.Center;
        tmp.color = Color.white;
        
        textObj.transform.localScale = new Vector3(1f/2.5f, 1f/0.45f, 1f/0.1f);
    }

    void CrearDamageOverlay()
    {
        Camera cam = GetComponentInChildren<Camera>();
        if (cam == null) cam = Camera.main;

        if (cam != null)
        {
            GameObject canvasObj = new GameObject("DamageEffect_Canvas");
            canvasObj.transform.SetParent(cam.transform, false);
            canvasObj.transform.localPosition = new Vector3(0, 0, 0.15f);
            canvasObj.transform.localRotation = Quaternion.identity;

            damageCanvas = canvasObj.AddComponent<Canvas>();
            damageCanvas.renderMode = RenderMode.WorldSpace;
            
            RectTransform rect = canvasObj.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(2f, 2f);

            GameObject imgObj = new GameObject("DamageFlash");
            imgObj.transform.SetParent(canvasObj.transform, false);
            RectTransform imgRect = imgObj.AddComponent<RectTransform>();
            imgRect.anchorMin = Vector2.zero;
            imgRect.anchorMax = Vector2.one;
            imgRect.sizeDelta = Vector2.zero;

            damageOverlay = imgObj.AddComponent<Image>();
            damageOverlay.color = new Color(1f, 0f, 0f, 0f);
            damageOverlay.raycastTarget = false;
        }
    }

    void CrearVisualEscudo()
    {
        Camera cam = GetComponentInChildren<Camera>();
        if (cam == null) cam = Camera.main;

        Transform parentTransform = (cam != null) ? cam.transform : transform;

        shieldVisualObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        shieldVisualObj.name = "VR_Shield_Sphere";
        shieldVisualObj.transform.SetParent(parentTransform, false);
        shieldVisualObj.transform.localPosition = Vector3.zero;
        shieldVisualObj.transform.localScale = new Vector3(1.6f, 1.6f, 1.6f);

        Collider col = shieldVisualObj.GetComponent<Collider>();
        if (col != null) Destroy(col);

        shieldRenderer = shieldVisualObj.GetComponent<Renderer>();
        Shader urpUnlit = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Transparent") ?? Shader.Find("Standard");
        Material shieldMat = new Material(urpUnlit);
        shieldMat.SetColor("_BaseColor", new Color(0f, 0.75f, 1f, 0.35f));
        shieldRenderer.material = shieldMat;

        shieldVisualObj.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<EnemyTarget>() != null || other.name.ToLower().Contains("pato") || other.name.ToLower().Contains("bird"))
        {
            TakeDamage(1);
            var enemy = other.GetComponent<EnemyTarget>();
            if (enemy != null)
            {
                enemy.OnHit(false);
            }
        }
    }
}

 
