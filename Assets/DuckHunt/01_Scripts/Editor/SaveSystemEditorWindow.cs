using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Ventana de herramientas en Unity Editor para inspeccionar, probar y gestionar las partidas guardadas.
/// Accesible desde el menu: Duck Hunt VR -> Sistema de Guardado
/// </summary>
public class SaveSystemEditorWindow : EditorWindow
{
    private string saveFilePath;
    private string fileContent = "";
    private Vector2 scrollPos;

    [MenuItem("Duck Hunt VR/Sistema de Guardado", false, 50)]
    public static void ShowWindow()
    {
        var window = GetWindow<SaveSystemEditorWindow>("Sistema de Guardado");
        window.minSize = new Vector2(450, 400);
        window.Show();
    }

    private void OnEnable()
    {
        saveFilePath = Path.Combine(Application.persistentDataPath, "duckhunt_save.json");
        RefreshStatus();
    }

    private void RefreshStatus()
    {
        if (File.Exists(saveFilePath))
        {
            try
            {
                fileContent = File.ReadAllText(saveFilePath);
            }
            catch (System.Exception ex)
            {
                fileContent = "Error al leer archivo: " + ex.Message;
            }
        }
        else
        {
            fileContent = "No existe archivo de guardado actualmente.";
        }
    }

    private void OnGUI()
    {
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Panel de Control: Save & Load System", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);

        EditorGUILayout.HelpBox($"Ruta del archivo:\n{saveFilePath}", MessageType.Info);

        bool existe = File.Exists(saveFilePath);

        EditorGUILayout.Space(10);
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Refrescar", GUILayout.Height(30)))
        {
            RefreshStatus();
        }

        if (GUILayout.Button("Abrir Carpeta en Explorador", GUILayout.Height(30)))
        {
            if (Directory.Exists(Application.persistentDataPath))
            {
                EditorUtility.RevealInFinder(saveFilePath);
            }
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(5);

        GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);
        if (GUILayout.Button("Eliminar Partida Guardada", GUILayout.Height(32)))
        {
            if (existe)
            {
                if (EditorUtility.DisplayDialog("Confirmar Eliminación", "¿Estás seguro de que deseas borrar la partida guardada?", "Sí, borrar", "Cancelar"))
                {
                    File.Delete(saveFilePath);
                    RefreshStatus();
                    Debug.Log("[SaveSystem] Partida guardada eliminada desde el Editor.");
                }
            }
            else
            {
                EditorUtility.DisplayDialog("Aviso", "No hay ningún archivo de guardado para borrar.", "Aceptar");
            }
        }
        GUI.backgroundColor = Color.white;

        EditorGUILayout.Space(15);
        EditorGUILayout.LabelField("Contenido del Archivo de Guardado:", EditorStyles.boldLabel);

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(180));
        EditorGUILayout.TextArea(fileContent, GUILayout.ExpandHeight(true));
        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space(10);
        if (Application.isPlaying)
        {
            EditorGUILayout.LabelField("Acciones en Modo Play:", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Forzar Guardado (Save Now)", GUILayout.Height(28)))
            {
                if (SaveLoadManager.Instance != null)
                {
                    SaveLoadManager.Instance.SaveGame();
                    RefreshStatus();
                }
            }

            if (GUILayout.Button("Cargar Guardado (Load Now)", GUILayout.Height(28)))
            {
                if (SaveLoadManager.Instance != null)
                {
                    SaveLoadManager.Instance.LoadLastSavedGame();
                }
            }

            EditorGUILayout.EndHorizontal();
        }
    }
}
