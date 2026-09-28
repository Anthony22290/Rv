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

    [Header("Movimiento")]
    public float moveSpeed = 5f;
    public EnemyMovementPattern movementPattern = EnemyMovementPattern.SineWave;
    public float waveAmplitude = 1.2f;
    public float waveFrequency = 3f;

    [Header("Aspecto Visual y Skin")]
    [Tooltip("Color.white mantiene la textura original intacta. Otros colores tintan el modelo.")]
    public Color bodyColor = Color.white;
    
    [Tooltip("Material que se aplicará al pato (opcional).")]
    public Material duckMaterial;
    
    public Vector3 scale = new Vector3(0.7f, 0.7f, 0.7f);

    [Header("Ciclo de Vida")]
    public float maxLifeTime = 7f; // Tiempo antes de escapar si no le disparan

    [Header("Comportamiento")]
    [Tooltip("Si es verdadero, el pato volará directamente hacia el jugador para atacarlo.")]
    public bool isAggressive = false;
}
