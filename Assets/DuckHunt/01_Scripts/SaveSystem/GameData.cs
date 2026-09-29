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
    public Vector3 playerPosition;
    public Quaternion playerRotation;
    public int playerHealth;
    public int score;

    /// <summary>
    /// Constructor por defecto (Nueva partida).
    /// </summary>
    public GameData()
    {
        saveTimestamp = string.Empty;
        currentSceneName = "MapaDesierto";
        playerPosition = Vector3.zero;
        playerRotation = Quaternion.identity;
        playerHealth = 3;
        score = 0;
        isValid = false;
    }
}
