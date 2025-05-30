using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

public class MapController : Singleton<MapController>
{
    private const string MapCacheFilePath = "map_cache";
    private const int ChunksPerFrame = 1;
    private const int MaxOceanSearchRadius = 15;

    [SerializeField] public Tilemap oceanTilemap;
    [SerializeField] public Tilemap groundTilemap;
    [SerializeField] public Tilemap chinjufuTilemap;
    public TileBase oceanTile, grassTile;
    public TileBase chinjuTile;
    public TileBase oilTile;
    [Range(0.01f, 0.5f)] public float islandDensity = 0.1f;


    [Header("Random Seed")]
    public int seed = 12345;

    public Camera mainCamera;
    public CameraBound2D cameraController;
    public GameObject oilShipPrefab;

    private Dictionary<Vector3Int, TileType> generatedTiles = new Dictionary<Vector3Int, TileType>();
    private HashSet<Vector3Int> chinjuTilePositions = new HashSet<Vector3Int>();
    private int chunkSize = 16;
    private int renderRadius = 4;

    private HashSet<Vector3Int> renderedTiles = new HashSet<Vector3Int>();
    private Vector3 lastCameraPosition;
    private Coroutine chunkRenderCoroutine;
    private HashSet<Vector3Int> pendingTiles = new HashSet<Vector3Int>();

    // Ocean tile management
    public Dictionary<Vector3Int, int> oceanTileLevels = new Dictionary<Vector3Int, int>();
    private Dictionary<Vector3Int, GameObject> oceanLevelTexts = new Dictionary<Vector3Int, GameObject>();

    [Header("Debug")]
    public bool showOceanLevelText = true;
    public bool debugDrawChunkBounds = false;

    protected Dictionary<Vector2Int, float> _noiseCache = new Dictionary<Vector2Int, float>();
    [SerializeField] private float updateThreshold = 1.0f;

    private List<Vector2Int> spiralChunkOffsets;
    private ObjectPool textObjectPool;

    protected override void Awake()
    {
        base.Awake();
        textObjectPool = new ObjectPool(transform, "OceanLevelText");
        spiralChunkOffsets = GenerateSpiralOffsets(renderRadius);
    }

    void Start()
    {
        RecalculateMap();

        if (oilShipPrefab == null)
        {
            oilShipPrefab = Resources.Load<GameObject>("Prefabs/Ship");
            if (oilShipPrefab == null)
            {
                Debug.LogError("[MapController] 無法加載石油船預製物");
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
    }

    private IEnumerator FocusOnChinjuTileAfterMapGeneration()
    {
        yield return new WaitUntil(() => groundTilemap != null && groundTilemap.GetUsedTilesCount() > 0);

        Vector3 chinjuTileWorldPosition = GetChinjuTileWorldPosition();
        if (chinjuTileWorldPosition != Vector3.zero && cameraController != null)
        {
            cameraController.FollowTarget(null);
            cameraController.transform.position = new Vector3(
                chinjuTileWorldPosition.x,
                chinjuTileWorldPosition.y,
                cameraController.transform.position.z
            );
            cameraController.RefreshCameraPosition();
        }
    }

    private void OnDestroy()
    {
        if (GameDataController.Instance != null)
        {
            GameDataController.Instance.OnMapDataChanged -= OnMapDataChanged;
        }

        // Clean up all text objects
        foreach (var textObj in oceanLevelTexts.Values)
        {
            if (textObj != null) Destroy(textObj);
        }
        oceanLevelTexts.Clear();
    }

    private void OnMapDataChanged()
    {
        UpdateVisibleChunks();
    }

    private void Update()
    {
        if (mainCamera != null &&
            Vector3.Distance(mainCamera.transform.position, lastCameraPosition) > updateThreshold)
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

        int chunkIndexX = Mathf.FloorToInt((float)camCell.x / chunkSize);
        int chunkIndexY = Mathf.FloorToInt((float)camCell.y / chunkSize);

        if (chunkRenderCoroutine != null)
        {
            StopCoroutine(chunkRenderCoroutine);
        }
        chunkRenderCoroutine = StartCoroutine(RenderTilesCoroutine(chunkIndexX, chunkIndexY));
    }

    private List<Vector2Int> GenerateSpiralOffsets(int radius)
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

    private IEnumerator RenderTilesCoroutine(int centerChunkX, int centerChunkY)
    {
        List<Vector3Int> tilesToRender = new List<Vector3Int>();
        HashSet<Vector3Int> currentRendering = new HashSet<Vector3Int>();

        foreach (var offset in spiralChunkOffsets)
        {
            int startX = (centerChunkX + offset.x) * chunkSize;
            int startY = (centerChunkY + offset.y) * chunkSize;

            for (int x = 0; x < chunkSize; x++)
            {
                for (int y = 0; y < chunkSize; y++)
                {
                    Vector3Int pos = new Vector3Int(startX + x, startY + y, 0);
                    if (!renderedTiles.Contains(pos) && !currentRendering.Contains(pos))
                    {
                        tilesToRender.Add(pos);
                        currentRendering.Add(pos);
                    }
                }
            }
        }

        pendingTiles.Clear();

        // Process tiles in batches
        int processed = 0;
        foreach (var pos in tilesToRender)
        {
            if (!generatedTiles.ContainsKey(pos))
            {
                generatedTiles[pos] = GetTileTypeAt(pos.x, pos.y);
            }

            RenderTile(pos);
            renderedTiles.Add(pos);

            processed++;
            if (processed >= ChunksPerFrame * chunkSize * chunkSize)
            {
                processed = 0;
                yield return null;
            }
        }

        CalculateOceanLevels();
        UpdateOceanLevelTexts();
    }

    private void RenderTile(Vector3Int pos)
    {
        TileType tileType = generatedTiles[pos];
        oceanTilemap.SetTile(pos, null);
        groundTilemap.SetTile(pos, null);

        switch (tileType)
        {
            case TileType.Ocean:
                oceanTilemap.SetTile(pos, oceanTile);
                break;
            case TileType.Grass:
                groundTilemap.SetTile(pos, grassTile);
                break;
            case TileType.Oil:
                groundTilemap.SetTile(pos, oilTile);
                break;
            case TileType.Chinju:
                groundTilemap.SetTile(pos, chinjuTile);
                chinjuTilePositions.Add(pos);
                break;
        }
    }

    private void UpdateOceanLevelTexts()
    {
        foreach (var pos in renderedTiles)
        {
            if (generatedTiles.TryGetValue(pos, out var type))
            {
                if (type == TileType.Ocean && showOceanLevelText)
                {
                    ShowOceanLevelText(pos);
                }
                else
                {
                    HideOceanLevelText(pos);
                }
            }
        }
    }

    protected float GetCachedNoise(int x, int y)
    {
        var key = new Vector2Int(x, y);
        if (!_noiseCache.TryGetValue(key, out float value))
        {
            // 使用哈希函数创建唯一种子
            uint hash = (uint)key.GetHashCode();
            uint seedHash = (uint)seed.GetHashCode();
            uint combined = hash ^ seedHash;

            // 创建独特坐标
            float xCoord = (combined & 0xFFFF) / 65536f * 100f + x * 0.1f;
            float yCoord = ((combined >> 16) & 0xFFFF) / 65536f * 100f + y * 0.1f;

            value = Mathf.PerlinNoise(xCoord, yCoord);
            _noiseCache[key] = value;
        }
        return value;
    }

    // Modify GetTileTypeAt to use lower frequency noise
    protected TileType GetTileTypeAt(int x, int y)
    {
        if (x == 0 && y == 0) return TileType.Chinju;

        // Force neighbors around Chinju to be land
        if ((x == 0 && (y == 1 || y == -1)) || (y == 0 && (x == 1 || x == -1)))
        {
            return TileType.Grass;
        }

        // Use lower frequency noise for larger landmasses
        float noiseValue = 0.6f * GetCachedNoise(x / 2, y / 2);
        noiseValue += 0.3f * GetCachedNoise(x, y);
        noiseValue += 0.1f * GetCachedNoise(x * 2, y * 2);
        noiseValue = Mathf.Clamp01(noiseValue);

        if (noiseValue > 1f - islandDensity)
        {
            return TileType.Grass;
        }
        return TileType.Ocean;
    }
    private void HandleMouseClick()
    {
        if (mainCamera == null) return;

        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Ray ray = mainCamera.ScreenPointToRay(mousePosition);
        RaycastHit2D hit = Physics2D.GetRayIntersection(ray, Mathf.Infinity);

        if (hit.collider != null)
        {
            Vector3 worldPoint = hit.point;
            Vector3Int tilePosition = groundTilemap.WorldToCell(worldPoint);
            TileBase tile = groundTilemap.GetTile(tilePosition);

            if (tile == chinjuTile && PopupManager.Instance != null)
            {
                if (PopupManager.Instance.IsPopupVisible("ChinjuUI"))
                {
                    PopupManager.Instance.HidePopup("ChinjuUI");
                }
                else
                {
                    PopupManager.Instance.HideAllPopups();
                    PopupManager.Instance.ShowPopup("ChinjuUI");
                }
            }
        }
    }

    private void InitializeTileLevels()
    {
        oceanTileLevels.Clear();
        foreach (var pos in oceanTilemap.cellBounds.allPositionsWithin)
        {
            if (oceanTilemap.HasTile(pos))
            {
                oceanTileLevels[pos] = 0;
            }
        }
    }

    private void CalculateOceanLevels()
    {
        Queue<Vector3Int> queue = new Queue<Vector3Int>();
        Vector3Int[] directions = {
            Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right
        };

        // Initialize with land tiles
        foreach (var pos in renderedTiles)
        {
            if (generatedTiles[pos] != TileType.Ocean)
            {
                oceanTileLevels[pos] = -1;
                foreach (var dir in directions)
                {
                    Vector3Int neighbor = pos + dir;
                    if (generatedTiles.TryGetValue(neighbor, out var type) &&
                        type == TileType.Ocean)
                    {
                        queue.Enqueue(neighbor);
                        oceanTileLevels[neighbor] = 1;
                    }
                }
            }
        }

        // BFS propagation
        while (queue.Count > 0)
        {
            Vector3Int current = queue.Dequeue();
            int currentLevel = oceanTileLevels[current];

            foreach (var dir in directions)
            {
                Vector3Int neighbor = current + dir;
                if (generatedTiles.TryGetValue(neighbor, out var type) &&
                    type == TileType.Ocean)
                {
                    if (!oceanTileLevels.ContainsKey(neighbor)
                        || oceanTileLevels[neighbor] > currentLevel + 1)
                    {
                        oceanTileLevels[neighbor] = currentLevel + 1;
                        queue.Enqueue(neighbor);
                    }
                }
            }
        }
    }

    private void ShowOceanLevelText(Vector3Int pos)
    {
        if (!oceanTileLevels.TryGetValue(pos, out int level)) return;

        GameObject textObj = textObjectPool.GetObject();
        textObj.name = $"OceanLevelText_{pos.x}_{pos.y}";
        textObj.transform.position = oceanTilemap.GetCellCenterWorld(pos) + new Vector3(0, 0, -0.5f);

        TextMesh textMesh = textObj.GetComponent<TextMesh>();
        if (textMesh == null) textMesh = textObj.AddComponent<TextMesh>();

        textMesh.text = level.ToString();
        textMesh.fontSize = 32;
        textMesh.characterSize = 0.2f;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.color = new Color(0.2f, 0.4f, 0.8f, 0.8f);

        oceanLevelTexts[pos] = textObj;
    }

    private void HideOceanLevelText(Vector3Int pos)
    {
        if (oceanLevelTexts.TryGetValue(pos, out GameObject textObj))
        {
            textObjectPool.ReturnObject(textObj);
            oceanLevelTexts.Remove(pos);
        }
    }

    public void RenderMap()
    {
        generatedTiles.Clear();
        renderedTiles.Clear();
        pendingTiles.Clear();
        oceanTileLevels.Clear();
        _noiseCache.Clear();

        oceanTilemap.ClearAllTiles();
        groundTilemap.ClearAllTiles();
        chinjufuTilemap.ClearAllTiles();

        UpdateVisibleChunks();
    }

    public void RecalculateMap()
    {
        var mapData = GameDataController.Instance?.CurrentGameData?.mapData;
        seed = mapData?.Seed ?? Random.Range(0, int.MaxValue);
        Random.InitState(seed);

        RenderMap();
    }

    public Vector3 GetChinjuTileWorldPosition()
    {
        return groundTilemap.GetCellCenterWorld(Vector3Int.zero);
    }

    public Vector3 FindNearestOceanTile(Vector3 referencePoint)
    {
        Vector3Int referenceTile = groundTilemap.WorldToCell(referencePoint);

        for (int radius = 1; radius <= MaxOceanSearchRadius; radius++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                for (int y = -radius; y <= radius; y++)
                {
                    if (Mathf.Abs(x) != radius && Mathf.Abs(y) != radius) continue;

                    Vector3Int checkPos = referenceTile + new Vector3Int(x, y, 0);
                    if (oceanTilemap.HasTile(checkPos))
                    {
                        return oceanTilemap.GetCellCenterWorld(checkPos);
                    }
                }
            }
        }

        Debug.LogWarning("找不到海洋瓦片！");
        return referencePoint + new Vector3(3, 0, 0);
    }

    private void OnDrawGizmosSelected()
    {
        if (!debugDrawChunkBounds || mainCamera == null || groundTilemap == null) return;

        // [保持原有的Gizmos繪製程式碼不變]
        // ...
    }
}

// Helper class for object pooling
public class ObjectPool
{
    private Transform parent;
    private string objectName;
    private Queue<GameObject> pool = new Queue<GameObject>();
    private List<GameObject> activeObjects = new List<GameObject>();

    public ObjectPool(Transform parent, string name)
    {
        this.parent = parent;
        this.objectName = name;
    }

    public GameObject GetObject()
    {
        GameObject obj;
        if (pool.Count > 0)
        {
            obj = pool.Dequeue();
            obj.SetActive(true);
        }
        else
        {
            obj = new GameObject(objectName);
            obj.transform.SetParent(parent);
        }
        activeObjects.Add(obj);
        return obj;
    }

    public void ReturnObject(GameObject obj)
    {
        obj.SetActive(false);
        activeObjects.Remove(obj);
        pool.Enqueue(obj);
    }
}

