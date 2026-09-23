using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;

public class DesertMapGenerator : EditorWindow
{
    [MenuItem("DuckHunt/Generar Mapa Desierto (En Escena Actual)")]
    public static void GenerateDesertMapMenu()
    {
        GenerateDesertMap();
    }

    [MenuItem("DuckHunt/Crear Nueva Escena Desierto (MapaDesierto.unity)")]
    public static void CreateDesertScene()
    {
        string scenePath = "Assets/DuckHunt/00_Scene/MapaDesierto.unity";
        string baseScenePath = "Assets/DuckHunt/00_Scene/BasicScene.unity";

        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            return;
        }

        // Si existe BasicScene.unity, usarla como plantilla para conservar el XR Origin, Pistola y UI intactos
        if (File.Exists(baseScenePath) && !File.Exists(scenePath))
        {
            AssetDatabase.CopyAsset(baseScenePath, scenePath);
            AssetDatabase.Refresh();
        }

        Scene desertScene;
        if (File.Exists(scenePath))
        {
            desertScene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        }
        else
        {
            desertScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            EditorSceneManager.SaveScene(desertScene, scenePath);
        }

        // Generar mapa del desierto y configurar spawner
        GenerateDesertMap();
        EnemySetupHelper.CreateEnemyAssetsAndPrefab();

        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        Debug.Log($"[DesertMapGenerator] ¡Escena completa de Desierto {scenePath} generada y guardada con éxito!");
    }

    public static void GenerateDesertMap()
    {
        // 1. Asegurar carpeta de materiales
        if (!AssetDatabase.IsValidFolder("Assets/DuckHunt/05_Materials"))
        {
            if (!AssetDatabase.IsValidFolder("Assets/DuckHunt"))
            {
                AssetDatabase.CreateFolder("Assets", "DuckHunt");
            }
            AssetDatabase.CreateFolder("Assets/DuckHunt", "05_Materials");
        }

        Shader litShader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");

        // Crear/Obtener Materiales del Desierto
        Material matSand = GetOrCreateMaterial("Assets/DuckHunt/05_Materials/M_DesertSand.mat", litShader, new Color(0.88f, 0.74f, 0.48f), 0.05f);
        Material matTrack = GetOrCreateMaterial("Assets/DuckHunt/05_Materials/M_DesertTrack.mat", litShader, new Color(0.72f, 0.55f, 0.35f), 0.02f);
        Material matRedSandstone = GetOrCreateMaterial("Assets/DuckHunt/05_Materials/M_RedSandstone.mat", litShader, new Color(0.76f, 0.38f, 0.22f), 0.12f);
        Material matDarkSandstone = GetOrCreateMaterial("Assets/DuckHunt/05_Materials/M_DarkSandstone.mat", litShader, new Color(0.55f, 0.25f, 0.15f), 0.08f);
        Material matCactusGreen = GetOrCreateMaterial("Assets/DuckHunt/05_Materials/M_CactusGreen.mat", litShader, new Color(0.24f, 0.45f, 0.20f), 0.08f);
        Material matCactusFlower = GetOrCreateMaterial("Assets/DuckHunt/05_Materials/M_CactusFlower.mat", litShader, new Color(0.96f, 0.78f, 0.15f), 0.0f);
        Material matDeadWood = GetOrCreateMaterial("Assets/DuckHunt/05_Materials/M_DesertDeadWood.mat", litShader, new Color(0.48f, 0.39f, 0.29f), 0.1f);
        Material matBone = GetOrCreateMaterial("Assets/DuckHunt/05_Materials/M_BoneWhite.mat", litShader, new Color(0.92f, 0.89f, 0.82f), 0.15f);
        Material matDryBush = GetOrCreateMaterial("Assets/DuckHunt/05_Materials/M_DryBush.mat", litShader, new Color(0.58f, 0.47f, 0.28f), 0.0f);
        Material matOasisWater = GetOrCreateMaterial("Assets/DuckHunt/05_Materials/M_OasisWater.mat", litShader, new Color(0.12f, 0.65f, 0.85f), 0.85f);
        Material matPalmLeaf = GetOrCreateMaterial("Assets/DuckHunt/05_Materials/M_PalmLeaf.mat", litShader, new Color(0.18f, 0.52f, 0.14f), 0.05f);
        Material matMetal = GetOrCreateMaterial("Assets/DuckHunt/05_Materials/M_DesertMetal.mat", litShader, new Color(0.30f, 0.28f, 0.26f), 0.5f);
        Material matTNT = GetOrCreateMaterial("Assets/DuckHunt/05_Materials/M_TNT_Red.mat", litShader, new Color(0.82f, 0.18f, 0.12f), 0.2f);
        Material matGold = GetOrCreateMaterial("Assets/DuckHunt/05_Materials/M_GoldOre.mat", litShader, new Color(0.98f, 0.80f, 0.20f), 0.7f);
        Material matDistantMesa = GetOrCreateMaterial("Assets/DuckHunt/05_Materials/M_DistantMesa.mat", litShader, new Color(0.68f, 0.40f, 0.28f), 0.0f);
        Material matCampfireStone = GetOrCreateMaterial("Assets/DuckHunt/05_Materials/M_CampfireStone.mat", litShader, new Color(0.35f, 0.32f, 0.30f), 0.1f);
        Material matCharcoal = GetOrCreateMaterial("Assets/DuckHunt/05_Materials/M_Charcoal.mat", litShader, new Color(0.15f, 0.12f, 0.12f), 0.05f);

        // 2. Limpiar mapa anterior si existe
        GameObject existingMap = GameObject.Find("[Desert_Map]");
        if (existingMap != null)
        {
            Undo.DestroyObjectImmediate(existingMap);
        }

        // Desactivar o destruir mapa de pradera si existe en la misma escena
        GameObject prairieMap = GameObject.Find("[Prairie_Map]");
        if (prairieMap != null)
        {
            prairieMap.SetActive(false);
        }

        // Desactivar Plane por defecto
        GameObject oldPlane = GameObject.Find("Plane");
        if (oldPlane != null && oldPlane.transform.parent == null)
        {
            oldPlane.SetActive(false);
        }

        // 3. Crear nodo raíz del Desierto
        GameObject mapRoot = new GameObject("[Desert_Map]");
        Undo.RegisterCreatedObjectUndo(mapRoot, "Create Desert Map");

        GameObject groundGroup = CreateSubGroup(mapRoot, "01_Terrain_And_Dunes");
        GameObject canyonGroup = CreateSubGroup(mapRoot, "02_Canyon_Walls_And_Arches");
        GameObject cactiGroup = CreateSubGroup(mapRoot, "03_Cacti_And_Vegetation");
        GameObject oasisGroup = CreateSubGroup(mapRoot, "04_Oasis_Area");
        GameObject propsGroup = CreateSubGroup(mapRoot, "05_Western_Props_And_Fossils");
        GameObject horizonGroup = CreateSubGroup(mapRoot, "06_Distant_Mesas_And_Horizons");

        Random.InitState(777);

        // --- A. PISTA CENTRAL Y DUNAS DE ARENA (Longitud 260m) ---
        for (int z = -10; z < 260; z += 20)
        {
            // Pista de arena apisonada central (ancho 5.5m)
            GameObject pathSeg = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pathSeg.name = $"DesertTrack_Segment_{z}";
            pathSeg.transform.parent = groundGroup.transform;
            pathSeg.transform.position = new Vector3(0, -0.05f, z + 10);
            pathSeg.transform.localScale = new Vector3(5.5f, 0.1f, 20.2f);
            pathSeg.GetComponent<MeshRenderer>().sharedMaterial = matTrack;

            // Duna Izquierda
            GameObject sandLeft = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sandLeft.name = $"Sand_Left_{z}";
            sandLeft.transform.parent = groundGroup.transform;
            sandLeft.transform.position = new Vector3(-25f, -0.06f, z + 10);
            sandLeft.transform.localScale = new Vector3(45f, 0.1f, 20.2f);
            sandLeft.GetComponent<MeshRenderer>().sharedMaterial = matSand;

            // Duna Derecha
            GameObject sandRight = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sandRight.name = $"Sand_Right_{z}";
            sandRight.transform.parent = groundGroup.transform;
            sandRight.transform.position = new Vector3(25f, -0.06f, z + 10);
            sandRight.transform.localScale = new Vector3(45f, 0.1f, 20.2f);
            sandRight.GetComponent<MeshRenderer>().sharedMaterial = matSand;

            // Ondulaciones suaves de dunas
            GameObject duneL = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            duneL.name = $"DuneWave_L_{z}";
            duneL.transform.parent = groundGroup.transform;
            duneL.transform.position = new Vector3(Random.Range(-18f, -10f), -0.8f, z + Random.Range(3f, 15f));
            duneL.transform.localScale = new Vector3(Random.Range(12f, 18f), 2.2f, Random.Range(15f, 25f));
            duneL.GetComponent<MeshRenderer>().sharedMaterial = matSand;

            GameObject duneR = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            duneR.name = $"DuneWave_R_{z}";
            duneR.transform.parent = groundGroup.transform;
            duneR.transform.position = new Vector3(Random.Range(10f, 18f), -0.8f, z + Random.Range(3f, 15f));
            duneR.transform.localScale = new Vector3(Random.Range(12f, 18f), 2.2f, Random.Range(15f, 25f));
            duneR.GetComponent<MeshRenderer>().sharedMaterial = matSand;
        }

        // --- B. PAREDES DE CAÑÓN ROJO Y FORMACIONES ROCOSAS ---
        for (int z = 0; z < 250; z += 18)
        {
            // Saltear zona del oasis (alrededor de z=110 a z=150) en el lado derecho para abrir vista
            bool isOasisZone = (z >= 110 && z <= 150);

            // Riscos Izquierda
            CreateCanyonCliff(canyonGroup.transform, new Vector3(Random.Range(-32f, -22f), 0, z + Random.Range(-3f, 3f)), matRedSandstone, matDarkSandstone);

            // Riscos Derecha
            if (!isOasisZone)
            {
                CreateCanyonCliff(canyonGroup.transform, new Vector3(Random.Range(22f, 32f), 0, z + Random.Range(-3f, 3f)), matRedSandstone, matDarkSandstone);
            }
            else
            {
                // Riscos más retirados para dar espacio al oasis
                CreateCanyonCliff(canyonGroup.transform, new Vector3(Random.Range(35f, 42f), 0, z + Random.Range(-3f, 3f)), matRedSandstone, matDarkSandstone);
            }
        }

        // --- GRAN ARCO NATURAL DE PIEDRA ARENISCA (z = 90m y z = 190m) ---
        CreateNaturalSandstoneArch(canyonGroup.transform, new Vector3(0, 0, 90f), matRedSandstone, matDarkSandstone);
        CreateNaturalSandstoneArch(canyonGroup.transform, new Vector3(0, 0, 190f), matRedSandstone, matDarkSandstone);

        // --- C. FLORA DEL DESIERTO (CACTUS SAGUARO, NOPALES, CACTUS BARRIL, MATORRALES) ---
        for (int z = 6; z < 245; z += 10)
        {
            if (z >= 115 && z <= 145) continue; // zona de lago oasis

            // Cactus Saguaro a la izquierda
            if (Random.value > 0.25f)
            {
                Vector3 cactusPosL = new Vector3(Random.Range(-18f, -4.5f), 0, z + Random.Range(-3f, 3f));
                CreateSaguaroCactus(cactiGroup.transform, cactusPosL, matCactusGreen, matCactusFlower);
            }

            // Cactus Saguaro a la derecha
            if (Random.value > 0.25f)
            {
                Vector3 cactusPosR = new Vector3(Random.Range(4.5f, 18f), 0, z + Random.Range(-3f, 3f));
                CreateSaguaroCactus(cactiGroup.transform, cactusPosR, matCactusGreen, matCactusFlower);
            }

            // Cactus Nopal / Prickly Pear
            if (Random.value > 0.4f)
            {
                Vector3 nopalPos = new Vector3((Random.value > 0.5f ? 1 : -1) * Random.Range(3.8f, 10f), 0, z + Random.Range(2f, 6f));
                CreatePricklyPearCactus(cactiGroup.transform, nopalPos, matCactusGreen, matCactusFlower);
            }

            // Cactus Barril con flores
            if (Random.value > 0.4f)
            {
                Vector3 barrelCactusPos = new Vector3((Random.value > 0.5f ? 1 : -1) * Random.Range(3.5f, 8f), 0, z + Random.Range(-4f, 4f));
                CreateBarrelCactus(cactiGroup.transform, barrelCactusPos, matCactusGreen, matCactusFlower);
            }

            // Arbustos secos / Rodadoras
            Vector3 bushPos = new Vector3(Random.Range(-12f, 12f), 0, z + Random.Range(1f, 5f));
            if (Mathf.Abs(bushPos.x) > 3.2f)
            {
                CreateTumbleweedBush(cactiGroup.transform, bushPos, matDryBush);
            }
        }

        // --- D. ZONA DEL OASIS DESÉRTICO (z = 130m, lado derecho) ---
        CreateDesertOasis(oasisGroup.transform, new Vector3(14f, 0, 130f), matOasisWater, matSand, matPalmLeaf, matDeadWood);

        // --- E. ACCESORIOS DEL OESTE, FÓSILES, MINAS Y CAMPAMENTOS ---
        // 1. Cráneo y costillas fósiles en la arena
        CreateFossilSkeleton(propsGroup.transform, new Vector3(-4.8f, 0.1f, 25f), matBone);
        CreateFossilSkeleton(propsGroup.transform, new Vector3(5.2f, 0.1f, 160f), matBone);

        // 2. Vagoneta Minera con Oro
        CreateMiningCart(propsGroup.transform, new Vector3(4.2f, 0, 50f), matDeadWood, matMetal, matGold);
        CreateMiningCart(propsGroup.transform, new Vector3(-4.5f, 0, 205f), matDeadWood, matMetal, matGold);

        // 3. Cajas de Dinamita TNT y Barriles
        CreateTNTStack(propsGroup.transform, new Vector3(-3.8f, 0, 52f), matTNT, matDeadWood);
        CreateTNTStack(propsGroup.transform, new Vector3(4.0f, 0, 208f), matTNT, matDeadWood);

        // 4. Fogata de campamento abandonada
        CreateCampfire(propsGroup.transform, new Vector3(-4.5f, 0, 115f), matCampfireStone, matCharcoal);

        // 5. Ruedas de Carromato semienterradas en las dunas
        CreateWagonWheel(propsGroup.transform, new Vector3(3.6f, 0.3f, 15f), matDeadWood, matMetal);
        CreateWagonWheel(propsGroup.transform, new Vector3(-4.2f, 0.3f, 105f), matDeadWood, matMetal);
        CreateWagonWheel(propsGroup.transform, new Vector3(4.8f, 0.3f, 230f), matDeadWood, matMetal);

        // 6. Señales del Desierto / Minas
        CreateDesertSign(propsGroup.transform, new Vector3(2.8f, 0, 5f), "CANYON PASS", matDeadWood);
        CreateDesertSign(propsGroup.transform, new Vector3(-3.0f, 0, 80f), "GOLD MINE ->", matDeadWood);
        CreateDesertSign(propsGroup.transform, new Vector3(2.8f, 0, 180f), "DANGER: GULCH", matDeadWood);

        // 7. Rocas y formaciones menores a lo largo del sendero
        for (int z = 12; z < 240; z += 16)
        {
            Vector3 rockPos = new Vector3((Random.value > 0.5f ? 1 : -1) * Random.Range(3.8f, 15f), 0, z);
            CreateDesertBoulders(propsGroup.transform, rockPos, matRedSandstone, matDarkSandstone);
        }

        // --- F. MESETAS MONUMENTALES DEL HORIZONTE (Estilo Monument Valley) ---
        for (int x = -100; x <= 100; x += 40)
        {
            GameObject mesa = GameObject.CreatePrimitive(PrimitiveType.Cube);
            mesa.name = $"Distant_Mesa_{x}";
            mesa.transform.parent = horizonGroup.transform;
            float mesaH = Random.Range(30f, 55f);
            float mesaW = Random.Range(35f, 60f);
            mesa.transform.position = new Vector3(x + Random.Range(-10f, 10f), mesaH * 0.5f - 2f, 300f + Random.Range(0, 40f));
            mesa.transform.localScale = new Vector3(mesaW, mesaH, Random.Range(30f, 50f));
            mesa.GetComponent<MeshRenderer>().sharedMaterial = matDistantMesa;

            // Corona superior
            GameObject mesaTop = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            mesaTop.name = $"Mesa_Spire_{x}";
            mesaTop.transform.parent = mesa.transform;
            mesaTop.transform.localPosition = new Vector3(0, 0.65f, 0);
            mesaTop.transform.localScale = new Vector3(0.5f, 0.4f, 0.5f);
            mesaTop.GetComponent<MeshRenderer>().sharedMaterial = matRedSandstone;
        }

        // --- G. ILUMINACIÓN Y NIEBLA DE POLVO DESÉRTICA ---
        Light dirLight = Object.FindAnyObjectByType<Light>();
        if (dirLight != null && dirLight.type == LightType.Directional)
        {
            dirLight.color = new Color(1.0f, 0.88f, 0.68f);
            dirLight.intensity = 1.45f;
            dirLight.transform.rotation = Quaternion.Euler(46f, -35f, 0f);
            dirLight.shadows = LightShadows.Soft;
        }

        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Exponential;
        RenderSettings.fogColor = new Color(0.90f, 0.74f, 0.52f);
        RenderSettings.fogDensity = 0.0055f;

        // --- H. CONFIGURACIÓN VR EN XR ORIGIN ---
        GameObject xrOrigin = GameObject.Find("XR Origin (XR Rig)");
        if (xrOrigin != null)
        {
            VRAutoForwardMover mover = xrOrigin.GetComponent<VRAutoForwardMover>();
            if (mover == null)
            {
                mover = xrOrigin.AddComponent<VRAutoForwardMover>();
            }
            mover.velocidad = 3.2f;
            mover.avanzar = true;
            mover.distanciaMaximaZ = 245f;
            mover.reiniciarAlFinal = true;
            xrOrigin.transform.position = new Vector3(0, 0, 0);
        }

        // Configurar EnemySpawner en la escena
        EnemySpawner spawner = Object.FindAnyObjectByType<EnemySpawner>();
        if (spawner != null)
        {
            spawner.spawnDistanceAheadMin = 15f;
            spawner.spawnDistanceAheadMax = 24f;
            spawner.spawnSideDistance = 11f;
            spawner.minSpawnHeight = 2.2f;
            spawner.maxSpawnHeight = 6.0f;
            EditorUtility.SetDirty(spawner);
        }

        // Marcar escena como modificada
        EditorUtility.SetDirty(mapRoot);
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log("¡Mapa del Desierto (Desert Canyon) generado con éxito!");
    }

    private static GameObject CreateSubGroup(GameObject parent, string name)
    {
        GameObject go = new GameObject(name);
        go.transform.parent = parent.transform;
        go.transform.localPosition = Vector3.zero;
        return go;
    }

    private static Material GetOrCreateMaterial(string path, Shader shader, Color color, float smoothness)
    {
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            mat = new Material(shader);
            mat.color = color;
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
            if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", smoothness);
            AssetDatabase.CreateAsset(mat, path);
        }
        else
        {
            mat.shader = shader;
            mat.color = color;
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
            if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", smoothness);
            EditorUtility.SetDirty(mat);
        }
        return mat;
    }

    // --- GENERADOR DE PAREDES DE CAÑÓN ESCALONADAS ---
    private static void CreateCanyonCliff(Transform parent, Vector3 pos, Material matRed, Material matDark)
    {
        GameObject cliff = new GameObject("Canyon_Cliff");
        cliff.transform.parent = parent;
        cliff.transform.position = pos;

        int layers = Random.Range(3, 5);
        float currentHeight = 0f;
        float baseWidth = Random.Range(18f, 26f);

        for (int i = 0; i < layers; i++)
        {
            GameObject layer = GameObject.CreatePrimitive(PrimitiveType.Cube);
            layer.transform.parent = cliff.transform;
            float layerH = Random.Range(3.5f, 6.5f);
            float layerW = baseWidth * (1f - (i * 0.12f));
            float layerD = Random.Range(16f, 22f);

            layer.transform.localPosition = new Vector3(Random.Range(-1.5f, 1.5f), currentHeight + layerH * 0.5f, Random.Range(-1f, 1f));
            layer.transform.localScale = new Vector3(layerW, layerH, layerD);
            layer.transform.localRotation = Quaternion.Euler(Random.Range(-3f, 3f), Random.Range(-10f, 10f), Random.Range(-3f, 3f));
            layer.GetComponent<MeshRenderer>().sharedMaterial = (i % 2 == 0) ? matRed : matDark;

            currentHeight += layerH * 0.85f;
        }

        if (Random.value > 0.4f)
        {
            GameObject spire = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            spire.transform.parent = cliff.transform;
            spire.transform.localPosition = new Vector3(Random.Range(-2f, 2f), currentHeight + 2.5f, Random.Range(-2f, 2f));
            spire.transform.localScale = new Vector3(Random.Range(3f, 5f), Random.Range(4f, 7f), Random.Range(3f, 5f));
            spire.GetComponent<MeshRenderer>().sharedMaterial = matRed;
        }
    }

    // --- GENERADOR DE ARCO NATURAL DE PIEDRA ---
    private static void CreateNaturalSandstoneArch(Transform parent, Vector3 centerPos, Material matRed, Material matDark)
    {
        GameObject archRoot = new GameObject("Natural_Sandstone_Arch");
        archRoot.transform.parent = parent;
        archRoot.transform.position = centerPos;

        float archSpan = 22f;
        float archHeight = 11f;

        // Pilar izquierdo
        GameObject pillarL = GameObject.CreatePrimitive(PrimitiveType.Cube);
        pillarL.transform.parent = archRoot.transform;
        pillarL.transform.localPosition = new Vector3(-archSpan * 0.5f, archHeight * 0.5f, 0);
        pillarL.transform.localScale = new Vector3(5.5f, archHeight, 6.5f);
        pillarL.GetComponent<MeshRenderer>().sharedMaterial = matRed;

        // Pilar derecho
        GameObject pillarR = GameObject.CreatePrimitive(PrimitiveType.Cube);
        pillarR.transform.parent = archRoot.transform;
        pillarR.transform.localPosition = new Vector3(archSpan * 0.5f, archHeight * 0.5f, 0);
        pillarR.transform.localScale = new Vector3(5.5f, archHeight, 6.5f);
        pillarR.GetComponent<MeshRenderer>().sharedMaterial = matRed;

        // Segmentos curvados del arco superior
        int archSegments = 7;
        for (int i = 0; i < archSegments; i++)
        {
            float t = (float)i / (archSegments - 1);
            float angle = Mathf.Lerp(15f, 165f, t) * Mathf.Deg2Rad;
            float x = -Mathf.Cos(angle) * (archSpan * 0.5f);
            float y = archHeight + Mathf.Sin(angle) * 3.8f - 1.5f;

            GameObject segment = GameObject.CreatePrimitive(PrimitiveType.Cube);
            segment.transform.parent = archRoot.transform;
            segment.transform.localPosition = new Vector3(x, y, 0);
            segment.transform.localRotation = Quaternion.Euler(0, 0, (t - 0.5f) * -50f);
            segment.transform.localScale = new Vector3(4.8f, 3.2f, 6.0f);
            segment.GetComponent<MeshRenderer>().sharedMaterial = (i % 2 == 0) ? matDark : matRed;
        }
    }

    // --- GENERADOR DE CACTUS SAGUARO CON BRAZOS ---
    private static void CreateSaguaroCactus(Transform parent, Vector3 pos, Material matCactus, Material matFlower)
    {
        GameObject saguaro = new GameObject("Saguaro_Cactus");
        saguaro.transform.parent = parent;
        saguaro.transform.position = pos;

        float mainHeight = Random.Range(4.5f, 7.5f);
        float trunkRadius = Random.Range(0.45f, 0.65f);

        // Tronco principal
        GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        trunk.name = "Main_Trunk";
        trunk.transform.parent = saguaro.transform;
        trunk.transform.localPosition = new Vector3(0, mainHeight * 0.5f, 0);
        trunk.transform.localScale = new Vector3(trunkRadius * 2f, mainHeight * 0.5f, trunkRadius * 2f);
        trunk.GetComponent<MeshRenderer>().sharedMaterial = matCactus;

        // Flor en la punta superior
        if (Random.value > 0.35f && matFlower != null)
        {
            GameObject flower = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            flower.name = "Cactus_Flower";
            flower.transform.parent = saguaro.transform;
            flower.transform.localPosition = new Vector3(0, mainHeight + 0.1f, 0);
            flower.transform.localScale = new Vector3(0.35f, 0.25f, 0.35f);
            flower.GetComponent<MeshRenderer>().sharedMaterial = matFlower;
        }

        // Brazos ramificados (1 a 3 brazos)
        int armsCount = Random.Range(1, 4);
        for (int i = 0; i < armsCount; i++)
        {
            float side = (i % 2 == 0) ? 1f : -1f;
            float armBranchY = Random.Range(mainHeight * 0.35f, mainHeight * 0.65f);
            float armLengthH = Random.Range(0.8f, 1.4f);
            float armHeightV = Random.Range(1.5f, 2.8f);
            float armRadius = trunkRadius * 0.75f;

            // Rama horizontal
            GameObject armH = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            armH.name = $"Arm_H_{i}";
            armH.transform.parent = saguaro.transform;
            armH.transform.localPosition = new Vector3(side * (armLengthH * 0.5f + trunkRadius * 0.5f), armBranchY, 0);
            armH.transform.localRotation = Quaternion.Euler(0, 0, 90f);
            armH.transform.localScale = new Vector3(armRadius * 2f, armLengthH * 0.5f, armRadius * 2f);
            armH.GetComponent<MeshRenderer>().sharedMaterial = matCactus;

            // Rama vertical
            GameObject armV = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            armV.name = $"Arm_V_{i}";
            armV.transform.parent = saguaro.transform;
            armV.transform.localPosition = new Vector3(side * (armLengthH + trunkRadius * 0.5f), armBranchY + armHeightV * 0.5f, 0);
            armV.transform.localScale = new Vector3(armRadius * 2f, armHeightV * 0.5f, armRadius * 2f);
            armV.GetComponent<MeshRenderer>().sharedMaterial = matCactus;

            // Flor de brazo
            if (Random.value > 0.5f && matFlower != null)
            {
                GameObject armFlower = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                armFlower.transform.parent = saguaro.transform;
                armFlower.transform.localPosition = new Vector3(side * (armLengthH + trunkRadius * 0.5f), armBranchY + armHeightV + 0.1f, 0);
                armFlower.transform.localScale = new Vector3(0.28f, 0.2f, 0.28f);
                armFlower.GetComponent<MeshRenderer>().sharedMaterial = matFlower;
            }
        }
    }

    // --- GENERADOR DE NOPAL / PRICKLY PEAR ---
    private static void CreatePricklyPearCactus(Transform parent, Vector3 pos, Material matCactus, Material matFlower)
    {
        GameObject nopal = new GameObject("Nopal_Cactus");
        nopal.transform.parent = parent;
        nopal.transform.position = pos;

        int padCount = Random.Range(4, 8);
        Vector3 currentP = Vector3.zero;

        for (int i = 0; i < padCount; i++)
        {
            GameObject pad = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            pad.name = $"Pad_{i}";
            pad.transform.parent = nopal.transform;
            float padScale = Random.Range(0.6f, 0.9f);
            
            if (i == 0)
            {
                currentP = new Vector3(0, 0.4f, 0);
            }
            else
            {
                currentP += new Vector3(Random.Range(-0.35f, 0.35f), Random.Range(0.35f, 0.55f), Random.Range(-0.25f, 0.25f));
            }

            pad.transform.localPosition = currentP;
            pad.transform.localScale = new Vector3(padScale, padScale * 1.1f, padScale * 0.25f);
            pad.transform.localRotation = Quaternion.Euler(Random.Range(-15f, 15f), Random.Range(0f, 180f), Random.Range(-25f, 25f));
            pad.GetComponent<MeshRenderer>().sharedMaterial = matCactus;

            if (Random.value > 0.45f && matFlower != null)
            {
                GameObject fruit = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                fruit.transform.parent = pad.transform;
                fruit.transform.localPosition = new Vector3(0, 0.5f, 0);
                fruit.transform.localScale = new Vector3(0.22f, 0.22f, 0.22f);
                fruit.GetComponent<MeshRenderer>().sharedMaterial = matFlower;
            }
        }
    }

    // --- GENERADOR DE CACTUS BARRIL ---
    private static void CreateBarrelCactus(Transform parent, Vector3 pos, Material matCactus, Material matFlower)
    {
        GameObject barrel = new GameObject("Barrel_Cactus");
        barrel.transform.parent = parent;
        barrel.transform.position = pos;

        float h = Random.Range(0.7f, 1.3f);
        float r = Random.Range(0.5f, 0.8f);

        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        body.transform.parent = barrel.transform;
        body.transform.localPosition = new Vector3(0, h * 0.45f, 0);
        body.transform.localScale = new Vector3(r, h, r);
        body.GetComponent<MeshRenderer>().sharedMaterial = matCactus;

        if (matFlower != null)
        {
            GameObject crownFlower = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            crownFlower.transform.parent = barrel.transform;
            crownFlower.transform.localPosition = new Vector3(0, h * 0.85f, 0);
            crownFlower.transform.localScale = new Vector3(0.3f, 0.2f, 0.3f);
            crownFlower.GetComponent<MeshRenderer>().sharedMaterial = matFlower;
        }
    }

    // --- GENERADOR DE RODADORA / MATORRAL SECO ---
    private static void CreateTumbleweedBush(Transform parent, Vector3 pos, Material matDryBush)
    {
        GameObject tumble = new GameObject("Tumbleweed_Bush");
        tumble.transform.parent = parent;
        tumble.transform.position = pos;

        int twigs = Random.Range(3, 5);
        for (int i = 0; i < twigs; i++)
        {
            GameObject twig = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            twig.transform.parent = tumble.transform;
            float s = Random.Range(0.6f, 1.2f);
            twig.transform.localPosition = new Vector3(Random.Range(-0.25f, 0.25f), s * 0.35f, Random.Range(-0.25f, 0.25f));
            twig.transform.localScale = new Vector3(s, s * 0.75f, s);
            twig.GetComponent<MeshRenderer>().sharedMaterial = matDryBush;
        }
    }

    // --- GENERADOR DEL OASIS DESÉRTICO ---
    private static void CreateDesertOasis(Transform parent, Vector3 centerPos, Material matWater, Material matSand, Material matPalmLeaf, Material matWood)
    {
        GameObject oasis = new GameObject("Desert_Oasis_Area");
        oasis.transform.parent = parent;
        oasis.transform.position = centerPos;

        // Estanque de agua azul turquesa
        GameObject waterPool = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        waterPool.name = "Oasis_Water_Pool";
        waterPool.transform.parent = oasis.transform;
        waterPool.transform.localPosition = new Vector3(0, 0.02f, 0);
        waterPool.transform.localScale = new Vector3(14f, 0.05f, 18f);
        waterPool.GetComponent<MeshRenderer>().sharedMaterial = matWater;

        // Palmeras alrededor del estanque
        Vector3[] palmOffsets = new Vector3[]
        {
            new Vector3(-6f, 0, -6f),
            new Vector3(6f, 0, -5f),
            new Vector3(5f, 0, 7f),
            new Vector3(-5f, 0, 6f)
        };

        foreach (var offset in palmOffsets)
        {
            CreatePalmTree(oasis.transform, centerPos + offset, matWood, matPalmLeaf);
        }
    }

    // --- GENERADOR DE PALMERA DE OASIS ---
    private static void CreatePalmTree(Transform parent, Vector3 pos, Material matTrunk, Material matLeaves)
    {
        GameObject palm = new GameObject("Oasis_Palm_Tree");
        palm.transform.parent = parent;
        palm.transform.position = pos;

        float trunkHeight = Random.Range(6.0f, 9.0f);
        float curveAngle = Random.Range(6f, 16f);
        float rotY = Random.Range(0f, 360f);

        // Tronco inclinado
        GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        trunk.transform.parent = palm.transform;
        trunk.transform.localPosition = new Vector3(0, trunkHeight * 0.5f, 0);
        trunk.transform.localRotation = Quaternion.Euler(curveAngle, rotY, 0);
        trunk.transform.localScale = new Vector3(0.55f, trunkHeight * 0.5f, 0.55f);
        trunk.GetComponent<MeshRenderer>().sharedMaterial = matTrunk;

        // Copa de Hojas / Frondas de Palmera
        Vector3 topPos = trunk.transform.position + trunk.transform.up * (trunkHeight * 0.5f);
        int frondCount = 7;
        for (int i = 0; i < frondCount; i++)
        {
            float frondAngle = (360f / frondCount) * i;
            GameObject frond = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frond.transform.parent = palm.transform;
            frond.transform.position = topPos;
            frond.transform.localRotation = Quaternion.Euler(30f, frondAngle, 0);
            frond.transform.localScale = new Vector3(0.8f, 0.08f, 3.8f);
            frond.GetComponent<MeshRenderer>().sharedMaterial = matLeaves;
        }
    }

    // --- GENERADOR DE FOGATA ABANDONADA ---
    private static void CreateCampfire(Transform parent, Vector3 pos, Material matStone, Material matCharcoal)
    {
        GameObject campfire = new GameObject("Desert_Campfire");
        campfire.transform.parent = parent;
        campfire.transform.position = pos;

        // Anillo de piedras
        int stones = 8;
        for (int i = 0; i < stones; i++)
        {
            float angle = (360f / stones) * i * Mathf.Deg2Rad;
            GameObject stone = GameObject.CreatePrimitive(PrimitiveType.Cube);
            stone.transform.parent = campfire.transform;
            stone.transform.localPosition = new Vector3(Mathf.Cos(angle) * 0.8f, 0.12f, Mathf.Sin(angle) * 0.8f);
            stone.transform.localRotation = Random.rotation;
            stone.transform.localScale = Vector3.one * Random.Range(0.25f, 0.4f);
            stone.GetComponent<MeshRenderer>().sharedMaterial = matStone;
        }

        // Troncos quemados cruzados
        for (int i = 0; i < 3; i++)
        {
            GameObject log = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            log.transform.parent = campfire.transform;
            log.transform.localPosition = new Vector3(0, 0.1f, 0);
            log.transform.localRotation = Quaternion.Euler(0, i * 60f, 0);
            log.transform.localScale = new Vector3(0.12f, 0.6f, 0.12f);
            log.GetComponent<MeshRenderer>().sharedMaterial = matCharcoal;
        }
    }

    // --- GENERADOR DE CRÁNEO Y COSTILLAS FÓSILES ---
    private static void CreateFossilSkeleton(Transform parent, Vector3 pos, Material matBone)
    {
        GameObject fossil = new GameObject("Desert_Fossil_Skeleton");
        fossil.transform.parent = parent;
        fossil.transform.position = pos;

        // Calavera de res / buey
        GameObject skull = GameObject.CreatePrimitive(PrimitiveType.Cube);
        skull.name = "Skull_Base";
        skull.transform.parent = fossil.transform;
        skull.transform.localPosition = new Vector3(0, 0.25f, 0.8f);
        skull.transform.localScale = new Vector3(0.45f, 0.35f, 0.65f);
        skull.GetComponent<MeshRenderer>().sharedMaterial = matBone;

        // Cuernos
        GameObject hornL = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        hornL.transform.parent = fossil.transform;
        hornL.transform.localPosition = new Vector3(-0.4f, 0.45f, 0.6f);
        hornL.transform.localRotation = Quaternion.Euler(20f, -40f, 65f);
        hornL.transform.localScale = new Vector3(0.08f, 0.35f, 0.08f);
        hornL.GetComponent<MeshRenderer>().sharedMaterial = matBone;

        GameObject hornR = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        hornR.transform.parent = fossil.transform;
        hornR.transform.localPosition = new Vector3(0.4f, 0.45f, 0.6f);
        hornR.transform.localRotation = Quaternion.Euler(20f, 40f, -65f);
        hornR.transform.localScale = new Vector3(0.08f, 0.35f, 0.08f);
        hornR.GetComponent<MeshRenderer>().sharedMaterial = matBone;

        // Costillas fósiles semienterradas
        for (int i = 0; i < 4; i++)
        {
            GameObject ribL = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ribL.transform.parent = fossil.transform;
            ribL.transform.localPosition = new Vector3(-0.35f, 0.3f, -i * 0.4f);
            ribL.transform.localRotation = Quaternion.Euler(0, 0, 35f);
            ribL.transform.localScale = new Vector3(0.06f, 0.45f, 0.08f);
            ribL.GetComponent<MeshRenderer>().sharedMaterial = matBone;

            GameObject ribR = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ribR.transform.parent = fossil.transform;
            ribR.transform.localPosition = new Vector3(0.35f, 0.3f, -i * 0.4f);
            ribR.transform.localRotation = Quaternion.Euler(0, 0, -35f);
            ribR.transform.localScale = new Vector3(0.06f, 0.45f, 0.08f);
            ribR.GetComponent<MeshRenderer>().sharedMaterial = matBone;
        }
    }

    // --- GENERADOR DE VAGONETA MINERA ---
    private static void CreateMiningCart(Transform parent, Vector3 pos, Material matWood, Material matMetal, Material matGold)
    {
        GameObject cart = new GameObject("Mining_Cart");
        cart.transform.parent = parent;
        cart.transform.position = pos;

        // Caja de madera
        GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
        box.transform.parent = cart.transform;
        box.transform.localPosition = new Vector3(0, 0.55f, 0);
        box.transform.localScale = new Vector3(1.2f, 0.7f, 1.6f);
        box.GetComponent<MeshRenderer>().sharedMaterial = matWood;

        // Mineral de oro en el interior de la vagoneta
        GameObject goldOre = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        goldOre.transform.parent = cart.transform;
        goldOre.transform.localPosition = new Vector3(0, 0.95f, 0);
        goldOre.transform.localScale = new Vector3(0.9f, 0.4f, 1.2f);
        goldOre.GetComponent<MeshRenderer>().sharedMaterial = matGold;

        // Ruedas metálicas
        Vector3[] wheelPos = new Vector3[]
        {
            new Vector3(-0.65f, 0.2f, -0.5f),
            new Vector3(0.65f, 0.2f, -0.5f),
            new Vector3(-0.65f, 0.2f, 0.5f),
            new Vector3(0.65f, 0.2f, 0.5f)
        };

        foreach (var wPos in wheelPos)
        {
            GameObject wheel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            wheel.transform.parent = cart.transform;
            wheel.transform.localPosition = wPos;
            wheel.transform.localRotation = Quaternion.Euler(0, 0, 90f);
            wheel.transform.localScale = new Vector3(0.4f, 0.06f, 0.4f);
            wheel.GetComponent<MeshRenderer>().sharedMaterial = matMetal;
        }
    }

    // --- GENERADOR DE CAJAS DE TNT ---
    private static void CreateTNTStack(Transform parent, Vector3 pos, Material matTNT, Material matWood)
    {
        GameObject tntGroup = new GameObject("TNT_Crates_Stack");
        tntGroup.transform.parent = parent;
        tntGroup.transform.position = pos;

        // Caja base 1
        GameObject crate1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        crate1.transform.parent = tntGroup.transform;
        crate1.transform.localPosition = new Vector3(-0.3f, 0.3f, 0);
        crate1.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
        crate1.GetComponent<MeshRenderer>().sharedMaterial = matTNT;

        // Caja base 2
        GameObject crate2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        crate2.transform.parent = tntGroup.transform;
        crate2.transform.localPosition = new Vector3(0.35f, 0.3f, 0.1f);
        crate2.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
        crate2.GetComponent<MeshRenderer>().sharedMaterial = matWood;

        // Caja superior
        GameObject crate3 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        crate3.transform.parent = tntGroup.transform;
        crate3.transform.localPosition = new Vector3(0, 0.85f, 0.05f);
        crate3.transform.localRotation = Quaternion.Euler(0, 15f, 0);
        crate3.transform.localScale = new Vector3(0.55f, 0.55f, 0.55f);
        crate3.GetComponent<MeshRenderer>().sharedMaterial = matTNT;
    }

    // --- GENERADOR DE RUEDA DE CARROMATO ---
    private static void CreateWagonWheel(Transform parent, Vector3 pos, Material matWood, Material matMetal)
    {
        GameObject wheel = new GameObject("Wagon_Wheel");
        wheel.transform.parent = parent;
        wheel.transform.position = pos;
        wheel.transform.localRotation = Quaternion.Euler(Random.Range(20f, 45f), Random.Range(-30f, 30f), 0);

        // Aro exterior
        GameObject rim = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        rim.transform.parent = wheel.transform;
        rim.transform.localRotation = Quaternion.Euler(90f, 0, 0);
        rim.transform.localScale = new Vector3(1.2f, 0.08f, 1.2f);
        rim.GetComponent<MeshRenderer>().sharedMaterial = matWood;

        // Eje central
        GameObject hub = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        hub.transform.parent = wheel.transform;
        hub.transform.localRotation = Quaternion.Euler(90f, 0, 0);
        hub.transform.localScale = new Vector3(0.25f, 0.18f, 0.25f);
        hub.GetComponent<MeshRenderer>().sharedMaterial = matMetal;
    }

    // --- GENERADOR DE SEÑAL RÚSTICA ---
    private static void CreateDesertSign(Transform parent, Vector3 pos, string label, Material matWood)
    {
        GameObject sign = new GameObject($"Desert_Sign_{label}");
        sign.transform.parent = parent;
        sign.transform.position = pos;

        // Poste
        GameObject post = GameObject.CreatePrimitive(PrimitiveType.Cube);
        post.transform.parent = sign.transform;
        post.transform.localPosition = new Vector3(0, 1.0f, 0);
        post.transform.localScale = new Vector3(0.14f, 2.0f, 0.14f);
        post.GetComponent<MeshRenderer>().sharedMaterial = matWood;

        // Tablón con corte rústico
        GameObject board = GameObject.CreatePrimitive(PrimitiveType.Cube);
        board.name = label;
        board.transform.parent = sign.transform;
        board.transform.localPosition = new Vector3(0, 1.65f, 0);
        board.transform.localScale = new Vector3(1.5f, 0.45f, 0.08f);
        board.transform.localRotation = Quaternion.Euler(0, Random.Range(-20f, 20f), Random.Range(-4f, 4f));
        board.GetComponent<MeshRenderer>().sharedMaterial = matWood;
    }

    // --- GENERADOR DE BLOQUES DE ROCAS DESÉRTICAS ---
    private static void CreateDesertBoulders(Transform parent, Vector3 pos, Material matRed, Material matDark)
    {
        GameObject boulderGroup = new GameObject("Desert_Boulders");
        boulderGroup.transform.parent = parent;
        boulderGroup.transform.position = pos;

        int count = Random.Range(2, 5);
        for (int i = 0; i < count; i++)
        {
            GameObject boulder = GameObject.CreatePrimitive(PrimitiveType.Cube);
            boulder.transform.parent = boulderGroup.transform;
            float s = Random.Range(0.8f, 2.2f);
            boulder.transform.localPosition = new Vector3(Random.Range(-1.2f, 1.2f), s * 0.35f, Random.Range(-1.2f, 1.2f));
            boulder.transform.localRotation = Random.rotation;
            boulder.transform.localScale = new Vector3(s, s * Random.Range(0.7f, 1.2f), s * Random.Range(0.8f, 1.4f));
            boulder.GetComponent<MeshRenderer>().sharedMaterial = (i % 2 == 0) ? matRed : matDark;
        }
    }

    // --- VENTANA DE EDITOR INTERACTIVA ---
    [MenuItem("DuckHunt/Ventana Generador de Mapas")]
    public static void OpenWindow()
    {
        DesertMapGenerator window = GetWindow<DesertMapGenerator>("Generador de Mapas Duck Hunt");
        window.minSize = new Vector2(380, 420);
        window.Show();
    }

    private void OnGUI()
    {
        GUILayout.Space(10);
        GUILayout.Label("Generador de Entornos VR Duck Hunt", EditorStyles.boldLabel);
        GUILayout.Space(10);

        EditorGUILayout.HelpBox("Esta herramienta permite generar mapas completos para el juego VR Duck Hunt con soporte para movimiento sobre rieles, iluminación y enemigos dinámicos.", MessageType.Info);

        GUILayout.Space(15);
        GUILayout.Label("1. Mapa del Desierto (Desert Canyon)", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Generar Mapa Desierto (En Escena Actual)", GUILayout.Height(35)))
        {
            GenerateDesertMap();
        }

        GUILayout.Space(5);
        if (GUILayout.Button("Crear Nueva Escena Desierto (MapaDesierto.unity)", GUILayout.Height(35)))
        {
            CreateDesertScene();
        }

        GUILayout.Space(20);
        GUILayout.Label("2. Mapa de la Pradera (Prairie / Meadow)", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Generar Mapa Pradera (En Escena Actual)", GUILayout.Height(35)))
        {
            PrairieMapGenerator.GeneratePrairieMap();
        }

        GUILayout.Space(20);
        GUILayout.Label("3. Configuración de Enemigos", EditorStyles.boldLabel);
        if (GUILayout.Button("Configurar Spawner y Scriptable Objects", GUILayout.Height(30)))
        {
            EnemySetupHelper.CreateEnemyAssetsAndPrefab();
        }
    }
}
