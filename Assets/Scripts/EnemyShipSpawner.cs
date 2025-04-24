using UnityEngine;
using UnityEngine.Tilemaps;

public class EnemyShipSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject enemyShipPrefab; // 生成時會自動帶有 EnemyShip 組件的 prefab
    public float spawnInterval = 2f;
    public Vector2 spawnAreaMin = new Vector2(-8f, 5f);
    public Vector2 spawnAreaMax = new Vector2(8f, 8f);

    private float timer;

    // 新增：自動尋找 Tilemap 與 oceanTile
    private Tilemap tilemap;
    private TileBase oceanTile;

    void Start()
    {
        tilemap = FindFirstObjectByType<Tilemap>();
        if (tilemap == null)
        {
            Debug.LogError("Tilemap not found in the scene!", this);
        }
        // 嘗試從 Resources 載入 oceanTile
        oceanTile = Resources.Load<TileBase>("Tilemap/OceanTile");
        if (oceanTile == null)
        {
            Debug.LogError("Ocean Tile not found in Resources!", this);
        }
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            SpawnEnemyShip();
            timer = 0f;
        }
    }

    void SpawnEnemyShip()
    {
        for (int attempt = 0; attempt < 10; attempt++)
        {
            float x = Random.Range(spawnAreaMin.x, spawnAreaMax.x);
            float y = Random.Range(spawnAreaMin.y, spawnAreaMax.y);
            Vector3 spawnPos = new Vector3(x, y, -1f); // z 改為 -1

            if (IsOceanTile(spawnPos))
            {
                Instantiate(enemyShipPrefab, spawnPos, Quaternion.identity);
                break;
            }
        }
    }

    // 判斷指定位置是否為海洋格
    bool IsOceanTile(Vector3 position)
    {
        if (tilemap == null || oceanTile == null)
            return false;
        Vector3Int cellPos = tilemap.WorldToCell(position);
        TileBase tile = tilemap.GetTile(cellPos);
        return tile == oceanTile;
    }
}
