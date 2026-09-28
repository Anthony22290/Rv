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

    private Vector3 ultimaPosicion;
    private float tiempoAgitacion;
    private int contadorAgitacion;
    public float umbralAgitacion = 0.015f;

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
        
        ActualizarTextoMunicion();
        ultimaPosicion = transform.position;

        if (textoMunicion != null)
        {
            reloadIconObj = new GameObject("ReloadIcon");
            reloadIconObj.transform.SetParent(textoMunicion.transform.parent);
            reloadIconObj.transform.localPosition = textoMunicion.transform.localPosition + new Vector3(0, 0.05f, 0);
            reloadIconObj.transform.localRotation = textoMunicion.transform.localRotation;
            
            float scaleInvert = 1f / transform.localScale.x;
            reloadIconObj.transform.localScale = new Vector3(scaleInvert * 0.05f, scaleInvert * 0.05f, scaleInvert * 0.05f);

            SpriteRenderer sr = reloadIconObj.AddComponent<SpriteRenderer>();
            
#if UNITY_EDITOR
            if (reloadSprite == null)
            {
                reloadSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/DuckHunt/02_Sprites/reload.png");
            }
#endif
            if (reloadSprite != null) sr.sprite = reloadSprite;
            else Debug.LogError("PistolaVR: No se pudo cargar reloadSprite. Verifica que la ruta Assets/DuckHunt/02_Sprites/reload.png sea correcta.");
            
            reloadIconObj.SetActive(false);
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
        bool estaAgarrada = interactable != null && interactable.isSelected;

        if (laserLine != null && puntoDeDisparo != null)
        {
            laserLine.enabled = estaAgarrada;

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
            }
        }

        if (estaAgarrada && !estaRecargando)
        {
            float deltaY = Mathf.Abs(transform.position.y - ultimaPosicion.y);
            if (deltaY > 0.008f)
            {
                contadorAgitacion++;
                tiempoAgitacion = Time.time;
            }

            if (Time.time - tiempoAgitacion > 0.4f)
            {
                contadorAgitacion = 0;
            }

            if (contadorAgitacion > 3)
            {
                if (datosArma != null && balasEnCargador < datosArma.capacidadCargador)
                {
                    StartCoroutine(RutinaRecarga());
                }
                contadorAgitacion = 0;
            }
        }
        
        if (estaRecargando && reloadIconObj != null)
        {
            reloadIconObj.transform.Rotate(0, 0, -360f * Time.deltaTime);
        }
        
        ultimaPosicion = transform.position;
    }

    void Disparar(ActivateEventArgs arg)
    {
        if (estaRecargando) return;

        if (datosArma != null)
        {
            if (Time.time - tiempoUltimoDisparo < datosArma.tiempoDisparo) return;
            if (balasEnCargador <= 0) return;

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

        if (sonidoDisparo != null && audioSource != null)
        {
            audioSource.PlayOneShot(sonidoDisparo);
        }

        if (arg.interactorObject is XRBaseInputInteractor inputInteractor)
        {
            inputInteractor.SendHapticImpulse(intensidadVibracion, duracionVibracion);
        }
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

    void ActualizarTextoMunicion()
    {
        if (textoMunicion != null && datosArma != null)
        {
            textoMunicion.text = balasEnCargador.ToString() + "/" + datosArma.capacidadCargador.ToString();
        }
    }
}
 

