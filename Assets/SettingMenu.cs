using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem; // 新增：使用新輸入系統

public class SettingMenu : MonoBehaviour
{
    private VisualElement root;
    private VisualElement settingsPanel;
    private StyleSheet styleSheet;
    private Button closeButton;
    private VisualElement menuPanel;
    private Button continueButton;
    private Button saveGameButton;
    private Button loadGameButton;
    private Button exitGameButton;
    private Button newGameButton; // 新增

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            ToggleMenu();
        }
    }

    void Start()
    {
        InitializeUI();
    }

    private void InitializeUI()
    {
        try
        {
            var menuDocument = GetComponent<UIDocument>();
            if (menuDocument == null)
            {
                Debug.LogError("[SettingMenu] 無法找到 UIDocument 組件");
                return;
            }

            root = menuDocument.rootVisualElement; // 確保 root 被正確初始化
            if (root == null)
            {
                Debug.LogError("[SettingMenu] 無法初始化 root 元素");
                return;
            }

            menuPanel = root.Q<VisualElement>("menu-panel");
            continueButton = root.Q<Button>("continueButton");
            saveGameButton = root.Q<Button>("saveGameButton");
            loadGameButton = root.Q<Button>("loadGameButton");
            exitGameButton = root.Q<Button>("exitGameButton");
            newGameButton = root.Q<Button>("newGameButton"); // 新增

            RegisterButtonCallbacks();
            HideGameMenu();
            Debug.Log("[SettingMenu] UI初始化成功");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SettingMenu] UI初始化失敗: {e.Message}\n{e.StackTrace}");
        }
    }

    private void RegisterButtonCallbacks()
    {
        continueButton.clicked += OnContinueButtonClicked;
        saveGameButton.clicked += OnSaveGameButtonClicked;
        loadGameButton.clicked += OnLoadGameButtonClicked;
        exitGameButton.clicked += OnExitGameButtonClicked;
        if (newGameButton != null) newGameButton.clicked += OnNewGameButtonClicked; // 新增
    }

    private void ToggleMenu()
    {
        if (menuPanel == null)
        {
            Debug.LogError("[SettingMenu] menuPanel 未初始化");
            return;
        }

        if (root.style.display == DisplayStyle.Flex)
        {
            HideGameMenu();
        }
        else
        {
            ShowGameMenu();
        }
    }

    public void ShowGameMenu()
    {
        if (menuPanel == null)
        {
            Debug.LogError("[SettingMenu] 無法顯示選單：選單引用丟失");
            return;
        }

        root.style.display = DisplayStyle.Flex;
        Debug.Log("[SettingMenu] 顯示遊戲選單");
    }

    public void HideGameMenu()
    {
        if (menuPanel == null)
        {
            Debug.LogError("[SettingMenu] 無法隱藏選單：選單引用丟失");
            return;
        }

        root.style.display = DisplayStyle.None;
        Debug.Log("[SettingMenu] 隱藏遊戲選單");
    }

    private void OnContinueButtonClicked()
    {
        Debug.Log("[SettingMenu] 繼續遊戲");
        HideGameMenu();
    }

    private void OnSaveGameButtonClicked()
    {
        Debug.Log("[SettingMenu] 儲存遊戲");
        if (GameDataController.Instance != null)
            GameManager.Instance.SaveGame(GameDataController.Instance.CurrentGameData);
    }

    private void OnLoadGameButtonClicked()
    {
        Debug.Log("[SettingMenu] 載入遊戲");
        var loadedData = GameManager.Instance.LoadGame();
        if (loadedData != null)
        {
            if (GameDataController.Instance != null)
                GameDataController.Instance.CurrentGameData = loadedData;
            Debug.Log("[SettingMenu] 遊戲載入成功");
            HideGameMenu();
        }
        else
        {
            Debug.LogWarning("[SettingMenu] 沒有找到儲存的遊戲數據");
        }
    }

    private void OnExitGameButtonClicked()
    {
        Debug.Log("[SettingMenu] 退出遊戲");
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    private void OnNewGameButtonClicked()
    {
        Debug.Log("[SettingMenu] 開始新遊戲");
        GameManager.Instance.StartNewGame();
        HideGameMenu();
    }

    public void ShowSettingsMenu()
    {
        if (settingsPanel == null)
        {
            Debug.LogError("[SettingMenu] 無法顯示設定選單：選單引用丟失");
            return;
        }

        settingsPanel.style.display = DisplayStyle.Flex;
        Debug.Log("[SettingMenu] 顯示設定選單");
    }

    public void HideSettingsMenu()
    {
        if (settingsPanel == null)
        {
            Debug.LogError("[SettingMenu] 無法隱藏設定選單：選單引用丟失");
            return;
        }

        settingsPanel.style.display = DisplayStyle.None;
        Debug.Log("[SettingMenu] 隱藏設定選單");
    }

    private void OnCloseButtonClicked()
    {
        HideSettingsMenu();
    }
}
