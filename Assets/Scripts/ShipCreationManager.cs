using UnityEngine;

public class ShipCreationManager : MonoBehaviour
{
    public static ShipCreationManager Instance { get; private set; }

    [SerializeField] private GameObject[] shipPrefabs = new GameObject[5];
    [SerializeField]
    public int[,] shipCosts = {
        { 800, 400, 200 }, // 航空母艦
        { 500, 200, 100 }, // 戰艦
        { 300, 120, 60 },  // 巡洋艦
        { 200, 80, 40 },   // 驅逐艦
        { 150, 60, 30 }    // 潛艦
    };

    [SerializeField] private MapController mapController;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public Ship TryCreateRandomShip(int inputGold, int inputOil, int inputCube)
    {
        System.Collections.Generic.List<int> candidates = new System.Collections.Generic.List<int>();
        System.Collections.Generic.List<int> weights = new System.Collections.Generic.List<int>();

        for (int i = 0; i < shipPrefabs.Length; i++)
        {
            if (GameDataController.Instance?.HasEnoughResources(shipCosts[i, 0], shipCosts[i, 1], shipCosts[i, 2]) ?? false)
            {
                int dist = Mathf.Abs(inputGold - shipCosts[i, 0]) +
                           Mathf.Abs(inputOil - shipCosts[i, 1]) +
                           Mathf.Abs(inputCube - shipCosts[i, 2]);
                int weight = Mathf.Max(1, 100 - dist);
                candidates.Add(i);
                weights.Add(weight);
            }
        }

        if (candidates.Count == 0)
        {
            Debug.LogWarning("[ShipCreationManager] 沒有任何船型可隨機建造！");
            return null;
        }

        int totalWeight = 0;
        foreach (var w in weights) totalWeight += w;
        int rand = Random.Range(0, totalWeight);
        int chosenIdx = 0;
        for (int i = 0; i < weights.Count; i++)
        {
            if (rand < weights[i])
            {
                chosenIdx = i;
                break;
            }
            rand -= weights[i];
        }
        int shipType = candidates[chosenIdx];

        if (GameDataController.Instance?.ConsumeResources(shipCosts[shipType, 0], shipCosts[shipType, 1], shipCosts[shipType, 2]) ?? false)
        {
            // 實例化船隻
            return InstantiateShip(shipType);
        }
        else
        {
            Debug.LogWarning("[ShipCreationManager] 資源不足，無法建造戰艦！");
            return null;
        }

    }

    private Ship InstantiateShip(int shipTypeIdx)
    {
        if (shipTypeIdx < 0 || shipTypeIdx >= shipPrefabs.Length)
        {
            Debug.LogError($"無效的船型索引：{shipTypeIdx}");
            return null;
        }

        GameObject shipPrefab = shipPrefabs[shipTypeIdx];
        if (shipPrefab == null)
        {
            Debug.LogError($"找不到船隻預製件：索引 {shipTypeIdx}");
            return null;
        }

        if (mapController == null)
        {
            Debug.LogError("MapController 未設置！");
            return null;
        }

        Vector3 chinjuTilePosition = mapController.GetChinjuTileWorldPosition();
        if (chinjuTilePosition == Vector3.zero)
        {
            Debug.LogError("找不到 Chinju Tile 的位置！");
            return null;
        }

        Vector3 spawnPosition = mapController.FindNearestOceanTile(chinjuTilePosition);
        if (spawnPosition == Vector3.zero)
        {
            Debug.LogError("找不到 Chinju Tile 附近的最近海洋格子！");
            return null;
        }

        spawnPosition.z = -1;

        GameObject battleShip = Instantiate(shipPrefab, spawnPosition, Quaternion.identity);
        battleShip.transform.SetParent(this.transform); // 設置為 ShipCreationManager 的子物件
        if (battleShip != null)
        {
            Debug.Log("[ShipCreationManager] 戰艦實例化成功！");
            SaveShipData(spawnPosition);
            return battleShip.GetComponent<Ship>();
        }
        else
        {
            Debug.LogError("[ShipCreationManager] 戰艦實例化失敗！");
            return null;
        }
    }

    public GameObject AssignRandomWeapon(GameObject ship)
    {
        if (shipPrefabs == null || shipPrefabs.Length == 0)
        {
            Debug.LogWarning("[ShipCreationManager] 沒有可用的武器預製件！");
            return null;
        }

        int randomIndex = Random.Range(0, shipPrefabs.Length);
        GameObject weaponPrefab = shipPrefabs[randomIndex];
        if (weaponPrefab != null)
        {
            GameObject weapon = Instantiate(weaponPrefab, ship.transform);
            weapon.transform.localPosition = Vector3.zero; // 將武器放置於船隻中心
            Debug.Log($"[ShipCreationManager] 為船隻分配了武器: {weaponPrefab.name}");
            return weapon;
        }
        else
        {
            Debug.LogWarning("[ShipCreationManager] 無法實例化武器預製件！");
            return null;
        }
    }

    private void SaveShipData(Vector3 position)
    {
        var data = GameDataController.Instance.CurrentGameData;
        if (data != null && data.playerData != null)
        {
            var shipData = new GameData.ShipData
            {
                Name = "戰艦",
                Health = 100,
                AttackPower = 20,
                Defense = 10,
                Position = position,
                Fuel = 100,
                Speed = 5,
                Rotation = 0
            };
            data.playerData.Ships.Add(shipData);
            Debug.Log("[ShipCreationManager] 已將新戰艦資料存入 GameData");
        }
        else
        {
            Debug.LogWarning("無法儲存船隻資料到 GameData，playerData 為 null");
        }
    }

    // 取得生成位置（可自訂）
    private Vector3 GetSpawnPosition()
    {
        return Vector3.zero; // 這裡可根據遊戲需求調整
    }
}
