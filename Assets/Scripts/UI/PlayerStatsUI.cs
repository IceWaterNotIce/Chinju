using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// 處理玩家資源狀態的UI顯示
/// </summary>
public class PlayerStatsUI : MonoBehaviour
{
    private UIDocument uiDocument;
    private Label goldLabel;
    private Label oilLabel;
    private Label cubeLabel;
    private Label oilTransportLabel; // 新增石油運輸進度的 Label

    // 移除 public GameData gameData;
    private GameData gameData;

    void OnEnable()
    {
        uiDocument = GetComponent<UIDocument>();
        var root = uiDocument.rootVisualElement;
        
        // 使用 UIHelper 簡化 UI 初始化邏輯
        goldLabel = UIHelper.InitializeElement<Label>(root, "goldLabel");
        oilLabel = UIHelper.InitializeElement<Label>(root, "oilLabel");
        cubeLabel = UIHelper.InitializeElement<Label>(root, "cubeLabel");
        oilTransportLabel = UIHelper.InitializeElement<Label>(root, "oilTransportLabel"); // 確保 UXML 中有對應的 Label

        // 監聽 GameDataController 的資料變更
        if (GameDataController.Instance != null)
            GameDataController.Instance.OnGameDataChanged += OnGameDataChanged;

        // 自動從 GameDataController 取得 GameData
        SetGameDataFromController();
    }

    void OnDisable()
    {
        if (GameDataController.Instance != null)
            GameDataController.Instance.OnGameDataChanged -= OnGameDataChanged;

        if (gameData != null && gameData.playerData != null)
        {
            gameData.playerData.OnResourceChanged -= UpdateAllResourcesFromData;
        }
    }

    // 當 GameDataController.CurrentGameData 被更換時呼叫
    private void OnGameDataChanged(GameData newData)
    {
        SetGameDataFromController();
    }

    /// <summary>
    /// 從 GameDataController 取得 GameData 並刷新 UI
    /// </summary>
    public void SetGameDataFromController()
    {
        var data = GameDataController.Instance != null ? GameDataController.Instance.CurrentGameData : null;
        if (gameData != null && gameData.playerData != null)
        {
            gameData.playerData.OnResourceChanged -= UpdateAllResourcesFromData;
        }
        gameData = data;
        if (gameData != null && gameData.playerData != null)
        {
            gameData.playerData.OnResourceChanged += UpdateAllResourcesFromData;

            // 新增：立即更新 UI
            UpdateAllResourcesFromData();
        }
    }

    /// <summary>
    /// 更新所有資源顯示
    /// </summary>
    /// <param name="gold">金幣數量</param>
    /// <param name="oil">石油數量</param>
    /// <param name="cube">方塊數量</param>
    public void UpdateResourceDisplay(int gold, int oil, int cube)
    {
        UpdateGoldAmount(gold);
        UpdateOilAmount(oil);
        UpdateCubeAmount(cube);
    }

    /// <summary>
    /// 更新金幣數量顯示
    /// </summary>
    /// <param name="amount">金幣數量</param>
    public void UpdateGoldAmount(int amount)
    {
        if (goldLabel != null)
        {
            goldLabel.text = $"金幣: {amount}";
        }
    }

    /// <summary>
    /// 更新石油數量顯示
    /// </summary>
    /// <param name="amount">石油數量</param>
    public void UpdateOilAmount(int amount)
    {
        if (oilLabel != null)
        {
            oilLabel.text = $"石油: {amount}";
        }
    }

    /// <summary>
    /// 更新方塊數量顯示
    /// </summary>
    /// <param name="amount">方塊數量</param>
    public void UpdateCubeAmount(int amount)
    {
        if (cubeLabel != null)
        {
            cubeLabel.text = $"方塊: {amount}";
        }
    }

    // 新增：從 GameData 更新 UI
    private void UpdateAllResourcesFromData()
    {
        if (gameData != null && gameData.playerData != null)
        {
            UpdateResourceDisplay(
                Mathf.RoundToInt(gameData.playerData.Gold),
                Mathf.RoundToInt(gameData.playerData.Oils),
                gameData.playerData.Cube
            );
        }
    }

    // 新增：更新石油運輸進度顯示
    public void UpdateOilTransportProgress(string progress)
    {
        if (oilTransportLabel != null)
        {
            oilTransportLabel.text = progress;
        }
    }
}
