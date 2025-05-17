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

        // 新增：全部加滿燃料按鈕
        var btnFillAllFuel = UIHelper.InitializeElement<Button>(chinjuRoot, "btnFillAllFuel");
        if (btnFillAllFuel != null) btnFillAllFuel.clicked += FillAllPlayerShipFuel;

        Debug.Log("[ChinjuUIController] UI 已初始化並隱藏所有面板");
    }

    // 新增：全部加滿燃料的實作
    private void FillAllPlayerShipFuel()
    {
        var gameData = GameDataController.Instance.CurrentGameData;
        if (gameData == null || gameData.playerData == null)
        {
            Debug.LogWarning("[ChinjuUIController] 無法加滿燃料，GameData 為 null");
            return;
        }

        float totalOilNeeded = 0f;
        foreach (var ship in gameData.playerData.Ships)
        {
            float need = Mathf.Max(0, ship.MaxFuel - ship.CurrentFuel);
            totalOilNeeded += need;
        }

        if (totalOilNeeded <= 0f)
        {
            Debug.Log("[ChinjuUIController] 所有船艦燃料已滿");
            return;
        }

        if (gameData.playerData.Oils < totalOilNeeded)
        {
            Debug.Log("[ChinjuUIController] 石油不足，無法全部加滿燃料");
            // 可加提示UI
            return;
        }

        // 扣除石油並加滿燃料
        gameData.playerData.Oils -= totalOilNeeded;
        foreach (var ship in gameData.playerData.Ships)
        {
            ship.CurrentFuel = ship.MaxFuel;
            Debug.Log($"[ChinjuUIController] {ship.Name} 燃料已加滿");
        }

        GameDataController.Instance.TriggerResourceChanged();
        Debug.Log($"[ChinjuUIController] 已消耗 {totalOilNeeded} 石油，全部船艦燃料加滿");
    }
}
