using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;

/// <summary>
/// 小地圖管理器類，處理小地圖顯示和控制
/// </summary>
public class MiniMapManager : MonoBehaviour
{
    /// <summary>
    /// 小地圖的縮放級別
    /// </summary>
    private float zoomLevel = 1f;
    
    /// <summary>
    /// 最小縮放級別
    /// </summary>
    private const float MIN_ZOOM = 0.5f;
    
    /// <summary>
    /// 最大縮放級別
    /// </summary>
    private const float MAX_ZOOM = 2f;

    /// <summary>
    /// 小地圖拖動速度
    /// </summary>
    private const float DRAG_SPEED = 0.5f;

    /// <summary>
    /// 小地圖相機
    /// </summary>
    [SerializeField]
    private Camera minimapCamera;

    /// <summary>
    /// 小地圖渲染材質
    /// </summary>
    private RenderTexture minimapRenderTexture;

    private VisualElement minimapContainer;
    private VisualElement minimapView;
    private Button zoomInButton;
    private Button zoomOutButton;

    private Vector2 dragStartPosition;
    private bool isDragging;
    private Vector3 cameraStartPosition;

    private void Awake()
    {
        SetupMinimapCamera();
    }

    /// <summary>
    /// 設置小地圖相機
    /// </summary>
    private void SetupMinimapCamera()
    {
      
    
    }

    /// <summary>
    /// 初始化小地圖
    /// </summary>
    public void Initialize(VisualElement root)
    {
        // 獲取小地圖相關元素
        minimapContainer = root.Q<VisualElement>("minimap-container");
        minimapView = root.Q<VisualElement>("minimap-view");
        zoomInButton = root.Q<Button>("minimap-zoom-in");
        zoomOutButton = root.Q<Button>("minimap-zoom-out");

        if (minimapContainer == null || minimapView == null)
        {
            Debug.LogError("[MiniMapManager] 無法找到小地圖UI元素");
            return;
        }

        // 設置小地圖背景
        if (minimapRenderTexture != null)
        {
            minimapView.style.backgroundImage = new StyleBackground(Background.FromRenderTexture(minimapRenderTexture));
        }

        // 註冊按鈕事件
        zoomInButton?.RegisterCallback<ClickEvent>(OnZoomIn);
        zoomOutButton?.RegisterCallback<ClickEvent>(OnZoomOut);

        // 註冊拖動事件
        minimapContainer.RegisterCallback<MouseDownEvent>(OnStartDrag);
        minimapContainer.RegisterCallback<MouseMoveEvent>(OnDrag);
        minimapContainer.RegisterCallback<MouseUpEvent>(OnEndDrag);
        minimapContainer.RegisterCallback<MouseLeaveEvent>(OnEndDrag);

        // 設置初始縮放
        UpdateZoom();
    }

    /// <summary>
    /// 處理放大按鈕點擊
    /// </summary>
    private void OnZoomIn(ClickEvent evt)
    {
        zoomLevel = Mathf.Min(zoomLevel + 0.2f, MAX_ZOOM);
        UpdateZoom();
        evt.StopPropagation();
    }

    /// <summary>
    /// 處理縮小按鈕點擊
    /// </summary>
    private void OnZoomOut(ClickEvent evt)
    {
        zoomLevel = Mathf.Max(zoomLevel - 0.2f, MIN_ZOOM);
        UpdateZoom();
        evt.StopPropagation();
    }

    /// <summary>
    /// 開始拖動小地圖
    /// </summary>
    private void OnStartDrag(MouseDownEvent evt)
    {
        if (evt.button == 0) // 左鍵點擊
        {
            isDragging = true;
            dragStartPosition = evt.mousePosition;
            cameraStartPosition = minimapCamera.transform.position;
            evt.StopPropagation();
        }
    }

    /// <summary>
    /// 拖動小地圖
    /// </summary>
    private void OnDrag(MouseMoveEvent evt)
    {
        if (isDragging)
        {
            Vector2 delta = (evt.mousePosition - dragStartPosition) * DRAG_SPEED;
            Vector3 newPosition = cameraStartPosition;
            newPosition.x -= delta.x * minimapCamera.orthographicSize / 100f;
            newPosition.z -= delta.y * minimapCamera.orthographicSize / 100f;
            minimapCamera.transform.position = newPosition;
            evt.StopPropagation();
        }
    }

    /// <summary>
    /// 結束拖動
    /// </summary>
    private void OnEndDrag(EventBase evt)
    {
        isDragging = false;
    }

    /// <summary>
    /// 更新小地圖縮放
    /// </summary>
    private void UpdateZoom()
    {
        if (minimapView == null || minimapCamera == null) return;
        minimapCamera.orthographicSize = 10f * (1f / zoomLevel);
        minimapView.style.scale = new StyleScale(new Scale(new Vector3(zoomLevel, zoomLevel, 1)));
    }

    /// <summary>
    /// 更新小地圖上的物件位置
    /// </summary>
    public void UpdateMapObjects()
    {
        // 在這裡實現更新地圖上物件位置的邏輯
        // 例如：遍歷場景中的所有重要物件，並在小地圖上更新它們的位置
    }

    private void OnDestroy()
    {
        if (minimapRenderTexture != null)
        {
            minimapRenderTexture.Release();
        }
    }
} 