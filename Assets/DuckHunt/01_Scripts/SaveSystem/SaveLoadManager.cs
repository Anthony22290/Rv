using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Gestor central de Guardado y Carga con soporte de Autoguardado periódico y eventos del ciclo de vida.
/// Persiste entre escenas con DontDestroyOnLoad.
/// </summary>
public class SaveLoadManager : MonoBehaviour
{
    private static SaveLoadManager _instance;
    private static bool _isQuitting;

    public static SaveLoadManager Instance
    {
        get
        {
            if (_instance == null && !_isQuitting)
            {
                _instance = FindAnyObjectByType<SaveLoadManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("SaveLoadManager_AutoCreated");
                    _instance = go.AddComponent<SaveLoadManager>();
                }
            }
            return _instance;
        }
    }

    /// <summary>
    /// Garantiza que el gestor exista aunque se entre en Play directamente desde un mapa (sin pasar por el menu).
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        _isQuitting = false;
        _ = Instance;
    }

    [Header("Configuracion de Archivo")]
    [SerializeField] private string saveFileName = "duckhunt_save.json";

    [Header("Autoguardado Automatico")]
    [Tooltip("Habilita el guardado automatico cada cierto intervalo de tiempo durante el juego")]
    public bool enablePeriodicAutoSave = true;

    [Tooltip("Intervalo en segundos para el guardado automatico (por defecto 15s)")]
    public float autoSaveInterval = 15.0f;

    [Tooltip("Guardar automaticamente al pausar la aplicacion o perder el foco (quitarse el visor, menu del sistema VR)")]
    public bool autoSaveOnPause = true;

    [Tooltip("Guardar automaticamente al salir de la aplicacion")]
    public bool autoSaveOnQuit = true;

    [Header("Nombres de Escenas del Menu")]
    public string mainMenuSceneName = "MainMenu";

    private GameData _currentGameData;
    private SaveFileManager _fileManager;
    private float _autoSaveTimer = 0f;
    private bool _isPendingDataLoad = false;

    public string SaveFilePath => FileManager.FullPath;

    private SaveFileManager FileManager
    {
        get
        {
            if (_fileManager == null)
            {
                _fileManager = new SaveFileManager(Application.persistentDataPath, saveFileName);
            }
            return _fileManager;
        }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Update()
    {
        if (!enablePeriodicAutoSave) return;

        _autoSaveTimer += Time.unscaledDeltaTime;
        if (_autoSaveTimer >= autoSaveInterval)
        {
            _autoSaveTimer = 0f;
            // AutoSave comprueba que se este jugando (no en el menu, ni muerto, ni con el nivel terminado)
            AutoSave();
        }
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus && autoSaveOnPause && CanSaveNow())
        {
            AutoSave();
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        // En PC VR (Link / SteamVR) abrir el menu del sistema quita el foco en vez de pausar
        if (!hasFocus && autoSaveOnPause && CanSaveNow())
        {
            AutoSave();
        }
    }

    private void OnApplicationQuit()
    {
        if (autoSaveOnQuit && CanSaveNow())
        {
            AutoSave();
        }
        _isQuitting = true;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        _autoSaveTimer = 0f;

        if (_isPendingDataLoad && _currentGameData != null)
        {
            _isPendingDataLoad = false;
            StartCoroutine(ApplySaveDataNextFrame());
        }
    }

    private IEnumerator ApplySaveDataNextFrame()
    {
        // Esperamos un frame para que los Start() y Awake() de la escena terminen de inicializarse
        yield return null;
        ApplyDataToAllSaveables();
        Debug.Log("[SaveLoadManager] Datos cargados y aplicados a los componentes de la escena.");
    }

    /// <summary>
    /// Indica si el estado actual es valido para guardarse (jugando, vivo y sin nivel completado).
    /// </summary>
    public bool CanSaveNow()
    {
        if (IsMainMenuActive()) return false;
        if (PlayerHealth.Instance != null && PlayerHealth.Instance.IsDead) return false;

        VRAutoForwardMover mover = FindAnyObjectByType<VRAutoForwardMover>();
        if (mover == null || mover.NivelCompletado) return false;

        return true;
    }

    /// <summary>
    /// Guarda el estado actual del juego en memoria y en disco.
    /// </summary>
    public void SaveGame()
    {
        if (!CanSaveNow())
        {
            Debug.Log("[SaveLoadManager] Guardado omitido: no se esta jugando un nivel (menu, muerte o nivel completado).");
            return;
        }

        GameData data = _currentGameData ?? new GameData();
        data.saveTimestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        data.currentSceneName = SceneManager.GetActiveScene().name;
        data.isValid = true;

        List<ISaveable> saveables = FindAllSaveableObjects();
        foreach (ISaveable saveable in saveables)
        {
            saveable.SaveData(data);
        }

        _currentGameData = data;
        FileManager.Save(data);
    }

    /// <summary>
    /// Guarda un punto de control al inicio del siguiente mapa (se llama al completar un nivel).
    /// El jugador aparecera en el punto de inicio del mapa y conservara su puntuacion.
    /// </summary>
    public void SaveCheckpointForScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName)) return;

        _currentGameData = new GameData
        {
            saveTimestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            currentSceneName = sceneName,
            isValid = true,
            hasPlayerPosition = false,
            playerHealth = 0,
            score = PlayerHUD.totalScore
        };

        FileManager.Save(_currentGameData);
        Debug.Log($"[SaveLoadManager] Punto de control guardado al inicio de: {sceneName}");
    }

    /// <summary>
    /// Ejecuta el autoguardado silencioso.
    /// </summary>
    public void AutoSave()
    {
        if (!CanSaveNow()) return;

        SaveGame();
        Debug.Log("[SaveLoadManager] >> Autoguardado automatico completado <<");
    }

    /// <summary>
    /// Carga la partida guardada previamente. Si esta en otra escena, carga la escena primero.
    /// </summary>
    public void LoadLastSavedGame()
    {
        _currentGameData = FileManager.Load();

        if (_currentGameData == null || !_currentGameData.isValid)
        {
            Debug.LogWarning("[SaveLoadManager] No se encontro archivo de guardado valido.");
            return;
        }

        string targetScene = _currentGameData.currentSceneName;

        if (string.IsNullOrEmpty(targetScene) || !Application.CanStreamedLevelBeLoaded(targetScene))
        {
            Debug.LogWarning($"[SaveLoadManager] La escena guardada '{targetScene}' no esta en Build Settings. Se usa MapaDesierto.");
            targetScene = "MapaDesierto";
        }

        // Siempre se recarga la escena para partir de un estado limpio (enemigos, power-ups, etc.)
        PlayerHUD.ResetScore();
        _isPendingDataLoad = true;
        Debug.Log($"[SaveLoadManager] Cargando escena guardada: {targetScene}");
        SceneManager.LoadScene(targetScene);
    }

    /// <summary>
    /// Inicia una nueva partida limpia. La partida anterior se descarta.
    /// </summary>
    public void NewGame(string startingSceneName = "MapaDesierto")
    {
        FileManager.DeleteSaveFile();
        _currentGameData = new GameData
        {
            currentSceneName = startingSceneName,
            isValid = true,
            saveTimestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        };
        _isPendingDataLoad = false;

        PlayerHUD.ResetScore();
        SceneManager.LoadScene(startingSceneName);
    }

    /// <summary>
    /// Aplica los datos cargados a todos los ISaveable en la escena actual.
    /// </summary>
    public void ApplyDataToAllSaveables()
    {
        if (_currentGameData == null) return;

        List<ISaveable> saveables = FindAllSaveableObjects();
        foreach (ISaveable saveable in saveables)
        {
            saveable.LoadData(_currentGameData);
        }
    }

    /// <summary>
    /// Busca todos los componentes ISaveable presentes en la escena.
    /// </summary>
    private List<ISaveable> FindAllSaveableObjects()
    {
        return FindObjectsByType<MonoBehaviour>()
            .OfType<ISaveable>()
            .ToList();
    }

    /// <summary>
    /// Comprueba en disco si hay una partida guardada valida.
    /// </summary>
    public bool HasSavedGame()
    {
        GameData data = GetCurrentSaveData();
        return data != null && data.isValid;
    }

    /// <summary>
    /// Devuelve los datos de la partida guardada en disco (o null si no existe).
    /// </summary>
    public GameData GetCurrentSaveData()
    {
        return FileManager.HasSaveFile() ? FileManager.Load() : null;
    }

    public void DeleteSave()
    {
        _currentGameData = null;
        FileManager.DeleteSaveFile();
    }

    private bool IsMainMenuActive()
    {
        return SceneManager.GetActiveScene().name.Equals(mainMenuSceneName, System.StringComparison.OrdinalIgnoreCase);
    }
}
