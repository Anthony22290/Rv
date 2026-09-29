using UnityEngine;

public class DarPistolaAlEmpezar : MonoBehaviour
{
    void Start()
    {
        // Solo instanciar si en la escena no existe ya una pistola (para no llenar de pistolas si reinician)
        PistolaVR[] pistolas = FindObjectsByType<PistolaVR>(FindObjectsSortMode.None);
        bool tienePistola = false;
        foreach (var p in pistolas)
        {
            if (p.gameObject.name.ToLower().Contains("pistola"))
            {
                tienePistola = true;
                break;
            }
        }

        if (!tienePistola)
        {
#if UNITY_EDITOR
            GameObject prefabPistola = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/DuckHunt/03_Prefabs/pistola.prefab");
            if (prefabPistola != null)
            {
                GameObject p = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(prefabPistola);
                // Ponersela en la cara/pecho para que caiga a sus pies o la agarre
                p.transform.position = transform.position + Vector3.up * 1.2f + transform.forward * 0.4f;
            }
#else
            // En build final, idealmente se usa Resources, pero como es un editor script, lo metemos directamente
            // usando un enlace directo si lo configuraramos, pero para arreglarlo rapido en todas las escenas,
            // mejor cargar desde un prefab en scene si quisieramos.
#endif
        }
    }
}
