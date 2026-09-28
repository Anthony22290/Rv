using UnityEngine;
using UnityEditor;

public class AssignReloadSprite
{
    [MenuItem("DuckHunt/Asignar Icono de Recarga")]
    public static void AssignIcon()
    {
        string[] guids = AssetDatabase.FindAssets("reload t:Sprite");
        if (guids.Length == 0)
        {
            Debug.LogError("No se encontro ningun sprite llamado 'reload'. Asegurate de que el tipo de textura sea 'Sprite (2D and UI)'.");
            return;
        }

        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
        Sprite reloadSprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);

        string[] armas = { "pistola", "escopeta", "rifle de caza", "lanzagranadas" };
        foreach (string arma in armas)
        {
            string prefabPath = "Assets/DuckHunt/03_Prefabs/" + arma + ".prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab != null)
            {
                PistolaVR pvr = prefab.GetComponent<PistolaVR>();
                if (pvr != null)
                {
                    pvr.reloadSprite = reloadSprite;
                    EditorUtility.SetDirty(prefab);
                }
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log("Icono de recarga asignado exitosamente a las armas.");
    }
}
