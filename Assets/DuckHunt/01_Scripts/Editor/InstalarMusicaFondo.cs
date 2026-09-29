using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class InstalarMusicaFondo
{
    [MenuItem("DuckHunt/Instalar Musica de Fondo")]
    public static void Instalar()
    {
        string[] escenas = { "MainMenu", "MapaDesierto", "Mapa2", "Mapa3" };
        string[] audios = { "menu", "desierto", "mundo2", "mundo3" };

        for (int i = 0; i < escenas.Length; i++)
        {
            string path = "Assets/DuckHunt/00_Scene/" + escenas[i] + ".unity";
            if (System.IO.File.Exists(path))
            {
                Scene s = EditorSceneManager.OpenScene(path);
                
                // Borrar musica vieja si existe
                GameObject oldMusic = GameObject.Find("MusicaDeFondo");
                if (oldMusic != null) Object.DestroyImmediate(oldMusic);

                GameObject musicObj = new GameObject("MusicaDeFondo");
                AudioSource source = musicObj.AddComponent<AudioSource>();
                
                AudioClip clip = Resources.Load<AudioClip>("Musica/" + audios[i]);
                if (clip != null)
                {
                    source.clip = clip;
                    source.loop = true;
                    source.playOnAwake = true;
                    source.volume = 0.4f; // Volumen moderado para que no tape los disparos
                    Debug.Log("Musica instalada en " + escenas[i]);
                }
                else
                {
                    Debug.LogError("No se encontro Resources/Musica/" + audios[i]);
                }

                EditorSceneManager.SaveScene(s);
            }
        }
        
        Debug.Log("Musica de fondo aplicada a todos los mapas.");
    }
}
