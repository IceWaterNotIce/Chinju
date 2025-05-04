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

    void TiggerMap()
    {
        //Enable the Line renderer
        
    }

    void Start()
    {
        var uiDoc = GetComponent<UIDocument>();
        if (uiDoc == null)
        {
            Debug.LogError("[ShipUI] 無法找到 UIDocument 組件！");
            return;
        }

        var root = uiDoc.rootVisualElement;
        if (root == null)
        {
            Debug.LogError("[ShipUI] 無法初始化 root 元素！");
            return;
        }

        Panel = UIHelper.InitializeElement<VisualElement>(root, "Panel");
        lblSpeedFrontFull = UIHelper.InitializeElement<Label>(Panel, "lblSpeedFrontFull");
        lblSpeedFrontThreeQuarters = UIHelper.InitializeElement<Label>(Panel, "lblSpeedFrontThreeQuarters");
        lblSpeedFrontHalf = UIHelper.InitializeElement<Label>(Panel, "lblSpeedFrontHalf");
        lblSpeedFrontQuarter = UIHelper.InitializeElement<Label>(Panel, "lblSpeedFrontQuarter");
        lblSpeedStop = UIHelper.InitializeElement<Label>(Panel, "lblSpeedStop");
        lblSpeedBackFull = UIHelper.InitializeElement<Label>(Panel, "lblSpeedBackFull");

        lblRotationLeftFull = UIHelper.InitializeElement<Label>(Panel, "lblRotationLeftFull");
        lblRotationLeftHalf = UIHelper.InitializeElement<Label>(Panel, "lblRotationLeftHalf");
        lblRotationStop = UIHelper.InitializeElement<Label>(Panel, "lblRotationStop");
        lblRotationRightHalf = UIHelper.InitializeElement<Label>(Panel, "lblRotationRightHalf");
        lblRotationRightFull = UIHelper.InitializeElement<Label>(Panel, "lblRotationRightFull");

        weaponListContainer = UIHelper.InitializeElement<VisualElement>(Panel, "weaponListContainer");
        if (weaponListContainer != null)
        {
            weaponListContainer.Clear();
        }
    }

    public void Initial(Ship s)
    {
        ship = s;

        // 檢查是否已存在 UIDocument 組件
        UIDocument uiDoc = GetComponent<UIDocument>();
        if (uiDoc == null)
        {
            uiDoc = gameObject.AddComponent<UIDocument>();
            if (uiDoc == null)
            {
                Debug.LogError("[ShipUI] UIDocument 無法初始化！");
                return;
            }
        }

        // load panel settings
        uiDoc.panelSettings = Resources.Load<PanelSettings>("UI/PanelSettings");
        if (uiDoc.panelSettings == null)
        {
            Debug.LogError("[ShipUI] 無法加載 PanelSettings 資源！");
            return;
        }

        // load the UXML file
        uiDoc.visualTreeAsset = Resources.Load<VisualTreeAsset>("UI/ShipUI");
        if (uiDoc.visualTreeAsset == null)
        {
            Debug.LogError("[ShipUI] 無法加載 ShipUI 資源！");
            return;
        }

        VisualElement root = uiDoc.rootVisualElement;
        if (root == null)
        {
            Debug.LogError("[ShipUI] UIDocument 的 rootVisualElement 為 null！");
            return;
        }

        Panel = root.Q<VisualElement>("Panel");
        if (Panel == null)
        {
            Debug.LogError("[ShipUI] 找不到名為 'Panel' 的 VisualElement！");
            return;
        }

        root.RegisterCallback<ClickEvent>(ev => Destroy(gameObject));

        lblSpeedFrontFull = UIHelper.InitializeElement<Label>(Panel, "lblSpeedFrontFull");
        lblSpeedFrontThreeQuarters = UIHelper.InitializeElement<Label>(Panel, "lblSpeedFrontThreeQuarters");
        lblSpeedFrontHalf = UIHelper.InitializeElement<Label>(Panel, "lblSpeedFrontHalf");
        lblSpeedFrontQuarter = UIHelper.InitializeElement<Label>(Panel, "lblSpeedFrontQuarter");
        lblSpeedStop = UIHelper.InitializeElement<Label>(Panel, "lblSpeedStop");
        lblSpeedBackFull = UIHelper.InitializeElement<Label>(Panel, "lblSpeedBackFull");

        lblRotationLeftFull = UIHelper.InitializeElement<Label>(Panel, "lblRotationLeftFull");
        lblRotationLeftHalf = UIHelper.InitializeElement<Label>(Panel, "lblRotationLeftHalf");
        lblRotationStop = UIHelper.InitializeElement<Label>(Panel, "lblRotationStop");
        lblRotationRightHalf = UIHelper.InitializeElement<Label>(Panel, "lblRotationRightHalf");
        lblRotationRightFull = UIHelper.InitializeElement<Label>(Panel, "lblRotationRightFull");

        // 新增：武器列表容器
        weaponListContainer = Panel.Q<VisualElement>("weaponListContainer");
        if (weaponListContainer == null)
        {
            weaponListContainer = new VisualElement();
            weaponListContainer.name = "weaponListContainer";
            weaponListContainer.style.flexDirection = FlexDirection.Row;
            weaponListContainer.style.marginTop = 10;
            Panel.Add(weaponListContainer);
        }
        weaponListContainer.Clear();

        // 依照 intWeaponLimit 顯示武器欄位
        int weaponSlotCount = ship.IntWeaponLimit;
        for (int i = 0; i < weaponSlotCount; i++)
        {
            Weapon weapon = (ship.weapons != null && i < ship.weapons.Count) ? ship.weapons[i] : null;
            VisualElement weaponIcon = new VisualElement();
            weaponIcon.style.width = 32;
            weaponIcon.style.height = 32;
            weaponIcon.style.marginRight = 8;

            if (weapon != null)
            {
                weaponIcon.style.backgroundColor = new Color(0.8f, 0.8f, 0.2f, 1f);
                weaponIcon.tooltip = $"武器{i+1}";
                int weaponIndex = i; // 避免閉包問題
                weaponIcon.RegisterCallback<ClickEvent>(ev => ShowWeaponDetail(ship.weapons[weaponIndex]));
            }
            else
            {
                weaponIcon.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.3f); // 空槽顏色
                weaponIcon.tooltip = $"空武器槽{i+1}";
                int weaponIndex = i; // 避免閉包問題
                weaponIcon.RegisterCallback<ClickEvent>(ev => ShowWeaponSelectionPanel(weaponIndex));
            }
            weaponListContainer.Add(weaponIcon);
        }

        lblLevel = Panel.Q<Label>("lblLevel");
        if (lblLevel == null)
        {
            lblLevel = new Label();
            lblLevel.name = "lblLevel";
            lblLevel.style.marginTop = 10;
            lblLevel.style.unityTextAlign = TextAnchor.MiddleLeft;
            Panel.Add(lblLevel);
        }

        lblExperience = Panel.Q<Label>("lblExperience");
        if (lblExperience == null)
        {
            lblExperience = new Label();
            lblExperience.name = "lblExperience";
            lblExperience.style.marginTop = 5;
            lblExperience.style.unityTextAlign = TextAnchor.MiddleLeft;
            Panel.Add(lblExperience);
        }

        UpdateLevelAndExperienceUI(); // 更新等級和經驗值的顯示

        // set the ui position
        Vector2 shipScreenPosition = Camera.main.WorldToScreenPoint(ship.transform.position);
        Debug.Log("Ship Screen Position: " + shipScreenPosition);
        Panel.style.left = shipScreenPosition.x;
        Panel.style.top = shipScreenPosition.y;

        // Set the speed labels
        lblSpeedFrontFull.RegisterCallback<ClickEvent>(ev => SpeedControll(1.0f));
        lblSpeedFrontThreeQuarters.RegisterCallback<ClickEvent>(ev => SpeedControll(0.75f));
        lblSpeedFrontHalf.RegisterCallback<ClickEvent>(ev => SpeedControll(0.5f));
        lblSpeedFrontQuarter.RegisterCallback<ClickEvent>(ev => SpeedControll(0.25f));
        lblSpeedStop.RegisterCallback<ClickEvent>(ev => SpeedControll(0.0f));
        lblSpeedBackFull.RegisterCallback<ClickEvent>(ev => SpeedControll(-0.25f));

        // Set the rotation labels
        lblRotationLeftFull.RegisterCallback<ClickEvent>(ev => RotationControll(1.0f));
        lblRotationLeftHalf.RegisterCallback<ClickEvent>(ev => RotationControll(0.5f));
        lblRotationStop.RegisterCallback<ClickEvent>(ev => RotationControll(0.0f));
        lblRotationRightHalf.RegisterCallback<ClickEvent>(ev => RotationControll(-0.5f));
        lblRotationRightFull.RegisterCallback<ClickEvent>(ev => RotationControll(-1.0f));
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
            Debug.LogError("ship speed control fail. Ship is not set.");
            return;
        }
        float MaxSpeed = ship.MaxSpeed;
        float TargetSpeed = MaxSpeed * percentage;
        ship.TargetSpeed = TargetSpeed;
        Debug.Log("Speed: " + TargetSpeed);

        // Distory the UI
        Destroy(gameObject);
    }

    void RotationControll(float percentage)
    {
        if (ship == null)
        {
            Debug.LogError("ship rotation control fail. Ship is not set.");
            return;
        }
        float MaxRotationSpeed = ship.MaxRotationSpeed;
        float TargetRotationSpeed = MaxRotationSpeed * percentage;
        ship.TargetRotationSpeed = TargetRotationSpeed;
        Debug.Log("Rotation Speed: " + TargetRotationSpeed);

        // Distory the UI
        Destroy(gameObject);
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
                icon.tooltip = $"武器{i+1}";
                int weaponIndex = i;
                icon.RegisterCallback<ClickEvent>(ev => ShowWeaponDetail(ship.weapons[weaponIndex]));
            }
            else
            {
                icon.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.3f);
                icon.tooltip = $"空武器槽{i+1}";
            }
            row.Add(icon);

            Label label = new Label(weapon != null ? $"武器{i+1}" : $"空武器槽{i+1}");
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
}
