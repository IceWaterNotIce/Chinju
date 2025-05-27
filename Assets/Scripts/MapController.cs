using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Threading.Tasks;

public class MapController : Singleton<MapController> // 改為繼承 Singleton
{

    /*
    *  1 tile length = 1 海里
    */
    private const string MapCacheFilePath = "map_cache";

    [SerializeField] public Tilemap oceanTilemap;
    [SerializeField] public Tilemap groundTilemap;
    public TileBase oceanTile, grassTile;
    public TileBase chinjuTile;
    public TileBase oilTile;
    public float islandDensity = 0.1f;

    [Header("Random Seed")]  
    public int seed = 12345;

    public Camera mainCamera;
    public CameraBound2D cameraController;
    public GameObject oilShipPrefab;

    private Dictionary<Vector3Int, TileType> generatedTiles = new Dictionary<Vector3Int, TileType>();
    private HashSet<Vector3Int> chinjuTilePositions = new HashSet<Vector3Int>();
    private int chunkSize = 32;
    private int renderRadius = 3;

    private HashSet<Vector3Int> renderedTiles = new HashSet<Vector3Int>();

    private Vector3 lastCameraPosition;

    private Coroutine chunkRenderCoroutine;
    private HashSet<Vector3Int> pendingTiles = new HashSet<Vector3Int>();
    private const int ChunksPerFrame = 1; // 每幀處理幾個 chunk

    private Queue<GameObject> oilShipPool = new Queue<GameObject>();

    // 新增：儲存每個海洋瓦片的層級
    public Dictionary<Vector3Int, int> oceanTileLevels = new Dictionary<Vector3Int, int>();

    // 新增：管理每個海洋層級文字顯示
    private Dictionary<Vector3Int, GameObject> oceanLevelTexts = new Dictionary<Vector3Int, GameObject>();

    [Header("Debug")]
    public bool showOceanLevelText = true;

    void Start()
    {
        RecalculateMap(); // 取代原本的初始化流程

        if (oilShipPrefab == null)
        {
            oilShipPrefab = Resources.Load<GameObject>("Prefabs/Ship");
            if (oilShipPrefab == null)
            {
                Debug.LogError("[MapController] 無法加載石油船預製物，請確保 'Prefabs/Ship' 存在！");
            }
        }

        GameDataController.Instance.OnMapDataChanged += OnMapDataChanged;

        if (cameraController != null)
        {
            cameraController.targetTilemap = groundTilemap;
            cameraController.RefreshBounds();
        }

        if (mainCamera != null)
            lastCameraPosition = mainCamera.transform.position;

        StartCoroutine(FocusOnChinjuTileAfterMapGeneration());

        // 初始化海洋層級標記
        InitializeTileLevels();
    }

    private IEnumerator FocusOnChinjuTileAfterMapGeneration()
    {
        yield return new WaitUntil(() => groundTilemap != null && groundTilemap.GetUsedTilesCount() > 0);

        Vector3 chinjuTileWorldPosition = GetChinjuTileWorldPosition();
        if (chinjuTileWorldPosition != Vector3.zero && cameraController != null)
        {
            cameraController.FollowTarget(null);
            Debug.Log($"[MapController] 聚焦到神獸 Tile 位置: {chinjuTileWorldPosition}");
            cameraController.transform.position = new Vector3(chinjuTileWorldPosition.x, chinjuTileWorldPosition.y, cameraController.transform.position.z);
            cameraController.RefreshCameraPosition();
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
            GameDataController.Instance.OnMapDataChanged -= OnMapDataChanged;
        }
    }

    private void OnMapDataChanged()
    {
        var mapData = GameDataController.Instance.CurrentGameData?.mapData;
        if (mapData != null)
        {
            UpdateVisibleChunks();
        }
    }

    private void Update()
    {
        if (mainCamera != null && mainCamera.transform.position != lastCameraPosition)
        {
            UpdateVisibleChunks();
            lastCameraPosition = mainCamera.transform.position;
        }

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            HandleMouseClick();
        }
    }

    private void UpdateVisibleChunks()
    {
        if (mainCamera == null || oceanTilemap == null || groundTilemap == null) return;

        Vector3 camWorldPos = mainCamera.transform.position;
        Vector3Int camCell = groundTilemap.WorldToCell(camWorldPos);

        int chunkX = Mathf.FloorToInt((float)camCell.x / chunkSize);
        int chunkY = Mathf.FloorToInt((float)camCell.y / chunkSize);

        List<Vector2Int> chunkOffsets = GetClockwiseChunkOffsets(renderRadius);

        HashSet<Vector3Int> shouldRender = new HashSet<Vector3Int>();
        foreach (var offset in chunkOffsets)
        {
            int cx = chunkX + offset.x;
            int cy = chunkY + offset.y;
            for (int x = 0; x < chunkSize; x++)
            {
                for (int y = 0; y < chunkSize; y++)
                {
                    Vector3Int pos = new Vector3Int(cx * chunkSize + x, cy * chunkSize + y, 0);
                    shouldRender.Add(pos);
                    if (!renderedTiles.Contains(pos) && !pendingTiles.Contains(pos))
                    {
                        pendingTiles.Add(pos);
                    }
                }
            }
        }

        if (chunkRenderCoroutine != null)
        {
            StopCoroutine(chunkRenderCoroutine);
        }
        chunkRenderCoroutine = StartCoroutine(RenderTilesCoroutine(chunkOffsets, chunkX, chunkY));
    }

    private List<Vector2Int> GetClockwiseChunkOffsets(int radius)
    {
        List<Vector2Int> offsets = new List<Vector2Int>();
        offsets.Add(Vector2Int.zero);

        for (int r = 1; r <= radius; r++)
        {
            int x = -r, y = -r;
            for (int i = 0; i < 2 * r; i++) offsets.Add(new Vector2Int(x + i, y));
            for (int i = 1; i < 2 * r; i++) offsets.Add(new Vector2Int(x + 2 * r - 1, y + i));
            for (int i = 1; i < 2 * r; i++) offsets.Add(new Vector2Int(x + 2 * r - 1 - i, y + 2 * r - 1));
            for (int i = 1; i < 2 * r - 1; i++) offsets.Add(new Vector2Int(x, y + 2 * r - 1 - i));
        }
        return offsets;
    }

    private IEnumerator RenderTilesCoroutine(List<Vector2Int> chunkOffsets, int centerChunkX, int centerChunkY)
    {
        List<Vector3Int> orderedTiles = new List<Vector3Int>();
        HashSet<Vector3Int> added = new HashSet<Vector3Int>();
        int chunkCount = 0; // 新增

        foreach (var offset in chunkOffsets)
        {
            int cx = centerChunkX + offset.x;
            int cy = centerChunkY + offset.y;
            for (int x = 0; x < chunkSize; x++)
            {
                for (int y = 0; y < chunkSize; y++)
                {
                    Vector3Int pos = new Vector3Int(cx * chunkSize + x, cy * chunkSize + y, 0);
                    if (pendingTiles.Contains(pos) && !added.Contains(pos))
                    {
                        orderedTiles.Add(pos);
                        added.Add(pos);
                    }
                }
            }

            chunkCount++;
            if (chunkCount >= ChunksPerFrame)
            {
                chunkCount = 0;
                yield return null;
            }
        }
        pendingTiles.Clear();

        List<Vector3Int> oceanTilesToShow = new List<Vector3Int>();

        foreach (var pos in orderedTiles)
        {
            if (!generatedTiles.ContainsKey(pos))
            {
                TileType type = GetTileTypeAt(pos.x, pos.y);
                generatedTiles[pos] = type;
            }
            TileType tileType = generatedTiles[pos];
            oceanTilemap.SetTile(pos, null);
            groundTilemap.SetTile(pos, null);

            switch (tileType)
            {
                case TileType.Ocean:
                    oceanTilemap.SetTile(pos, oceanTile);
                    oceanTilesToShow.Add(pos);
                    break;
                case TileType.Grass:
                    oceanTilemap.SetTile(pos, oceanTile);
                    groundTilemap.SetTile(pos, grassTile);
                    HideOceanLevelText(pos);
                    break;
                case TileType.Oil:
                    oceanTilemap.SetTile(pos, oceanTile);
                    groundTilemap.SetTile(pos, oilTile);
                    HideOceanLevelText(pos);
                    break;
                case TileType.Chinju:
                    oceanTilemap.SetTile(pos, oceanTile);
                    groundTilemap.SetTile(pos, chinjuTile);
                    HideOceanLevelText(pos);
                    break;
            }
            renderedTiles.Add(pos);
        }

        // 先計算層級
        CalculateOceanLevels();

        // 顯示本次 chunk 的 ocean level 文字
        foreach (var pos in oceanTilesToShow)
        {
            ShowOceanLevelText(pos);
        }
        foreach (var pos in renderedTiles)
        {
            if (generatedTiles.TryGetValue(pos, out var type) && type == TileType.Ocean)
            {
                ShowOceanLevelText(pos);
            }
        }
    }

    // 顯示海洋層級文字
    private void ShowOceanLevelText(Vector3Int pos)
    {
        if (!showOceanLevelText)
        {
            HideOceanLevelText(pos);
            return;
        }
        int level = 0;
        if (!oceanTileLevels.TryGetValue(pos, out level)) level = 0;

        GameObject textObj;
        if (!oceanLevelTexts.TryGetValue(pos, out textObj) || textObj == null)
        {
            textObj = new GameObject($"OceanLevelText_{pos.x}_{pos.y}");
            textObj.transform.SetParent(this.transform);
            textObj.transform.position = oceanTilemap.GetCellCenterWorld(pos) + new Vector3(0, 0, -0.5f);

            var textMesh = textObj.AddComponent<TextMesh>();
            textMesh.fontSize = 32;
            textMesh.characterSize = 0.2f;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            textMesh.color = Color.blue;
            oceanLevelTexts[pos] = textObj;
        }
        else
        {
            textObj.SetActive(true);
            textObj.transform.position = oceanTilemap.GetCellCenterWorld(pos) + new Vector3(0, 0, -0.5f);
        }

        var mesh = textObj.GetComponent<TextMesh>();
        mesh.text = level.ToString();
    }

    // 隱藏或移除非海洋的層級文字
    private void HideOceanLevelText(Vector3Int pos)
    {
        if (oceanLevelTexts.TryGetValue(pos, out var textObj) && textObj != null)
        {
            textObj.SetActive(false);
        }
    }

    private TileType GetTileTypeAt(int x, int y)
    {
        if (x == 0 && y == 0)
        {
            chinjuTilePositions.Add(new Vector3Int(x, y, 0));
            return TileType.Chinju;
        }

        int gx = x / 2;
        int gy = y / 2;
        float noiseValue = Mathf.PerlinNoise((gx * 0.1f + seed * 0.1f), (gy * 0.1f + seed * 0.1f));
        if (noiseValue > 1f - islandDensity)
        {
            float oilNoise = Mathf.PerlinNoise((gx + seed) * 0.2f, (gy + seed) * 0.2f);
            if (oilNoise > 0.7f)
                return TileType.Oil;
            return TileType.Grass;
        }
        return TileType.Ocean;
    }

    private void HandleMouseClick()
    {
        if (mainCamera == null)
        {
            Debug.LogError("[MapController] 主攝影機未設置！");
            return;
        }

        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Vector3 worldPoint = mainCamera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, -mainCamera.transform.position.z));

        Vector3Int tilePosition = groundTilemap.WorldToCell(worldPoint);
        TileBase tile = groundTilemap.GetTile(tilePosition);

        if (tile != null)
        {
            if (tile == grassTile)
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
        else
        {
            // 檢查是否為海洋
            TileBase ocean = oceanTilemap.GetTile(tilePosition);
            if (ocean == oceanTile)
            {
                Debug.Log("[MapController] 這是海洋 Tile");
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
            yield return new WaitForSeconds(120f);

            var gameData = GameDataController.Instance?.CurrentGameData;
            if (gameData?.playerData != null)
            {
                Vector3 oilTileWorldPosition = groundTilemap.GetCellCenterWorld(oilTilePosition);
                Vector3 spawnPosition = FindNearestOceanTile(oilTileWorldPosition);

                if (spawnPosition != Vector3.zero && oilShipPrefab != null)
                {
                    GameObject oilShip = GetOilShip(spawnPosition);
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

        float travelTime = 30f;
        float elapsedTime = 0f;

        while (elapsedTime < travelTime)
        {
            if (oilShip == null) yield break;
            oilShip.transform.position = Vector3.Lerp(oilTileWorldPosition, chinjuTileWorldPosition, elapsedTime / travelTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (oilShip != null)
        {
            ReturnOilShip(oilShip);
            var gameData = GameDataController.Instance?.CurrentGameData;
            if (gameData?.playerData != null)
            {
                gameData.playerData.Oils += 20;
                gameData.playerData.OnResourceChanged?.Invoke();
                Debug.Log("[MapController] 石油船到達神獸 Tile，+20 石油！");
            }
        }
    }

    private GameObject GetOilShip(Vector3 position)
    {
        if (oilShipPool.Count > 0)
        {
            var ship = oilShipPool.Dequeue();
            ship.transform.position = position;
            ship.SetActive(true);
            return ship;
        }
        return Instantiate(oilShipPrefab, position, Quaternion.identity);
    }

    private void ReturnOilShip(GameObject ship)
    {
        ship.SetActive(false);
        oilShipPool.Enqueue(ship);
    }

    public Vector3 FindNearestOceanTile(Vector3 referencePoint)
    {
        Vector3Int[] directions = new Vector3Int[]
        {
            new Vector3Int(0, 1, 0),
            new Vector3Int(0, -1, 0),
            new Vector3Int(-1, 0, 0),
            new Vector3Int(1, 0, 0)
        };

        Vector3Int referenceTile = groundTilemap.WorldToCell(referencePoint);

        foreach (var direction in directions)
        {
            Vector3Int neighborTile = referenceTile + direction;
            if (oceanTilemap.GetTile(neighborTile) == oceanTile)
            {
                return oceanTilemap.GetCellCenterWorld(neighborTile);
            }
        }

        return Vector3.zero;
    }

    public Vector3 GetChinjuTileWorldPosition()
    {
        return groundTilemap.GetCellCenterWorld(Vector3Int.zero);
    }

    // 初始化所有海洋瓦片為 0，陸地瓦片為 -1
    private void InitializeTileLevels()
    {
        oceanTileLevels.Clear();
        // 標記海洋
        foreach (var pos in oceanTilemap.cellBounds.allPositionsWithin)
        {
            if (oceanTilemap.HasTile(pos))
            {
                oceanTileLevels[pos] = 0;
            }
        }
        // 標記陸地
        foreach (var pos in groundTilemap.cellBounds.allPositionsWithin)
        {
            if (groundTilemap.HasTile(pos))
            {
                oceanTileLevels[pos] = -1;
            }
        }
    }

    // 計算每個海洋瓦片的層級
    private void CalculateOceanLevels()
    {
        Queue<Vector3Int> queue = new Queue<Vector3Int>();
        // 將所有陸地瓦片相鄰的海洋瓦片加入隊列（層級 1）
        foreach (var pos in groundTilemap.cellBounds.allPositionsWithin)
        {
            if (groundTilemap.HasTile(pos))
            {
                MarkNeighborOceanTiles(pos, 1, queue);
            }
        }
        // BFS 擴散
        while (queue.Count > 0)
        {
            Vector3Int currentPos = queue.Dequeue();
            int currentLevel = oceanTileLevels[currentPos];
            MarkNeighborOceanTiles(currentPos, currentLevel + 1, queue);
        }
    }

    // 標記相鄰的海洋瓦片
    private void MarkNeighborOceanTiles(Vector3Int centerPos, int level, Queue<Vector3Int> queue)
    {
        Vector3Int[] directions = {
            Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right
        };
        foreach (var dir in directions)
        {
            Vector3Int neighborPos = centerPos + dir;
            // 如果是海洋瓦片且未被標記或標記值更大
            if (oceanTilemap.HasTile(neighborPos) &&
                (!oceanTileLevels.ContainsKey(neighborPos) || oceanTileLevels[neighborPos] > level))
            {
                oceanTileLevels[neighborPos] = level;
                queue.Enqueue(neighborPos);
            }
        }
    }

    // 新增：重繪地圖方法，供 GameManager 呼叫
    public void RenderMap()
    {
        // 清除已產生的地圖資料與已渲染區域
        generatedTiles.Clear();
        renderedTiles.Clear();
        pendingTiles.Clear();
        oceanTileLevels.Clear();
        // 清空 tilemap
        if (oceanTilemap != null) oceanTilemap.ClearAllTiles();
        if (groundTilemap != null) groundTilemap.ClearAllTiles();
        // 重新產生地圖
        UpdateVisibleChunks();
    }

    /// <summary>
    /// 重新計算地圖（可用於外部強制刷新地圖資料）
    /// </summary>
    public void RecalculateMap()
    {
        // 重新設定 seed（可依需求調整，這裡預設用 GameData 的 seed）
        var mapData = GameDataController.Instance?.CurrentGameData?.mapData;
        if (mapData != null)
            seed = mapData.Seed;
        else
            seed = Random.Range(0, int.MaxValue);

        Random.InitState(seed);

        // 清除所有已產生資料
        generatedTiles.Clear();
        renderedTiles.Clear();
        pendingTiles.Clear();
        oceanTileLevels.Clear();

        // 清空 tilemap
        if (oceanTilemap != null) oceanTilemap.ClearAllTiles();
        if (groundTilemap != null) groundTilemap.ClearAllTiles();

        // 重新產生地圖
        UpdateVisibleChunks();
    }
}