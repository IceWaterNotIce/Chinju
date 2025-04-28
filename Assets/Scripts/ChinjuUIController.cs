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

        // 自動尋找子物件
        weaponCreatePanelObj = transform.Find("WeaponCreatePanel")?.gameObject;
        shipCreatePanelObj = transform.Find("ShipCreatePanel")?.gameObject;

        // 預設隱藏子面板（用 display 控制）
        SetPanelDisplay(weaponCreatePanelObj, false);
        SetPanelDisplay(shipCreatePanelObj, false);

        // 預設隱藏主面板
        Hide();
    }

    public void Show()
    {
        if (chinjuRoot != null)
            chinjuRoot.style.display = DisplayStyle.Flex;
    }

    public void Hide()
    {
        if (chinjuRoot != null)
            chinjuRoot.style.display = DisplayStyle.None;
        // 同時關閉子面板
        CloseCurrentPanel();
    }

    // 開啟武器創建面板
    public void OpenWeaponCreatePanel()
    {
        CloseCurrentPanel();
        SetPanelDisplay(weaponCreatePanelObj, true);
        var ctrl = weaponCreatePanelObj?.GetComponent<WeaponCreatePanelController>();
        ctrl?.Show();
        // 隱藏主面板
        Hide();
    }

    // 開啟船艦創建面板
    public void OpenShipCreatePanel()
    {
        CloseCurrentPanel();
        SetPanelDisplay(shipCreatePanelObj, true);
        var ctrl = shipCreatePanelObj?.GetComponent<ShipCreationPanel>();
        ctrl?.Show();
        // 隱藏主面板
        Hide();
    }

    // 關閉目前開啟的面板
    public void CloseCurrentPanel()
    {
        SetPanelDisplay(weaponCreatePanelObj, false);
        var weaponCtrl = weaponCreatePanelObj?.GetComponent<WeaponCreatePanelController>();
        weaponCtrl?.Hide();

        SetPanelDisplay(shipCreatePanelObj, false);
        var shipCtrl = shipCreatePanelObj?.GetComponent<ShipCreationPanel>();
        shipCtrl?.Hide();
    }

    // 用 display 控制子面板顯示/隱藏
    private void SetPanelDisplay(GameObject panelObj, bool show)
    {
        if (panelObj == null) return;
        var doc = panelObj.GetComponent<UIDocument>();
        if (doc != null && doc.rootVisualElement != null)
        {
            doc.rootVisualElement.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}
