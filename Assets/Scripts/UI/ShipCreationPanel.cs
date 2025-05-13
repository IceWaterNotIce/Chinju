using Unity.VisualScripting;
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

    void Awake()
    {
        PopupManager.Instance.RegisterPopup("ShipCreationPanel", gameObject);
    }

    void OnEnable()
    {
        // 確保 GameDataController 已初始化
        if (GameDataController.Instance != null && GameDataController.Instance.CurrentGameData == null)
        {
            GameDataController.Instance.CurrentGameData = new GameData();
        }
        InitializeUI();
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
            closeBtn = new Button(() => 
            {
               PopupManager.Instance.HidePopup("ShipCreationPanel");
            }) { text = "關閉" };
            closeBtn.name = "close-ship-panel-btn";
            closeBtn.style.marginTop = 8;
            root.Add(closeBtn);
        }
        else
        {
            closeBtn.clicked += () => 
            {
                PopupManager.Instance.HidePopup("ShipCreationPanel");
            };
        }

        // 初始化面板狀態
        PopupManager.Instance.RegisterPopup("ShipCreationPanel", gameObject);
        UpdateCostDisplay(0, 0, 0);

        Debug.Log("[ShipCreationPanel] 船隻建造面板初始化完成");
    }


    private void OnResourceInputChanged()
    {
        int gold = Mathf.Max(0, goldInputField.value);
        int oil = Mathf.Max(0, oilInputField.value);
        int cube = Mathf.Max(0, cubeInputField.value);
        UpdateCostDisplay(gold, oil, cube);

        if (gold >= 10 && oil >= 10 && cube >= 1)
        {
            createShipBtn.SetEnabled(true);
        }
        else
        {
            createShipBtn.SetEnabled(false);
        }
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

        PlayerShip newShip = ShipCreationManager.Instance.TryCreateRandomShip(goldInputField.value, oilInputField.value, cubeInputField.value);


        if (newShip != null)
        {
            newShip.AddRandomWeapon();
        }
        else
        {
            Debug.LogError("[ShipCreationPanel] 無法創建船隻，請檢查資源或其他條件");
        }

        ResetPanel();
    }
}