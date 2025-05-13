using UnityEngine;
using UnityEngine.UIElements;

public class BottomNavBarUIController : MonoBehaviour
{
    [SerializeField]
    private GameObject weaponPanel; // 參考武器面板

    private Button toggleWeaponPanelButton; // 切換武器面板按鈕
    private Button toggleShipListPanelButton; // 切換船隻列表面板按鈕

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

    // Update is called once per frame
    void Update()
    {

    }
}
