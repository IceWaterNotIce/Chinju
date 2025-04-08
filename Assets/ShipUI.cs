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
}
