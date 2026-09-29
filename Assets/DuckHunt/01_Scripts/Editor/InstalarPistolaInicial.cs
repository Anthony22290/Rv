using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class InstalarPistolaInicial
{
    [MenuItem("DuckHunt/Instalar Pistola Inicial en Mapas")]
    public static void Instalar()
    {
        string[] escenas = { "MapaDesierto", "Mapa2", "Mapa3", "Mapa4" };
        GameObject prefabPistola = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/DuckHunt/03_Prefabs/pistola.prefab");

        if (prefabPistola == null) { Debug.LogError("No hay prefab pistola"); return; }

        foreach (string escena in escenas)
        {
            string path = "Assets/DuckHunt/00_Scene/" + escena + ".unity";
            if (System.IO.File.Exists(path))
            {
                Scene s = EditorSceneManager.OpenScene(path);
                
                // Buscar jugador (Mover)
                VRAutoForwardMover mover = Object.FindAnyObjectByType<VRAutoForwardMover>();
                if (mover != null)
                {
                    // Buscar si ya hay una pistola muy cerca
                    bool yaTiene = false;
                    PistolaVR[] pistolas = Object.FindObjectsByType<PistolaVR>(FindObjectsSortMode.None);
                    foreach (var p in pistolas)
                    {
                        if (p.gameObject.name.ToLower().Contains("pistola") && Vector3.Distance(p.transform.position, mover.transform.position) < 2f)
                        {
                            yaTiene = true; break;
                        }
                    }

                    if (!yaTiene)
                    {
                        GameObject p = (GameObject)PrefabUtility.InstantiatePrefab(prefabPistola);
                        p.transform.position = mover.transform.position + Vector3.up * 1f + mover.transform.forward * 0.4f;
                        Debug.Log("Pistola inicial instalada en " + escena);
                    }
                }
                EditorSceneManager.SaveScene(s);
            }
        }
        Debug.Log("Pistolas iniciales aplicadas en todos los mapas jugables.");
    }
}
