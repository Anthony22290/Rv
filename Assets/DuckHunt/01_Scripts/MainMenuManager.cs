using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Configuración de Escena")]
    [Tooltip("Nombre de la escena a cargar al presionar Jugar")]
    public string gameSceneName = "MapaDesierto";

    /// <summary>
    /// Inicia la partida cargando la escena principal del juego.
    /// </summary>
    public void Jugar()
    {
        Debug.Log("[MainMenu] Cargando escena del juego: " + gameSceneName);
        PlayerHUD.ResetScore();
        if (!string.IsNullOrEmpty(gameSceneName))
        {
            SceneManager.LoadScene(gameSceneName);
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }

    /// <summary>
    /// Cierra el juego o detiene el modo de juego en el editor de Unity.
    /// </summary>
    public void Salir()
    {
        Debug.Log("[MainMenu] Saliendo del juego...");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
