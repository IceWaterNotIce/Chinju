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

    private string currentSaveFileName = "savegame.json";
    private float gameTime; // 遊戲時間（秒）

    // 遊戲內一天的秒數（現實 20 分鐘 = 遊戲 1 天，10 分鐘 = 12 小時）
    public const float RealSecondsPerGameDay = 600f * 2; // 1200 秒 = 1 天
    private const int GameSecondsPerDay = 24 * 60 * 60; // 86400 秒 = 1 天

    public const float RealGameTimeScale = 1200f;

    public delegate void GameEvent();
    public static event GameEvent OnGameSaved;
    public static event GameEvent OnGameLoaded;

    private const string LastSaveFileKey = "LastSaveFileName"; // 新增：PlayerPrefs key

    protected override void Awake()
    {
        base.Awake();
    }

    void Start()
    {
        // 新增：從 PlayerPrefs 讀取最後一次存檔檔名
        string lastSaveFile = PlayerPrefs.GetString(LastSaveFileKey, "savegame.json");
        SetCurrentSaveFileName(lastSaveFile);

        if (GameDataController.Instance != null && GameDataController.Instance.CurrentGameData == null)
        {
            InitializeGameData();
        }

        Debug.Log("[GameManager] 初始化開始");
        // 預設存檔名稱
        currentSaveFileName = "savegame.json";
        // 不再直接設定 saveFilePath，改用方法動態取得
        Debug.Log("[GameManager] 初始化完成");

        // 修改：載入最後一次存檔
        LoadGame(currentSaveFileName);
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

    /// <summary>
    /// 設定目前操作的存檔檔名（含副檔名 .json）
    /// </summary>
    public void SetCurrentSaveFileName(string fileName)
    {
        if (!fileName.EndsWith(".json"))
            fileName += ".json";
        currentSaveFileName = fileName;
        // 新增：儲存到 PlayerPrefs
        PlayerPrefs.SetString(LastSaveFileKey, currentSaveFileName);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// 取得目前存檔的完整路徑
    /// </summary>
    private string GetSaveFilePath(string fileName = null)
    {
        string name = fileName ?? currentSaveFileName;
        return Path.Combine(Application.persistentDataPath, name);
    }

    /// <summary>
    /// 儲存遊戲，可指定檔名
    /// </summary>
    public void SaveGame(string fileName = null)
    {
        if (GameDataController.Instance != null)
        {
            var data = GameDataController.Instance.CurrentGameData;

            if (data != null)
            {
                try
                {
                    // 保存玩家資源（確保最新值）
                    var playerData = GameDataController.Instance.CurrentGameData.playerData;
                    data.playerData.Oils = playerData.Oils;
                    data.playerData.Gold = playerData.Gold;
                    data.playerData.Cube = playerData.Cube;
                    data.playerData.Level = playerData.Level;
                    data.playerData.Exp = playerData.Exp;

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
                    string path = GetSaveFilePath(fileName);
                    File.WriteAllText(path, json);
                    Debug.Log($"[GameManager] 遊戲已保存至 {path}");
                    // 新增：儲存最後一次存檔檔名
                    SetCurrentSaveFileName(Path.GetFileName(path));
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

    /// <summary>
    /// 載入遊戲，可指定檔名
    /// </summary>
    public GameData LoadGame(string fileName = null)
    {
        string path = GetSaveFilePath(fileName);
        if (File.Exists(path))
        {
            try
            {
                string json = File.ReadAllText(path);
                GameData data = JsonUtility.FromJson<GameData>(json);

                if (data != null)
                {
                    if (GameDataController.Instance != null)
                    {
                        GameDataController.Instance.CurrentGameData = data;
                        // 載入玩家資源到 PlayerData
                        var playerData = GameDataController.Instance.CurrentGameData.playerData;
                        playerData.Oils = data.playerData.Oils;
                        playerData.Gold = data.playerData.Gold;
                        playerData.Cube = data.playerData.Cube;
                        playerData.Level = data.playerData.Level;
                        playerData.Exp = data.playerData.Exp;
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
            Debug.LogWarning($"[GameManager] 找不到存檔文件: {path}");
        }

        return null;
    }

    /// <summary>
    /// 取得所有現有存檔檔名（*.json）
    /// </summary>
    public List<string> GetAllSaveFiles()
    {
        var files = Directory.GetFiles(Application.persistentDataPath, "*.json");
        return files.Select(f => Path.GetFileName(f)).ToList();
    }

    /// <summary>
    /// 開始新遊戲，會先儲存目前遊戲，再建立新遊戲資料並切換新檔案
    /// </summary>
    /// <param name="newSaveFileName">新遊戲存檔名稱（可為 null，預設自動產生）</param>
    /// <param name="mapSeed">地圖種子（可為 null，null 則隨機）</param>
    public void StartNewGame(string newSaveFileName = null, int? mapSeed = null)
    {
        // 1. 儲存目前遊戲（如果有資料）
        if (GameDataController.Instance != null && GameDataController.Instance.CurrentGameData != null)
        {
            SaveGame(); // 儲存到目前檔案
        }

        // 2. 產生新檔名
        if (string.IsNullOrEmpty(newSaveFileName))
        {
            string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
            newSaveFileName = $"savegame_{timestamp}.json";
        }
        else if (!newSaveFileName.EndsWith(".json"))
        {
            newSaveFileName += ".json";
        }

        // 3. 切換目前存檔名稱
        SetCurrentSaveFileName(newSaveFileName);

        // 4. 清除現有船隻（玩家與敵人）
        var existingFleets = GameObject.FindObjectsByType<Fleet>(FindObjectsSortMode.None);
        foreach (var fleet in existingFleets)
        {
            GameObject.Destroy(fleet.gameObject);
        }
        var existingShips = GameObject.FindObjectsByType<Ship>(FindObjectsSortMode.None);
        foreach (var ship in existingShips)
        {
            GameObject.Destroy(ship.gameObject);
        }
        var playerShips = GameObject.FindObjectsByType<PlayerShip>(FindObjectsSortMode.None);
        foreach (var ship in playerShips)
        {
            GameObject.Destroy(ship.gameObject);
        }
        var enemyShips = GameObject.FindObjectsByType<EnemyShip>(FindObjectsSortMode.None);
        foreach (var ship in enemyShips)
        {
            GameObject.Destroy(ship.gameObject);
        }

        // 5. 重置遊戲數據
        int seed = mapSeed ?? Random.Range(0, int.MaxValue); // 新增：使用指定或隨機種子
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
                Seed = seed,
                Width = 100,
                Height = 100,
                IslandDensity = 0.1f,
                ChinjuTiles = new List<Vector3Int>()
            }
        };

        // 6. 設定到 GameDataController
        if (GameDataController.Instance != null)
            GameDataController.Instance.CurrentGameData = newGameData;

        GameDataController.Instance.TriggerResourceChanged();

        // 7. 立即儲存新遊戲檔案
        SaveGame();

        Debug.Log($"[GameManager] 新遊戲已開始，並儲存於 {newSaveFileName}");
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
            SaveGame(); // 預設存檔
        Debug.Log("[GameManager] 遊戲數據已在退出時保存");
    }
}