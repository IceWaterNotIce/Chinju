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
            var playerData = GameDataController.Instance.CurrentGameData.playerData;
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




    public void SaveGame()
    {
        // 儲存時，確保是 GameDataController 的資料
        if (GameDataController.Instance != null)
        {
            var data = GameDataController.Instance.CurrentGameData;

            // 儲存前，先更新所有玩家船艦資料
            if (data != null && data.playerData != null && data.playerData.Ships != null)
            {
                // 取得場景中所有 Ship 物件
                var shipsInScene = GameObject.FindObjectsByType<Ship>(FindObjectsSortMode.None);
                for (int i = 0; i < data.playerData.Ships.Count; i++)
                {
                    // 根據索引對應（假設順序一致），將場景 Ship 狀態存回 ShipData
                    if (i < shipsInScene.Length && shipsInScene[i] != null)
                    {
                        data.playerData.Ships[i] = shipsInScene[i].SaveShipData();
                    }
                }
            }

            string json = JsonUtility.ToJson(data);
            File.WriteAllText(saveFilePath, json);
            Debug.Log("[GameManager] 遊戲已保存至 " + saveFilePath);
        }
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
            if (data != null && data.playerData != null && data.playerData.OnResourceChanged != null)
                data.playerData.OnResourceChanged.Invoke();

            // 新增：根據存檔自動生成所有船艦
            if (data != null && data.playerData != null && data.playerData.Ships != null)
            {
                // 先刪除場景中所有現有 Ship 物件，避免重複
                var existingShips = GameObject.FindObjectsByType<Ship>(FindObjectsSortMode.None);
                foreach (var ship in existingShips)
                {
                    GameObject.Destroy(ship.gameObject);
                }

                // 可根據實際需求指定船艦Prefab路徑
                var shipPrefab = Resources.Load<GameObject>("Prefabs/Ship");
                if (shipPrefab != null)
                {
                    foreach (var shipData in data.playerData.Ships)
                    {
                        var shipObj = GameObject.Instantiate(shipPrefab, shipData.Position, Quaternion.Euler(0, 0, shipData.Rotation));
                        var shipComp = shipObj.GetComponent<Ship>();
                        if (shipComp != null)
                        {
                            shipComp.LoadShipData(shipData);
                        }
                    }
                }
                else
                {
                    Debug.LogWarning("找不到 Resources/Ship 預製物，無法自動生成船艦！");
                }
            }

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
        // 儲存地圖時，直接操作 GameDataController 的 mapData
        if (GameDataController.Instance != null && GameDataController.Instance.CurrentGameData != null)
            mapData = GameDataController.Instance.CurrentGameData.mapData;
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
        // 載入地圖時，直接操作 GameDataController 的 mapData
        if (GameDataController.Instance != null && GameDataController.Instance.CurrentGameData != null)
            mapData = GameDataController.Instance.CurrentGameData.mapData;
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
            playerData = new GameData.PlayerData
            {
                Oils = 200,
                Gold = 500,
                Cube = 100,
                Ships = new List<GameData.ShipData>()
            },
            mapData = new GameData.MapData
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
        if (newGameData.playerData != null && newGameData.playerData.OnResourceChanged != null)
            newGameData.playerData.OnResourceChanged.Invoke();

        // 保存新遊戲狀態
        SaveGame();

        // 載入新遊戲場景
        LoadGame();
        
        Debug.Log("[GameManager] 新遊戲已開始");
    }

    private void OnApplicationQuit()
    {
        if (GameDataController.Instance != null)
            SaveGame();
        Debug.Log("[GameManager] 遊戲數據已在退出時保存");
    }
}