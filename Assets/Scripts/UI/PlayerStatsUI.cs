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

    // 新增 GameData 參考
    public GameData gameData;

    void OnEnable()
    {
        uiDocument = GetComponent<UIDocument>();
        var root = uiDocument.rootVisualElement;
        
        // 獲取資源標籤引用
        goldLabel = root.Q<Label>("GoldLabel");
        oilLabel = root.Q<Label>("OilLabel");
        cubeLabel = root.Q<Label>("CubeLabel");

        // 假設 gameData 已經被指派
        if (gameData != null && gameData.PlayerDatad != null)
        {
            gameData.PlayerDatad.OnResourceChanged += UpdateAllResourcesFromData;
            UpdateAllResourcesFromData();
        }
    }

    void OnDisable()
    {
        if (gameData != null && gameData.PlayerDatad != null)
        {
            gameData.PlayerDatad.OnResourceChanged -= UpdateAllResourcesFromData;
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
        if (gameData != null && gameData.PlayerDatad != null)
        {
            UpdateResourceDisplay(
                (int)gameData.PlayerDatad.Gold,
                (int)gameData.PlayerDatad.Oils,
                gameData.PlayerDatad.Cube
            );
        }
    }
}
