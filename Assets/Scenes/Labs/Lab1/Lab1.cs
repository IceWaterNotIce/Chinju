using UnityEngine;
using UnityEngine.UIElements;

public class Lab1 : MonoBehaviour
{
    public UIDocument uiDocument; // 引用 UIDocument
    private VisualElement root;
    private VisualElement currentRect;
    private Vector2 startPos;
    private bool isDrawing = false;
    private Button startDrawButton; // 新增按鈕引用
    private bool canDraw = false;   // 控制是否允許繪製

    void Start()
    {
        root = uiDocument.rootVisualElement; // 獲取 UI 根元素
        startDrawButton = root.Q<Button>("StartDrawButton"); // 獲取按鈕
        if (startDrawButton != null)
        {
            startDrawButton.clicked += EnableDrawing; // 設定按鈕點擊事件
        }
        root.RegisterCallback<PointerDownEvent>(OnPointerDown); // 修正為 PointerDownEvent
        root.RegisterCallback<PointerMoveEvent>(OnPointerMove); // 修正為 PointerMoveEvent
        root.RegisterCallback<PointerUpEvent>(OnPointerUp);     // 修正為 PointerUpEvent
    }

    private void EnableDrawing()
    {
        canDraw = true; // 啟用繪製功能
        Debug.Log("[Lab1] 繪製功能已啟用");
    }

    private void OnPointerDown(PointerDownEvent evt)
    {
        if (!canDraw || evt.button != 0) return; // 檢查是否允許繪製
        startPos = evt.localPosition;
        currentRect = new VisualElement();
        currentRect.AddToClassList("rect"); // 套用矩形樣式
        currentRect.style.position = Position.Absolute;
        currentRect.style.left = startPos.x;
        currentRect.style.top = startPos.y;
        root.Add(currentRect);
        isDrawing = true;
        Debug.Log("[Lab1] 開始繪製矩形");
    }

    private void OnPointerMove(PointerMoveEvent evt)
    {
        if (isDrawing && currentRect != null)
        {
            Vector2 mousePos = evt.localPosition;
            Vector2 size = mousePos - startPos;

            // 設定矩形大小和位置
            currentRect.style.width = Mathf.Abs(size.x);
            currentRect.style.height = Mathf.Abs(size.y);
            currentRect.style.left = Mathf.Min(startPos.x, mousePos.x);
            currentRect.style.top = Mathf.Min(startPos.y, mousePos.y);
        }
    }

    private void OnPointerUp(PointerUpEvent evt)
    {
        if (evt.button == 0 && isDrawing) // 左鍵
        {
            isDrawing = false;
            currentRect = null; // 重置 currentRect 狀態
            canDraw = false; // 繪製完成後禁用繪製功能
            Debug.Log("[Lab1] 繪製結束");
            Debug.Log($"[Lab1] 繪製的矩形位置: {startPos} 到 {evt.localPosition}");
            
        }
    }
}