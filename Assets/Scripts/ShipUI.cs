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

    void TiggerMap()
    {
        //Enable the Line renderer
        
    }

    void Start()
    {

    }

    public void Initial(Ship s)
    {
        ship = s;

        // Add UI Document component
        UIDocument uiDoc = gameObject.AddComponent<UIDocument>();
        // load panel settings
        uiDoc.panelSettings = Resources.Load<PanelSettings>("UI/PanelSettings");
        // load the UXML file
        uiDoc.visualTreeAsset = Resources.Load<VisualTreeAsset>("UI/ShipUI");
        VisualElement root = uiDoc.rootVisualElement;
        Panel = root.Q<VisualElement>("Panel");
        root.RegisterCallback<ClickEvent>(ev => Destroy(gameObject));

        lblSpeedFrontFull = Panel.Q<Label>("lblSpeedFrontFull");
        lblSpeedFrontThreeQuarters = root.Q<Label>("lblSpeedFrontThreeQuarters");
        lblSpeedFrontHalf = Panel.Q<Label>("lblSpeedFrontHalf");
        lblSpeedFrontQuarter = root.Q<Label>("lblSpeedFrontQuarter");
        lblSpeedStop = Panel.Q<Label>("lblSpeedStop");
        lblSpeedBackFull = Panel.Q<Label>("lblSpeedBackFull");

        lblRotationLeftFull = root.Q<Label>("lblRotationLeftFull");
        lblRotationLeftHalf = root.Q<Label>("lblRotationLeftHalf");
        lblRotationStop = root.Q<Label>("lblRotationStop");
        lblRotationRightHalf = root.Q<Label>("lblRotationRightHalf");
        lblRotationRightFull = root.Q<Label>("lblRotationRightFull");

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
            }
            weaponListContainer.Add(weaponIcon);
        }

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
        weaponDetailPopup.style.position = Position.Absolute;
        weaponDetailPopup.style.left = 100;
        weaponDetailPopup.style.top = 100;
        weaponDetailPopup.style.width = 220;
        weaponDetailPopup.style.backgroundColor = new Color(0, 0, 0, 0.85f);
        weaponDetailPopup.style.paddingLeft = 10;
        weaponDetailPopup.style.paddingRight = 10;
        weaponDetailPopup.style.paddingTop = 10;
        weaponDetailPopup.style.paddingBottom = 10;
        weaponDetailPopup.style.borderTopLeftRadius = 8;
        weaponDetailPopup.style.borderTopRightRadius = 8;
        weaponDetailPopup.style.borderBottomLeftRadius = 8;
        weaponDetailPopup.style.borderBottomRightRadius = 8;

        Label title = new Label("武器資訊");
        title.style.unityFontStyleAndWeight = FontStyle.Bold;
        weaponDetailPopup.Add(title);

        weaponDetailPopup.Add(new Label($"最大攻擊距離: {weapon.MaxAttackDistance}"));
        weaponDetailPopup.Add(new Label($"最小攻擊距離: {weapon.MinAttackDistance}"));
        weaponDetailPopup.Add(new Label($"攻擊冷卻: {weapon.AttackCooldown} 秒"));
        weaponDetailPopup.Add(new Label($"彈藥預製體: {(weapon.AmmoPrefab != null ? weapon.AmmoPrefab.name : "無")}"));

        Button closeBtn = new Button(() => weaponDetailPopup.RemoveFromHierarchy()) { text = "關閉" };
        closeBtn.style.marginTop = 10;
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
        weaponsPanel.style.position = Position.Absolute;
        weaponsPanel.style.left = 200;
        weaponsPanel.style.top = 100;
        weaponsPanel.style.width = 300;
        weaponsPanel.style.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.95f);
        weaponsPanel.style.paddingLeft = 16;
        weaponsPanel.style.paddingRight = 16;
        weaponsPanel.style.paddingTop = 16;
        weaponsPanel.style.paddingBottom = 16;
        weaponsPanel.style.borderTopLeftRadius = 10;
        weaponsPanel.style.borderTopRightRadius = 10;
        weaponsPanel.style.borderBottomLeftRadius = 10;
        weaponsPanel.style.borderBottomRightRadius = 10;

        Label title = new Label("武器總覽");
        title.style.unityFontStyleAndWeight = FontStyle.Bold;
        title.style.fontSize = 16;
        weaponsPanel.Add(title);

        int weaponSlotCount = ship.IntWeaponLimit;
        for (int i = 0; i < weaponSlotCount; i++)
        {
            Weapon weapon = (ship.weapons != null && i < ship.weapons.Count) ? ship.weapons[i] : null;
            VisualElement row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.marginTop = 8;
            row.style.alignItems = Align.Center;

            VisualElement icon = new VisualElement();
            icon.style.width = 32;
            icon.style.height = 32;
            icon.style.marginRight = 8;

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
            label.style.unityTextAlign = TextAnchor.MiddleLeft;
            row.Add(label);

            weaponsPanel.Add(row);
        }

        Button closeBtn = new Button(() => weaponsPanel.RemoveFromHierarchy()) { text = "關閉" };
        closeBtn.style.marginTop = 16;
        weaponsPanel.Add(closeBtn);

        Panel.Add(weaponsPanel);
    }
}
