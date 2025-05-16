using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Threading.Tasks;

public class MapController : MonoBehaviour
{
    private const string MapCacheFilePath = "map_cache"; // 不需要副檔名

    [SerializeField] private Tilemap tilemap;  // 通過 Inspector 引用
    public TileBase oceanTile, grassTile;
    public TileBase chinjuTile; // 新增Chinju Tile
    public TileBase oilTile; // 新增石油 Tile
    public float islandDensity = 0.1f;

    [Header("Random Seed")]
    public int seed = 12345; // 預設種子
    public bool useRandomSeed = true; // 是否每次隨機生成

    public Camera mainCamera; // 新增主攝影機引用
    public CameraBound2D cameraController; // 新增 CameraController 引用
    public GameObject oilShipPrefab; // 新增：石油船的預製物

    // 新增：無限地圖資料結構
    private Dictionary<Vector3Int, TileType> generatedTiles = new Dictionary<Vector3Int, TileType>();
    private HashSet<Vector3Int> chinjuTilePositions = new HashSet<Vector3Int>();
    private int chunkSize = 32; // 每次生成的區塊大小
    private int renderRadius = 3; // 以攝影機為中心，渲染多少個 chunk

    // 新增：記錄目前已渲染的 tile
    private HashSet<Vector3Int> renderedTiles = new HashSet<Vector3Int>();

    void Start()
    {
        if (useRandomSeed)
        {
            
            seed = Random.Range(0, int.MaxValue);
        }
        Random.InitState(seed); // 初始化隨機數生成器

        // 不再載入/儲存地圖檔案，直接動態生成
        // 初始化中心區塊
        UpdateVisibleChunks();

        // 確保石油船預製物已設置
        if (oilShipPrefab == null)
        {
            oilShipPrefab = Resources.Load<GameObject>("Prefabs/Ship"); // 加載石油船預製物
            if (oilShipPrefab == null)
            {
                Debug.LogError("[MapController] 無法加載石油船預製物，請確保 'Prefabs/Ship' 存在！");
            }
        }

        GameDataController.Instance.OnMapDataChanged += OnMapDataChanged; // 訂閱地圖數據變更事件

        // 確保攝影機控制器的 Tilemap 已初始化並刷新邊界
        if (cameraController != null)
        {
            cameraController.targetTilemap = tilemap;
            cameraController.RefreshBounds();
        }

        StartCoroutine(FocusOnChinjuTileAfterMapGeneration());
    }

    private IEnumerator FocusOnChinjuTileAfterMapGeneration()
    {
        // 等待地圖生成完成
        yield return new WaitUntil(() => tilemap != null && tilemap.GetUsedTilesCount() > 0);

        Vector3 chinjuTileWorldPosition = GetChinjuTileWorldPosition();
        if (chinjuTileWorldPosition != Vector3.zero && cameraController != null)
        {
            cameraController.FollowTarget(null); // 停止任何目標跟隨
            Debug.Log($"[MapController] 聚焦到神獸 Tile 位置: {chinjuTileWorldPosition}");
            cameraController.transform.position = new Vector3(chinjuTileWorldPosition.x, chinjuTileWorldPosition.y, cameraController.transform.position.z);
            cameraController.RefreshCameraPosition(); // 使用公開方法刷新攝影機位置
        }
        else
        {
            Debug.LogWarning("[MapController] 無法聚焦到神獸 Tile，可能是攝影機控制器未設置或神獸 Tile 不存在！");
        }
    }

    private void OnDestroy()
    {
        if (GameDataController.Instance != null)
        {
            GameDataController.Instance.OnMapDataChanged -= OnMapDataChanged; // 取消訂閱
        }
    }

    private void OnMapDataChanged()
    {
        var mapData = GameDataController.Instance.CurrentGameData?.mapData;
        if (mapData != null)
        {
            // LoadMap(mapData); // 已無此方法，直接刷新可見區塊
            UpdateVisibleChunks();
        }
    }

    private void Update()
    {
        // 攝影機移動時動態生成地圖
        UpdateVisibleChunks();

        // 檢測滑鼠左鍵點擊
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            HandleMouseClick();
        }
    }

    // 動態生成並渲染攝影機附近的 chunk，並自動銷毀視野外的 tile
    private void UpdateVisibleChunks()
    {
        if (mainCamera == null || tilemap == null) return;

        Vector3 camWorldPos = mainCamera.transform.position;
        Vector3Int camCell = tilemap.WorldToCell(camWorldPos);

        int chunkX = Mathf.FloorToInt((float)camCell.x / chunkSize);
        int chunkY = Mathf.FloorToInt((float)camCell.y / chunkSize);

        // 計算本次應該顯示的 tile 範圍
        HashSet<Vector3Int> shouldRender = new HashSet<Vector3Int>();
        for (int dx = -renderRadius; dx <= renderRadius; dx++)
        {
            for (int dy = -renderRadius; dy <= renderRadius; dy++)
            {
                int cx = chunkX + dx;
                int cy = chunkY + dy;
                for (int x = 0; x < chunkSize; x++)
                {
                    for (int y = 0; y < chunkSize; y++)
                    {
                        Vector3Int pos = new Vector3Int(cx * chunkSize + x, cy * chunkSize + y, 0);
                        shouldRender.Add(pos);
                        if (!renderedTiles.Contains(pos))
                        {
                            // 只生成未渲染過的 tile
                            if (!generatedTiles.ContainsKey(pos))
                            {
                                TileType type = GetTileTypeAt(pos.x, pos.y);
                                generatedTiles[pos] = type;
                            }
                            TileBase tile = null;
                            switch (generatedTiles[pos])
                            {
                                case TileType.Ocean: tile = oceanTile; break;
                                case TileType.Grass: tile = grassTile; break;
                                case TileType.Oil: tile = oilTile; break;
                                case TileType.Chinju: tile = chinjuTile; break;
                            }
                            if (tile != null)
                            {
                                tilemap.SetTile(pos, tile);
                            }
                            renderedTiles.Add(pos);
                        }
                    }
                }
            }
        }

        // 移除視野外的 tile
        var toRemove = new List<Vector3Int>();
        foreach (var pos in renderedTiles)
        {
            if (!shouldRender.Contains(pos))
            {
                tilemap.SetTile(pos, null);
                toRemove.Add(pos);
            }
        }
        foreach (var pos in toRemove)
        {
            renderedTiles.Remove(pos);
        }
    }

    // 生成一個 chunk
    private void GenerateChunk(int chunkX, int chunkY)
    {
        for (int x = 0; x < chunkSize; x++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                Vector3Int pos = new Vector3Int(chunkX * chunkSize + x, chunkY * chunkSize + y, 0);
                if (generatedTiles.ContainsKey(pos)) continue;

                TileType type = GetTileTypeAt(pos.x, pos.y);
                generatedTiles[pos] = type;

                TileBase tile = null;
                switch (type)
                {
                    case TileType.Ocean: tile = oceanTile; break;
                    case TileType.Grass: tile = grassTile; break;
                    case TileType.Oil: tile = oilTile; break;
                    case TileType.Chinju: tile = chinjuTile; break;
                }
                if (tile != null)
                {
                    tilemap.SetTile(pos, tile);
                }
            }
        }
    }

    // 根據座標與 seed 決定 tile 類型
    private TileType GetTileTypeAt(int x, int y)
    {
        // 神獸 tile 固定在 (0,0)
        if (x == 0 && y == 0)
        {
            chinjuTilePositions.Add(new Vector3Int(x, y, 0));
            return TileType.Chinju;
        }

        float noiseValue = Mathf.PerlinNoise((x + seed) * 0.1f, (y + seed) * 0.1f);
        if (noiseValue > 1f - islandDensity)
        {
            // 草地
            float oilNoise = Mathf.PerlinNoise((x + seed) * 0.2f, (y + seed) * 0.2f);
            if (oilNoise > 0.7f)
                return TileType.Oil;
            return TileType.Grass;
        }
        return TileType.Ocean;
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

                    if (PopupManager.Instance.IsAllPopupsHidden())
                    {
                        Debug.Log("[MapController] 正在開啟 Chinju UI 面板...");
                        PopupManager.Instance.ShowPopup("ChinjuUI");
                    }
                    else
                    {
                        PopupManager.Instance.HidePopup("ChinjuUI");
                    }
                }
                else if (tile == oilTile)
                {
                    Debug.Log("[MapController] 這是石油 Tile");
                    HandleOilTileClick(tilePosition);
                }
            }
        }
    }

    private void HandleOilTileClick(Vector3Int tilePosition)
    {
        var gameData = GameDataController.Instance?.CurrentGameData;
        if (gameData?.playerData == null)
        {
            Debug.LogError("[MapController] 無法處理石油 Tile 點擊，GameData 或 PlayerData 為 null！");
            return;
        }

        if (gameData.playerData.Gold >= 50)
        {
            gameData.playerData.Gold -= 50;
            gameData.playerData.OnResourceChanged?.Invoke();

            Debug.Log("[MapController] 已解鎖石油 Tile，開始運輸石油！");
            StartCoroutine(TransportOilToChinju(tilePosition));
        }
        else
        {
            Debug.Log("[MapController] 金幣不足，無法解鎖石油 Tile！");
        }
    }

    private System.Collections.IEnumerator TransportOilToChinju(Vector3Int oilTilePosition)
    {
        while (true)
        {
            yield return new WaitForSeconds(120f); // 每 2 分鐘生成一次石油船

            var gameData = GameDataController.Instance?.CurrentGameData;
            if (gameData?.playerData != null)
            {
                // 在石油圖塊附近生成石油船
                Vector3 oilTileWorldPosition = tilemap.GetCellCenterWorld(oilTilePosition);
                Vector3 spawnPosition = FindNearestOceanTile(oilTileWorldPosition);

                if (spawnPosition != Vector3.zero && oilShipPrefab != null)
                {
                    GameObject oilShip = Instantiate(oilShipPrefab, spawnPosition, Quaternion.identity);
                    StartCoroutine(MoveOilShipToChinju(oilShip, oilTileWorldPosition));
                    Debug.Log("[MapController] 石油船已生成並開始運輸石油！");
                }
                else
                {
                    Debug.LogWarning("[MapController] 無法生成石油船，可能是附近沒有海洋格子或未設置石油船預製物！");
                }
            }
        }
    }

    private System.Collections.IEnumerator MoveOilShipToChinju(GameObject oilShip, Vector3 oilTileWorldPosition)
    {
        Vector3 chinjuTileWorldPosition = GetChinjuTileWorldPosition();
        if (chinjuTileWorldPosition == Vector3.zero)
        {
            Debug.LogError("[MapController] 無法找到神獸 Tile 的位置！");
            yield break;
        }

        float travelTime = 30f; // 石油船移動到神獸 Tile 的時間
        float elapsedTime = 0f;

        while (elapsedTime < travelTime)
        {
            if (oilShip == null) yield break; // 如果石油船被銷毀則停止
            oilShip.transform.position = Vector3.Lerp(oilTileWorldPosition, chinjuTileWorldPosition, elapsedTime / travelTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 石油船到達神獸 Tile
        if (oilShip != null)
        {
            Destroy(oilShip); // 銷毀石油船
            var gameData = GameDataController.Instance?.CurrentGameData;
            if (gameData?.playerData != null)
            {
                gameData.playerData.Oils += 20; // 每次運輸 20 單位石油
                gameData.playerData.OnResourceChanged?.Invoke();
                Debug.Log("[MapController] 石油船到達神獸 Tile，+20 石油！");
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
        // 神獸 tile 固定在 (0,0)
        return tilemap.GetCellCenterWorld(Vector3Int.zero);
    }
}