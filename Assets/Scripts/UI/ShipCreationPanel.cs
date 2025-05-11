using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// 控制船隻建造面板的行為
/// </summary>
public class ShipCreationPanel : MonoBehaviour
{
    private UIDocument uiDocument;
    private VisualElement root; // 修改為 root
    private Button[] shipTypeBtns = new Button[5]; // 五種船型按鈕
    private Button createShipBtn;
    private IntegerField goldInputField;
    private IntegerField oilInputField;
    private IntegerField cubeInputField;

    // 建議建造成本（用於提升相近資源輸入時的隨機機率）


    [SerializeField] private MapController mapController;
    [SerializeField] private ChinjuUIController chinjuUIController; // 新增

    void Awake()
    {
        // 確保 GameDataController 已初始化
        if (GameDataController.Instance != null && GameDataController.Instance.CurrentGameData == null)
        {
            GameDataController.Instance.CurrentGameData = new GameData();
        }
        InitializeUI();
    }

    void OnEnable()
    {
        // 確保 GameDataController 已初始化
        if (GameDataController.Instance != null && GameDataController.Instance.CurrentGameData == null)
        {
            GameDataController.Instance.CurrentGameData = new GameData();
        }
        if (root == null)
        {
            InitializeUI();
        }
    }

    private void InitializeUI()
    {
        Debug.Log("[ShipCreationPanel] 初始化船隻建造面板...");

        // 獲取 UIDocument
        uiDocument = GetComponent<UIDocument>();
        if (uiDocument == null)
        {
            Debug.LogError("找不到 UIDocument 組件！");
            return;
        }

        // 獲取根元素
        root = uiDocument.rootVisualElement;
        if (root == null)
        {
            Debug.LogError("無法獲取 UI 根元素！");
            return;
        }

        // 獲取面板根元素
        var panel = root.Q<VisualElement>("ship-creation-panel");
        if (panel == null)
        {
            Debug.LogError("找不到 ship-creation-panel 元素！");
            return;
        }

        // 取得五個船型按鈕
        for (int i = 0; i < 5; i++)
        {
            shipTypeBtns[i] = UIHelper.InitializeElement<Button>(root, $"ship-type-btn-{i + 1}");
            int idx = i;
            if (shipTypeBtns[i] != null)
            {
                shipTypeBtns[i].clicked += () => SelectShipType(idx);
            }
        }

        createShipBtn = UIHelper.InitializeElement<Button>(root, "create-ship-btn");
        goldInputField = UIHelper.InitializeElement<IntegerField>(root, "gold-input");
        oilInputField = UIHelper.InitializeElement<IntegerField>(root, "oil-input");
        cubeInputField = UIHelper.InitializeElement<IntegerField>(root, "cube-input");

        // 若找不到則自動建立（方便測試）
        if (goldInputField == null)
        {
            goldInputField = new IntegerField("金幣") { value = 10 };
            goldInputField.name = "gold-input";
            panel.Add(goldInputField);
        }
        if (oilInputField == null)
        {
            oilInputField = new IntegerField("石油") { value = 10 };
            oilInputField.name = "oil-input";
            panel.Add(oilInputField);
        }
        if (cubeInputField == null)
        {
            cubeInputField = new IntegerField("方塊") { value = 1 };
            cubeInputField.name = "cube-input";
            panel.Add(cubeInputField);
        }

        // 註冊輸入變更事件
        goldInputField.RegisterValueChangedCallback(evt => OnResourceInputChanged());
        oilInputField.RegisterValueChangedCallback(evt => OnResourceInputChanged());
        cubeInputField.RegisterValueChangedCallback(evt => OnResourceInputChanged());

        // 檢查必要元素
        if (createShipBtn == null) Debug.LogError("找不到 create-ship-btn！");

        if (createShipBtn != null)
        {
            createShipBtn.clicked += OnBuildButtonClicked;
            createShipBtn.SetEnabled(false);
        }

        // 新增：關閉按鈕
        var closeBtn = root.Q<Button>("close-ship-panel-btn");
        if (closeBtn == null)
        {
            closeBtn = new Button(() => Hide()) { text = "關閉" };
            closeBtn.name = "close-ship-panel-btn";
            closeBtn.style.marginTop = 8;
            root.Add(closeBtn);
        }
        else
        {
            closeBtn.clicked += Hide;
        }

        // 初始化面板狀態
        root.style.display = DisplayStyle.None; // 修改為控制 root
        UpdateCostDisplay(0, 0, 0);

        Debug.Log("[ShipCreationPanel] 船隻建造面板初始化完成");
    }

    void Start()
    {
        // 自動尋找 ChinjuUIController
        if (chinjuUIController == null)
            chinjuUIController = FindFirstObjectByType<ChinjuUIController>();
    }

    private void OnResourceInputChanged()
    {
        int gold = Mathf.Max(0, goldInputField.value);
        int oil = Mathf.Max(0, oilInputField.value);
        int cube = Mathf.Max(0, cubeInputField.value);
        UpdateCostDisplay(gold, oil, cube);

        // 啟用條件：資源大於10/10/1
        bool enable = false;

        if (gold >= 10 && oil >= 10 && cube >= 1)
        {
            // 檢查有沒有可負擭的船型
            var gameData = GameDataController.Instance != null ? GameDataController.Instance.CurrentGameData : null;
            if (gameData != null && gameData.playerData != null)
            {
                for (int i = 0; i < 5; i++)
                {
                    if (gameData.playerData.Gold >= ShipCreationManager.Instance.shipCosts[i, 0] &&
                        gameData.playerData.Oils >= ShipCreationManager.Instance.shipCosts[i, 1] &&
                        gameData.playerData.Cube >= ShipCreationManager.Instance.shipCosts[i, 2])
                    {
                        enable = true;
                        break;
                    }
                }
            }
        }

        createShipBtn.SetEnabled(enable);
    }

    private void SelectShipType(int idx)
    {
        // 取消所有選擇
        for (int i = 0; i < shipTypeBtns.Length; i++)
        {
            shipTypeBtns[i].RemoveFromClassList("selected");
        }
        // 標記選擇
        shipTypeBtns[idx].AddToClassList("selected");

        // 根據船型自動填入建造成本
        goldInputField.value = ShipCreationManager.Instance.shipCosts[idx, 0];
        oilInputField.value = ShipCreationManager.Instance.shipCosts[idx, 1];
        cubeInputField.value = ShipCreationManager.Instance.shipCosts[idx, 2];

        OnResourceInputChanged();
    }

    /// <summary>
    /// 顯示面板
    /// </summary>
    public void Show()
    {
        if (root != null)
        {
            root.style.display = DisplayStyle.Flex;
            Debug.Log($"[WeaponCreatePanelController] 顯示武器創建面板，root.style.display = {root.style.display}");
        }
        else
        {
            Debug.LogError("[WeaponCreatePanelController] 無法顯示武器創建面板，root 為 null");
        }
    }

    /// <summary>
    /// 隱藏面板
    /// </summary>
    public void Hide()
    {
        if (root != null)
        {
            root.style.display = DisplayStyle.None;
            Debug.Log("[WeaponCreatePanelController] 隱藏武器創建面板, root.style.display = " + root.style.display);

        }
        else
        {
            Debug.LogError("[WeaponCreatePanelController] 無法隱藏武器創建面板，root 為 null");
        }
    }

    /// <summary>
    /// 切換面板顯示狀態
    /// </summary>
    public void Toggle()
    {
        Debug.Log("[ShipCreationPanel] 切換船隻建造面板顯示狀態");
        if (root != null)
        {
            bool isVisible = root.style.display == DisplayStyle.Flex; // 修改為檢查根元素
            Debug.Log($"[ShipCreationPanel] 當前面板狀態：{(isVisible ? "顯示" : "隱藏")}");
            root.style.display = isVisible ? DisplayStyle.None : DisplayStyle.Flex; // 修改為控制根元素
            if (isVisible)
            {
                ResetPanel();
            }
        }
        else
        {
            Debug.LogError("[ShipCreationPanel] 無法切換面板：root 是 null");
        }
    }

    /// <summary>
    /// 重置面板狀態
    /// </summary>
    private void ResetPanel()
    {
        // 取消所有選擇
        for (int i = 0; i < shipTypeBtns.Length; i++)
        {
            shipTypeBtns[i].RemoveFromClassList("selected");
        }
        createShipBtn.SetEnabled(false);
        UpdateCostDisplay(0, 0, 0);
    }

    /// <summary>
    /// 更新資源消耗顯示
    /// </summary>
    private void UpdateCostDisplay(int gold, int oil, int cube)
    {
        // 更新輸入欄位的值
        goldInputField.value = gold;
        oilInputField.value = oil;
        cubeInputField.value = cube;
    }

    private void OnBuildButtonClicked()
    {
        Debug.Log("[ShipCreationPanel] 點擊建造按鈕");

        if (ShipCreationManager.Instance == null)
        {
            Debug.LogError("[ShipCreationPanel] ShipCreationManager.Instance 為 null，無法建造船隻");
            return;
        }

        Ship newShip = null;


        newShip = ShipCreationManager.Instance.TryCreateRandomShip(goldInputField.value, oilInputField.value, cubeInputField.value);


        if (newShip != null)
        {
            newShip.AddRandomWeapon(); // 隨機生成武器並分配給新船隻
        }
        else
        {
            Debug.LogError("[ShipCreationPanel] 無法創建船隻，請檢查資源或其他條件");
        }

        ResetPanel();
    }
}