using UnityEngine;
using UnityEngine.Tilemaps;

public class IslandGenerator : MonoBehaviour 
{
    public Tilemap tilemap;
    public TileBase oceanTile, grassTile;
    public int width = 100;
    public int height = 100;
    public float islandDensity = 0.1f;
    
    [Header("Random Seed")]
    public int seed = 12345; // 預設種子
    public bool useRandomSeed = true; // 是否每次隨機生成

    void Start() 
    {
        
        Random.InitState(seed); // 初始化隨機數生成器
        
        Debug.Log("Current Map Seed: " + seed); // 輸出種子供調試

        GenerateMap();
    }

    void GenerateMap()
    {
        // 先填充海洋
        tilemap.ClearAllTiles();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                tilemap.SetTile(new Vector3Int(x, y, 0), oceanTile);
            }
        }

        // 隨機生成島嶼（使用確定性隨機）
        for (int x = 0; x < width; x++) 
        {
            for (int y = 0; y < height; y++) 
            {
                // 使用 Perlin Noise 替代純隨機，使島嶼更自然
                float noiseValue = Mathf.PerlinNoise(
                    (x + seed) * 0.1f, 
                    (y + seed) * 0.1f
                );
                
                if (noiseValue > 1f - islandDensity) 
                {
                    tilemap.SetTile(new Vector3Int(x, y, 0), grassTile);
                }
            }
        }
    }
}