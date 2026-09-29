using UnityEngine;

/// <summary>
/// Componente que se coloca en el jugador / XR Origin para guardar y restaurar
/// su posicion, rotacion, vida actual y puntuacion.
/// </summary>
public class PlayerSaveable : MonoBehaviour, ISaveable
{
    public void SaveData(GameData data)
    {
        data.hasPlayerPosition = true;
        data.playerPosition = transform.position;
        data.playerRotation = transform.rotation;

        if (PlayerHealth.Instance != null)
        {
            data.playerHealth = PlayerHealth.Instance.currentHealth;
        }

        data.score = PlayerHUD.totalScore;
    }

    public void LoadData(GameData data)
    {
        // Sin posicion guardada (checkpoint de inicio de nivel) el jugador se queda en el spawn del mapa
        if (data.hasPlayerPosition)
        {
            // En VR el XR Origin lleva CharacterController: hay que desactivarlo para poder teletransportarlo
            CharacterController cc = GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            transform.SetPositionAndRotation(data.playerPosition, data.playerRotation);

            if (cc != null) cc.enabled = true;
        }

        // Restaurar vida (limitada a la vida maxima de este mapa)
        if (PlayerHealth.Instance != null && data.playerHealth > 0)
        {
            PlayerHealth.Instance.currentHealth = Mathf.Min(data.playerHealth, PlayerHealth.Instance.maxHealth);
            if (PlayerHUD.Instance != null)
            {
                PlayerHUD.Instance.UpdateHealth(PlayerHealth.Instance.currentHealth, PlayerHealth.Instance.maxHealth);
            }
        }

        // Restaurar puntuacion
        PlayerHUD.SetScore(data.score);

        Debug.Log($"[PlayerSaveable] Datos restaurados -> Posicion: {transform.position}, Vida: {(PlayerHealth.Instance != null ? PlayerHealth.Instance.currentHealth : data.playerHealth)}, Puntos: {data.score}");
    }
}
