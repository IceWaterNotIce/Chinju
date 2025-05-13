using UnityEngine;
using UnityEngine.UIElements;

public class BottomNavBarUIController : MonoBehaviour
{
    private Button toggleWeaponPanelButton; // 切換武器面板按鈕
    private Button toggleShipListPanelButton; // 切換船隻列表面板按鈕
    private Button toggleSettingMenuButton; // 新增：切換設定選單按鈕

    void Start()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        toggleWeaponPanelButton = root.Q<Button>("ToggleWeaponPanelButton");
        if (toggleWeaponPanelButton != null)
        {
            toggleWeaponPanelButton.clicked += ToggleWeaponPanel;
        }
        else
        {
            Debug.LogError("[BottomNavBarUIController] 找不到 'ToggleWeaponPanelButton' 按鈕。");
        }

        toggleShipListPanelButton = root.Q<Button>("ToggleShipListPanelButton");
        if (toggleShipListPanelButton != null)
        {
            toggleShipListPanelButton.clicked += ToggleShipListPanel;
        }
        else
        {
            Debug.LogError("[BottomNavBarUIController] 找不到 'ToggleShipListPanelButton' 按鈕。");
        }

        // 新增：綁定切換設定選單按鈕
        toggleSettingMenuButton = root.Q<Button>("ToggleSettingMenuButton");
        if (toggleSettingMenuButton != null)
        {
            toggleSettingMenuButton.clicked += ToggleSettingMenu;
        }
        else
        {
            Debug.LogError("[BottomNavBarUIController] 找不到 'ToggleSettingMenuButton' 按鈕。");
        }
    }

    private void ToggleWeaponPanel()
    {
        if (PopupManager.Instance.IsPopupVisible("WeaponPanel"))
        {
            PopupManager.Instance.HidePopup("WeaponPanel");
        }
        else
        {
            PopupManager.Instance.ShowPopup("WeaponPanel");
        }
    }

    private void ToggleShipListPanel()
    {
        if (PopupManager.Instance.IsPopupVisible("ShipListPanel"))
        {
            PopupManager.Instance.HidePopup("ShipListPanel");
        }
        else
        {
            PopupManager.Instance.ShowPopup("ShipListPanel");
        }
    }

    // 新增：切換設定選單
    private void ToggleSettingMenu()
    {
        if (PopupManager.Instance.IsPopupVisible("SettingMenu"))
        {
            PopupManager.Instance.HidePopup("SettingMenu");
        }
        else
        {
            PopupManager.Instance.ShowPopup("SettingMenu");
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
