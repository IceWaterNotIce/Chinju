using UnityEngine;

public class ShipUI : MonoBehaviour {
    // ...existing code...

    private Ship ship;

    private void Start() {
        ship = FindObjectOfType<Ship>();
        if (ship != null) {
            ship.OnShipDataChanged += UpdateUI;
            UpdateUI(); // 初始化 UI
        }
    }

    private void OnDestroy() {
        if (ship != null) {
            ship.OnShipDataChanged -= UpdateUI;
        }
    }

    private void UpdateUI() {
        // 更新 UI 元素，例如：
        // healthBar.SetValue(ship.Health);
        // fuelBar.SetValue(ship.Fuel);
    }

    // ...existing code...
}