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
                                ?? other.GetComponent<VRAutoForwardMover>();

        if (mover == null)
        {
            var rig = other.GetComponentInParent<Unity.XR.CoreUtils.XROrigin>();
            if (rig != null) mover = rig.GetComponent<VRAutoForwardMover>();
        }

        if (mover != null)
        {
            metaCruzada = true;
            Debug.Log("[FinishLineTrigger] El jugador ha cruzado la linea de meta.");
            mover.CompletarNivel();
        }
        else
        {
            // Si el jugador esta a menos de 5m de la meta
            var playerMover = Object.FindAnyObjectByType<VRAutoForwardMover>();
            if (playerMover != null && Vector3.Distance(playerMover.transform.position, transform.position) < 5f)
            {
                metaCruzada = true;
                Debug.Log("[FinishLineTrigger] El jugador ha cruzado la linea de meta.");
                playerMover.CompletarNivel();
            }
        }
    }
}
