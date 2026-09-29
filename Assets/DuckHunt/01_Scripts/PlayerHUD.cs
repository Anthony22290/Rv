using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Interfaz de Usuario (HUD en Realidad Virtual) para mostrar los Puntos, la Vida del jugador y Power-Ups activos.
/// Permanece cómodamente visible en el campo de visión del jugador con auto-generación de interfaz.
/// </summary>
public class PlayerHUD : MonoBehaviour
{
    public static PlayerHUD Instance { get; private set; }

    [Header("Referencias de Texto UI")]
    [Tooltip("Texto para mostrar la puntuación")]
    public TextMeshProUGUI scoreText;

    [Tooltip("Texto para mostrar la vida / corazones")]
    public TextMeshProUGUI healthText;

    [Tooltip("Texto para mostrar estados de Power-Ups")]
    public TextMeshProUGUI powerUpText;

    [Header("Configuración de Posición en VR")]
    [Tooltip("Distancia desde la cámara")]
    public float distanceFromCamera = 1.35f;

    [Tooltip("Desplazamiento horizontal y vertical respecto al centro de visión")]
    public Vector3 offset = new Vector3(0f, 0.42f, 0f);

    [Tooltip("Velocidad de seguimiento suave del HUD")]
    public float followSpeed = 9.0f;

    [Header("Puntuación Global")]
    public static int totalScore = 0;

    [Header("Estado de Power-Ups")]
    public static float doublePointsTimer = 0f;
    public static bool isDoublePointsActive => doublePointsTimer > 0f;

    private static float shieldTimer = 0f;
    public static bool isShieldActive => shieldTimer > 0f;

    private Camera targetCamera;
    private int displayedScore = 0;
    private Coroutine scoreAnimCoroutine;

    void Awake()
    {
        Instance = this;
        targetCamera = Camera.main;
        if (targetCamera == null)
        {
            var mover = Object.FindAnyObjectByType<VRAutoForwardMover>();
            if (mover != null) targetCamera = mover.GetComponentInChildren<Camera>();
        }

        if (scoreText == null || healthText == null)
        {
            CrearHUDUIAutomatico();
        }
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
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        displayedScore = totalScore;
        UpdateScoreDisplay(totalScore);

        PlayerHealth playerHealth = Object.FindAnyObjectByType<PlayerHealth>();
        if (playerHealth != null)
        {
            UpdateHealth(playerHealth.currentHealth, playerHealth.maxHealth);
        }
        else
        {
            UpdateHealth(3, 3);
        }

        UpdatePowerUpDisplay();
    }

    void Update()
    {
        bool changed = false;

        if (doublePointsTimer > 0f)
        {
            doublePointsTimer -= Time.deltaTime;
            if (doublePointsTimer <= 0f)
            {
                doublePointsTimer = 0f;
                Debug.Log("[PlayerHUD] Power-Up Doble Puntos terminado.");
            }
            changed = true;
        }

        if (shieldTimer > 0f)
        {
            shieldTimer -= Time.deltaTime;
            if (shieldTimer <= 0f)
            {
                shieldTimer = 0f;
                Debug.Log("[PlayerHUD] Power-Up Escudo terminado.");
            }
            changed = true;
        }

        if (changed)
        {
            UpdatePowerUpDisplay();
        }
    }

    void LateUpdate()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
            if (targetCamera == null) return;
        }

        Vector3 targetPosition = targetCamera.transform.position 
            + (targetCamera.transform.forward * distanceFromCamera) 
            + (targetCamera.transform.up * offset.y) 
            + (targetCamera.transform.right * offset.x);

        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * followSpeed);
        transform.rotation = Quaternion.LookRotation(transform.position - targetCamera.transform.position);
    }

    /// <summary>
    /// Activa el multiplicador de doble puntos por la duración indicada en segundos.
    /// </summary>
    public static void ActivateDoublePoints(float duration = 15f)
    {
        doublePointsTimer = Mathf.Max(doublePointsTimer, duration);
        Debug.Log($"[PlayerHUD] ⭐ ¡DOBLE PUNTOS ACTIVADO! ({duration}s)");
        if (Instance != null)
        {
            Instance.UpdatePowerUpDisplay();
        }
    }

    /// <summary>
    /// Actualiza el temporizador del escudo en el HUD.
    /// </summary>
    public static void SetShieldStatus(float duration)
    {
        shieldTimer = duration;
        if (Instance != null)
        {
            Instance.UpdatePowerUpDisplay();
        }
    }

    /// <summary>
    /// Suma puntos a la puntuación global (aplicando multiplicador x2 si está activo) y actualiza la interfaz.
    /// </summary>
    public static void AddScore(int points)
    {
        int finalPoints = isDoublePointsActive ? (points * 2) : points;
        totalScore += finalPoints;

        if (isDoublePointsActive)
        {
            Debug.Log($"[PlayerHUD] 🎯 +{finalPoints} PUNTOS (⭐ 2X ACTIVO!) | Puntuación total: {totalScore}");
        }
        else
        {
            Debug.Log($"[PlayerHUD] 🎯 +{finalPoints} PUNTOS | Puntuación total: {totalScore}");
        }

        if (Instance == null)
        {
            GameObject hudObj = new GameObject("PlayerHUD_AutoGenerated");
            Instance = hudObj.AddComponent<PlayerHUD>();
        }
        
        Instance.OnScoreChanged(totalScore, finalPoints);
    }

    /// <summary>
    /// Reinicia la puntuación al iniciar una nueva partida o tras morir.
    /// </summary>
    public static void ResetScore()
    {
        totalScore = 0;
        doublePointsTimer = 0f;
        shieldTimer = 0f;
        if (Instance != null)
        {
            Instance.displayedScore = 0;
            Instance.UpdateScoreDisplay(0);
            Instance.UpdatePowerUpDisplay();
        }
    }

    private void OnScoreChanged(int newScore, int addedPoints)
    {
        if (scoreAnimCoroutine != null)
        {
            StopCoroutine(scoreAnimCoroutine);
        }
        scoreAnimCoroutine = StartCoroutine(AnimateScoreRoutine(newScore, isDoublePointsActive));
    }

    private IEnumerator AnimateScoreRoutine(int targetScore, bool wasDouble)
    {
        int startScore = displayedScore;
        float elapsed = 0f;
        float duration = 0.35f;

        if (scoreText != null)
        {
            scoreText.transform.localScale = Vector3.one * (wasDouble ? 1.4f : 1.25f);
            scoreText.color = wasDouble ? new Color(1.0f, 0.6f, 0.1f) : new Color(1.0f, 0.95f, 0.3f);
        }

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            displayedScore = (int)Mathf.Lerp(startScore, targetScore, t);
            UpdateScoreDisplay(displayedScore);
            yield return null;
        }

        displayedScore = targetScore;
        UpdateScoreDisplay(displayedScore);

        if (scoreText != null)
        {
            scoreText.transform.localScale = Vector3.one;
            scoreText.color = Color.white;
        }
    }

    public void UpdateScoreDisplay(int score)
    {
        if (scoreText != null)
        {
            string bonusIndicator = isDoublePointsActive ? " <color=#FFA500>[2X]</color>" : "";
            scoreText.text = $"🎯 PUNTOS: <color=#FFD700>{score:N0}</color>{bonusIndicator}";
        }
    }

    public void UpdateHealth(int currentHealth, int maxHealth)
    {
        if (healthText != null)
        {
            string hearts = "";
            for (int i = 0; i < maxHealth; i++)
            {
                if (i < currentHealth)
                {
                    hearts += "<color=#FF3B30>❤️</color> ";
                }
                else
                {
                    hearts += "<color=#555555>🖤</color> ";
                }
            }

            healthText.text = $"VIDA: {hearts}({currentHealth}/{maxHealth})";
        }
    }

    public void UpdatePowerUpDisplay()
    {
        if (powerUpText == null) return;

        string powerUpInfo = "";
        if (isShieldActive)
        {
            powerUpInfo += $"<color=#00E5FF>🛡️ ESCUDO ({Mathf.CeilToInt(shieldTimer)}s)</color> ";
        }
        if (isDoublePointsActive)
        {
            powerUpInfo += $"<color=#FFD700>⭐ 2X PUNTOS ({Mathf.CeilToInt(doublePointsTimer)}s)</color>";
        }

        powerUpText.text = powerUpInfo;
        powerUpText.gameObject.SetActive(!string.IsNullOrEmpty(powerUpInfo));
    }

    private void CrearHUDUIAutomatico()
    {
        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = gameObject.AddComponent<Canvas>();
        }
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = targetCamera;

        CanvasScaler scaler = GetComponent<CanvasScaler>();
        if (scaler == null)
        {
            scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.dynamicPixelsPerUnit = 3f;
        }

        RectTransform rt = GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.sizeDelta = new Vector2(760f, 160f);
            transform.localScale = Vector3.one * 0.0016f;
        }

        // Panel contenedor estilizado
        GameObject panelObj = new GameObject("HUD_Panel");
        panelObj.transform.SetParent(transform, false);
        RectTransform panelRT = panelObj.AddComponent<RectTransform>();
        panelRT.anchorMin = Vector2.zero;
        panelRT.anchorMax = Vector2.one;
        panelRT.sizeDelta = Vector2.zero;

        Image panelImg = panelObj.AddComponent<Image>();
        panelImg.color = new Color(0.06f, 0.06f, 0.06f, 0.85f);

        Outline outline = panelObj.AddComponent<Outline>();
        outline.effectColor = new Color(0.9f, 0.75f, 0.2f, 0.8f);
        outline.effectDistance = new Vector2(2.5f, -2.5f);

        // Texto de Puntos (Izquierda)
        GameObject scoreObj = new GameObject("Score_Text");
        scoreObj.transform.SetParent(panelObj.transform, false);
        RectTransform scoreRT = scoreObj.AddComponent<RectTransform>();
        scoreRT.anchorMin = new Vector2(0.04f, 0.35f);
        scoreRT.anchorMax = new Vector2(0.52f, 0.92f);
        scoreRT.sizeDelta = Vector2.zero;

        scoreText = scoreObj.AddComponent<TextMeshProUGUI>();
        scoreText.text = $"🎯 PUNTOS: <color=#FFD700>{totalScore:N0}</color>";
        scoreText.fontSize = 32;
        scoreText.fontStyle = FontStyles.Bold;
        scoreText.alignment = TextAlignmentOptions.MidlineLeft;
        scoreText.color = Color.white;

        // Texto de Vida (Derecha)
        GameObject healthObj = new GameObject("Health_Text");
        healthObj.transform.SetParent(panelObj.transform, false);
        RectTransform healthRT = healthObj.AddComponent<RectTransform>();
        healthRT.anchorMin = new Vector2(0.52f, 0.35f);
        healthRT.anchorMax = new Vector2(0.96f, 0.92f);
        healthRT.sizeDelta = Vector2.zero;

        healthText = healthObj.AddComponent<TextMeshProUGUI>();
        healthText.text = "VIDA: <color=#FF3B30>❤️</color> <color=#FF3B30>❤️</color> <color=#FF3B30>❤️</color> (3/3)";
        healthText.fontSize = 28;
        healthText.fontStyle = FontStyles.Bold;
        healthText.alignment = TextAlignmentOptions.MidlineRight;
        healthText.color = Color.white;

        // Texto de Power-Ups Activos (Fila Inferior)
        GameObject powerUpObj = new GameObject("PowerUp_Text");
        powerUpObj.transform.SetParent(panelObj.transform, false);
        RectTransform powerUpRT = powerUpObj.AddComponent<RectTransform>();
        powerUpRT.anchorMin = new Vector2(0.04f, 0.05f);
        powerUpRT.anchorMax = new Vector2(0.96f, 0.35f);
        powerUpRT.sizeDelta = Vector2.zero;

        powerUpText = powerUpObj.AddComponent<TextMeshProUGUI>();
        powerUpText.text = "";
        powerUpText.fontSize = 22;
        powerUpText.fontStyle = FontStyles.Bold;
        powerUpText.alignment = TextAlignmentOptions.Center;
        powerUpText.color = Color.cyan;
        powerUpText.gameObject.SetActive(false);
    }
}
