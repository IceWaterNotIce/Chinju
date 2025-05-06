using UnityEngine;
using UnityEngine.Tilemaps;

public class EnemyShipSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject enemyShipPrefab;
    public float spawnInterval = 2f;
    public int maxEnemyShips = 100;
    private Vector2 spawnAreaMin = new Vector2(0, 0);
    private Vector2 spawnAreaMax = new Vector2(100, 100);

    [Header("Weapon Settings")]
    [SerializeField] private GameObject[] weaponPrefabs; // 可用的武器預製件

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
        int currentEnemyCount = GameObject.FindObjectsByType<EnemyShip>(FindObjectsSortMode.None).Length;
        if (currentEnemyCount >= maxEnemyShips)
        {
            Debug.Log($"[EnemyShipSpawner] 當前敵方船隻數量已達上限 ({maxEnemyShips})，停止生成。");
            return;
        }

        for (int attempt = 0; attempt < 10; attempt++)
        {
            float x = Random.Range(spawnAreaMin.x, spawnAreaMax.x);
            float y = Random.Range(spawnAreaMin.y, spawnAreaMax.y);
            Vector3 spawnPos = new Vector3(x, y, -1f);

            if (IsOceanTile(spawnPos))
            {
                var enemyShip = EnemyShipPool.Instance.GetEnemyShip();
                enemyShip.transform.position = spawnPos;
                enemyShip.transform.rotation = Quaternion.identity;

                // 設置為 EnemyShipSpawner 的子物件
                enemyShip.transform.SetParent(this.transform);

                AssignRandomWeapon(enemyShip); // 隨機分配武器

                Debug.Log($"[EnemyShipSpawner] 成功生成敵方船隻於位置: {spawnPos}");
                break;
            }
        }
    }

    private void AssignRandomWeapon(GameObject enemyShip)
    {
        if (weaponPrefabs == null || weaponPrefabs.Length == 0)
        {
            Debug.LogWarning("[EnemyShipSpawner] 沒有可用的武器預製件！");
            return;
        }

        int randomIndex = Random.Range(0, weaponPrefabs.Length);
        GameObject weaponPrefab = weaponPrefabs[randomIndex];
        if (weaponPrefab != null)
        {
            GameObject weapon = Instantiate(weaponPrefab, enemyShip.transform);
            weapon.transform.localPosition = Vector3.zero; // 將武器放置於船隻中心

            // 將武器添加到 Ship 的 weapons 列表中
            if (enemyShip.TryGetComponent<Ship>(out Ship ship))
            {
                if (weapon.TryGetComponent<Weapon>(out Weapon weaponComponent))
                {
                    ship.weapons.Add(weaponComponent);
                    Debug.Log($"[EnemyShipSpawner] 為敵方船隻分配了武器: {weaponPrefab.name}");
                }
                else
                {
                    Debug.LogWarning("[EnemyShipSpawner] 武器預製件缺少 Weapon 組件！");
                }
            }
        }
        else
        {
            Debug.LogWarning("[EnemyShipSpawner] 無法實例化武器預製件！");
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
