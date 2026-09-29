using UnityEngine;
using UnityEditor;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class HolographicAmmoPlacer
{
    [MenuItem("DuckHunt/Colocar Municion Estilo Shooter")]
    public static void PlaceAmmo()
    {
        string[] armas = { "pistola", "escopeta", "rifle de caza", "lanzagranadas" };
        string prefabsPath = "Assets/DuckHunt/03_Prefabs/";
        
        foreach (string arma in armas)
        {
            string prefabPath = prefabsPath + arma + ".prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null) continue;
            
            GameObject instance = Object.Instantiate(prefab);
            instance.transform.position = Vector3.zero;
            instance.transform.rotation = Quaternion.identity;
            
            // Limpiar UI antigua
            Transform[] allChildren = instance.GetComponentsInChildren<Transform>(true);
            foreach (Transform t in allChildren)
            {
                if (t != null && (t.name.Contains("TextoMunicion") || t.name.Contains("CanvasMunicion") || t.name.Contains("ReloadIcon")))
                {
                    Object.DestroyImmediate(t.gameObject);
                }
            }

            GameObject holoObj = new GameObject("TextoMunicion");
            holoObj.transform.SetParent(instance.transform);
            
            PistolaVR pvr = instance.GetComponent<PistolaVR>();
            XRGrabInteractable grab = instance.GetComponent<XRGrabInteractable>();
            
            // Usar el punto exacto donde la mano agarra el arma
            Transform attachPoint = instance.transform;
            if (grab != null && grab.attachTransform != null) attachPoint = grab.attachTransform;

            Vector3 rootPos = attachPoint.position;
            Vector3 forwardDir = instance.transform.forward;
            Vector3 upDir = instance.transform.up;

            if (pvr != null && pvr.puntoDeDisparo != null)
            {
                forwardDir = pvr.puntoDeDisparo.forward;
                upDir = pvr.puntoDeDisparo.up;
            }

            // Subir 7 cm sobre la mano, y avanzar 4 cm.
            Vector3 backTopPos = rootPos + (upDir * 0.07f) + (forwardDir * 0.04f);
            
            holoObj.transform.position = backTopPos;
            
            // Mirando directamente al jugador
            holoObj.transform.rotation = Quaternion.LookRotation(-forwardDir, upDir);
            holoObj.transform.Rotate(30f, 0f, 0f, Space.Self);
            
            TextMeshPro tmp = holoObj.AddComponent<TextMeshPro>();
            tmp.text = "12/30";
            tmp.fontSize = 2f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.fontStyle = FontStyles.Bold;
            
            GameObject background = GameObject.CreatePrimitive(PrimitiveType.Quad);
            background.name = "FondoOscuro";
            background.transform.SetParent(holoObj.transform);
            background.transform.localPosition = new Vector3(0, 0, 0.005f); // un poco detras
            background.transform.localRotation = Quaternion.identity;
            background.transform.localScale = new Vector3(0.6f, 0.3f, 1f); 
            
            // Reparar magenta en URP
            Shader s = Shader.Find("Universal Render Pipeline/Unlit");
            if (s == null) s = Shader.Find("Unlit/Color");
            Material bgMat = new Material(s);
            if (s.name.Contains("Universal"))
            {
                bgMat.SetColor("_BaseColor", new Color(0.1f, 0.1f, 0.1f, 0.85f));
                bgMat.SetFloat("_Surface", 1); // Transparent
            }
            else
            {
                bgMat.color = new Color(0.1f, 0.1f, 0.1f, 0.85f);
                bgMat.SetFloat("_Mode", 3);
            }
            
            background.GetComponent<Renderer>().material = bgMat;
            Object.DestroyImmediate(background.GetComponent<Collider>());

            // Escala absoluta de 2cm independientemente de si el prefab esta escalado a 100
            Vector3 parentScale = instance.transform.lossyScale;
            holoObj.transform.localScale = new Vector3(0.04f / parentScale.x, 0.04f / parentScale.y, 0.04f / parentScale.z);
            
            if (pvr != null)
            {
                pvr.textoMunicion = tmp;
                pvr.ActualizarTextoMunicion(); // para que diga los numeros reales del arma
            }
            
            PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
            Object.DestroyImmediate(instance);
        }
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("CONTADORES ARREGLADOS.");
    }
}
 
