using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;

public class GeneradorMapa3
{
    [MenuItem("DuckHunt/Generar Mapa 3 Dark")]
    public static void GenerarMapa3()
    {
        string mapa2Path = "Assets/DuckHunt/00_Scene/Mapa2.unity";
        string mapa3Path = "Assets/DuckHunt/00_Scene/Mapa3.unity";

        if (!System.IO.File.Exists(mapa3Path))
        {
            AssetDatabase.CopyAsset(mapa2Path, mapa3Path);
            AssetDatabase.Refresh();
        }

        Scene mapa3 = EditorSceneManager.OpenScene(mapa3Path);
        
        // Oscurecer ambiente (Tema Dark)
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.05f, 0.05f, 0.1f);
        RenderSettings.skybox = null; // Quitar skybox
        
        Light[] luces = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
        foreach (var l in luces)
        {
            if (l.type == LightType.Directional)
            {
                l.intensity = 0.2f;
                l.color = new Color(0.2f, 0.25f, 0.4f);
            }
        }

        // Extender el camino
        VRAutoForwardMover mover3 = Object.FindAnyObjectByType<VRAutoForwardMover>();
        if (mover3 != null)
        {
            mover3.distanciaMaximaZ *= 1.8f; // Hacer el mapa casi el doble de largo
            EditorUtility.SetDirty(mover3);
        }

        // Multiplicar Mesas y Patos, y borrar Escopetas
        GameObject[] todosLosObjetos = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        
        List<GameObject> mesas = new List<GameObject>();
        List<GameObject> patos = new List<GameObject>();
        List<GameObject> armas = new List<GameObject>();

        foreach (var obj in todosLosObjetos)
        {
            if (obj.scene != mapa3) continue;

            if (obj.name.ToLower().Contains("escopeta"))
            {
                Object.DestroyImmediate(obj);
                continue;
            }

            if (obj.name.ToLower().Contains("mesa") || obj.name.ToLower().Contains("table")) mesas.Add(obj);
            if (obj.name.ToLower().Contains("pato") || obj.name.ToLower().Contains("spawner")) patos.Add(obj);
            if (obj.name.ToLower().Contains("pistola") || obj.name.ToLower().Contains("rifle") || obj.name.ToLower().Contains("lanzagranadas")) armas.Add(obj);
        }

        // Duplicar mesas hacia adelante
        int mesaCount = mesas.Count;
        for (int i = 0; i < mesaCount; i++)
        {
            GameObject m = mesas[i];
            if (m != null)
            {
                // Mesa a medio camino extra
                GameObject nuevaMesa1 = Object.Instantiate(m, m.transform.parent);
                nuevaMesa1.transform.position += new Vector3(0, 0, 50f);
                
                // Mesa al final extra
                GameObject nuevaMesa2 = Object.Instantiate(m, m.transform.parent);
                nuevaMesa2.transform.position += new Vector3(0, 0, 100f);
            }
        }
        
        // Copiar armas a las nuevas posiciones (para que las mesas nuevas tengan armas)
        int armaCount = armas.Count;
        for (int i = 0; i < armaCount; i++)
        {
            GameObject a = armas[i];
            if (a != null)
            {
                GameObject armaNueva1 = Object.Instantiate(a, a.transform.parent);
                armaNueva1.transform.position += new Vector3(0, 0, 50f);
                
                GameObject armaNueva2 = Object.Instantiate(a, a.transform.parent);
                armaNueva2.transform.position += new Vector3(0, 0, 100f);
            }
        }

        // Duplicar patos hacia adelante
        int patoCount = patos.Count;
        for (int i = 0; i < patoCount; i++)
        {
            GameObject p = patos[i];
            if (p != null)
            {
                GameObject nuevoPato1 = Object.Instantiate(p, p.transform.parent);
                nuevoPato1.transform.position += new Vector3(Random.Range(-5f, 5f), Random.Range(-2f, 2f), 40f);
                
                GameObject nuevoPato2 = Object.Instantiate(p, p.transform.parent);
                nuevoPato2.transform.position += new Vector3(Random.Range(-5f, 5f), Random.Range(-2f, 2f), 80f);
                
                GameObject nuevoPato3 = Object.Instantiate(p, p.transform.parent);
                nuevoPato3.transform.position += new Vector3(Random.Range(-5f, 5f), Random.Range(-2f, 2f), 120f);
            }
        }

        EditorSceneManager.SaveScene(mapa3);

        // 4. Agregar a Build Settings
        List<EditorBuildSettingsScene> buildScenes = EditorBuildSettings.scenes.ToList();
        bool contieneMapa3 = false;
        foreach (var bs in buildScenes)
        {
            if (bs.path == mapa3Path) contieneMapa3 = true;
        }

        if (!contieneMapa3)
        {
            buildScenes.Add(new EditorBuildSettingsScene(mapa3Path, true));
            EditorBuildSettings.scenes = buildScenes.ToArray();
        }

        Debug.Log("¡Mapa 3 Generado Exitosamente! Se agrego al Build, es mas oscuro, no tiene escopetas y es mucho mas largo con mas patos y mesas.");
    }
}
 
