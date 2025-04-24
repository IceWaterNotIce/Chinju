using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;
using System.Collections;

/// <summary>
/// 小地圖管理器類，處理小地圖顯示和控制
/// </summary>
public class MiniMapManager : MonoBehaviour
{
    /// <summary>
    /// 小地圖的縮放級別
    /// </summary>
    private float zoomLevel = 25f;
    
    /// <summary>
    /// 最小縮放級別
    /// </summary>
    private const float MIN_ZOOM = 0.5f;
    
    /// <summary>
    /// 最大縮放級別
    /// </summary>
    private const float MAX_ZOOM = 50f;

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
    /// 主相機引用
    /// </summary>
    [SerializeField]
    private Camera mainCamera;

    /// <summary>
    /// 主相機移動速度
    /// </summary>
    private const float MAIN_CAMERA_MOVE_SPEED = 2.0f;

    /// <summary>
    /// 小地圖渲染材質
    /// </summary>
    private VisualElement minimapContainer;
    private VisualElement minimapView;
    private Button zoomInButton;
    private Button zoomOutButton;

    private Vector2 dragStartPosition;
    private bool isDragging;
    private Vector3 cameraStartPosition;


    private UIDocument uiDocument;

    /// <summary>
    /// 小地圖的RenderTexture
    /// </summary>
    [SerializeField]
    private RenderTexture minimapRenderTexture;

    private void Awake()
    {
        SetupMinimapCamera();
    }

    private void Start()
    {
        // 獲取 UIDocument 組件
        uiDocument = GetComponent<UIDocument>();
        if (uiDocument == null)
        {
            Debug.LogError("[MiniMapManager] UIDocument component not found!");
            return;
        }

        // 等待一幀確保 UI 已經準備好
        StartCoroutine(InitializeAfterUIReady());
    }

    private System.Collections.IEnumerator InitializeAfterUIReady()
    {
        yield return new WaitForEndOfFrame();
        Initialize(uiDocument.rootVisualElement);
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
        if (root == null)
        {
            Debug.LogError("[MiniMapManager] Root VisualElement is null!");
            return;
        }

        // 獲取小地圖相關元素
        minimapContainer = root.Q<VisualElement>("minimap-container");
        minimapView = root.Q<VisualElement>("minimap");
        zoomInButton = root.Q<Button>("minimap-zoom-in");
        zoomOutButton = root.Q<Button>("minimap-zoom-out");

        if (minimapContainer == null || minimapView == null)
        {
            Debug.LogError("[MiniMapManager] 無法找到小地圖UI元素");
            return;
        }

        Debug.Log("[MiniMapManager] UI Elements found: " +
                  $"Container: {minimapContainer != null}, " +
                  $"View: {minimapView != null}, " +
                  $"ZoomIn: {zoomInButton != null}, " +
                  $"ZoomOut: {zoomOutButton != null}");


        // 註冊按鈕事件
        if (zoomInButton != null)
        {
            zoomInButton.RegisterCallback<ClickEvent>(OnZoomIn);
            Debug.Log("[MiniMapManager] Zoom In button registered");
        }
        
        if (zoomOutButton != null)
        {
            zoomOutButton.RegisterCallback<ClickEvent>(OnZoomOut);
            Debug.Log("[MiniMapManager] Zoom Out button registered");
        }

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
        Debug.Log("[MiniMapManager] OnZoomIn");
        zoomLevel = Mathf.Min(zoomLevel + 5f, MAX_ZOOM);
        UpdateZoom();
        evt.StopPropagation();
    }

    /// <summary>
    /// 處理縮小按鈕點擊
    /// </summary>
    private void OnZoomOut(ClickEvent evt)
    {
        Debug.Log("[MiniMapManager] OnZoomOut");
        zoomLevel = Mathf.Max(zoomLevel - 5f, MIN_ZOOM);
        UpdateZoom();
        evt.StopPropagation();
    }

    /// <summary>
    /// 將小地圖上的點轉換為世界坐標
    /// </summary>
    private Vector3 ConvertMinimapToWorldPosition(Vector2 minimapPosition)
    {
        // 獲取小地圖容器的尺寸
        Vector2 containerSize = minimapContainer.worldBound.size;
        
        // 計算點擊位置相對於小地圖容器的比例
        Vector2 normalizedPosition = new Vector2(
            (minimapPosition.x - minimapContainer.worldBound.x) / containerSize.x,
            (minimapPosition.y - minimapContainer.worldBound.y) / containerSize.y
        );

        // 計算小地圖相機視野範圍
        float cameraHeight = minimapCamera.orthographicSize * 2;
        float cameraWidth = cameraHeight * minimapCamera.aspect;

        // 計算世界空間位置
        Vector3 cameraPosition = minimapCamera.transform.position;
        Vector3 worldPosition = new Vector3(
            cameraPosition.x - (cameraWidth / 2) + (cameraWidth * normalizedPosition.x),
            mainCamera.transform.position.y, // 保持當前高度
            cameraPosition.z - (cameraHeight / 2) + (cameraHeight * normalizedPosition.y)
        );

        return worldPosition;
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
            // 直接移動到點擊位置
            if (mainCamera != null)
            {
                Vector3 targetPosition = ConvertMinimapToWorldPosition(evt.mousePosition);
                mainCamera.transform.position = new Vector3(targetPosition.x, mainCamera.transform.position.y, targetPosition.z);
            }
            evt.StopPropagation();
        }
    }

    /// <summary>
    /// 拖動小地圖
    /// </summary>
    private void OnDrag(MouseMoveEvent evt)
    {
        if (isDragging && mainCamera != null)
        {
            // 直接更新到當前鼠標位置
            Vector3 targetPosition = ConvertMinimapToWorldPosition(evt.mousePosition);
            mainCamera.transform.position = new Vector3(targetPosition.x, mainCamera.transform.position.y, targetPosition.z);
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

        minimapCamera.orthographicSize = zoomLevel;
    }

    /// <summary>
    /// 更新小地圖上的物件位置
    /// </summary>
    public void UpdateMapObjects()
    {
        // 在這裡實現更新地圖上物件位置的邏輯
        // 例如：遍歷場景中的所有重要物件，並在小地圖上更新它們的位置
    }

    
} 