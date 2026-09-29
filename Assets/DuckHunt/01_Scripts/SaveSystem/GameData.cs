using System;
using UnityEngine;

/// <summary>
/// Contenedor de datos serializable para almacenar el estado del juego.
/// </summary>
[Serializable]
public class GameData
{
    [Header("Informacion General")]
    public string saveTimestamp;
    public string currentSceneName;
    public bool isValid;

    [Header("Datos del Jugador")]
    [Tooltip("Si es false, el jugador aparece en el punto de inicio del mapa (checkpoint de inicio de nivel)")]
    public bool hasPlayerPosition;
    public Vector3 playerPosition;
    public Quaternion playerRotation;
    [Tooltip("0 o menos = usar la vida inicial del mapa")]
    public int playerHealth;
    public int score;

    /// <summary>
    /// Constructor por defecto (Nueva partida).
    /// </summary>
    public GameData()
    {
        saveTimestamp = string.Empty;
        currentSceneName = "MapaDesierto";
        hasPlayerPosition = false;
        playerPosition = Vector3.zero;
        playerRotation = Quaternion.identity;
        playerHealth = 0;
        score = 0;
        isValid = false;
    }
}
