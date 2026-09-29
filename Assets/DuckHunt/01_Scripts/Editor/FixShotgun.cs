using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public class FixShotgun
{
    static FixShotgun()
    {
        EditorApplication.delayCall += DoFix;
    }

    [MenuItem("DuckHunt/Arreglar Escopeta")]
    public static void DoFix()
    {
        string[] armas = { "escopeta", "rifle de caza", "lanzagranadas", "pistola" };
        foreach (string arma in armas)
        {
            string prefabPath = "Assets/DuckHunt/03_Prefabs/" + arma + ".prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab != null)
            {
                GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                PistolaVR pvr = instance.GetComponent<PistolaVR>();
                
                if (pvr != null && pvr.puntoDeDisparo != null)
                {
                    // Alinear para que dispare siempre hacia adelante respecto al arma
                    pvr.puntoDeDisparo.localRotation = Quaternion.identity;
                }
                
                PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
                Object.DestroyImmediate(instance);
            }
        }
        AssetDatabase.SaveAssets();
        Debug.Log("Rotacion de Puntos de Disparo corregida en todas las armas automaticamente.");
    }
}
 
