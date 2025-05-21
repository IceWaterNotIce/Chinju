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

    // 遊戲內一天的秒數（現實 20 分鐘 = 遊戲 1 天，10 分鐘 = 12 小時）
    private const float RealSecondsPerGameDay = 600f * 2; // 1200 秒 = 1 天
    private const int GameSecondsPerDay = 24 * 60 * 60; // 86400 秒 = 1 天

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
        // 讓現實 10 分鐘等於遊戲 12 小時
        float gameSecondsPerRealSecond = (GameSecondsPerDay / RealSecondsPerGameDay);
        gameTime += Time.deltaTime * gameSecondsPerRealSecond;
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
                    var playerShips = GameObject.FindObjectsByType<PlayerShip>(FindObjectsSortMode.None)
                        .Where(ship => ship != null)
                        .ToList();

                    data.playerData.Ships.Clear();
                    foreach (var ship in playerShips)
                    {
                        data.playerData.Ships.Add(ship.SaveShipData());
                    }

                    // 保存敵人數據
                    var enemyShips = GameObject.FindObjectsByType<EnemyShip>(FindObjectsSortMode.None)
                        .Where(ship => ship != null)
                        .ToList();

                    data.enemyShips.Clear();
                    foreach (var ship in enemyShips)
                    {
                        var shipData = ship.SaveShipData();

                        // 保存武器數據
                        shipData.Weapons.Clear();
                        foreach (var weapon in ship.GetWeapons())
                        {
                            var weaponData = new GameData.WeaponData
                            {
                                Name = weapon.name,
                                Damage = (int)weapon.Damage,
                                MaxAttackDistance = weapon.MaxAttackDistance,
                                AttackSpeed = weapon.AttackSpeed,
                                PrefabName = weapon.Name
                            };
                            shipData.Weapons.Add(weaponData);
                        }

                        data.enemyShips.Add(shipData);
                    }

                    // 保存遊戲時間
                    data.gameTime = gameTime;

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

                    // 使用 ShipCreationManager 載入玩家船隻數據並實例化
                    foreach (var shipData in data.playerData.Ships)
                    {
                        if (ShipCreationManager.Instance != null)
                        {
                            ShipCreationManager.Instance.InstantiateShipFromData(shipData);
                        }
                        else
                        {
                            Debug.LogError("[GameManager] ShipCreationManager 未初始化，無法實例化船隻！");
                        }
                    }

                    // CLear existing enemy ships before loading new ones
                    var existingEnemyShips = GameObject.FindObjectsByType<EnemyShip>(FindObjectsSortMode.None);
                    foreach (var enemyShip in existingEnemyShips)
                    {
                        GameObject.Destroy(enemyShip.gameObject);
                    }

                    // 使用 EnemyShipSpawner 載入敵人數據並生成敵人
                    foreach (var shipData in data.enemyShips)
                    {
                        if (EnemyShipManager.Instance != null)
                        {
                            EnemyShipManager.Instance.SpawnEnemyFromData(shipData);
                        }
                        else
                        {
                            Debug.LogError("[GameManager] EnemyShipSpawner 未初始化，無法生成敵人！");
                        }
                    }

                    // 載入遊戲時間
                    gameTime = data.gameTime;

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
        // 將遊戲時間轉換為年月日時分秒
        int totalGameSeconds = Mathf.FloorToInt(gameTime);

        int seconds = totalGameSeconds % 60;
        int minutes = (totalGameSeconds / 60) % 60;
        int hours = (totalGameSeconds / 3600) % 24;
        int days = (totalGameSeconds / 86400) % 30 + 1; // 1-based day
        int months = (totalGameSeconds / (86400 * 30)) % 12 + 1; // 1-based month
        int years = (totalGameSeconds / (86400 * 30 * 12)) + 1; // 1-based year

        // 格式 ss:mm:dd:MM:YYYY
        return $"{seconds:D2}:{minutes:D2}:{days:D2}:{months:D2}:{years:D4}";
    }

    public float GetGameTimeSeconds()
    {
        return gameTime;
    }

    private void OnApplicationQuit()
    {
        if (GameDataController.Instance != null)
            SaveGame();
        Debug.Log("[GameManager] 遊戲數據已在退出時保存");
    }
}