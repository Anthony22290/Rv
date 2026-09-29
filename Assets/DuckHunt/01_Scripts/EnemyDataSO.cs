using UnityEngine;

public enum EnemyMovementPattern
{
    StraightLine,
    SineWave,
    ArcFly
}

[CreateAssetMenu(fileName = "NewEnemyData", menuName = "DuckHunt/Enemy Data", order = 1)]
public class EnemyDataSO : ScriptableObject
{
    [Header("Identificación")]
    public string enemyName = "Pato Común";
    public int scorePoints = 100;

    [Header("Modelo 3D y Skin (Living Birds)")]
    [Tooltip("Prefab del pájaro animado de Living Birds (ej: lb_cardinalHQ, lb_blueJayHQ, etc.)")]
    public GameObject birdPrefab;

    [Tooltip("Color de tintado opcional (Color.white mantiene textura original)")]
    public Color bodyColor = Color.white;

    [Tooltip("Material personalizado opcional")]
    public Material duckMaterial;

    [Tooltip("Escala del pájaro en el juego")]
    public Vector3 scale = new Vector3(1.4f, 1.4f, 1.4f);

    [Header("Movimiento")]
    public float moveSpeed = 4.5f;
    public EnemyMovementPattern movementPattern = EnemyMovementPattern.SineWave;
    public float waveAmplitude = 1.2f;
    public float waveFrequency = 3f;

    [Header("Sonidos")]
    [Tooltip("Canto o llamada característica del pájaro")]
    public AudioClip birdChirpSound;

    [Tooltip("Sonido de aleteo o vuelo")]
    public AudioClip birdFlySound;

    [Tooltip("Sonido de impacto o graznido")]
    public AudioClip birdHitSound;

    [Header("Ciclo de Vida y Comportamiento")]
    public float maxLifeTime = 8f;

    [Tooltip("Si es verdadero, vuela agresivamente hacia el jugador para atacarlo")]
    public bool isAggressive = false;
}
