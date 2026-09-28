using UnityEngine;
using UnityEditor;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using TMPro;

public class WeaponFixer
{
    [MenuItem("DuckHunt/URGENT FIX WEAPONS")]
    public static void FixAllWeapons()
    {
        string[] armas = { "pistola", "escopeta", "rifle de caza", "lanzagranadas" };
        string prefabsPath = "Assets/DuckHunt/03_Prefabs/";
        
        foreach (string arma in armas)
        {
            string prefabPath = prefabsPath + arma + ".prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null) continue;
            
            GameObject instance = Object.Instantiate(prefab);
            
            // 1. Restore Grip: Delete PuntoAgarre completely
            Transform pa = instance.transform.Find("PuntoAgarre");
            if (pa != null) Object.DestroyImmediate(pa.gameObject);
            
            XRGrabInteractable grab = instance.GetComponent<XRGrabInteractable>();
            if (grab != null) grab.attachTransform = null; // Revert to root
            
            // 2. Destroy ALL old texts and canvases
            Transform[] allChildren = instance.GetComponentsInChildren<Transform>(true);
            foreach (Transform t in allChildren)
            {
                if (t != null && (t.name.Contains("TextoMunicion") || t.name.Contains("CanvasMunicion")))
                {
                    Object.DestroyImmediate(t.gameObject);
                }
            }
            
            // 3. Create simple elegant text at the back of the gun
            PistolaVR pvr = instance.GetComponent<PistolaVR>();
            if (pvr != null && pvr.puntoDeDisparo != null)
            {
                GameObject txtObj = new GameObject("TextoMunicion");
                txtObj.transform.SetParent(instance.transform);
                
                // Position it 30cm behind the fire point and 5cm up, aligned with firepoint's rotation
                txtObj.transform.position = pvr.puntoDeDisparo.position - pvr.puntoDeDisparo.forward * 0.35f + pvr.puntoDeDisparo.up * 0.05f;
                txtObj.transform.rotation = pvr.puntoDeDisparo.rotation; // Facing forward
                txtObj.transform.Rotate(0, 180, 0); // Face the player
                
                // Make it tiny and pure
                TextMeshPro tmp = txtObj.AddComponent<TextMeshPro>();
                tmp.text = "0/0";
                tmp.fontSize = 0.5f;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.color = Color.white;
                
                // Neutralize scale so it's consistently sized
                float scaleInvert = 1f / instance.transform.localScale.x;
                txtObj.transform.localScale = new Vector3(scaleInvert * 0.05f, scaleInvert * 0.05f, scaleInvert * 0.05f);
                
                pvr.textoMunicion = tmp;
            }
            
            PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
            Object.DestroyImmediate(instance);
        }
        
        // Remove WeaponSetupHelper to prevent it from running again
        AssetDatabase.DeleteAsset("Assets/DuckHunt/01_Scripts/Editor/WeaponSetupHelper.cs");
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("ARMAS ARREGLADAS Y RESTAURADAS");
    }
}
