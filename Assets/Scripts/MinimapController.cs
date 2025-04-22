using UnityEngine;
using UnityEngine.UI;

public class MinimapController : MonoBehaviour
{
    public Camera minimapCamera; // 小地圖攝影機
    public Camera mainCamera;    // 主攝影機
    public RectTransform minimapRect; // 小地圖的 UI RectTransform
    public RectTransform viewRect;    // 用於顯示主攝影機範圍的矩形 UI

    void Update()
    {
        if (mainCamera == null || minimapCamera == null || minimapRect == null || viewRect == null) return;

        // 獲取主攝影機的視角邊界（世界座標）
        Vector3 bottomLeft = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, mainCamera.nearClipPlane));
        Vector3 topRight = mainCamera.ViewportToWorldPoint(new Vector3(1, 1, mainCamera.nearClipPlane));

        // 將世界座標轉換為小地圖攝影機的視口座標
        Vector3 minimapBottomLeft = minimapCamera.WorldToViewportPoint(bottomLeft);
        Vector3 minimapTopRight = minimapCamera.WorldToViewportPoint(topRight);

        // 計算矩形在小地圖上的位置和大小
        float x = minimapBottomLeft.x * minimapRect.rect.width;
        float y = minimapBottomLeft.y * minimapRect.rect.height;
        float width = (minimapTopRight.x - minimapBottomLeft.x) * minimapRect.rect.width;
        float height = (minimapTopRight.y - minimapBottomLeft.y) * minimapRect.rect.height;

        // 更新 UI 矩形的位置和大小
        viewRect.anchoredPosition = new Vector2(x, y);
        viewRect.sizeDelta = new Vector2(width, height);
    }
}