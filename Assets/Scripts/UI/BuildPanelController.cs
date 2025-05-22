using UnityEngine;
using UnityEngine.UIElements;

public class BuildPanelController : MonoBehaviour
{
    private VisualElement buildRoot;

    private void Awake()
    {
        PopupManager.Instance.RegisterPopup("BuildPanel", gameObject);
    }

    private void OnEnable()
    {
        InitializeUI();
    }

    private void InitializeUI()
    {
        var uiDoc = GetComponent<UIDocument>();
        buildRoot = uiDoc?.rootVisualElement;

        var btnBuildShip = buildRoot?.Q<Button>("btnBuildShip");
        if (btnBuildShip != null) btnBuildShip.clicked += () =>
        {
            Debug.Log("[BuildPanelController] 點擊建造船艦");
            // TODO: 實作建造船艦邏輯
            PopupManager.Instance.HidePopup("BuildPanel");
            PopupManager.Instance.ShowPopup("ShipCreatePanel");
        };

        var btnBuildWeapon = buildRoot?.Q<Button>("btnBuildWeapon");
        if (btnBuildWeapon != null) btnBuildWeapon.clicked += () =>
        {
            Debug.Log("[BuildPanelController] 點擊建造武器");
            // TODO: 實作建造武器邏輯
            PopupManager.Instance.HidePopup("BuildPanel");
            PopupManager.Instance.ShowPopup("WeaponCreatePanel");
        };

        var btnClose = buildRoot?.Q<Button>("btnCloseBuildPanel");
        if (btnClose != null) btnClose.clicked += () =>
        {
            PopupManager.Instance.HidePopup("BuildPanel");
            Debug.Log("[BuildPanelController] 點擊關閉建造面板");
        };
    }
}
