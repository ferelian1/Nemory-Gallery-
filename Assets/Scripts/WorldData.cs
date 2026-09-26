// WorldData.cs
using System.Collections.Generic;
using UnityEngine;

// [System.Serializable] penting agar Unity bisa mengubahnya ke format JSON
[System.Serializable]
public class TileData
{
    public bool isHub;
    public Vector2Int position;
    public Quaternion rotation;
}

[System.Serializable]
public class GalleryData
{
    public string galleryName;
    public List<string> photoPaths;
    public List<TileData> layout;

    public GalleryData(string name, List<string> photos, List<TileData> lo)
    {
        galleryName = name;
        photoPaths = photos;
        layout = lo;
    }
}