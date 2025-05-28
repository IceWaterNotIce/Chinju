using UnityEngine;
using UnityEngine.UIElements;

public class BottomNavBarUIController : MonoBehaviour
{
    private Button shopButton;
    private Button fleetButton;
    private Button shipButton;
    private Button weaponButton;
    private Button missionButton;
    private Button buildButton;
    private Button settingButton;

    void Start()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        shopButton = root.Q<Button>("ShopButton");
        if (shopButton != null)
            shopButton.clicked += ToggleShopPanel;

        fleetButton = root.Q<Button>("FleetButton");
        if (fleetButton != null)
            fleetButton.clicked += ToggleFleetPanel;

        shipButton = root.Q<Button>("ShipButton");
        if (shipButton != null)
            shipButton.clicked += ToggleShipPanel;

        weaponButton = root.Q<Button>("WeaponButton");
        if (weaponButton != null)
            weaponButton.clicked += ToggleWeaponPanel;

        missionButton = root.Q<Button>("MissionButton");
        if (missionButton != null)
            missionButton.clicked += ToggleMissionPanel;

        buildButton = root.Q<Button>("BuildButton");
        if (buildButton != null)
            buildButton.clicked += ToggleBuildPanel;

        settingButton = root.Q<Button>("SettingButton");
        if (settingButton != null)
            settingButton.clicked += ToggleSettingPanel;
    }

    private void ToggleShopPanel()
    {
        TogglePanel("ShopPanel");
    }
    private void ToggleFleetPanel()
    {
        TogglePanel("FleetPanel");
    }
    private void ToggleShipPanel()
    {
        TogglePanel("ShipPanel");
    }
    private void ToggleWeaponPanel()
    {
        TogglePanel("WeaponPanel");
    }
    private void ToggleMissionPanel()
    {
        TogglePanel("MissionPanel");
    }
    private void ToggleBuildPanel()
    {
        TogglePanel("BuildPanel");
    }
    private void ToggleSettingPanel()
    {
        TogglePanel("SettingPanel");
    }

    private void TogglePanel(string panelName)
    {
        if (PopupManager.Instance.IsPopupVisible(panelName))
            PopupManager.Instance.HidePopup(panelName);
        else
            PopupManager.Instance.ShowPopup(panelName);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
