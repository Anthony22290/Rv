/// <summary>
/// Interfaz que implementan los componentes que requieren persistencia de datos.
/// </summary>
public interface ISaveable
{
    /// <summary>
    /// Guarda el estado del componente en el objeto GameData.
    /// </summary>
    void SaveData(GameData data);

    /// <summary>
    /// Restaura el estado del componente a partir del objeto GameData.
    /// </summary>
    void LoadData(GameData data);
}
