using UnityEngine;
using UnityEditor;

public class LaserFixerWindow : EditorWindow
{
    [MenuItem("DuckHunt/Herramienta Calibrar Laser")]
    public static void ShowWindow()
    {
        GetWindow<LaserFixerWindow>("Calibrar Laser");
    }

    void OnGUI()
    {
        GUILayout.Label("Ajustar Laser y Texto del Arma", EditorStyles.boldLabel);
        GUILayout.Label("1. Dale a Play.\n2. Agarra la escopeta.\n3. Usa estos botones hasta que el laser apunte bien.", EditorStyles.helpBox);

        if (GUILayout.Button("Girar Laser ARRIBA")) RotateLaser(Vector3.right, -90f);
        if (GUILayout.Button("Girar Laser ABAJO")) RotateLaser(Vector3.right, 90f);
        if (GUILayout.Button("Girar Laser IZQUIERDA")) RotateLaser(Vector3.up, -90f);
        if (GUILayout.Button("Girar Laser DERECHA")) RotateLaser(Vector3.up, 90f);
        if (GUILayout.Button("Girar Laser ROTAR LADO")) RotateLaser(Vector3.forward, 90f);
        
        GUILayout.Space(20);
        if (GUILayout.Button("Guardar Rotacion en Prefab", GUILayout.Height(40))) SaveToPrefab();
    }

    void RotateLaser(Vector3 axis, float angle)
    {
        PistolaVR[] pistolas = FindObjectsByType<PistolaVR>(FindObjectsSortMode.None);
        foreach (var p in pistolas)
        {
            if (p.puntoDeDisparo != null)
            {
                p.puntoDeDisparo.Rotate(axis, angle, Space.Self);
                p.ActualizarTextoMunicion(); // Refresca ui
            }
        }
    }

    void SaveToPrefab()
    {
        PistolaVR[] pistolas = FindObjectsByType<PistolaVR>(FindObjectsSortMode.None);
        foreach (var p in pistolas)
        {
            // Solo escopeta
            if (p.gameObject.name.ToLower().Contains("escopeta"))
            {
                string path = "Assets/DuckHunt/03_Prefabs/escopeta.prefab";
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null)
                {
                    PistolaVR prefabPvr = prefab.GetComponent<PistolaVR>();
                    if (prefabPvr != null && prefabPvr.puntoDeDisparo != null)
                    {
                        prefabPvr.puntoDeDisparo.localRotation = p.puntoDeDisparo.localRotation;
                        PrefabUtility.SavePrefabAsset(prefab);
                        Debug.Log("Escopeta Guardada con la nueva rotacion.");
                    }
                }
            }
        }
    }
}
 
