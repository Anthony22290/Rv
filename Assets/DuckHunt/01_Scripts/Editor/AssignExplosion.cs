using UnityEngine;
using UnityEditor;

public class AssignExplosion
{
    [MenuItem("DuckHunt/Asignar Explosion a Patos")]
    public static void DoAssign()
    {
        string explPath = "Assets/DuckHunt/03_Prefabs/_Explosion (BASE).prefab";
        GameObject explPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(explPath);
        
        if (explPrefab == null)
        {
            explPath = "Assets/DuckHunt/03_Prefabs/_Explosion FREE (BASE).prefab";
            explPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(explPath);
        }

        if (explPrefab == null)
        {
            Debug.LogError("No se encontro el prefab de explosion");
            return;
        }

        string[] guids = AssetDatabase.FindAssets("t:GameObject", new[] { "Assets/DuckHunt/03_Prefabs" });
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (path.Contains("Pato") || path.Contains("Duck"))
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null)
                {
                    EnemyTarget et = prefab.GetComponent<EnemyTarget>();
                    if (et != null)
                    {
                        et.explosionPrefab = explPrefab;
                        EditorUtility.SetDirty(prefab);
                        Debug.Log("Asignada explosion a: " + prefab.name);
                    }
                }
            }
        }
        AssetDatabase.SaveAssets();
    }
}
