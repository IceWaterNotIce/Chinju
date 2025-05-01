using System;
using UnityEngine;

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
        if (currentGameData?.playerData?.OnResourceChanged != null)
        {
            currentGameData.playerData.OnResourceChanged.Invoke();
            Debug.Log("[GameDataController] 資源事件已觸發，UI 更新完成");
        }
    }
}
