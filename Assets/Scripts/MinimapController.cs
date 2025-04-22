using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class MinimapController : MonoBehaviour
{
    public Camera minimapCamera; // 小地圖攝影機
    public Camera mainCamera;    // 主攝影機
    public RectTransform minimapRect; // 小地圖的 UI RectTransform
    public RectTransform viewRect;    // 用於顯示主攝影機範圍的矩形 UI
    private Vector2 clickPosition; // 儲存點擊位置
    private InputSystem_Actions inputActions;

    void Start()
    {
        inputActions = new InputSystem_Actions();
        inputActions.UI.Click.performed += OnClickMinimap; // 綁定 Click 行為
        inputActions.Enable(); // 啟用輸入系統
    }

    void Update()
    {
        if (mainCamera == null || minimapCamera == null || minimapRect == null || viewRect == null) return;

        // 如果有點擊位置，處理跳轉
        if (clickPosition != Vector2.zero)
        {
            Vector2 localPoint;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(minimapRect, clickPosition, null, out localPoint))
            {
                // 檢查點擊是否在小地圖範圍內
                if (minimapRect.rect.Contains(localPoint))
                {
                    JumpToWorldPosition(localPoint);
                }
            }
            clickPosition = Vector2.zero; // 重置點擊位置
        }

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

    private void JumpToWorldPosition(Vector2 localMousePosition)
    {
        // 將本地座標轉換為 0 到 1 的範圍
        Vector2 viewportPoint = new Vector2(
            (localMousePosition.x - minimapRect.rect.xMin) / minimapRect.rect.width,
            (localMousePosition.y - minimapRect.rect.yMin) / minimapRect.rect.height
        );

        // 將視口座標轉換為世界座標
        Vector3 worldPosition = minimapCamera.ViewportToWorldPoint(new Vector3(viewportPoint.x, viewportPoint.y, minimapCamera.nearClipPlane));

        // 更新主攝影機的位置，確保 X 和 Y 軸都正確更新
        mainCamera.transform.position = new Vector3(worldPosition.x, worldPosition.y, mainCamera.transform.position.z);
    }

    public void OnClickMinimap(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            clickPosition = Mouse.current.position.ReadValue(); // 取得滑鼠點擊位置
        }
    }

    void OnDestroy()
    {
        // 禁用輸入系統，避免資源洩漏
        inputActions.Disable();
        inputActions.Dispose();
    }
}