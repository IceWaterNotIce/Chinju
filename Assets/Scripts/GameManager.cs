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
        if (GameDataController.Instance != null)
        {
            var data = GameDataController.Instance.CurrentGameData;

            if (data != null)
            {
                // 更新玩家船艦數據
                if (data.playerData != null && data.playerData.Ships != null)
                {
                    var shipsInScene = GameObject.FindObjectsByType<Ship>(FindObjectsSortMode.None);
                    for (int i = 0; i < shipsInScene.Length; i++)
                    {
                        var shipData = shipsInScene[i].SaveShipData();
                        shipData.PrefabName = shipsInScene[i].gameObject.name.Replace("(Clone)", "").Trim(); // 修正保存的 PrefabName
                        if (i < data.playerData.Ships.Count)
                        {
                            data.playerData.Ships[i] = shipData;
                        }
                        else
                        {
                            data.playerData.Ships.Add(shipData);
                        }
                    }
                }

                try
                {
                    string json = JsonUtility.ToJson(data, true); // 格式化輸出
                    File.WriteAllText(saveFilePath, json);
                    Debug.Log($"[GameManager] 遊戲已保存至 {saveFilePath}");
                }
                catch (IOException ex)
                {
                    Debug.LogError($"[GameManager] 儲存遊戲時發生錯誤: {ex.Message}");
                }
            }
            else
            {
                Debug.LogWarning("[GameManager] 無法保存遊戲，GameData 為 null");
            }
        }
        else
        {
            Debug.LogWarning("[GameManager] 無法保存遊戲，GameDataController 未初始化");
        }
    }

    public GameData LoadGame()
    {
        if (File.Exists(saveFilePath))
        {
            try
            {
                string json = File.ReadAllText(saveFilePath);
                GameData data = JsonUtility.FromJson<GameData>(json);

                if (data != null)
                {
                    if (GameDataController.Instance != null)
                    {
                        GameDataController.Instance.CurrentGameData = data;
                        Debug.Log("[GameManager] 遊戲數據已設置到 GameDataController");
                    }

                    data.playerData?.OnResourceChanged?.Invoke();

                    if (data.playerData?.Ships != null)
                    {
                        var existingShips = GameObject.FindObjectsByType<Ship>(FindObjectsSortMode.None);
                        foreach (var ship in existingShips)
                        {
                            GameObject.Destroy(ship.gameObject);
                        }

                        foreach (var shipData in data.playerData.Ships)
                        {
                            var shipPrefab = Resources.Load<GameObject>($"Prefabs/{shipData.PrefabName}");
                            if (shipPrefab != null)
                            {
                                var shipObj = GameObject.Instantiate(shipPrefab, shipData.Position, Quaternion.Euler(0, 0, shipData.Rotation));
                                var shipComp = shipObj.GetComponent<Ship>();
                                if (shipComp != null)
                                {
                                    shipComp.LoadShipData(shipData); // 確保唯一的 LoadShipData 方法被調用
                                    foreach (var weaponData in shipData.Weapons)
                                    {
                                        var weaponPrefab = Resources.Load<GameObject>($"Prefabs/{weaponData.PrefabName}");
                                        if (weaponPrefab != null)
                                        {
                                            var weaponObj = GameObject.Instantiate(weaponPrefab);
                                            var weaponComp = weaponObj.GetComponent<Weapon>();
                                            weaponComp?.LoadWeaponData(weaponData);
                                            shipComp.AddWeapon(weaponComp);
                                        }
                                        else
                                        {
                                            Debug.LogWarning($"[GameManager] 找不到武器預製物: {weaponData.PrefabName}");
                                        }
                                    }
                                }
                            }
                            else
                            {
                                Debug.LogWarning($"[GameManager] 找不到船隻預製物: {shipData.PrefabName}");
                            }
                        }
                    }
                }
                else
                {
                    Debug.LogWarning("[GameManager] 載入的遊戲數據為 null");
                }

                return data;
            }
            catch (IOException ex)
            {
                Debug.LogError($"[GameManager] 載入遊戲時發生錯誤: {ex.Message}");
                return null;
            }
        }
        else
        {
            Debug.LogWarning($"[GameManager] 找不到存檔文件: {saveFilePath}");
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