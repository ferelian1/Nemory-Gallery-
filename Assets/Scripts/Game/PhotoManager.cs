using System.Collections.Generic;
using UnityEngine;
using System.Linq; // Diperlukan untuk .ToList()

public class PhotoManager : MonoBehaviour
{
    // FUNGSI LAMA (untuk foto dari galeri HP)
    public void AssignPhotosToFrames(List<string> photoPaths, List<Transform> photoFrames)
    {
        if (photoPaths == null || photoFrames == null || photoPaths.Count == 0 || photoFrames.Count == 0) return;

        List<Texture2D> textures = new List<Texture2D>();
        foreach (string path in photoPaths)
        {
            textures.Add(NativeGallery.LoadImageAtPath(path, -1, false));
        }

        // Panggil fungsi utama dengan data tekstur yang sudah di-load
        AssignPhotosToFrames(textures, photoFrames);
    }

    // FUNGSI BARU (untuk foto dari Resources) - INI ADALAH FUNGSI UTAMANYA
    public void AssignPhotosToFrames(List<Texture2D> textures, List<Transform> photoFrames)
    {
        if (textures == null || photoFrames == null || textures.Count == 0 || photoFrames.Count == 0)
        {
            Debug.Log("Tidak ada tekstur atau bingkai untuk ditampilkan.");
            return;
        }

        for (int i = 0; i < photoFrames.Count; i++)
        {
            // Cek apakah kita masih punya foto unik untuk ditampilkan
            if (i < textures.Count)
            {
                Texture2D texture = textures[i];
                if (texture != null)
                {
                    Renderer frameRenderer = photoFrames[i].GetComponent<Renderer>();
                    if (frameRenderer != null)
                    {
                        frameRenderer.material.mainTexture = texture;
                    }
                }
            }
            else
            {
                // Jika sudah tidak ada foto lagi, biarkan bingkai kosong
                // (Kita tidak melakukan apa-apa, jadi bingkai akan memakai material defaultnya)
            }
        }
    }
}