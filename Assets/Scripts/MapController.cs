using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Threading.Tasks;

public class MapController : MonoBehaviour
{
    private const string MapCacheFilePath = "map_cache";

    [SerializeField] private Tilemap tilemap;
    public TileBase oceanTile, grassTile;
    public TileBase chinjuTile;
    public TileBase oilTile;
    public float islandDensity = 0.1f;

    [Header("Random Seed")]
    public int seed = 12345;
    public bool useRandomSeed = true;

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
    private const int TilesPerFrame = 128;

    private Queue<GameObject> oilShipPool = new Queue<GameObject>();

    void Start()
    {
        if (useRandomSeed)
        {
            seed = Random.Range(0, int.MaxValue);
        }
        Random.InitState(seed);

        UpdateVisibleChunks();

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
            cameraController.targetTilemap = tilemap;
            cameraController.RefreshBounds();
        }

        if (mainCamera != null)
            lastCameraPosition = mainCamera.transform.position;

        StartCoroutine(FocusOnChinjuTileAfterMapGeneration());
    }

    private IEnumerator FocusOnChinjuTileAfterMapGeneration()
    {
        yield return new WaitUntil(() => tilemap != null && tilemap.GetUsedTilesCount() > 0);

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
        if (mainCamera == null || tilemap == null) return;

        Vector3 camWorldPos = mainCamera.transform.position;
        Vector3Int camCell = tilemap.WorldToCell(camWorldPos);

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
        }
        pendingTiles.Clear();

        int count = 0;
        foreach (var pos in orderedTiles)
        {
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

            count++;
            if (count >= TilesPerFrame)
            {
                count = 0;
                yield return null;
            }
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

        Vector3Int tilePosition = tilemap.WorldToCell(worldPoint);
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
                Vector3 oilTileWorldPosition = tilemap.GetCellCenterWorld(oilTilePosition);
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

        Vector3Int referenceTile = tilemap.WorldToCell(referencePoint);

        foreach (var direction in directions)
        {
            Vector3Int neighborTile = referenceTile + direction;
            if (tilemap.GetTile(neighborTile) == oceanTile)
            {
                return tilemap.GetCellCenterWorld(neighborTile);
            }
        }

        return Vector3.zero;
    }

    public Vector3 GetChinjuTileWorldPosition()
    {
        return tilemap.GetCellCenterWorld(Vector3Int.zero);
    }
}