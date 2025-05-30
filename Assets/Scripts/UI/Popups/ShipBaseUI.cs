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

    [SerializeField] private float hideUIAreaThreshold = 30f; // 可配置相機閾值
    [SerializeField] private float baseFontSize = 14f; // 基準字體大小
    [SerializeField] private float baseBarHeight = 10f; // 基準血條高度
    [SerializeField] private float minFontSize = 8f, maxFontSize = 32f; // 字體大小範圍
    [SerializeField] private float minBarHeight = 5f, maxBarHeight = 20f; // 血條高度範圍

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

    private void SubscribeToShipEvents()
    {
        if (ship != null)
        {
            ship.OnHealthChanged += (currentHealth) => UpdateHealth(currentHealth, ship.MaxHealth);
        }
        if( GetComponent<Warship>() != null)
        {
            Warship warship = GetComponent<Warship>();
            // 假設 Warship 有 OnLevelChanged 事件
            warship.OnLevelChanged += (newLevel) => UpdateLevel(newLevel);
        }
    }

    private void UnsubscribeFromShipEvents()
    {
        if (ship != null)
        {
            ship.OnHealthChanged -= (currentHealth) => UpdateHealth(currentHealth, ship.MaxHealth);
        }
        if (GetComponent<Warship>() != null)
        {
            Warship warship = GetComponent<Warship>();
            // 假設 Warship 有 OnLevelChanged 事件
            warship.OnLevelChanged -= (newLevel) => UpdateLevel(newLevel);
        }
    }

    public void SetShip(Ship targetShip)
    {
        UnsubscribeFromShipEvents();
        ship = targetShip;
        SubscribeToShipEvents();
        if (ship != null)
            UpdateUI();
        // 移除事件訂閱，因為 Ship 沒有這些事件
    }

    private void UpdateUI()
    {
        // 這裡假設 Ship 有 name、level、health 屬性，否則需檢查型別
        string shipName = ship.name;
        int shipLevel = ship as Warship ? ((Warship)ship).Level : 1; // 假設 Warship 有 Level 屬性
        float currentHealth = ship.Health;
        float maxHealth = ship.MaxHealth;

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
    }

    private void Update()
    {
        var cam = Camera.main;
        if (ship != null && cam != null)
        {
            if (cam.orthographicSize > hideUIAreaThreshold)
            {
                root.style.visibility = Visibility.Hidden;
            }
            else
            {
                Collider2D collider = ship.GetComponent<Collider2D>();
                float yOffset = 0.1f;
                if (collider != null)
                {
                    yOffset = collider.bounds.extents.y + 0.01f; // 確保 UI 在船隻上方
                }

                // 判斷船隻是否在螢幕內
                Vector3 screenPos = cam.WorldToViewportPoint(ship.transform.position);
                bool isOnScreen = screenPos.z > 0 &&
                                  screenPos.x >= 0 && screenPos.x <= 1 &&
                                  screenPos.y >= 0 && screenPos.y <= 1;

                if (isOnScreen)
                {
                    // 使用 UIHelper 綁定 UI 到世界座標
                    UIHelper.BindToWorldPosition(
                        root,
                        ship.transform.position,
                        cam,
                        true,
                        yOffset
                    );
                    root.style.visibility = Visibility.Visible;
                }
                else
                {
                    root.style.visibility = Visibility.Hidden;
                }
            }
        }

        // 動態調整字體大小與血條寬度
        AdjustUISizeByCamera();
    }

    private void AdjustUISizeByCamera()
    {
        var cam = Camera.main;
        if (cam == null) return;

        float scale = cam.orthographicSize / baseFontSize;

        nameLabel.style.fontSize = Mathf.Clamp(Mathf.RoundToInt(baseFontSize * scale), minFontSize, maxFontSize);
        levelLabel.style.fontSize = Mathf.Clamp(Mathf.RoundToInt(baseFontSize * scale * 0.85f), minFontSize, maxFontSize);

        healthBar.style.height = Mathf.Clamp(baseBarHeight * scale, minBarHeight, maxBarHeight);
    }

    private void OnDestroy()
    {
        UnsubscribeFromShipEvents();
    }
}