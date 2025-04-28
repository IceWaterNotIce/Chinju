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

    // 假設玩家資源
    public int userResource = 100;

    public List<WeaponRecipe> weaponRecipes = new List<WeaponRecipe>
    {
        new WeaponRecipe { name = "長劍", cost = 30 },
        new WeaponRecipe { name = "弓箭", cost = 20 },
        new WeaponRecipe { name = "斧頭", cost = 40 }
    };

    private VisualElement root; // 修改為 root
    private ChinjuUIController chinjuUIController;

    void Awake()
    {
        root = GetComponent<UIDocument>().rootVisualElement; // 修改為 root

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
            resourceLabel.text = $"目前資源：{userResource}";
        }

        UpdateResourceLabel();

        createButton.clicked += () =>
        {
            int idx = dropdown.index;
            if (idx < 0 || idx >= weaponRecipes.Count) return;
            var recipe = weaponRecipes[idx];
            if (userResource >= recipe.cost)
            {
                userResource -= recipe.cost;
                UpdateResourceLabel();
                resultLabel.text = $"成功創建：{recipe.name}！";
                Debug.Log($"[WeaponCreatePanelController] 成功創建：{recipe.name}，剩餘資源：{userResource}");
                Hide();
            }
            else
            {
                resultLabel.text = "資源不足，無法創建武器。";
                Debug.LogWarning("[WeaponCreatePanelController] 資源不足，無法創建武器。");
            }
        };

        closeBtn.clicked += Hide; // 綁定關閉按鈕的點擊事件

        // 自動尋找 ChinjuUIController
        if (chinjuUIController == null)
            chinjuUIController = FindFirstObjectByType<ChinjuUIController>();

        root.style.display = DisplayStyle.None; // 修改為控制 root
        Debug.Log("[WeaponCreatePanelController] 面板初始化完成並預設隱藏");
    }

    void OnEnable()
    {
        // 僅控制顯示狀態，不再 clone panel
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
