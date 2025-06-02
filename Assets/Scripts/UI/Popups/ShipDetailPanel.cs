using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Events; // 新增
using System.Collections.Generic;
using System; // <--- 加入這行
using System.Linq;
public class ShipDetailPanel : Singleton<ShipDetailPanel>
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

    // 新增：武器列表容器
    private VisualElement weaponListContainer;
    private VisualElement weaponDetailPopup;

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
    private VisualElement waypointsContainer; // 新增：waypoint 標記的容器
    private Label lblName; // 新增：顯示船名的 Label

    private VisualElement root; // 緩存 rootVisualElement
    private Camera MainCamera;      // 緩存 Camera.main
    #endregion

    #region Unity Methods
    void Start()
    {
        InitializeUI();
        MainCamera = Camera.main;
    }

    void Update()
    {
        SetUIPosition();
        SetRectPosition();
        UpdateWaypointMarkersPosition();
        if (MainCamera == null)
            MainCamera = Camera.main;
    }

    private void OnDestroy()
    {
        Debug.Log("[ShipDetailPanel] 銷毀 ShipDetailPanel");
        if (ship != null)
        {
            if (ship.OnCombatModeChanged != null)
                ship.OnCombatModeChanged.RemoveListener(UpdateCombatMode);
        }
    }
    #endregion

    #region Initialization
    public void Initial(PlayerShip s)
    {
        ship = s;
        InitializeUI();

        UpdateHealth(ship.Health, ship.MaxHealth);
        UpdateFuel(ship.CurrentFuel, ship.MaxFuel);

        ship.OnHealthChanged += health => UpdateHealth(health, ship.MaxHealth);
        ship.OnFuelChanged += fuel => UpdateFuel(fuel, ship.MaxFuel);
        var warship = ship.gameObject.GetComponent<Warship>();
        if (warship != null)
        {
            warship.OnExperienceChanged += exp => UpdateExperience(exp, warship.Level);
            warship.OnLevelChanged += level => UpdateLevel(level);
        }
        ship.OnCombatModeChanged.AddListener(isInCombat => UpdateCombatMode(isInCombat));

        SetUIPosition();

        if (ship.NavigationArea.width > 0 && ship.NavigationArea.height > 0)
        {
            DrawSavedRect(ship.NavigationArea);
        }
        if (btnToggleCombatMode != null)
        {
            btnToggleCombatMode.text = $"戰鬥模式: {ship.Mode}";
        }
        if (lblName != null)
        {
            lblName.text = $"名稱: {ship.name}";
        }
    }

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

        uiDoc.visualTreeAsset = Resources.Load<VisualTreeAsset>("UI/ShipDetailPanel");
        if (uiDoc.visualTreeAsset == null)
        {
            LogError("無法加載 ShipDetailPanel 資源！");
            return;
        }

        root = uiDoc.rootVisualElement; // 緩存 rootVisualElement
        if (root == null)
        {
            LogError("UIDocument 的 rootVisualElement 為 null！");
            return;
        }

        UIPanel = UIHelper.InitializeElement<VisualElement>(root, "UIPanel");
        // 不再檢查 UIPanel == null

        // --- Inline InitializeSpeedLabels ---
        lblSpeedFrontFull = InitializeSpeedLabel("lblSpeedFrontFull", 1.0f);
        lblSpeedFrontThreeQuarters = InitializeSpeedLabel("lblSpeedFrontThreeQuarters", 0.75f);
        lblSpeedFrontHalf = InitializeSpeedLabel("lblSpeedFrontHalf", 0.5f);
        lblSpeedFrontQuarter = InitializeSpeedLabel("lblSpeedFrontQuarter", 0.25f);
        lblSpeedStop = InitializeSpeedLabel("lblSpeedStop", 0.0f);
        lblSpeedBackFull = InitializeSpeedLabel("lblSpeedBackFull", -0.25f);

        // --- Inline InitializeRotationLabels ---
        lblRotationLeftFull = InitializeRotationLabel("lblRotationLeftFull", 1.0f);
        lblRotationLeftHalf = InitializeRotationLabel("lblRotationLeftHalf", 0.5f);
        lblRotationStop = InitializeRotationLabel("lblRotationStop", 0.0f);
        lblRotationRightHalf = InitializeRotationLabel("lblRotationRightHalf", -0.5f);
        lblRotationRightFull = InitializeRotationLabel("lblRotationRightFull", -1.0f);

        // --- Inline InitializeWeaponListContainer ---
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

        // --- Inline InitializeLevelAndExperienceLabels ---
        lblLevel = UIHelper.InitializeElement<Label>(UIPanel, "lblLevel");
        lblExperience = UIHelper.InitializeElement<Label>(UIPanel, "lblExperience");

        // --- Inline InitializeHealthAndFuelLabels ---
        healthBar = UIHelper.InitializeElement<VisualElement>(UIPanel, "healthBar");
        fuelBar = UIHelper.InitializeElement<VisualElement>(UIPanel, "fuelBar");
        expBar = UIHelper.InitializeElement<VisualElement>(UIPanel, "expBar");

        // --- Inline InitializeCancelFollowButton ---
        btnCancelFollow = UIHelper.InitializeElement<Button>(UIPanel, "btnCancelFollow");
        btnCancelFollow.clicked += () =>
        {
            var cameraController = Camera.main?.GetComponent<CameraBound2D>();
            if (cameraController != null)
            {
                cameraController.StopFollowing();
                Debug.Log("[ShipDetailPanel] 已取消攝影機跟隨。");
            }
        };

        // --- Inline InitializeDrawButton ---
        var rootDoc = GetComponent<UIDocument>().rootVisualElement;
        startDrawButton = rootDoc.Q<Button>("StartDrawButton");
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

        // --- Inline InitializeCloseUIButton ---
        btnCloseUI = UIHelper.InitializeElement<Button>(UIPanel, "btnCloseUI");
        btnCloseUI.clicked += () =>
        {
            Destroy(gameObject); // 銷毀 ShipDetailPanel
            Debug.Log("[ShipDetailPanel] Ship UI 已關閉。");
        };

        // --- Inline InitializeFleetCombatModeButton ---
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
                PlayerShip leader = ship.LeaderShip != null ? ship.LeaderShip as PlayerShip : ship;
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
                    Debug.Log($"[ShipDetailPanel] 已將船隊所有船隻戰鬥模式設為: {nextMode}");
                    if (btnToggleCombatMode != null)
                        btnToggleCombatMode.text = $"戰鬥模式: {nextMode}";
                }
            };
        }
        else
        {
            var existBtn = UIPanel.Q<Button>("btnFleetCombatMode");
            if (existBtn != null)
                existBtn.RemoveFromHierarchy();
        }

        // --- Inline InitializeToggleCombatModeButton ---
        btnToggleCombatMode = UIHelper.InitializeElement<Button>(UIPanel, "btnToggleCombatMode");
        btnToggleCombatMode.clicked += () =>
        {
            if (ship != null)
            {
                // 切換枚舉狀態
                var mode = ship.Mode;
                mode = (CombatMode)(((int)mode + 1) % Enum.GetValues(typeof(CombatMode)).Length);
                ship.Mode = mode;
                Debug.Log($"[ShipDetailPanel] 戰鬥模式切換為: {mode}");
                btnToggleCombatMode.text = $"戰鬥模式: {mode}";
            }
        };

        // --- Inline InitializeFormFleetButton ---
        btnFormFleet = UIHelper.InitializeElement<Button>(UIPanel, "btnFormFleet");
        btnFormFleet.clicked += () =>
        {
            isSelectingShipForLine = true; // 啟用選擇船隻模式
            Debug.Log("[ShipDetailPanel] 選擇船隻以形成船隊模式啟用");
        };

        // --- Inline InitializeDrawWaypointButton ---
        btnDrawWaypoint = UIHelper.InitializeElement<Button>(UIPanel, "btnDrawWaypoint");
        if (btnDrawWaypoint == null)
        {
            btnDrawWaypoint = new Button() { name = "btnDrawWaypoint", text = "繪製航點" };
            UIPanel.Add(btnDrawWaypoint);
        }
        btnDrawWaypoint.clicked += ToggleDrawWaypointMode;
        UpdateDrawWaypointButtonState();

        // --- Inline RegisterPointerEvents ---
        root.RegisterCallback<PointerDownEvent>(OnPointerDown);
        root.RegisterCallback<PointerMoveEvent>(OnPointerMove);
        root.RegisterCallback<PointerUpEvent>(OnPointerUp);
        root.RegisterCallback<PointerDownEvent>(HandleShipSelectionForLine);
        root.RegisterCallback<PointerDownEvent>(OnWaypointPointerDown);

        lblFuel = UIHelper.InitializeElement<Label>(UIPanel, "lblFuel");
        lblHealth = UIHelper.InitializeElement<Label>(UIPanel, "lblHealth");
        lblLevel = UIHelper.InitializeElement<Label>(UIPanel, "lblLevel");
        lblExperience = UIHelper.InitializeElement<Label>(UIPanel, "lblExperience");
        lblName = UIHelper.InitializeElement<Label>(UIPanel, "lblName"); // 新增：初始化 lblName
        // 新增：初始化 waypointsContainer
        waypointsContainer = UIHelper.InitializeElement<VisualElement>(root, "waypointsContainer");
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

    void UpdateExperience(float exp, int level)
    {
        if (lblExperience != null)
        {
            lblExperience.text = $"經驗值: {exp}/{level * 10}";
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

    private void SetUIPosition()
    {
        if (ship == null)
        {
            LogError("Ship 為 null，無法設定 UI 位置！");
            return;
        }

        // 使用 UIHelper 綁定 UI 到世界座標
        UIHelper.BindToWorldPosition(UIPanel, ship.transform.position, MainCamera, true);
    }

    private void SetRectPosition()
    {
        DrawSavedRect(ship.NavigationArea);
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

    private Label InitializeSpeedLabel(string name, float speedPercentage)
    {
        var label = UIHelper.InitializeElement<Label>(UIPanel, name);
        label.RegisterCallback<ClickEvent>(ev => SpeedControll(speedPercentage));
        return label;
    }


    private Label InitializeRotationLabel(string name, float rotationPercentage)
    {
        var label = UIHelper.InitializeElement<Label>(UIPanel, name);
        label.RegisterCallback<ClickEvent>(ev => RotationControll(rotationPercentage));
        return label;
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
            Debug.Log("[ShipDetailPanel] 矩形和數據已清除");
        }
    }

    private void EnableDrawing()
    {
        canDraw = true; // 啟用繪製功能
        Debug.Log("[ShipDetailPanel] 繪製功能已啟用");
    }

    private void OnPointerDown(PointerDownEvent evt)
    {
        Debug.Log("[ShipDetailPanel] PointerDownEvent");
        if (rectContainer == null)
        {
            rectContainer = root.Q<VisualElement>("rectContainer");
            if (rectContainer == null)
            {
                Debug.LogError("[ShipDetailPanel] 找不到名為 'rectContainer' 的 VisualElement！");
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
        Debug.Log("[ShipDetailPanel] 開始繪製矩形");
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
                    Debug.Log($"[ShipDetailPanel] 矩形區域已保存到船隻: {worldRect}");
                }

                currentRect = null; // 重置 currentRect 狀態
                //delete all rects
                rectContainer.Clear();
            }

            Debug.Log("[ShipDetailPanel] 繪製結束");
        }
    }

    private void DrawSavedRect(Rect rect)
    {
        if (rect == Rect.zero) return; // 如果矩形為零，則不繪製
        var rectContainer = root.Q<VisualElement>("rectContainer");

        if (savedRectElement != null)
        {
            savedRectElement.RemoveFromHierarchy();
        }

        savedRectElement = new VisualElement();
        savedRectElement.AddToClassList("rect"); // 套用矩形樣式
        savedRectElement.style.position = Position.Absolute;

        rectContainer.Add(savedRectElement);
        UpdateSavedRectPosition(rect); // 初始化位置
        Debug.Log($"[ShipDetailPanel] 繪製保存的矩形區域: {rect}");
    }

    private void UpdateSavedRectPosition(Rect rect)
    {
        if (savedRectElement == null) return;

        // 取世界座標的四個角
        Vector3 worldA = new Vector3(rect.xMin, rect.yMin, 0);
        Vector3 worldB = new Vector3(rect.xMax, rect.yMax, 0);

        // 使用 UIHelper 轉換螢幕座標
        Vector2 screenA = MainCamera.WorldToScreenPoint(worldA);
        Vector2 screenB = MainCamera.WorldToScreenPoint(worldB);

        float left = Mathf.Min(screenA.x, screenB.x);
        float right = Mathf.Max(screenA.x, screenB.x);
        float bottom = Mathf.Min(screenA.y, screenB.y);
        float top = Mathf.Max(screenA.y, screenB.y);

        float uiLeft = left;
        float uiTop = Screen.height - top;
        float width = right - left;
        float height = top - bottom;

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
        var playerData = GameDataController.Instance.CurrentGameData.players.FirstOrDefault();
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

    #region Waypoint UI
    private void UpdateDrawWaypointButtonState()
    {
        bool canDraw = true;
        if (ship != null && ship.transform.parent != null && ship.transform.parent.GetComponent<Fleet>() != null)
        {
            canDraw = FleetManager.Instance.IsFleetLeader(ship);
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
        if (waypointsContainer != null)
            waypointsContainer.Clear();
        waypointMarkers.Clear();
        ship?.ClearWaypoints();
    }


    private void OnWaypointPointerDown(PointerDownEvent evt)
    {
        if (!IsDrawingWaypoint || evt.button != 0) return;
        // 取得滑鼠點擊的螢幕座標
        Vector2 screenPos = evt.position;
        // 轉換為世界座標
        Vector3 worldPos = MainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, Screen.height - screenPos.y, 0));
        worldPos.z = 0;
        // 傳給 PlayerShip
        ship?.AddWaypoint(worldPos);
        // 畫一個 waypoint 標記
        DrawWaypointMarker(worldPos);
    }

    private void DrawWaypointMarker(Vector3 worldPos)
    {
        // 不再檢查 waypointsContainer == null
        var marker = new VisualElement();
        marker.AddToClassList("waypoint-marker");
        marker.style.position = Position.Absolute;
        marker.style.width = 16;
        marker.style.height = 16;
        waypointsContainer.Add(marker);
        waypointMarkers.Add(marker);

        // 設定初始位置
        SetWaypointMarkerPosition(marker, worldPos);
    }

    // 根據世界座標設定 waypoint marker 的 UI 位置（改用 UIHelper）
    private void SetWaypointMarkerPosition(VisualElement marker, Vector3 worldPos)
    {
        UIHelper.BindToWorldPosition(marker, worldPos, MainCamera, true);
        marker.style.left = marker.resolvedStyle.left - 8;
        marker.style.top = marker.resolvedStyle.top - 8;
    }

    // 新增：每幀更新所有 waypoint marker 的位置
    private void UpdateWaypointMarkersPosition()
    {
        DrawSavedWaypointMarkers();
    }

    private void DrawSavedWaypointMarkers()
    {
        if (ship == null || ship.Waypoints == null || waypointsContainer == null) return;

        // 清除現有的 waypoint 標記
        waypointsContainer.Clear();
        waypointMarkers.Clear();

        foreach (var waypoint in ship.Waypoints)
        {
            DrawWaypointMarker(waypoint);
        }
    }
    #endregion

    #region Pointer & Selection Events
    private void HandleShipSelectionForLine(PointerDownEvent evt)
    {
        Debug.Log("[ShipDetailPanel] HandleShipSelectionForLine called");
        if (!isSelectingShipForLine || evt.button != 0) return;

        Vector2 worldPoint = MainCamera.ScreenToWorldPoint(UnityEngine.InputSystem.Mouse.current.position.ReadValue());
        RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.down, Mathf.Infinity, LayerMask.GetMask("Ship"));
        Debug.Log("[ShipDetailPanel] Raycast hit: " + hit.collider.name);

        if (hit.collider == null) return;

        var selectedShip = hit.collider.GetComponent<Warship>();
        if (selectedShip == null || selectedShip == ship) return;

        if (selectedShip.IsFollower) FleetManager.Instance.AddShipToFleet(ship, selectedShip.transform.parent.GetComponent<Fleet>());
        else FleetManager.Instance.CreateFleet(new Warship[] { selectedShip, ship });

        isSelectingShipForLine = false;
        Destroy(gameObject);

        Debug.Log("[ShipDetailPanel] 已選擇船隻以形成船隊。");
    }
    #endregion

    #region Utility
    private void LogError(string message)
    {
        // Replace Debug.LogError with centralized logging
        Debug.LogError($"[ShipDetailPanel] {message}");
    }
    #endregion
}
