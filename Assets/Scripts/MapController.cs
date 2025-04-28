using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;

public class MapController : MonoBehaviour 
{
    [SerializeField] private Tilemap tilemap;  // 通過 Inspector 引用
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

    [Header("UI References")]
    [SerializeField] private ChinjuUIController chinjuUIController;

    private GameManager gameManager;

    void Start() 
    {
        gameManager = Object.FindFirstObjectByType<GameManager>(); // 使用 FindFirstObjectByType 替代 FindObjectOfType
        if (useRandomSeed)
        {
            seed = Random.Range(0, int.MaxValue);
        }
        Random.InitState(seed); // 初始化隨機數生成器
        
        // 自動尋找 ChinjuUIController
        if (chinjuUIController == null)
        {
            chinjuUIController = FindFirstObjectByType<ChinjuUIController>();
            if (chinjuUIController == null)
            {
                Debug.LogError("[MapController] ChinjuUIController 未設置且場景中找不到 ChinjuUIController！");
            }
        }

        // 檢查必要組件
        if (chinjuUIController == null)
        {
            Debug.LogError("[MapController] ChinjuUIController 未設置！請在 Inspector 中設置引用。");
        }
        
        GenerateMap();

        // 新增：自動掛載 VisibilityManager
        var visMgr = FindFirstObjectByType<VisibilityManager>();
        if (visMgr == null)
        {
            var go = new GameObject("VisibilityManager");
            visMgr = go.AddComponent<VisibilityManager>();
        }
        visMgr.tilemap = tilemap;
        visMgr.chinjuTile = chinjuTile;
        // 你需要在 Inspector 指定 maskMaterial
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
                        // 確保該圖塊周圍有海洋圖塊
                        bool hasOceanNeighbor = false;
                        for (int dx = -1; dx <= 1; dx++)
                        {
                            for (int dy = -1; dy <= 1; dy++)
                            {
                                if (dx == 0 && dy == 0) continue;
                                Vector3Int neighborPos = new Vector3Int(x + dx, y + dy, 0);
                                if (tilemap.GetTile(neighborPos) == oceanTile)
                                {
                                    hasOceanNeighbor = true;
                                    break;
                                }
                            }
                            if (hasOceanNeighbor) break;
                        }

                        if (hasOceanNeighbor)
                        {
                            closestDistance = distance;
                            centerIslandTile = new Vector3Int(x, y, 0);
                        }
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
                if (cameraController.targetTilemap == null)
                {
                    cameraController.targetTilemap = tilemap; // 確保 CameraController 的 Tilemap 已設置
                }
                cameraController.RefreshBounds();
            }
        }

        // Save map data to GameManager
        if (gameManager != null)
        {
            GameData.MapData mapData = new GameData.MapData
            {
                Seed = seed,
                Width = width,
                Height = height,
                IslandDensity = islandDensity
            };
            gameManager.SaveMapData(tilemap, mapData);
        }
    }

    public void LoadMap(GameData.MapData mapData)
    {
        if (gameManager != null)
        {
            gameManager.LoadMapData(tilemap, mapData, chinjuTile);
        }
    }

    void Update()
    {
        // 檢測滑鼠左鍵點擊
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            HandleMouseClick();
        }
    }

    /// <summary>
    /// 處理滑鼠點擊事件
    /// </summary>
    private void HandleMouseClick()
    {
        if (mainCamera == null)
        {
            Debug.LogError("[MapController] 主攝影機未設置！");
            return;
        }

        // 獲取滑鼠位置並轉換為世界座標
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Vector3 worldPoint = mainCamera.ScreenToWorldPoint(mousePosition);
        
        // 執行 Raycast
        RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.down);

        if (hit.collider != null)
        {
            Debug.Log("[MapController] 點擊在 " + hit.collider.name);
            Debug.Log("[MapController] 點擊位置: " + hit.point);

            // 將世界座標轉換為 tilemap 的網格座標
            Vector3Int tilePosition = tilemap.WorldToCell(hit.point);

            // 嘗試獲取該位置的 tile
            TileBase tile = tilemap.GetTile(tilePosition);

            if (tile != null)
            {
                if (tile == oceanTile)
                {
                    Debug.Log("[MapController] 這是海洋 Tile");
                }
                else if (tile == grassTile)
                {
                    Debug.Log("[MapController] 這是草地 Tile");
                }
                else if (tile == chinjuTile)
                {
                    Debug.Log("[MapController] 這是神獸 Tile");
                    if (chinjuUIController != null)
                    {
                        Debug.Log("[MapController] 正在開啟 Chinju UI 面板...");
                        chinjuUIController.ToggleMainPanelOnly();
                    }
                    else
                    {
                        Debug.LogError("[MapController] ChinjuUIController 是 null！");
                    }
                }
            }
        }
    }

    /// <summary>
    /// 找到最近的海洋格子
    /// </summary>
    public Vector3 FindNearestOceanTile(Vector3 referencePoint)
    {
        Vector3Int[] directions = new Vector3Int[]
        {
            new Vector3Int(0, 1, 0),  // 上
            new Vector3Int(0, -1, 0), // 下
            new Vector3Int(-1, 0, 0), // 左
            new Vector3Int(1, 0, 0)   // 右
        };

        Vector3Int referenceTile = tilemap.WorldToCell(referencePoint);

        foreach (var direction in directions)
        {
            Vector3Int neighborTile = referenceTile + direction;
            if (IsOceanTile(neighborTile))
            {
                return tilemap.GetCellCenterWorld(neighborTile);
            }
        }

        return Vector3.zero; // 找不到海洋格子
    }

    /// <summary>
    /// 檢查某個位置是否是海洋格子
    /// </summary>
    private bool IsOceanTile(Vector3Int tilePosition)
    {
        TileBase tile = tilemap.GetTile(tilePosition);
        return tile == oceanTile;
    }

    /// <summary>
    /// 獲取 Chinju Tile 的世界位置
    /// </summary>
    public Vector3 GetChinjuTileWorldPosition()
    {
        BoundsInt bounds = tilemap.cellBounds;
        foreach (Vector3Int position in bounds.allPositionsWithin)
        {
            TileBase tile = tilemap.GetTile(position);
            if (tile == chinjuTile)
            {
                return tilemap.GetCellCenterWorld(position);
            }
        }

        return Vector3.zero; // 找不到 Chinju Tile
    }
}