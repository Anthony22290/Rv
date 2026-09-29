using UnityEngine;

/// <summary>
/// Componente que se coloca en el jugador / XR Origin para guardar y restaurar
/// su posicion, rotacion, vida actual y puntuacion.
/// </summary>
public class PlayerSaveable : MonoBehaviour, ISaveable
{
    public void SaveData(GameData data)
    {
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
        // Restaurar posicion si es valida (no es el vector por defecto si no corresponde)
        if (data.playerPosition != Vector3.zero)
        {
            // Desactivar temporalmente el CharacterController si existiera para reposicionar limpiamente
            CharacterController cc = GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            transform.position = data.playerPosition;
            transform.rotation = data.playerRotation;

            if (cc != null) cc.enabled = true;
        }

        // Restaurar vida
        if (PlayerHealth.Instance != null && data.playerHealth > 0)
        {
            PlayerHealth.Instance.currentHealth = data.playerHealth;
            if (PlayerHUD.Instance != null)
            {
                PlayerHUD.Instance.UpdateHealth(PlayerHealth.Instance.currentHealth, PlayerHealth.Instance.maxHealth);
            }
        }

        // Restaurar puntuacion
        PlayerHUD.SetScore(data.score);

        Debug.Log($"[PlayerSaveable] Datos restaurados -> Posicion: {transform.position}, Vida: {data.playerHealth}, Puntos: {data.score}");
    }
}
