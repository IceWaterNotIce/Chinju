using UnityEngine;
using UnityEngine.UIElements;
using System.IO;
using System.Collections.Generic;

public class GameDataSelectPanel : MonoBehaviour
{
    private VisualElement root;
    private VisualElement fileListContainer;
    private Button closeButton;
    // 新增：開啟資料夾按鈕
    private Button openFolderButton;

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

        // 新增：取得開啟資料夾按鈕並註冊事件
        openFolderButton = UIHelper.InitializeElement<Button>(root, "openFolderButton");
        if (openFolderButton != null)
        {
            openFolderButton.clicked += OpenSaveFolderInExplorer;
        }

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
        if (Path.GetFileName(filePath) != "savegame.json") // 僅允許載入統一檔案名稱
        {
            Debug.LogWarning($"[GameDataSelectPanel] 不支援載入非統一檔案名稱: {filePath}");
            return;
        }
        // 檢查檔案是否存在
        if (!File.Exists(filePath))
        {
            Debug.LogWarning($"[GameDataSelectPanel] 檔案不存在: {filePath}");
            return;
        }
        // get the file name without path
        string fileName = Path.GetFileName(filePath);
        GameManager.Instance.LoadGame(fileName, isServer: true); // 修正：加入 isServer 參數
        PopupManager.Instance.HidePopup("GameDataSelectPanel");
        PopupManager.Instance.HidePopup("SettingMenu");
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

    // 新增：開啟存檔資料夾
    private void OpenSaveFolderInExplorer()
    {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        System.Diagnostics.Process.Start("explorer.exe", saveFolderPath.Replace("/", "\\"));
#elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
        System.Diagnostics.Process.Start("open", saveFolderPath);
#elif UNITY_EDITOR_LINUX || UNITY_STANDALONE_LINUX
        System.Diagnostics.Process.Start("xdg-open", saveFolderPath);
#else
        Debug.Log("不支援的作業系統，無法開啟資料夾。");
#endif
    }
}
