using UnityEngine;

public class ShipCreationManager : MonoBehaviour
{
    public static ShipCreationManager Instance { get; private set; }

    [SerializeField] private GameObject[] shipPrefabs = new GameObject[5];

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // 嘗試建造船隻，回傳是否成功
    public bool TryCreateShip(int shipType, int goldCost, int oilCost, int cubeCost)
    {
        // 取得玩家資源（這裡假設有 GameDataController.Instance.CurrentGameData）
        var data = GameDataController.Instance.CurrentGameData;
        if (data.PlayerDatad.Gold < goldCost || data.PlayerDatad.Oils < oilCost || data.PlayerDatad.Cube < cubeCost)
            return false;

        // 扣除資源
        data.PlayerDatad.Gold -= goldCost;
        data.PlayerDatad.Oils -= oilCost;
        data.PlayerDatad.Cube -= cubeCost;

        // 生成船隻
        if (shipType < 0 || shipType >= shipPrefabs.Length) return false;
        Instantiate(shipPrefabs[shipType], GetSpawnPosition(), Quaternion.identity);

        return true;
    }

    // 取得生成位置（可自訂）
    private Vector3 GetSpawnPosition()
    {
        return Vector3.zero; // 這裡可根據遊戲需求調整
    }
}
