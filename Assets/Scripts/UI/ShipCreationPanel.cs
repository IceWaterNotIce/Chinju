using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// 控制船隻建造面板的行為
/// </summary>
public class ShipCreationPanel : MonoBehaviour
{
    private UIDocument uiDocument;
    private VisualElement root;
    private VisualElement panel;
    private Button battleShipBtn;
    private Button createShipBtn;
    private Label goldCostLabel;
    private Label oilCostLabel;
    private Label cubeCostLabel;

    // 戰艦建造成本
    private readonly int[] shipCosts = { 500, 200, 100 }; // 金幣, 石油, 方塊

    void OnEnable()
    {
        uiDocument = GetComponent<UIDocument>();
        root = uiDocument.rootVisualElement;
        
        // 獲取面板根元素
        panel = root.Q<VisualElement>("ship-creation-panel");
        if (panel != null)
        {
            panel.style.display = DisplayStyle.None; // 初始時隱藏面板
        }

        // 獲取UI元素引用
        battleShipBtn = root.Q<Button>("battle-ship-btn");
        createShipBtn = root.Q<Button>("create-ship-btn");
        goldCostLabel = root.Q<Label>("gold-cost");
        oilCostLabel = root.Q<Label>("oil-cost");
        cubeCostLabel = root.Q<Label>("cube-cost");

        // 註冊按鈕點擊事件
        battleShipBtn.clicked += SelectBattleShip;
        createShipBtn.clicked += CreateShip;

        // 初始化UI
        createShipBtn.SetEnabled(false);
        UpdateCostDisplay(0, 0, 0);
    }

    /// <summary>
    /// 顯示面板
    /// </summary>
    public void Show()
    {
        if (panel != null)
        {
            panel.style.display = DisplayStyle.Flex;
        }
    }

    /// <summary>
    /// 隱藏面板
    /// </summary>
    public void Hide()
    {
        if (panel != null)
        {
            panel.style.display = DisplayStyle.None;
            // 重置面板狀態
            ResetPanel();
        }
    }

    /// <summary>
    /// 切換面板顯示狀態
    /// </summary>
    public void Toggle()
    {
        if (panel != null)
        {
            bool isVisible = panel.style.display == DisplayStyle.Flex;
            panel.style.display = isVisible ? DisplayStyle.None : DisplayStyle.Flex;
            
            // 如果隱藏面板，重置狀態
            if (!isVisible)
            {
                ResetPanel();
            }
        }
    }

    /// <summary>
    /// 重置面板狀態
    /// </summary>
    private void ResetPanel()
    {
        battleShipBtn.RemoveFromClassList("selected");
        createShipBtn.SetEnabled(false);
        UpdateCostDisplay(0, 0, 0);
    }

    /// <summary>
    /// 選擇戰艦
    /// </summary>
    private void SelectBattleShip()
    {
        battleShipBtn.ToggleInClassList("selected");
        createShipBtn.SetEnabled(battleShipBtn.ClassListContains("selected"));

        // 更新資源消耗顯示
        if (battleShipBtn.ClassListContains("selected"))
        {
            UpdateCostDisplay(shipCosts[0], shipCosts[1], shipCosts[2]);
        }
        else
        {
            UpdateCostDisplay(0, 0, 0);
        }
    }

    /// <summary>
    /// 更新資源消耗顯示
    /// </summary>
    private void UpdateCostDisplay(int gold, int oil, int cube)
    {
        goldCostLabel.text = $"金幣: {gold}";
        oilCostLabel.text = $"石油: {oil}";
        cubeCostLabel.text = $"方塊: {cube}";
    }

    /// <summary>
    /// 創建戰艦
    /// </summary>
    private void CreateShip()
    {
        if (!battleShipBtn.ClassListContains("selected")) return;

        int goldCost = shipCosts[0];
        int oilCost = shipCosts[1];
        int cubeCost = shipCosts[2];

        // TODO: 檢查資源是否足夠
        // TODO: 扣除資源
        // TODO: 創建戰艦實例
        
        Debug.Log("開始建造戰艦");
        
        // 重置選擇狀態
        battleShipBtn.RemoveFromClassList("selected");
        createShipBtn.SetEnabled(false);
        UpdateCostDisplay(0, 0, 0);
    }
} 