using UnityEngine;
using UnityEngine.UIElements;

public class ChinjuUIController : MonoBehaviour
{
    public WeaponCreatePanelController weaponCreatePanelController;
    public ShipCreationPanel shipCreationPanel;
    private VisualElement chinjuRoot;

    private void Awake()
    {
        PopupManager.Instance.RegisterPopup("ChinjuUI", gameObject);
    }

    private void OnEnable()
    {
        InitializeUI();
    }

    private void InitializeUI()
    {
        var uiDoc = GetComponent<UIDocument>();
        chinjuRoot = uiDoc?.rootVisualElement;

        var btnOpenWeaponCreatePanel = UIHelper.InitializeElement<Button>(chinjuRoot, "btnOpenWeaponCreatePanel");
        if (btnOpenWeaponCreatePanel != null) btnOpenWeaponCreatePanel.clicked += () =>
        {
            PopupManager.Instance.ShowPopup("WeaponCreatePanel");
            PopupManager.Instance.HidePopup("ChinjuUI");
        };
        var btnOpenShipCreatePanel = UIHelper.InitializeElement<Button>(chinjuRoot, "btnOpenShipCreatePanel");
        if (btnOpenShipCreatePanel != null) btnOpenShipCreatePanel.clicked += () =>
        {
            PopupManager.Instance.ShowPopup("ShipCreationPanel");
            PopupManager.Instance.HidePopup("ChinjuUI");
        };

        Debug.Log("[ChinjuUIController] UI 已初始化並隱藏所有面板");
    }
}
