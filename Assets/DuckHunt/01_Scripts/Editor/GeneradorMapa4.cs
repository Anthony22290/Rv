using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;

public class GeneradorMapa4
{
    [MenuItem("DuckHunt/Generar Mapa 4 (Nivel Final)")]
    public static void GenerarMapa4Func()
    {
        string mapa3Path = "Assets/DuckHunt/00_Scene/Mapa3.unity";
        string mapa4Path = "Assets/DuckHunt/00_Scene/Mapa4.unity";

        if (!System.IO.File.Exists(mapa3Path))
        {
            Debug.LogError("No existe el Mapa 3. ¡Generalo primero!");
            return;
        }

        if (!System.IO.File.Exists(mapa4Path))
        {
            AssetDatabase.CopyAsset(mapa3Path, mapa4Path);
            AssetDatabase.Refresh();
        }

        // 1. Modificar Mapa 3 para que conecte a Mapa 4
        Scene mapa3 = EditorSceneManager.OpenScene(mapa3Path);
        VRAutoForwardMover mover3 = Object.FindAnyObjectByType<VRAutoForwardMover>();
        if (mover3 != null)
        {
            mover3.siguienteEscena = "Mapa4";
            mover3.esNivelFinal = false;
            EditorUtility.SetDirty(mover3);
        }
        EditorSceneManager.SaveScene(mapa3);

        // 2. Modificar Mapa 4
        Scene mapa4 = EditorSceneManager.OpenScene(mapa4Path);
        
        // Estética Nivel Final (Rojo/Infernal)
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.3f, 0.05f, 0.05f); // Luz ambiente roja oscura
        
        // Niebla
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = new Color(0.15f, 0.0f, 0.0f);
        RenderSettings.fogDensity = 0.015f;

        Light[] luces = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
        foreach (var l in luces)
        {
            if (l.type == LightType.Directional)
            {
                l.intensity = 0.8f;
                l.color = new Color(1.0f, 0.2f, 0.1f); // Luz direccional roja/naranja intensa
                l.transform.rotation = Quaternion.Euler(20f, -30f, 0f); // Atardecer muy bajo
            }
        }

        // Extender el camino y hacerlo Nivel Final
        VRAutoForwardMover mover4 = Object.FindAnyObjectByType<VRAutoForwardMover>();
        if (mover4 != null)
        {
            mover4.siguienteEscena = "";
            mover4.esNivelFinal = true; // Activa la pantalla de victoria
            mover4.menuSceneName = "MainMenu";
            
            // Mas distancia para ser el ultimo mapa
            mover4.distanciaMaximaZ *= 1.3f; 
            mover4.velocidad *= 1.2f; // Un poco mas rapido!

            EditorUtility.SetDirty(mover4);
        }

        // Aumentar drasticamente la dificultad (duplicar todos los patos)
        GameObject[] todosLosObjetos = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        
        List<GameObject> patos = new List<GameObject>();

        foreach (var obj in todosLosObjetos)
        {
            if (obj.scene != mapa4) continue;
            if (obj.name.ToLower().Contains("pato") || obj.name.ToLower().Contains("spawner")) patos.Add(obj);
        }

        int patoCount = patos.Count;
        for (int i = 0; i < patoCount; i++)
        {
            GameObject p = patos[i];
            if (p != null)
            {
                // Copia extra a la izquierda
                GameObject nuevoPato1 = Object.Instantiate(p, p.transform.parent);
                nuevoPato1.transform.position += new Vector3(-8f + Random.Range(-2f, 2f), Random.Range(1f, 3f), Random.Range(-10f, 10f));
                
                // Copia extra a la derecha
                GameObject nuevoPato2 = Object.Instantiate(p, p.transform.parent);
                nuevoPato2.transform.position += new Vector3(8f + Random.Range(-2f, 2f), Random.Range(1f, 3f), Random.Range(-10f, 10f));
            }
        }

        // Borrar musica vieja e instalar una placeholder si no hay
        GameObject bgMusic = GameObject.Find("MusicaDeFondo");
        if (bgMusic != null)
        {
            AudioSource src = bgMusic.GetComponent<AudioSource>();
            AudioClip clip = Resources.Load<AudioClip>("Musica/mundo4"); // Si el usuario agrega mundo4
            if (clip != null) src.clip = clip;
            else src.pitch = 0.8f; // Si usa la del mundo 3, la ralentizamos para que suene dark
        }

        EditorSceneManager.SaveScene(mapa4);

        // 3. Agregar a Build Settings
        List<EditorBuildSettingsScene> buildScenes = EditorBuildSettings.scenes.ToList();
        bool contieneMapa4 = false;
        foreach (var bs in buildScenes)
        {
            if (bs.path == mapa4Path) contieneMapa4 = true;
        }

        if (!contieneMapa4)
        {
            buildScenes.Add(new EditorBuildSettingsScene(mapa4Path, true));
            EditorBuildSettings.scenes = buildScenes.ToArray();
        }

        Debug.Log("¡Mapa 4 Generado Exitosamente! Se agrego al Build, es ROJO oscuro (Infernal), mas rapido, y tiene el doble de patos.");
    }
}
 
