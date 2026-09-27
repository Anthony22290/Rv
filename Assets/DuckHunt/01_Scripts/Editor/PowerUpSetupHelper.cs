using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

/// <summary>
/// Asistente en el Editor de Unity para generar los Scriptable Objects de los Power-Ups
/// y asegurar que el PowerUpManager y el LeftHandPowerUpController estén en todas las escenas del juego.
/// </summary>
[InitializeOnLoad]
public static class PowerUpSetupHelper
{
    private const string SO_FOLDER = "Assets/DuckHunt/06_ScriptableObjects";
    private const string PATH_ESCUDO = SO_FOLDER + "/PowerUp_Escudo.asset";
    private const string PATH_RALENTIZAR = SO_FOLDER + "/PowerUp_Ralentizar.asset";
    private const string PATH_DOBLE_PUNTOS = SO_FOLDER + "/PowerUp_DoblePuntos.asset";

    private const string PATH_DESIERTO = "Assets/DuckHunt/00_Scene/MapaDesierto.unity";
    private const string PATH_MAPA2 = "Assets/DuckHunt/00_Scene/Mapa2.unity";
    private const string PATH_BASIC_SCENE = "Assets/DuckHunt/00_Scene/BasicScene.unity";

    static PowerUpSetupHelper()
    {
        EditorApplication.delayCall += EnsurePowerUpsConfigured;
    }

    [MenuItem("DuckHunt/Configurar Power-Ups y Scriptable Objects")]
    public static void EnsurePowerUpsConfigured()
    {
        Debug.Log("[PowerUpSetupHelper] 🛠️ Configurando y validando Power-Ups...");

        if (!Directory.Exists(SO_FOLDER))
        {
            Directory.CreateDirectory(SO_FOLDER);
            AssetDatabase.Refresh();
        }

        PowerUpDataSO escudoSO = AsegurarScriptableObject(
            PATH_ESCUDO,
            "Escudo Defensivo",
            PowerUpType.Escudo,
            "🛡️",
            "Crea una barrera de energía que absorbe el siguiente impacto recibido.",
            0f,
            1,
            1f,
            new Color(0.15f, 0.75f, 1f, 1f),
            new Color(0f, 0.4f, 0.9f, 1f)
        );

        PowerUpDataSO slowSO = AsegurarScriptableObject(
            PATH_RALENTIZAR,
            "Cámara Lenta",
            PowerUpType.Ralentizar,
            "⏱️",
            "Ralentiza el vuelo de todos los patos durante 5 segundos para apuntar con precisión.",
            5f,
            1,
            0.35f,
            new Color(0.7f, 0.35f, 1f, 1f),
            new Color(0.5f, 0.1f, 0.9f, 1f)
        );

        PowerUpDataSO x2SO = AsegurarScriptableObject(
            PATH_DOBLE_PUNTOS,
            "Puntos Dobles",
            PowerUpType.DoblePuntos,
            "⭐",
            "Multiplica por 2 todos los puntos obtenidos por cada pato abatido.",
            10f,
            2,
            1f,
            new Color(1f, 0.85f, 0.1f, 1f),
            new Color(0.9f, 0.65f, 0f, 1f)
        );

        AssetDatabase.SaveAssets();

        // Configurar escenas con PowerUpManager y LeftHandPowerUpController
        ConfigurarEscena(PATH_DESIERTO, escudoSO, slowSO, x2SO);
        ConfigurarEscena(PATH_MAPA2, escudoSO, slowSO, x2SO);
        ConfigurarEscena(PATH_BASIC_SCENE, escudoSO, slowSO, x2SO);

        Debug.Log("[PowerUpSetupHelper] ✅ Power-Ups configurados exitosamente en Scriptable Objects y Escenas.");
    }

    private static PowerUpDataSO AsegurarScriptableObject(
        string assetPath, 
        string name, 
        PowerUpType type, 
        string emoji, 
        string desc, 
        float duration, 
        int multiplier, 
        float slowFactor, 
        Color themeCol, 
        Color emissionCol)
    {
        PowerUpDataSO asset = AssetDatabase.LoadAssetAtPath<PowerUpDataSO>(assetPath);
        if (asset == null)
        {
            asset = ScriptableObject.CreateInstance<PowerUpDataSO>();
            asset.powerUpName = name;
            asset.powerUpType = type;
            asset.iconEmoji = emoji;
            asset.description = desc;
            asset.duration = duration;
            asset.scoreMultiplier = multiplier;
            asset.slowSpeedFactor = slowFactor;
            asset.themeColor = themeCol;
            asset.emissionColor = emissionCol;

            AssetDatabase.CreateAsset(asset, assetPath);
            Debug.Log($"[PowerUpSetupHelper] Creado ScriptableObject: {assetPath}");
        }
        else
        {
            asset.powerUpName = name;
            asset.powerUpType = type;
            asset.iconEmoji = emoji;
            asset.description = desc;
            asset.duration = duration;
            asset.scoreMultiplier = multiplier;
            asset.slowSpeedFactor = slowFactor;
            asset.themeColor = themeCol;
            asset.emissionColor = emissionCol;
            EditorUtility.SetDirty(asset);
        }
        return asset;
    }

    private static void ConfigurarEscena(string scenePath, params PowerUpDataSO[] powerUps)
    {
        if (!File.Exists(scenePath)) return;

        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        // 1. Asegurar PowerUpManager
        var powerUpMgr = Object.FindAnyObjectByType<PowerUpManager>();
        if (powerUpMgr == null)
        {
            GameObject mgrObj = new GameObject("[PowerUpManager]");
            powerUpMgr = mgrObj.AddComponent<PowerUpManager>();
        }
        powerUpMgr.availablePowerUps = powerUps;
        powerUpMgr.dropChance = 0.45f;
        EditorUtility.SetDirty(powerUpMgr);

        // 2. Asegurar LeftHandPowerUpController en el controlador izquierdo o en el XR Origin
        var leftCtrl = Object.FindAnyObjectByType<LeftHandPowerUpController>();
        if (leftCtrl == null)
        {
            GameObject ctrlObj = new GameObject("LeftHand_PowerUp_Socket");
            leftCtrl = ctrlObj.AddComponent<LeftHandPowerUpController>();
        }
        EditorUtility.SetDirty(leftCtrl);

        EditorSceneManager.SaveScene(scene);
    }
}
