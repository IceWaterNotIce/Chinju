using UnityEngine;
using UnityEngine.UIElements;

public class TimeControlBar : MonoBehaviour
{
    private float[] timeScales = { 0f, 0.1f, 1f, 2f, 3f }; // 支援的時間倍率
    private int currentScaleIndex = 2; // 預設為正常速度 (1x)
    private UIDocument uiDocument;

    void Start()
    {
        uiDocument = GetComponent<UIDocument>();
        if (uiDocument == null)
        {
            Debug.LogError("找不到 UIDocument 組件！");
            return;
        }

        var root = uiDocument.rootVisualElement;
        if (root == null)
        {
            Debug.LogError("無法獲取 UI 根元素！");
            return;
        }

        BindButton(root, "pause-btn", () => SetTimeScale(0));
        BindButton(root, "slow-btn", () => SetTimeScale(1));
        BindButton(root, "normal-btn", () => SetTimeScale(2));
        BindButton(root, "fast-btn", () => SetTimeScale(3));
        BindButton(root, "super-fast-btn", () => SetTimeScale(4));

        UpdateTimeScale();
    }

    private void BindButton(VisualElement root, string buttonName, System.Action action)
    {
        var button = UIHelper.InitializeElement<Button>(root, buttonName);
        if (button != null) button.clicked += action;
    }

    private void UpdateTimeScale()
    {
        Time.timeScale = timeScales[currentScaleIndex];
        Debug.Log($"時間倍率已更新為: {timeScales[currentScaleIndex]}x");
    }

    public void SetTimeScale(int index)
    {
        if (index < 0 || index >= timeScales.Length)
        {
            Debug.LogError("無效的時間倍率索引！");
            return;
        }

        currentScaleIndex = index;
        UpdateTimeScale();
    }
}
