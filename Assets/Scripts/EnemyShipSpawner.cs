using UnityEngine;
using UnityEngine.Tilemaps;

public class EnemyShipSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject enemyShipPrefab;
    public float spawnInterval = 2f;
    private Vector2 spawnAreaMin = new Vector2(0, 0);
    private Vector2 spawnAreaMax = new Vector2(100, 100);

    private float timer;

    private Tilemap tilemap;
    private TileBase oceanTile;

    void Start()
    {
        tilemap = FindFirstObjectByType<Tilemap>();
        if (tilemap == null)
        {
            Debug.LogError("Tilemap not found in the scene!", this);
        }
        oceanTile = Resources.Load<TileBase>("Tilemap/OceanTile");
        if (oceanTile == null)
        {
            Debug.LogError("Ocean Tile not found in Resources!", this);
        }
        // 不再自動計算 spawnAreaMin/max，直接使用 100x100
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
            Vector3 spawnPos = new Vector3(x, y, -1f);

            if (IsOceanTile(spawnPos))
            {
                Instantiate(enemyShipPrefab, spawnPos, Quaternion.identity);
                break;
            }
        }
    }

    bool IsOceanTile(Vector3 position)
    {
        if (tilemap == null || oceanTile == null)
            return false;
        Vector3Int cellPos = tilemap.WorldToCell(position);
        TileBase tile = tilemap.GetTile(cellPos);
        return tile == oceanTile;
    }
}
