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

    protected override void Awake()
    {
        base.Awake();
    }

    void Start()
    {
        // 確保 GameDataController 已初始化
        if (GameDataController.Instance != null && GameDataController.Instance.CurrentGameData == null)
        {
            GameDataController.Instance.CurrentGameData = new GameData();
        }

        Debug.Log("[GameManager] 初始化開始");
        saveFilePath = Path.Combine(Application.persistentDataPath, "savegame.json");

        // 初始化 GameDataController
        if (GameDataController.Instance != null && GameDataController.Instance.CurrentGameData == null)
        {
            GameDataController.Instance.CurrentGameData = new GameData();
            // 新增：主動觸發資源事件，讓 UI 立即刷新
            var playerData = GameDataController.Instance.CurrentGameData.PlayerDatad;
            if (playerData != null && playerData.OnResourceChanged != null){
                playerData.OnResourceChanged.Invoke();
                Debug.Log("[GameManager] 資源事件已觸發，UI 更新完成");
            }
        }

        Debug.Log("[GameManager] 初始化完成");

        // 載入遊戲數據
        LoadGame();
    }

   
    private void OnCancelAction(InputAction.CallbackContext context)
    {
        if (context.phase != InputActionPhase.Performed) return;
    }

    /// <summary>
    /// 初始化UI元素
    /// </summary>




    public void SaveGame(GameData data)
    {
        // 儲存時，確保是 GameDataController 的資料
        if (GameDataController.Instance != null)
            data = GameDataController.Instance.CurrentGameData;
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
            // 載入後設置到 GameDataController
            if (GameDataController.Instance != null)
            {
                GameDataController.Instance.CurrentGameData = data;
                Debug.Log("[GameManager] 遊戲數據已設置到 GameDataController");
            }
            // 新增：主動觸發資源事件，讓 UI 立即刷新
            if (data != null && data.PlayerDatad != null && data.PlayerDatad.OnResourceChanged != null)
                data.PlayerDatad.OnResourceChanged.Invoke();
            return data;
        }
        else
        {
            Debug.LogWarning("[GameManager] 找不到存檔文件: " + saveFilePath);
            return null;
        }
    }

    public void SaveMapData(Tilemap tilemap, GameData.MapData mapData)
    {
        // 儲存地圖時，直接操作 GameDataController 的 MapDatad
        if (GameDataController.Instance != null && GameDataController.Instance.CurrentGameData != null)
            mapData = GameDataController.Instance.CurrentGameData.MapDatad;
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
        // 載入地圖時，直接操作 GameDataController 的 MapDatad
        if (GameDataController.Instance != null && GameDataController.Instance.CurrentGameData != null)
            mapData = GameDataController.Instance.CurrentGameData.MapDatad;
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
        var newGameData = new GameData
        {
            PlayerDatad = new GameData.PlayerData
            {
                Oils = 200,
                Gold = 500,
                Cube = 100,
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

        // 設定到 GameDataController
        if (GameDataController.Instance != null)
            GameDataController.Instance.CurrentGameData = newGameData;

        // 新增：主動觸發資源事件，讓 UI 立即刷新
        if (newGameData.PlayerDatad != null && newGameData.PlayerDatad.OnResourceChanged != null)
            newGameData.PlayerDatad.OnResourceChanged.Invoke();

        // 保存新遊戲狀態
        SaveGame(newGameData);

        // 載入新遊戲場景
        LoadGame();
        
        Debug.Log("[GameManager] 新遊戲已開始");
    }

    private void OnApplicationQuit()
    {
        if (GameDataController.Instance != null)
            SaveGame(GameDataController.Instance.CurrentGameData);
        Debug.Log("[GameManager] 遊戲數據已在退出時保存");
    }
}