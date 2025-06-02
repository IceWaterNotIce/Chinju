using UnityEngine;
using UnityEngine.UIElements;

public class ServerListPanel : MonoBehaviour
{
    private VisualElement root;
    private ScrollView serverListScrollView;
    private Button backToSettingMenuBtn;

    void OnEnable()
    {
        InitializeUI();
    }

    private void InitializeUI()
    {
        var menuDocument = GetComponent<UIDocument>();
        if (menuDocument == null)
        {
            Debug.LogError("[ServerListPanel] 無法找到 UIDocument 組件");
            return;
        }

        root = menuDocument.rootVisualElement;
        if (root == null)
        {
            Debug.LogError("[ServerListPanel] 無法初始化 root 元素");
            return;
        }

        serverListScrollView = root.Q<ScrollView>("serverListScrollView");
        backToSettingMenuBtn = root.Q<Button>("backToSettingMenuBtn");

        if (backToSettingMenuBtn != null)
        {
            backToSettingMenuBtn.clicked += BackToSettingMenu;
        }

        PopulateServerList();
    }

    private void PopulateServerList()
    {
        if (serverListScrollView == null) return;

        // 假設伺服器列表為靜態數據，未來可改為動態獲取
        string[] servers = { "伺服器 1", "伺服器 2", "伺服器 3" };
        foreach (var server in servers)
        {

        }
    }

    private void BackToSettingMenu()
    {
        PopupManager.Instance.ShowPopup("SettingMenu");
        PopupManager.Instance.HidePopup("ServerListPanel");
    }
}
