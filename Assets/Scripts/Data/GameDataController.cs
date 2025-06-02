using System;
using UnityEngine;
using System.Linq; // 新增引用
using System.Collections.Generic;
using System.IO; // 保留：用於文件操作

// 遊戲資料控制器，集中管理 GameData 實例
public class GameDataController : Singleton<GameDataController>
{
    [SerializeField]
    private GameData currentGameData;
    public GameData CurrentGameData
    {
        get => currentGameData;
        set
        {
            if (currentGameData != value)
            {
                currentGameData = value;
                OnGameDataChanged?.Invoke(currentGameData);

                // 主動觸發資源事件，讓 UI 立即刷新
                currentGameData?.players.FirstOrDefault()?.OnResourceChanged?.Invoke();
            }
        }
    }

    public event System.Action<GameData> OnGameDataChanged;
    public event System.Action OnMapDataChanged; // 新增：地圖數據變更事件
    public event System.Action OnFleetDataChanged; // 新增：艦隊數據變更事件

    private new void Awake()
    {
        Debug.Log("[GameDataController] Awake 方法執行。");

        // 確保 currentGameData 初始化，但避免覆蓋序列化加載的數據
        if (currentGameData == null)
        {
            Debug.Log("[GameDataController] 初始化 currentGameData。");
            currentGameData = new GameData
            {
                players = new List<GameData.PlayerData> { new GameData.PlayerData() }
            };
        }
        else
        {
            Debug.Log("[GameDataController] currentGameData 已存在，可能由序列化加載。");
        }
    }

    public void TriggerResourceChanged()
    {
        currentGameData?.players.FirstOrDefault()?.OnResourceChanged?.Invoke();
        Debug.Log("[GameDataController] 資源事件已觸發，UI 更新完成");
    }

    public void TriggerMapDataChanged()
    {
        OnMapDataChanged?.Invoke(); // 發送地圖數據變更事件
        Debug.Log("[GameDataController] 地圖數據事件已觸發");
    }

    public void TriggerFleetDataChanged()
    {
        OnFleetDataChanged?.Invoke(); // 發送艦隊數據變更事件
        Debug.Log("[GameDataController] 艦隊數據事件已觸發");
    }

    /// <summary>
    /// 檢查玩家是否擁有足夠的資源
    /// </summary>
    /// <param name="gold">所需金幣</param>
    /// <param name="oil">所需石油</param>
    /// <param name="cube">所需方塊</param>
    /// <param name="fuel">所需燃料</param>
    /// <param name="shipId">指定檢查的船隻 ID（可選）</param> <!-- 新增參數 -->
    /// <returns>是否擁有足夠資源</returns>
    public bool HasEnoughResources(int gold, int oil, int cube, float fuel = 0f, string shipId = null) // 新增船隻檢查
    {
        if (currentGameData?.players == null)
        {
            Debug.LogWarning("[GameDataController] 無法檢查資源，PlayerData 為 null！");
            return false;
        }

        var playerData = currentGameData.players.FirstOrDefault();

        bool hasEnoughFuel = true;
        if (!string.IsNullOrEmpty(shipId))
        {
            var ship = playerData.Ships.FirstOrDefault(s => s.ShipId == shipId);
            hasEnoughFuel = ship != null && ship.CurrentFuel >= fuel;
        }
        else
        {
            hasEnoughFuel = playerData.Ships.All(ship => ship.CurrentFuel >= fuel);
        }

        return playerData.Gold >= gold &&
               playerData.Oils >= oil &&
               playerData.Cube >= cube &&
               hasEnoughFuel; // 檢查指定船隻或所有船隻的燃料
    }

    /// <summary>
    /// 消耗玩家資源
    /// </summary>
    /// <param name="gold">消耗金幣</param>
    /// <param name="oil">消耗石油</param>
    /// <param name="cube">消耗方塊</param>
    /// <param name="fuel">消耗燃料</param> <!-- 新增參數 -->
    /// <returns>是否成功消耗資源</returns>
    public bool ConsumeResources(int gold, int oil, int cube, float fuel = 0f) // 新增燃料參數
    {
        if (!HasEnoughResources(gold, oil, cube, fuel))
        {
            var localPlayerData = currentGameData?.players.FirstOrDefault(); // 修正命名衝突
            if (localPlayerData != null)
            {
                Debug.LogWarning($"[GameDataController] 資源不足！金幣: {localPlayerData.Gold}/{gold}, 石油: {localPlayerData.Oils}/{oil}, 方塊: {localPlayerData.Cube}/{cube}, 燃料: {fuel}");
            }
            return false;
        }

        var playerData = currentGameData.players.FirstOrDefault();
        playerData.Gold -= gold;
        playerData.Oils -= oil;
        playerData.Cube -= cube;

        foreach (var ship in playerData.Ships)
        {
            ship.CurrentFuel -= fuel; // 消耗燃料
        }

        TriggerResourceChanged();
        Debug.Log($"[GameDataController] 成功消耗資源：金幣-{gold}，石油-{oil}，方塊-{cube}，燃料-{fuel}");
        return true;
    }

    public void SaveGameData()
    {
        if (currentGameData == null)
        {
            Debug.LogError("[GameDataController] 無法保存遊戲數據，currentGameData 為 null！");
            return;
        }

        try
        {
            string json = JsonUtility.ToJson(currentGameData, true);
            string path = Path.Combine(Application.persistentDataPath, "savegame.json");
            File.WriteAllText(path, json);
            Debug.Log($"[GameDataController] 遊戲數據已保存至 {path}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[GameDataController] 保存遊戲數據時發生錯誤: {ex.Message}");
        }
    }

    private void SyncPlayerResources(GameData.PlayerData source, GameData.PlayerData target)
    {
        if (source != null && target != null)
        {
            target.Oils = source.Oils;
            target.Gold = source.Gold;
            target.Cube = source.Cube;
            target.Level = source.Level;
            target.Exp = source.Exp;
        }
    }

    public void LoadGameData(GameData data)
    {
        if (data == null)
        {
            Debug.LogError("[GameDataController] 無法加載遊戲數據，data 為 null！");
            return;
        }

        // 確保 players 列表存在
        if (data.players == null || data.players.Count == 0)
        {
            Debug.LogWarning("[GameDataController] 玩家數據列表為空，無法加載！");
            return;
        }

        // 加載玩家資源
        var playerData = data.players.FirstOrDefault();
        if (playerData != null)
        {
            SyncPlayerResources(playerData, playerData);
        }

        CurrentGameData = data; // 更新當前遊戲數據
        Debug.Log("[GameDataController] 遊戲數據已成功加載。");
    }


    public void ResetGameData()
    {
        Debug.Log("[GameDataController] 重置遊戲數據。");
        currentGameData = new GameData
        {
            players = new List<GameData.PlayerData> { new GameData.PlayerData() }
        };
        OnGameDataChanged?.Invoke(currentGameData);
    }

    public void UpdateShipPosition(String ShipId, Vector3 newPosition)
    {
        if (currentGameData == null || currentGameData.players == null)
        {
            Debug.LogWarning("[GameDataController] 無法更新船隻位置，GameData 或 PlayerData 為 null！");
            return;
        }

        var playerData = currentGameData.players.FirstOrDefault();
        if (playerData != null)
        {
            var shipData = playerData.Ships.FirstOrDefault(s => s.ShipId == ShipId);
            if (shipData != null)
            {
                shipData.Position = newPosition;
                Debug.Log($"[GameDataController] 更新船隻 {ShipId} 的位置為: {newPosition}");
            }
            else
            {
                Debug.LogWarning($"[GameDataController] 找不到船隻 {ShipId} 的數據，無法更新位置。");
            }
        }

        var enemyShipData = currentGameData.enemyData?.EnemyShips?.FirstOrDefault(s => s.ShipId == ShipId);
        if (enemyShipData != null)
        {
            enemyShipData.Position = newPosition;
            Debug.Log($"[GameDataController] 更新敵方船隻 {ShipId} 的位置為: {newPosition}");
        }
        else
        {
            Debug.LogWarning($"[GameDataController] 找不到敵方船隻 {ShipId} 的數據，無法更新位置。");
        }
    }

    public void UpdateShipFuel(string shipId, float newFuel)
    {
        if (currentGameData == null || currentGameData.players == null)
        {
            Debug.LogWarning("[GameDataController] 無法更新船隻燃料，GameData 或 PlayerData 為 null！");
            return;
        }

        var playerData = currentGameData.players.FirstOrDefault();
        if (playerData != null)
        {
            var shipData = playerData.Ships.FirstOrDefault(s => s.ShipId == shipId);
            if (shipData != null)
            {
                shipData.CurrentFuel = newFuel;
                Debug.Log($"[GameDataController] 更新船隻 {shipId} 的燃料為: {newFuel}");
            }
            else
            {
                Debug.LogWarning($"[GameDataController] 找不到船隻 {shipId} 的數據，無法更新燃料。");
            }
        }

        var enemyShipData = currentGameData.enemyData?.EnemyShips?.FirstOrDefault(s => s.ShipId == shipId);
        if (enemyShipData != null)
        {
            enemyShipData.CurrentFuel = newFuel;
            Debug.Log($"[GameDataController] 更新敵方船隻 {shipId} 的燃料為: {newFuel}");
        }
        else
        {
            Debug.LogWarning($"[GameDataController] 找不到敵方船隻 {shipId} 的數據，無法更新燃料。");
        }
    }
}
