using UnityEngine;
using UnityEngine.UIElements;

public class GameDataCreatePanel : MonoBehaviour
{
    private VisualElement root;
    private TextField fileNameField;
    private TextField mapSeedField; // 新增
    private Button createButton;
    private Button cancelButton;

    void Awake()
    {
        PopupManager.Instance.RegisterPopup("GameDataCreatePanel", gameObject);
    }

    void OnEnable()
    {
        var uiDocument = GetComponent<UIDocument>();
        root = uiDocument.rootVisualElement;
        fileNameField = root.Q<TextField>("fileNameField");
        mapSeedField = root.Q<TextField>("mapSeedField"); // 新增
        createButton = root.Q<Button>("createButton");
        cancelButton = root.Q<Button>("cancelButton");

        createButton.clicked += OnCreateClicked;
        cancelButton.clicked += OnCancelClicked;
    }

    void OnDisable()
    {
        createButton.clicked -= OnCreateClicked;
        cancelButton.clicked -= OnCancelClicked;
    }

    private void OnCreateClicked()
    {
        string fileName = fileNameField.value.Trim();
        if (string.IsNullOrEmpty(fileName))
        {
            Debug.LogWarning("[GameDataCreatePanel] 檔名不可為空");
            return;
        }
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

        // 呼叫 GameManager 建立新遊戲，傳入 mapSeed
        GameManager.Instance.StartNewGame(fileName, mapSeed);

        PopupManager.Instance.HidePopup("GameDataCreatePanel");
        PopupManager.Instance.HidePopup("SettingMenu");
    }

    private void OnCancelClicked()
    {
        PopupManager.Instance.HidePopup("GameDataCreatePanel");
    }
}
