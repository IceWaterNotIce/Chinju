using UnityEngine;
using System.IO;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

/// <summary>
/// 遊戲管理器類，負責管理遊戲的核心功能
/// </summary>
public class GameManager : Singleton<GameManager>
{
    public const string serverUrl = "https://icewaternotice.com/games/Word Curse/";
    public const string githubUrl = "https://raw.githubusercontent.com/IceWaterNotIce/Word-Curse/main/";

    private string saveFilePath;
    public GameData currentGameData = new GameData();
    private VisualElement root;
    private VisualElement menuPanel;
    private Button continueButton;
    private Button saveGameButton;
    private Button loadGameButton;
    private Button exitGameButton;
    private InputSystem_Actions inputActions;
    private bool isMenuVisible = false;

    protected override void Awake()
    {
        base.Awake();
        InitializeInputSystem();
    }

    void Start()
    {
        Debug.Log("[GameManager] 初始化開始");
        saveFilePath = Path.Combine(Application.persistentDataPath, "savegame.json");
        
        // 初始化UI
        InitializeUI();
        
        Debug.Log("[GameManager] 初始化完成");
    }

    /// <summary>
    /// 初始化輸入系統
    /// </summary>
    private void InitializeInputSystem()
    {
        try
        {
            inputActions = new InputSystem_Actions();
            inputActions.UI.Cancel.performed += OnCancelAction;
            inputActions.UI.Enable();
            Debug.Log("[GameManager] 輸入系統初始化成功");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[GameManager] 輸入系統初始化失敗: {e.Message}\n{e.StackTrace}");
        }
    }

    private void OnCancelAction(InputAction.CallbackContext context)
    {
        if (context.phase != InputActionPhase.Performed) return;
        ToggleMenu();
    }

    /// <summary>
    /// 初始化UI元素
    /// </summary>
    private void InitializeUI()
    {
        try
        {
            // 載入 UI 資源
            var menuDocument = GetComponent<UIDocument>();
            if (menuDocument == null)
            {
                menuDocument = gameObject.AddComponent<UIDocument>();
            }

            var visualTree = Resources.Load<VisualTreeAsset>("UI/GameMenu");
            if (visualTree == null)
            {
                Debug.LogError("[GameManager] 無法載入 UI/GameMenu.uxml");
                return;
            }

            var styleSheet = Resources.Load<StyleSheet>("UI/GameMenu");
            if (styleSheet == null)
            {
                Debug.LogError("[GameManager] 無法載入 UI/GameMenu.uss");
                return;
            }

            // 設置 UI Document
            menuDocument.visualTreeAsset = visualTree;
            root = menuDocument.rootVisualElement;
            root.styleSheets.Add(styleSheet);
            
            // 獲取UI元素
            menuPanel = root.Q<VisualElement>("menu-panel");
            continueButton = root.Q<Button>("continueButton");
            saveGameButton = root.Q<Button>("saveGameButton");
            loadGameButton = root.Q<Button>("loadGameButton");
            exitGameButton = root.Q<Button>("exitGameButton");

            // 註冊按鈕事件
            RegisterButtonCallbacks();
            
            // 初始隱藏選單
            HideGameMenu();
            Debug.Log("[GameManager] UI初始化成功");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[GameManager] UI初始化失敗: {e.Message}\n{e.StackTrace}");
        }
    }

    /// <summary>
    /// 註冊按鈕回調
    /// </summary>
    private void RegisterButtonCallbacks()
    {
        if (continueButton != null) continueButton.clicked += OnContinueButtonClicked;
        if (saveGameButton != null) saveGameButton.clicked += OnSaveGameButtonClicked;
        if (loadGameButton != null) loadGameButton.clicked += OnLoadGameButtonClicked;
        if (exitGameButton != null) exitGameButton.clicked += OnExitGameButtonClicked;
    }

    /// <summary>
    /// 切換選單顯示狀態
    /// </summary>
    private void ToggleMenu()
    {
        if (isMenuVisible)
        {
            HideGameMenu();
        }
        else
        {
            ShowGameMenu();
        }
    }

    /// <summary>
    /// 顯示遊戲選單
    /// </summary>
    public void ShowGameMenu()
    {
        if (menuPanel == null)
        {
            Debug.LogError("[GameManager] 無法顯示選單：選單引用丟失");
            return;
        }

        menuPanel.style.display = DisplayStyle.Flex;
        isMenuVisible = true;
        Debug.Log("[GameManager] 顯示遊戲選單");
    }

    /// <summary>
    /// 隱藏遊戲選單
    /// </summary>
    public void HideGameMenu()
    {
        if (menuPanel == null)
        {
            Debug.LogError("[GameManager] 無法隱藏選單：選單引用丟失");
            return;
        }

        menuPanel.style.display = DisplayStyle.None;
        isMenuVisible = false;
        Debug.Log("[GameManager] 隱藏遊戲選單");
    }

    /// <summary>
    /// 獲取選單可見性狀態
    /// </summary>
    public bool IsMenuVisible()
    {
        return isMenuVisible;
    }

    private void OnEnable()
    {
        inputActions?.Enable();
    }

    private void OnDisable()
    {
        inputActions?.Disable();
    }

    private void OnDestroy()
    {
        // 取消註冊按鈕事件
        if (continueButton != null) continueButton.clicked -= OnContinueButtonClicked;
        if (saveGameButton != null) saveGameButton.clicked -= OnSaveGameButtonClicked;
        if (loadGameButton != null) loadGameButton.clicked -= OnLoadGameButtonClicked;
        if (exitGameButton != null) exitGameButton.clicked -= OnExitGameButtonClicked;

        // 取消註冊輸入系統事件
        if (inputActions != null)
        {
            inputActions.UI.Cancel.performed -= OnCancelAction;
            inputActions.Dispose();
        }
    }

    public void SaveGame(GameData data)
    {
        string json = JsonUtility.ToJson(data);
        File.WriteAllText(saveFilePath, json);
        Debug.Log("[GameManager] 遊戲已保存至 " + saveFilePath);
    }

    public GameData LoadGame()
    {
        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            GameData data = JsonUtility.FromJson<GameData>(json);
            Debug.Log("[GameManager] 遊戲已從 " + saveFilePath + " 載入");
            return data;
        }
        else
        {
            Debug.LogWarning("[GameManager] 找不到存檔文件: " + saveFilePath);
            return null;
        }
    }

    /// <summary>
    /// 儲存遊戲按鈕點擊處理
    /// </summary>
    public void OnSaveGameButtonClicked()
    {
        Debug.Log("[GameManager] 觸發儲存遊戲");
        try
        {
            SaveGame(currentGameData);
            Debug.Log("[GameManager] 遊戲儲存成功");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[GameManager] 儲存遊戲失敗: {e.Message}");
        }
    }

    /// <summary>
    /// 繼續遊戲按鈕點擊處理
    /// </summary>
    public void OnContinueButtonClicked()
    {
        Debug.Log("[GameManager] 繼續遊戲");
        HideGameMenu();
    }

    /// <summary>
    /// 載入遊戲按鈕點擊處理
    /// </summary>
    public void OnLoadGameButtonClicked()
    {
        Debug.Log("[GameManager] 觸發載入遊戲");
        try
        {
            GameData loadedData = LoadGame();
            if (loadedData != null)
            {
                currentGameData = loadedData;
                Debug.Log("[GameManager] 遊戲載入成功");
                HideGameMenu();
            }
            else
            {
                Debug.LogWarning("[GameManager] 沒有找到儲存的遊戲數據");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[GameManager] 載入遊戲失敗: {e.Message}");
        }
    }

    /// <summary>
    /// 退出遊戲按鈕點擊處理
    /// </summary>
    public void OnExitGameButtonClicked()
    {
        Debug.Log("[GameManager] 退出遊戲");
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    public void SaveMapData(Tilemap tilemap, GameData.MapData mapData)
    {
        mapData.ChinjuTiles.Clear();
        foreach (var position in tilemap.cellBounds.allPositionsWithin)
        {
            if (tilemap.GetTile(position) != null)
            {
                mapData.ChinjuTiles.Add(position);
            }
        }
        Debug.Log("[GameManager] 地圖數據已保存");
    }

    public void LoadMapData(Tilemap tilemap, GameData.MapData mapData, TileBase chinjuTile)
    {
        tilemap.ClearAllTiles();
        foreach (var position in mapData.ChinjuTiles)
        {
            tilemap.SetTile(position, chinjuTile);
        }
        Debug.Log("[GameManager] 地圖數據已載入");
    }

    public void StartNewGame()
    {
        // 重置遊戲數據
        currentGameData = new GameData
        {
            PlayerDatad = new GameData.PlayerData
            {
                Oils = 100,
                Gold = 500,
                Cube = 0,
                Ships = new List<GameData.ShipData>()
            },
            MapDatad = new GameData.MapData
            {
                Seed = Random.Range(0, int.MaxValue),
                Width = 100,
                Height = 100,
                IslandDensity = 0.1f,
                ChinjuTiles = new List<Vector3Int>()
            }
        };

        // 保存新遊戲狀態
        SaveGame(currentGameData);

        // 通知其他系統更新
        UIManager.Instance.UpdateResourceDisplay(
            (int)currentGameData.PlayerDatad.Gold,
            (int)currentGameData.PlayerDatad.Oils,
            (int)currentGameData.PlayerDatad.Cube
        );
        
        Debug.Log("[GameManager] 新遊戲已開始");
    }

    private void OnApplicationQuit()
    {
        SaveGame(currentGameData);
        Debug.Log("[GameManager] 遊戲數據已在退出時保存");
    }
}