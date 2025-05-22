using UnityEngine;
using UnityEngine.UIElements;

public class UIDistanceMarker : MonoBehaviour
{
    public Camera targetCamera;
    public float worldDistance = 1f; // 
    private VisualElement root;
    private VisualElement line;
    private Label label;

    void OnEnable()
    {
        var uiDoc = GetComponent<UIDocument>();
        if (uiDoc == null)
        {
            Debug.LogError("[UIDistanceMarker] 需要 UIDocument 組件");
            return;
        }
        root = uiDoc.rootVisualElement;
        // 使用 UIHelper 取得 UI 元素
        line = UIHelper.InitializeElement<VisualElement>(root, "distance-line");
        label = UIHelper.InitializeElement<Label>(root, "distance-label");
        UpdateMarker();
    }

    void Update()
    { 
        UpdateMarker();
    }

    void UpdateMarker()
    {
        if (targetCamera == null || line == null || label == null) return;

        // 取得 1km 在世界座標的兩點
        Vector3 worldStart = targetCamera.transform.position;
        Vector3 worldEnd = worldStart + targetCamera.transform.right * worldDistance;

        // 轉換為螢幕座標
        Vector3 screenStart = targetCamera.WorldToScreenPoint(worldStart);
        Vector3 screenEnd = targetCamera.WorldToScreenPoint(worldEnd);

        float pixelLength = Mathf.Abs(screenEnd.x - screenStart.x);

        // 設定 UI 線條長度
        line.style.width = pixelLength;
        label.text = "1 km";

        // 可選：根據需求調整 UI 位置
        root.style.left = 20;
        root.style.bottom = 40;
    }
}
