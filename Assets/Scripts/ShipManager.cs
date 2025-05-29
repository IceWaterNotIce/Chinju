using System.Collections.Generic;
using UnityEngine;

public class ShipManager : Singleton<ShipManager>
{
    private Dictionary<string, Ship> ships = new Dictionary<string, Ship>();
    private List<GameObject> shipPrefabs = new List<GameObject>();
    [SerializeField] private MapController mapController;

    private new void Awake() // 使用 new 關鍵字以隱藏基類的 Awake 方法
    {
        base.Awake(); // 呼叫基類的 Awake 方法以確保 Singleton 正常運作

        // 自動載入船隻預製物
        var loadedPrefabs = Resources.LoadAll<GameObject>("Prefabs/Ships/Warships");
        shipPrefabs = new List<GameObject>(loadedPrefabs);
        Debug.Log($"[ShipManager] 已自動載入 {shipPrefabs.Count} 個船隻預製物");
    }

    public Ship GetShipById(string shipId)
    {
        ships.TryGetValue(shipId, out var ship);
        return ship;
    }

    public void RegisterShip(Ship ship)
    {
        if (!ships.ContainsKey(ship.ShipId))
        {
            ships.Add(ship.ShipId, ship);
            Debug.Log($"[ShipManager] Registered ship: {ship.name} with ID: {ship.ShipId}");
        }
    }

    public void UnregisterShip(Ship ship)
    {
        if (ships.ContainsKey(ship.ShipId))
        {
            ships.Remove(ship.ShipId);
            Debug.Log($"[ShipManager] Unregistered ship: {ship.name} with ID: {ship.ShipId}");
        }
    }

    public PlayerShip TryCreateRandomShip(int inputGold, int inputOil, int inputCube)
    {
        GameData.PlayerData playerData = GameDataController.Instance.CurrentGameData?.playerData;

        if (playerData == null)
        {
            Debug.LogError("[ShipManager] PlayerData 為 null，無法建造船隻");
            return null;
        }
        if (playerData.Gold < inputGold || playerData.Oils < inputOil || playerData.Cube < inputCube)
        {
            Debug.LogError("[ShipManager] 資源不足，無法建造船隻");
            return null;
        }

        if (shipPrefabs == null || shipPrefabs.Count == 0)
        {
            Debug.LogError("[ShipManager] 沒有可用的船隻預製物！");
            return null;
        }

        int shipTypeIdx = Random.Range(0, shipPrefabs.Count);
        GameObject shipPrefab = shipPrefabs[shipTypeIdx];
        string shipName = shipPrefab.name;

        var existShip = playerData.Ships.Find(s => s.Name == shipName);
        if (existShip != null)
        {
            existShip.Level += 1;
            Debug.Log($"[ShipManager] 玩家已擁有 {shipName}，等級提升至 {existShip.Level}");
            playerData.Gold -= inputGold;
            playerData.Oils -= inputOil;
            playerData.Cube -= inputCube;
            GameDataController.Instance.TriggerResourceChanged();
            return null;
        }

        playerData.Gold -= inputGold;
        playerData.Oils -= inputOil;
        playerData.Cube -= inputCube;
        GameDataController.Instance.TriggerResourceChanged();

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
        if (Physics2D.OverlapCircle(spawnPosition, 0.5f, LayerMask.GetMask("Ship")) != null)
        {
            spawnPosition += new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0);
        }

        spawnPosition.z = -1;

        GameObject battleShip = Instantiate(shipPrefab, spawnPosition, Quaternion.identity);
        battleShip.transform.SetParent(this.transform);
        if (battleShip != null)
        {
            Debug.Log("[ShipManager] 戰艦實例化成功！");
            SaveShipData(spawnPosition);
            return battleShip.GetComponent<PlayerShip>();
        }
        else
        {
            Debug.LogError("[ShipManager] 戰艦實例化失敗！");
            return null;
        }
    }

    private void SaveShipData(Vector3 position)
    {
        var data = GameDataController.Instance.CurrentGameData;
        if (data != null && data.playerData != null)
        {
            string prefabName = shipPrefabs.Count > 0 ? shipPrefabs[shipPrefabs.Count - 1].name : "";
            var shipData = new GameData.ShipData
            {
                Name = prefabName,
                Health = 100,
                AttackPower = 20,
                Defense = 10,
                Position = position,
                CurrentFuel = 100,
                Speed = 5,
                Rotation = 0,
                Level = 1,
                PrefabName = prefabName
            };
            data.playerData.Ships.Add(shipData);
            Debug.Log("[ShipManager] 已將新戰艦資料存入 GameData");
        }
        else
        {
            Debug.LogWarning("無法儲存船隻資料到 GameData，playerData 為 null");
        }
    }

    public void InstantiateShipFromData(GameData.ShipData shipData)
    {
        if (shipData == null)
        {
            Debug.LogWarning("[ShipManager] ShipData 為 null，無法實例化船隻！");
            return;
        }

        GameObject shipPrefab = Resources.Load<GameObject>($"Prefabs/Ships/Warships/{shipData.PrefabName}");
        if (shipPrefab != null)
        {
            GameObject shipObj = Instantiate(shipPrefab, shipData.Position, Quaternion.Euler(0, 0, shipData.Rotation));
            var shipComp = shipObj.GetComponent<PlayerShip>();
            if (shipComp != null)
            {
                shipComp.LoadShipData(shipData);
                Debug.Log($"[ShipManager] 已成功實例化船隻: {shipData.PrefabName}");
            }
            else
            {
                Debug.LogWarning("[ShipManager] 實例化的物件缺少 PlayerShip 組件！");
            }
        }
        else
        {
            Debug.LogWarning($"[ShipManager] 找不到船隻預製物: {shipData.PrefabName}");
        }
    }

    public GameObject AssignRandomWeapon(GameObject ship)
    {
        if (shipPrefabs == null || shipPrefabs.Count == 0)
        {
            Debug.LogWarning("[ShipManager] 沒有可用的武器預製件！");
            return null;
        }

        GameObject weaponPrefab = Resources.Load<GameObject>($"Prefabs/Weapons/Turret");
        if (weaponPrefab != null)
        {
            GameObject weapon = Instantiate(weaponPrefab, ship.transform);
            weapon.transform.localPosition = Vector3.zero;
            Debug.Log($"[ShipManager] 為船隻分配了武器: {weaponPrefab.name}");
            ship.GetComponent<Warship>().AddWeapon(weapon.GetComponent<Weapon>());
            return weapon;
        }
        else
        {
            Debug.LogWarning("[ShipManager] 無法實例化武器預製件！");
            return null;
        }
    }
}
