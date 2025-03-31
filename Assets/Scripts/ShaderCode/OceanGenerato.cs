using UnityEngine;
using System.Collections.Generic;
[ExecuteAlways]
public class OceanGenerato : MonoBehaviour
{
    [Header("Prefab Settings")]
    public GameObject prefab; // The prefab to tile

    [Header("Tiling Settings")]
    public int tilesX = 5; // Number of tiles along the X axis
    public int tilesY = 5; // Number of tiles along the Y axis
    public float tileSize = 1f; // Distance between tiles

    [Header("Update Settings")]
    public bool regenerateTiles = false; // Trigger to regenerate tiles

    private List<GameObject> spawnedTiles = new List<GameObject>();

    private void OnEnable()
    {
        if (spawnedTiles.Count == tilesX * tilesY)
        {
            return;
        }

        GenerateTiles();
    }

    private void Update()
    {
        if (regenerateTiles)
        {
            regenerateTiles = false;
            GenerateTiles();
        }
    }

    private void GenerateTiles()
    {
        // Destroy all existing tiles
        foreach (var tile in spawnedTiles)
        {
            if (tile != null)
            {
                DestroyImmediate(tile);
            }
        }

        spawnedTiles.Clear();

        // Create new tiles
        for (int x = 0; x < tilesX; x++)
        {
            for (int y = 0; y < tilesY; y++)
            {
                // Calculate the position for each tile
                Vector3 position = new Vector3(x * tileSize, 0, y * tileSize);

                // Instantiate the prefab at the calculated position
                GameObject newTile = Instantiate(prefab, position+transform.position, Quaternion.identity, transform);

                // Add the new tile to the list
                spawnedTiles.Add(newTile);
            }
        }
    }
}
