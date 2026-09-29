using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
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

    private void Start()
    {
        ActualizarEstadoBotonCargar();
    }

    /// <summary>
    /// Actualiza la visibilidad o estado interactivo del boton de Cargar.
    /// </summary>
    public void ActualizarEstadoBotonCargar()
    {
        bool existeGuardado = SaveLoadManager.Instance != null && SaveLoadManager.Instance.HasSavedGame();

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
                GameData data = SaveLoadManager.Instance.GetCurrentSaveData();
                if (data != null && !string.IsNullOrEmpty(data.saveTimestamp))
                {
                    textoDetallesGuardado.text = $"Último guardado: {data.currentSceneName} ({data.saveTimestamp})";
                }
                else
                {
                    textoDetallesGuardado.text = "Partida guardada disponible";
                }
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
        Debug.Log("[MainMenu] Iniciando NUEVA partida en escena: " + gameSceneName);
        SaveLoadManager.Instance.NewGame(gameSceneName);
    }

    /// <summary>
    /// Carga la ultima partida guardada por el sistema de autoguardado.
    /// </summary>
    public void CargarPartida()
    {
        if (SaveLoadManager.Instance.HasSavedGame())
        {
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
