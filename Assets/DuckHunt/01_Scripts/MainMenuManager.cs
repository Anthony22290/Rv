using System.Collections;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    [Header("Configuración de Escena")]
    [Tooltip("Nombre de la escena a cargar al presionar Nueva Partida")]
    public string gameSceneName = "MapaDesierto";

    [Header("UI de Guardado y Carga")]
    [Tooltip("Boton u objeto del boton 'Cargar Partida' / 'Continuar'")]
    public GameObject botonCargarPartida;

    [Tooltip("Texto opcional para mostrar info del ultimo guardado (Fecha, Mapa)")]
    public TextMeshProUGUI textoDetallesGuardado;

    [Header("Realidad Virtual")]
    [Tooltip("Coloca el menu frente a la cabeza del jugador al iniciar, a la altura y orientacion reales del visor")]
    public bool colocarFrenteAlJugador = true;

    [Tooltip("Distancia en metros entre la cabeza del jugador y el menu")]
    public float distanciaMenu = 3.2f;

    [Tooltip("Segundos de espera minimos antes de intentar colocar el menu (el visor tarda en reportar su posicion)")]
    public float esperaTracking = 0.5f;

    // Rango de altura de la cabeza (sobre el XR Origin) que se considera un visor puesto, sentado o de pie
    private const float AlturaMinimaCabeza = 0.9f;
    private const float AlturaMaximaCabeza = 2.3f;

    private bool _cargando;

    private void Start()
    {
        if (textoDetallesGuardado != null)
        {
            // El texto informativo no debe interceptar el rayo de los mandos VR
            textoDetallesGuardado.raycastTarget = false;
        }

        ActualizarEstadoBotonCargar();

        if (colocarFrenteAlJugador)
        {
            StartCoroutine(ColocarMenuFrenteAlJugador());
        }
    }

    /// <summary>
    /// Coloca el menu frente al jugador cuando el visor tiene una pose valida.
    /// En PC VR es habitual pulsar Play y ponerse el visor despues: mientras la cabeza no este
    /// a una altura creible el menu se queda en su posicion original de la escena.
    /// </summary>
    private IEnumerator ColocarMenuFrenteAlJugador()
    {
        yield return new WaitForSeconds(esperaTracking);

        bool colocadoConVisor = false;
        bool colocado = false;
        var espera = new WaitForSeconds(0.25f);

        // Si se coloco sin visor (pruebas en editor) se vuelve a colocar una vez cuando el visor se active
        while (!colocadoConVisor)
        {
            Camera cam = Camera.main;
            bool visorActivo = XRSettings.isDeviceActive;

            if (cam != null && (!colocado || visorActivo) && CabezaEnPosicionValida(cam, visorActivo))
            {
                Vector3 forward = Vector3.ProjectOnPlane(cam.transform.forward, Vector3.up);
                if (forward.sqrMagnitude < 0.0001f) forward = Vector3.forward;
                forward.Normalize();

                transform.SetPositionAndRotation(cam.transform.position + forward * distanciaMenu, Quaternion.LookRotation(forward));
                colocado = true;
                colocadoConVisor = visorActivo;
            }

            yield return espera;
        }
    }

    private static bool CabezaEnPosicionValida(Camera cam, bool visorActivo)
    {
        // Sin visor la camara queda a la altura del Camera Offset del XR Origin
        if (!visorActivo) return true;

        InputDevice cabeza = InputDevices.GetDeviceAtXRNode(XRNode.Head);
        if (cabeza.isValid && cabeza.TryGetFeatureValue(CommonUsages.isTracked, out bool trackeada) && !trackeada)
        {
            return false;
        }

        XROrigin origin = cam.GetComponentInParent<XROrigin>();
        float altura = origin != null ? cam.transform.position.y - origin.transform.position.y : cam.transform.position.y;
        return altura >= AlturaMinimaCabeza && altura <= AlturaMaximaCabeza;
    }

    /// <summary>
    /// Actualiza la visibilidad o estado interactivo del boton de Cargar.
    /// </summary>
    public void ActualizarEstadoBotonCargar()
    {
        GameData data = SaveLoadManager.Instance != null ? SaveLoadManager.Instance.GetCurrentSaveData() : null;
        bool existeGuardado = data != null && data.isValid;

        if (botonCargarPartida != null)
        {
            botonCargarPartida.SetActive(true);
            Button btn = botonCargarPartida.GetComponent<Button>();
            if (btn != null)
            {
                btn.interactable = true; // Siempre interactuable para que se vea claro en VR y editor
            }
        }

        if (textoDetallesGuardado != null)
        {
            if (existeGuardado)
            {
                textoDetallesGuardado.text = !string.IsNullOrEmpty(data.saveTimestamp)
                    ? $"Último guardado: {data.currentSceneName} ({data.saveTimestamp})"
                    : "Partida guardada disponible";
            }
            else
            {
                textoDetallesGuardado.text = "Sin partida guardada previa";
            }
        }
    }

    /// <summary>
    /// Inicia una nueva partida desde cero.
    /// </summary>
    public void Jugar()
    {
        NuevaPartida();
    }

    /// <summary>
    /// Comienza una nueva partida reiniciando la puntuacion y cargando el mapa inicial.
    /// </summary>
    public void NuevaPartida()
    {
        // Evita cargas dobles si el gatillo del mando dispara el boton varias veces
        if (_cargando) return;
        _cargando = true;

        Debug.Log("[MainMenu] Iniciando NUEVA partida en escena: " + gameSceneName);
        SaveLoadManager.Instance.NewGame(gameSceneName);
    }

    /// <summary>
    /// Carga la ultima partida guardada por el sistema de autoguardado.
    /// </summary>
    public void CargarPartida()
    {
        if (_cargando) return;

        if (SaveLoadManager.Instance.HasSavedGame())
        {
            _cargando = true;
            Debug.Log("[MainMenu] Cargando ultima partida guardada...");
            SaveLoadManager.Instance.LoadLastSavedGame();
        }
        else
        {
            Debug.LogWarning("[MainMenu] No hay partida guardada para cargar. Iniciando nueva...");
            NuevaPartida();
        }
    }

    /// <summary>
    /// Cierra el juego o detiene el modo de juego en el editor de Unity.
    /// </summary>
    public void Salir()
    {
        Debug.Log("[MainMenu] Saliendo del juego...");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
