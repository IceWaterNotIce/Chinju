using UnityEngine;
using UnityEngine.UIElements;

public class BottomNavBarUIController : MonoBehaviour
{
    [SerializeField]
    private GameObject weaponPanel; // 參考武器面板

    private Button toggleWeaponPanelButton; // 切換按鈕

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

    // Update is called once per frame
    void Update()
    {

    }
}
