using System;
using System.IO;
using UnityEngine;

/// <summary>
/// Gestiona la lectura y escritura del archivo JSON de guardado en el disco.
/// </summary>
public class SaveFileManager
{
    private readonly string _dirPath;
    private readonly string _fileName;

    public SaveFileManager(string dirPath, string fileName)
    {
        _dirPath = dirPath;
        _fileName = fileName;
    }

    /// <summary>
    /// Carga los datos desde el archivo en disco. Retorna null si no existe o si ocurre un error.
    /// </summary>
    public GameData Load()
    {
        string fullPath = Path.Combine(_dirPath, _fileName);
        if (!File.Exists(fullPath))
        {
            return null;
        }

        try
        {
            string json = File.ReadAllText(fullPath);
            GameData data = JsonUtility.FromJson<GameData>(json);
            return data;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[SaveFileManager] Error al cargar los datos desde {fullPath}: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Guarda el objeto GameData en el archivo de disco en formato JSON legible.
    /// </summary>
    public void Save(GameData data)
    {
        string fullPath = Path.Combine(_dirPath, _fileName);
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath) ?? _dirPath);
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(fullPath, json);
            Debug.Log($"[SaveFileManager] Partida guardada con éxito en: {fullPath}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[SaveFileManager] Error al guardar los datos en {fullPath}: {ex.Message}");
        }
    }

    /// <summary>
    /// Comprueba si el archivo de guardado existe en el disco.
    /// </summary>
    public bool HasSaveFile()
    {
        string fullPath = Path.Combine(_dirPath, _fileName);
        return File.Exists(fullPath);
    }

    /// <summary>
    /// Elimina el archivo de guardado si existe.
    /// </summary>
    public void DeleteSaveFile()
    {
        string fullPath = Path.Combine(_dirPath, _fileName);
        if (File.Exists(fullPath))
        {
            try
            {
                File.Delete(fullPath);
                Debug.Log($"[SaveFileManager] Archivo de guardado eliminado: {fullPath}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveFileManager] Error al eliminar archivo de guardado: {ex.Message}");
            }
        }
    }
}
