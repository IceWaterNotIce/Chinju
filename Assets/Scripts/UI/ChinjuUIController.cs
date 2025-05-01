using UnityEngine;
using UnityEngine.UIElements;

public class ChinjuUIController : MonoBehaviour
{
    public WeaponCreatePanelController weaponCreatePanelController;
    public ShipCreationPanel shipCreationPanel;
    private VisualElement chinjuRoot;

    void Start()
    {
        var uiDoc = GetComponent<UIDocument>();
        chinjuRoot = uiDoc?.rootVisualElement;

        BindButton("btnOpenWeaponCreatePanel", OpenWeaponCreatePanel);
        BindButton("btnOpenShipCreatePanel", OpenShipCreatePanel);

        ValidatePanel(weaponCreatePanelController, "WeaponCreatePanel");
        ValidatePanel(shipCreationPanel, "ShipCreatePanel");

        weaponCreatePanelController?.Hide();
        shipCreationPanel?.Hide();

        Debug.Log("[ChinjuUIController] Hide Chinju UI Panel at Start");
        Hide();
    }

    private void BindButton(string buttonName, System.Action action)
    {
        var button = chinjuRoot?.Q<Button>(buttonName);
        if (button != null) button.clicked += action;
    }

    private void ValidatePanel(MonoBehaviour panel, string panelName)
    {
        if (panel == null)
        {
            Debug.LogError($"[ChinjuUIController] 無法找到 {panelName} 子物件！");
        }
    }

    public void Show()
    {
        SetPanelDisplay(DisplayStyle.Flex, "[ChinjuUIController] Show Chinju UI Panel");
    }

    public void Hide()
    {
        SetPanelDisplay(DisplayStyle.None, "[ChinjuUIController] Hide Chinju UI Panel");
    }

    private void SetPanelDisplay(DisplayStyle displayStyle, string logMessage)
    {
        if (chinjuRoot != null)
        {
            chinjuRoot.style.display = displayStyle;
            Debug.Log(logMessage);
        }
    }

    public void ToggleMainPanelOnly()
    {
        if (chinjuRoot != null)
        {
            var newDisplay = chinjuRoot.style.display == DisplayStyle.None ? DisplayStyle.Flex : DisplayStyle.None;
            SetPanelDisplay(newDisplay, $"[ChinjuUIController] {(newDisplay == DisplayStyle.Flex ? "Show" : "Hide")} Main Panel Only");
        }
    }

    public void OpenWeaponCreatePanel()
    {
        weaponCreatePanelController.Show();
        Debug.Log("[ChinjuUIController] Open Weapon Create Panel");
    }

    public void OpenShipCreatePanel()
    {
        shipCreationPanel.Show();
        Debug.Log("[ChinjuUIController] Open Ship Create Panel");
    }


    public void CloseCurrentPanel()
    {
        Debug.Log("[ChinjuUIController] Close All Sub Panels");
        weaponCreatePanelController?.Hide();
        shipCreationPanel?.Hide();
    }
}
