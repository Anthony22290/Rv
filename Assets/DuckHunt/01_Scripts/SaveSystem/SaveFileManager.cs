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

    public string FullPath => Path.Combine(_dirPath, _fileName);

    /// <summary>
    /// Carga los datos desde el archivo en disco. Retorna null si no existe o si ocurre un error.
    /// </summary>
    public GameData Load()
    {
        string fullPath = FullPath;
        if (!File.Exists(fullPath))
        {
            return null;
        }

        try
        {
            string json = File.ReadAllText(fullPath);
            GameData data = JsonUtility.FromJson<GameData>(json);

            // Compatibilidad con guardados anteriores que no tenian el campo hasPlayerPosition
            if (data != null && !json.Contains("\"hasPlayerPosition\""))
            {
                data.hasPlayerPosition = data.playerPosition != Vector3.zero;
            }
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
    /// Escribe primero en un archivo temporal para no corromper el guardado si la app se cierra
    /// a mitad de escritura (habitual en Quest al quitarse el visor).
    /// </summary>
    public void Save(GameData data)
    {
        string fullPath = FullPath;
        string tempPath = fullPath + ".tmp";
        try
        {
            Directory.CreateDirectory(_dirPath);
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(tempPath, json);
            File.Copy(tempPath, fullPath, true);
            File.Delete(tempPath);
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
        return File.Exists(FullPath);
    }

    /// <summary>
    /// Elimina el archivo de guardado si existe.
    /// </summary>
    public void DeleteSaveFile()
    {
        string fullPath = FullPath;
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
