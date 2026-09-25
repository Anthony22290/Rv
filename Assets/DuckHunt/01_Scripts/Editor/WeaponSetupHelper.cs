using UnityEngine;
using UnityEditor;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using TMPro;

[InitializeOnLoad]
public class WeaponSetupHelper : EditorWindow
{
    static WeaponSetupHelper()
    {
        EditorApplication.delayCall += RunOnce;
    }

    static void RunOnce()
    {
        if (!EditorPrefs.GetBool("ArmasConfiguradas_v5", false))
        {
            SetupWeapons();
            EditorPrefs.SetBool("ArmasConfiguradas_v5", true);
        }
    }

    [MenuItem("DuckHunt/Configurar Armas (Auto)")]
    public static void SetupWeapons()
    {
        string prefabsPath = "Assets/DuckHunt/03_Prefabs/";
        string soPath = "Assets/DuckHunt/06_ScriptableObjects/";

        if (!AssetDatabase.IsValidFolder("Assets/DuckHunt/06_ScriptableObjects"))
        {
            AssetDatabase.CreateFolder("Assets/DuckHunt", "06_ScriptableObjects");
        }

        // --- ARREGLAR LA BALA MISIL ---
        string misilPath = prefabsPath + "misilBala.prefab";
        GameObject misilBala = AssetDatabase.LoadAssetAtPath<GameObject>(misilPath);
        if (misilBala != null)
        {
            // Usar Object.Instantiate para desvincular de prefabs de modelo y evitar errores de AddComponent
            GameObject instMisil = Object.Instantiate(misilBala);
            instMisil.name = "misilBala";
            
            if (instMisil.GetComponent<Bala>() == null)
            {
                Bala b = instMisil.AddComponent<Bala>();
                if (b != null) b.velocidad = 25f;
            }
            
            // Reducir la escala del misil para que no sea gigante y no choque instantáneamente
            instMisil.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);

            Collider col = instMisil.GetComponent<Collider>();
            if (col == null) { col = instMisil.AddComponent<SphereCollider>(); }
            col.isTrigger = true;
            
            // Si es un sphere collider, asegurarse que el radio sea pequeño
            if (col is SphereCollider sc) { sc.radius = 0.1f; }
            
            PrefabUtility.SaveAsPrefabAsset(instMisil, misilPath);
            DestroyImmediate(instMisil);
        }

        GameObject balaNormal = AssetDatabase.LoadAssetAtPath<GameObject>(prefabsPath + "bala normal.prefab");
        misilBala = AssetDatabase.LoadAssetAtPath<GameObject>(misilPath);

        string[] armas = { "pistola", "escopeta", "rifle de caza", "lanzagranadas" };

        foreach (string arma in armas)
        {
            string soFile = soPath + "ArmaData_" + arma.Replace(" ", "") + ".asset";
            ArmaDataSO armaData = AssetDatabase.LoadAssetAtPath<ArmaDataSO>(soFile);
            if (armaData == null)
            {
                armaData = ScriptableObject.CreateInstance<ArmaDataSO>();
                AssetDatabase.CreateAsset(armaData, soFile);
            }

            if (arma == "pistola") { armaData.velocidadBala = 45f; armaData.tiempoDisparo = 0.2f; armaData.tiempoRecarga = 1.5f; armaData.capacidadCargador = 15; }
            else if (arma == "escopeta") { armaData.velocidadBala = 50f; armaData.tiempoDisparo = 1.0f; armaData.tiempoRecarga = 2.5f; armaData.capacidadCargador = 8; }
            else if (arma == "rifle de caza") { armaData.velocidadBala = 80f; armaData.tiempoDisparo = 1.5f; armaData.tiempoRecarga = 3.0f; armaData.capacidadCargador = 7; }
            else if (arma == "lanzagranadas") { armaData.velocidadBala = 25f; armaData.tiempoDisparo = 2.0f; armaData.tiempoRecarga = 4.0f; armaData.capacidadCargador = 3; }
            EditorUtility.SetDirty(armaData);

            string prefabPath = prefabsPath + arma + ".prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab != null)
            {
                // Usar Object.Instantiate
                GameObject instance = Object.Instantiate(prefab);
                instance.name = arma;

                // Ajustar tamaños relativos
                if (arma != "pistola")
                {
                    instance.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f); 
                }
                
                Rigidbody rb = instance.GetComponent<Rigidbody>();
                if (rb == null) rb = instance.AddComponent<Rigidbody>();

                Collider colW = instance.GetComponent<Collider>();
                if (colW == null)
                {
                    BoxCollider bc = instance.AddComponent<BoxCollider>();
                    bc.size = new Vector3(0.1f, 0.2f, 0.5f);
                }

                XRGrabInteractable grab = instance.GetComponent<XRGrabInteractable>();
                if (grab == null) grab = instance.AddComponent<XRGrabInteractable>();
                
                Transform attachPoint = instance.transform.Find("PuntoAgarre");
                if (attachPoint == null)
                {
                    GameObject apObj = new GameObject("PuntoAgarre");
                    apObj.transform.SetParent(instance.transform);
                    apObj.transform.localPosition = Vector3.zero;
                    attachPoint = apObj.transform;
                }
                grab.attachTransform = attachPoint;

                PistolaVR pistolaVR = instance.GetComponent<PistolaVR>();
                if (pistolaVR == null) pistolaVR = instance.AddComponent<PistolaVR>();

                pistolaVR.datosArma = armaData;
                pistolaVR.balaPrefab = (arma == "lanzagranadas") ? misilBala : balaNormal;

                if (pistolaVR.puntoDeDisparo == null)
                {
                    Transform fp = instance.transform.Find("FirePoint");
                    if (fp == null)
                    {
                        GameObject fpObj = new GameObject("FirePoint");
                        fpObj.transform.SetParent(instance.transform);
                        fpObj.transform.localPosition = new Vector3(0, 0, 0.5f);
                        fp = fpObj.transform;
                    }
                    pistolaVR.puntoDeDisparo = fp;
                }

                if (pistolaVR.textoMunicion == null)
                {
                    Transform txtTrans = attachPoint.Find("TextoMunicion");
                    if (txtTrans == null)
                    {
                        GameObject txtObj = new GameObject("TextoMunicion");
                        txtObj.transform.SetParent(attachPoint); 
                        
                        txtObj.transform.localPosition = new Vector3(0, 0.05f, 0);
                        txtObj.transform.localEulerAngles = new Vector3(0, 0, 0); // No rotar 180 para que no quede al revés
                        
                        // Neutralizar la escala del arma para que el texto siempre sea del mismo tamaño
                        float scaleInvert = 1f / instance.transform.localScale.x;
                        txtObj.transform.localScale = new Vector3(scaleInvert, scaleInvert, scaleInvert);
                        
                        TextMeshPro tmp = txtObj.AddComponent<TextMeshPro>();
                        tmp.text = "0/0";
                        tmp.fontSize = 0.05f; // Tamaño realista para VR
                        tmp.alignment = TextAlignmentOptions.Center;
                        tmp.color = Color.cyan;
                        
                        RectTransform rt = txtObj.GetComponent<RectTransform>();
                        rt.sizeDelta = new Vector2(0.5f, 0.2f);
                        
                        pistolaVR.textoMunicion = tmp;
                    }
                    else
                    {
                        pistolaVR.textoMunicion = txtTrans.GetComponent<TextMeshPro>();
                    }
                }
                else
                {
                    // Si ya existe, aplicamos la corrección de escala y tamaño
                    pistolaVR.textoMunicion.fontSize = 0.05f;
                    pistolaVR.textoMunicion.transform.localEulerAngles = new Vector3(0, 0, 0);
                    float scaleInvert = 1f / instance.transform.localScale.x;
                    pistolaVR.textoMunicion.transform.localScale = new Vector3(scaleInvert, scaleInvert, scaleInvert);
                }

                PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
                DestroyImmediate(instance);
                Debug.Log("Prefab actualizado con éxito: " + arma);
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Todas las armas y misiles configurados correctamente (v3).");
    }
}
