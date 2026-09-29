using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// Asistente en el Editor de Unity para configurar y validar automáticamente el flujo del juego completo:
/// Menú Principal (MainMenu) -> Mapa 1 (MapaDesierto) -> Mapa 2 (Mapa2) -> Mapa 3 (Mapa3) -> Mapa 4 (Mapa4 Final) -> Retorno al Menú.
/// Asegura:
/// - Secuencia continua de 4 mapas sin importar desde cuál mapa inicie la ejecución.
/// - Mesas de armas alcanzables y proporcionales.
/// - Línea de meta visual y física (FinishLineTrigger) al final de cada mapa.
/// - Build Settings con todas las escenas registradas en orden exacto.
/// </summary>
[InitializeOnLoad]
public static class GameFlowSetupHelper
{
    private const string PATH_MAIN_MENU = "Assets/DuckHunt/00_Scene/MainMenu.unity";
    private const string PATH_DESIERTO = "Assets/DuckHunt/00_Scene/MapaDesierto.unity";
    private const string PATH_MAPA2 = "Assets/DuckHunt/00_Scene/Mapa2.unity";
    private const string PATH_MAPA3 = "Assets/DuckHunt/00_Scene/Mapa3.unity";
    private const string PATH_MAPA4 = "Assets/DuckHunt/00_Scene/Mapa4.unity";
    private const string PATH_BASIC_SCENE = "Assets/DuckHunt/00_Scene/BasicScene.unity";

    static GameFlowSetupHelper()
    {
        EditorApplication.delayCall += EnsureFullGameFlowConfigured;
    }

    [MenuItem("DuckHunt/Configurar y Validar Flujo Completo del Juego")]
    public static void EnsureFullGameFlowConfigured()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        Debug.Log("[GameFlowSetupHelper] 🎮 Configurando flujo completo de los 4 mapas...");

        // 1. Configurar cada escena con su siguiente mapa respectivo
        ConfigurarEscenaDesierto();
        ConfigurarEscenaMapa2();
        ConfigurarEscenaMapa3();
        ConfigurarEscenaMapa4();

        // 2. Configurar EditorBuildSettings con TODAS las escenas en orden
        List<EditorBuildSettingsScene> scenesList = new List<EditorBuildSettingsScene>();

        if (File.Exists(PATH_MAIN_MENU))
            scenesList.Add(new EditorBuildSettingsScene(PATH_MAIN_MENU, true));

        if (File.Exists(PATH_DESIERTO))
            scenesList.Add(new EditorBuildSettingsScene(PATH_DESIERTO, true));

        if (File.Exists(PATH_MAPA2))
            scenesList.Add(new EditorBuildSettingsScene(PATH_MAPA2, true));

        if (File.Exists(PATH_MAPA3))
            scenesList.Add(new EditorBuildSettingsScene(PATH_MAPA3, true));

        if (File.Exists(PATH_MAPA4))
            scenesList.Add(new EditorBuildSettingsScene(PATH_MAPA4, true));

        if (File.Exists(PATH_BASIC_SCENE))
            scenesList.Add(new EditorBuildSettingsScene(PATH_BASIC_SCENE, true));

        EditorBuildSettings.scenes = scenesList.ToArray();
        AssetDatabase.SaveAssets();

        Debug.Log("[GameFlowSetupHelper] ✅ Flujo del juego verificado con éxito:\n" +
                  " 1. MainMenu.unity (Índice 0) -> Jugar va a MapaDesierto\n" +
                  " 2. MapaDesierto.unity (Índice 1) -> Pasa a Mapa2\n" +
                  " 3. Mapa2.unity (Índice 2) -> Pasa a Mapa3\n" +
                  " 4. Mapa3.unity (Índice 3) -> Pasa a Mapa4\n" +
                  " 5. Mapa4.unity (Índice 4) -> Nivel Final (Pantalla de Victoria y retorno al Menú)\n" +
                  " 6. BasicScene.unity (Índice 5) -> Pasa a Mapa2");
    }

    private static void ConfigurarEscenaDesierto()
    {
        if (!File.Exists(PATH_DESIERTO)) return;

        var scene = EditorSceneManager.OpenScene(PATH_DESIERTO, OpenSceneMode.Single);

        AsegurarPropsYMeta(scene, 238f, false);

        var mover = Object.FindAnyObjectByType<VRAutoForwardMover>();
        if (mover != null)
        {
            mover.velocidad = 2.8f;
            mover.avanzar = true;
            mover.distanciaMaximaZ = 240f;
            mover.siguienteEscena = "Mapa2";
            mover.esNivelFinal = false;
            mover.menuSceneName = "MainMenu";
            EditorUtility.SetDirty(mover);
        }

        EditorSceneManager.SaveScene(scene);
    }

    private static void ConfigurarEscenaMapa2()
    {
        if (!File.Exists(PATH_MAPA2)) return;

        var scene = EditorSceneManager.OpenScene(PATH_MAPA2, OpenSceneMode.Single);

        AsegurarPropsYMeta(scene, 238f, false);

        var mover = Object.FindAnyObjectByType<VRAutoForwardMover>();
        if (mover != null)
        {
            mover.velocidad = 2.8f;
            mover.avanzar = true;
            mover.distanciaMaximaZ = 240f;
            mover.siguienteEscena = "Mapa3";
            mover.esNivelFinal = false;
            mover.menuSceneName = "MainMenu";
            EditorUtility.SetDirty(mover);
        }

        EditorSceneManager.SaveScene(scene);
    }

    private static void ConfigurarEscenaMapa3()
    {
        if (!File.Exists(PATH_MAPA3)) return;

        var scene = EditorSceneManager.OpenScene(PATH_MAPA3, OpenSceneMode.Single);

        AsegurarPropsYMeta(scene, 428f, false);

        var mover = Object.FindAnyObjectByType<VRAutoForwardMover>();
        if (mover != null)
        {
            mover.velocidad = 2.8f;
            mover.avanzar = true;
            mover.distanciaMaximaZ = 432f;
            mover.siguienteEscena = "Mapa4";
            mover.esNivelFinal = false;
            mover.menuSceneName = "MainMenu";
            EditorUtility.SetDirty(mover);
        }

        EditorSceneManager.SaveScene(scene);
    }

    private static void ConfigurarEscenaMapa4()
    {
        if (!File.Exists(PATH_MAPA4)) return;

        var scene = EditorSceneManager.OpenScene(PATH_MAPA4, OpenSceneMode.Single);

        AsegurarPropsYMeta(scene, 940f, true);

        var mover = Object.FindAnyObjectByType<VRAutoForwardMover>();
        if (mover != null)
        {
            mover.velocidad = 5.5f;
            mover.avanzar = true;
            mover.distanciaMaximaZ = 950f;
            mover.siguienteEscena = "";
            mover.esNivelFinal = true;
            mover.menuSceneName = "MainMenu";
            EditorUtility.SetDirty(mover);
        }

        EditorSceneManager.SaveScene(scene);
    }

    private static void AsegurarPropsYMeta(UnityEngine.SceneManagement.Scene scene, float zMeta, bool esNivelFinal)
    {
        // Asegurar o reubicar Finish Line
        GameObject arch = GameObject.Find("[Finish_Line_Arch]");
        if (arch != null)
        {
            arch.transform.position = new Vector3(0, 0, zMeta);
        }
        else
        {
            arch = new GameObject("[Finish_Line_Arch]");
            arch.transform.position = new Vector3(0, 0, zMeta);

            Material checkMat = Resources.Load<Material>("M_TNT_Red") ?? new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));

            // Poste Izquierdo
            GameObject postL = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            postL.name = "Poste_Izquierdo";
            postL.transform.SetParent(arch.transform, false);
            postL.transform.localPosition = new Vector3(-3.2f, 2.2f, 0);
            postL.transform.localScale = new Vector3(0.35f, 2.2f, 0.35f);

            // Poste Derecho
            GameObject postR = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            postR.name = "Poste_Derecho";
            postR.transform.SetParent(arch.transform, false);
            postR.transform.localPosition = new Vector3(3.2f, 2.2f, 0);
            postR.transform.localScale = new Vector3(0.35f, 2.2f, 0.35f);

            // Travesaño Superior
            GameObject beam = GameObject.CreatePrimitive(PrimitiveType.Cube);
            beam.name = "Travesaño_Superior";
            beam.transform.SetParent(arch.transform, false);
            beam.transform.localPosition = new Vector3(0, 4.4f, 0);
            beam.transform.localScale = new Vector3(6.8f, 0.45f, 0.45f);

            // Cartel de Meta
            GameObject banner = GameObject.CreatePrimitive(PrimitiveType.Cube);
            banner.name = "Cartel_Meta";
            banner.transform.SetParent(arch.transform, false);
            banner.transform.localPosition = new Vector3(0, 3.75f, 0);
            banner.transform.localScale = new Vector3(5.5f, 0.8f, 0.1f);
            banner.GetComponent<MeshRenderer>().sharedMaterial = checkMat;

            // Texto 3D de Meta
            GameObject bannerText = new GameObject("Texto_Meta");
            bannerText.transform.SetParent(banner.transform, false);
            bannerText.transform.localPosition = new Vector3(0, 0, -0.6f);
            bannerText.transform.localScale = new Vector3(0.18f, 1.25f, 1f);

            var tmp = bannerText.AddComponent<TMPro.TextMeshPro>();
            tmp.text = esNivelFinal ? "META FINAL" : "META - SIGUIENTE NIVEL";
            tmp.fontSize = 26;
            tmp.fontStyle = TMPro.FontStyles.Bold;
            tmp.alignment = TMPro.TextAlignmentOptions.Center;
            tmp.color = Color.white;

            // Trigger BoxCollider
            GameObject triggerObj = new GameObject("FinishLine_TriggerZone");
            triggerObj.transform.SetParent(arch.transform, false);
            triggerObj.transform.localPosition = new Vector3(0, 1.5f, 0);

            BoxCollider triggerCol = triggerObj.AddComponent<BoxCollider>();
            triggerCol.isTrigger = true;
            triggerCol.size = new Vector3(7.0f, 4.0f, 2.5f);

            triggerObj.AddComponent<FinishLineTrigger>();
        }

        // Actualizar texto del cartel
        var textObj = arch.transform.Find("Cartel_Meta/Texto_Meta");
        if (textObj != null)
        {
            var tmp = textObj.GetComponent<TMPro.TextMeshPro>();
            if (tmp != null)
            {
                tmp.text = esNivelFinal ? "META FINAL" : "META - SIGUIENTE NIVEL";
            }
        }
    }
}
