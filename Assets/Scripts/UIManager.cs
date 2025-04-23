using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// UI 管理器類，負責管理所有 UI 相關操作
/// </summary>
public class UIManager : Singleton<UIManager>
{
    private VisualElement root;
    private Label goldLabel;
    private Label oilLabel;
    private Label cubeLabel;

    protected override void Awake()
    {
        base.Awake();
        InitializeUI();
    }

    /// <summary>
    /// 初始化 UI 系統
    /// </summary>
    private void InitializeUI()
    {
        try
        {
            // 載入 UI 資源
            var mainUIDocument = GetComponent<UIDocument>();
            if (mainUIDocument == null)
            {
                mainUIDocument = gameObject.AddComponent<UIDocument>();
            }

            var visualTree = Resources.Load<VisualTreeAsset>("UI/MainUI");
            if (visualTree == null)
            {
                Debug.LogError("[UIManager] 無法載入 UI/MainUI.uxml");
                return;
            }

            var styleSheet = Resources.Load<StyleSheet>("UI/CommonStyles");
            if (styleSheet == null)
            {
                Debug.LogError("[UIManager] 無法載入 UI/CommonStyles.uss");
                return;
            }

            // 設置 UI Document
            mainUIDocument.visualTreeAsset = visualTree;
            root = mainUIDocument.rootVisualElement;
            root.styleSheets.Add(styleSheet);
            
            // 獲取資源顯示標籤
            goldLabel = root.Q<Label>("goldLabel");
            oilLabel = root.Q<Label>("oilLabel");
            cubeLabel = root.Q<Label>("cubeLabel");

            Debug.Log("[UIManager] UI初始化成功");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[UIManager] UI初始化失敗: {e.Message}\n{e.StackTrace}");
        }
    }

    /// <summary>
    /// 更新資源顯示
    /// </summary>
    public void UpdateResourceDisplay(int gold, int oil, int cube)
    {
        if (goldLabel != null) goldLabel.text = $"金幣: {gold}";
        if (oilLabel != null) oilLabel.text = $"石油: {oil}";
        if (cubeLabel != null) cubeLabel.text = $"方塊: {cube}";
    }

    /// <summary>
    /// 顯示提示訊息
    /// </summary>
    public void ShowMessage(string message, MessageType type = MessageType.Info)
    {
        var messageContainer = root.Q<VisualElement>("messageContainer");
        if (messageContainer == null) return;

        var messageElement = new Label(message)
        {
            name = "message"
        };
        
        messageElement.AddToClassList("message");
        messageElement.AddToClassList($"message-{type.ToString().ToLower()}");

        messageContainer.Add(messageElement);
        
        // 3秒後自動移除訊息
        messageElement.schedule.Execute(() => {
            messageContainer.Remove(messageElement);
        }).StartingIn(3000);
    }
}

/// <summary>
/// 訊息類型枚舉
/// </summary>
public enum MessageType
{
    Info,
    Warning,
    Error,
    Success
} 