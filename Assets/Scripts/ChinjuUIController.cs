using UnityEngine;
using UnityEngine.UIElements;

public class ChinjuUIController : MonoBehaviour
{
    private GameObject weaponCreatePanelObj;
    private GameObject shipCreatePanelObj;
    private VisualElement chinjuRoot;

    void Start()
    {
        // 取得 ChinjuUI 的 root VisualElement
        var uiDoc = GetComponent<UIDocument>();
        if (uiDoc != null)
        {
            chinjuRoot = uiDoc.rootVisualElement;
        }

        // 綁定主面板按鈕事件
        if (chinjuRoot != null)
        {
            var btnWeapon = chinjuRoot.Q<Button>("btnOpenWeaponCreatePanel");
            if (btnWeapon != null)
                btnWeapon.clicked += OpenWeaponCreatePanel;

            var btnShip = chinjuRoot.Q<Button>("btnOpenShipCreatePanel");
            if (btnShip != null)
                btnShip.clicked += OpenShipCreatePanel;
        }

        // 自動尋找子物件
        weaponCreatePanelObj = transform.Find("WeaponCreatePanel")?.gameObject;
        shipCreatePanelObj = transform.Find("ShipCreatePanel")?.gameObject;

        // 預設隱藏子面板（用 display 控制）
        weaponCreatePanelObj.GetComponent<WeaponCreatePanelController>().Hide();
        shipCreatePanelObj.GetComponent<ShipCreationPanel>().Hide();

        // 預設隱藏主面板
        Debug.Log("[ChinjuUIController] Hide Chinju UI Panel at Start");
        Hide();
    }

    public void Show()
    {
        if (chinjuRoot != null)
        {
            chinjuRoot.style.display = DisplayStyle.Flex;
            Debug.Log("[ChinjuUIController] Show Chinju UI Panel");
        }
    }

    public void Hide()
    {
        if (chinjuRoot != null)
        {
            chinjuRoot.style.display = DisplayStyle.None;
            Debug.Log("[ChinjuUIController] Hide Chinju UI Panel");
        }
    }

    // 只顯示主面板並關閉所有子面板
    public void ShowMainPanelOnly()
    {
        CloseCurrentPanel();
        Show();
    }

    // 開啟武器創建面板
    public void OpenWeaponCreatePanel()
    {
        Debug.Log("[ChinjuUIController] Open Weapon Create Panel");
        CloseCurrentPanel();

        var ctrl = weaponCreatePanelObj?.GetComponent<WeaponCreatePanelController>();
        ctrl?.Show();
        // 隱藏主面板
        Hide();
    }

    // 開啟船艦創建面板
    public void OpenShipCreatePanel()
    {
        Debug.Log("[ChinjuUIController] Open Ship Create Panel");
        CloseCurrentPanel();

        var ctrl = shipCreatePanelObj?.GetComponent<ShipCreationPanel>();
        ctrl?.Show();
        // 隱藏主面板
        Hide();
    }

    // 關閉目前開啟的面板
    public void CloseCurrentPanel()
    {
        Debug.Log("[ChinjuUIController] Close All Sub Panels");

        var weaponCtrl = weaponCreatePanelObj?.GetComponent<WeaponCreatePanelController>();
        weaponCtrl?.Hide();


        var shipCtrl = shipCreatePanelObj?.GetComponent<ShipCreationPanel>();
        shipCtrl?.Hide();
    }

}
