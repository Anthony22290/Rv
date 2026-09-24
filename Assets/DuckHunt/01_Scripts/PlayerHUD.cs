using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Interfaz de Usuario (HUD en Realidad Virtual) para mostrar los Puntos y la Vida del jugador.
/// Permanece cómodamente visible en el campo de visión del jugador.
/// </summary>
public class PlayerHUD : MonoBehaviour
{
    public static PlayerHUD Instance { get; private set; }

    [Header("Referencias de Texto UI")]
    [Tooltip("Texto para mostrar la puntuación")]
    public TextMeshProUGUI scoreText;

    [Tooltip("Texto para mostrar la vida / corazones")]
    public TextMeshProUGUI healthText;

    [Header("Configuración de Posición en VR")]
    [Tooltip("Distancia desde la cámara")]
    public float distanceFromCamera = 1.35f;

    [Tooltip("Desplazamiento horizontal y vertical respecto al centro de visión")]
    public Vector3 offset = new Vector3(0f, 0.42f, 0f);

    [Tooltip("Velocidad de seguimiento suave del HUD")]
    public float followSpeed = 8.5f;

    [Header("Puntuación Global")]
    public static int totalScore = 0;

    private Camera targetCamera;
    private int displayedScore = 0;
    private Coroutine scoreAnimCoroutine;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        targetCamera = Camera.main;
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
    }

    void LateUpdate()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
            if (targetCamera == null) return;
        }

        // Posicionar el HUD frente a la cámara suavemente
        Vector3 targetPosition = targetCamera.transform.position 
            + (targetCamera.transform.forward * distanceFromCamera) 
            + (targetCamera.transform.up * offset.y) 
            + (targetCamera.transform.right * offset.x);

        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * followSpeed);
        
        // Orientar hacia el jugador
        transform.rotation = Quaternion.LookRotation(transform.position - targetCamera.transform.position);
    }

    /// <summary>
    /// Suma puntos a la puntuación global y actualiza la interfaz.
    /// </summary>
    public static void AddScore(int points)
    {
        totalScore += points;
        if (Instance != null)
        {
            Instance.OnScoreChanged(totalScore, points);
        }
    }

    /// <summary>
    /// Reinicia la puntuación al iniciar una nueva partida o morir.
    /// </summary>
    public static void ResetScore()
    {
        totalScore = 0;
        if (Instance != null)
        {
            Instance.UpdateScoreDisplay(0);
        }
    }

    private void OnScoreChanged(int newScore, int addedPoints)
    {
        if (scoreAnimCoroutine != null)
        {
            StopCoroutine(scoreAnimCoroutine);
        }
        scoreAnimCoroutine = StartCoroutine(AnimateScoreRoutine(newScore));
    }

    private IEnumerator AnimateScoreRoutine(int targetScore)
    {
        int startScore = displayedScore;
        float elapsed = 0f;
        float duration = 0.35f;

        if (scoreText != null)
        {
            scoreText.transform.localScale = Vector3.one * 1.2f;
            scoreText.color = new Color(1.0f, 0.95f, 0.3f);
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
            scoreText.text = $"🎯 PUNTOS: <color=#FFD700>{score:N0}</color>";
        }
    }

    /// <summary>
    /// Actualiza la visualización de la vida (corazones y número).
    /// </summary>
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
}
