using UnityEngine;
using UnityEngine.UI; // Wajib ada untuk mengakses InputField dan Button
using System.Collections.Generic;
using System.Linq;
using TMPro;
using System.IO;
using System.Collections;



public class GameManager : MonoBehaviour
{

    [SerializeField] private MapManager mapManager;
    [SerializeField] private PhotoManager photoManager;
    [SerializeField] private GameObject player;
    [SerializeField] private Transform startPoint;
    [SerializeField] private GameObject mainCamera;

    [Header("UI Panels")]
    [SerializeField] private GameObject panelMainMenu;
    [SerializeField] private GameObject panelGalleryList;
    [SerializeField] private GameObject panelCreateGallery;
    [SerializeField] private GameObject panelInGameUI;

    [Header("UI Elements")]
    [SerializeField] private TMP_InputField createGalleryInputField; // Drag InputField ke sini
    [SerializeField] private GameObject galleryLoadButtonTemplate; // Drag tombol template ke sini
    [SerializeField] private Transform galleryListContent; // Drag 'Content' dari ScrollView ke sini
    [SerializeField] private TMP_Text statusTextUI;


    [Header("Delete Confirmation UI")]
    [SerializeField] private GameObject panelDeleteConfirm;
    [SerializeField] private TMP_Text deleteConfirmText;
    [SerializeField] private Button confirmDeleteButton;
    [SerializeField] private Button cancelDeleteButton;

    private string currentGalleryName;
    private List<string> currentPhotoPaths;
    private GalleryData currentGalleryData;
    private Texture2D lastCroppedTexture;
    private bool cropResult_Success;
    private Coroutine statusCoroutine;

    void Start()
    {
        // Secara default, nonaktifkan semua panel di awal
        ShowMainMenuPanel();

        if (statusTextUI != null) statusTextUI.gameObject.SetActive(false);
    }


    private void ShowStatus(string message, float duration = 2f)
    {
        if (statusTextUI == null) return;
        if (statusCoroutine != null)
        {
            StopCoroutine(statusCoroutine);
        }
        statusCoroutine = StartCoroutine(StatusDisplayRoutine(message, duration));
    }
    private IEnumerator StatusDisplayRoutine(string message, float duration)
    {
        statusTextUI.gameObject.SetActive(true);
        statusTextUI.text = message;
        yield return new WaitForSeconds(duration);
        statusTextUI.gameObject.SetActive(false);
    }
    

    // --- Navigasi UI ---

    public void ShowMainMenuPanel()
    {
        panelMainMenu.SetActive(true);
        panelGalleryList.SetActive(false);
        panelCreateGallery.SetActive(false);
        panelInGameUI.SetActive(false);
        player.SetActive(false);
    }

    public void ShowCreateGalleryPanel()
    {
        panelMainMenu.SetActive(false);
        panelCreateGallery.SetActive(true);
    }

    public void ShowLoadGalleryPanel()
    {
        panelMainMenu.SetActive(false);
        panelGalleryList.SetActive(true);

        // Hapus daftar lama
        foreach (Transform child in galleryListContent)
        {
            if (child.gameObject != galleryLoadButtonTemplate) // Jangan hapus template
                Destroy(child.gameObject);
        }

        // Buat daftar baru
        List<string> galleryNames = SaveManager.GetAllSavedGalleryNames();
        galleryLoadButtonTemplate.SetActive(false); // Sembunyikan template
        foreach (string name in galleryNames)
        {
            GameObject newButtonRow = Instantiate(galleryLoadButtonTemplate, galleryListContent);
            
            newButtonRow.GetComponentInChildren<TMP_Text>().text = name; // Ganti Text atau TextMeshProUGUI
            TMP_Text galleryNameText = newButtonRow.transform.Find("GalleryName").GetComponent<TMP_Text>();
            Button addButton = newButtonRow.transform.Find("Button_AddPhoto").GetComponent<Button>();
            Button deleteButton = newButtonRow.transform.Find("Button_DeleteGallery").GetComponent<Button>();
            

            string galleryNameToLoad = name;
            // Tombol Load sekarang adalah komponen Button di objek induk itu sendiri
            Button loadButton = newButtonRow.GetComponent<Button>();
            
            newButtonRow.GetComponent<Button>().onClick.AddListener(() => OnClickLoadGallery(galleryNameToLoad));
            addButton.onClick.AddListener(() => OnClickAddPhotos(galleryNameToLoad)); 
            deleteButton.onClick.AddListener(() => OnClickDeleteGallery(galleryNameToLoad)); 

            newButtonRow.SetActive(true);
        }
    }

    public void ExitToMainMenu()
    {
        mapManager.ClearGallery();
        ShowMainMenuPanel();
        mainCamera.SetActive(true);
    }


    public void OnClickCreateNewGallery()
    {
        currentGalleryName = createGalleryInputField.text;
        if (string.IsNullOrWhiteSpace(currentGalleryName))
        {
            ShowStatus("Gallery name cannot empty!", 1f);
            return;
        }

        NativeGallery.GetImagesFromGallery((paths) =>
        {
            if (paths != null && paths.Length > 0)
            {
                // Cukup panggil satu coroutine utama ini
                StartCoroutine(StartCroppingProcess(paths, true, null));
            }
        }, "Select Your Photos", "image/*");
    }
    private IEnumerator StartCroppingProcess(string[] paths, bool isCreatingNew, GalleryData existingGallery)
    {
        if (isCreatingNew)
        {
            ShowStatus($"Starting... {paths.Length} photos chosen.", 1f);
        }
        else
        {
            ShowStatus($"Adding {paths.Length} new photos...", 1f);
        }
        yield return new WaitForSeconds(3f);
        
        var successfullyCroppedPaths = new List<string>();
        mainCamera.SetActive(false);

        for (int i = 0; i < paths.Length; i++)
        {
            // ... (logika cropping sama persis seperti sebelumnya) ...
            string currentPath = paths[i];
            Texture2D lastCroppedTexture = null;
            bool cropResult_Success = false;

            Texture2D textureToCrop = NativeGallery.LoadImageAtPath(currentPath, -1, false);
            if (textureToCrop == null) continue;

            ImageCropper.Instance.Show(
                textureToCrop,
                (result, originalImage, croppedImage) =>
                {
                    cropResult_Success = result;
                    lastCroppedTexture = croppedImage;
                    if (originalImage != null) Destroy(originalImage);
                },
                new ImageCropper.Settings() { autoZoomEnabled = true, selectionMinAspectRatio = 1.0f, selectionMaxAspectRatio = 1.0f, markTextureNonReadable = false }
            );

            yield return new WaitUntil(() => !ImageCropper.Instance.IsOpen);
            yield return null;

            if (cropResult_Success && lastCroppedTexture != null)
            {
                // ... (logika penyimpanan file sama persis) ...
                byte[] bytes = lastCroppedTexture.EncodeToPNG();
                string fileName = "cropped_" + System.Guid.NewGuid().ToString() + ".png";
                string filePath = Path.Combine(Application.persistentDataPath, fileName);
                File.WriteAllBytes(filePath, bytes);
                successfullyCroppedPaths.Add(filePath);
                Destroy(lastCroppedTexture);
            }
        }
        
        // --- Logika Setelah Selesai Cropping ---
        if (isCreatingNew)
        {
            // Jika membuat galeri baru
            if (successfullyCroppedPaths.Count > 0)
            {
                currentPhotoPaths = successfullyCroppedPaths;
                mapManager.GenerateGallery(currentPhotoPaths.Count);
                List<TileData> layout = mapManager.GetLayoutData();
                currentGalleryData = new GalleryData(currentGalleryName, currentPhotoPaths, layout);
                SaveManager.SaveGallery(currentGalleryData);
                EnterGallery(true);
            }
            else
            {
                ShowStatus("Gallery creation cancelled.", 1f);
                yield return new WaitForSeconds(3f);
                ExitToMainMenu();
            }
        }
        else
        {
            // Jika menambahkan ke galeri yang ada
            existingGallery.photoPaths.AddRange(successfullyCroppedPaths);
            mapManager.GenerateGallery(existingGallery.photoPaths.Count);
            existingGallery.layout = mapManager.GetLayoutData();
            SaveManager.SaveGallery(existingGallery);

            ShowStatus("Photos added successfully!", 1f);
            yield return new WaitForSeconds(2f);
            OnClickLoadGallery(existingGallery.galleryName); // Muat ulang galeri yang sudah diperbarui
        }
    }
     public void OnClickAddPhotos(string galleryName)
    {
        GalleryData galleryToUpdate = SaveManager.LoadGallery(galleryName);
        if (galleryToUpdate == null)
        {
            ShowStatus("Failed to load gallery data!");
            return;
        }

        NativeGallery.GetImagesFromGallery((newPaths) =>
        {
            if (newPaths != null && newPaths.Length > 0)
            {
                // Panggil coroutine yang sama, tapi dengan mode 'bukan membuat baru'
                StartCoroutine(StartCroppingProcess(newPaths, false, galleryToUpdate));
            }
        }, "Select new photos");
    }

    public void OnClickDeleteGallery(string galleryName)
    {
        panelDeleteConfirm.SetActive(true);
        deleteConfirmText.text = $"Are you sure you want to delete '{galleryName}'? This can't be undone.";

        // Hapus listener lama untuk mencegah bug, lalu tambahkan yang baru
        confirmDeleteButton.onClick.RemoveAllListeners();
        confirmDeleteButton.onClick.AddListener(() =>
        {
            ConfirmDelete(galleryName);
        });

        cancelDeleteButton.onClick.RemoveAllListeners();
        cancelDeleteButton.onClick.AddListener(() =>
        {
            panelDeleteConfirm.SetActive(false);
        });
    }

    private void ConfirmDelete(string galleryName)
    {
        panelDeleteConfirm.SetActive(false);
        SaveManager.DeleteGallery(galleryName);
        ShowStatus($"Gallery '{galleryName}' deleted.", 2f);
        ShowLoadGalleryPanel(); // Refresh daftar galeri
    }


    public void OnClickLoadGallery(string galleryNameToLoad)
    {
        Debug.Log("Mencoba memuat dunia: " + galleryNameToLoad);
        currentGalleryData = SaveManager.LoadGallery(galleryNameToLoad);
        if (currentGalleryData != null)
        {
            currentGalleryName = currentGalleryData.galleryName;

            // Bangun layout galeri dari data save-an
            mapManager.BuildGalleryFromData(currentGalleryData.layout);

            // INI BAGIAN PENTING: Cek nama dunia untuk menentukan cara memuat foto
            if (galleryNameToLoad == "For You")
            {
                // Jika ini dunia kejutan, JANGAN gunakan path.
                Debug.Log("Memuat foto untuk dunia kejutan dari Resources.");
                EnterGallery(false); // Masuk ke scene

                // Langsung muat Texture2D dari Resources dan kirim ke PhotoManager
                Object[] surpriseTextures = Resources.LoadAll("SurprisePhotos", typeof(Texture2D));
                List<Texture2D> textures = surpriseTextures.Cast<Texture2D>().ToList();
                photoManager.AssignPhotosToFrames(textures, mapManager.GetPhotoFrames());
            }
            else
            {
                // Jika ini dunia biasa, gunakan path dari galeri HP.
                Debug.Log("Memuat foto untuk dunia '" + galleryNameToLoad + "' dari path galeri.");
                currentPhotoPaths = currentGalleryData.photoPaths;
                EnterGallery(true); // Masuk ke scene
            }
        }
        else
        {
            Debug.LogError("Gagal memuat data untuk dunia: " + galleryNameToLoad);
        }
    }
    private void EnterGallery(bool assignFromPath)
    {
        mainCamera.SetActive(false);
        panelMainMenu.SetActive(false);
        panelGalleryList.SetActive(false);
        panelCreateGallery.SetActive(false);
        // Aktifkan UI dalam game
        panelInGameUI.SetActive(true);

        if (player != null && startPoint != null)
        {
            player.transform.position = startPoint.position;
            player.SetActive(true);
        }

        if (assignFromPath)
        {
            if (currentPhotoPaths != null && currentPhotoPaths.Count > 0)
            {
                List<Transform> frames = mapManager.GetPhotoFrames();
                photoManager.AssignPhotosToFrames(currentPhotoPaths, mapManager.GetPhotoFrames());
            }
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayRandomMusic();
        }
    }
}