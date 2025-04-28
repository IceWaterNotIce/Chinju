using UnityEngine;
using UnityEngine.UIElements;

public class ChinjuUIController : MonoBehaviour
{
    public GameObject weaponCreatePanelObj;
    public GameObject shipCreatePanelObj;
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

        // 檢查子面板是否找到
        if (weaponCreatePanelObj == null)
        {
            Debug.LogError("[ChinjuUIController] 無法找到 WeaponCreatePanel 子物件！");
        }
        if (shipCreatePanelObj == null)
        {
            Debug.LogError("[ChinjuUIController] 無法找到 ShipCreatePanel 子物件！");
        }

        // 預設隱藏子面板（用 display 控制）
        weaponCreatePanelObj?.GetComponent<WeaponCreatePanelController>()?.Hide();
        shipCreatePanelObj?.GetComponent<ShipCreationPanel>()?.Hide();

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
            Debug.Log("[ChinjuUIController] " + chinjuRoot.style.display);
        }
    }

    // 只顯示主面板並關閉所有子面板
    public void ToggleMainPanelOnly()
    {
        if (chinjuRoot != null)
        {
            if (chinjuRoot.style.display == DisplayStyle.None)
            {
                chinjuRoot.style.display = DisplayStyle.Flex;
                Debug.Log("[ChinjuUIController] Show Main Panel Only");
            }
            else
            {
                chinjuRoot.style.display = DisplayStyle.None;
                Debug.Log("[ChinjuUIController] Hide Main Panel Only");
            }
        }
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
