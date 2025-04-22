using UnityEngine;
using UnityEngine.Tilemaps;

public class IslandGenerator : MonoBehaviour 
{
    public Tilemap tilemap;
    public TileBase oceanTile, grassTile;
    public TileBase chinjuTile; // 新增Chinju Tile
    public int width = 100;
    public int height = 100;
    public float islandDensity = 0.1f;
    
    [Header("Random Seed")]
    public int seed = 12345; // 預設種子
    public bool useRandomSeed = true; // 是否每次隨機生成

    public Camera mainCamera; // 新增主攝影機引用
    public CameraBound2D cameraController; // 新增 CameraController 引用

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
        Vector3Int centerIslandTile = Vector3Int.zero;
        float closestDistance = float.MaxValue;

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

                    // 計算與地圖中心的距離
                    float distance = Vector2.Distance(new Vector2(x, y), new Vector2(width / 2, height / 2));
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        centerIslandTile = new Vector3Int(x, y, 0);
                    }
                }
            }
        }

        // 替換中心島嶼圖塊為Chinju Tile
        if (centerIslandTile != Vector3Int.zero)
        {
            tilemap.SetTile(centerIslandTile, chinjuTile);

            // 將主攝影機移動到中心島嶼圖塊的位置
            Vector3 worldPosition = tilemap.GetCellCenterWorld(centerIslandTile); // 使用 GetCellCenterWorld 確保獲取中心點
            if (mainCamera != null)
            {
                mainCamera.transform.position = new Vector3(worldPosition.x, worldPosition.y, mainCamera.transform.position.z);
            }

            // 通知 CameraController 更新邊界
            if (cameraController != null)
            {
                cameraController.RefreshBounds();
            }
        }
    }
}