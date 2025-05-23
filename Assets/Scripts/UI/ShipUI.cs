using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Events; // 新增
using System.Collections.Generic;
using System; // <--- 加入這行

public class ShipUI : Singleton<ShipUI>
{
    #region Fields
    public PlayerShip ship;
    private VisualElement UIPanel;
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

    private VisualElement savedRectElement; // 保存的矩形 UI 元素

    private Button btnToggleCombatMode; // 新增切換戰鬥模式的按鈕

    private Button btnFormFleet; // 新增：形成船隊的按鈕
    private bool isSelectingShipForLine = false; // 狀態標誌，用於選擇船隻

    private Button btnFleetCombatMode; // 新增：編輯船隊戰鬥模式按鈕
    private VisualElement rectContainer; // <-- 移到這裡
    private VisualElement healthBar; // 新增
    private VisualElement fuelBar;   // 新增
    private VisualElement expBar;    // 新增

    private Button btnDrawWaypoint; // 新增：切換繪製 waypoint 模式按鈕
    private bool IsDrawingWaypoint = false; // 新增：繪製 waypoint 模式狀態
    private List<VisualElement> waypointMarkers = new List<VisualElement>(); // 新增：waypoint 標記列表
    private Label lblName; // 新增：顯示船名的 Label
    #endregion

    #region Unity Methods
    void Start()
    {
        InitializeUI();
    }

    void UpdateExperience(float exp,int level)
    {
        if (lblExperience != null)
        {
            lblExperience.text = $"經驗值: {exp}/{level*10}";
        }
        if (expBar != null)
        {
            expBar.style.width = new StyleLength(new Length(exp / level * 100, LengthUnit.Percent));
        }
    }

    void UpdateLevel(int level)
    {
        if (lblLevel != null)
        {
            lblLevel.text = $"等級: {level}";
        }
    }

    void UpdateCombatMode(bool isInCombat)
    {
        if (btnToggleCombatMode != null)
        {
            // 改為顯示枚舉狀態
            btnToggleCombatMode.text = $"戰鬥模式: {ship.Mode}";
        }
    }

    public void Initial(PlayerShip s)
    {
        ship = s;
        InitializeUI();

        UpdateHealth(ship.Health, ship.MaxHealth);
        UpdateFuel(ship.CurrentFuel, ship.MaxFuel);

        // 訂閱事件
        ship.OnHealthChanged += health => UpdateHealth(health, ship.MaxHealth);
        ship.OnFuelChanged += fuel => UpdateFuel(fuel, ship.MaxFuel);
        ship.gameObject.GetComponent<Warship>().OnExperienceChanged += exp => UpdateExperience(exp, ship.gameObject.GetComponent<Warship>().Level);
        ship.gameObject.GetComponent<Warship>().OnLevelChanged += level => UpdateLevel(level);

        // 新增：訂閱等級、經驗值、戰鬥模式變化事件
        ship.OnCombatModeChanged.AddListener(isInCombat => UpdateCombatMode(isInCombat));

        SetUIPosition();

        // 如果船隻有保存的矩形區域，繪製矩形 UI
        if (ship.NavigationArea.width > 0 && ship.NavigationArea.height > 0)
        {
            DrawSavedRect(ship.NavigationArea);
        }

        // 修正：根據 CombatMode 狀態設定按鈕文字
        if (btnToggleCombatMode != null)
        {
            btnToggleCombatMode.text = $"戰鬥模式: {ship.Mode}";
        }

        // 新增：顯示船名
        UpdateShipName();
    }

    void Update()
    {
        SetUIPosition(); // 每幀更新 UI 位置
        SetRectPosition(); // 每幀更新矩形位置
        UpdateShipName(); // 每幀同步船名
    }
    #endregion

    #region Speed & Rotation Control
    void SpeedControll(float percentage)
    {
        if (ship == null)
        {
            LogError("ship speed control fail. Ship is not set.");
            return;
        }

        ClearRectAndData();

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
        ClearRectAndData();

        float MaxRotationSpeed = ship.MaxRotationSpeed;
        float TargetRotationSpeed = MaxRotationSpeed * percentage;
        ship.TargetRotationSpeed = TargetRotationSpeed;
        Debug.Log("Rotation Speed: " + TargetRotationSpeed);

    }
    #endregion

    #region Rect Drawing
    private void ClearRectAndData()
    {
        if (rectContainer != null)
        {
            rectContainer.Clear(); // 清除所有矩形
        }

        if (ship != null)
        {
            ship.NavigationArea = new Rect(); // 重置矩形數據
            Debug.Log("[ShipUI] 矩形和數據已清除");
        }
    }

    private void EnableDrawing()
    {
        canDraw = true; // 啟用繪製功能
        Debug.Log("[ShipUI] 繪製功能已啟用");
    }

    private void OnPointerDown(PointerDownEvent evt)
    {
        Debug.Log("[ShipUI] PointerDownEvent");
        if (rectContainer == null)
        {
            rectContainer = GetComponent<UIDocument>().rootVisualElement.Q<VisualElement>("rectContainer");
            if (rectContainer == null)
            {
                Debug.LogError("[ShipUI] 找不到名為 'rectContainer' 的 VisualElement！");
                return;
            }
        }
        if (!canDraw || evt.button != 0) return; // 檢查是否允許繪製
        startPos = evt.localPosition;

        // Adjust start position relative to the rectContainer
        Vector2 containerPosition = rectContainer.worldBound.position;
        startPos -= containerPosition;

        currentRect = new VisualElement();
        currentRect.AddToClassList("rect"); // 套用矩形樣式
        currentRect.style.position = Position.Absolute;
        currentRect.style.left = startPos.x;
        currentRect.style.top = startPos.y;
        rectContainer.Add(currentRect); // Add to rectContainer instead of Panel
        isDrawing = true;
        Debug.Log("[ShipUI] 開始繪製矩形");
    }

    private void OnPointerMove(PointerMoveEvent evt)
    {
        if (isDrawing && currentRect != null)
        {
            Vector2 mousePos = evt.localPosition;

            // Adjust mouse position relative to the rectContainer
            Vector2 containerPosition = rectContainer.worldBound.position;
            mousePos -= containerPosition;

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
            canDraw = false; // 繪製完成後禁用繪製功能

            if (currentRect != null)
            {
                // 計算矩形區域
                Rect rect = new Rect(
                    Mathf.Min(startPos.x, evt.localPosition.x),
                    Mathf.Min(startPos.y, evt.localPosition.y),
                    Mathf.Abs(evt.localPosition.x - startPos.x),
                    Mathf.Abs(evt.localPosition.y - startPos.y)
                );

                // 將屏幕坐標轉換為世界空間坐標
                Vector3 screenToWorldMin = Camera.main.ScreenToWorldPoint(new Vector3(rect.xMin, Screen.height - rect.yMax, 0));
                Vector3 screenToWorldMax = Camera.main.ScreenToWorldPoint(new Vector3(rect.xMax, Screen.height - rect.yMin, 0));

                Rect worldRect = new Rect(
                    screenToWorldMin.x,
                    screenToWorldMin.y,
                    screenToWorldMax.x - screenToWorldMin.x,
                    screenToWorldMax.y - screenToWorldMin.y
                );

                // 保存矩形區域到船隻數據
                if (ship != null)
                {
                    ship.NavigationArea = worldRect;
                    Debug.Log($"[ShipUI] 矩形區域已保存到船隻: {worldRect}");
                }

                currentRect = null; // 重置 currentRect 狀態
                //delete all rects
                rectContainer.Clear();
            }

            Debug.Log("[ShipUI] 繪製結束");
        }
    }

    private void DrawSavedRect(Rect rect)
    {
        if (rect == Rect.zero) return; // 如果矩形為零，則不繪製
        var rectContainer = GetComponent<UIDocument>().rootVisualElement.Q<VisualElement>("rectContainer");

        if (savedRectElement != null)
        {
            savedRectElement.RemoveFromHierarchy();
        }

        savedRectElement = new VisualElement();
        savedRectElement.AddToClassList("rect"); // 套用矩形樣式
        savedRectElement.style.position = Position.Absolute;

        rectContainer.Add(savedRectElement);
        UpdateSavedRectPosition(rect); // 初始化位置
        Debug.Log($"[ShipUI] 繪製保存的矩形區域: {rect}");
    }

    private void UpdateSavedRectPosition(Rect rect)
    {
        if (savedRectElement == null) return;

        // 取世界座標的四個角
        Vector3 worldA = new Vector3(rect.xMin, rect.yMin, 0);
        Vector3 worldB = new Vector3(rect.xMax, rect.yMax, 0);

        // 轉螢幕座標
        Vector2 screenA = Camera.main.WorldToScreenPoint(worldA);
        Vector2 screenB = Camera.main.WorldToScreenPoint(worldB);

        // 取螢幕座標最小最大，確保方向正確
        float left = Mathf.Min(screenA.x, screenB.x);
        float right = Mathf.Max(screenA.x, screenB.x);
        float bottom = Mathf.Min(screenA.y, screenB.y);
        float top = Mathf.Max(screenA.y, screenB.y);

        // UI Toolkit Y 軸反向
        float uiLeft = left;
        float uiTop = Screen.height - top;
        float width = right - left;
        float height = top - bottom;

        // 綁定左上角
        savedRectElement.style.left = uiLeft;
        savedRectElement.style.top = uiTop;
        savedRectElement.style.width = Mathf.Abs(width);
        savedRectElement.style.height = Mathf.Abs(height);
    }
    #endregion

    #region Weapon UI
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
        weaponDetailPopup.Add(new Label($"彈藥預製體: {(weapon.AmmoPrefab != null ? weapon.AmmoPrefab.name : "無")}"));

        Button closeBtn = new Button(() => weaponDetailPopup.RemoveFromHierarchy()) { text = "關閉" };
        weaponDetailPopup.Add(closeBtn);

        UIPanel.Add(weaponDetailPopup);
    }

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

        int weaponSlotCount = ship.WeaponLimit;
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

        UIPanel.Add(weaponsPanel);
    }

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
                        AttackSpeed = weaponData.AttackSpeed,
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

        UIPanel.Add(weaponDetailPopup);
    }

    private void RefreshWeaponList()
    {
        if (ship == null || ship.weapons == null)
        {
            LogError("Ship or weapons list is null. Cannot refresh weapon list.");
            return;
        }

        weaponListContainer.Clear();

        int weaponSlotCount = ship.WeaponLimit;
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
    #endregion

    #region UI Label/Element Initialization
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
        var label = UIHelper.InitializeElement<Label>(UIPanel, name);
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
        var label = UIHelper.InitializeElement<Label>(UIPanel, name);
        label.RegisterCallback<ClickEvent>(ev => RotationControll(rotationPercentage));
        return label;
    }

    private void InitializeLevelAndExperienceLabels()
    {
        // 確保只初始化一次，不重複建立
        lblLevel = UIHelper.InitializeElement<Label>(UIPanel, "lblLevel");
        lblExperience = UIHelper.InitializeElement<Label>(UIPanel, "lblExperience");
    }

    private void InitializeHealthAndFuelLabels()
    {
        // 刪除/註解掉 Slider 相關初始化
        // sliderHealth = InitializeSlider("sliderHealth", 100, 10);
        // sliderFuel = InitializeSlider("sliderFuel", 100, 5);
        // sliderExperience = InitializeSlider("sliderExperience", 10, 5);

        // 新增：取得 progress bar VisualElement
        healthBar = UIHelper.InitializeElement<VisualElement>(UIPanel, "healthBar");
        fuelBar = UIHelper.InitializeElement<VisualElement>(UIPanel, "fuelBar");
        expBar = UIHelper.InitializeElement<VisualElement>(UIPanel, "expBar");
    }

    private Slider InitializeSlider(string name, float maxValue, int marginTop)
    {
        var slider = UIPanel.Q<Slider>(name);
        if (slider == null)
        {
            slider = new Slider(0, maxValue);
            slider.name = name;
            slider.style.marginTop = marginTop;
            slider.style.width = 180;
            slider.style.height = 18;
            slider.style.unityTextAlign = TextAnchor.MiddleLeft;
            UIPanel.Add(slider);
        }
        slider.highValue = maxValue;
        slider.value = 0;
        slider.SetEnabled(false); // 只顯示，不允許用戶操作
        return slider;
    }

    private Label InitializeLabel(string name, int marginTop)
    {
        var label = UIPanel.Q<Label>(name);
        if (label == null)
        {
            label = new Label();
            label.name = name;
            label.style.marginTop = marginTop;
            label.style.unityTextAlign = TextAnchor.MiddleLeft;
            UIPanel.Add(label);
        }
        return label;
    }

    private void InitializeWeaponListContainer()
    {
        weaponListContainer = UIPanel.Q<VisualElement>("weaponListContainer") ?? new VisualElement
        {
            name = "weaponListContainer",
            style =
            {
                flexDirection = FlexDirection.Row,
                marginTop = 10
            }
        };
        UIPanel.Add(weaponListContainer);
        weaponListContainer.Clear();
    }

    private void InitializeCancelFollowButton()
    {
        btnCancelFollow = UIHelper.InitializeElement<Button>(UIPanel, "btnCancelFollow");
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
                if (rectContainer != null && rectContainer.childCount > 0)
                {

                    // Clear existing rectangles and reset PlayerShip data
                    ClearRectAndData();
                    startDrawButton.text = "Start Draw"; // Update button text
                }
                else
                {
                    // Enable drawing mode
                    EnableDrawing();
                    startDrawButton.text = "Clear Rect"; // Update button text
                }
            };
        }
        else
        {
            LogError("找不到名為 'StartDrawButton' 的按鈕！");
        }
    }

    private void InitializeCloseUIButton()
    {
        btnCloseUI = UIHelper.InitializeElement<Button>(UIPanel, "btnCloseUI");
        if (btnCloseUI != null)
        {
            btnCloseUI.clicked += () =>
            {
                Destroy(gameObject); // 銷毀 ShipUI
                Debug.Log("[ShipUI] Ship UI 已關閉。");
            };
        }
    }

    private void InitializeToggleCombatModeButton()
    {
        btnToggleCombatMode = UIHelper.InitializeElement<Button>(UIPanel, "btnToggleCombatMode");
        if (btnToggleCombatMode != null)
        {
            btnToggleCombatMode.clicked += () =>
            {
                if (ship != null)
                {
                    // 切換枚舉狀態
                    var mode = ship.Mode;
                    mode = (CombatMode)(((int)mode + 1) % Enum.GetValues(typeof(CombatMode)).Length);
                    ship.Mode = mode;
                    Debug.Log($"[ShipUI] 戰鬥模式切換為: {mode}");
                    btnToggleCombatMode.text = $"戰鬥模式: {mode}";
                }
            };
        }
    }

    private void InitializeFormFleetButton()
    {
        btnFormFleet = UIHelper.InitializeElement<Button>(UIPanel, "btnFormFleet");
        if (btnFormFleet != null)
        {
            btnFormFleet.clicked += () =>
            {
                isSelectingShipForLine = true; // 啟用選擇船隻模式
                Debug.Log("[ShipUI] 選擇船隻以形成船隊模式啟用");
            };
        }
    }

    private void InitializeFleetCombatModeButton()
    {
        // 僅當船隻在 fleet 中才顯示
        if (ship != null && (ship.IsFollower || ship.LeaderShip != null))
        {
            btnFleetCombatMode = UIPanel.Q<Button>("btnFleetCombatMode");
            if (btnFleetCombatMode == null)
            {
                btnFleetCombatMode = new Button();
                btnFleetCombatMode.name = "btnFleetCombatMode";
                btnFleetCombatMode.text = "編輯船隊戰鬥模式";
                btnFleetCombatMode.style.marginTop = 10;
                UIPanel.Add(btnFleetCombatMode);
            }
            btnFleetCombatMode.clicked += () =>
            {
                // 找到 fleet leader
                PlayerShip leader = ship.LeaderShip != null ? ship.LeaderShip : ship;
                Fleet fleet = leader.GetComponent<Fleet>();
                if (fleet != null && fleet.followers != null)
                {
                    // 統一切換到下一個模式
                    var currentMode = leader.Mode;
                    var nextMode = (CombatMode)(((int)currentMode + 1) % Enum.GetValues(typeof(CombatMode)).Length);
                    foreach (var follower in fleet.followers)
                    {
                        PlayerShip ps = follower as PlayerShip;
                        if (ps != null)
                        {
                            ps.Mode = nextMode;
                        }
                    }
                    leader.Mode = nextMode;
                    Debug.Log($"[ShipUI] 已將船隊所有船隻戰鬥模式設為: {nextMode}");
                    if (btnToggleCombatMode != null)
                        btnToggleCombatMode.text = $"戰鬥模式: {nextMode}";
                }
            };
        }
        else
        {
            // 若不在 fleet，移除按鈕
            var existBtn = UIPanel.Q<Button>("btnFleetCombatMode");
            if (existBtn != null)
                existBtn.RemoveFromHierarchy();
        }
    }

    private void InitializeDrawWaypointButton()
    {
        btnDrawWaypoint = UIHelper.InitializeElement<Button>(UIPanel, "btnDrawWaypoint");
        if (btnDrawWaypoint == null)
        {
            btnDrawWaypoint = new Button() { name = "btnDrawWaypoint", text = "繪製航點" };
            UIPanel.Add(btnDrawWaypoint);
        }
        btnDrawWaypoint.clicked += ToggleDrawWaypointMode;
        UpdateDrawWaypointButtonState();
    }

    private void UpdateDrawWaypointButtonState()
    {
        bool canDraw = true;
        if (ship != null && ship.transform.parent != null)
        {
            Fleet fleet = ship.transform.parent.GetComponent<Fleet>();
            if (fleet != null && fleet.followers.Count > 0 && fleet.followers[0] != ship)
            {
                canDraw = false;
            }
        }
        btnDrawWaypoint.SetEnabled(canDraw);
        btnDrawWaypoint.text = IsDrawingWaypoint ? "結束繪製航點" : "繪製航點";
    }

    private void ToggleDrawWaypointMode()
    {
        IsDrawingWaypoint = !IsDrawingWaypoint;
        btnDrawWaypoint.text = IsDrawingWaypoint ? "結束繪製航點" : "繪製航點";
        if (!IsDrawingWaypoint)
        {
            ClearWaypointMarkers();
        }
    }

    private void ClearWaypointMarkers()
    {
        foreach (var marker in waypointMarkers)
        {
            marker.RemoveFromHierarchy();
        }
        waypointMarkers.Clear();
        ship?.ClearWaypoints();
    }
    #endregion

    #region UI Update & Position


    private void UpdateHealth(float currentHealth, float maxHealth)
    {
        if (lblHealth != null)
        {
            lblHealth.text = $"{Mathf.RoundToInt(currentHealth)}/{Mathf.RoundToInt(maxHealth)}";
        }
        if (healthBar != null)
        {
            float percent = (maxHealth > 0) ? currentHealth / maxHealth : 0f;
            healthBar.style.width = Length.Percent(Mathf.Clamp01(percent) * 100f);

            // 動態顏色（移除，交由 USS 控制）
            // Color color;
            // if (percent > 0.6f)
            //     color = Color.green;
            // else if (percent > 0.3f)
            //     color = Color.yellow;
            // else
            //     color = Color.red;
            // healthBar.style.backgroundColor = color;
        }
    }

    private void UpdateFuel(float currentFuel, float maxFuel)
    {
        Debug.Log($"[ShipUI] 更新燃料: {currentFuel}/{maxFuel}");
        if (lblFuel != null)
        {
            lblFuel.text = $"{Mathf.RoundToInt(currentFuel)}/{Mathf.RoundToInt(maxFuel)}";
        }
        if (fuelBar != null)
        {
            float percent = (maxFuel > 0) ? currentFuel / maxFuel : 0f;
            fuelBar.style.width = Length.Percent(Mathf.Clamp01(percent) * 100f);
            // fuelBar.style.backgroundColor = new Color(1f, 0.8f, 0.2f, 1f); // 橘黃色（移除）
        }
    }

    private void SetUIPosition()
    {
        if (ship == null)
        {
            LogError("Ship 為 null，無法設定 UI 位置！");
            return;
        }

        // 根據船隻的位置更新 UI 的位置
        Vector2 shipScreenPosition = Camera.main.WorldToScreenPoint(ship.transform.position);
        UIPanel.style.left = shipScreenPosition.x;
        UIPanel.style.top = Screen.height - shipScreenPosition.y; // 修正為屏幕坐標系
        //Debug.Log($"[ShipUI] 設定 UI 位置為: {shipScreenPosition}");
    }

    private void SetRectPosition()
    {
        DrawSavedRect(ship.NavigationArea);
    }
    #endregion

    #region Pointer & Selection Events
    private void RegisterPointerEvents()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        root.RegisterCallback<PointerDownEvent>(OnPointerDown); // 修正為 PointerDownEvent
        root.RegisterCallback<PointerMoveEvent>(OnPointerMove); // 修正為 PointerMoveEvent
        root.RegisterCallback<PointerUpEvent>(OnPointerUp);     // 修正為 PointerUpEvent
        root.RegisterCallback<PointerDownEvent>(HandleShipSelectionForLine); // 新增處理船隻選擇的事件
        root.RegisterCallback<PointerDownEvent>(OnWaypointPointerDown); // 新增：繪製 waypoint 模式下的 pointer event
    }

    private void HandleShipSelectionForLine(PointerDownEvent evt)
    {
        Debug.Log("[ShipUI] HandleShipSelectionForLine");
        if (isSelectingShipForLine && evt.button == 0) // 左鍵點擊
        {
            Vector2 worldPoint = Camera.main.ScreenToWorldPoint(UnityEngine.InputSystem.Mouse.current.position.ReadValue());
            RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.down, LayerMask.GetMask("Ship")); // 使用射線檢測

            if (hit.collider != null)
            {
                var selectedShip = hit.collider.GetComponent<Warship>(); // 確保檢測到的是 Ship 類型
                if (selectedShip != null && selectedShip != ship)
                {
                    if (selectedShip.IsFollower)
                    {
                        Debug.Log($"[ShipUI] {selectedShip.name} 已經是船隊成員，無法再次選擇。");
                        //debug ship parent
                        Debug.Log($"[ShipUI] {selectedShip.name} 的父物件: {selectedShip.transform.parent.name}");
                        // check if parent is Fleet
                        if (selectedShip.transform.parent != null && selectedShip.transform.parent.GetComponent<Fleet>() != null)
                        {
                            Debug.Log($"[ShipUI] {selectedShip.name} 的父物件是 Fleet");

                            // get leader
                            PlayerShip leader = selectedShip.transform.parent.GetComponent<Fleet>().followers[0] as PlayerShip;
                            if (leader != null)
                            {
                                Debug.Log($"[ShipUI] {selectedShip.name} 的領導者是: {leader.name}");
                                // 取消選擇
                                ship.transform.SetParent(selectedShip.transform.parent.transform);
                                ship.IsFollower = true;
                                ship.LeaderShip = leader;
                                selectedShip.transform.parent.GetComponent<Fleet>().followers.Add(ship);
                                Debug.Log($"[ShipUI] {ship.name} 已加入 {selectedShip.name} 的船隊");
                                isSelectingShipForLine = false; // 停止選擇模式
                                                                // 關閉 UI
                                Destroy(gameObject);
                                Debug.Log("[ShipUI] Ship UI 已關閉。");

                                return;
                                
                            }
                            else
                            {
                                Debug.LogWarning("[ShipUI] 無法獲取 Fleet 的領導者");
                                return;
                            }

                        }
                    }
                    if (selectedShip == null)
                    {
                        Debug.LogWarning("[ShipUI] 選擇的物件不是船隻");
                        return;
                    }
                    // 建立 Fleet parent 物件，並設為 ShipCreationManager 的子物件
                    GameObject fleetParent = new GameObject("FleetGroup");
                    fleetParent.transform.position = selectedShip.transform.position;
                    // 設定 fleetParent 為 ShipCreationManager 的子物件
                    if (ShipCreationManager.Instance != null)
                        fleetParent.transform.SetParent(ShipCreationManager.Instance.transform);

                    // 將 leader 船與被選擇船設為 parent 的子物件
                    selectedShip.transform.SetParent(fleetParent.transform);
                    ship.transform.SetParent(fleetParent.transform);

                    // 掛載 Fleet 組件到 parent
                    var fleet = fleetParent.AddComponent<Fleet>();
                    fleet.followers.Add(selectedShip);
                    fleet.followers.Add(ship);

                    // 設定跟隨狀態
                    ship.IsFollower = true;
                    ship.LeaderShip = selectedShip as PlayerShip;

                    Debug.Log($"[ShipUI] 已建立 FleetGroup 並將 {selectedShip.name} 和 {ship.name} 加入船隊");
                    isSelectingShipForLine = false; // 停止選擇模式

                    // 關閉 UI
                    Destroy(gameObject);
                    Debug.Log("[ShipUI] Ship UI 已關閉。");
                }
                else
                {
                    Debug.Log($"[ShipUI] 點擊的物件不是船隻或是自己: {hit.collider.gameObject.name}");
                }
            }
            else
            {
                Debug.LogWarning("[ShipUI] Raycast 未檢測到任何物件");
            }
        }
        Debug.Log("[ShipUI] Ship selection for line ended");
    }

    private void OnWaypointPointerDown(PointerDownEvent evt)
    {
        if (!IsDrawingWaypoint || evt.button != 0) return;
        // 取得滑鼠點擊的螢幕座標
        Vector2 screenPos = evt.position;
        // 轉換為世界座標
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, Screen.height - screenPos.y, 0));
        worldPos.z = 0;
        // 傳給 PlayerShip
        ship?.AddWaypoint(worldPos);
        // 畫一個 waypoint 標記
        DrawWaypointMarker(worldPos);
    }

    private void DrawWaypointMarker(Vector3 worldPos)
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        var marker = new VisualElement();
        marker.AddToClassList("waypoint-marker");
        // 世界座標轉螢幕座標
        Vector2 screenPos = Camera.main.WorldToScreenPoint(worldPos);
        // UI Toolkit Y 軸反向
        marker.style.position = Position.Absolute;
        marker.style.left = screenPos.x - 8; // 8 為圖示半徑
        marker.style.top = Screen.height - screenPos.y - 8;
        marker.style.width = 16;
        marker.style.height = 16;
        root.Add(marker);
        waypointMarkers.Add(marker);
    }
    #endregion

    #region UnityEvent Subscription
 

    private void OnShipCombatModeChanged(bool isCombatMode)
    {
        if (btnToggleCombatMode != null)
            btnToggleCombatMode.text = $"戰鬥模式: {ship.Mode}";
    }
    #endregion

    #region Utility
    private void LogError(string message)
    {
        // Replace Debug.LogError with centralized logging
        Debug.LogError($"[ShipUI] {message}");
    }

    private void OnDestroy()
    {
        Debug.Log("[ShipUI] 銷毀 ShipUI");
        //Debug where call this destroy
        // Debug.Log(new System.Diagnostics.StackTrace().ToString());
    }
    #endregion

    #region UI Initialization
    private void InitializeUI()
    {
        // 載入 UI 資源
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

        UIPanel = UIHelper.InitializeElement<VisualElement>(root, "UIPanel");
        if (UIPanel == null)
        {
            LogError("找不到名為 'Panel' 的 VisualElement！");
            return;
        }

        InitializeSpeedLabels();
        InitializeRotationLabels();
        InitializeWeaponListContainer();
        InitializeLevelAndExperienceLabels(); // 確保初始化
        InitializeHealthAndFuelLabels();
        InitializeCancelFollowButton();
        InitializeDrawButton();
        InitializeCloseUIButton();
        InitializeFleetCombatModeButton();
        InitializeToggleCombatModeButton();
        InitializeFormFleetButton();
        InitializeDrawWaypointButton(); // 新增
        RegisterPointerEvents();

        lblFuel = UIHelper.InitializeElement<Label>(UIPanel, "lblFuel");
        lblHealth = UIHelper.InitializeElement<Label>(UIPanel, "lblHealth");
        lblLevel = UIHelper.InitializeElement<Label>(UIPanel, "lblLevel");
        lblExperience = UIHelper.InitializeElement<Label>(UIPanel, "lblExperience");
        lblName = UIHelper.InitializeElement<Label>(UIPanel, "lblName"); // 新增：初始化 lblName
    }
    #endregion

    // 新增：更新船名顯示
    private void UpdateShipName()
    {
        if (lblName != null && ship != null)
        {
            lblName.text = $"名稱: {ship.name}";
        }
    }
}
