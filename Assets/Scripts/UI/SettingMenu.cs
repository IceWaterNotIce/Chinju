using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem; // 新增：使用新輸入系統

public class SettingMenu : MonoBehaviour
{
    private VisualElement root;
    private Button continueButton;
    private Button saveGameButton;
    private Button loadGameButton;
    private Button exitGameButton;
    private Button newGameButton; // 新增

    void Awake()
    {
        PopupManager.Instance.RegisterPopup("SettingMenu", gameObject);
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            ToggleMenu();
        }
    }

    void OnEnable()
    {
        InitializeUI();
    }

    void OnDestroy()
    {
        UnregisterButtonCallbacks();
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

            continueButton = UIHelper.InitializeElement<Button>(root, "continueButton");
            saveGameButton = UIHelper.InitializeElement<Button>(root, "saveGameButton");
            loadGameButton = UIHelper.InitializeElement<Button>(root, "loadGameButton");
            exitGameButton = UIHelper.InitializeElement<Button>(root, "exitGameButton");
            newGameButton = UIHelper.InitializeElement<Button>(root, "newGameButton"); // 新增

            RegisterButtonCallbacks();
            PopupManager.Instance.RegisterPopup("SettingMenu", gameObject);
            Debug.Log("[SettingMenu] UI初始化成功");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SettingMenu] UI初始化失敗: {e.Message}\n{e.StackTrace}");
            return; // 新增：初始化失敗直接 return
        }
    }

    private void RegisterButtonCallbacks()
    {
        if (continueButton != null) continueButton.clicked += OnContinueButtonClicked;
        if (saveGameButton != null) saveGameButton.clicked += OnSaveGameButtonClicked;
        if (loadGameButton != null) loadGameButton.clicked += OnLoadGameButtonClicked;
        if (exitGameButton != null) exitGameButton.clicked += OnExitGameButtonClicked;
        if (newGameButton != null) newGameButton.clicked += OnNewGameButtonClicked;
        // 若有 closeButton，這裡也註冊
        // var closeButton = root.Q<Button>("closeButton");
        // if (closeButton != null) closeButton.clicked += OnCloseButtonClicked;
    }

    private void UnregisterButtonCallbacks()
    {
        if (continueButton != null) continueButton.clicked -= OnContinueButtonClicked;
        if (saveGameButton != null) saveGameButton.clicked -= OnSaveGameButtonClicked;
        if (loadGameButton != null) loadGameButton.clicked -= OnLoadGameButtonClicked;
        if (exitGameButton != null) exitGameButton.clicked -= OnExitGameButtonClicked;
        if (newGameButton != null) newGameButton.clicked -= OnNewGameButtonClicked;
        // 若有 closeButton，這裡也解除
        // var closeButton = root.Q<Button>("closeButton");
        // if (closeButton != null) closeButton.clicked -= OnCloseButtonClicked;
    }

    private void ToggleMenu()
    {
        if (root == null)
        {
            Debug.LogError("[SettingMenu] root 未初始化");
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
        if (root == null)
        {
            Debug.LogError("[SettingMenu] 無法顯示選單：選單引用丟失");
            return;
        }

        PopupManager.Instance.ShowPopup("SettingMenu");
        Time.timeScale = 0f; // 暫停遊戲
        Debug.Log("[SettingMenu] 顯示遊戲選單並暫停遊戲");
    }

    public void HideGameMenu()
    {
        if (root == null)
        {
            Debug.LogError("[SettingMenu] 無法隱藏選單：選單引用丟失");
            return;
        }

        PopupManager.Instance.HidePopup("SettingMenu");
        Time.timeScale = 1f; // 恢復遊戲
        Debug.Log("[SettingMenu] 隱藏遊戲選單並恢復遊戲");
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
            GameManager.Instance.SaveGame(); // Adjusted to match the method signature
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
        if (root == null)
        {
            Debug.LogError("[SettingMenu] 無法顯示設定選單：選單引用丟失");
            return;
        }

        PopupManager.Instance.ShowPopup("SettingMenu");
        Debug.Log("[SettingMenu] 顯示設定選單");
    }

    public void HideSettingsMenu()
    {
        if (root == null)
        {
            Debug.LogError("[SettingMenu] 無法隱藏設定選單：選單引用丟失");
            return;
        }

        PopupManager.Instance.HidePopup("SettingMenu");
        Debug.Log("[SettingMenu] 隱藏設定選單");
    }

    private void OnCloseButtonClicked()
    {
        HideSettingsMenu();
    }
}
