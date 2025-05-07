using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    public const string serverUrl = "https://icewaternotice.com/games/Word Curse/";
    public const string githubUrl = "https://raw.githubusercontent.com/IceWaterNotIce/Word-Curse/main/";

    private string saveFilePath;
    private float gameTime; // 遊戲時間（秒）

    public delegate void GameEvent();
    public static event GameEvent OnGameSaved;
    public static event GameEvent OnGameLoaded;

    protected override void Awake()
    {
        base.Awake();
    }

    void Start()
    {
        if (GameDataController.Instance != null && GameDataController.Instance.CurrentGameData == null)
        {
            InitializeGameData();
        }

        Debug.Log("[GameManager] 初始化開始");
        saveFilePath = Path.Combine(Application.persistentDataPath, "savegame.json");

        Debug.Log("[GameManager] 初始化完成");

        LoadGame();
    }

    void Update()
    {
        gameTime += Time.deltaTime; // 累加遊戲時間
    }

    private void InitializeGameData()
    {
        GameDataController.Instance.CurrentGameData = new GameData();
        GameDataController.Instance.TriggerResourceChanged();
    }

    public void SaveGame()
    {
        if (GameDataController.Instance != null)
        {
            var data = GameDataController.Instance.CurrentGameData;

            if (data != null)
            {
                try
                {
                    // 保存玩家船隻數據
                    var playerShips = GameObject.FindObjectsByType<Ship>(FindObjectsSortMode.None)
                        .Where(ship => ship.IsPlayerShip)
                        .ToList();

                    data.playerData.Ships.Clear();
                    foreach (var ship in playerShips)
                    {
                        data.playerData.Ships.Add(ship.SaveShipData());
                    }

                    string json = JsonUtility.ToJson(data, true);
                    File.WriteAllText(saveFilePath, json);
                    Debug.Log($"[GameManager] 遊戲已保存至 {saveFilePath}");
                    OnGameSaved?.Invoke(); // 發送保存事件
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

                    // 載入玩家船隻數據
                    foreach (var shipData in data.playerData.Ships)
                    {
                        var shipPrefab = Resources.Load<GameObject>($"Prefabs/Ship/{shipData.PrefabName}");
                        if (shipPrefab != null)
                        {
                            var shipObj = Instantiate(shipPrefab, shipData.Position, Quaternion.Euler(0, 0, shipData.Rotation));
                            var shipComp = shipObj.GetComponent<Ship>();
                            if (shipComp != null)
                            {
                                shipComp.LoadShipData(shipData);
                            }
                        }
                        else
                        {
                            Debug.LogWarning($"[GameManager] 找不到船隻預製物: {shipData.PrefabName}");
                        }
                    }

                    GameDataController.Instance.TriggerResourceChanged();
                    OnGameLoaded?.Invoke(); // 發送載入事件
                }
                else
                {
                    Debug.LogWarning("[GameManager] 載入的遊戲數據為 null");
                }

                return data;
            }
            catch (IOException ex)
            {
                Debug.LogError($"[GameManager] 載入遊戲時發生 IO 錯誤: {ex.Message}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[GameManager] 載入遊戲時發生未知錯誤: {ex.Message}");
            }
        }
        else
        {
            Debug.LogWarning($"[GameManager] 找不到存檔文件: {saveFilePath}");
        }

        return null;
    }

    public void StartNewGame()
    {
        // 清除現有船隻
        var existingShips = GameObject.FindObjectsByType<Ship>(FindObjectsSortMode.None);
        foreach (var ship in existingShips)
        {
            GameObject.Destroy(ship.gameObject);
        }

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

        GameDataController.Instance.TriggerResourceChanged();
        Debug.Log("[GameManager] 新遊戲已開始");
    }

    public string GetFormattedGameTime()
    {
        int hours = Mathf.FloorToInt(gameTime / 3600);
        int minutes = Mathf.FloorToInt((gameTime % 3600) / 60);
        int seconds = Mathf.FloorToInt(gameTime % 60);
        return $"{hours:D2}:{minutes:D2}:{seconds:D2}";
    }

    private void OnApplicationQuit()
    {
        if (GameDataController.Instance != null)
            SaveGame();
        Debug.Log("[GameManager] 遊戲數據已在退出時保存");
    }
}