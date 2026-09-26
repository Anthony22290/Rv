using UnityEngine;

/// <summary>
/// Trigger físico para la línea de meta al final de cada mapa.
/// Al cruzar la línea, pasa de mapa o finaliza el juego automáticamente.
/// </summary>
[RequireComponent(typeof(Collider))]
public class FinishLineTrigger : MonoBehaviour
{
    private bool metaCruzada = false;

    void Awake()
    {
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (metaCruzada) return;

        // Detectar al jugador (XR Origin, Cámara o Collider del jugador)
        VRAutoForwardMover mover = other.GetComponentInParent<VRAutoForwardMover>() 
                                ?? other.GetComponent<VRAutoForwardMover>()
                                ?? Object.FindAnyObjectByType<VRAutoForwardMover>();

        if (mover != null)
        {
            metaCruzada = true;
            Debug.Log("[FinishLineTrigger] 🏁 ¡El jugador ha cruzado la línea de meta!");
            mover.CompletarNivel();
        }
    }
}
