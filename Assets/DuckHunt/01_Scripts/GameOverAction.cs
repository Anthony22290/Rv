using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverAction : MonoBehaviour
{
    public bool isRestart;
    public string menuSceneName;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bala") || other.name.ToLower().Contains("bala") || other.name.ToLower().Contains("bullet"))
        {
            if (isRestart)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
            else
            {
                if (!string.IsNullOrEmpty(menuSceneName))
                    SceneManager.LoadScene(menuSceneName);
                else
                    SceneManager.LoadScene(0); // Cargar primer escenario (menú) por defecto
            }
        }
    }
}
