using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(Camera))]
public class CameraBound2D : MonoBehaviour
{
    [Header("Tilemap Settings")]
    public Tilemap targetTilemap; // 拖入要作為邊界的Tilemap
    public float padding = 0.5f; // 邊界內縮緩衝值

    [Header("Movement Settings")]
    public float baseMoveSpeed = 5f; // 基礎移動速度
    public float smoothTime = 0.2f; // 平滑移動時間

    [Header("Zoom Settings")]
    public float zoomSpeed = 2f; // 縮放速度
    public float minZoom = 3f;   // 最小縮放
    public float maxZoom = 10f;  // 最大縮放
    public float zoomSmoothTime = 0.2f; // 平滑縮放時間
    InputAction _middleMouseAction;
    private Camera cam;
    private Bounds mapBounds;
    private float camOrthoSize;
    private float camRatio;

    private Vector2 moveInput; // 儲存移動輸入
    private float zoomInput;  // 儲存縮放輸入
    private Vector2 mouseWheelMoveInput = Vector2.zero; // 新增：滑鼠滾輪移動輸入

    // 新增：滑鼠中鍵拖曳相關
    private bool isMiddleMouseDown = false;
    private Vector2 lastMousePosition;
    private Vector2 mouseDragDelta;

    private Vector3 velocity = Vector3.zero; // 用於平滑移動
    private float zoomVelocity = 0f; // 用於平滑縮放

    private Transform followTarget; // 新增：跟隨目標

    private void Awake()
    {
        _middleMouseAction = new InputAction("MiddleMouse", binding: "<Mouse>/middleButton");
        _middleMouseAction.performed += ctx => isMiddleMouseDown = true;
        _middleMouseAction.canceled += ctx => isMiddleMouseDown = false;
        _middleMouseAction.Enable();
    }

    void Start()
    {
        cam = GetComponent<Camera>();
        StartCoroutine(InitializeBounds()); // 確保Tilemap已經初始化
    }

    // 計算Tilemap的有效邊界（只考慮有圖塊的區域）
    private IEnumerator InitializeBounds()
    {
        yield return new WaitForEndOfFrame();
        CalculateTilemapBounds();
    }

    private void CalculateTilemapBounds()
    {
        if (targetTilemap == null)
        {
            Debug.LogError("Tilemap not assigned! Please assign a Tilemap to the CameraBound2D script.");
            return;
        }

        // 設定一個超大範圍
        mapBounds = new Bounds(Vector3.zero, new Vector3(99999, 99999, 0));

        if (cam == null)
        {
            cam = GetComponent<Camera>();
            if (cam == null)
            {
                Debug.LogError("Camera component is missing! Please ensure the CameraBound2D script is attached to a GameObject with a Camera.");
                return;
            }
        }
        camOrthoSize = cam.orthographicSize;
        camRatio = (float)Screen.width / Screen.height;

        // Debug.Log($"Map Bounds: {mapBounds.min} to {mapBounds.max}");
    }

    void Update()
    {
        HandleMovement(); // 處理移動輸入
        HandleZoom();     // 處理縮放輸入

        // 每幀重置滑鼠滾輪移動輸入
        mouseWheelMoveInput = Vector2.zero;

        // 每幀重置滑鼠拖曳移動量
        mouseDragDelta = Vector2.zero;
    }

    void LateUpdate()
    {
        if (followTarget != null)
        {
            // 改用 Lerp 以獲得更平滑的跟隨效果
            Vector3 targetPosition = new Vector3(followTarget.position.x, followTarget.position.y, transform.position.z);
            float lerpSpeed = 8f; // 可依需求調整
            transform.position = Vector3.Lerp(transform.position, targetPosition, lerpSpeed * Time.deltaTime);
            return;
        }

        if (targetTilemap == null) return;

        // 註解掉以下 clamp 限制，讓攝影機可自由移動
        /*
        float camWidth = cam.orthographicSize * camRatio;
        float camHeight = cam.orthographicSize;

        transform.position = new Vector3(
            Mathf.Clamp(transform.position.x, mapBounds.min.x + camWidth + padding, mapBounds.max.x - camWidth - padding),
            Mathf.Clamp(transform.position.y, mapBounds.min.y + camHeight + padding, mapBounds.max.y - camHeight - padding),
            transform.position.z
        );
        */
    }

    // 處理移動輸入
    private void HandleMovement()
    {
        // 根據攝影機縮放比例動態調整移動速度
        float dynamicMoveSpeed = baseMoveSpeed * (cam.orthographicSize / minZoom);

        // 滑鼠中鍵拖曳時，直接用滑鼠移動量平移攝影機
        if (isMiddleMouseDown)
        {
            // 依需求可調整靈敏度
            float dragSensitivity = 0.01f * cam.orthographicSize;
            Vector3 dragMove = new Vector3(-mouseDragDelta.x, -mouseDragDelta.y, 0) * dragSensitivity;
            transform.position += dragMove;
        }
        else
        {
            // 鍵盤/搖桿等移動
            Vector3 targetPosition = transform.position + new Vector3(moveInput.x, moveInput.y, 0) * dynamicMoveSpeed * Time.deltaTime;
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
        }
    }

    // 處理縮放輸入
    private void HandleZoom()
    {
        if (zoomInput != 0)
        {
            // 計算目標縮放大小
            float targetZoom = cam.orthographicSize - zoomInput * zoomSpeed * Time.deltaTime;
            targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);

            // 平滑縮放到目標大小
            cam.orthographicSize = Mathf.SmoothDamp(cam.orthographicSize, targetZoom, ref zoomVelocity, zoomSmoothTime);
        }
    }

    // 當Tilemap動態變化時重新計算邊界
    public void RefreshBounds()
    {
        CalculateTilemapBounds();

        // 確保攝影機位置在更新邊界後立即生效
        if (followTarget == null)
        {
            LateUpdate();
        }
    }

    // 新增 Input System 的輸入回調
    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>(); // 取得移動輸入
    }

    public void OnZoom(InputValue value)
    {
        zoomInput = value.Get<Vector2>().y; // 取得縮放輸入
    }

    // 新增 Input System 的滑鼠移動回調
    public void OnMouseDelta(InputValue value)
    {
        if (isMiddleMouseDown)
        {
            Vector2 delta = value.Get<Vector2>();
            mouseDragDelta = delta;
        }
    }

    // 新增：設置跟隨目標的方法
    public void FollowTarget(Transform target)
    {
        followTarget = target;
    }

    // 新增：停止跟隨的方法
    public void StopFollowing()
    {
        followTarget = null;
    }

    public void RefreshCameraPosition()
    {
        LateUpdate(); // 調用 LateUpdate 以立即刷新攝影機位置
    }
}