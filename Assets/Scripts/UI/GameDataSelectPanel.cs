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

    private VisualTreeAsset gameDataListItemTemplate;

    void Awake()
    {
        PopupManager.Instance.RegisterPopup("GameDataSelectPanel", gameObject);
        // 載入 UXML 模板
        gameDataListItemTemplate = Resources.Load<VisualTreeAsset>("UI/GameDataListItem");
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

            // 使用 UXML 模板建立項目
            VisualElement row;
            if (gameDataListItemTemplate != null)
                row = gameDataListItemTemplate.Instantiate();
            else
                row = new VisualElement(); // fallback

            // 綁定 Label
            var fileNameLabel = row.Q<Label>("fileNameLabel");
            if (fileNameLabel != null)
                fileNameLabel.text = fileName;

            // 綁定載入按鈕
            var loadBtn = row.Q<Button>("loadButton");
            if (loadBtn != null)
            {
                loadBtn.clicked += () => OnSelectFile(file);
            }

            // 綁定刪除按鈕
            var delBtn = row.Q<Button>("deleteButton");
            if (delBtn != null)
            {
                delBtn.clicked += () => OnDeleteFile(file);
            }

            fileListContainer.Add(row);
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

    // 新增刪除檔案的方法
    private void OnDeleteFile(string filePath)
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            RefreshFileList();
        }
        else
        {
            Debug.LogWarning($"[GameDataSelectPanel] 檔案不存在: {filePath}");
        }
    }
}
