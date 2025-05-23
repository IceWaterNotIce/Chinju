using UnityEngine;
using UnityEngine.UIElements;
using System.IO;
using System.Collections.Generic;

public class GameDataSelectPanel : MonoBehaviour
{
    private VisualElement root;
    private VisualElement fileListContainer;
    private Button closeButton;

    private string saveFolderPath;

    void Awake()
    {
        PopupManager.Instance.RegisterPopup("GameDataSelectPanel", gameObject);
    }

    void OnEnable()
    {
        var uiDocument = GetComponent<UIDocument>();
        root = uiDocument.rootVisualElement;
        fileListContainer = UIHelper.InitializeElement<VisualElement>(root, "fileListContainer");
        closeButton = UIHelper.InitializeElement<Button>(root, "closeButton");
        closeButton.clicked += () => PopupManager.Instance.HidePopup("GameDataSelectPanel");

        saveFolderPath = Application.persistentDataPath;
        RefreshFileList();
    }

    private void RefreshFileList()
    {
        fileListContainer.Clear();
        var files = Directory.GetFiles(saveFolderPath, "*.json");
        if (files.Length == 0)
        {
            fileListContainer.Add(new Label("沒有找到任何存檔。"));
            return;
        }

        foreach (var file in files)
        {
            string fileName = Path.GetFileName(file);
            var btn = new Button(() => OnSelectFile(file)) { text = fileName };
            btn.AddToClassList("save-file-btn");
            fileListContainer.Add(btn);
        }
    }

    private void OnSelectFile(string filePath)
    {
        string json = File.ReadAllText(filePath);
        var data = JsonUtility.FromJson<GameData>(json);
        if (data != null)
        {
            GameDataController.Instance.CurrentGameData = data;
            GameManager.Instance.LoadGame(); // 觸發載入流程
            PopupManager.Instance.HidePopup("GameDataSelectPanel");
            PopupManager.Instance.HidePopup("SettingMenu");
        }
        else
        {
            Debug.LogWarning($"[GameDataSelectPanel] 載入失敗: {filePath}");
        }
    }
}
