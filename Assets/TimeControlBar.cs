using UnityEngine;
using UnityEngine.UIElements;

public class TimeControlBar : MonoBehaviour
{
    private float[] timeScales = { 0f, 0.5f, 1f, 2f, 3f }; // 支援的時間倍率
    private int currentScaleIndex = 2; // 預設為正常速度 (1x)
    private UIDocument uiDocument;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
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

        // 綁定按鈕事件
        root.Q<Button>("pause-btn").clicked += () => SetTimeScale(0);
        root.Q<Button>("slow-btn").clicked += () => SetTimeScale(1);
        root.Q<Button>("normal-btn").clicked += () => SetTimeScale(2);
        root.Q<Button>("fast-btn").clicked += () => SetTimeScale(3);
        root.Q<Button>("super-fast-btn").clicked += () => SetTimeScale(4);

        UpdateTimeScale();
    }

    // Update is called once per frame
    void Update()
    {
        // 測試用按鍵控制
        if (Input.GetKeyDown(KeyCode.Alpha1)) SetTimeScale(0); // 暫停
        if (Input.GetKeyDown(KeyCode.Alpha2)) SetTimeScale(1); // 減速
        if (Input.GetKeyDown(KeyCode.Alpha3)) SetTimeScale(2); // 正常速度
        if (Input.GetKeyDown(KeyCode.Alpha4)) SetTimeScale(3); // 加速
        if (Input.GetKeyDown(KeyCode.Alpha5)) SetTimeScale(4); // 超加速
    }

    /// <summary>
    /// 設置時間倍率
    /// </summary>
    /// <param name="index">時間倍率索引</param>
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

    /// <summary>
    /// 更新遊戲時間倍率
    /// </summary>
    private void UpdateTimeScale()
    {
        Time.timeScale = timeScales[currentScaleIndex];
        Debug.Log($"時間倍率已更新為: {timeScales[currentScaleIndex]}x");
    }
}
