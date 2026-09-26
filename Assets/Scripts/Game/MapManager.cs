using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MapManager : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject hubPrefab;
    public List<GameObject> corridorPrefabs;

    [Header("Generation Settings")]
    public int moduleSize = 10;
    [Tooltip("Panjang minimal setiap cabang koridor")]
    public int minBranchLength = 1;
    [Tooltip("Panjang maksimal setiap cabang koridor")]
    public int maxBranchLength = 3;


    private const string HUB_TAG = "Hub";
    private const string PHOTOFRAME_TAG = "PhotoFrame";
    // Menyimpan semua tile yang sudah di-spawn
    private Dictionary<Vector2Int, GalleryTile> spawnedTiles = new Dictionary<Vector2Int, GalleryTile>();
    // Menyimpan semua transform dari bingkai foto
    private List<Transform> photoFrames = new List<Transform>();


    public void GenerateGallery(int photoCount)
    {
        ClearGallery();
        if (photoCount <= 0) return;
        int corridorsNeeded = Mathf.CeilToInt(photoCount / 4.0f);
        int corridorsBuilt = 0;

        List<Vector2Int> activeHubs = new List<Vector2Int>();

        Vector2Int startPos = Vector2Int.zero;
        SpawnTile(hubPrefab, startPos, Quaternion.identity);
        activeHubs.Add(startPos);

        while (corridorsBuilt < corridorsNeeded && activeHubs.Count > 0)
        {
            int randomIndex = Random.Range(0, activeHubs.Count);
            Vector2Int currentHubPos = activeHubs[randomIndex];

            List<Vector2Int> availableDirections = GetAvailableDirections(currentHubPos);

            if (availableDirections.Count == 0)
            {
                activeHubs.RemoveAt(randomIndex);
                continue;
            }

            Vector2Int direction = availableDirections[Random.Range(0, availableDirections.Count)];

            int branchLength = Random.Range(minBranchLength, maxBranchLength + 1);
            Vector2Int currentPos = currentHubPos;

            for (int i = 0; i < branchLength && corridorsBuilt < corridorsNeeded; i++)
            {
                currentPos += direction;
                if (spawnedTiles.ContainsKey(currentPos)) break;

                Quaternion rotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.y));

                GameObject RandomCorridor = corridorPrefabs[Random.Range(0, corridorPrefabs.Count)];
                SpawnTile(RandomCorridor, currentPos, rotation);
                corridorsBuilt++;
            }

            Vector2Int nextHubPos = currentPos + direction;
            if (corridorsBuilt < corridorsNeeded && !spawnedTiles.ContainsKey(nextHubPos))
            {
                SpawnTile(hubPrefab, nextHubPos, Quaternion.identity);
                if (!activeHubs.Contains(nextHubPos))
                {
                    activeHubs.Add(nextHubPos);
                }
            }
        }

        UpdateAllTileConnections();
    }

    private List<Vector2Int> GetAvailableDirections(Vector2Int pos)
    {
        List<Vector2Int> directions = new List<Vector2Int> { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        List<Vector2Int> available = new List<Vector2Int>();
        foreach (var dir in directions)
        {
            if (!spawnedTiles.ContainsKey(pos + dir)) available.Add(dir);
        }
        return available;
    }

    private void SpawnTile(GameObject prefab, Vector2Int gridPos, Quaternion rotation)
    {
        Vector3 worldPos = new Vector3(gridPos.x * moduleSize, 0, gridPos.y * moduleSize);
        GameObject newTileObj = Instantiate(prefab, worldPos, rotation, transform);
        newTileObj.name = $"{prefab.name}_{gridPos.x}_{gridPos.y}";
        
        GalleryTile newTile = newTileObj.GetComponent<GalleryTile>();
        spawnedTiles.Add(gridPos, newTile);

        // Mengumpulkan bingkai foto menggunakan Tag
        if (!newTile.CompareTag(HUB_TAG))
        {
            foreach (Transform t in newTileObj.GetComponentsInChildren<Transform>())
            {
                if (t.CompareTag(PHOTOFRAME_TAG))
                {
                    photoFrames.Add(t);
                }
            }
        }
    }

    private void UpdateAllTileConnections()
    {
        foreach (var entry in spawnedTiles)
        {
            Vector2Int pos = entry.Key;
            GalleryTile tile = entry.Value;
            bool isHub = tile.gameObject.name.Contains("Hub");

            if (isHub)
            {
                bool hasNorth = spawnedTiles.ContainsKey(pos + Vector2Int.up);
                bool hasSouth = spawnedTiles.ContainsKey(pos + Vector2Int.down);
                bool hasEast = spawnedTiles.ContainsKey(pos + Vector2Int.right);
                bool hasWest = spawnedTiles.ContainsKey(pos + Vector2Int.left);
                tile.SetupHubConnections(hasNorth, hasEast, hasSouth, hasWest);
            }
            else
            {
                Vector3 forward = tile.transform.forward;
                Vector3 back = -tile.transform.forward;
                Vector2Int frontNeighborPos = pos + new Vector2Int(Mathf.RoundToInt(forward.x), Mathf.RoundToInt(forward.z));
                Vector2Int backNeighborPos = pos + new Vector2Int(Mathf.RoundToInt(back.x), Mathf.RoundToInt(back.z));
                bool hasFrontNeighbor = spawnedTiles.ContainsKey(frontNeighborPos);
                bool hasBackNeighbor = spawnedTiles.ContainsKey(backNeighborPos);
                tile.SetupCorridorConnections(hasFrontNeighbor, hasBackNeighbor);
            }
        }
    }

    public void ClearGallery()
    {
        foreach (var entry in spawnedTiles)
        {
            if(entry.Value != null) Destroy(entry.Value.gameObject);
        }
        spawnedTiles.Clear();
        photoFrames.Clear();
    }

    public void BuildGalleryFromData(List<TileData> layout)
    {
        ClearGallery();
        
        if (corridorPrefabs == null || corridorPrefabs.Count == 0)
        {
            Debug.LogError("Daftar 'Corridor Prefabs' di MapManager kosong! Tolong isi di Inspector.");
            return;
        }

        foreach (var tileData in layout)
        {
            GameObject prefabToSpawn;
            if (tileData.isHub)
            {
                prefabToSpawn = hubPrefab;
            }
            else
            {
                // --- PERBAIKAN DI SINI ---
                // Pilih prefab koridor secara acak DI DALAM loop
                prefabToSpawn = corridorPrefabs[Random.Range(0, corridorPrefabs.Count)];
            }
            SpawnTile(prefabToSpawn, tileData.position, tileData.rotation);
        }
        UpdateAllTileConnections();
    }
    
    public List<TileData> GetLayoutData()
    {
        var layout = new List<TileData>();
        foreach (var entry in spawnedTiles)
        {
            layout.Add(new TileData
            {
                position = entry.Key,
                rotation = entry.Value.transform.rotation,
                isHub = entry.Value.gameObject.name.Contains("Hub")
            });
        }
        return layout;
    }

    public List<Transform> GetPhotoFrames()
    {
        return photoFrames;
    }
}