using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

public class WeaponCreatePanelController : MonoBehaviour
{
    public List<Weapon> weaponPrefabs = new List<Weapon>(); // 修改為直接存儲 Weapon 預製體

    private VisualElement root;
    private ChinjuUIController chinjuUIController;

    void Awake()
    {
        // 註冊面板到 PopupManager
        PopupManager.Instance.RegisterPopup("WeaponCreatePanel", gameObject);
        Debug.Log("[WeaponCreatePanelController] 面板初始化完成並預設隱藏");
    }

    private void OnEnable()
    {
        InitializeUI();
    }

    private void InitializeUI()
    {
        root = GetComponent<UIDocument>().rootVisualElement;

        var dropdown = UIHelper.InitializeElement<DropdownField>(root, "weapon-dropdown");
        var resourceLabel = UIHelper.InitializeElement<Label>(root, "resource-label");
        var createButton = UIHelper.InitializeElement<Button>(root, "create-button");
        var resultLabel = UIHelper.InitializeElement<Label>(root, "result-label");
        var closeBtn = UIHelper.InitializeElement<Button>(root, "close-button");

        LoadWeaponPrefabs(); // 動態加載武器預製體

        dropdown.choices = new List<string>();
        foreach (var weapon in weaponPrefabs)
            dropdown.choices.Add($"{weapon.Name} (花費: {weapon.Cost})");
        dropdown.index = 0;

        void UpdateResourceLabel()
        {
            var playerData = GameDataController.Instance?.CurrentGameData?.playerData;
            if (playerData != null)
            {
                resourceLabel.text = $"目前金幣：{playerData.Gold}";
            }
            else
            {
                resourceLabel.text = "無法取得玩家資料";
            }
        }

        UpdateResourceLabel();

        createButton.clicked += () =>
        {
            var playerData = GameDataController.Instance?.CurrentGameData?.playerData;
            if (playerData == null)
            {
                Debug.LogError("[WeaponCreatePanelController] 無法取得玩家資料，無法創建武器。");
                return;
            }

            int idx = dropdown.index;
            if (idx < 0 || idx >= weaponPrefabs.Count) return;
            var selectedWeapon = weaponPrefabs[idx];

            if (GameDataController.Instance?.ConsumeResources(selectedWeapon.Cost, 0, 0) ?? false)
            {
                // 實例化武器並添加到玩家資料
                var newWeapon = Instantiate(selectedWeapon);
                if (newWeapon != null)
                {
                    playerData.Weapons.Add(newWeapon.ToWeaponData());
                    resultLabel.text = $"成功創建：{selectedWeapon.Name}！";
                    Debug.Log($"[WeaponCreatePanelController] 成功創建：{selectedWeapon.Name}，剩餘金幣：{playerData.Gold}");
                }

                PopupManager.Instance.HidePopup("WeaponCreatePanel");
            }
            else
            {
                resultLabel.text = "金幣不足，無法創建武器。";
                Debug.LogWarning("[WeaponCreatePanelController] 金幣不足，無法創建武器。");
            }
        };

        closeBtn.clicked += () =>
        {
            PopupManager.Instance.HidePopup("WeaponCreatePanel");
        };

        if (chinjuUIController == null)
            chinjuUIController = FindFirstObjectByType<ChinjuUIController>();
    }

    void LoadWeaponPrefabs()
    {
        weaponPrefabs.Clear();
        var loadedPrefabs = Resources.LoadAll<Weapon>("Prefabs/Weapon");
        weaponPrefabs.AddRange(loadedPrefabs);
    }
}
