using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class PrairieMapGenerator : EditorWindow
{
    [MenuItem("DuckHunt/Generar Mapa Pradera")]
    public static void GeneratePrairieMap()
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

        // Crear/Obtener Materiales
        Material matGrass = GetOrCreateMaterial("Assets/DuckHunt/05_Materials/M_Grass.mat", litShader, new Color(0.32f, 0.58f, 0.22f), 0.1f);
        Material matDirtPath = GetOrCreateMaterial("Assets/DuckHunt/05_Materials/M_DirtPath.mat", litShader, new Color(0.62f, 0.48f, 0.32f), 0.05f);
        Material matTallGrass = GetOrCreateMaterial("Assets/DuckHunt/05_Materials/M_TallGrass.mat", litShader, new Color(0.48f, 0.68f, 0.18f), 0.0f);
        Material matWood = GetOrCreateMaterial("Assets/DuckHunt/05_Materials/M_Wood.mat", litShader, new Color(0.42f, 0.28f, 0.16f), 0.15f);
        Material matFoliage = GetOrCreateMaterial("Assets/DuckHunt/05_Materials/M_Foliage.mat", litShader, new Color(0.24f, 0.48f, 0.18f), 0.05f);
        Material matDarkFoliage = GetOrCreateMaterial("Assets/DuckHunt/05_Materials/M_DarkFoliage.mat", litShader, new Color(0.18f, 0.38f, 0.14f), 0.05f);
        Material matRock = GetOrCreateMaterial("Assets/DuckHunt/05_Materials/M_Rock.mat", litShader, new Color(0.50f, 0.50f, 0.48f), 0.2f);
        Material matFlowerYellow = GetOrCreateMaterial("Assets/DuckHunt/05_Materials/M_FlowerYellow.mat", litShader, new Color(0.95f, 0.85f, 0.15f), 0.0f);
        Material matFlowerRed = GetOrCreateMaterial("Assets/DuckHunt/05_Materials/M_FlowerRed.mat", litShader, new Color(0.85f, 0.22f, 0.22f), 0.0f);
        Material matBarrel = GetOrCreateMaterial("Assets/DuckHunt/05_Materials/M_BarrelRed.mat", litShader, new Color(0.78f, 0.20f, 0.15f), 0.3f);
        Material matMetal = GetOrCreateMaterial("Assets/DuckHunt/05_Materials/M_Metal.mat", litShader, new Color(0.25f, 0.25f, 0.27f), 0.6f);
        Material matHills = GetOrCreateMaterial("Assets/DuckHunt/05_Materials/M_Hills.mat", litShader, new Color(0.35f, 0.50f, 0.30f), 0.0f);

        // 2. Limpiar mapa anterior si existe
        GameObject existingMap = GameObject.Find("[Prairie_Map]");
        if (existingMap != null)
        {
            Undo.DestroyObjectImmediate(existingMap);
        }

        // 3. Crear nodo raíz
        GameObject mapRoot = new GameObject("[Prairie_Map]");
        Undo.RegisterCreatedObjectUndo(mapRoot, "Create Prairie Map");

        GameObject groundGroup = CreateSubGroup(mapRoot, "01_Terrain_And_Paths");
        GameObject tallGrassGroup = CreateSubGroup(mapRoot, "02_Tall_Grass_Patches");
        GameObject treesGroup = CreateSubGroup(mapRoot, "03_Trees_And_Bushes");
        GameObject fencesGroup = CreateSubGroup(mapRoot, "04_Fences_And_Structures");
        GameObject rocksPropsGroup = CreateSubGroup(mapRoot, "05_Rocks_And_Props");
        GameObject hillsGroup = CreateSubGroup(mapRoot, "06_Distant_Hills");

        // Deshabilitar o remover el viejo 'Plane' por defecto si existe
        GameObject oldPlane = GameObject.Find("Plane");
        if (oldPlane != null && oldPlane.transform.parent == null)
        {
            oldPlane.SetActive(false);
        }

        // --- A. TERRENO Y CAMINO ---
        // Camino de tierra principal (largo 260m, ancho 5m)
        for (int z = -10; z < 250; z += 20)
        {
            GameObject pathSeg = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pathSeg.name = $"DirtPath_Segment_{z}";
            pathSeg.transform.parent = groundGroup.transform;
            pathSeg.transform.position = new Vector3(0, -0.05f, z + 10);
            pathSeg.transform.localScale = new Vector3(5f, 0.1f, 20.2f);
            pathSeg.GetComponent<MeshRenderer>().sharedMaterial = matDirtPath;

            // Pradera Izquierda
            GameObject grassLeft = GameObject.CreatePrimitive(PrimitiveType.Cube);
            grassLeft.name = $"Grass_Left_{z}";
            grassLeft.transform.parent = groundGroup.transform;
            grassLeft.transform.position = new Vector3(-25f, -0.06f, z + 10);
            grassLeft.transform.localScale = new Vector3(45f, 0.1f, 20.2f);
            grassLeft.GetComponent<MeshRenderer>().sharedMaterial = matGrass;

            // Pradera Derecha
            GameObject grassRight = GameObject.CreatePrimitive(PrimitiveType.Cube);
            grassRight.name = $"Grass_Right_{z}";
            grassRight.transform.parent = groundGroup.transform;
            grassRight.transform.position = new Vector3(25f, -0.06f, z + 10);
            grassRight.transform.localScale = new Vector3(45f, 0.1f, 20.2f);
            grassRight.GetComponent<MeshRenderer>().sharedMaterial = matGrass;
        }

        // --- B. COLINAS LATERALES Y HORIZONTE ---
        Random.InitState(42);
        for (int z = 0; z < 250; z += 18)
        {
            // Colinas izquierda
            float scaleY1 = Random.Range(3.5f, 7f);
            float scaleXZ1 = Random.Range(18f, 30f);
            GameObject hillL = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            hillL.name = $"Hill_Left_{z}";
            hillL.transform.parent = hillsGroup.transform;
            hillL.transform.position = new Vector3(Random.Range(-35f, -28f), scaleY1 * 0.35f - 1f, z + Random.Range(-4f, 4f));
            hillL.transform.localScale = new Vector3(scaleXZ1, scaleY1, scaleXZ1 * 1.3f);
            hillL.GetComponent<MeshRenderer>().sharedMaterial = matHills;

            // Colinas derecha
            float scaleY2 = Random.Range(3.5f, 7f);
            float scaleXZ2 = Random.Range(18f, 30f);
            GameObject hillR = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            hillR.name = $"Hill_Right_{z}";
            hillR.transform.parent = hillsGroup.transform;
            hillR.transform.position = new Vector3(Random.Range(28f, 35f), scaleY2 * 0.35f - 1f, z + Random.Range(-4f, 4f));
            hillR.transform.localScale = new Vector3(scaleXZ2, scaleY2, scaleXZ2 * 1.3f);
            hillR.GetComponent<MeshRenderer>().sharedMaterial = matHills;
        }

        // Montañas de fondo al final del recorrido
        for (int x = -70; x <= 70; x += 30)
        {
            GameObject bgMountain = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bgMountain.name = $"BG_Mountain_{x}";
            bgMountain.transform.parent = hillsGroup.transform;
            bgMountain.transform.position = new Vector3(x + Random.Range(-5f, 5f), Random.Range(18f, 28f), 280f + Random.Range(0, 30f));
            bgMountain.transform.localScale = new Vector3(60f, 45f, 50f);
            bgMountain.GetComponent<MeshRenderer>().sharedMaterial = matHills;
        }

        // --- C. PASTIZALES ALTOS (TALL GRASS PATCHES) ---
        for (int z = 5; z < 240; z += 6)
        {
            // Parches al borde izquierdo del camino
            CreateTallGrassClump(tallGrassGroup.transform, new Vector3(Random.Range(-5.5f, -3.2f), 0, z + Random.Range(-1.5f, 1.5f)), matTallGrass, matFlowerYellow);
            // Parches al borde derecho del camino
            CreateTallGrassClump(tallGrassGroup.transform, new Vector3(Random.Range(3.2f, 5.5f), 0, z + Random.Range(-1.5f, 1.5f)), matTallGrass, matFlowerRed);

            // Parches profundos en el prado
            if (z % 12 == 0)
            {
                CreateTallGrassClump(tallGrassGroup.transform, new Vector3(Random.Range(-15f, -8f), 0, z + Random.Range(-2f, 2f)), matTallGrass, matFlowerYellow);
                CreateTallGrassClump(tallGrassGroup.transform, new Vector3(Random.Range(8f, 15f), 0, z + Random.Range(-2f, 2f)), matTallGrass, matFlowerRed);
            }
        }

        // --- D. CERCAS DE MADERA RÚSTICAS ---
        for (int z = 2; z < 235; z += 12)
        {
            // Secciones alternadas de cerca a la izquierda y derecha
            bool placeLeft = (z % 24 < 16);
            bool placeRight = ((z + 12) % 24 < 16);

            if (placeLeft)
            {
                CreateFenceSection(fencesGroup.transform, new Vector3(-3.2f, 0, z), 10f, matWood, matMetal);
            }
            if (placeRight)
            {
                CreateFenceSection(fencesGroup.transform, new Vector3(3.2f, 0, z), 10f, matWood, matMetal);
            }
        }

        // --- E. ÁRBOLES Y ARBUSTOS ---
        for (int z = 10; z < 240; z += 14)
        {
            // Árboles a los lados
            if (Random.value > 0.3f)
            {
                Vector3 treePosL = new Vector3(Random.Range(-22f, -9f), 0, z + Random.Range(-3f, 3f));
                CreatePrairieTree(treesGroup.transform, treePosL, matWood, matFoliage, matDarkFoliage);
            }
            if (Random.value > 0.3f)
            {
                Vector3 treePosR = new Vector3(Random.Range(9f, 22f), 0, z + Random.Range(-3f, 3f));
                CreatePrairieTree(treesGroup.transform, treePosR, matWood, matFoliage, matDarkFoliage);
            }

            // Arbustos
            Vector3 bushPos = new Vector3(Random.Range(-7f, 7f), 0, z + Random.Range(3f, 7f));
            if (Mathf.Abs(bushPos.x) > 3.2f) // fuera del camino central
            {
                CreateBush(treesGroup.transform, bushPos, matFoliage);
            }
        }

        // --- F. ROCAS Y ELEMENTOS INTERACTIVOS (BARRILES Y LATAS) ---
        for (int z = 15; z < 230; z += 25)
        {
            // Rocas decorativas
            Vector3 rockPos = new Vector3(Random.Range(-12f, 12f), 0, z);
            if (Mathf.Abs(rockPos.x) > 3.5f)
            {
                CreateRockCluster(rocksPropsGroup.transform, rockPos, matRock);
            }

            // Barriles rústicos / explosivos en los bordes
            if (Random.value > 0.4f)
            {
                Vector3 barrelPos = new Vector3((Random.value > 0.5f ? 1 : -1) * Random.Range(3.8f, 6.5f), 0.5f, z + Random.Range(-2f, 2f));
                CreateBarrel(rocksPropsGroup.transform, barrelPos, matBarrel, matMetal);
            }
        }

        // Cartel de inicio de la pradera
        CreateStartSign(fencesGroup.transform, new Vector3(2.8f, 0, 4f), matWood);

        // --- G. AJUSTE DE ILUMINACIÓN Y NIEBLA AMBIENTAL ---
        Light dirLight = Object.FindAnyObjectByType<Light>();
        if (dirLight != null && dirLight.type == LightType.Directional)
        {
            dirLight.color = new Color(1.0f, 0.94f, 0.82f); // Luz dorada cálida
            dirLight.intensity = 1.35f;
            dirLight.transform.rotation = Quaternion.Euler(38f, -40f, 0f);
            dirLight.shadows = LightShadows.Soft;
        }

        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Exponential;
        RenderSettings.fogColor = new Color(0.78f, 0.86f, 0.88f);
        RenderSettings.fogDensity = 0.007f;

        // --- H. CONFIGURAR VR AUTO FORWARD MOVER EN XR ORIGIN ---
        GameObject xrOrigin = GameObject.Find("XR Origin (XR Rig)");
        if (xrOrigin != null)
        {
            VRAutoForwardMover mover = xrOrigin.GetComponent<VRAutoForwardMover>();
            if (mover == null)
            {
                mover = xrOrigin.AddComponent<VRAutoForwardMover>();
            }
            mover.velocidad = 3.0f;
            mover.avanzar = true;
            mover.distanciaMaximaZ = 240f;
            mover.reiniciarAlFinal = true;
            xrOrigin.transform.position = new Vector3(0, 0, 0);
        }

        // Marcar escena como modificada para guardado
        EditorUtility.SetDirty(mapRoot);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("¡Mapa de Pradera Abierta con Pasto Alto generado con exito!");
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

    private static void CreateTallGrassClump(Transform parent, Vector3 pos, Material matGrass, Material matFlower)
    {
        GameObject clump = new GameObject("TallGrass_Clump");
        clump.transform.parent = parent;
        clump.transform.position = pos;

        // Conjunto de briznas/láminas cruzadas de pasto alto
        int bladeCount = Random.Range(5, 8);
        for (int i = 0; i < bladeCount; i++)
        {
            GameObject blade = GameObject.CreatePrimitive(PrimitiveType.Cube);
            blade.transform.parent = clump.transform;
            float height = Random.Range(1.3f, 2.2f);
            float width = Random.Range(0.15f, 0.25f);
            float rotY = Random.Range(0f, 360f);
            float leanX = Random.Range(-12f, 12f);
            float leanZ = Random.Range(-12f, 12f);

            blade.transform.localPosition = new Vector3(Random.Range(-0.4f, 0.4f), height * 0.5f, Random.Range(-0.4f, 0.4f));
            blade.transform.localRotation = Quaternion.Euler(leanX, rotY, leanZ);
            blade.transform.localScale = new Vector3(width, height, 0.05f);
            blade.GetComponent<MeshRenderer>().sharedMaterial = matGrass;
        }

        // Flor silvestre opcional en el centro del pastizal
        if (matFlower != null && Random.value > 0.4f)
        {
            GameObject flower = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            flower.transform.parent = clump.transform;
            flower.transform.localPosition = new Vector3(0, Random.Range(1.1f, 1.6f), 0);
            flower.transform.localScale = Vector3.one * 0.22f;
            flower.GetComponent<MeshRenderer>().sharedMaterial = matFlower;
        }
    }

    private static void CreateFenceSection(Transform parent, Vector3 startPos, float length, Material matWood, Material matMetal)
    {
        GameObject fence = new GameObject("Fence_Section");
        fence.transform.parent = parent;
        fence.transform.position = startPos;

        // Postes verticales
        for (float offsetZ = 0; offsetZ <= length; offsetZ += 3.3f)
        {
            GameObject post = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            post.transform.parent = fence.transform;
            post.transform.localPosition = new Vector3(0, 0.7f, offsetZ);
            post.transform.localScale = new Vector3(0.18f, 0.7f, 0.18f);
            post.GetComponent<MeshRenderer>().sharedMaterial = matWood;

            // Lata de tiro al blanco sobre algunos postes
            if (Random.value > 0.65f)
            {
                GameObject can = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                can.name = "Target_Can";
                can.tag = "Untagged"; // o Enemy si se desea
                can.transform.parent = post.transform;
                can.transform.localPosition = new Vector3(0, 1.2f, 0);
                can.transform.localScale = new Vector3(0.8f, 0.3f, 0.8f);
                can.GetComponent<MeshRenderer>().sharedMaterial = matMetal;
            }
        }

        // Rieles horizontales de madera
        for (float h = 0.45f; h <= 1.05f; h += 0.55f)
        {
            GameObject rail = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rail.transform.parent = fence.transform;
            rail.transform.localPosition = new Vector3(0, h, length * 0.5f);
            rail.transform.localScale = new Vector3(0.08f, 0.12f, length);
            rail.GetComponent<MeshRenderer>().sharedMaterial = matWood;
        }
    }

    private static void CreatePrairieTree(Transform parent, Vector3 pos, Material matWood, Material matLeaves1, Material matLeaves2)
    {
        GameObject tree = new GameObject("Prairie_Tree");
        tree.transform.parent = parent;
        tree.transform.position = pos;

        float trunkHeight = Random.Range(3.5f, 5.5f);

        // Tronco
        GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        trunk.transform.parent = tree.transform;
        trunk.transform.localPosition = new Vector3(0, trunkHeight * 0.5f, 0);
        trunk.transform.localScale = new Vector3(0.6f, trunkHeight * 0.5f, 0.6f);
        trunk.GetComponent<MeshRenderer>().sharedMaterial = matWood;

        // Copa de hojas escalonadas
        int layers = Random.Range(3, 5);
        for (int i = 0; i < layers; i++)
        {
            GameObject canopy = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            canopy.transform.parent = tree.transform;
            float layerY = trunkHeight * 0.75f + (i * 1.1f);
            float radius = (layers - i * 0.4f) * Random.Range(1.2f, 1.7f);
            canopy.transform.localPosition = new Vector3(Random.Range(-0.3f, 0.3f), layerY, Random.Range(-0.3f, 0.3f));
            canopy.transform.localScale = new Vector3(radius, radius * 0.85f, radius);
            canopy.GetComponent<MeshRenderer>().sharedMaterial = (i % 2 == 0) ? matLeaves1 : matLeaves2;
        }
    }

    private static void CreateBush(Transform parent, Vector3 pos, Material matFoliage)
    {
        GameObject bush = new GameObject("Bush");
        bush.transform.parent = parent;
        bush.transform.position = pos;

        int sphereCount = Random.Range(2, 4);
        for (int i = 0; i < sphereCount; i++)
        {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.parent = bush.transform;
            float r = Random.Range(0.8f, 1.4f);
            sphere.transform.localPosition = new Vector3(Random.Range(-0.4f, 0.4f), r * 0.45f, Random.Range(-0.4f, 0.4f));
            sphere.transform.localScale = new Vector3(r, r * 0.75f, r);
            sphere.GetComponent<MeshRenderer>().sharedMaterial = matFoliage;
        }
    }

    private static void CreateRockCluster(Transform parent, Vector3 pos, Material matRock)
    {
        GameObject cluster = new GameObject("Rock_Cluster");
        cluster.transform.parent = parent;
        cluster.transform.position = pos;

        int count = Random.Range(2, 5);
        for (int i = 0; i < count; i++)
        {
            GameObject rock = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rock.transform.parent = cluster.transform;
            float s = Random.Range(0.6f, 1.6f);
            rock.transform.localPosition = new Vector3(Random.Range(-0.8f, 0.8f), s * 0.35f, Random.Range(-0.8f, 0.8f));
            rock.transform.localRotation = Random.rotation;
            rock.transform.localScale = new Vector3(s, s * Random.Range(0.6f, 1f), s * Random.Range(0.8f, 1.3f));
            rock.GetComponent<MeshRenderer>().sharedMaterial = matRock;
        }
    }

    private static void CreateBarrel(Transform parent, Vector3 pos, Material matBarrel, Material matMetal)
    {
        GameObject barrel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        barrel.name = "Explosive_Barrel";
        barrel.tag = "Untagged";
        barrel.transform.parent = parent;
        barrel.transform.position = pos;
        barrel.transform.localScale = new Vector3(0.7f, 0.55f, 0.7f);
        barrel.GetComponent<MeshRenderer>().sharedMaterial = matBarrel;

        // Anillos metálicos del barril
        for (float ringY = -0.5f; ringY <= 0.5f; ringY += 1.0f)
        {
            GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ring.name = "Ring";
            ring.transform.parent = barrel.transform;
            ring.transform.localPosition = new Vector3(0, ringY * 0.6f, 0);
            ring.transform.localScale = new Vector3(1.03f, 0.04f, 1.03f);
            ring.GetComponent<MeshRenderer>().sharedMaterial = matMetal;
            Object.DestroyImmediate(ring.GetComponent<Collider>());
        }
    }

    private static void CreateStartSign(Transform parent, Vector3 pos, Material matWood)
    {
        GameObject sign = new GameObject("Prairie_Start_Sign");
        sign.transform.parent = parent;
        sign.transform.position = pos;

        // Poste
        GameObject post = GameObject.CreatePrimitive(PrimitiveType.Cube);
        post.transform.parent = sign.transform;
        post.transform.localPosition = new Vector3(0, 1.0f, 0);
        post.transform.localScale = new Vector3(0.15f, 2.0f, 0.15f);
        post.GetComponent<MeshRenderer>().sharedMaterial = matWood;

        // Tablón
        GameObject board = GameObject.CreatePrimitive(PrimitiveType.Cube);
        board.transform.parent = sign.transform;
        board.transform.localPosition = new Vector3(0, 1.7f, 0);
        board.transform.localScale = new Vector3(1.6f, 0.5f, 0.08f);
        board.transform.localRotation = Quaternion.Euler(0, -15f, 2f);
        board.GetComponent<MeshRenderer>().sharedMaterial = matWood;
    }
}
