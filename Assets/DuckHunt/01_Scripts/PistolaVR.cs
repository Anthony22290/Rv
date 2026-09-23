using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[RequireComponent(typeof(XRGrabInteractable))]
public class PistolaVR : MonoBehaviour
{
    [Header("Puntos de Referencia")]
    [Tooltip("Punta del cañón de la pistola desde donde sale la bala y el láser")]
    public Transform puntoDeDisparo;

    [Tooltip("Prefab del proyectil / bala")]
    public GameObject balaPrefab;

    [Header("Puntero Láser de Apuntado")]
    [Tooltip("Muestra una línea láser roja hacia donde apunta la pistola")]
    public bool usarLaserApuntado = true;
    public float distanciaLaser = 35f;
    public Color colorLaser = new Color(1f, 0.1f, 0.1f, 0.8f);

    [Header("Feedback Háptico (Vibración Oculus)")]
    public float duracionVibracion = 0.1f;
    public float intensidadVibracion = 0.7f;

    [Header("Efectos")]
    public AudioClip sonidoDisparo;
    private AudioSource audioSource;

    private XRGrabInteractable interactable;
    private LineRenderer laserLine;

    void Awake()
    {
        interactable = GetComponent<XRGrabInteractable>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        interactable.activated.AddListener(Disparar);

        // Crear y configurar el puntero láser visual
        if (usarLaserApuntado)
        {
            ConfigurarLaser();
        }
    }

    void OnDestroy()
    {
        if (interactable != null)
        {
            interactable.activated.RemoveListener(Disparar);
        }
    }

    void ConfigurarLaser()
    {
        GameObject laserObj = new GameObject("LaserSight");
        laserObj.transform.SetParent(puntoDeDisparo != null ? puntoDeDisparo : transform, false);

        laserLine = laserObj.AddComponent<LineRenderer>();
        laserLine.startWidth = 0.008f;
        laserLine.endWidth = 0.004f;
        laserLine.positionCount = 2;
        laserLine.useWorldSpace = true;

        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null) shader = Shader.Find("Unlit/Color");
        Material laserMat = new Material(shader);
        laserMat.SetColor("_BaseColor", colorLaser);
        laserLine.material = laserMat;
    }

    void Update()
    {
        if (laserLine != null && puntoDeDisparo != null)
        {
            // Solo mostrar el láser si la pistola está siendo sostenida
            bool estaAgarrada = interactable != null && interactable.isSelected;
            laserLine.enabled = estaAgarrada;

            if (estaAgarrada)
            {
                Vector3 origin = puntoDeDisparo.position;
                Vector3 direction = puntoDeDisparo.forward;
                Vector3 targetPoint = origin + (direction * distanciaLaser);

                // Si choca con un enemigo u obstáculo, cortar el láser ahí
                if (Physics.Raycast(origin, direction, out RaycastHit hit, distanciaLaser))
                {
                    targetPoint = hit.point;
                }

                laserLine.SetPosition(0, origin);
                laserLine.SetPosition(1, targetPoint);
            }
        }
    }

    void Disparar(ActivateEventArgs arg)
    {
        Transform originTransform = (puntoDeDisparo != null) ? puntoDeDisparo : transform;

        // 1. Instanciar la bala en la posición y orientación exacta del cañón
        if (balaPrefab != null)
        {
            Instantiate(balaPrefab, originTransform.position, originTransform.rotation);
        }

        // 2. Feedback Sonoro
        if (sonidoDisparo != null && audioSource != null)
        {
            audioSource.PlayOneShot(sonidoDisparo);
        }

        // 3. Feedback Háptico en el mando de Oculus Quest
        if (arg.interactorObject is XRBaseInputInteractor inputInteractor)
        {
            inputInteractor.SendHapticImpulse(intensidadVibracion, duracionVibracion);
        }
    }
}