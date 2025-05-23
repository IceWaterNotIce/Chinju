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
        root = uiDocument.rootVisualElement;
        nameLabel = UIHelper.InitializeElement<Label>(uiDocument.rootVisualElement, "ship-name");
        levelLabel = UIHelper.InitializeElement<Label>(uiDocument.rootVisualElement, "ship-level");
        healthBar = UIHelper.InitializeElement<VisualElement>(uiDocument.rootVisualElement, "health-bar");
        healthBarFill = UIHelper.InitializeElement<VisualElement>(uiDocument.rootVisualElement, "health-bar-fill");
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
        // 只更新 width，顏色交由 USS 控制
        healthBarFill.style.width = Length.Percent(percent * 100);
        // 若仍需根據血量改變顏色，可保留下方程式，否則移除
        /*
        if (percent > 0.6f)
            healthBarFill.style.backgroundColor = Color.green;
        else if (percent > 0.3f)
            healthBarFill.style.backgroundColor = Color.yellow;
        else
            healthBarFill.style.backgroundColor = Color.red;
        */
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

        // 動態調整字體大小與血條寬度
        AdjustUISizeByCamera();
    }

    private void AdjustUISizeByCamera()
    {
        var cam = Camera.main;
        if (cam == null) return;

        float baseOrtho = 20f; // 基準視野
        float scale = cam.orthographicSize / baseOrtho;

        // 動態字體大小
        int baseNameFontSize = 14;
        int baseLevelFontSize = 12;
        int minFontSize = 8, maxFontSize = 32;
        nameLabel.style.fontSize = Mathf.Clamp(Mathf.RoundToInt(baseNameFontSize * scale), minFontSize, maxFontSize);
        levelLabel.style.fontSize = Mathf.Clamp(Mathf.RoundToInt(baseLevelFontSize * scale), minFontSize, maxFontSize);

        // 動態血條
        float baseBarHeight = 10f;
        float minBarHeight = 5f, maxBarHeight = 20f;
        healthBar.style.height = Mathf.Clamp(baseBarHeight * scale, minBarHeight, maxBarHeight);
    }

    private void OnDestroy()
    {
        // 無需事件解除訂閱
    }
}