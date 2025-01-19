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
        lblSpeedBackFull.RegisterCallback<ClickEvent>(ev => SpeedControll(-1.0f));
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
        float Speed = MaxSpeed * percentage;
        ship.Speed = Speed;
        Debug.Log("Speed: " + Speed);

        // Distory the UI
        Destroy(gameObject);
    }
}
