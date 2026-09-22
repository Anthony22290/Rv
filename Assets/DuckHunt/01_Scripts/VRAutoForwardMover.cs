using UnityEngine;

/// <summary>
/// Mueve al jugador (XR Origin) constantemente hacia adelante a lo largo del eje Z.
/// Diseñado para mecánicas de shooter sobre rieles en VR.
/// </summary>
public class VRAutoForwardMover : MonoBehaviour
{
    [Header("Configuracion de Movimiento")]
    [Tooltip("Velocidad de avance en metros por segundo")]
    public float velocidad = 2.5f;

    [Tooltip("Permite pausar o reanudar el avance")]
    public bool avanzar = true;

    [Tooltip("Direccion del movimiento (por defecto hacia adelante en Z)")]
    public Vector3 direccion = Vector3.forward;

    [Header("Limites del Recorrido")]
    [Tooltip("Distancia maxima antes de reiniciar o detenerse")]
    public float distanciaMaximaZ = 300f;

    [Tooltip("Si es true, reinicia al inicio al llegar al final")]
    public bool reiniciarAlFinal = true;

    private Vector3 posicionInicial;

    void Start()
    {
        posicionInicial = transform.position;
    }

    void Update()
    {
        if (!avanzar) return;

        // Desplazamiento continuo y suave
        transform.position += direccion.normalized * velocidad * Time.deltaTime;

        // Comprobar limite de recorrido
        if (transform.position.z >= posicionInicial.z + distanciaMaximaZ)
        {
            if (reiniciarAlFinal)
            {
                transform.position = posicionInicial;
            }
            else
            {
                avanzar = false;
            }
        }
    }
}
