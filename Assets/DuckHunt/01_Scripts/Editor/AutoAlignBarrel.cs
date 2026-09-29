using UnityEngine;
using UnityEditor;

public class AutoAlignBarrel
{
    [MenuItem("DuckHunt/Auto-Alinear Cañon Escopeta")]
    public static void Align()
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
                    // Encontrar el MeshRenderer mas grande
                    Renderer[] renderers = instance.GetComponentsInChildren<Renderer>();
                    if (renderers.Length > 0)
                    {
                        Bounds combinedBounds = renderers[0].bounds;
                        foreach (Renderer r in renderers)
                        {
                            combinedBounds.Encapsulate(r.bounds);
                        }
                        
                        // Encontrar el vertice mas alejado del centro (la punta del cañon)
                        MeshFilter[] meshes = instance.GetComponentsInChildren<MeshFilter>();
                        Vector3 furthestPoint = combinedBounds.center;
                        float maxDist = 0;
                        
                        foreach (MeshFilter mf in meshes)
                        {
                            if (mf.sharedMesh == null) continue;
                            foreach (Vector3 vertex in mf.sharedMesh.vertices)
                            {
                                Vector3 worldVert = mf.transform.TransformPoint(vertex);
                                float dist = Vector3.Distance(combinedBounds.center, worldVert);
                                if (dist > maxDist)
                                {
                                    // Validar que la punta este mas o menos "hacia adelante" o "arriba" 
                                    // respecto a la rotacion visual
                                    maxDist = dist;
                                    furthestPoint = worldVert;
                                }
                            }
                        }
                        
                        // La direccion del cañon es desde el centro del arma hasta la punta mas lejana
                        Vector3 aimDirection = (furthestPoint - combinedBounds.center).normalized;
                        
                        if (aimDirection != Vector3.zero)
                        {
                            pvr.puntoDeDisparo.position = furthestPoint; // Mover a la punta real
                            pvr.puntoDeDisparo.rotation = Quaternion.LookRotation(aimDirection);
                        }
                    }
                }
                
                PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
                Object.DestroyImmediate(instance);
            }
        }
        AssetDatabase.SaveAssets();
        Debug.Log("Cañones alineados usando analisis de malla geométrica.");
    }
}
