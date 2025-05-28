using UnityEngine;
using UnityEngine.UIElements;

public class MenuButtonPanel : MonoBehaviour
{
    private Button continueButton;
    private Button newGameButton;
    private Button selectGameDataButton;
    private Button saveGameButton;
    private Button exitGameButton;

    private VisualElement root;

    void Awake()
    {
        PopupManager.Instance.RegisterPopup("MenuButtonPanel", gameObject);
    }

    void OnEnable()
    {
        var doc = GetComponent<UIDocument>();
        if (doc != null)
            root = doc.rootVisualElement.Q<VisualElement>("menu-button-panel");
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

        if (continueButton != null) continueButton.clicked += OnContinueButtonClicked;
        if (newGameButton != null) newGameButton.clicked += OnNewGameButtonClicked;
        if (selectGameDataButton != null) selectGameDataButton.clicked += OnSelectGameDataButtonClicked;
        if (saveGameButton != null) saveGameButton.clicked += OnSaveGameButtonClicked;
        if (exitGameButton != null) exitGameButton.clicked += OnExitGameButtonClicked;
    }

    private void OnContinueButtonClicked()
    {
        Debug.Log("[MenuButtonPanel] 繼續遊戲");
        PopupManager.Instance.HidePopup("MenuButtonPanel");
        PopupManager.Instance.ShowPopup("SettingMenu");
        Time.timeScale = 1f;
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
}
