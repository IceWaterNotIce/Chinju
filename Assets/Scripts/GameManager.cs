using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using System; // 新增：解決 Exception 和 ArgumentException 無法識別的問題

#region GameManagerClass
public class GameManager : Singleton<GameManager>
{
    #region ConstantsAndFields
    public const string serverUrl = "https://icewaternotice.com/games/Word Curse/";
    public const string githubUrl = "https://raw.githubusercontent.com/IceWaterNotIce/Word-Curse/main/";

    public const int SaveDataVersion = 1; // 新增：存檔版本號

    [SerializeField]
    private string currentSaveFileName = "savegame.json"; // 統一檔案名稱
    [SerializeField]
    private float gameTime; // 遊戲時間（秒）

    // 遊戲內一天的秒數（現實 20 分鐘 = 遊戲 1 天，10 分鐘 = 12 小時）
    public const float RealSecondsPerGameDay = 600f * 2; // 1200 秒 = 1 天
    private const int GameSecondsPerDay = 24 * 60 * 60; // 86400 秒 = 1 天

    public const float RealGameTimeScale = 1200f;

    // 新增：靜態列表用於記錄船隻和艦隊
    private static List<GameObject> registeredShips = new List<GameObject>();
    private static List<GameObject> registeredFleets = new List<GameObject>();
    private static List<PlayerShip> registeredPlayerShips = new List<PlayerShip>();
    private static List<EnemyShip> registeredEnemyShips = new List<EnemyShip>();

    // 新增：初始配置參數
    [System.Serializable]
    public class InitialGameConfig
    {
        public int InitialOils = 200;
        public int InitialGold = 500;
        public int InitialCube = 100;
        public int MapWidth = 100;
        public int MapHeight = 100;
        public float IslandDensity = 0.1f;

        // 新增：驗證配置是否有效
        public bool IsValid()
        {
            return InitialOils >= 0 &&
                   InitialGold >= 0 &&
                   InitialCube >= 0 &&
                   MapWidth > 0 &&
                   MapHeight > 0 &&
                   IslandDensity >= 0 && IslandDensity <= 1;
        }
    }

    [SerializeField]
    private InitialGameConfig initialConfig = new InitialGameConfig();

    // 新增：自訂存檔資料夾路徑
    [SerializeField]
    private string customSaveDirectory = null;

    // 新增：PlayerPrefs 的鍵值常量
    private const string LastSaveFileKey = "LastSaveFileName";

    [SerializeField]
    private bool isPaused = false; // 新增：遊戲暫停狀態
    [SerializeField]
    private float pauseTimeScale = 0f; // 新增：暫停時的時間縮放

    [SerializeField]
    private float fleetValidationDelay = 1f; // 艦隊驗證延遲時間可配置化

    private readonly float gameSecondsPerRealSecond = GameSecondsPerDay / RealSecondsPerGameDay; // 改為 readonly 變數
    private float _cachedTimeScale; // 新增：緩存遊戲時間縮放比例

    private Dictionary<int, Action<GameData>> _upgradeActions = new()
    {
        { 1, data => { /* 版本1升級邏輯 */ } },
        { 2, data => { /* 版本2升級邏輯 */ } }
    };

    #endregion

    #region UnityLifecycle
    protected override void Awake()
    {
        base.Awake();
        registeredShips.Clear(); // 清理靜態列表避免記憶體洩漏
        registeredFleets.Clear();
        registeredPlayerShips.Clear();
        registeredEnemyShips.Clear();
        SceneManager.sceneLoaded += OnSceneLoaded; // 新增：場景切換事件
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ClearAllShipsAndFleets();
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded; // 移除事件
    }

    void Start()
    {
        _cachedTimeScale = GameSecondsPerDay / RealSecondsPerGameDay; // 新增：緩存計算
        // 新增：從 PlayerPrefs 讀取最後一次存檔檔名
        string lastSaveFile = PlayerPrefs.GetString(LastSaveFileKey, "savegame.json");
        SetCurrentSaveFileName(lastSaveFile);

        if (GameDataController.Instance != null &&
            GameDataController.Instance.CurrentGameData == null)
        {
            InitializeGameData();
        }
        else
        {
            LoadGame(currentSaveFileName);
        }

        Debug.Log("[GameManager] 初始化開始");
        // 預設存檔名稱
        currentSaveFileName = "savegame.json";
        // 不再直接設定 saveFilePath，改用方法動態取得
        Debug.Log("[GameManager] 初始化完成");
    }

    void Update()
    {
        if (!isPaused)
        {
            gameTime += Time.deltaTime * _cachedTimeScale; // 使用緩存值
        }
    }

    private void OnApplicationQuit()
    {
        if (GameDataController.Instance != null)
            SaveGame(); // 預設存檔
        Debug.Log("[GameManager] 遊戲數據已在退出時保存");
    }

    // 新增：暫停與恢復遊戲
    public void SetPause(bool pause)
    {
        isPaused = pause;
        Time.timeScale = pause ? pauseTimeScale : 1f; // 控制 Unity 時間縮放
    }
    #endregion

    #region GameDataInit
    private void InitializeGameData()
    {
        if (!initialConfig.IsValid()) // 新增：檢查配置有效性
        {
            Debug.LogError("[GameManager] InitialGameConfig 無效，請檢查配置參數！");
            return;
        }

        GameDataController.Instance.CurrentGameData = new GameData();
        GameDataController.Instance.TriggerResourceChanged();

        // 新增：驗證艦隊的追隨者
        FleetManager.Instance?.ValidateFleetFollowers();
    }
    #endregion

    #region Events
    // 修改：使用 UnityEvent 替代靜態事件，避免內存洩漏
    public UnityEngine.Events.UnityEvent OnGameSavedEvent = new UnityEngine.Events.UnityEvent();
    public UnityEngine.Events.UnityEvent OnGameLoadedEvent = new UnityEngine.Events.UnityEvent();
    #endregion

    #region SaveLoad
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
    /// 設定自訂存檔資料夾路徑（傳 null 則恢復預設 Application.persistentDataPath）
    /// </summary>
    public void SetCustomSaveDirectory(string directory)
    {
        if (!string.IsNullOrEmpty(directory))
        {
            customSaveDirectory = directory;
            // 若資料夾不存在則建立
            if (!Directory.Exists(customSaveDirectory))
                Directory.CreateDirectory(customSaveDirectory);
        }
        else
        {
            customSaveDirectory = Application.persistentDataPath; // 修改：恢復為參數化的預設路徑
        }
    }

    /// <summary>
    /// 取得目前存檔的完整路徑
    /// </summary>
    private string GetSaveFilePath(string fileName = null)
    {
        string name = fileName ?? currentSaveFileName;
        // 若有自訂資料夾則用自訂，否則用參數化的預設路徑
        string dir = string.IsNullOrEmpty(customSaveDirectory) ? Application.persistentDataPath : customSaveDirectory;
        return Path.Combine(dir, name);
    }

    /// <summary>
    /// 驗證並取得合法的存檔路徑
    /// </summary>
    private string GetValidatedSaveFilePath(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("檔名不能為空");

        if (fileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            throw new ArgumentException($"檔名包含非法字元: {fileName}");

        string dir = string.IsNullOrEmpty(customSaveDirectory) ? Application.persistentDataPath : customSaveDirectory;
        return Path.Combine(dir, fileName);
    }

    private void SaveGameInternal(GameData data, string fileName)
    {
        string path = GetValidatedSaveFilePath(fileName);

        if (File.Exists(path))
        {
            string backupPath = path + ".bak";
            File.Copy(path, backupPath, true);
            Debug.Log($"[GameManager] 已備份舊存檔至 {backupPath}");
        }

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(path, json);
        Debug.Log($"[GameManager] 遊戲已保存至 {path}");
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
                    var playerData = GameDataController.Instance.CurrentGameData.players.FirstOrDefault();
                    data.players[0].Oils = playerData.Oils;
                    data.players[0].Gold = playerData.Gold;
                    data.players[0].Cube = playerData.Cube;
                    data.players[0].Level = playerData.Level;
                    data.players[0].Exp = playerData.Exp;

                    // 保存玩家船隻數據（含艦隊編組/父子關係）
                    var playerShips = GameObject.FindObjectsByType<PlayerShip>(FindObjectsSortMode.None)
                        .Where(ship => ship != null)
                        .ToList();

                    data.players[0].Ships.Clear();
                    foreach (var ship in registeredPlayerShips)
                    {
                        data.players[0].Ships.Add(ship.SaveShipData());
                    }

                    // 保存敵人數據
                    var enemyShips = GameObject.FindObjectsByType<EnemyShip>(FindObjectsSortMode.None)
                        .Where(ship => ship != null)
                        .ToList();

                    data.enemyData.EnemyShips.Clear(); // 修正：改為使用 enemyData.EnemyShips
                    foreach (var ship in enemyShips) // 改用場景中實際存在的敵艦
                    {
                        var shipData = ship.SaveShipData();

                        // 保存武器數據
                        shipData.Weapons.Clear();
                        foreach (var weapon in ship.GetWeapons())
                        {
                            var weaponData = new GameData.WeaponData
                            {
                                WeaponId = weapon.WeaponId, // 保存唯一標識
                                Name = weapon.name,
                                Damage = (int)weapon.Damage,
                                MaxAttackDistance = weapon.MaxAttackDistance,
                                AttackSpeed = weapon.AttackSpeed,
                                PrefabName = weapon.Name
                            };
                            shipData.Weapons.Add(weaponData);
                        }

                        data.enemyData.EnemyShips.Add(shipData); // 修正：改為使用 enemyData.EnemyShips
                    }
                    Debug.Log($"[GameManager] 正在保存 {enemyShips.Count} 艘敵艦");
                    foreach (var ship in enemyShips)
                    {
                        Debug.Log($"[GameManager] 保存敵艦: {ship.name}, ID: {ship.ShipId}");
                    }

                    // 保存玩家艦隊數據
                    var allFleets = GameObject.FindObjectsByType<Fleet>(FindObjectsSortMode.None)
                        .Where(fleet => fleet != null)
                        .ToList();

                    data.players[0].Fleets.Clear();
                    data.enemyData.EnemyFleets.Clear(); // 修正：清空敵方艦隊列表

                    foreach (var fleet in allFleets)
                    {
                        var fleetData = fleet.SaveFleetData();
                        if (fleetData != null) // 只保存有效艦隊
                        {
                            if (fleet.IsPlayerFleet) // 判斷是否為玩家艦隊
                            {
                                data.players[0].Fleets.Add(fleetData);
                            }
                            else // 否則為敵方艦隊
                            {
                                data.enemyData.EnemyFleets.Add(fleetData);
                            }
                        }
                    }

                    // 儲存遊戲時間
                    data.gameTime = gameTime;

                    // 新增：設置存檔版本號
                    data.version = SaveDataVersion;

                    // 保存最後遊玩時間
                    data.lastPlayedTime = DateTime.Now.ToString("o"); // 新增：保存 ISO 格式的最後遊玩時間

                    // 保存彈藥池狀態
                    if (AmmoManager.Instance != null)
                    {
                        data.ammoStates = AmmoManager.Instance.SaveAmmoStates(); // 新增：保存彈藥位置
                    }

                    string json = JsonUtility.ToJson(data, true);
                    string path = GetSaveFilePath(fileName);

                    // === 新增：自動備份舊存檔 ===
                    if (File.Exists(path))
                    {
                        string backupPath = path + ".bak";
                        File.Copy(path, backupPath, true);
                        Debug.Log($"[GameManager] 已備份舊存檔至 {backupPath}");
                    }
                    // === 備份結束 ===

                    File.WriteAllText(path, json);
                    Debug.Log($"[GameManager] 遊戲已保存至 {path}");
                    // 新增：儲存最後一次存檔檔名
                    SetCurrentSaveFileName(Path.GetFileName(path));
                    OnGameSavedEvent.Invoke(); // 發送保存事件
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
    /// 非同步儲存遊戲，可指定檔名
    /// </summary>
    public async Task SaveGameAsync(string fileName = null)
    {
        if (GameDataController.Instance != null)
        {
            var data = GameDataController.Instance.CurrentGameData;
            if (data != null)
            {
                try
                {
                    string path = GetValidatedSaveFilePath(fileName ?? currentSaveFileName);
                    string json = JsonUtility.ToJson(data, true);

                    using (FileStream stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        byte[] jsonData = System.Text.Encoding.UTF8.GetBytes(json);
                        await stream.WriteAsync(jsonData, 0, jsonData.Length);
                    }

                    Debug.Log($"[GameManager] 遊戲已非同步保存至 {path}");
                    OnGameSavedEvent.Invoke();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[GameManager] 非同步儲存遊戲時發生錯誤: {ex.Message}");
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
    /// 升級存檔數據以適配新版本
    /// </summary>
    private GameData UpgradeSaveData(GameData data)
    {
        int currentVersion = data.version;
        while (currentVersion < SaveDataVersion)
        {
            if (_upgradeActions.TryGetValue(currentVersion, out var upgradeAction))
            {
                upgradeAction(data);
                currentVersion++;
                Debug.Log($"[存檔升級] 應用版本 {currentVersion} 升級");
            }
            else
            {
                Debug.LogError($"[存檔升級] 找不到版本 {currentVersion} 的升級處理");
                break;
            }
        }
        data.version = SaveDataVersion;
        return data;
    }

    /// <summary>
    /// 載入遊戲，可指定檔名
    /// </summary>
    public GameData LoadGame(string fileName = null, string filedir = null)
    {

        if (GameDataController.Instance != null && GameDataController.Instance.CurrentGameData == null)
        {
            InitializeGameData();
        }

        ClearAllShipsAndFleets();
        string dir = string.IsNullOrEmpty(filedir) ? Application.persistentDataPath : filedir;
        string path = Path.Combine(dir, fileName ?? currentSaveFileName); // 支持自訂檔案目錄和檔名

        if (File.Exists(path))
        {
            try
            {
                string json = File.ReadAllText(path);
                GameData data = JsonUtility.FromJson<GameData>(json);

                // 新增：升級存檔數據
                data = UpgradeSaveData(data);

                // 驗證存檔完整性，補齊缺失欄位
                if (data != null)
                {
                    if (data.players == null || data.players.Count == 0)
                        data.players = new List<GameData.PlayerData> { new GameData.PlayerData() };
                    if (data.mapData == null)
                        data.mapData = new GameData.MapData();
                    if (data.enemyData.EnemyShips == null)
                        data.enemyData.EnemyShips = new List<GameData.ShipData>();
                    if (data.version == 0)
                        data.version = SaveDataVersion;
                    if (data.players[0].Ships == null)
                        data.players[0].Ships = new List<GameData.ShipData>();
                    if (data.players[0].Weapons == null)
                        data.players[0].Weapons = new List<GameData.WeaponData>();
                    if (data.mapData.ChinjuTiles == null)
                        data.mapData.ChinjuTiles = new List<Vector3Int>();
                }
                else
                {
                    Debug.LogError("[GameManager] 存檔反序列化後為 null，檔案可能已損壞。");
                    return null;
                }

                // 檢查版本號
                if (data.version != SaveDataVersion)
                {
                    Debug.LogError($"[GameManager] 存檔版本不兼容，當前版本: {SaveDataVersion}，存檔版本: {data.version}。請升級或轉換存檔格式。");
                    return null;
                }

                if (data != null)
                {
                    if (GameDataController.Instance != null)
                    {
                        GameDataController.Instance.CurrentGameData = data;
                        // 載入玩家資源到 PlayerData
                        var playerData = GameDataController.Instance.CurrentGameData.players[0];
                        playerData.Oils = data.players[0].Oils;
                        playerData.Gold = data.players[0].Gold;
                        playerData.Cube = data.players[0].Cube;
                        playerData.Level = data.players[0].Level;
                        playerData.Exp = data.players[0].Exp;
                        Debug.Log("[GameManager] 遊戲數據已設置到 GameDataController");
                    }

                    // 使用 ShipManager 載入玩家船隻數據並實例化
                    foreach (var shipData in data.players[0].Ships)
                    {
                        if (ShipManager.Instance != null)
                        {
                            ShipManager.Instance.InstantiateShipFromData(shipData);
                        }
                        else
                        {
                            Debug.LogError("[GameManager] ShipManager 未初始化，無法實例化船隻！");
                        }
                    }

                    // 清除現有敵人船隻後載入新敵人
                    var existingEnemyShips = GameObject.FindObjectsByType<EnemyShip>(FindObjectsSortMode.None);
                    foreach (var enemyShip in existingEnemyShips)
                    {
                        GameObject.Destroy(enemyShip.gameObject);
                    }

                    foreach (var shipData in data.enemyData.EnemyShips)
                    {
                        if (EnemyShipManager.Instance != null)
                        {
                            EnemyShipManager.Instance.SpawnEnemyFromData(shipData);
                        }
                        else
                        {
                            Debug.LogError("[GameManager] EnemyShipManager 未初始化，無法生成敵人！");
                        }
                    }

                    // 使用 FleetManager 載入玩家艦隊數據並實例化
                    foreach (var fleetData in data.players[0].Fleets)
                    {
                        if (FleetManager.Instance != null)
                        {
                            FleetManager.Instance.InstantiateFleetFromData(fleetData);
                        }
                        else
                        {
                            Debug.LogError("[GameManager] FleetManager 未初始化，無法實例化艦隊！");
                        }
                    }

                    // 新增：清除空艦隊
                    if (FleetManager.Instance != null)
                    {
                        FleetManager.Instance.RemoveEmptyFleets();
                    }

                    // 載入遊戲時間
                    gameTime = data.gameTime;

                    GameDataController.Instance.TriggerResourceChanged();
                    OnGameLoadedEvent.Invoke(); // 發送載入事件

                    // 新增：載入遊戲後重繪地圖
                    MapController.Instance?.RecalculateMap();
                }
                else
                {
                    Debug.LogWarning("[GameManager] 載入的遊戲數據為 null");
                }

                // 新增：驗證艦隊的追隨者
                if (FleetManager.Instance != null)
                {
                    FleetManager.Instance.ValidateFleetFollowers();
                }

                if (DateTime.TryParse(data.lastPlayedTime, out DateTime lastPlayed))
                {
                    TimeSpan timeSinceLastPlayed = DateTime.Now - lastPlayed;
                    gameTime += (float)timeSinceLastPlayed.TotalSeconds; // 新增：根據系統時間更新遊戲時間
                }

                // 載入彈藥池狀態
                if (AmmoManager.Instance != null && data.ammoStates != null)
                {
                    AmmoManager.Instance.LoadAmmoStates(data.ammoStates); // 新增：載入彈藥位置
                }

                // 在
                FleetManager.Instance.RemoveEmptyFleets(); // 確保載入後清理空艦隊

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
    #endregion

    #region NewGame
    /// <summary>
    /// 開始新遊戲，會先儲存目前遊戲，再建立新遊戲資料並切換新檔案
    /// </summary>
    /// <param name="newSaveFileName">新遊戲存檔名稱（可為 null，預設自動產生）</param>
    /// <param name="mapSeed">地圖種子（可為 null，null 則隨機）</param>
    /// <param name="saveDir">自訂存檔資料夾（可為 null 或空字串）</param>
    public void StartNewGame(string newSaveFileName = null, int? mapSeed = null, string saveDir = null)
    {
        if (!initialConfig.IsValid()) // 新增：檢查配置有效性
        {
            Debug.LogError("[GameManager] InitialGameConfig 無效，無法開始新遊戲！");
            return;
        }

        // 設定自訂存檔資料夾（可為 null）
        SetCustomSaveDirectory(string.IsNullOrEmpty(saveDir) ? null : saveDir);

        // 1. 儲存目前遊戲（如果有資料）
        if (GameDataController.Instance != null && GameDataController.Instance.CurrentGameData != null)
        {
            SaveGame(); // 儲存到目前檔案
        }

        // 2. 切換目前存檔名稱
        currentSaveFileName = "savegame.json"; // 統一檔案名稱

        // 3. 清除現有船隻（玩家與敵人）
        ClearAllShipsAndFleets();


        // 4. 重置遊戲數據
        int seed = mapSeed ?? UnityEngine.Random.Range(0, int.MaxValue); // 新增：使用指定或隨機種子
        var newGameData = new GameData
        {
            players = new List<GameData.PlayerData>
            {
                new GameData.PlayerData
                {
                    Oils = initialConfig.InitialOils,
                    Gold = initialConfig.InitialGold,
                    Cube = initialConfig.InitialCube,
                    Ships = new List<GameData.ShipData>()
                }
            },
            mapData = new GameData.MapData
            {
                Seed = seed,
                Width = initialConfig.MapWidth,
                Height = initialConfig.MapHeight,
                IslandDensity = initialConfig.IslandDensity,
                ChinjuTiles = new List<Vector3Int>()
            }
        };

        // 5. 設定到 GameDataController
        if (GameDataController.Instance != null)
            GameDataController.Instance.CurrentGameData = newGameData;

        GameDataController.Instance.TriggerResourceChanged();

        // 新增：新遊戲後重繪地圖
        if (MapController.Instance != null)
            MapController.Instance.RecalculateMap();

        // 6. 立即儲存新遊戲檔案
        SaveGame();

        Debug.Log($"[GameManager] 新遊戲已開始，並儲存於 {currentSaveFileName}，資料夾：{customSaveDirectory}");
    }

    /// <summary>
    /// 清除場景中所有 Fleet、Ship、PlayerShip、EnemyShip 物件
    /// </summary>
    private void ClearAllShipsAndFleets()
    {
        Debug.Log("[Cleanup] 開始清除所有船隻和艦隊");

        foreach (var fleet in registeredFleets)
        {
            if (fleet != null)
                GameObject.Destroy(fleet);
        }
        registeredFleets.Clear();

        foreach (var ship in registeredShips)
        {
            if (ship != null)
                GameObject.Destroy(ship);
        }
        registeredShips.Clear();

        ShipManager.Instance?.ClearAllShips(); // 清除所有玩家船隻
        EnemyShipManager.Instance?.ClearAllEnemyShips(); // 清除所有敵人船隻

        // 確保 FleetManager 也清除內部狀態
        FleetManager.Instance?.ResetAllFleets();
    }
    #endregion

    #region TimeUtility
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
    #endregion

    #region FleetManagerInjection
    public interface IFleetManager
    {
        void RemoveEmptyFleets();
        void ValidateFleetFollowers();
        Fleet InstantiateFleetFromData(GameData.FleetData fleetData);
        // 可擴展其他艦隊操作契約
    }

    #region ShipRegistration
    public void RegisterPlayerShip(PlayerShip ship)
    {
        if (!registeredPlayerShips.Contains(ship))
            registeredPlayerShips.Add(ship);
    }

    public void RegisterEnemyShip(EnemyShip ship)
    {
        if (!registeredEnemyShips.Contains(ship))
            registeredEnemyShips.Add(ship);
    }

    public void RegisterFleet(GameObject fleet)
    {
        if (!registeredFleets.Contains(fleet))
            registeredFleets.Add(fleet);
    }
    #endregion
    #endregion
    #endregion
}
