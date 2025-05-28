using UnityEngine;
using UnityEngine.UIElements;

public class GameDataCreatePanel : MonoBehaviour
{
    private VisualElement root;
    private TextField fileNameField;
    private TextField mapSeedField; // 新增
    private TextField saveDirField; // 新增
    private Button createButton;
    private Button cancelButton;
    private Button browseDirButton; // 新增

    void Awake()
    {
        PopupManager.Instance.RegisterPopup("GameDataCreatePanel", gameObject);
    }

    void OnEnable()
    {
        var uiDocument = GetComponent<UIDocument>();
        root = uiDocument.rootVisualElement;
        fileNameField = UIHelper.InitializeElement<TextField>(root, "fileNameField");
        mapSeedField = UIHelper.InitializeElement<TextField>(root, "mapSeedField");
        saveDirField = UIHelper.InitializeElement<TextField>(root, "saveDirField");
        createButton = UIHelper.InitializeElement<Button>(root, "createButton");
        cancelButton = UIHelper.InitializeElement<Button>(root, "cancelButton");
        browseDirButton = UIHelper.InitializeElement<Button>(root, "browseDirButton");

        createButton.clicked += OnCreateClicked;
        cancelButton.clicked += OnCancelClicked;
        if (browseDirButton != null)
            browseDirButton.clicked += OnBrowseDirClicked; // 新增
    }

    void OnDisable()
    {
        if (createButton != null)
            createButton.clicked -= OnCreateClicked;
        if (cancelButton != null)
            cancelButton.clicked -= OnCancelClicked;
        if (browseDirButton != null)
            browseDirButton.clicked -= OnBrowseDirClicked; // 新增
    }

    private void OnCreateClicked()
    {
        string fileName = fileNameField.value.Trim();
        if (!fileName.EndsWith(".json"))
            fileName += ".json";

        int? mapSeed = null;
        if (mapSeedField != null && !string.IsNullOrWhiteSpace(mapSeedField.value))
        {
            if (int.TryParse(mapSeedField.value.Trim(), out int seed))
                mapSeed = seed;
            else
                Debug.LogWarning("[GameDataCreatePanel] 地圖種子格式錯誤，將使用隨機種子");
        }

        string saveDir = saveDirField != null ? saveDirField.value.Trim() : "";

        // 呼叫 GameManager 建立新遊戲，傳入 mapSeed 與 saveDir
        GameManager.Instance.StartNewGame(fileName, mapSeed, saveDir);

        PopupManager.Instance.HidePopup("GameDataCreatePanel");
        PopupManager.Instance.HidePopup("SettingMenu");
    }

    private void OnCancelClicked()
    {
        PopupManager.Instance.HidePopup("GameDataCreatePanel");
    }

    // 新增：開啟資料夾選擇視窗
    private void OnBrowseDirClicked()
    {
#if UNITY_EDITOR
        string path = UnityEditor.EditorUtility.OpenFolderPanel("選擇存檔資料夾", "", "");
        if (!string.IsNullOrEmpty(path) && saveDirField != null)
        {
            saveDirField.value = path;
        }
#else
        Debug.LogWarning("[GameDataCreatePanel] 僅支援於 Editor 模式選擇資料夾");
#endif
    }
}
