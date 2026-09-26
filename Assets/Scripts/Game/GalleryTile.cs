using UnityEngine;

public class GalleryTile : MonoBehaviour
{
    // HANYA KONTROL UJUNG LORONG (DEPAN & BELAKANG)
    [Header("Main Openings")]
    public GameObject FrontDoor; // Di prefabmu ini adalah NorthDoor
    public GameObject FrontWall; // Di prefabmu ini adalah NorthWall
    public GameObject BackDoor;  // Di prefabmu ini adalah SouthDoor
    public GameObject BackWall;  // Di prefabmu ini adalah SouthWall

    // Khusus untuk Hub
    [Header("Side Openings (For Hub Only)")]
    public GameObject EastDoor;
    public GameObject WestDoor;

    /// <summary>
    /// Mengatur koneksi untuk lorong. Hanya peduli depan dan belakang.
    /// </summary>
    public void SetupCorridorConnections(bool openFront, bool openBack)
    {
        if (FrontDoor != null) FrontDoor.SetActive(openFront);
        if (FrontWall != null) FrontWall.SetActive(!openFront);

        if (BackDoor != null) BackDoor.SetActive(openBack);
        if (BackWall != null) BackWall.SetActive(!openBack);
    }
    
    public void SetupHubConnections(bool hasNorth, bool hasEast, bool hasSouth, bool hasWest)
{
    // Logika untuk Arah Utara
    if (FrontDoor != null) FrontDoor.SetActive(hasNorth);
    if (FrontWall != null) FrontWall.SetActive(!hasNorth);

    // Logika untuk Arah Selatan
    if (BackDoor != null) BackDoor.SetActive(hasSouth);
    if (BackWall != null) BackWall.SetActive(!hasSouth);

    // Logika untuk Arah Timur
    if (EastDoor != null) EastDoor.SetActive(hasEast);
    GameObject EastWall = transform.Find("EastWall")?.gameObject; 
    if(EastWall != null) EastWall.SetActive(!hasEast);

    // Logika untuk Arah Barat
    if (WestDoor != null) WestDoor.SetActive(hasWest);
    GameObject WestWall = transform.Find("WestWall")?.gameObject;
    if(WestWall != null) WestWall.SetActive(!hasWest);
}
}