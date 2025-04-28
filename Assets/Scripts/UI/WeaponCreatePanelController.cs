using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

public class WeaponCreatePanelController : MonoBehaviour
{
    [System.Serializable]
    public class WeaponRecipe
    {
        public string name;
        public int cost;
    }

    public List<WeaponRecipe> weaponRecipes = new List<WeaponRecipe>
    {
        new WeaponRecipe { name = "長劍", cost = 30 },
        new WeaponRecipe { name = "弓箭", cost = 20 },
        new WeaponRecipe { name = "斧頭", cost = 40 }
    };

    private VisualElement root;
    private ChinjuUIController chinjuUIController;

    void Awake()
    {
        root = GetComponent<UIDocument>().rootVisualElement;

        var dropdown = root.Q<DropdownField>("weapon-dropdown");
        var resourceLabel = root.Q<Label>("resource-label");
        var createButton = root.Q<Button>("create-button");
        var resultLabel = root.Q<Label>("result-label");
        var closeBtn = root.Q<Button>("close-button");

        dropdown.choices = new List<string>();
        foreach (var recipe in weaponRecipes)
            dropdown.choices.Add($"{recipe.name} (花費: {recipe.cost})");
        dropdown.index = 0;

        void UpdateResourceLabel()
        {
            var playerData = GameDataController.Instance?.CurrentGameData?.PlayerDatad;
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
            var playerData = GameDataController.Instance?.CurrentGameData?.PlayerDatad;
            if (playerData == null)
            {
                Debug.LogError("[WeaponCreatePanelController] 無法取得玩家資料，無法創建武器。");
                return;
            }

            int idx = dropdown.index;
            if (idx < 0 || idx >= weaponRecipes.Count) return;
            var recipe = weaponRecipes[idx];
            if (playerData.Gold >= recipe.cost)
            {
                playerData.Gold -= recipe.cost;
                playerData.OnResourceChanged?.Invoke();
                UpdateResourceLabel();

                // 新增武器數據到玩家資料
                var newWeapon = new GameData.WeaponData
                {
                    Name = recipe.name,
                    Damage = Random.Range(10, 20), // 假設隨機生成傷害值
                    Range = Random.Range(1.0f, 3.0f), // 假設隨機生成範圍值
                    AttackSpeed = Random.Range(0.5f, 1.5f), // 假設隨機生成攻擊速度
                    CooldownTime = Random.Range(1.0f, 2.0f) // 假設隨機生成冷卻時間
                };
                playerData.Weapons.Add(newWeapon);

                resultLabel.text = $"成功創建：{recipe.name}！";
                Debug.Log($"[WeaponCreatePanelController] 成功創建：{recipe.name}，剩餘金幣：{playerData.Gold}");
                Debug.Log($"[WeaponCreatePanelController] 新增武器數據：{newWeapon.Name}，傷害：{newWeapon.Damage}，範圍：{newWeapon.Range}，攻擊速度：{newWeapon.AttackSpeed}，冷卻時間：{newWeapon.CooldownTime}");

                Hide();
            }
            else
            {
                resultLabel.text = "金幣不足，無法創建武器。";
                Debug.LogWarning("[WeaponCreatePanelController] 金幣不足，無法創建武器。");
            }
        };

        closeBtn.clicked += Hide;

        if (chinjuUIController == null)
            chinjuUIController = FindFirstObjectByType<ChinjuUIController>();

        root.style.display = DisplayStyle.None;
        Debug.Log("[WeaponCreatePanelController] 面板初始化完成並預設隱藏");
    }

    void OnEnable()
    {
        Hide();
    }

    public void Show()
    {
        if (root != null)
        {
            root.style.display = DisplayStyle.Flex;
            Debug.Log($"[WeaponCreatePanelController] 顯示武器創建面板，root.style.display = {root.style.display}");
        }
        else
        {
            Debug.LogError("[WeaponCreatePanelController] 無法顯示武器創建面板，root 為 null");
        }
    }

    public void Hide()
    {
        if (root != null)
        {
            root.style.display = DisplayStyle.None;
            Debug.Log("[WeaponCreatePanelController] 隱藏武器創建面板");
        }
        else
        {
            Debug.LogError("[WeaponCreatePanelController] 無法隱藏武器創建面板，root 為 null");
        }

        if (chinjuUIController != null)
        {
            chinjuUIController.Show();
        }
    }
}
