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
    public float distanciaLaser = 45f;
    public Color colorLaser = new Color(1.0f, 0.05f, 0.05f, 0.95f);

    [Header("Feedback Haptico")]
    public float duracionVibracion = 0.08f;
    public float intensidadVibracion = 0.6f;

    [Header("Efectos")]
    public AudioClip sonidoDisparo;
    public AudioClip sonidoRecarga;
    private AudioSource audioSource;

    [Header("UI de Recarga")]
    public Sprite reloadSprite;
    private GameObject reloadIconObj;

    private XRGrabInteractable interactable;
    private LineRenderer laserLine;
    private GameObject laserDot;

    private float tiempoUltimoDisparo;
    private int balasEnCargador;
    private bool estaRecargando = false;

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
        interactable.selectEntered.AddListener(AlAgarrar);

        AsegurarAlineacionPuntoDisparo();

        if (usarLaserApuntado) ConfigurarLaser();

        if (datosArma != null) balasEnCargador = datosArma.capacidadCargador;
        else balasEnCargador = 12;

        CrearHologramaMunicion();
        ActualizarTextoMunicion();
        lastPos = transform.position;
    }

    private void AsegurarAlineacionPuntoDisparo()
    {
        if (puntoDeDisparo == null)
        {
            puntoDeDisparo = transform.Find("FirePoint");
        }

        if (puntoDeDisparo == null)
        {
            GameObject fp = new GameObject("FirePoint");
            fp.transform.SetParent(transform, false);
            puntoDeDisparo = fp.transform;
        }

        string gunName = gameObject.name.ToLower();
        if (gunName.Contains("pistola"))
        {
            puntoDeDisparo.localPosition = new Vector3(0.52f, 0.60f, 0.00f);
            puntoDeDisparo.localEulerAngles = new Vector3(0f, 90f, 0f);
        }
        else if (gunName.Contains("escopeta"))
        {
            puntoDeDisparo.localPosition = new Vector3(-0.52f, 0.68f, 0.00f);
            puntoDeDisparo.localEulerAngles = new Vector3(0f, -90f, 0f);
        }
        else
        {
            puntoDeDisparo.localPosition = new Vector3(0.00f, 0.20f, 0.52f);
            puntoDeDisparo.localEulerAngles = new Vector3(0f, 0f, 0f);
        }
    }

    void AlAgarrar(SelectEnterEventArgs args)
    {
        if (balasEnCargador <= 0 && !estaRecargando)
        {
            StartCoroutine(RutinaRecarga());
        }
    }

    void CrearHologramaMunicion()
    {
        Transform viejoHolo = transform.Find("TextoMunicion_Holo");
        if (viejoHolo != null) Destroy(viejoHolo.gameObject);

        GameObject holoObj = new GameObject("TextoMunicion_Holo");
        holoObj.transform.SetParent(transform, false);
        
        Transform attachPoint = interactable.attachTransform != null ? interactable.attachTransform : transform;
        Vector3 forwardDir = (puntoDeDisparo != null) ? puntoDeDisparo.forward : transform.forward;
        Vector3 upDir = (puntoDeDisparo != null) ? puntoDeDisparo.up : transform.up;

        holoObj.transform.position = attachPoint.position + (upDir * 0.07f) - (forwardDir * 0.04f);
        holoObj.transform.rotation = Quaternion.LookRotation(forwardDir, upDir);
        holoObj.transform.Rotate(-35f, 0f, 0f, Space.Self);

        textoMunicion = holoObj.AddComponent<TextMeshPro>();
        textoMunicion.text = "12/12";
        textoMunicion.fontSize = 2.2f;
        textoMunicion.alignment = TextAlignmentOptions.Center;
        textoMunicion.color = Color.white;
        textoMunicion.fontStyle = FontStyles.Bold;

        Vector3 parentScale = transform.lossyScale;
        holoObj.transform.localScale = new Vector3(
            Mathf.Approximately(parentScale.x, 0) ? 0.04f : 0.04f / Mathf.Abs(parentScale.x),
            Mathf.Approximately(parentScale.y, 0) ? 0.04f : 0.04f / Mathf.Abs(parentScale.y),
            Mathf.Approximately(parentScale.z, 0) ? 0.04f : 0.04f / Mathf.Abs(parentScale.z)
        );

        reloadIconObj = new GameObject("ReloadIcon");
        reloadIconObj.transform.SetParent(holoObj.transform, false);
        reloadIconObj.transform.localPosition = new Vector3(0, 0, -0.01f);
        reloadIconObj.transform.localRotation = Quaternion.identity;
        
        SpriteRenderer sr = reloadIconObj.AddComponent<SpriteRenderer>();
#if UNITY_EDITOR
        if (reloadSprite == null) reloadSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/DuckHunt/02_Sprites/reload.png");
#endif
        if (reloadSprite != null) sr.sprite = reloadSprite;
        reloadIconObj.transform.localScale = new Vector3(0.12f, 0.12f, 0.12f);
        reloadIconObj.SetActive(false);
    }

    void OnDestroy()
    {
        if (interactable != null)
        {
            interactable.activated.RemoveListener(Disparar);
            interactable.selectEntered.RemoveListener(AlAgarrar);
        }
    }

    void ConfigurarLaser()
    {
        Transform parentT = (puntoDeDisparo != null) ? puntoDeDisparo : transform;
        Transform viejoLaser = parentT.Find("LaserSight_Dynamic");
        if (viejoLaser != null) Destroy(viejoLaser.gameObject);

        GameObject laserObj = new GameObject("LaserSight_Dynamic");
        laserObj.transform.SetParent(parentT, false);
        laserObj.transform.localPosition = Vector3.zero;
        laserObj.transform.localRotation = Quaternion.identity;

        laserLine = laserObj.AddComponent<LineRenderer>();
        laserLine.startWidth = 0.008f;
        laserLine.endWidth = 0.003f;
        laserLine.positionCount = 2;
        laserLine.useWorldSpace = true;

        Shader shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color");
        Material laserMat = new Material(shader);
        laserMat.SetColor("_BaseColor", colorLaser);
        laserLine.material = laserMat;
        laserLine.enabled = false;

        // Punto de impacto laser (Laser Dot)
        laserDot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        laserDot.name = "LaserDot";
        laserDot.transform.SetParent(laserObj.transform, false);
        laserDot.transform.localScale = Vector3.one * 0.035f;
        
        Collider dotCol = laserDot.GetComponent<Collider>();
        if (dotCol != null) Destroy(dotCol);

        Renderer dotRen = laserDot.GetComponent<Renderer>();
        dotRen.material = laserMat;
        laserDot.SetActive(false);
    }

    void Update()
    {
        bool estaAgarrada = interactable != null && interactable.isSelected;

        if (laserLine != null && puntoDeDisparo != null)
        {
            laserLine.enabled = estaAgarrada;
            if (laserDot != null) laserDot.SetActive(estaAgarrada);

            if (estaAgarrada)
            {
                Vector3 origin = puntoDeDisparo.position;
                Vector3 direction = puntoDeDisparo.forward;
                Vector3 targetPoint = origin + (direction * distanciaLaser);

                if (Physics.Raycast(origin, direction, out RaycastHit hit, distanciaLaser))
                {
                    targetPoint = hit.point;
                }

                laserLine.SetPosition(0, origin);
                laserLine.SetPosition(1, targetPoint);

                if (laserDot != null)
                {
                    laserDot.transform.position = targetPoint;
                }
            }
        }

        // Recarga por agitacion
        if (estaAgarrada && !estaRecargando)
        {
            float moveY = transform.position.y - lastPos.y;
            if (Mathf.Abs(moveY) > 0.006f)
            {
                float currentDir = Mathf.Sign(moveY);
                if (currentDir != lastMoveDirection && lastMoveDirection != 0)
                {
                    shakeCount++;
                    shakeTimer = 0f;
                }
                lastMoveDirection = currentDir;
            }

            shakeTimer += Time.deltaTime;
            if (shakeTimer > 0.45f)
            {
                shakeCount = 0;
                lastMoveDirection = 0f;
            }

            if (shakeCount >= 3)
            {
                int maxCap = (datosArma != null) ? datosArma.capacidadCargador : 12;
                if (balasEnCargador < maxCap)
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

        float cadencia = (datosArma != null) ? datosArma.tiempoDisparo : 0.12f;
        if (Time.time - tiempoUltimoDisparo < cadencia) return;

        if (balasEnCargador <= 0)
        {
            StartCoroutine(RutinaRecarga());
            return;
        }

        balasEnCargador--;
        ActualizarTextoMunicion();
        tiempoUltimoDisparo = Time.time;

        Vector3 spawnOrigin = (puntoDeDisparo != null) ? puntoDeDisparo.position : transform.position;
        Vector3 spawnDirection = (puntoDeDisparo != null) ? puntoDeDisparo.forward : transform.forward;
        Quaternion spawnRotation = Quaternion.LookRotation(spawnDirection);

        if (balaPrefab != null)
        {
            // Instanciar bala con orientación exacta hacia el punto del láser
            GameObject nuevaBala = Instantiate(balaPrefab, spawnOrigin, spawnRotation);
            Bala scriptBala = nuevaBala.GetComponent<Bala>();
            if (scriptBala != null && datosArma != null)
            {
                scriptBala.velocidad = datosArma.velocidadBala;
            }
        }

        AudioClip clipDisparo = (sonidoDisparo != null) ? sonidoDisparo : SonidosArmas.SonidoPistola;
        if (audioSource != null && clipDisparo != null)
        {
            audioSource.PlayOneShot(clipDisparo);
        }

        if (arg.interactorObject is XRBaseInputInteractor inputInteractor)
        {
            inputInteractor.SendHapticImpulse(intensidadVibracion, duracionVibracion);
        }

        if (balasEnCargador <= 0)
        {
            StartCoroutine(RutinaRecarga());
        }
    }

    IEnumerator RutinaRecarga()
    {
        if (estaRecargando) yield break;
        estaRecargando = true;

        AudioClip clipRecarga = (sonidoRecarga != null) ? sonidoRecarga : null;
        if (clipRecarga != null && audioSource != null) audioSource.PlayOneShot(clipRecarga);

        if (textoMunicion != null) textoMunicion.text = "...";
        if (reloadIconObj != null) reloadIconObj.SetActive(true);
        
        float tiempoRecarga = (datosArma != null) ? Mathf.Min(datosArma.tiempoRecarga, 0.8f) : 0.8f;
        yield return new WaitForSeconds(tiempoRecarga);
        
        int maxCap = (datosArma != null) ? datosArma.capacidadCargador : 12;
        balasEnCargador = maxCap;
        ActualizarTextoMunicion();
        
        if (reloadIconObj != null) reloadIconObj.SetActive(false);
        estaRecargando = false;
    }

    public void ActualizarTextoMunicion()
    {
        if (textoMunicion != null)
        {
            int maxCap = (datosArma != null) ? datosArma.capacidadCargador : 12;
            textoMunicion.text = $"{balasEnCargador}/{maxCap}";
        }
    }
}
