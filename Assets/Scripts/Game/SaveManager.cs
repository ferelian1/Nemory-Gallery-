// SaveManager.cs
using UnityEngine;
using System.IO;
using System.Collections.Generic;

public static class SaveManager
{
    // Kita akan menyimpan file di lokasi ini, aman untuk semua platform
    private static readonly string saveDirectory = Path.Combine(Application.persistentDataPath, "worlds");

    public static void SaveGallery(GalleryData data)
    {
        // Pastikan folder ada
        if (!Directory.Exists(saveDirectory))
        {
            Directory.CreateDirectory(saveDirectory);
        }

        string filePath = Path.Combine(saveDirectory, data.galleryName + ".json");
        string json = JsonUtility.ToJson(data, true); // 'true' untuk format yang rapi
        File.WriteAllText(filePath, json);
        Debug.Log($"Dunia disimpan di: {filePath}");
    }

    public static GalleryData LoadGallery(string galleryName)
    {
        string filePath = Path.Combine(saveDirectory, galleryName + ".json");
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            Debug.Log("file ada di : " + filePath);
            return JsonUtility.FromJson<GalleryData>(json);
        }
        Debug.LogWarning($"File dunia tidak ditemukan: {filePath}");
        return null;
    }


    public static void DeleteGallery(string galleryName)
    {
        string filePath = Path.Combine(saveDirectory, galleryName + ".json");
        if (File.Exists(filePath))
        {
            try
            {
                File.Delete(filePath);
                Debug.Log($"Gallery '{galleryName}' successfully deleted.");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to delete gallery '{galleryName}': {e.Message}");
            }
        }
        else
        {
            Debug.LogWarning($"Attempted to delete a gallery that does not exist: {galleryName}");
        }
    }

    public static List<string> GetAllSavedGalleryNames()
    {
        if (!Directory.Exists(saveDirectory)) return new List<string>();

        var worldNames = new List<string>();
        DirectoryInfo directoryInfo = new DirectoryInfo(saveDirectory);
        foreach (var file in directoryInfo.GetFiles("*.json"))
        {
            worldNames.Add(Path.GetFileNameWithoutExtension(file.Name));
        }
        return worldNames;
    }


}