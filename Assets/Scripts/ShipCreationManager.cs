using UnityEngine;
using System.Collections.Generic;

public class ShipCreationManager : MonoBehaviour
{
    public static ShipCreationManager Instance { get; private set; }

    private List<GameObject> shipPrefabs = new List<GameObject>();

    [SerializeField] private MapController mapController;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // 自動載入 Resources/Prefabs/Player 下所有船隻預製物
        var loadedPrefabs = Resources.LoadAll<GameObject>("Prefabs/PlayerShips");
        shipPrefabs = new List<GameObject>(loadedPrefabs);
        Debug.Log($"[ShipCreationManager] 已自動載入 {shipPrefabs.Count} 個船隻預製物");
    }

    public PlayerShip TryCreateRandomShip(int inputGold, int inputOil, int inputCube)
    {

        // check if resources are enough
        GameDataController gameDataController = GameDataController.Instance;
        if (gameDataController == null)
        {
            Debug.LogError("[ShipCreatePanel] GameDataController 為 null，無法建造船隻");
            return null;

        }
        GameData.PlayerData playerData = gameDataController.CurrentGameData?.playerData;

        if (playerData == null)
        {
            Debug.LogError("[ShipCreatePanel] PlayerData 為 null，無法建造船隻");
            return null;
        }
        if (playerData.Gold < inputGold || playerData.Oils < inputOil || playerData.Cube < inputCube)
        {
            Debug.LogError("[ShipCreatePanel] 資源不足，無法建造船隻");
            return null;
        }

        // 隨機選擇一個船隻類型
        if (shipPrefabs == null || shipPrefabs.Count == 0)
        {
            Debug.LogError("[ShipCreationManager] 沒有可用的船隻預製物！");
            return null;
        }
        int shipTypeIdx = Random.Range(0, shipPrefabs.Count);

        // 扣除資源
        playerData.Gold -= inputGold;
        playerData.Oils -= inputOil;
        playerData.Cube -= inputCube;
        gameDataController.TriggerResourceChanged();

        // 實例化船隻
        PlayerShip newShip = InstantiateShip(shipTypeIdx);
        return newShip;

    }

    private PlayerShip InstantiateShip(int shipTypeIdx)
    {
        if (shipTypeIdx < 0 || shipTypeIdx >= shipPrefabs.Count)
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
        bool foundValid = false;
        for (int attempt = 0; attempt < 10; attempt++)
        {
            // 檢查半徑 1 內是否有其他 PlayerShip
            bool hasOtherPlayerShip = false;
            Collider2D[] colliders = Physics2D.OverlapCircleAll(spawnPosition, 1f);
            foreach (var col in colliders)
            {
                if (col.GetComponent<PlayerShip>() != null)
                {
                    hasOtherPlayerShip = true;
                    break;
                }
            }
            if (!hasOtherPlayerShip)
            {
                foundValid = true;
                break;
            }
            // 若有其他玩家船，嘗試尋找下一個最近海洋格子
            spawnPosition = mapController.FindNearestOceanTile(spawnPosition + Random.insideUnitSphere * 2f);
        }
        if (!foundValid || spawnPosition == Vector3.zero)
        {
            Debug.LogError("找不到 Chinju Tile 附近的最近海洋格子，或附近有其他玩家船！");
            return null;
        }

        spawnPosition.z = -1;

        GameObject battleShip = Instantiate(shipPrefab, spawnPosition, Quaternion.identity);
        battleShip.transform.SetParent(this.transform); // 設置為 ShipCreationManager 的子物件
        if (battleShip != null)
        {
            Debug.Log("[ShipCreationManager] 戰艦實例化成功！");
            SaveShipData(spawnPosition);


            return battleShip.GetComponent<PlayerShip>();
        }
        else
        {
            Debug.LogError("[ShipCreationManager] 戰艦實例化失敗！");
            return null;
        }
    }

    public GameObject AssignRandomWeapon(GameObject ship)
    {
        if (shipPrefabs == null || shipPrefabs.Count == 0)
        {
            Debug.LogWarning("[ShipCreationManager] 沒有可用的武器預製件！");
            return null;
        }

        GameObject weaponPrefab = Resources.Load<GameObject>($"Prefabs/Weapons/Turret");
        if (weaponPrefab != null)
        {
            GameObject weapon = Instantiate(weaponPrefab, ship.transform);
            weapon.transform.localPosition = Vector3.zero; // 將武器放置於船隻中心
            Debug.Log($"[ShipCreationManager] 為船隻分配了武器: {weaponPrefab.name}");

           ship.GetComponent<Warship>().AddWeapon(weapon.GetComponent<Weapon>());

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
                CurrentFuel = 100,
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

    public void InstantiateShipFromData(GameData.ShipData shipData)
    {
        if (shipData == null)
        {
            Debug.LogWarning("[ShipCreationManager] ShipData 為 null，無法實例化船隻！");
            return;
        }

        GameObject shipPrefab = Resources.Load<GameObject>($"Prefabs/Player/{shipData.PrefabName}");
        if (shipPrefab != null)
        {
            GameObject shipObj = Instantiate(shipPrefab, shipData.Position, Quaternion.Euler(0, 0, shipData.Rotation));
            var shipComp = shipObj.GetComponent<PlayerShip>();
            if (shipComp != null)
            {
                shipComp.LoadShipData(shipData);
                Debug.Log($"[ShipCreationManager] 已成功實例化船隻: {shipData.PrefabName}");
            }
            else
            {
                Debug.LogWarning("[ShipCreationManager] 實例化的物件缺少 PlayerShip 組件！");
            }
        }
        else
        {
            Debug.LogWarning($"[ShipCreationManager] 找不到船隻預製物: {shipData.PrefabName}");
        }
    }
}
