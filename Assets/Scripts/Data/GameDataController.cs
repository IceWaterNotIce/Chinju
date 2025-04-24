using UnityEngine;

// 遊戲資料控制器，集中管理 GameData 實例
public class GameDataController : MonoBehaviour
{
    public static GameDataController Instance { get; private set; }

    private GameData currentGameData;
    public GameData CurrentGameData
    {
        get => currentGameData;
        set
        {
            if (currentGameData != value)
            {
                currentGameData = value;
                if (OnGameDataChanged != null)
                    OnGameDataChanged.Invoke(currentGameData);
                // 新增：主動觸發資源事件，讓 UI 立即刷新
                if (currentGameData != null && currentGameData.PlayerDatad != null && currentGameData.PlayerDatad.OnResourceChanged != null)
                    currentGameData.PlayerDatad.OnResourceChanged.Invoke();
            }
        }
    }

    public System.Action<GameData> OnGameDataChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
