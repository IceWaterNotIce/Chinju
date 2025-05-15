using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class EnemyShipSpawner : MonoBehaviour
{
    public static EnemyShipSpawner Instance { get; private set; }

    [Header("Spawn Settings")]
    public float spawnInterval = 2f;
    public int maxEnemyShips = 100;
    private Vector2 spawnAreaMin = new Vector2(0, 0);
    private Vector2 spawnAreaMax = new Vector2(100, 100);

    [Header("Progress Bar Controller")]
    [SerializeField] private ProgressBarController progressBarController;

    [Header("Progress Bar Settings")]
    private VisualElement progressBar; // 修正：定義進度條變數
    private float progressBarTimer = 0f; // 修正：定義進度條計時器
    [SerializeField] private float progressBarDuration = 10f; // 修正：定義進度條持續時間
    private float originalSpawnInterval; // 修正：定義原始生成間隔

    private float timer;

    private Tilemap tilemap;
    private TileBase oceanTile;

    private bool isSpeedBoosted = false; // 用於追蹤是否處於加速狀態
    private float speedBoostTimer = 0f; // 用於計時加速持續時間

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

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
        originalSpawnInterval = spawnInterval; // 保存原始生成間隔
        InitializeProgressBar();

        if (progressBarController != null)
        {
            progressBarController.OnProgressComplete += HandleProgressComplete;
        }
    }

    private void OnDestroy()
    {
        if (progressBarController != null)
        {
            progressBarController.OnProgressComplete -= HandleProgressComplete;
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

        if (isSpeedBoosted)
        {
            speedBoostTimer += Time.deltaTime;
            if (speedBoostTimer >= 5f) // 加速持續 5 秒
            {
                spawnInterval = originalSpawnInterval; // 恢復到原始生成間隔
                isSpeedBoosted = false;
                speedBoostTimer = 0f;
                Debug.Log($"[EnemyShipSpawner] Spawn interval reset to original: {spawnInterval} seconds.");
            }
        }

        UpdateProgressBar();
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

                Debug.Log($"[EnemyShipSpawner] 成功生成敵方船隻於位置: {spawnPos}");
                break;
            }
        }
    }

    public void SpawnEnemyFromData(GameData.ShipData shipData)
    {
        if (shipData == null)
        {
            Debug.LogWarning("[EnemyShipSpawner] ShipData 為 null，無法生成敵人！");
            return;
        }

        GameObject enemyShip = EnemyShipPool.Instance.GetEnemyShip();
        if (enemyShip != null)
        {
            var enemyComp = enemyShip.GetComponent<EnemyShip>();
            if (enemyComp != null)
            {
                enemyComp.LoadShipData(shipData);

                // 載入武器
                foreach (var weaponData in shipData.Weapons)
                {
                    if (!string.IsNullOrEmpty(weaponData.PrefabName))
                    {
                        GameObject weaponPrefab = Resources.Load<GameObject>($"Prefabs/Weapons/{weaponData.PrefabName}");
                        if (weaponPrefab != null)
                        {
                            GameObject weapon = Instantiate(weaponPrefab, enemyShip.transform);
                            weapon.transform.localPosition = Vector3.zero;

                            if (weapon.TryGetComponent<Weapon>(out Weapon weaponComponent))
                            {
                                enemyComp.AddWeapon(weaponComponent);
                                Debug.Log($"[EnemyShipSpawner] 已為敵方船隻載入武器: {weaponData.Name}");
                            }
                            else
                            {
                                Debug.LogWarning($"[EnemyShipSpawner] 武器預製件 {weaponData.PrefabName} 缺少 Weapon 組件！");
                            }
                        }
                        else
                        {
                            Debug.LogWarning($"[EnemyShipSpawner] 無法找到武器預製件: {weaponData.PrefabName}");
                        }
                    }
                }

                Debug.Log($"[EnemyShipSpawner] 已成功生成敵人: {shipData.PrefabName}");
            }
            else
            {
                Debug.LogWarning("[EnemyShipSpawner] 生成的物件缺少 EnemyShip 組件！");
            }
        }
        else
        {
            Debug.LogWarning("[EnemyShipSpawner] 無法生成敵人！");
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

    private void InitializeProgressBar()
    {
        var uiDocument = FindFirstObjectByType<UIDocument>();
        if (uiDocument != null)
        {
            var root = uiDocument.rootVisualElement;
            progressBar = root.Q<VisualElement>("progress-bar");
        }

        if (progressBar == null)
        {
            Debug.LogError("[EnemyShipSpawner] Progress bar not found in UI!");
        }
    }

    private void UpdateProgressBar()
    {
        if (progressBar == null) return;

        progressBarTimer += Time.deltaTime;
        float progress = Mathf.Clamp01(progressBarTimer / progressBarDuration);
        progressBar.style.width = new Length(progress * 100, LengthUnit.Percent);

        if (progressBarTimer >= progressBarDuration)
        {
            if (!isSpeedBoosted)
            {
                spawnInterval = Mathf.Max(1f, spawnInterval - 5f); // 每次減少 5 秒，最低為 1 秒
                isSpeedBoosted = true; // 設置為加速狀態
                Debug.Log($"[EnemyShipSpawner] Spawn interval decreased to {spawnInterval} seconds.");
            }
            progressBarTimer = 0f; // 重置進度條計時器
        }
    }

    private void HandleProgressComplete()
    {
        spawnInterval = Mathf.Max(1f, spawnInterval - 5f); // 每次減少 5 秒，最低為 1 秒
        Debug.Log($"[EnemyShipSpawner] Spawn interval decreased to {spawnInterval} seconds.");
    }
}
