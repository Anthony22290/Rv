using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Management;

/// <summary>
/// Crea el XR Interaction Simulator (teclado y raton) en el editor SOLO cuando no hay un visor VR real conectado.
/// La creacion automatica de XRI esta desactivada (Project Settings > XR Plug-in Management > XR Interaction Toolkit)
/// porque con el visor conectado por Link el simulador elimina el visor real del Input System y reemplaza
/// los mandos: la camara se queda en el suelo y los mandos reales dejan de funcionar.
/// </summary>
public static class XRSimuladorCondicional
{
#if UNITY_EDITOR
    // Prefab "XR Interaction Simulator" del sample de XR Interaction Toolkit
    private const string SimulatorPrefabGuid = "58d0a4ac86f2348deb02f3880c71378e";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Inicializar()
    {
        // XR Management inicializa OpenXR antes de cargar la escena: si hay loader activo, hay un visor real
        XRManagerSettings manager = XRGeneralSettings.Instance != null ? XRGeneralSettings.Instance.Manager : null;
        bool hayVisorReal = XRSettings.isDeviceActive || (manager != null && manager.activeLoader != null);

        if (hayVisorReal)
        {
            Debug.Log($"[XRSimuladorCondicional] Visor VR detectado ({XRSettings.loadedDeviceName}): se usan el visor y los mandos reales, sin simulador.");
            return;
        }

        if (Object.FindAnyObjectByType<UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation.XRInteractionSimulator>() != null)
        {
            return;
        }

        string path = UnityEditor.AssetDatabase.GUIDToAssetPath(SimulatorPrefabGuid);
        GameObject prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null)
        {
            Debug.LogWarning("[XRSimuladorCondicional] No se encontro el prefab del XR Interaction Simulator (sample de XRI).");
            return;
        }

        GameObject simulador = Object.Instantiate(prefab);
        simulador.name = prefab.name;
        Object.DontDestroyOnLoad(simulador);
        Debug.Log("[XRSimuladorCondicional] Sin visor VR: XR Interaction Simulator activado (teclado y raton).");
    }
#endif
}
