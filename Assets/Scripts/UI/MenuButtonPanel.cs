using UnityEngine;
using UnityEngine.UIElements;

public class MenuButtonPanel : MonoBehaviour
{
    public SettingMenu settingMenu;

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
    void Enable()
    {
         var doc = GetComponent<UIDocument>();
        if (doc != null)
            root = doc.rootVisualElement.Q<VisualElement>("menu-button-panel");
        if (root == null)
        {
            Debug.LogError("[MenuButtonPanel] root is null. Please ensure the UIDocument is set up correctly.");
            return;
        }
        Initialize(root, settingMenu);
    }
    public void Initialize(VisualElement root, SettingMenu menu)
    {
        settingMenu = menu;
        continueButton = root.Q<Button>("continueButton");
        newGameButton = root.Q<Button>("newGameButton");
        selectGameDataButton = root.Q<Button>("selectGameDataButton");
        saveGameButton = root.Q<Button>("saveGameButton");
        exitGameButton = root.Q<Button>("exitGameButton");

        if (continueButton != null) continueButton.clicked += settingMenu.OnContinueButtonClicked;
        if (newGameButton != null) newGameButton.clicked += settingMenu.OnNewGameButtonClicked;
        if (selectGameDataButton != null) selectGameDataButton.clicked += settingMenu.OnSelectGameDataButtonClicked;
        if (saveGameButton != null) saveGameButton.clicked += settingMenu.OnSaveGameButtonClicked;
        if (exitGameButton != null) exitGameButton.clicked += settingMenu.OnExitGameButtonClicked;
    }

    // 可在此添加面板相關邏輯
}
