using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;

public class GeneradorMapa4DesdeCero
{
    [MenuItem("DuckHunt/Generar Mapa 4 (NUEVO DESDE CERO)")]
    public static void Generar()
    {
        string mapa2Path = "Assets/DuckHunt/00_Scene/Mapa2.unity";
        string mapa4Path = "Assets/DuckHunt/00_Scene/Mapa4.unity";

        Scene mapa2 = EditorSceneManager.OpenScene(mapa2Path);
        
        GameObject playerRig = null;
        GameObject enemySpawnerTemplate = null;
        GameObject powerUpSpawnerTemplate = null;
        
        foreach (GameObject obj in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            if (obj.scene != mapa2) continue;
            
            if (playerRig == null && obj.GetComponent<VRAutoForwardMover>() != null) playerRig = obj;
            if (enemySpawnerTemplate == null && obj.GetComponent<EnemySpawner>() != null) enemySpawnerTemplate = obj;
            if (powerUpSpawnerTemplate == null && obj.GetComponent<PowerUpSpawner>() != null) powerUpSpawnerTemplate = obj;
        }

        if (playerRig == null) { Debug.LogError("No se encontro el jugador"); return; }

        VRAutoForwardMover mover2 = playerRig.GetComponent<VRAutoForwardMover>();
        mover2.siguienteEscena = "Mapa4";
        mover2.esNivelFinal = false;
        EditorSceneManager.SaveScene(mapa2);

        string tempDir = "Assets/TempGen/";
        if (!System.IO.Directory.Exists(tempDir)) System.IO.Directory.CreateDirectory(tempDir);
        
        GameObject prefabPlayer = PrefabUtility.SaveAsPrefabAsset(playerRig, tempDir + "Player.prefab");
        GameObject prefabEnemySpawner = enemySpawnerTemplate != null ? PrefabUtility.SaveAsPrefabAsset(enemySpawnerTemplate, tempDir + "ESpawner.prefab") : null;
        GameObject prefabPowerSpawner = powerUpSpawnerTemplate != null ? PrefabUtility.SaveAsPrefabAsset(powerUpSpawnerTemplate, tempDir + "PSpawner.prefab") : null;

        GameObject prefabPistola = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/DuckHunt/03_Prefabs/pistola.prefab");
        GameObject prefabRifle = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/DuckHunt/03_Prefabs/rifle de caza.prefab");
        GameObject prefabLanza = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/DuckHunt/03_Prefabs/lanzagranadas.prefab");

        Scene mapa4 = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        mapa4.name = "Mapa4";
        
        // Estetica mucho mas detallada
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.2f, 0.05f, 0.2f); // Morado/Rojo oscuro
        RenderSettings.skybox = null;
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = new Color(0.15f, 0.0f, 0.1f);
        RenderSettings.fogDensity = 0.015f;

        GameObject luzObj = new GameObject("SolCibernetico");
        Light luz = luzObj.AddComponent<Light>();
        luz.type = LightType.Directional;
        luz.color = new Color(1f, 0.2f, 0.2f);
        luz.intensity = 1.2f;
        luzObj.transform.rotation = Quaternion.Euler(25f, 60f, 0f);

        GameObject trackRoot = new GameObject("Arquitectura_Mapa4");
        
        int numPuntos = 50;
        float segmentLength = 20f;
        List<Vector3> pathPoints = new List<Vector3>();
        
        Material matSuelo = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
        matSuelo.color = new Color(0.05f, 0.05f, 0.05f);
        matSuelo.SetFloat("_Metallic", 0.8f);
        matSuelo.SetFloat("_Glossiness", 0.7f); // Suelo reflectante

        Material matBordes = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
        matBordes.color = new Color(0.0f, 0.0f, 0.0f);
        matBordes.EnableKeyword("_EMISSION");
        matBordes.SetColor("_EmissionColor", new Color(0.8f, 0.0f, 0.2f)); // Neon rojo

        Material matMesa = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
        matMesa.color = new Color(0.2f, 0.2f, 0.2f);

        for (int i = 0; i < numPuntos; i++)
        {
            Vector3 currentPos = new Vector3(0, 0, i * segmentLength);
            pathPoints.Add(currentPos);

            // Suelo principal
            GameObject piso = GameObject.CreatePrimitive(PrimitiveType.Cube);
            piso.transform.position = currentPos - new Vector3(0, 1.5f, 0);
            piso.transform.localScale = new Vector3(10f, 1f, segmentLength);
            piso.GetComponent<Renderer>().sharedMaterial = matSuelo;
            piso.transform.SetParent(trackRoot.transform);

            // Bordes de neon
            GameObject bordeIzq = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bordeIzq.transform.position = currentPos + new Vector3(-5.5f, -1.0f, 0);
            bordeIzq.transform.localScale = new Vector3(1f, 0.5f, segmentLength);
            bordeIzq.GetComponent<Renderer>().sharedMaterial = matBordes;
            bordeIzq.transform.SetParent(trackRoot.transform);

            GameObject bordeDer = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bordeDer.transform.position = currentPos + new Vector3(5.5f, -1.0f, 0);
            bordeDer.transform.localScale = new Vector3(1f, 0.5f, segmentLength);
            bordeDer.GetComponent<Renderer>().sharedMaterial = matBordes;
            bordeDer.transform.SetParent(trackRoot.transform);

            // Arcos Gigantes visuales
            if (i % 3 == 0)
            {
                GameObject pIzq = GameObject.CreatePrimitive(PrimitiveType.Cube);
                pIzq.transform.position = currentPos + new Vector3(-6f, 8f, 0);
                pIzq.transform.localScale = new Vector3(2f, 20f, 2f);
                pIzq.GetComponent<Renderer>().sharedMaterial = matSuelo;
                pIzq.transform.SetParent(trackRoot.transform);

                GameObject pDer = GameObject.CreatePrimitive(PrimitiveType.Cube);
                pDer.transform.position = currentPos + new Vector3(6f, 8f, 0);
                pDer.transform.localScale = new Vector3(2f, 20f, 2f);
                pDer.GetComponent<Renderer>().sharedMaterial = matSuelo;
                pDer.transform.SetParent(trackRoot.transform);

                GameObject techo = GameObject.CreatePrimitive(PrimitiveType.Cube);
                techo.transform.position = currentPos + new Vector3(0, 18f, 0);
                techo.transform.localScale = new Vector3(14f, 2f, 2f);
                techo.GetComponent<Renderer>().sharedMaterial = matBordes;
                techo.transform.SetParent(trackRoot.transform);
            }

            // Mesas procedurales
            if (i > 1 && i % 4 == 0)
            {
                // Crear una mesa procedural
                GameObject mesaObj = new GameObject("MesaArmas");
                mesaObj.transform.SetParent(trackRoot.transform);
                mesaObj.transform.position = currentPos + new Vector3((i % 8 == 0) ? -3.5f : 3.5f, -1f, 0);

                GameObject pata = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                pata.transform.SetParent(mesaObj.transform);
                pata.transform.localPosition = new Vector3(0, 0.5f, 0);
                pata.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                pata.GetComponent<Renderer>().sharedMaterial = matSuelo;

                GameObject tapa = GameObject.CreatePrimitive(PrimitiveType.Cube);
                tapa.transform.SetParent(mesaObj.transform);
                tapa.transform.localPosition = new Vector3(0, 1f, 0);
                tapa.transform.localScale = new Vector3(2.5f, 0.1f, 1.5f);
                tapa.GetComponent<Renderer>().sharedMaterial = matMesa;

                // Armas
                if (prefabRifle != null)
                {
                    GameObject r = (GameObject)PrefabUtility.InstantiatePrefab(prefabRifle);
                    r.transform.position = mesaObj.transform.position + Vector3.up * 1.2f + Vector3.forward * 0.2f;
                }
                if (prefabLanza != null)
                {
                    GameObject l = (GameObject)PrefabUtility.InstantiatePrefab(prefabLanza);
                    l.transform.position = mesaObj.transform.position + Vector3.up * 1.2f - Vector3.forward * 0.2f;
                }
            }
        }

        // Jugador
        GameObject instPlayer = (GameObject)PrefabUtility.InstantiatePrefab(prefabPlayer);
        instPlayer.transform.position = pathPoints[0] + Vector3.up * 1f;
        instPlayer.transform.rotation = Quaternion.identity;

        VRAutoForwardMover mover4 = instPlayer.GetComponent<VRAutoForwardMover>();
        mover4.direccion = Vector3.forward;
        mover4.distanciaMaximaZ = numPuntos * segmentLength * 0.95f; 
        mover4.esNivelFinal = true;
        mover4.siguienteEscena = "";
        mover4.velocidad = 5.5f;

        // Limpiar basura del rig original
        foreach (Transform child in instPlayer.transform)
        {
            if (child.name.ToLower().Contains("pato") || child.name.ToLower().Contains("arma") || child.name.ToLower().Contains("mesa"))
                Object.DestroyImmediate(child.gameObject);
        }

        // Spawners (esto reactiva las oleadas de patos y los powerups del mapa 2, pero a la velocidad/distancia nueva)
        if (prefabEnemySpawner != null)
        {
            GameObject eSpawner = (GameObject)PrefabUtility.InstantiatePrefab(prefabEnemySpawner);
            EnemySpawner es = eSpawner.GetComponent<EnemySpawner>();
            if (es != null)
            {
                es.playerTransform = instPlayer.transform;
                es.spawnIntervalMin = 1.0f; // Patos mucho mas frecuentes
                es.spawnIntervalMax = 2.0f;
            }
        }

        if (prefabPowerSpawner != null)
        {
            GameObject pSpawner = (GameObject)PrefabUtility.InstantiatePrefab(prefabPowerSpawner);
            PowerUpSpawner ps = pSpawner.GetComponent<PowerUpSpawner>();
            if (ps != null) ps.playerTransform = instPlayer.transform;
        }

        GameObject musicObj = new GameObject("MusicaDeFondo");
        AudioSource src = musicObj.AddComponent<AudioSource>();
        AudioClip bossMusic = Resources.Load<AudioClip>("Musica/mundo4");
        if (bossMusic == null) bossMusic = Resources.Load<AudioClip>("Musica/mundo3");
        if (bossMusic != null) { src.clip = bossMusic; src.loop = true; src.playOnAwake = true; src.volume = 0.5f; }

        // Unpack temporary prefabs
        foreach (GameObject obj in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            if (PrefabUtility.IsAnyPrefabInstanceRoot(obj))
            {
                string path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(obj);
                if (path != null && path.Contains("TempGen"))
                    PrefabUtility.UnpackPrefabInstance(obj, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            }
        }

        EditorSceneManager.SaveScene(mapa4, mapa4Path);
        AssetDatabase.DeleteAsset(tempDir);

        List<EditorBuildSettingsScene> buildScenes = EditorBuildSettings.scenes.ToList();
        if (!buildScenes.Any(b => b.path == mapa4Path))
        {
            buildScenes.Add(new EditorBuildSettingsScene(mapa4Path, true));
            EditorBuildSettings.scenes = buildScenes.ToArray();
        }

        Debug.Log("MAPA 4 SUPER DETALLADO GENERADO!");
    }
}
 
