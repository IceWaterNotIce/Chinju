using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class ShipBaseUI : MonoBehaviour
{
    private VisualElement root;
    private Label nameLabel;
    private Label levelLabel;
    private VisualElement healthBar;
    private VisualElement healthBarFill;
    
    private Ship ship; // 假設有一個基礎的 Ship 類別

    private void Awake()
    {
        InitializeUI();
        // 自動綁定同 GameObject 上的 Ship 組件
        SetShip(GetComponent<Ship>());
    }

    private void InitializeUI()
    {
        // 獲取 UIDocument 組件
        var uiDocument = GetComponent<UIDocument>();
        if (uiDocument == null)
        {
            Debug.LogError("UIDocument component is missing!");
            return;
        }

        // 創建基礎 UI 結構
        root = new VisualElement();
        root.name = "base-ship-ui";
        root.style.flexDirection = FlexDirection.Column;
        root.style.alignItems = Align.Center;
        root.style.position = Position.Absolute;
        
        // 名稱標籤
        nameLabel = new Label();
        nameLabel.name = "ship-name";
        nameLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
        nameLabel.style.fontSize = 14;
        nameLabel.style.color = Color.white;
        root.Add(nameLabel);
        
        // 等級標籤
        levelLabel = new Label();
        levelLabel.name = "ship-level";
        levelLabel.style.fontSize = 12;
        levelLabel.style.color = new Color(0.8f, 0.8f, 0.8f);
        root.Add(levelLabel);
        
        // 生命值條
        healthBar = new VisualElement();
        healthBar.name = "health-bar";
        healthBar.style.width = 60;
        healthBar.style.height = 6;
        healthBar.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.7f);
        healthBar.style.marginTop = 2;
        
        healthBarFill = new VisualElement();
        healthBarFill.name = "health-bar-fill";
        healthBarFill.style.width = Length.Percent(100);
        healthBarFill.style.height = Length.Percent(100);
        healthBarFill.style.backgroundColor = Color.green;
        healthBar.Add(healthBarFill);
        
        root.Add(healthBar);
        
        // 添加到 UIDocument
        uiDocument.rootVisualElement.Add(root);
    }

    public void SetShip(Ship targetShip)
    {
        ship = targetShip;
        if (ship != null)
            UpdateUI();
        // 移除事件訂閱，因為 Ship 沒有這些事件
    }

    private void UpdateUI()
    {
        // 這裡假設 Ship 有 name、level、health 屬性，否則需檢查型別
        string shipName = ship.name;
        int shipLevel = 1;
        float currentHealth = 1, maxHealth = 1;

        // 嘗試轉型為 Warship 或 PlayerShip 以取得更多資訊
        var warship = ship as Warship;
        if (warship != null)
        {
            shipLevel = warship.Level;
            currentHealth = warship.Health;
            maxHealth = warship.MaxHealth;
        }
        else
        {
            // fallback: 只顯示名稱
            shipName = ship.name;
        }

        UpdateName(shipName);
        UpdateLevel(shipLevel);
        UpdateHealth(currentHealth, maxHealth);
    }

    private void UpdateName(string name)
    {
        nameLabel.text = name;
    }

    private void UpdateLevel(int level)
    {
        levelLabel.text = $"Lv.{level}";
    }

    private void UpdateHealth(float current, float max)
    {
        float percent = Mathf.Clamp01(current / max);
        healthBarFill.style.width = Length.Percent(percent * 100);
        
        // 根據生命值百分比改變顏色
        if (percent > 0.6f)
            healthBarFill.style.backgroundColor = Color.green;
        else if (percent > 0.3f)
            healthBarFill.style.backgroundColor = Color.yellow;
        else
            healthBarFill.style.backgroundColor = Color.red;
    }

    private void Update()
    {
        if (ship != null)
        {
            // 使用 UIHelper 綁定 UI 到世界座標
            UIHelper.BindToWorldPosition(
                root,
                ship.transform.position,
                Camera.main,
                true,
                1.5f // 可根據需求調整 Y 偏移
            );
        }
    }

    private void OnDestroy()
    {
        // 無需事件解除訂閱
    }
}