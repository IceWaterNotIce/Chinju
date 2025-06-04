using UnityEngine;
using UnityEngine.UIElements;
using Unity.Services.Core; // 新增命名空間
using Unity.Services.Authentication;

public class MenuButtonPanel : MonoBehaviour
{
    private Button continueButton;
    private Button newGameButton;
    private Button selectGameDataButton;
    private Button saveGameButton;
    private Button exitGameButton;
    private Button loginLogoutButton;

    private Image gameIcon;
    private Label gameVersion;

    private VisualElement root;

    async void Awake()
    {
        PopupManager.Instance.RegisterPopup("MenuButtonPanel", gameObject);

        try
        {
            await UnityServices.InitializeAsync(); // 初始化 Unity Services
            Debug.Log("Unity Services 已初始化");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Unity Services 初始化失敗: {ex.Message}");
        }
    }

    void OnEnable()
    {
        var doc = GetComponent<UIDocument>();
        if (doc != null)
            root = doc.rootVisualElement;
        if (root == null)
        {
            Debug.LogError("[MenuButtonPanel] root is null. Please ensure the UIDocument is set up correctly.");
            return;
        }
        Initialize(root);
    }

    public void Initialize(VisualElement root)
    {
        continueButton = UIHelper.InitializeElement<Button>(root, "continueButton");
        newGameButton = UIHelper.InitializeElement<Button>(root, "newGameButton");
        selectGameDataButton = UIHelper.InitializeElement<Button>(root, "selectGameDataButton");
        saveGameButton = UIHelper.InitializeElement<Button>(root, "saveGameButton");
        exitGameButton = UIHelper.InitializeElement<Button>(root, "exitGameButton");
        gameIcon = UIHelper.InitializeElement<Image>(root, "gameIcon");
        gameVersion = UIHelper.InitializeElement<Label>(root, "gameVersion");
        loginLogoutButton = UIHelper.InitializeElement<Button>(root, "loginLogoutButton");

        if (gameVersion != null)
            gameVersion.text = $"版本: {Application.version}";

        if (continueButton != null) continueButton.clicked += OnContinueButtonClicked;
        if (newGameButton != null) newGameButton.clicked += OnNewGameButtonClicked;
        if (selectGameDataButton != null) selectGameDataButton.clicked += OnSelectGameDataButtonClicked;
        if (saveGameButton != null) saveGameButton.clicked += OnSaveGameButtonClicked;
        if (exitGameButton != null) exitGameButton.clicked += OnExitGameButtonClicked;
        if (loginLogoutButton != null) loginLogoutButton.clicked += OnLoginLogoutButtonClicked;
    }

    private void OnContinueButtonClicked()
    {
        Debug.Log("[MenuButtonPanel] 繼續遊戲");
        PopupManager.Instance.HidePopup("MenuButtonPanel");
        GameManager.Instance.LoadGame();
    }

    private void OnSaveGameButtonClicked()
    {
        Debug.Log("[MenuButtonPanel] 儲存遊戲");
        if (GameDataController.Instance != null)
            GameManager.Instance.SaveGame();
    }

    private void OnExitGameButtonClicked()
    {
        Debug.Log("[MenuButtonPanel] 退出遊戲");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void OnNewGameButtonClicked()
    {
        PopupManager.Instance.ShowPopup("GameDataCreatePanel");
        PopupManager.Instance.HidePopup("MenuButtonPanel");
        PopupManager.Instance.HidePopup("SettingMenu");
        Time.timeScale = 1f;
    }

    private void OnSelectGameDataButtonClicked()
    {
        PopupManager.Instance.ShowPopup("GameDataSelectPanel");
        PopupManager.Instance.HidePopup("MenuButtonPanel");
        PopupManager.Instance.HidePopup("SettingMenu");
    }

    private void OnLoginLogoutButtonClicked()
    {
        Debug.Log("[MenuButtonPanel] 登入/登出");
        if (AuthenticationService.Instance.IsSignedIn)
        {
            AuthenticationService.Instance.SignOut();
            Debug.Log("已登出");
        }
        else
        {
            PopupManager.Instance.ShowPopup("LoginPanel");
            Debug.Log("顯示登入面板");
        }
    }
}
