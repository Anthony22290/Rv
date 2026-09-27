using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gestor global de Power-Ups: almacena los efectos activos (Escudo, Ralentizar patos, Doble Puntuación),
/// gestiona los tiempos de duración y coordina el drop al abatir enemigos.
/// </summary>
public class PowerUpManager : MonoBehaviour
{
    public static PowerUpManager Instance { get; private set; }

    [Header("Configuración de Drops")]
    [Tooltip("Probabilidad de que un pato suelte un Power-Up al morir (0.0 a 1.0)")]
    [Range(0f, 1f)]
    public float dropChance = 0.45f;

    [Tooltip("Catálogo de Power-Ups disponibles (ScriptableObjects)")]
    public PowerUpDataSO[] availablePowerUps;

    [Header("Audio")]
    private AudioSource audioSource;

    // Estados de Efectos Activos
    public bool HasShield { get; private set; } = false;
    public bool IsSlowMotionActive { get; private set; } = false;
    public float SlowMotionTimeRemaining { get; private set; } = 0f;
    public float CurrentSlowFactor { get; private set; } = 0.35f;

    public bool IsDoubleScoreActive { get; private set; } = false;
    public float DoubleScoreTimeRemaining { get; private set; } = 0f;

    public float CurrentEnemySpeedMultiplier => IsSlowMotionActive ? CurrentSlowFactor : 1.0f;
    public int CurrentScoreMultiplier => IsDoubleScoreActive ? 2 : 1;

    private Coroutine slowMotionCoroutine;
    private Coroutine doubleScoreCoroutine;
    private GameObject shieldVisualDome;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
        }

        // Auto-cargar Scriptable Objects si no fueron asignados en el inspector
        if (availablePowerUps == null || availablePowerUps.Length == 0)
        {
            availablePowerUps = Resources.FindObjectsOfTypeAll<PowerUpDataSO>();
        }

        if (availablePowerUps == null || availablePowerUps.Length == 0)
        {
            // Fallback: Crear instancias en memoria si no se encontraron
            CrearPowerUpsPorDefecto();
        }
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void CrearPowerUpsPorDefecto()
    {
        List<PowerUpDataSO> list = new List<PowerUpDataSO>();

        var escudo = ScriptableObject.CreateInstance<PowerUpDataSO>();
        escudo.powerUpName = "Escudo Defensivo";
        escudo.powerUpType = PowerUpType.Escudo;
        escudo.iconEmoji = "🛡️";
        escudo.description = "Protege contra el siguiente impacto de daño.";
        escudo.duration = 0f; // Permanente hasta recibir golpe
        escudo.themeColor = new Color(0.15f, 0.75f, 1f, 1f);
        list.Add(escudo);

        var slow = ScriptableObject.CreateInstance<PowerUpDataSO>();
        slow.powerUpName = "Cámara Lenta";
        slow.powerUpType = PowerUpType.Ralentizar;
        slow.iconEmoji = "⏱️";
        slow.description = "Ralentiza el vuelo de los pájaros durante 5 segundos.";
        slow.duration = 5f;
        slow.slowSpeedFactor = 0.35f;
        slow.themeColor = new Color(0.7f, 0.3f, 1f, 1f);
        list.Add(slow);

        var x2 = ScriptableObject.CreateInstance<PowerUpDataSO>();
        x2.powerUpName = "Doble Puntuación";
        x2.powerUpType = PowerUpType.DoblePuntos;
        x2.iconEmoji = "⭐";
        x2.description = "Duplica todos los puntos obtenidos durante 10 segundos.";
        x2.duration = 10f;
        x2.scoreMultiplier = 2;
        x2.themeColor = new Color(1f, 0.85f, 0.1f, 1f);
        list.Add(x2);

        availablePowerUps = list.ToArray();
    }

    /// <summary>
    /// Intenta otorgar un Power-Up al abatir un pato.
    /// </summary>
    public void TryDropPowerUp(Vector3 enemyPosition)
    {
        if (availablePowerUps == null || availablePowerUps.Length == 0) return;

        float roll = Random.value;
        if (roll <= dropChance)
        {
            int randomIndex = Random.Range(0, availablePowerUps.Length);
            PowerUpDataSO selectedPowerUp = availablePowerUps[randomIndex];

            if (selectedPowerUp != null)
            {
                OtorgarPowerUpAManoIzquierda(selectedPowerUp);
            }
        }
    }

    /// <summary>
    /// Coloca el Power-Up en la mano o control izquierdo del jugador.
    /// </summary>
    public void OtorgarPowerUpAManoIzquierda(PowerUpDataSO powerUp)
    {
        if (LeftHandPowerUpController.Instance != null)
        {
            LeftHandPowerUpController.Instance.EquiparPowerUp(powerUp);
        }
        else
        {
            // Si el controlador izquierdo aún no se ha inicializado, buscarlo o crearlo
            var leftCtrl = Object.FindAnyObjectByType<LeftHandPowerUpController>();
            if (leftCtrl != null)
            {
                leftCtrl.EquiparPowerUp(powerUp);
            }
            else
            {
                Debug.Log($"[PowerUpManager] 🎁 ¡Power-Up obtenido: {powerUp.powerUpName}! (Activado directamente)");
                ActivarPowerUp(powerUp);
            }
        }
    }

    /// <summary>
    /// Aplica los efectos del Power-Up cuando el jugador lo presiona.
    /// </summary>
    public void ActivarPowerUp(PowerUpDataSO powerUp)
    {
        if (powerUp == null) return;

        Debug.Log($"[PowerUpManager] ⚡ ¡ACTIVANDO POWER-UP: {powerUp.powerUpName} ({powerUp.powerUpType})!");

        // Reproducir sonido de activación
        AudioClip clip = powerUp.customActivationSound != null ? powerUp.customActivationSound : SonidosArmas.SonidoPowerUpActivar;
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip, 0.9f);
        }

        switch (powerUp.powerUpType)
        {
            case PowerUpType.Escudo:
                ActivarEscudo();
                break;

            case PowerUpType.Ralentizar:
                ActivarRalentizarPajaros(powerUp.duration > 0 ? powerUp.duration : 5f, powerUp.slowSpeedFactor);
                break;

            case PowerUpType.DoblePuntos:
                ActivarDoblePuntos(powerUp.duration > 0 ? powerUp.duration : 10f);
                break;
        }

        if (PlayerHUD.Instance != null)
        {
            PlayerHUD.Instance.MostrarNotificacionPowerUp(powerUp);
        }
    }

    private void ActivarEscudo()
    {
        HasShield = true;
        CrearEfectoVisualEscudo();
    }

    private void CrearEfectoVisualEscudo()
    {
        if (shieldVisualDome != null) Destroy(shieldVisualDome);

        Camera cam = Camera.main;
        if (cam == null) return;

        shieldVisualDome = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        shieldVisualDome.name = "ShieldVisualDome";
        shieldVisualDome.transform.SetParent(cam.transform, false);
        shieldVisualDome.transform.localPosition = new Vector3(0, 0, 0.6f);
        shieldVisualDome.transform.localScale = Vector3.one * 1.8f;

        // Quitar collider para no interferir
        Collider col = shieldVisualDome.GetComponent<Collider>();
        if (col != null) Destroy(col);

        Shader shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color");
        Material mat = new Material(shader);
        Color shieldCol = new Color(0.2f, 0.8f, 1f, 0.22f);
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", shieldCol);
        else mat.color = shieldCol;

        var rend = shieldVisualDome.GetComponent<MeshRenderer>();
        if (rend != null) rend.material = mat;
    }

    /// <summary>
    /// Si el jugador tiene escudo, absorbe el daño y retorna true.
    /// </summary>
    public bool IntentarConsumirEscudo()
    {
        if (HasShield)
        {
            HasShield = false;
            if (shieldVisualDome != null)
            {
                Destroy(shieldVisualDome);
            }

            if (audioSource != null)
            {
                audioSource.PlayOneShot(SonidosArmas.SonidoEscudoBloqueo, 1.0f);
            }

            Debug.Log("[PowerUpManager] 🛡️ ¡EL ESCUDO ABSORBIÓ EL IMPACTO POR COMPLETO!");

            if (PlayerHUD.Instance != null)
            {
                PlayerHUD.Instance.ActualizarEstadoPowerUps();
            }

            return true;
        }
        return false;
    }

    private void ActivarRalentizarPajaros(float duracion, float slowFactor)
    {
        if (slowMotionCoroutine != null)
        {
            StopCoroutine(slowMotionCoroutine);
        }
        slowMotionCoroutine = StartCoroutine(RutinaRalentizarPajaros(duracion, slowFactor));
    }

    private IEnumerator RutinaRalentizarPajaros(float duracion, float slowFactor)
    {
        IsSlowMotionActive = true;
        CurrentSlowFactor = slowFactor > 0 ? slowFactor : 0.35f;
        SlowMotionTimeRemaining = duracion;

        if (audioSource != null)
        {
            audioSource.PlayOneShot(SonidosArmas.SonidoSlowMotion, 0.85f);
        }

        while (SlowMotionTimeRemaining > 0)
        {
            SlowMotionTimeRemaining -= Time.deltaTime;
            yield return null;
        }

        IsSlowMotionActive = false;
        SlowMotionTimeRemaining = 0f;

        if (PlayerHUD.Instance != null)
        {
            PlayerHUD.Instance.ActualizarEstadoPowerUps();
        }
    }

    private void ActivarDoblePuntos(float duracion)
    {
        if (doubleScoreCoroutine != null)
        {
            StopCoroutine(doubleScoreCoroutine);
        }
        doubleScoreCoroutine = StartCoroutine(RutinaDoblePuntos(duracion));
    }

    private IEnumerator RutinaDoblePuntos(float duracion)
    {
        IsDoubleScoreActive = true;
        DoubleScoreTimeRemaining = duracion;

        if (audioSource != null)
        {
            audioSource.PlayOneShot(SonidosArmas.SonidoDoblePuntos, 0.85f);
        }

        while (DoubleScoreTimeRemaining > 0)
        {
            DoubleScoreTimeRemaining -= Time.deltaTime;
            yield return null;
        }

        IsDoubleScoreActive = false;
        DoubleScoreTimeRemaining = 0f;

        if (PlayerHUD.Instance != null)
        {
            PlayerHUD.Instance.ActualizarEstadoPowerUps();
        }
    }
}
