using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables; 

[RequireComponent(typeof(XRGrabInteractable))]
public class PistolaVR : MonoBehaviour
{
    public Transform puntoDeDisparo;
    public GameObject balaPrefab; // Aquí arrastraremos tu prefab
    
    private XRGrabInteractable interactable;

    void Awake()
    {
        interactable = GetComponent<XRGrabInteractable>();
        // Conecta el clic izquierdo (Gatillo) a la función de disparar
        interactable.activated.AddListener(Disparar);
    }

    void OnDestroy()
    {
        interactable.activated.RemoveListener(Disparar);
    }

    void Disparar(ActivateEventArgs arg)
    {
        // Clona tu prefab de la bala en la posición y rotación exacta del PuntoDeDisparo
        Instantiate(balaPrefab, puntoDeDisparo.position, puntoDeDisparo.rotation);
    }
}