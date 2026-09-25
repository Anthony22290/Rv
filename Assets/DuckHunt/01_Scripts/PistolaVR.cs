using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using TMPro;
using System.Collections;

[RequireComponent(typeof(XRGrabInteractable))]
public class PistolaVR : MonoBehaviour
{
    [Header("Datos del Arma (Scriptable Object)")]
    public ArmaDataSO datosArma;

    [Header("Puntos de Referencia")]
    [Tooltip("Punta del cañón de la pistola desde donde sale la bala y el láser")]
    public Transform puntoDeDisparo;
    
    [Tooltip("Texto flotante donde se muestra la munición")]
    public TextMeshPro textoMunicion;

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

    // Control de disparo y recarga
    private float tiempoUltimoDisparo;
    private int balasEnCargador;
    private bool estaRecargando;

    // Detección de agitación (Shake to reload)
    private Vector3 ultimaPosicion;
    private float tiempoAgitacion;
    private int contadorAgitacion;
    public float umbralAgitacion = 0.05f;

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

        // Inicializar munición
        if (datosArma != null)
        {
            balasEnCargador = datosArma.capacidadCargador;
        }
        
        ActualizarTextoMunicion();
        ultimaPosicion = transform.position;
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
        bool estaAgarrada = interactable != null && interactable.isSelected;

        if (laserLine != null && puntoDeDisparo != null)
        {
            // Solo mostrar el láser si la pistola está siendo sostenida
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

        // Lógica de Agitar para Recargar
        if (estaAgarrada && !estaRecargando)
        {
            float deltaMovimiento = Vector3.Distance(transform.position, ultimaPosicion);
            // Reducido significativamente para que detecte mejor en cada frame
            if (deltaMovimiento > 0.005f) 
            {
                contadorAgitacion++;
                tiempoAgitacion = Time.time;
            }

            // Si no se ha movido mucho en 0.5s, reiniciar contador
            if (Time.time - tiempoAgitacion > 0.4f)
            {
                contadorAgitacion = 0;
            }

            // Reducido a 4 movimientos bruscos para que sea más fácil
            if (contadorAgitacion > 4)
            {
                if (datosArma != null && balasEnCargador < datosArma.capacidadCargador)
                {
                    StartCoroutine(RutinaRecarga());
                }
                contadorAgitacion = 0;
            }
        }
        
        ultimaPosicion = transform.position;
    }

    void Disparar(ActivateEventArgs arg)
    {
        if (estaRecargando) return;

        if (datosArma != null)
        {
            if (Time.time - tiempoUltimoDisparo < datosArma.tiempoDisparo)
            {
                return; // Cadencia de tiro (aún no puede disparar)
            }

            if (balasEnCargador <= 0)
            {
                // No dispara si no hay balas. Ahora la recarga es agitando.
                // Podríamos emitir un sonido de "clic" de cargador vacío aquí
                return;
            }

            balasEnCargador--;
            ActualizarTextoMunicion();
            tiempoUltimoDisparo = Time.time;
        }

        Transform originTransform = (puntoDeDisparo != null) ? puntoDeDisparo : transform;

        // 1. Instanciar la bala en la posición y orientación exacta del cañón
        if (balaPrefab != null)
        {
            GameObject nuevaBala = Instantiate(balaPrefab, originTransform.position, originTransform.rotation);
            if (datosArma != null)
            {
                Bala scriptBala = nuevaBala.GetComponent<Bala>();
                if (scriptBala != null)
                {
                    scriptBala.velocidad = datosArma.velocidadBala;
                }
            }
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

    IEnumerator RutinaRecarga()
    {
        estaRecargando = true;
        if (textoMunicion != null) textoMunicion.text = "R";
        
        // Se puede añadir sonido de recarga aquí
        yield return new WaitForSeconds(datosArma != null ? datosArma.tiempoRecarga : 1.5f);
        
        if (datosArma != null)
        {
            balasEnCargador = datosArma.capacidadCargador;
            ActualizarTextoMunicion();
        }
        estaRecargando = false;
    }

    void ActualizarTextoMunicion()
    {
        if (textoMunicion != null && datosArma != null)
        {
            textoMunicion.text = balasEnCargador.ToString() + "/" + datosArma.capacidadCargador.ToString();
        }
    }
}