using UnityEngine;
using UnityEditor;
using System.IO;

public class ConfigurarSonidos
{
    [MenuItem("DuckHunt/Configurar Sonidos Armas")]
    public static void DoFix()
    {
        string[] armas = { "escopeta", "rifle de caza", "lanzagranadas", "pistola" };
        string prefabFolder = "Assets/DuckHunt/03_Prefabs/";
        string soundFolder = "Assets/DuckHunt/04_Sounds/";

        // Load clips
        AudioClip sndPistolaDisparo = AssetDatabase.LoadAssetAtPath<AudioClip>(soundFolder + "pistola disparo.mp3");
        AudioClip sndPistolaRecarga = AssetDatabase.LoadAssetAtPath<AudioClip>(soundFolder + "recarga pistola.mp3");
        
        AudioClip sndRifleDisparo = AssetDatabase.LoadAssetAtPath<AudioClip>(soundFolder + "rifle.mp3");
        AudioClip sndRifleRecarga = AssetDatabase.LoadAssetAtPath<AudioClip>(soundFolder + "reload rifle.mp3");
        
        AudioClip sndLanzaDisparo = AssetDatabase.LoadAssetAtPath<AudioClip>(soundFolder + "lanzagranadas.mp3");
        AudioClip sndLanzaRecarga = AssetDatabase.LoadAssetAtPath<AudioClip>(soundFolder + "lanzagranas recarga.mp3");
        
        AudioClip sndImpactoPato = AssetDatabase.LoadAssetAtPath<AudioClip>(soundFolder + "impactop pato.mp3");
        
        foreach (string arma in armas)
        {
            string prefabPath = prefabFolder + arma + ".prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab != null)
            {
                GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                PistolaVR pvr = instance.GetComponent<PistolaVR>();
                
                if (pvr != null)
                {
                    if (arma == "pistola")
                    {
                        pvr.sonidoDisparo = sndPistolaDisparo;
                        pvr.sonidoRecarga = sndPistolaRecarga;
                    }
                    else if (arma == "rifle de caza")
                    {
                        pvr.sonidoDisparo = sndRifleDisparo;
                        pvr.sonidoRecarga = sndRifleRecarga;
                    }
                    else if (arma == "escopeta")
                    {
                        // Escopeta usa los del rifle por defecto ya que no especifico uno
                        pvr.sonidoDisparo = sndRifleDisparo;
                        pvr.sonidoRecarga = sndRifleRecarga;
                    }
                    else if (arma == "lanzagranadas")
                    {
                        pvr.sonidoDisparo = sndLanzaDisparo;
                        pvr.sonidoRecarga = sndLanzaRecarga;
                        
                        // Agregar sonido de impacto a su bala prefab
                        if (pvr.balaPrefab != null)
                        {
                            string balaPath = AssetDatabase.GetAssetPath(pvr.balaPrefab);
                            GameObject balaInst = PrefabUtility.InstantiatePrefab(pvr.balaPrefab) as GameObject;
                            Bala b = balaInst.GetComponent<Bala>();
                            if (b != null)
                            {
                                b.sonidoImpacto = sndImpactoPato;
                            }
                            PrefabUtility.SaveAsPrefabAsset(balaInst, balaPath);
                            Object.DestroyImmediate(balaInst);
                        }
                    }
                }
                
                PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
                Object.DestroyImmediate(instance);
            }
        }
        AssetDatabase.SaveAssets();
        Debug.Log("Sonidos configurados exitosamente en todas las armas y balas.");
    }
}
 
