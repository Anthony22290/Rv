#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

public static class EnemySetupHelper
{
    [MenuItem("DuckHunt/1. Generar Enemigos Placeholder y Scriptable Objects")]
    public static void CreateEnemyAssetsAndPrefab()
    {
        // 1. Asegurar carpetas
        string soFolder = "Assets/DuckHunt/06_ScriptableObjects";
        string prefabFolder = "Assets/DuckHunt/03_Prefabs";

        if (!AssetDatabase.IsValidFolder(soFolder))
        {
            AssetDatabase.CreateFolder("Assets/DuckHunt", "06_ScriptableObjects");
        }

        // 2. Crear Scriptable Objects
        EnemyDataSO normalDuck = CreateOrGetSO($"{soFolder}/Pato_Normal.asset", "Pato Común", 100, 3.5f, Color.green, EnemyMovementPattern.SineWave, 1f, 2.5f, new Vector3(0.8f, 0.6f, 0.8f));
        EnemyDataSO fastDuck = CreateOrGetSO($"{soFolder}/Pato_Rapido.asset", "Pato Veloz", 250, 6.0f, new Color(0.2f, 0.6f, 1f), EnemyMovementPattern.SineWave, 1.5f, 4.5f, new Vector3(0.6f, 0.5f, 0.6f));
        EnemyDataSO bonusDuck = CreateOrGetSO($"{soFolder}/Pato_Dorado.asset", "Pato Dorado Bonus", 500, 8.0f, new Color(1f, 0.85f, 0.1f), EnemyMovementPattern.ArcFly, 2.5f, 1.5f, new Vector3(0.5f, 0.4f, 0.5f));

        // 3. Crear Prefab Placeholder con forma de Pato (Cuerpo + Cabeza + Pico + Alas)
        GameObject duckRoot = new GameObject("Pato_Placeholder");
        duckRoot.tag = "Enemy";

        // Cuerpo (Cápsula achatada)
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.name = "Body";
        body.transform.SetParent(duckRoot.transform, false);
        body.transform.localScale = new Vector3(0.6f, 0.4f, 0.8f);
        body.transform.localRotation = Quaternion.Euler(90f, 0, 0);

        // Cabeza (Esfera)
        GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.name = "Head";
        head.transform.SetParent(duckRoot.transform, false);
        head.transform.localPosition = new Vector3(0, 0.25f, 0.45f);
        head.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);

        // Pico (Cubo naranja alargado)
        GameObject beak = GameObject.CreatePrimitive(PrimitiveType.Cube);
        beak.name = "Beak";
        beak.transform.SetParent(duckRoot.transform, false);
        beak.transform.localPosition = new Vector3(0, 0.2f, 0.7f);
        beak.transform.localScale = new Vector3(0.18f, 0.08f, 0.25f);
        var beakRend = beak.GetComponent<Renderer>();
        if (beakRend != null && beakRend.sharedMaterial != null)
        {
            beakRend.sharedMaterial.color = new Color(1f, 0.5f, 0f);
        }

        // Ala Izquierda
        GameObject wingL = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wingL.name = "Wing_L";
        wingL.transform.SetParent(duckRoot.transform, false);
        wingL.transform.localPosition = new Vector3(-0.35f, 0.05f, 0);
        wingL.transform.localScale = new Vector3(0.1f, 0.3f, 0.5f);
        wingL.transform.localRotation = Quaternion.Euler(0, 0, 25f);

        // Ala Derecha
        GameObject wingR = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wingR.name = "Wing_R";
        wingR.transform.SetParent(duckRoot.transform, false);
        wingR.transform.localPosition = new Vector3(0.35f, 0.05f, 0);
        wingR.transform.localScale = new Vector3(0.1f, 0.3f, 0.5f);
        wingR.transform.localRotation = Quaternion.Euler(0, 0, -25f);

        // Quitar colliders individuales de las partes hijas
        foreach (var col in duckRoot.GetComponentsInChildren<Collider>())
        {
            UnityEngine.Object.DestroyImmediate(col);
        }

        // Colocar un BoxCollider principal en la raíz
        BoxCollider mainCollider = duckRoot.AddComponent<BoxCollider>();
        mainCollider.isTrigger = true;
        mainCollider.size = new Vector3(1f, 0.8f, 1.4f);

        // Añadir componente EnemyTarget
        EnemyTarget target = duckRoot.AddComponent<EnemyTarget>();
        target.enemyData = normalDuck;

        // Guardar como Prefab
        string prefabPath = $"{prefabFolder}/PatoPlaceholder.prefab";
        GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(duckRoot, prefabPath);
        UnityEngine.Object.DestroyImmediate(duckRoot);

        // 4. Crear o buscar Spawner en la Escena activa
        EnemySpawner spawner = UnityEngine.Object.FindAnyObjectByType<EnemySpawner>();
        if (spawner == null)
        {
            GameObject spawnerObj = new GameObject("EnemySpawner");
            spawner = spawnerObj.AddComponent<EnemySpawner>();
        }

        // Usar Pato3D.prefab si existe, de lo contrario savedPrefab
        GameObject pato3DPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{prefabFolder}/Pato3D.prefab");
        spawner.enemyBasePrefab = (pato3DPrefab != null) ? pato3DPrefab : savedPrefab;
        spawner.enemyTypes = new EnemyDataSO[] { normalDuck, fastDuck, bonusDuck };
        spawner.spawnDistanceAheadMin = 14f;
        spawner.spawnDistanceAheadMax = 22f;
        spawner.spawnSideDistance = 12f;
        spawner.minSpawnHeight = 2.0f;
        spawner.maxSpawnHeight = 5.5f;

        var mover = UnityEngine.Object.FindAnyObjectByType<VRAutoForwardMover>();
        if (mover != null)
        {
            spawner.playerTransform = mover.transform;
        }

        EditorUtility.SetDirty(spawner);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("¡Enemigos, Scriptable Objects y Spawner Dinámico configurados con éxito!");
    }

    private static EnemyDataSO CreateOrGetSO(string path, string name, int points, float speed, Color color, EnemyMovementPattern pattern, float amp, float freq, Vector3 scale)
    {
        EnemyDataSO so = AssetDatabase.LoadAssetAtPath<EnemyDataSO>(path);
        if (so == null)
        {
            so = ScriptableObject.CreateInstance<EnemyDataSO>();
            AssetDatabase.CreateAsset(so, path);
        }

        so.enemyName = name;
        so.scorePoints = points;
        so.moveSpeed = speed;
        so.bodyColor = color;
        so.movementPattern = pattern;
        so.waveAmplitude = amp;
        so.waveFrequency = freq;
        so.scale = scale;
        so.maxLifeTime = 8f;

        EditorUtility.SetDirty(so);
        return so;
    }
}
#endif
