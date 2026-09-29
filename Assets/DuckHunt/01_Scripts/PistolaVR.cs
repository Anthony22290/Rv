using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using TMPro;
using System.Collections;

[RequireComponent(typeof(XRGrabInteractable))]
public class PistolaVR : MonoBehaviour
{
    [Header("Datos del Arma")]
    public ArmaDataSO datosArma;

    [Header("Puntos de Referencia")]
    public Transform puntoDeDisparo;
    public TextMeshPro textoMunicion;
    public GameObject balaPrefab;

    [Header("Puntero Laser")]
    public bool usarLaserApuntado = true;
    public float distanciaLaser = 35f;
    public Color colorLaser = new Color(1f, 0.1f, 0.1f, 0.8f);

    [Header("Feedback Haptico")]
    public float duracionVibracion = 0.1f;
    public float intensidadVibracion = 0.7f;

    [Header("Efectos")]
    public AudioClip sonidoDisparo;
    private AudioSource audioSource;

    [Header("UI de Recarga")]
    public Sprite reloadSprite;
    private GameObject reloadIconObj;

    private XRGrabInteractable interactable;
    private LineRenderer laserLine;

    private float tiempoUltimoDisparo;
    private int balasEnCargador;
    private bool estaRecargando;

    // Shake detection
    private Vector3 lastPos;
    private float shakeTimer = 0f;
    private int shakeCount = 0;
    private float lastMoveDirection = 0f;

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

        if (usarLaserApuntado) ConfigurarLaser();

        if (datosArma != null) balasEnCargador = datosArma.capacidadCargador;
        
        // Destruir basura antigua
        foreach (Transform t in GetComponentsInChildren<Transform>(true))
        {
            if (t != transform && (t.name == "TextoMunicion" || t.name == "CanvasMunicion" || t.name == "ReloadIcon"))
            {
                Destroy(t.gameObject);
            }
        }

        // Crear Holograma Dinamico a prueba de tontos
        CrearHologramaMunicion();
        
        ActualizarTextoMunicion();
        lastPos = transform.position;
    }

    void CrearHologramaMunicion()
    {
        GameObject holoObj = new GameObject("TextoMunicion");
        holoObj.transform.SetParent(transform);
        
        Transform attachPoint = interactable.attachTransform != null ? interactable.attachTransform : transform;
        Vector3 forwardDir = transform.forward;
        Vector3 upDir = transform.up;

        if (puntoDeDisparo != null)
        {
            forwardDir = puntoDeDisparo.forward;
            upDir = puntoDeDisparo.up;
        }

        // Arriba y un poco adelante de la mano
        holoObj.transform.position = attachPoint.position + (upDir * 0.08f) + (forwardDir * 0.06f);
        
        // Mirar hacia el jugador
        holoObj.transform.rotation = Quaternion.LookRotation(forwardDir, upDir);
        holoObj.transform.Rotate(-30f, 0f, 0f, Space.Self);

        textoMunicion = holoObj.AddComponent<TextMeshPro>();
        textoMunicion.text = "12/30";
        textoMunicion.fontSize = 2f;
        textoMunicion.alignment = TextAlignmentOptions.Center;
        textoMunicion.color = Color.white;
        textoMunicion.fontStyle = FontStyles.Bold;

        // Ajustar escala para que mida 4cm reales
        Vector3 parentScale = transform.lossyScale;
        holoObj.transform.localScale = new Vector3(0.04f / parentScale.x, 0.04f / parentScale.y, 0.04f / parentScale.z);

        // --- ICONO DE RECARGA ---
        reloadIconObj = new GameObject("ReloadIcon");
        reloadIconObj.transform.SetParent(transform);
        reloadIconObj.transform.position = holoObj.transform.position + holoObj.transform.forward * 0.01f;
        reloadIconObj.transform.rotation = holoObj.transform.rotation;
        
        SpriteRenderer sr = reloadIconObj.AddComponent<SpriteRenderer>();
#if UNITY_EDITOR
        if (reloadSprite == null) reloadSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/DuckHunt/02_Sprites/reload.png");
#endif
        if (reloadSprite != null) sr.sprite = reloadSprite;

        // Escala absoluta de 5cm
        reloadIconObj.transform.localScale = new Vector3(0.025f / parentScale.x, 0.025f / parentScale.y, 0.025f / parentScale.z);
        reloadIconObj.SetActive(false);
    }

    void OnDestroy()
    {
        if (interactable != null) interactable.activated.RemoveListener(Disparar);
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
        Shader shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color");
        Material laserMat = new Material(shader);
        if (shader.name.Contains("Universal")) laserMat.SetColor("_BaseColor", colorLaser);
        else laserMat.color = colorLaser;
        laserLine.material = laserMat;
    }

    void Update()
    {
        bool estaAgarrada = interactable != null && interactable.isSelected;

        if (laserLine != null && puntoDeDisparo != null)
        {
            laserLine.enabled = estaAgarrada;
            if (estaAgarrada)
            {
                Vector3 origin = puntoDeDisparo.position;
                Vector3 direction = puntoDeDisparo.forward;
                Vector3 targetPoint = origin + (direction * distanciaLaser);
                if (Physics.Raycast(origin, direction, out RaycastHit hit, distanciaLaser)) targetPoint = hit.point;
                laserLine.SetPosition(0, origin);
                laserLine.SetPosition(1, targetPoint);
            }
        }

        // DETECCION DE AGITACION ROBUSTA (Arriba y Abajo)
        if (estaAgarrada && !estaRecargando)
        {
            float moveY = transform.position.y - lastPos.y;
            
            // Si el movimiento es notable (> 5mm por frame)
            if (Mathf.Abs(moveY) > 0.005f)
            {
                float currentDir = Mathf.Sign(moveY);
                
                // Si cambiamos de direccion bruscamente (ej: subiamos y ahora bajamos)
                if (currentDir != lastMoveDirection && lastMoveDirection != 0)
                {
                    shakeCount++;
                    shakeTimer = 0f; // reset timer
                }
                lastMoveDirection = currentDir;
            }

            shakeTimer += Time.deltaTime;
            
            // Si pasa mucho tiempo sin agitar, reiniciamos
            if (shakeTimer > 0.5f)
            {
                shakeCount = 0;
                lastMoveDirection = 0f;
            }

            // Con 3 cambios de direccion (ej: Arriba -> Abajo -> Arriba) recarga
            if (shakeCount >= 3)
            {
                if (datosArma != null && balasEnCargador < datosArma.capacidadCargador)
                {
                    StartCoroutine(RutinaRecarga());
                }
                shakeCount = 0;
            }
        }
        
        if (estaRecargando && reloadIconObj != null)
        {
            reloadIconObj.transform.Rotate(0, 0, -500f * Time.deltaTime, Space.Self);
        }
        
        lastPos = transform.position;
    }

    void Disparar(ActivateEventArgs arg)
    {
        if (estaRecargando) return;

        if (datosArma != null)
        {
            if (Time.time - tiempoUltimoDisparo < datosArma.tiempoDisparo) return;
            if (balasEnCargador <= 0)
            {
                StartCoroutine(RutinaRecarga());
                return;
            }

            balasEnCargador--;
            ActualizarTextoMunicion();
            tiempoUltimoDisparo = Time.time;
        }

        Transform originTransform = (puntoDeDisparo != null) ? puntoDeDisparo : transform;

        if (balaPrefab != null)
        {
            GameObject nuevaBala = Instantiate(balaPrefab, originTransform.position, originTransform.rotation);
            if (datosArma != null)
            {
                Bala scriptBala = nuevaBala.GetComponent<Bala>();
                if (scriptBala != null) scriptBala.velocidad = datosArma.velocidadBala;
            }
        }

        if (sonidoDisparo != null && audioSource != null) audioSource.PlayOneShot(sonidoDisparo);
        if (arg.interactorObject is XRBaseInputInteractor inputInteractor) inputInteractor.SendHapticImpulse(intensidadVibracion, duracionVibracion);
    }

    IEnumerator RutinaRecarga()
    {
        estaRecargando = true;
        if (textoMunicion != null) textoMunicion.text = "";
        if (reloadIconObj != null) reloadIconObj.SetActive(true);
        
        yield return new WaitForSeconds(datosArma != null ? datosArma.tiempoRecarga : 1.5f);
        
        if (datosArma != null)
        {
            balasEnCargador = datosArma.capacidadCargador;
            ActualizarTextoMunicion();
        }
        
        if (reloadIconObj != null) reloadIconObj.SetActive(false);
        estaRecargando = false;
    }

    public void ActualizarTextoMunicion()
    {
        if (textoMunicion != null && datosArma != null)
        {
            textoMunicion.text = balasEnCargador.ToString() + "/" + datosArma.capacidadCargador.ToString();
        }
    }
}
 

 

 

 

 

 

 
