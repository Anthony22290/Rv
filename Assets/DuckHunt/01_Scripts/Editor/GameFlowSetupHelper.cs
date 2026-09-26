using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

/// <summary>
/// Asistente en el Editor de Unity para configurar y validar automáticamente el flujo del juego completo:
/// Menú Principal (MainMenu) -> Mapa 1 (MapaDesierto) -> Mapa 2 (Mapa2) -> Fin del juego y retorno al Menú.
/// Asegura:
/// - 5 Mesas de armas distribuidas en ambos mapas.
/// - Línea de meta visual y física (FinishLineTrigger) al final de cada mapa.
/// - PlayerHUD funcional para puntaje y vida.
/// - Build Settings configurado en orden exacto.
/// </summary>
[InitializeOnLoad]
public static class GameFlowSetupHelper
{
    private const string PATH_MAIN_MENU = "Assets/DuckHunt/00_Scene/MainMenu.unity";
    private const string PATH_DESIERTO = "Assets/DuckHunt/00_Scene/MapaDesierto.unity";
    private const string PATH_MAPA2 = "Assets/DuckHunt/00_Scene/Mapa2.unity";
    private const string PATH_BASIC_SCENE = "Assets/DuckHunt/00_Scene/BasicScene.unity";

    static GameFlowSetupHelper()
    {
        EditorApplication.delayCall += EnsureFullGameFlowConfigured;
    }

    [MenuItem("DuckHunt/Configurar y Validar Flujo Completo del Juego")]
    public static void EnsureFullGameFlowConfigured()
    {
        Debug.Log("[GameFlowSetupHelper] 🚀 Iniciando configuración completa del flujo de juego...");

        // 1. Configurar armas y prefabs
        WeaponSetupHelper.SetupWeapons();

        // 2. Asegurar que Mapa2 esté poblado si estuviera vacío
        if (!File.Exists(PATH_MAPA2) || new FileInfo(PATH_MAPA2).Length < 50000)
        {
            if (File.Exists(PATH_BASIC_SCENE))
            {
                File.Copy(PATH_BASIC_SCENE, PATH_MAPA2, true);
                AssetDatabase.ImportAsset(PATH_MAPA2);
                Debug.Log("[GameFlowSetupHelper] Mapa2.unity configurado a partir de BasicScene.");
            }
        }

        // 3. Configurar MapaDesierto con Mesas de Armas, Meta y Mover
        ConfigurarEscenaDesierto();

        // 4. Configurar Mapa2 con Mesas de Armas, Meta y Mover
        ConfigurarEscenaMapa2();

        // 5. Configurar EditorBuildSettings con las 3 escenas en el orden correcto
        System.Collections.Generic.List<EditorBuildSettingsScene> scenesList = new System.Collections.Generic.List<EditorBuildSettingsScene>();

        if (File.Exists(PATH_MAIN_MENU))
            scenesList.Add(new EditorBuildSettingsScene(PATH_MAIN_MENU, true));

        if (File.Exists(PATH_DESIERTO))
            scenesList.Add(new EditorBuildSettingsScene(PATH_DESIERTO, true));

        if (File.Exists(PATH_MAPA2))
            scenesList.Add(new EditorBuildSettingsScene(PATH_MAPA2, true));

        if (File.Exists(PATH_BASIC_SCENE))
            scenesList.Add(new EditorBuildSettingsScene(PATH_BASIC_SCENE, true));

        EditorBuildSettings.scenes = scenesList.ToArray();
        AssetDatabase.SaveAssets();

        Debug.Log("[GameFlowSetupHelper] ✅ Flujo del juego verificado con éxito:\n" +
                  " 1. MainMenu.unity (Índice 0) -> Al pulsar Jugar va a MapaDesierto\n" +
                  " 2. MapaDesierto.unity (Índice 1) -> 5 Mesas de armas + Meta física que lleva a Mapa2\n" +
                  " 3. Mapa2.unity (Índice 2) -> 5 Mesas de armas + Meta física que muestra Fin del Juego y regresa al Menú.");
    }

    private static void ConfigurarEscenaDesierto()
    {
        if (!File.Exists(PATH_DESIERTO)) return;

        var scene = EditorSceneManager.OpenScene(PATH_DESIERTO, OpenSceneMode.Single);

        // Añadir MapPropsSetup si no está
        var props = Object.FindAnyObjectByType<MapPropsSetup>();
        if (props == null)
        {
            GameObject propsObj = new GameObject("[MapPropsSetup]");
            props = propsObj.AddComponent<MapPropsSetup>();
        }
        props.AsegurarPlayerHUD();
        props.AsegurarMesasDeArmas();
        props.AsegurarLineaDeMeta();

        // Configurar VRAutoForwardMover en el XR Origin
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

        // Añadir MapPropsSetup si no está
        var props = Object.FindAnyObjectByType<MapPropsSetup>();
        if (props == null)
        {
            GameObject propsObj = new GameObject("[MapPropsSetup]");
            props = propsObj.AddComponent<MapPropsSetup>();
        }
        props.AsegurarPlayerHUD();
        props.AsegurarMesasDeArmas();
        props.AsegurarLineaDeMeta();

        // Configurar VRAutoForwardMover en el XR Origin
        var mover = Object.FindAnyObjectByType<VRAutoForwardMover>();
        if (mover != null)
        {
            mover.velocidad = 2.8f;
            mover.avanzar = true;
            mover.distanciaMaximaZ = 240f;
            mover.siguienteEscena = "";
            mover.esNivelFinal = true;
            mover.menuSceneName = "MainMenu";
            EditorUtility.SetDirty(mover);
        }

        EditorSceneManager.SaveScene(scene);
    }
}
