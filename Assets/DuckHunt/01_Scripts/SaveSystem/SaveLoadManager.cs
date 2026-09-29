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
    public static SaveLoadManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindAnyObjectByType<SaveLoadManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("SaveLoadManager_AutoCreated");
                    _instance = go.AddComponent<SaveLoadManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }

    [Header("Configuracion de Archivo")]
    [SerializeField] private string saveFileName = "duckhunt_save.json";

    [Header("Autoguardado Automatico")]
    [Tooltip("Habilita el guardado automatico cada cierto intervalo de tiempo durante el juego")]
    public bool enablePeriodicAutoSave = true;

    [Tooltip("Intervalo en segundos para el guardado automatico (por defecto 15s)")]
    public float autoSaveInterval = 15.0f;

    [Tooltip("Guardar automaticamente al pausar o minimizar la aplicacion")]
    public bool autoSaveOnPause = true;

    [Tooltip("Guardar automaticamente al salir de la aplicacion")]
    public bool autoSaveOnQuit = true;

    [Header("Nombres de Escenas del Menu")]
    public string mainMenuSceneName = "MainMenu";

    private GameData _currentGameData;
    private SaveFileManager _fileManager;
    private float _autoSaveTimer = 0f;
    private bool _isPendingDataLoad = false;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        if (_fileManager == null)
        {
            _fileManager = new SaveFileManager(Application.persistentDataPath, saveFileName);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        // Verificar si existe partida previa al iniciar
        if (_fileManager.HasSaveFile())
        {
            _currentGameData = _fileManager.Load();
        }
    }

    private void Update()
    {
        // Solo autoguardar si estamos en una escena de juego (no en el menu principal)
        if (enablePeriodicAutoSave && !IsMainMenuActive())
        {
            _autoSaveTimer += Time.deltaTime;
            if (_autoSaveTimer >= autoSaveInterval)
            {
                _autoSaveTimer = 0f;
                AutoSave();
            }
        }
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus && autoSaveOnPause && !IsMainMenuActive())
        {
            AutoSave();
        }
    }

    private void OnApplicationQuit()
    {
        if (autoSaveOnQuit && !IsMainMenuActive())
        {
            AutoSave();
        }
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
    /// Guarda el estado actual del juego en memoria y en disco.
    /// </summary>
    public void SaveGame()
    {
        if (IsMainMenuActive())
        {
            Debug.Log("[SaveLoadManager] No se guarda en el Menu Principal.");
            return;
        }

        if (_currentGameData == null)
        {
            _currentGameData = new GameData();
        }

        _currentGameData.saveTimestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        _currentGameData.currentSceneName = SceneManager.GetActiveScene().name;
        _currentGameData.isValid = true;

        List<ISaveable> saveables = FindAllSaveableObjects();
        foreach (ISaveable saveable in saveables)
        {
            saveable.SaveData(_currentGameData);
        }

        _fileManager.Save(_currentGameData);
    }

    /// <summary>
    /// Ejecuta el autoguardado silencioso.
    /// </summary>
    public void AutoSave()
    {
        SaveGame();
        Debug.Log("[SaveLoadManager] >> Autoguardado automatico completado <<");
    }

    /// <summary>
    /// Carga la partida guardada previamente. Si esta en otra escena, carga la escena primero.
    /// </summary>
    public void LoadLastSavedGame()
    {
        _currentGameData = _fileManager.Load();

        if (_currentGameData == null || !_currentGameData.isValid)
        {
            Debug.LogWarning("[SaveLoadManager] No se encontro archivo de guardado valido.");
            return;
        }

        string targetScene = _currentGameData.currentSceneName;

        if (string.IsNullOrEmpty(targetScene))
        {
            targetScene = "MapaDesierto";
        }

        _isPendingDataLoad = true;

        // Si ya estamos en la escena correspondiente, aplicamos directamente
        if (SceneManager.GetActiveScene().name == targetScene)
        {
            _isPendingDataLoad = false;
            ApplyDataToAllSaveables();
        }
        else
        {
            Debug.Log($"[SaveLoadManager] Cargando escena guardada: {targetScene}");
            SceneManager.LoadScene(targetScene);
        }
    }

    /// <summary>
    /// Inicia una nueva partida limpia eliminando o sobreescribiendo datos temporales.
    /// </summary>
    public void NewGame(string startingSceneName = "MapaDesierto")
    {
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

    public bool HasSavedGame()
    {
        if (_fileManager == null)
        {
            _fileManager = new SaveFileManager(Application.persistentDataPath, saveFileName);
        }

        if (!_fileManager.HasSaveFile()) return false;

        if (_currentGameData == null)
        {
            _currentGameData = _fileManager.Load();
        }

        return _currentGameData != null && _currentGameData.isValid;
    }

    public GameData GetCurrentSaveData()
    {
        if (_currentGameData == null && _fileManager != null && _fileManager.HasSaveFile())
        {
            _currentGameData = _fileManager.Load();
        }
        return _currentGameData;
    }

    public void DeleteSave()
    {
        _currentGameData = null;
        if (_fileManager != null)
        {
            _fileManager.DeleteSaveFile();
        }
    }

    private bool IsMainMenuActive()
    {
        return SceneManager.GetActiveScene().name.Equals(mainMenuSceneName, System.StringComparison.OrdinalIgnoreCase);
    }
}
