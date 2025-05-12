using UnityEngine;
using UnityEngine.UIElements;

public class ShipUI : Singleton<ShipUI>
{
    public Ship ship;
    private VisualElement Panel;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private Label lblSpeedFrontFull;
    private Label lblSpeedFrontThreeQuarters;
    private Label lblSpeedFrontHalf;
    private Label lblSpeedFrontQuarter;
    private Label lblSpeedStop;
    private Label lblSpeedBackFull;

    private Label lblRotationLeftFull;
    private Label lblRotationLeftHalf;
    private Label lblRotationStop;
    private Label lblRotationRightHalf;
    private Label lblRotationRightFull;

    private Button btnTriggerMap;

    // 新增：武器列表容器
    private VisualElement weaponListContainer;
    private VisualElement weaponDetailPopup;

    // 新增：武器總覽面板
    private VisualElement weaponsPanel;

    private Label lblLevel; // 新增：顯示等級的 Label
    private Label lblExperience; // 新增：顯示經驗值的 Label

    private const float PopupWidth = 300f; // Constant for popup width
    private const float PopupPadding = 10f; // Constant for popup padding

    private Label lblHealth; // 新增：顯示健康值的 Label
    private Label lblFuel; // 新增：顯示燃料的 Label

    private Button btnCancelFollow; // 新增取消跟隨按鈕

    private Button startDrawButton;

    private bool canDraw = false;
    private bool isDrawing = false;
    private Vector2 startPos;
    private VisualElement currentRect;

    private Button btnCloseUI; // 新增關閉 UI 的按鈕

    void Start()
    {
        var uiDoc = GetComponent<UIDocument>();
        if (uiDoc == null)
        {
            LogError("無法找到 UIDocument 組件！");
            return;
        }

        var root = uiDoc.rootVisualElement;
        if (root == null)
        {
            LogError("無法初始化 root 元素！");
            return;
        }

        Panel = UIHelper.InitializeElement<VisualElement>(root, "Panel");
        if (Panel == null)
        {
            LogError("找不到名為 'Panel' 的 VisualElement！");
            return;
        }

        InitializeSpeedLabels();
        InitializeRotationLabels();
        InitializeWeaponListContainer();
        InitializeLevelAndExperienceLabels();
        InitializeHealthAndFuelLabels();
        InitializeCancelFollowButton();
        InitializeDrawButton();
        InitializeCloseUIButton();
        RegisterPointerEvents();
    }

    public void Initial(Ship s)
    {
        ship = s;

        var uiDoc = GetComponent<UIDocument>() ?? gameObject.AddComponent<UIDocument>();
        if (uiDoc == null)
        {
            LogError("UIDocument 無法初始化！");
            return;
        }

        uiDoc.panelSettings = Resources.Load<PanelSettings>("UI/PanelSettings");
        if (uiDoc.panelSettings == null)
        {
            LogError("無法加載 PanelSettings 資源！");
            return;
        }

        uiDoc.visualTreeAsset = Resources.Load<VisualTreeAsset>("UI/ShipUI");
        if (uiDoc.visualTreeAsset == null)
        {
            LogError("無法加載 ShipUI 資源！");
            return;
        }


        var root = uiDoc.rootVisualElement;
        if (root == null)
        {
            LogError("UIDocument 的 rootVisualElement 為 null！");
            return;
        }

        var styleSheet = Resources.Load<StyleSheet>("UI/ShipUI");
        if (styleSheet != null)
        {
            root.styleSheets.Add(styleSheet);
        }
        else
        {
            LogError("無法加載 ShipUI 的樣式表！");
            return;
        }

        Panel = root.Q<VisualElement>("Panel");
        if (Panel == null)
        {
            LogError("找不到名為 'Panel' 的 VisualElement！");
            return;
        }

        InitializeSpeedLabels();
        InitializeRotationLabels();
        InitializeWeaponListContainer();
        InitializeLevelAndExperienceLabels();
        InitializeHealthAndFuelLabels();
        UpdateLevelAndExperienceUI();
        UpdateHealth(ship.Health, ship.MaxHealth);
        UpdateFuel(ship.CurrentFuel, ship.MaxFuel);

        // 訂閱事件
        ship.OnHealthChanged += health => UpdateHealth(health, ship.MaxHealth);
        ship.OnFuelChanged += fuel => UpdateFuel(fuel, ship.MaxFuel);

        SetUIPosition();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateLevelAndExperienceUI(); // 每幀更新等級和經驗值的顯示
    }

    void SpeedControll(float percentage)
    {
        if (ship == null)
        {
            LogError("ship speed control fail. Ship is not set.");
            return;
        }
        float MaxSpeed = ship.MaxSpeed;
        float TargetSpeed = MaxSpeed * percentage;
        ship.TargetSpeed = TargetSpeed;
        Debug.Log("Speed: " + TargetSpeed);


    }

    void RotationControll(float percentage)
    {
        if (ship == null)
        {
            LogError("ship rotation control fail. Ship is not set.");
            return;
        }
        float MaxRotationSpeed = ship.MaxRotationSpeed;
        float TargetRotationSpeed = MaxRotationSpeed * percentage;
        ship.TargetRotationSpeed = TargetRotationSpeed;
        Debug.Log("Rotation Speed: " + TargetRotationSpeed);

    }

    // 新增：顯示武器詳細資訊
    private void ShowWeaponDetail(Weapon weapon)
    {
        if (weaponDetailPopup != null)
        {
            weaponDetailPopup.RemoveFromHierarchy();
        }
        weaponDetailPopup = new VisualElement();
        weaponDetailPopup.AddToClassList("weapon-detail-popup");

        Label title = new Label("武器資訊");
        title.AddToClassList("title");
        weaponDetailPopup.Add(title);

        weaponDetailPopup.Add(new Label($"最大攻擊距離: {weapon.MaxAttackDistance}"));
        weaponDetailPopup.Add(new Label($"最小攻擊距離: {weapon.MinAttackDistance}"));
        weaponDetailPopup.Add(new Label($"攻擊冷卻: {weapon.CooldownTime} 秒"));
        weaponDetailPopup.Add(new Label($"彈藥預製體: {(weapon.AmmoPrefab != null ? weapon.AmmoPrefab.name : "無")}"));

        Button closeBtn = new Button(() => weaponDetailPopup.RemoveFromHierarchy()) { text = "關閉" };
        weaponDetailPopup.Add(closeBtn);

        Panel.Add(weaponDetailPopup);
    }

    // 新增：武器總覽面板
    public void ShowWeaponsPanel()
    {
        if (weaponsPanel != null)
        {
            weaponsPanel.RemoveFromHierarchy();
        }

        weaponsPanel = new VisualElement();
        weaponsPanel.AddToClassList("weapons-panel");

        Label title = new Label("武器總覽");
        title.AddToClassList("title");
        weaponsPanel.Add(title);

        int weaponSlotCount = ship.IntWeaponLimit;
        for (int i = 0; i < weaponSlotCount; i++)
        {
            Weapon weapon = (ship.weapons != null && i < ship.weapons.Count) ? ship.weapons[i] : null;
            VisualElement row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;

            VisualElement icon = new VisualElement();
            icon.AddToClassList("weapon-icon");

            if (weapon != null)
            {
                icon.style.backgroundColor = new Color(0.8f, 0.8f, 0.2f, 1f);
                icon.tooltip = $"武器{i + 1}";
                int weaponIndex = i;
                icon.RegisterCallback<ClickEvent>(ev => ShowWeaponDetail(ship.weapons[weaponIndex]));
            }
            else
            {
                icon.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.3f);
                icon.tooltip = $"空武器槽{i + 1}";
            }
            row.Add(icon);

            Label label = new Label(weapon != null ? $"武器{i + 1}" : $"空武器槽{i + 1}");
            row.Add(label);

            weaponsPanel.Add(row);
        }

        Button closeBtn = new Button(() => weaponsPanel.RemoveFromHierarchy()) { text = "關閉" };
        weaponsPanel.Add(closeBtn);

        Panel.Add(weaponsPanel);
    }

    // 新增：顯示武器選擇面板
    private void ShowWeaponSelectionPanel(int slotIndex)
    {
        if (weaponDetailPopup != null)
        {
            weaponDetailPopup.RemoveFromHierarchy();
        }

        weaponDetailPopup = new VisualElement();
        weaponDetailPopup.AddToClassList("weapon-selection-popup");

        // Dynamically calculate position
        Vector2 screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);
        weaponDetailPopup.style.left = screenCenter.x - PopupWidth / 2;
        weaponDetailPopup.style.top = screenCenter.y - PopupWidth / 2;

        Label title = new Label("選擇武器");
        title.AddToClassList("title");
        weaponDetailPopup.Add(title);

        // 從玩家資料中獲取武器清單
        var playerData = GameDataController.Instance.CurrentGameData.playerData;
        if (playerData != null && playerData.Weapons != null)
        {
            foreach (var weaponData in playerData.Weapons)
            {
                Button weaponButton = new Button(() =>
                {
                    // 將 GameData.WeaponData 轉換為 Weapon
                    Weapon weapon = new Weapon
                    {
                        Name = weaponData.Name,
                        Damage = weaponData.Damage,
                        MaxAttackDistance = weaponData.MaxAttackDistance,
                        MinAttackDistance = weaponData.MinAttackDistance,
                        AttackSpeed = weaponData.AttackSpeed,
                        CooldownTime = weaponData.CooldownTime
                    };

                    ship.weapons[slotIndex] = weapon; // 插入武器到指定槽位
                    weaponDetailPopup.RemoveFromHierarchy(); // 關閉選擇面板
                    RefreshWeaponList(); // 更新武器列表
                })
                {
                    text = weaponData.Name
                };
                weaponButton.style.marginTop = 5;
                weaponDetailPopup.Add(weaponButton);
            }
        }
        else
        {
            Label noWeaponLabel = new Label("目前沒有可用的武器。");
            noWeaponLabel.style.marginTop = 10;
            weaponDetailPopup.Add(noWeaponLabel);
        }

        Button closeBtn = new Button(() => weaponDetailPopup.RemoveFromHierarchy()) { text = "關閉" };
        closeBtn.style.marginTop = PopupPadding;
        weaponDetailPopup.Add(closeBtn);

        Panel.Add(weaponDetailPopup);
    }

    // 新增：刷新武器列表的方法
    private void RefreshWeaponList()
    {
        if (ship == null || ship.weapons == null)
        {
            LogError("Ship or weapons list is null. Cannot refresh weapon list.");
            return;
        }

        weaponListContainer.Clear();

        int weaponSlotCount = ship.IntWeaponLimit;
        for (int i = 0; i < weaponSlotCount; i++)
        {
            Weapon weapon = (i < ship.weapons.Count) ? ship.weapons[i] : null;
            VisualElement weaponIcon = CreateWeaponIcon(weapon, i);
            weaponListContainer.Add(weaponIcon);
        }
    }

    private VisualElement CreateWeaponIcon(Weapon weapon, int index)
    {
        VisualElement weaponIcon = new VisualElement();
        weaponIcon.AddToClassList("weapon-icon");

        if (weapon != null)
        {
            weaponIcon.style.backgroundColor = new Color(0.8f, 0.8f, 0.2f, 1f);
            weaponIcon.tooltip = $"武器{index + 1}";
            weaponIcon.RegisterCallback<ClickEvent>(ev => ShowWeaponDetail(weapon));
        }
        else
        {
            weaponIcon.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.3f);
            weaponIcon.tooltip = $"空武器槽{index + 1}";
            weaponIcon.RegisterCallback<ClickEvent>(ev => ShowWeaponSelectionPanel(index));
        }

        return weaponIcon;
    }

    private void LogError(string message)
    {
        // Replace Debug.LogError with centralized logging
        Debug.LogError($"[ShipUI] {message}");
    }

    private void UpdateLevelAndExperienceUI()
    {
        if (ship != null)
        {
            lblLevel.text = $"等級: {ship.Level}";
            lblExperience.text = $"經驗值: {ship.Experience}/{ship.Level * 10}";
        }
    }

    private void UpdateHealth(float currentHealth, float maxHealth)
    {
        if (lblHealth != null)
        {
            lblHealth.text = $"健康值: {Mathf.RoundToInt(currentHealth)}/{Mathf.RoundToInt(maxHealth)}";
        }
    }

    private void UpdateFuel(float currentFuel, float maxFuel)
    {
        if (lblFuel != null)
        {
            lblFuel.text = $"燃料: {Mathf.RoundToInt(currentFuel)}/{Mathf.RoundToInt(maxFuel)}";
        }
    }

    private void InitializeSpeedLabels()
    {
        lblSpeedFrontFull = InitializeSpeedLabel("lblSpeedFrontFull", 1.0f);
        lblSpeedFrontThreeQuarters = InitializeSpeedLabel("lblSpeedFrontThreeQuarters", 0.75f);
        lblSpeedFrontHalf = InitializeSpeedLabel("lblSpeedFrontHalf", 0.5f);
        lblSpeedFrontQuarter = InitializeSpeedLabel("lblSpeedFrontQuarter", 0.25f);
        lblSpeedStop = InitializeSpeedLabel("lblSpeedStop", 0.0f);
        lblSpeedBackFull = InitializeSpeedLabel("lblSpeedBackFull", -0.25f);
    }

    private Label InitializeSpeedLabel(string name, float speedPercentage)
    {
        var label = UIHelper.InitializeElement<Label>(Panel, name);
        label.RegisterCallback<ClickEvent>(ev => SpeedControll(speedPercentage));
        return label;
    }

    private void InitializeRotationLabels()
    {
        lblRotationLeftFull = InitializeRotationLabel("lblRotationLeftFull", 1.0f);
        lblRotationLeftHalf = InitializeRotationLabel("lblRotationLeftHalf", 0.5f);
        lblRotationStop = InitializeRotationLabel("lblRotationStop", 0.0f);
        lblRotationRightHalf = InitializeRotationLabel("lblRotationRightHalf", -0.5f);
        lblRotationRightFull = InitializeRotationLabel("lblRotationRightFull", -1.0f);
    }

    private Label InitializeRotationLabel(string name, float rotationPercentage)
    {
        var label = UIHelper.InitializeElement<Label>(Panel, name);
        label.RegisterCallback<ClickEvent>(ev => RotationControll(rotationPercentage));
        return label;
    }

    private void InitializeLevelAndExperienceLabels()
    {
        lblLevel = InitializeLabel("lblLevel", 10);
        lblExperience = InitializeLabel("lblExperience", 5);
    }

    private void InitializeHealthAndFuelLabels()
    {
        lblHealth = InitializeLabel("lblHealth", 10);
        lblFuel = InitializeLabel("lblFuel", 5);
    }

    private Label InitializeLabel(string name, int marginTop)
    {
        var label = Panel.Q<Label>(name);
        if (label == null)
        {
            label = new Label();
            label.name = name;
            label.style.marginTop = marginTop;
            label.style.unityTextAlign = TextAnchor.MiddleLeft;
            Panel.Add(label);
        }
        return label;
    }

    private void InitializeWeaponListContainer()
    {
        weaponListContainer = Panel.Q<VisualElement>("weaponListContainer") ?? new VisualElement
        {
            name = "weaponListContainer",
            style =
            {
                flexDirection = FlexDirection.Row,
                marginTop = 10
            }
        };
        Panel.Add(weaponListContainer);
        weaponListContainer.Clear();
    }

    private void InitializeCancelFollowButton()
    {
        btnCancelFollow = UIHelper.InitializeElement<Button>(Panel, "btnCancelFollow");
        if (btnCancelFollow != null)
        {
            btnCancelFollow.clicked += () =>
            {
                var cameraController = Camera.main?.GetComponent<CameraBound2D>();
                if (cameraController != null)
                {
                    cameraController.StopFollowing();
                    Debug.Log("[ShipUI] 已取消攝影機跟隨。");
                }
            };
        }
    }

    private void InitializeDrawButton()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        startDrawButton = root.Q<Button>("StartDrawButton");
        if (startDrawButton != null)
        {
            startDrawButton.clicked += () =>
            {
                EnableDrawing();
            };
        }
        else
        {
            LogError("找不到名為 'StartDrawButton' 的按鈕！");
        }
    }

    private void InitializeCloseUIButton()
    {
        btnCloseUI = UIHelper.InitializeElement<Button>(Panel, "btnCloseUI");
        if (btnCloseUI != null)
        {
            btnCloseUI.clicked += () =>
            {
                Destroy(gameObject); // 銷毀 ShipUI
                Debug.Log("[ShipUI] Ship UI 已關閉。");
            };
        }
        else
        {
            LogError("找不到名為 'btnCloseUI' 的按鈕！");
        }
    }

    private void SetUIPosition()
    {
        if (Camera.main == null)
        {
            LogError("Camera.main 為 null，無法設定 UI 位置！");
            return;
        }

        Vector2 shipScreenPosition = Camera.main.WorldToScreenPoint(ship.transform.position);
        Debug.Log("Ship Screen Position: " + shipScreenPosition);
        Panel.style.left = shipScreenPosition.x;
        Panel.style.top = shipScreenPosition.y;
    }

    private void RegisterPointerEvents()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        root.RegisterCallback<PointerDownEvent>(OnPointerDown); // 修正為 PointerDownEvent
        root.RegisterCallback<PointerMoveEvent>(OnPointerMove); // 修正為 PointerMoveEvent
        root.RegisterCallback<PointerUpEvent>(OnPointerUp);     // 修正為 PointerUpEvent
    }

    private void EnableDrawing()
    {
        canDraw = true; // 啟用繪製功能
        Debug.Log("[ShipUI] 繪製功能已啟用");
    }

    private void OnPointerDown(PointerDownEvent evt)
    {
        if (!canDraw || evt.button != 0) return; // 檢查是否允許繪製
        startPos = evt.localPosition;
        currentRect = new VisualElement();
        currentRect.AddToClassList("rect"); // 套用矩形樣式
        currentRect.style.position = Position.Absolute;
        currentRect.style.left = startPos.x;
        currentRect.style.top = startPos.y;
        var root = GetComponent<UIDocument>().rootVisualElement;
        root.Add(currentRect);
        isDrawing = true;
        Debug.Log("[ShipUI] 開始繪製矩形");
    }

    private void OnPointerMove(PointerMoveEvent evt)
    {
        if (isDrawing && currentRect != null)
        {
            Vector2 mousePos = evt.localPosition;
            Vector2 size = mousePos - startPos;

            // 設定矩形大小和位置
            currentRect.style.width = Mathf.Abs(size.x);
            currentRect.style.height = Mathf.Abs(size.y);
            currentRect.style.left = Mathf.Min(startPos.x, mousePos.x);
            currentRect.style.top = Mathf.Min(startPos.y, mousePos.y);
        }
    }

    private void OnPointerUp(PointerUpEvent evt)
    {
        if (evt.button == 0 && isDrawing) // 左鍵
        {
            isDrawing = false;
            currentRect = null; // 重置 currentRect 狀態
            canDraw = false; // 繪製完成後禁用繪製功能
            Debug.Log("[ShipUI] 繪製結束");
            Debug.Log($"[ShipUI] 繪製的矩形位置: {startPos} 到 {evt.localPosition}");
        }
    }

    private void OnDestroy()
    {
        Debug.Log("[ShipUI] 銷毀 ShipUI");
        //Debug where call this destroy
        Debug.Log(new System.Diagnostics.StackTrace().ToString());
    }
}
