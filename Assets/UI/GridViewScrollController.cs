using UnityEngine;
using UnityEngine.UIElements;

public class GridViewScrollController : MonoBehaviour
{
    public UIDocument uiDocument;
    public float scrollSpeed = 50f;

    private VisualElement gridContainer;
    private Scroller scroller;
    private float gridHeight;
    private float viewportHeight;

    void OnEnable()
    {
        var root = uiDocument.rootVisualElement;
        gridContainer = root.Q<VisualElement>("GridContainer");
        scroller = root.Q<Scroller>();

        // 計算GridContainer總高度與Viewport高度
        gridHeight = gridContainer.resolvedStyle.height;
        var gridViewport = root.Q<VisualElement>("GridViewport");
        viewportHeight = gridViewport.resolvedStyle.height;

        // 綁定滾動條事件
        scroller.valueChanged += OnScrollValueChanged;

        // 綁定滑鼠滾輪事件
        gridViewport.RegisterCallback<WheelEvent>(OnWheel);
    }

    void OnDisable()
    {
        if (scroller != null)
            scroller.valueChanged -= OnScrollValueChanged;
        var root = uiDocument.rootVisualElement;
        var gridViewport = root.Q<VisualElement>("GridViewport");
        if (gridViewport != null)
            gridViewport.UnregisterCallback<WheelEvent>(OnWheel);
    }

    private void OnWheel(WheelEvent evt)
    {
        float delta = evt.delta.y * scrollSpeed;
        scroller.value = Mathf.Clamp(scroller.value + delta, scroller.lowValue, scroller.highValue);
        evt.StopPropagation();
    }

    private void OnScrollValueChanged(float value)
    {
        // 計算GridContainer的top位置
        float maxOffset = Mathf.Max(0, gridHeight - viewportHeight);
        float top = -Mathf.Lerp(0, maxOffset, value / scroller.highValue);
        gridContainer.style.top = top;
    }
}
