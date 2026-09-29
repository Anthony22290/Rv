using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using TMPro;
using System.Collections.Generic;

public class WeaponFixer
{
    public class Config
    {
        public string name;
        public Vector3 scale;
        public Vector3 colCenter;
        public Vector3 colSize;
        public Vector3 attachPos;
        public Vector3 attachRot;
        public Vector3 firePos;
        public Vector3 fireRot;

        public Config(string n, Vector3 s, Vector3 cc, Vector3 cs, Vector3 ap, Vector3 ar, Vector3 fp, Vector3 fr)
        {
            name = n;
            scale = s;
            colCenter = cc;
            colSize = cs;
            attachPos = ap;
            attachRot = ar;
            firePos = fp;
            fireRot = fr;
        }
    }

    [MenuItem("DuckHunt/ARREGLAR TODAS LAS ARMAS (Prefabs y Escenas)")]
    public static void FixAllWeaponsAndScenes()
    {
        FixPrefabs();
        FixAllScenes();
        Debug.Log("=========================================");
        Debug.Log("TODAS LAS ARMAS Y MAPAS ARREGLADOS CON EXITO");
        Debug.Log("=========================================");
    }

    public static void FixPrefabs()
    {
        List<Config> configs = new List<Config>
        {
            // Pistola: ~28cm
            new Config(
                "pistola",
                new Vector3(0.28f, 0.28f, 0.28f),
                new Vector3(-0.05f, 0.35f, 0.0f),
                new Vector3(1.10f, 0.85f, 0.45f),
                new Vector3(-0.35f, 0.22f, 0.0f),
                new Vector3(0f, 90f, 0f),
                new Vector3(0.52f, 0.60f, 0.0f),
                new Vector3(0f, 90f, 0f)
            ),
            // Escopeta: ~65cm
            new Config(
                "escopeta",
                new Vector3(0.65f, 0.65f, 0.65f),
                new Vector3(0.0f, 0.38f, 0.0f),
                new Vector3(1.10f, 0.90f, 0.40f),
                new Vector3(0.15f, 0.22f, 0.0f),
                new Vector3(0f, -90f, 0f),
                new Vector3(-0.52f, 0.68f, 0.0f),
                new Vector3(0f, -90f, 0f)
            ),
            // Rifle de Caza: ~80cm
            new Config(
                "rifle de caza",
                new Vector3(0.80f, 0.80f, 0.80f),
                new Vector3(0.0f, 0.10f, 0.0f),
                new Vector3(0.30f, 0.40f, 1.15f),
                new Vector3(0.0f, 0.10f, -0.20f),
                new Vector3(0f, 0f, 0f),
                new Vector3(0.0f, 0.16f, 0.52f),
                new Vector3(0f, 0f, 0f)
            ),
            // Lanzagranadas: ~60cm
            new Config(
                "lanzagranadas",
                new Vector3(0.60f, 0.60f, 0.60f),
                new Vector3(0.0f, 0.28f, 0.0f),
                new Vector3(0.40f, 0.70f, 1.15f),
                new Vector3(0.0f, 0.20f, -0.20f),
                new Vector3(0f, 0f, 0f),
                new Vector3(0.0f, 0.32f, 0.52f),
                new Vector3(0f, 0f, 0f)
            )
        };

        string prefabsPath = "Assets/DuckHunt/03_Prefabs/";

        foreach (var cfg in configs)
        {
            string path = prefabsPath + cfg.name + ".prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
            {
                Debug.LogWarning("No se encontro prefab: " + path);
                continue;
            }

            GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            instance.transform.localScale = cfg.scale;

            // BoxCollider con tamaño generoso para agarre fácil en VR
            BoxCollider box = instance.GetComponent<BoxCollider>();
            if (box == null) box = instance.AddComponent<BoxCollider>();
            box.center = cfg.colCenter;
            box.size = cfg.colSize;
            box.isTrigger = false;

            // Rigidbody
            Rigidbody rb = instance.GetComponent<Rigidbody>();
            if (rb == null) rb = instance.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = true;

            // PuntoAgarre
            Transform attach = instance.transform.Find("PuntoAgarre");
            if (attach == null)
            {
                GameObject aObj = new GameObject("PuntoAgarre");
                aObj.transform.SetParent(instance.transform, false);
                attach = aObj.transform;
            }
            attach.localPosition = cfg.attachPos;
            attach.localEulerAngles = cfg.attachRot;

            // FirePoint
            Transform fp = instance.transform.Find("FirePoint");
            if (fp == null)
            {
                GameObject fpObj = new GameObject("FirePoint");
                fpObj.transform.SetParent(instance.transform, false);
                fp = fpObj.transform;
            }
            fp.localPosition = cfg.firePos;
            fp.localEulerAngles = cfg.fireRot;

            // XRGrabInteractable
            XRGrabInteractable grab = instance.GetComponent<XRGrabInteractable>();
            if (grab == null) grab = instance.AddComponent<XRGrabInteractable>();
            grab.attachTransform = attach;
            grab.retainTransformParent = false;
            grab.throwOnDetach = true;
            grab.movementType = XRBaseInteractable.MovementType.Instantaneous;

            // PistolaVR
            PistolaVR pvr = instance.GetComponent<PistolaVR>();
            if (pvr != null)
            {
                pvr.puntoDeDisparo = fp;
            }

            PrefabUtility.SaveAsPrefabAsset(instance, path);
            Object.DestroyImmediate(instance);
            Debug.Log($"[WeaponFixer] Prefab actualizado: {cfg.name} (Escala={cfg.scale})");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    public static void FixAllScenes()
    {
        string[] scenes = {
            "Assets/DuckHunt/00_Scene/BasicScene.unity",
            "Assets/DuckHunt/00_Scene/MapaDesierto.unity",
            "Assets/DuckHunt/00_Scene/Mapa2.unity",
            "Assets/DuckHunt/00_Scene/Mapa3.unity",
            "Assets/DuckHunt/00_Scene/Mapa4.unity",
            "Assets/DuckHunt/00_Scene/pruebas armas.unity"
        };

        foreach (string scenePath in scenes)
        {
            if (!System.IO.File.Exists(scenePath)) continue;

            Scene s = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            Debug.Log($"[WeaponFixer] Procesando escena: {s.name}");

            // 1. Destruir mesas viejas desalineadas si existen
            GameObject oldTables = GameObject.Find("[Weapon_Tables]");
            if (oldTables != null)
            {
                Object.DestroyImmediate(oldTables);
            }

            // 2. Destruir armas sueltas viejas en la escena para que no queden duplicadas o microscopicas
            GameObject[] allObjects = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            foreach (var go in allObjects)
            {
                if (go == null || go.scene != s) continue;
                string lower = go.name.ToLower();
                if (lower.Contains("mesa_") || (lower.StartsWith("mesa") && (lower.Contains("escopeta") || lower.Contains("pistola") || lower.Contains("rifle") || lower.Contains("lanzagranadas"))))
                {
                    Object.DestroyImmediate(go);
                    continue;
                }
                if (go.GetComponent<PistolaVR>() != null && go.transform.parent == null && (lower.Contains("escopeta") || lower.Contains("rifle") || lower.Contains("lanzagranadas")))
                {
                    Object.DestroyImmediate(go);
                    continue;
                }
            }

            // 3. Crear mesas de armas limpias y alcanzables (X=±0.85m)
            CrearMesasParaEscena(s.name);

            // 4. Actualizar o asegurar pistola inicial del jugador
            AsegurarPistolaInicial();

            EditorSceneManager.MarkSceneDirty(s);
            EditorSceneManager.SaveScene(s);
            Debug.Log($"[WeaponFixer] Escena {s.name} guardada correctamente.");
        }
    }

    private static void CrearMesasParaEscena(string sceneName)
    {
        if (sceneName == "BasicScene" || sceneName == "pruebas armas") return;

        GameObject tablesRoot = new GameObject("[Weapon_Tables]");
        tablesRoot.transform.position = Vector3.zero;

        Material woodMat = Resources.Load<Material>("M_Wood") ?? ObtenerMaterialColor(new Color(0.45f, 0.30f, 0.18f));
        Material goldMat = Resources.Load<Material>("M_GoldOre") ?? ObtenerMaterialColor(new Color(1f, 0.85f, 0.2f));

        if (sceneName == "Mapa4")
        {
            // Para Mapa4 (nivel largo y rápido), colocamos mesas a distancias intermedias alcanzables
            float[] distancias = { 40f, 100f, 180f, 260f, 350f, 450f, 550f, 700f, 850f };
            string[] armas = {
                "Assets/DuckHunt/03_Prefabs/pistola.prefab",
                "Assets/DuckHunt/03_Prefabs/escopeta.prefab",
                "Assets/DuckHunt/03_Prefabs/rifle de caza.prefab",
                "Assets/DuckHunt/03_Prefabs/lanzagranadas.prefab",
                "Assets/DuckHunt/03_Prefabs/escopeta.prefab",
                "Assets/DuckHunt/03_Prefabs/rifle de caza.prefab",
                "Assets/DuckHunt/03_Prefabs/lanzagranadas.prefab",
                "Assets/DuckHunt/03_Prefabs/rifle de caza.prefab",
                "Assets/DuckHunt/03_Prefabs/lanzagranadas.prefab"
            };
            string[] nombres = {
                "🔫 PISTOLA",
                "💥 ESCOPETA",
                "🎯 RIFLE DE CAZA",
                "🚀 LANZAGRANADAS RPG",
                "⚡ ESCOPETA POTENCIADA",
                "🎯 RIFLE DE FRANCOTIRADOR",
                "🚀 BAZUKA PESADA",
                "🎯 RIFLE ELITE",
                "🚀 LANZAGRANADAS FINAL"
            };

            for (int i = 0; i < distancias.Length; i++)
            {
                float side = (i % 2 == 0) ? 0.85f : -0.85f;
                CrearMesa(tablesRoot.transform, new Vector3(side, 0f, distancias[i]), nombres[i], armas[i], woodMat, goldMat);
            }
        }
        else if (sceneName == "Mapa3")
        {
            // Mapa 3: 5 mesas estándar a X=±0.85m (sin escopetas normales si es temático, o con rifle y lanzagranadas)
            CrearMesa(tablesRoot.transform, new Vector3(0.85f, 0f, 18f), "🔫 PISTOLA 3D", "Assets/DuckHunt/03_Prefabs/pistola.prefab", woodMat, goldMat);
            CrearMesa(tablesRoot.transform, new Vector3(-0.85f, 0f, 65f), "🎯 RIFLE DE CAZA", "Assets/DuckHunt/03_Prefabs/rifle de caza.prefab", woodMat, goldMat);
            CrearMesa(tablesRoot.transform, new Vector3(0.85f, 0f, 115f), "🚀 LANZAGRANADAS RPG", "Assets/DuckHunt/03_Prefabs/lanzagranadas.prefab", woodMat, goldMat);
            CrearMesa(tablesRoot.transform, new Vector3(-0.85f, 0f, 165f), "🎯 RIFLE PESADO", "Assets/DuckHunt/03_Prefabs/rifle de caza.prefab", woodMat, goldMat);
            CrearMesa(tablesRoot.transform, new Vector3(0.85f, 0f, 205f), "🚀 LANZAGRANADAS FINAL", "Assets/DuckHunt/03_Prefabs/lanzagranadas.prefab", woodMat, goldMat);
        }
        else
        {
            // MapaDesierto y Mapa2
            CrearMesa(tablesRoot.transform, new Vector3(0.85f, 0f, 18f), "🔫 PISTOLA 3D", "Assets/DuckHunt/03_Prefabs/pistola.prefab", woodMat, goldMat);
            CrearMesa(tablesRoot.transform, new Vector3(-0.85f, 0f, 65f), "💥 ESCOPETA", "Assets/DuckHunt/03_Prefabs/escopeta.prefab", woodMat, goldMat);
            CrearMesa(tablesRoot.transform, new Vector3(0.85f, 0f, 115f), "🎯 RIFLE DE CAZA", "Assets/DuckHunt/03_Prefabs/rifle de caza.prefab", woodMat, goldMat);
            CrearMesa(tablesRoot.transform, new Vector3(-0.85f, 0f, 165f), "🚀 LANZAGRANADAS RPG", "Assets/DuckHunt/03_Prefabs/lanzagranadas.prefab", woodMat, goldMat);
            CrearMesa(tablesRoot.transform, new Vector3(0.85f, 0f, 205f), "⚡ ESCOPETA POTENCIADA", "Assets/DuckHunt/03_Prefabs/escopeta.prefab", woodMat, goldMat);
        }
    }

    private static void CrearMesa(Transform parent, Vector3 posicion, string nombreArma, string prefabPath, Material matMadera, Material matDorado)
    {
        GameObject tableGroup = new GameObject($"Mesa_{nombreArma.Replace(" ", "_")}");
        tableGroup.transform.SetParent(parent);
        tableGroup.transform.position = posicion;

        float rotationY = (posicion.x > 0) ? -20f : 20f;
        tableGroup.transform.rotation = Quaternion.Euler(0, rotationY, 0);

        // Tablero de mesa
        GameObject tableTop = GameObject.CreatePrimitive(PrimitiveType.Cube);
        tableTop.name = "TableTop";
        tableTop.transform.SetParent(tableGroup.transform, false);
        tableTop.transform.localPosition = new Vector3(0, 0.82f, 0);
        tableTop.transform.localScale = new Vector3(1.10f, 0.08f, 0.60f);
        tableTop.GetComponent<MeshRenderer>().sharedMaterial = matMadera;

        // 4 Patas
        Vector3[] legOffsets = new Vector3[] {
            new Vector3(-0.45f, 0.39f, -0.22f),
            new Vector3(0.45f, 0.39f, -0.22f),
            new Vector3(-0.45f, 0.39f, 0.22f),
            new Vector3(0.45f, 0.39f, 0.22f)
        };
        foreach (var legPos in legOffsets)
        {
            GameObject leg = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            leg.name = "Leg";
            leg.transform.SetParent(tableGroup.transform, false);
            leg.transform.localPosition = legPos;
            leg.transform.localScale = new Vector3(0.06f, 0.40f, 0.06f);
            leg.GetComponent<MeshRenderer>().sharedMaterial = matMadera;
        }

        // Tapete dorado
        GameObject matObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        matObj.name = "WeaponPad";
        matObj.transform.SetParent(tableGroup.transform, false);
        matObj.transform.localPosition = new Vector3(0, 0.87f, 0);
        matObj.transform.localScale = new Vector3(0.90f, 0.02f, 0.45f);
        matObj.GetComponent<MeshRenderer>().sharedMaterial = matDorado;

        // Cartel 3D
        GameObject textObj = new GameObject("Label_Arma");
        textObj.transform.SetParent(tableGroup.transform, false);
        textObj.transform.localPosition = new Vector3(0, 1.25f, 0);
        textObj.transform.localScale = Vector3.one * 0.02f;

        TextMeshPro tmp = textObj.AddComponent<TextMeshPro>();
        tmp.text = nombreArma;
        tmp.fontSize = 22;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = new Color(1f, 0.95f, 0.3f);

        // Instanciar arma en la mesa
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab != null)
        {
            GameObject weaponInstance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, tableGroup.transform);
            weaponInstance.transform.localPosition = new Vector3(0f, 0.95f, 0f);
            weaponInstance.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);

            Rigidbody rb = weaponInstance.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true;
        }
    }

    private static void AsegurarPistolaInicial()
    {
        VRAutoForwardMover mover = Object.FindAnyObjectByType<VRAutoForwardMover>();
        if (mover == null) return;

        // Buscar si ya hay una pistola cerca del jugador
        PistolaVR[] pistolas = Object.FindObjectsByType<PistolaVR>(FindObjectsSortMode.None);
        PistolaVR pistolaInicial = null;
        foreach (var p in pistolas)
        {
            if (p.gameObject.name.ToLower().Contains("pistola") && Vector3.Distance(p.transform.position, mover.transform.position) < 3.5f)
            {
                pistolaInicial = p;
                break;
            }
        }

        GameObject prefabPistola = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/DuckHunt/03_Prefabs/pistola.prefab");
        if (prefabPistola == null) return;

        if (pistolaInicial != null)
        {
            // Actualizar escala y posición
            pistolaInicial.transform.localScale = new Vector3(0.28f, 0.28f, 0.28f);
            pistolaInicial.transform.position = mover.transform.position + Vector3.up * 1.0f + mover.transform.forward * 0.45f;
            Rigidbody rb = pistolaInicial.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true;
        }
        else
        {
            GameObject p = (GameObject)PrefabUtility.InstantiatePrefab(prefabPistola);
            p.transform.position = mover.transform.position + Vector3.up * 1.0f + mover.transform.forward * 0.45f;
            Rigidbody rb = p.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true;
        }
    }

    private static Material ObtenerMaterialColor(Color color)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        Material m = new Material(shader);
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", color);
        else m.color = color;
        return m;
    }
}
