using UnityEngine;

[CreateAssetMenu(fileName = "NuevaArmaData", menuName = "DuckHunt/Arma Data")]
public class ArmaDataSO : ScriptableObject
{
    [Header("Configuracion de Disparo")]
    [Tooltip("Velocidad de la bala al ser disparada")]
    public float velocidadBala = 45f;

    [Tooltip("Tiempo mínimo entre cada disparo (Cadencia)")]
    public float tiempoDisparo = 0.2f;

    [Tooltip("Tiempo que tarda en recargar")]
    public float tiempoRecarga = 1.5f;

    [Tooltip("Cantidad de balas en el cargador antes de recargar")]
    public int capacidadCargador = 10;
}
