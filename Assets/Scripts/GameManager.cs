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
        Debug.Log("[GameManager] 初始化開始");
        saveFilePath = Path.Combine(Application.persistentDataPath, "savegame.json");

        // 初始化 GameDataController
        if (GameDataController.Instance != null && GameDataController.Instance.CurrentGameData == null)
        {
            GameDataController.Instance.CurrentGameData = new GameData();
        }

        Debug.Log("[GameManager] 初始化完成");
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
                GameDataController.Instance.CurrentGameData = data;
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
        var newGameData = new GameData
        {
            PlayerDatad = new GameData.PlayerData
            {
                Oils = 100,
                Gold = 500,
                Cube = 50,
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