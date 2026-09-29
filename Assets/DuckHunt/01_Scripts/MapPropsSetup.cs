using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Genera y asegura en tiempo de ejecución o en el editor:
/// 1. Las 5 mesas de armas distribuidas a lo largo del camino (Pistola, Escopeta, Rifle de Caza, Lanzagranadas/Bazuka, Escopeta Pesada).
/// 2. La Línea de Meta / Finish Line física y visual con Trigger para cambio de nivel o victoria.
/// 3. El PlayerHUD visible en VR.
/// </summary>
public class MapPropsSetup : MonoBehaviour
{
    [Header("Configuración de Posiciones (Eje Z)")]
    public float zMesa1 = 18f;   // Pistola
    public float zMesa2 = 65f;   // Escopeta
    public float zMesa3 = 115f;  // Rifle de Caza
    public float zMesa4 = 165f;  // Lanzagranadas / Bazuka
    public float zMesa5 = 205f;  // Armería Avanzada
    public float zMeta = 238f;   // Línea de Meta

    void Awake()
    {
        AsegurarPlayerHUD();
        AsegurarMesasDeArmas();
        AsegurarLineaDeMeta();
    }

    public void AsegurarPlayerHUD()
    {
        if (Object.FindAnyObjectByType<PlayerHUD>() == null)
        {
            GameObject hudObj = new GameObject("PlayerHUD_VR");
            hudObj.AddComponent<PlayerHUD>();
        }
    }

    public void AsegurarMesasDeArmas()
    {
        if (GameObject.Find("[Weapon_Tables]") != null) return;

        GameObject tablesRoot = new GameObject("[Weapon_Tables]");
        tablesRoot.transform.position = Vector3.zero;

        Material woodMat = Resources.Load<Material>("M_Wood") ?? ObtenerMaterialColor(new Color(0.45f, 0.30f, 0.18f));
        Material goldMat = Resources.Load<Material>("M_GoldOre") ?? ObtenerMaterialColor(new Color(1f, 0.85f, 0.2f));

        // 1. Mesa 1: Pistola VR (Z = 18m, Lado Derecho)
        CrearMesaConArma(
            tablesRoot.transform,
            new Vector3(1.65f, 0f, zMesa1),
            "🔫 PISTOLA 3D",
            "Assets/DuckHunt/03_Prefabs/pistola.prefab",
            woodMat, goldMat
        );

        // 2. Mesa 2: Escopeta (Z = 65m, Lado Izquierdo)
        CrearMesaConArma(
            tablesRoot.transform,
            new Vector3(-1.65f, 0f, zMesa2),
            "💥 ESCOPETA",
            "Assets/DuckHunt/03_Prefabs/escopeta.prefab",
            woodMat, goldMat
        );

        // 3. Mesa 3: Rifle de Caza (Z = 115m, Lado Derecho)
        CrearMesaConArma(
            tablesRoot.transform,
            new Vector3(1.65f, 0f, zMesa3),
            "🎯 RIFLE DE CAZA",
            "Assets/DuckHunt/03_Prefabs/rifle de caza.prefab",
            woodMat, goldMat
        );

        // 4. Mesa 4: Lanzagranadas / Bazuka (Z = 165m, Lado Izquierdo)
        CrearMesaConArma(
            tablesRoot.transform,
            new Vector3(-1.65f, 0f, zMesa4),
            "🚀 LANZAGRANADAS RPG",
            "Assets/DuckHunt/03_Prefabs/lanzagranadas.prefab",
            woodMat, goldMat
        );

        // 5. Mesa 5: Armería Especial (Z = 205m, Lado Derecho)
        CrearMesaConArma(
            tablesRoot.transform,
            new Vector3(1.65f, 0f, zMesa5),
            "⚡ ESCOPETA POTENCIADA",
            "Assets/DuckHunt/03_Prefabs/escopeta.prefab",
            woodMat, goldMat
        );

        Debug.Log("[MapPropsSetup] ✅ 5 Mesas con armas colocadas estratégicamente a lo largo del mapa.");
    }

    private void CrearMesaConArma(Transform parent, Vector3 posicion, string nombreArma, string prefabPath, Material matMadera, Material matDorado)
    {
        GameObject tableGroup = new GameObject($"Mesa_{nombreArma.Replace(" ", "_")}");
        tableGroup.transform.SetParent(parent);
        tableGroup.transform.position = posicion;

        // Orientar la mesa hacia el centro del camino
        float rotationY = (posicion.x > 0) ? -25f : 25f;
        tableGroup.transform.rotation = Quaternion.Euler(0, rotationY, 0);

        // Tablero de la mesa
        GameObject tableTop = GameObject.CreatePrimitive(PrimitiveType.Cube);
        tableTop.name = "TableTop";
        tableTop.transform.SetParent(tableGroup.transform, false);
        tableTop.transform.localPosition = new Vector3(0, 0.8f, 0);
        tableTop.transform.localScale = new Vector3(1.35f, 0.1f, 0.75f);
        tableTop.GetComponent<MeshRenderer>().sharedMaterial = matMadera;

        // 4 Patas
        Vector3[] legOffsets = new Vector3[] {
            new Vector3(-0.55f, 0.38f, -0.28f),
            new Vector3(0.55f, 0.38f, -0.28f),
            new Vector3(-0.55f, 0.38f, 0.28f),
            new Vector3(0.55f, 0.38f, 0.28f)
        };
        foreach (var legPos in legOffsets)
        {
            GameObject leg = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            leg.name = "Leg";
            leg.transform.SetParent(tableGroup.transform, false);
            leg.transform.localPosition = legPos;
            leg.transform.localScale = new Vector3(0.08f, 0.40f, 0.08f);
            leg.GetComponent<MeshRenderer>().sharedMaterial = matMadera;
        }

        // Tapete / Pedestal brillante
        GameObject matObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        matObj.name = "WeaponPad";
        matObj.transform.SetParent(tableGroup.transform, false);
        matObj.transform.localPosition = new Vector3(0, 0.86f, 0);
        matObj.transform.localScale = new Vector3(1.0f, 0.02f, 0.5f);
        matObj.GetComponent<MeshRenderer>().sharedMaterial = matDorado;

        // Cartel 3D flotante con el nombre del arma
        GameObject textObj = new GameObject("Label_Arma");
        textObj.transform.SetParent(tableGroup.transform, false);
        textObj.transform.localPosition = new Vector3(0, 1.35f, 0);
        textObj.transform.localScale = Vector3.one * 0.02f;

        TextMeshPro tmp = textObj.AddComponent<TextMeshPro>();
        tmp.text = nombreArma;
        tmp.fontSize = 24;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = new Color(1f, 0.95f, 0.3f);

        // Instanciar el arma sobre la mesa
        GameObject weaponInstance = null;
#if UNITY_EDITOR
        GameObject prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab != null)
        {
            weaponInstance = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(prefab, tableGroup.transform);
        }
#endif
        if (weaponInstance == null)
        {
            weaponInstance = Resources.Load<GameObject>(prefabPath);
            if (weaponInstance != null)
            {
                weaponInstance = Instantiate(weaponInstance, tableGroup.transform);
            }
        }

        if (weaponInstance != null)
        {
            weaponInstance.transform.localPosition = new Vector3(0f, 0.95f, 0f);
            weaponInstance.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
            
            Rigidbody rb = weaponInstance.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
            }
        }
    }

    public void AsegurarLineaDeMeta()
    {
        if (GameObject.Find("[Finish_Line_Arch]") != null) return;

        GameObject archRoot = new GameObject("[Finish_Line_Arch]");
        archRoot.transform.position = new Vector3(0, 0, zMeta);

        Material metalMat = Resources.Load<Material>("M_Metal") ?? ObtenerMaterialColor(new Color(0.3f, 0.3f, 0.35f));
        Material checkMat = Resources.Load<Material>("M_TNT_Red") ?? ObtenerMaterialColor(new Color(0.9f, 0.2f, 0.2f));

        // Poste Izquierdo
        GameObject postL = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        postL.name = "Poste_Izquierdo";
        postL.transform.SetParent(archRoot.transform, false);
        postL.transform.localPosition = new Vector3(-3.2f, 2.2f, 0);
        postL.transform.localScale = new Vector3(0.35f, 2.2f, 0.35f);
        postL.GetComponent<MeshRenderer>().sharedMaterial = metalMat;

        // Poste Derecho
        GameObject postR = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        postR.name = "Poste_Derecho";
        postR.transform.SetParent(archRoot.transform, false);
        postR.transform.localPosition = new Vector3(3.2f, 2.2f, 0);
        postR.transform.localScale = new Vector3(0.35f, 2.2f, 0.35f);
        postR.GetComponent<MeshRenderer>().sharedMaterial = metalMat;

        // Travesaño Superior
        GameObject beam = GameObject.CreatePrimitive(PrimitiveType.Cube);
        beam.name = "Travesaño_Superior";
        beam.transform.SetParent(archRoot.transform, false);
        beam.transform.localPosition = new Vector3(0, 4.4f, 0);
        beam.transform.localScale = new Vector3(6.8f, 0.45f, 0.45f);
        beam.GetComponent<MeshRenderer>().sharedMaterial = metalMat;

        // Cartel de Meta
        GameObject banner = GameObject.CreatePrimitive(PrimitiveType.Cube);
        banner.name = "Cartel_Meta";
        banner.transform.SetParent(archRoot.transform, false);
        banner.transform.localPosition = new Vector3(0, 3.75f, 0);
        banner.transform.localScale = new Vector3(5.5f, 0.8f, 0.1f);
        banner.GetComponent<MeshRenderer>().sharedMaterial = checkMat;

        // Texto 3D de Meta
        GameObject bannerText = new GameObject("Texto_Meta");
        bannerText.transform.SetParent(banner.transform, false);
        bannerText.transform.localPosition = new Vector3(0, 0, -0.6f);
        bannerText.transform.localScale = new Vector3(0.18f, 1.25f, 1f);

        string activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        bool esUltimo = activeScene.Equals("Mapa3", System.StringComparison.OrdinalIgnoreCase) ||
                        activeScene.Equals("BasicScene", System.StringComparison.OrdinalIgnoreCase);

        TextMeshPro tmp = bannerText.AddComponent<TextMeshPro>();
        tmp.text = esUltimo ? "META FINAL" : "META - SIGUIENTE NIVEL";
        tmp.fontSize = 26;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;

        // Línea de Meta en el Suelo (Cinta brillante)
        GameObject groundLine = GameObject.CreatePrimitive(PrimitiveType.Cube);
        groundLine.name = "Linea_Piso";
        groundLine.transform.SetParent(archRoot.transform, false);
        groundLine.transform.localPosition = new Vector3(0, 0.03f, 0);
        groundLine.transform.localScale = new Vector3(6.2f, 0.05f, 1.2f);
        groundLine.GetComponent<MeshRenderer>().sharedMaterial = checkMat;

        // Trigger BoxCollider para detectar cuando el jugador cruza la línea
        GameObject triggerObj = new GameObject("FinishLine_TriggerZone");
        triggerObj.transform.SetParent(archRoot.transform, false);
        triggerObj.transform.localPosition = new Vector3(0, 1.5f, 0);

        BoxCollider triggerCol = triggerObj.AddComponent<BoxCollider>();
        triggerCol.isTrigger = true;
        triggerCol.size = new Vector3(7.0f, 4.0f, 2.5f);

        triggerObj.AddComponent<FinishLineTrigger>();

        Debug.Log($"[MapPropsSetup] 🏁 Línea de Meta física con Trigger colocada en Z = {zMeta}m.");
    }

    private Material ObtenerMaterialColor(Color color)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        Material m = new Material(shader);
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", color);
        else m.color = color;
        return m;
    }
}
