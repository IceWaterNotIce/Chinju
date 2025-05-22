using UnityEngine;
using UnityEngine.UIElements;

public class DamagePopupManager : MonoBehaviour
{
    [SerializeField] private UIDocument _uiDocument;
    [SerializeField] private VisualTreeAsset _damagePopupTemplate;

    public static DamagePopupManager Instance { get; private set; }

    private void Awake() {
        Instance = this;
    }

    public void CreateDamagePopup(int damage, Vector3 worldPos, bool isCritical = false) {
        // 世界座標轉螢幕座標
        Vector2 screenPos = RuntimePanelUtils.CameraTransformWorldToPanel(
            _uiDocument.rootVisualElement.panel, worldPos, Camera.main
        );

        // 實例化 UXML
        VisualElement popup = _damagePopupTemplate.Instantiate();
        _uiDocument.rootVisualElement.Add(popup);

        // 創建新的 GameObject 並添加控制腳本
        GameObject popupGO = new GameObject("DamagePopup");
        DamagePopupUI popupUI = popupGO.AddComponent<DamagePopupUI>();
        popupUI.Init(popup, damage, screenPos, isCritical);
    }
}