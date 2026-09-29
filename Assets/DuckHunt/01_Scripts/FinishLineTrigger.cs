using UnityEngine;

/// <summary>
/// Trigger fisico para la linea de meta al final de cada mapa.
/// Al cruzar la linea, pasa de mapa o finaliza el juego automaticamente.
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

        VRAutoForwardMover mover = other.GetComponentInParent<VRAutoForwardMover>() 
                                ?? other.GetComponent<VRAutoForwardMover>()
                                ?? Object.FindAnyObjectByType<VRAutoForwardMover>();

        if (mover != null)
        {
            metaCruzada = true;
            Debug.Log("[FinishLineTrigger] El jugador ha cruzado la linea de meta.");
            mover.CompletarNivel();
        }
    }
}
