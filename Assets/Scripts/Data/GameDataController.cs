using System;
using UnityEngine;
using System.Linq; // 新增引用

// 遊戲資料控制器，集中管理 GameData 實例
public class GameDataController : MonoBehaviour
{
    public static GameDataController Instance { get; private set; }

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
                currentGameData?.playerData?.OnResourceChanged?.Invoke();
            }
        }
    }

    public System.Action<GameData> OnGameDataChanged;
    public System.Action OnMapDataChanged; // 新增：地圖數據變更事件

    private void Awake()
    {
        Debug.Log("[GameDataController] Awake 方法執行。");

        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 確保 currentGameData 初始化
        if (currentGameData == null)
        {
            Debug.Log("[GameDataController] 初始化 currentGameData。");
            currentGameData = new GameData
            {
                playerData = new GameData.PlayerData()
            };
        }
        else
        {
            Debug.Log("[GameDataController] currentGameData 已存在。");
        }
    }

    public void TriggerResourceChanged()
    {
        currentGameData?.playerData?.OnResourceChanged?.Invoke();
        Debug.Log("[GameDataController] 資源事件已觸發，UI 更新完成");
    }

    public void TriggerMapDataChanged()
    {
        OnMapDataChanged?.Invoke(); // 發送地圖數據變更事件
        Debug.Log("[GameDataController] 地圖數據事件已觸發");
    }

    /// <summary>
    /// 檢查玩家是否擁有足夠的資源
    /// </summary>
    /// <param name="gold">所需金幣</param>
    /// <param name="oil">所需石油</param>
    /// <param name="cube">所需方塊</param>
    /// <param name="fuel">所需燃料</param> <!-- 新增參數 -->
    /// <returns>是否擁有足夠資源</returns>
    public bool HasEnoughResources(int gold, int oil, int cube, float fuel = 0f) // 新增燃料參數
    {
        if (currentGameData?.playerData == null)
        {
            Debug.LogWarning("[GameDataController] 無法檢查資源，PlayerData 為 null！");
            return false;
        }

        var playerData = currentGameData.playerData;
        return playerData.Gold >= gold &&
               playerData.Oils >= oil &&
               playerData.Cube >= cube &&
               playerData.Ships.All(ship => ship.CurrentFuel >= fuel); // 檢查所有船隻的燃料
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
            Debug.LogWarning("[GameDataController] 資源不足，無法消耗！");
            return false;
        }

        var playerData = currentGameData.playerData;
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
}
