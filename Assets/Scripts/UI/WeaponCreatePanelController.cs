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

    public VisualTreeAsset weaponCreatePanelUXML;
    public StyleSheet weaponCreatePanelUSS;

    private VisualElement panel;

    void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        panel = weaponCreatePanelUXML.CloneTree();
        panel.styleSheets.Add(weaponCreatePanelUSS);

        var dropdown = panel.Q<DropdownField>("weapon-dropdown");
        var resourceLabel = panel.Q<Label>("resource-label");
        var createButton = panel.Q<Button>("create-button");
        var resultLabel = panel.Q<Label>("result-label");

        // 新增關閉按鈕
        var closeBtn = new Button(() => Hide()) { text = "關閉" };
        closeBtn.style.marginTop = 8;
        panel.Add(closeBtn);

        dropdown.choices = new List<string>();
        foreach (var recipe in weaponRecipes)
            dropdown.choices.Add($"{recipe.name} (花費: {recipe.cost})");
        dropdown.index = 0;

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
                Hide();
            }
            else
            {
                resultLabel.text = "資源不足，無法創建武器。";
            }
        };

        void UpdateResourceLabel()
        {
            resourceLabel.text = $"目前資源：{userResource}";
        }

        root.Add(panel);
        Hide(); // 預設隱藏
    }

    public void Show()
    {
        if (panel != null)
            panel.style.display = DisplayStyle.Flex;
    }

    public void Hide()
    {
        if (panel != null)
            panel.style.display = DisplayStyle.None;
    }
}
