using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Mueve al jugador (XR Origin) constantemente hacia adelante a lo largo del camino.
/// Gestiona la progresion de niveles (MapaDesierto -> Mapa2) y la pantalla de finalizacion del juego en VR.
/// </summary>
public class VRAutoForwardMover : MonoBehaviour
{
    [Header("Configuracion de Movimiento")]
    [Tooltip("Velocidad de avance en metros por segundo")]
    public float velocidad = 2.8f;

    [Tooltip("Permite pausar o reanudar el avance")]
    public bool avanzar = true;

    [Tooltip("Direccion del movimiento (por defecto hacia adelante en Z)")]
    public Vector3 direccion = Vector3.forward;

    [Header("Limites y Progresion de Escena")]
    [Tooltip("Distancia maxima en metros antes de completar el mapa")]
    public float distanciaMaximaZ = 240f;

    [Tooltip("Indica si este es el mapa final del juego (Mapa2). Si es true, muestra mensaje de victoria y regresa al menu.")]
    public bool esNivelFinal = false;

    [Tooltip("Si es true y no es nivel final ni hay siguiente escena, reinicia posicion")]
    public bool reiniciarAlFinal = false;

    [Tooltip("Nombre de la siguiente escena a cargar si no es el nivel final")]
    public string siguienteEscena = "Mapa2";

    [Tooltip("Nombre de la escena del menu principal")]
    public string menuSceneName = "MainMenu";

    [Tooltip("Segundos de espera mostrando la pantalla de victoria antes de volver al menu")]
    public float tiempoEsperaFinJuego = 5.0f;

    [Tooltip("Segundos de espera en la transicion entre mapas")]
    public float tiempoTransicionEntreMapas = 2.0f;

    private Vector3 posicionInicial;
    private bool nivelCompletado = false;
    private Camera targetCamera;

    void Awake()
    {
        targetCamera = GetComponentInChildren<Camera>();
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }
    }

    void Start()
    {
        posicionInicial = transform.position;

        string activeScene = SceneManager.GetActiveScene().name;
        if (activeScene.Equals("MapaDesierto", System.StringComparison.OrdinalIgnoreCase))
        {
            siguienteEscena = "Mapa2";
            esNivelFinal = false;
        }
        else if (activeScene.Equals("Mapa2", System.StringComparison.OrdinalIgnoreCase))
        {
            siguienteEscena = "Mapa3";
            esNivelFinal = false;
        }
        else if (activeScene.Equals("Mapa3", System.StringComparison.OrdinalIgnoreCase))
        {
            siguienteEscena = "Mapa4";
            esNivelFinal = false;
        }
        else if (activeScene.Equals("Mapa4", System.StringComparison.OrdinalIgnoreCase) || 
                 activeScene.Equals("BasicScene", System.StringComparison.OrdinalIgnoreCase))
        {
            esNivelFinal = true;
            siguienteEscena = "";
        }
        else if (activeScene.Equals("Mapa2", System.StringComparison.OrdinalIgnoreCase))
        {
            siguienteEscena = "Mapa3";
            esNivelFinal = false;
        }
        else if (activeScene.Equals("Mapa3", System.StringComparison.OrdinalIgnoreCase) || 
                 activeScene.Equals("BasicScene", System.StringComparison.OrdinalIgnoreCase))
        {
            esNivelFinal = true;
            siguienteEscena = "";
        }
        else if (activeScene.Equals("Mapa2", System.StringComparison.OrdinalIgnoreCase) || 
                 activeScene.Equals("BasicScene", System.StringComparison.OrdinalIgnoreCase))
        {
            esNivelFinal = true;
            siguienteEscena = "";
        }

        Debug.Log($"[VRAutoForwardMover] Iniciado en '{activeScene}'. Destino final: {(esNivelFinal ? "Fin del Juego -> Menu" : $"Siguiente Mapa -> {siguienteEscena}")}");
    }

    void Update()
    {
        if (!avanzar || nivelCompletado) return;

        transform.position += direccion.normalized * velocidad * Time.deltaTime;

        if (transform.position.z >= posicionInicial.z + distanciaMaximaZ)
        {
            CompletarNivel();
        }
    }

    public void CompletarNivel()
    {
        if (nivelCompletado) return;
        nivelCompletado = true;
        avanzar = false;

        var spawner = Object.FindAnyObjectByType<EnemySpawner>();
        if (spawner != null)
        {
            spawner.StopSpawning();
        }

        var powerSpawner = Object.FindAnyObjectByType<PowerUpSpawner>();
        if (powerSpawner != null)
        {
            powerSpawner.StopSpawning();
        }

        if (esNivelFinal || string.IsNullOrEmpty(siguienteEscena))
        {
            Debug.Log("[VRAutoForwardMover] JUEGO COMPLETADO. Mostrando pantalla final...");
            StartCoroutine(FinDelJuegoRutina());
        }
        else
        {
            Debug.Log($"[VRAutoForwardMover] Mapa completado. Transicionando a: {siguienteEscena}");
            StartCoroutine(TransicionSiguienteNivelRutina());
        }
    }

    private IEnumerator TransicionSiguienteNivelRutina()
    {
        MostrarMensajeVR(SceneManager.GetActiveScene().name.ToUpper() + " COMPLETADO", $"Cargando siguiente nivel: {siguienteEscena}...", new Color(0.2f, 0.8f, 0.3f));

        yield return new WaitForSeconds(tiempoTransicionEntreMapas);

        SceneManager.LoadScene(siguienteEscena);
    }

    private IEnumerator FinDelJuegoRutina()
    {
        // Detener musica de fondo actual
        GameObject bgMusic = GameObject.Find("MusicaDeFondo");
        if (bgMusic != null) Destroy(bgMusic);

        AudioSource audioSrc = gameObject.AddComponent<AudioSource>();
        audioSrc.playOnAwake = false;
        audioSrc.spatialBlend = 0f;
        
        AudioClip victoriaClip = Resources.Load<AudioClip>("Musica/victoria");
        if (victoriaClip != null) audioSrc.PlayOneShot(victoriaClip, 0.9f);
        else audioSrc.PlayOneShot(SonidosArmas.SonidoVictoria, 0.9f);

        GameObject victoryCanvasObj = CrearCanvasFinJuego(out TextMeshProUGUI txtCountdown);

        float tiempoRestante = tiempoEsperaFinJuego;
        while (tiempoRestante > 0)
        {
            if (txtCountdown != null)
            {
                txtCountdown.text = $"Volviendo al Menu Principal en <color=#FFD700>{Mathf.CeilToInt(tiempoRestante)}s</color>...";
            }
            yield return new WaitForSeconds(1.0f);
            tiempoRestante -= 1.0f;
        }

        if (victoryCanvasObj != null) Destroy(victoryCanvasObj);

        if (!string.IsNullOrEmpty(menuSceneName))
        {
            SceneManager.LoadScene(menuSceneName);
        }
        else
        {
            SceneManager.LoadScene(0);
        }
    }

    private GameObject CrearCanvasFinJuego(out TextMeshProUGUI txtCountdown)
    {
        if (targetCamera == null) targetCamera = Camera.main;

        GameObject canvasObj = new GameObject("VR_Victory_Canvas");
        if (targetCamera != null)
        {
            canvasObj.transform.position = targetCamera.transform.position + (targetCamera.transform.forward * 1.8f);
            canvasObj.transform.rotation = Quaternion.LookRotation(canvasObj.transform.position - targetCamera.transform.position);
        }
        else
        {
            canvasObj.transform.position = transform.position + new Vector3(0, 1.5f, 2.5f);
        }
        canvasObj.transform.localScale = Vector3.one * 0.003f;

        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = targetCamera;

        RectTransform canvasRect = canvasObj.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(850f, 450f);

        GameObject panelObj = new GameObject("Panel");
        panelObj.transform.SetParent(canvasObj.transform, false);
        RectTransform panelRect = panelObj.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;

        Image panelImg = panelObj.AddComponent<Image>();
        panelImg.color = new Color(0.05f, 0.05f, 0.05f, 0.92f);

        Outline outline = panelObj.AddComponent<Outline>();
        outline.effectColor = new Color(1.0f, 0.8f, 0.1f, 0.9f);
        outline.effectDistance = new Vector2(3f, -3f);

        // Titulo de Victoria
        GameObject titleObj = new GameObject("Texto_Titulo");
        titleObj.transform.SetParent(panelObj.transform, false);
        RectTransform titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.05f, 0.70f);
        titleRect.anchorMax = new Vector2(0.95f, 0.95f);
        titleRect.sizeDelta = Vector2.zero;

        TextMeshProUGUI titleTmp = titleObj.AddComponent<TextMeshProUGUI>();
        titleTmp.text = "FELICITACIONES";
        titleTmp.fontSize = 44;
        titleTmp.fontStyle = FontStyles.Bold;
        titleTmp.alignment = TextAlignmentOptions.Center;
        titleTmp.color = new Color(1.0f, 0.85f, 0.1f);

        // Subtitulo
        GameObject subObj = new GameObject("Texto_Subtitulo");
        subObj.transform.SetParent(panelObj.transform, false);
        RectTransform subRect = subObj.AddComponent<RectTransform>();
        subRect.anchorMin = new Vector2(0.05f, 0.50f);
        subRect.anchorMax = new Vector2(0.95f, 0.68f);
        subRect.sizeDelta = Vector2.zero;

        TextMeshProUGUI subTmp = subObj.AddComponent<TextMeshProUGUI>();
        subTmp.text = "HAS COMPLETADO TODOS LOS NIVELES";
        subTmp.fontSize = 28;
        subTmp.alignment = TextAlignmentOptions.Center;
        subTmp.color = new Color(0.9f, 0.9f, 0.9f);

        // Puntuacion Total
        GameObject scoreObj = new GameObject("Texto_Puntos");
        scoreObj.transform.SetParent(panelObj.transform, false);
        RectTransform scoreRect = scoreObj.AddComponent<RectTransform>();
        scoreRect.anchorMin = new Vector2(0.05f, 0.25f);
        scoreRect.anchorMax = new Vector2(0.95f, 0.46f);
        scoreRect.sizeDelta = Vector2.zero;

        TextMeshProUGUI scoreTmp = scoreObj.AddComponent<TextMeshProUGUI>();
        scoreTmp.text = $"PUNTUACION FINAL: <color=#FFD700>{PlayerHUD.totalScore:N0}</color> PTS";
        scoreTmp.fontSize = 38;
        scoreTmp.fontStyle = FontStyles.Bold;
        scoreTmp.alignment = TextAlignmentOptions.Center;
        scoreTmp.color = Color.white;

        // Cuenta atras al menu
        GameObject countObj = new GameObject("Texto_Countdown");
        countObj.transform.SetParent(panelObj.transform, false);
        RectTransform countRect = countObj.AddComponent<RectTransform>();
        countRect.anchorMin = new Vector2(0.05f, 0.05f);
        countRect.anchorMax = new Vector2(0.95f, 0.22f);
        countRect.sizeDelta = Vector2.zero;

        txtCountdown = countObj.AddComponent<TextMeshProUGUI>();
        txtCountdown.text = $"Volviendo al Menu Principal en {Mathf.CeilToInt(tiempoEsperaFinJuego)}s...";
        txtCountdown.fontSize = 24;
        txtCountdown.alignment = TextAlignmentOptions.Center;
        txtCountdown.color = new Color(0.75f, 0.75f, 0.75f);

        return canvasObj;
    }

    private void MostrarMensajeVR(string titulo, string detalle, Color colorTitulo)
    {
        if (targetCamera == null) targetCamera = Camera.main;

        GameObject canvasObj = new GameObject("VR_Transition_Canvas");
        if (targetCamera != null)
        {
            canvasObj.transform.position = targetCamera.transform.position + (targetCamera.transform.forward * 1.6f) + (Vector3.up * 0.1f);
            canvasObj.transform.rotation = Quaternion.LookRotation(canvasObj.transform.position - targetCamera.transform.position);
        }
        else
        {
            canvasObj.transform.position = transform.position + new Vector3(0, 1.5f, 2.0f);
        }
        canvasObj.transform.localScale = Vector3.one * 0.0028f;

        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = targetCamera;

        RectTransform canvasRect = canvasObj.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(800f, 300f);

        GameObject panelObj = new GameObject("Panel");
        panelObj.transform.SetParent(canvasObj.transform, false);
        RectTransform panelRect = panelObj.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;

        Image panelImg = panelObj.AddComponent<Image>();
        panelImg.color = new Color(0.08f, 0.08f, 0.08f, 0.90f);

        GameObject titleObj = new GameObject("Titulo");
        titleObj.transform.SetParent(panelObj.transform, false);
        RectTransform titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.05f, 0.45f);
        titleRect.anchorMax = new Vector2(0.95f, 0.90f);
        titleRect.sizeDelta = Vector2.zero;

        TextMeshProUGUI tmpTitle = titleObj.AddComponent<TextMeshProUGUI>();
        tmpTitle.text = titulo;
        tmpTitle.fontSize = 42;
        tmpTitle.fontStyle = FontStyles.Bold;
        tmpTitle.alignment = TextAlignmentOptions.Center;
        tmpTitle.color = colorTitulo;

        GameObject detObj = new GameObject("Detalle");
        detObj.transform.SetParent(panelObj.transform, false);
        RectTransform detRect = detObj.AddComponent<RectTransform>();
        detRect.anchorMin = new Vector2(0.05f, 0.10f);
        detRect.anchorMax = new Vector2(0.95f, 0.45f);
        detRect.sizeDelta = Vector2.zero;

        TextMeshProUGUI tmpDet = detObj.AddComponent<TextMeshProUGUI>();
        tmpDet.text = detalle;
        tmpDet.fontSize = 28;
        tmpDet.alignment = TextAlignmentOptions.Center;
        tmpDet.color = Color.white;
    }
}




