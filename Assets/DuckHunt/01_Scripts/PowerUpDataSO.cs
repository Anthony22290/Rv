using UnityEngine;

public enum PowerUpType
{
    Escudo,
    Ralentizar,
    DoblePuntos
}

[CreateAssetMenu(fileName = "NewPowerUp", menuName = "DuckHunt/PowerUp Data", order = 2)]
public class PowerUpDataSO : ScriptableObject
{
    [Header("Identificación")]
    public string powerUpName = "Escudo Defensivo";
    public PowerUpType powerUpType = PowerUpType.Escudo;
    public string iconEmoji = "🛡️";

    [TextArea(2, 4)]
    public string description = "Protege contra el siguiente golpe o impacto recibido.";

    [Header("Configuración y Duración")]
    [Tooltip("Duración del efecto en segundos (5s para tiempo lento, 10s para doble puntos, etc.).")]
    public float duration = 5f;

    [Tooltip("Factor multiplicador de puntos (para DoblePuntos).")]
    public int scoreMultiplier = 2;

    [Tooltip("Factor de velocidad de los patos (para Ralentizar, ej: 0.35 para 35% de velocidad).")]
    public float slowSpeedFactor = 0.35f;

    [Header("Aspecto Visual en Mano Izquierda")]
    public Color themeColor = new Color(0.2f, 0.75f, 1f, 1f);
    public Color emissionColor = new Color(0.1f, 0.5f, 1f, 1f);

    [Header("Sonidos")]
    public AudioClip customActivationSound;
}
