using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// 控制船隻建造面板的行為
/// </summary>
public class ShipCreationPanel : MonoBehaviour
{
    private UIDocument uiDocument;
    private VisualElement panel;
    private Button battleShipBtn;
    private Button createShipBtn;
    private Label goldCostLabel;
    private Label oilCostLabel;
    private Label cubeCostLabel;

    // 戰艦建造成本
    private readonly int[] shipCosts = { 500, 200, 100 }; // 金幣, 石油, 方塊

    [SerializeField] private MapController mapController; // 新增 MapController 引用

    void Awake()
    {
        InitializeUI();
    }

    void OnEnable()
    {
        if (panel == null)
        {
            InitializeUI();
        }
    }

    private void InitializeUI()
    {
        Debug.Log("初始化船隻建造面板...");
        
        // 獲取 UIDocument
        uiDocument = GetComponent<UIDocument>();
        if (uiDocument == null)
        {
            Debug.LogError("找不到 UIDocument 組件！");
            return;
        }

        // 獲取根元素
        var root = uiDocument.rootVisualElement;
        if (root == null)
        {
            Debug.LogError("無法獲取 UI 根元素！");
            return;
        }

        // 獲取面板根元素
        panel = root.Q<VisualElement>("ship-creation-panel");
        if (panel == null)
        {
            Debug.LogError("找不到 ship-creation-panel 元素！");
            return;
        }

        // 獲取UI元素引用
        battleShipBtn = root.Q<Button>("battle-ship-btn");
        createShipBtn = root.Q<Button>("create-ship-btn");
        goldCostLabel = root.Q<Label>("gold-cost");
        oilCostLabel = root.Q<Label>("oil-cost");
        cubeCostLabel = root.Q<Label>("cube-cost");

        // 檢查必要元素
        if (battleShipBtn == null) Debug.LogError("找不到 battle-ship-btn！");
        if (createShipBtn == null) Debug.LogError("找不到 create-ship-btn！");
        if (goldCostLabel == null) Debug.LogError("找不到 gold-cost 標籤！");
        if (oilCostLabel == null) Debug.LogError("找不到 oil-cost 標籤！");
        if (cubeCostLabel == null) Debug.LogError("找不到 cube-cost 標籤！");

        // 註冊按鈕點擊事件
        if (battleShipBtn != null)
        {
            battleShipBtn.clicked += SelectBattleShip;
        }
        if (createShipBtn != null)
        {
            createShipBtn.clicked += CreateShip;
            createShipBtn.SetEnabled(false);
        }

        // 初始化面板狀態
        panel.style.display = DisplayStyle.None;
        UpdateCostDisplay(0, 0, 0);

        Debug.Log("船隻建造面板初始化完成");
    }

    /// <summary>
    /// 顯示面板
    /// </summary>
    public void Show()
    {
        Debug.Log("顯示船隻建造面板");
        if (panel != null)
        {
            panel.style.display = DisplayStyle.Flex;
        }
        else
        {
            Debug.LogError("無法顯示面板：panel 是 null");
        }
    }

    /// <summary>
    /// 隱藏面板
    /// </summary>
    public void Hide()
    {
        Debug.Log("隱藏船隻建造面板");
        if (panel != null)
        {
            panel.style.display = DisplayStyle.None;
            ResetPanel();
        }
        else
        {
            Debug.LogError("無法隱藏面板：panel 是 null");
        }
    }

    /// <summary>
    /// 切換面板顯示狀態
    /// </summary>
    public void Toggle()
    {
        Debug.Log("切換船隻建造面板顯示狀態");
        if (panel != null)
        {
            bool isVisible = panel.style.display == DisplayStyle.Flex;
            Debug.Log($"當前面板狀態：{(isVisible ? "顯示" : "隱藏")}");
            panel.style.display = isVisible ? DisplayStyle.None : DisplayStyle.Flex;
            
            if (isVisible)
            {
                ResetPanel();
            }
        }
        else
        {
            Debug.LogError("無法切換面板：panel 是 null");
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

        // 創建戰艦實例
        InstantiateShip();

        Debug.Log("開始建造戰艦");

        // 重置選擇狀態
        battleShipBtn.RemoveFromClassList("selected");
        createShipBtn.SetEnabled(false);
        UpdateCostDisplay(0, 0, 0);
    }

    /// <summary>
    /// 實例化戰艦
    /// </summary>
    private void InstantiateShip()
    {
        // 假設有一個戰艦的預製件
        GameObject battleShipPrefab = Resources.Load<GameObject>("Prefabs/Ship");
        if (battleShipPrefab == null)
        {
            Debug.LogError("找不到戰艦預製件！");
            return;
        }

        // 使用 MapController 找到 Chinju Tile 附近的最近海洋格子
        if (mapController == null)
        {
            Debug.LogError("MapController 未設置！");
            return;
        }

        Vector3 chinjuTilePosition = mapController.GetChinjuTileWorldPosition();
        if (chinjuTilePosition == Vector3.zero)
        {
            Debug.LogError("找不到 Chinju Tile 的位置！");
            return;
        }

        Vector3 spawnPosition = mapController.FindNearestOceanTile(chinjuTilePosition);
        if (spawnPosition == Vector3.zero)
        {
            Debug.LogError("找不到 Chinju Tile 附近的最近海洋格子！");
            return;
        }

        // 固定 z 座標為 -1
        spawnPosition.z = -1;

        // 在場景中生成戰艦
        GameObject battleShip = Instantiate(battleShipPrefab, spawnPosition, Quaternion.identity);
        if (battleShip != null)
        {
            Debug.Log("戰艦實例化成功！");
        }
        else
        {
            Debug.LogError("戰艦實例化失敗！");
        }
    }
}