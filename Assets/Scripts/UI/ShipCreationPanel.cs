using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// 控制船隻建造面板的行為
/// </summary>
public class ShipCreationPanel : MonoBehaviour
{
    private UIDocument uiDocument;
    private VisualElement panel;
    private Button[] shipTypeBtns = new Button[5]; // 五種船型按鈕
    private Button createShipBtn;
    private Label goldCostLabel;
    private Label oilCostLabel;
    private Label cubeCostLabel;
    private IntegerField goldInputField;
    private IntegerField oilInputField;
    private IntegerField cubeInputField;

    // 建議建造成本（用於提升相近資源輸入時的隨機機率）
    private readonly int[,] shipCosts = {
        { 800, 400, 200 }, // 航空母艦
        { 500, 200, 100 }, // 戰艦
        { 300, 120, 60 },  // 巡洋艦
        { 200, 80, 40 },   // 驅逐艦
        { 150, 60, 30 }    // 潛艦
    };

    [SerializeField] private MapController mapController;

    private int selectedShipTypeIndex = -1; // -1 表示未選擇

    void Awake()
    {
        // 確保 GameDataController 已初始化
        if (GameDataController.Instance != null && GameDataController.Instance.CurrentGameData == null)
        {
            GameDataController.Instance.CurrentGameData = new GameData();
        }
        InitializeUI();
    }

    void OnEnable()
    {
        // 確保 GameDataController 已初始化
        if (GameDataController.Instance != null && GameDataController.Instance.CurrentGameData == null)
        {
            GameDataController.Instance.CurrentGameData = new GameData();
        }
        if (panel == null)
        {
            InitializeUI();
        }
    }

    private void InitializeUI()
    {
        Debug.Log("初始化船隻建造面板...");
        
        // 獲取 UIDocument
        uiDocument = GetComponent<UIDocument>();
        if (uiDocument == null)
        {
            Debug.LogError("找不到 UIDocument 組件！");
            return;
        }

        // 獲取根元素
        var root = uiDocument.rootVisualElement;
        if (root == null)
        {
            Debug.LogError("無法獲取 UI 根元素！");
            return;
        }

        // 獲取面板根元素
        panel = root.Q<VisualElement>("ship-creation-panel");
        if (panel == null)
        {
            Debug.LogError("找不到 ship-creation-panel 元素！");
            return;
        }

        // 取得五個船型按鈕
        for (int i = 0; i < 5; i++)
        {
            shipTypeBtns[i] = root.Q<Button>($"ship-type-btn-{i + 1}");
            int idx = i;
            if (shipTypeBtns[i] != null)
            {
                shipTypeBtns[i].clicked += () => SelectShipType(idx);
            }
            else
            {
                Debug.LogError($"找不到 ship-type-btn-{i + 1}！");
            }
        }

        createShipBtn = root.Q<Button>("create-ship-btn");
        goldCostLabel = root.Q<Label>("gold-cost");
        oilCostLabel = root.Q<Label>("oil-cost");
        cubeCostLabel = root.Q<Label>("cube-cost");

        // 新增：取得輸入欄位
        goldInputField = root.Q<IntegerField>("gold-input");
        oilInputField = root.Q<IntegerField>("oil-input");
        cubeInputField = root.Q<IntegerField>("cube-input");

        // 若找不到則自動建立（方便測試）
        if (goldInputField == null)
        {
            goldInputField = new IntegerField("金幣") { value = 10 };
            goldInputField.name = "gold-input";
            panel.Add(goldInputField);
        }
        if (oilInputField == null)
        {
            oilInputField = new IntegerField("石油") { value = 10 };
            oilInputField.name = "oil-input";
            panel.Add(oilInputField);
        }
        if (cubeInputField == null)
        {
            cubeInputField = new IntegerField("方塊") { value = 1 };
            cubeInputField.name = "cube-input";
            panel.Add(cubeInputField);
        }

        // 註冊輸入變更事件
        goldInputField.RegisterValueChangedCallback(evt => OnResourceInputChanged());
        oilInputField.RegisterValueChangedCallback(evt => OnResourceInputChanged());
        cubeInputField.RegisterValueChangedCallback(evt => OnResourceInputChanged());

        // 檢查必要元素
        if (createShipBtn == null) Debug.LogError("找不到 create-ship-btn！");
        if (goldCostLabel == null) Debug.LogError("找不到 gold-cost 標籤！");
        if (oilCostLabel == null) Debug.LogError("找不到 oil-cost 標籤！");
        if (cubeCostLabel == null) Debug.LogError("找不到 cube-cost 標籤！");

        if (createShipBtn != null)
        {
            createShipBtn.clicked += OnBuildButtonClicked;
            createShipBtn.SetEnabled(false);
        }

        // 初始化面板狀態
        panel.style.display = DisplayStyle.None;
        UpdateCostDisplay(0, 0, 0);

        Debug.Log("船隻建造面板初始化完成");
    }

    private void OnResourceInputChanged()
    {
        int gold = Mathf.Max(0, goldInputField.value);
        int oil = Mathf.Max(0, oilInputField.value);
        int cube = Mathf.Max(0, cubeInputField.value);
        UpdateCostDisplay(gold, oil, cube);

        // 啟用條件：有選擇船型且資源大於0，或沒選擇船型但資源大於10/10/1且有可負擔船型
        bool enable = false;
        if (selectedShipTypeIndex >= 0)
        {
            enable = gold > 0 && oil > 0 && cube > 0;
        }
        else
        {
            if (gold >= 10 && oil >= 10 && cube >= 1)
            {
                // 檢查有沒有可負擔的船型
                var gameData = GameDataController.Instance != null ? GameDataController.Instance.CurrentGameData : null;
                if (gameData != null && gameData.PlayerDatad != null)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        if (gameData.PlayerDatad.Gold >= shipCosts[i, 0] &&
                            gameData.PlayerDatad.Oils >= shipCosts[i, 1] &&
                            gameData.PlayerDatad.Cube >= shipCosts[i, 2])
                        {
                            enable = true;
                            break;
                        }
                    }
                }
            }
        }
        createShipBtn.SetEnabled(enable);
    }

    private void SelectShipType(int idx)
    {
        // 取消所有選擇
        for (int i = 0; i < shipTypeBtns.Length; i++)
        {
            shipTypeBtns[i].RemoveFromClassList("selected");
        }
        // 標記選擇
        shipTypeBtns[idx].AddToClassList("selected");
        selectedShipTypeIndex = idx;

        // 根據船型自動填入建造成本
        goldInputField.value = shipCosts[idx, 0];
        oilInputField.value = shipCosts[idx, 1];
        cubeInputField.value = shipCosts[idx, 2];

        OnResourceInputChanged();
    }

    /// <summary>
    /// 顯示面板
    /// </summary>
    public void Show()
    {
        Debug.Log("顯示船隻建造面板");
        if (panel != null)
        {
            panel.style.display = DisplayStyle.Flex;
        }
        else
        {
            Debug.LogError("無法顯示面板：panel 是 null");
        }
    }

    /// <summary>
    /// 隱藏面板
    /// </summary>
    public void Hide()
    {
        Debug.Log("隱藏船隻建造面板");
        if (panel != null)
        {
            panel.style.display = DisplayStyle.None;
            ResetPanel();
        }
        else
        {
            Debug.LogError("無法隱藏面板：panel 是 null");
        }
    }

    /// <summary>
    /// 切換面板顯示狀態
    /// </summary>
    public void Toggle()
    {
        Debug.Log("切換船隻建造面板顯示狀態");
        if (panel != null)
        {
            bool isVisible = panel.style.display == DisplayStyle.Flex;
            Debug.Log($"當前面板狀態：{(isVisible ? "顯示" : "隱藏")}");
            panel.style.display = isVisible ? DisplayStyle.None : DisplayStyle.Flex;
            
            if (isVisible)
            {
                ResetPanel();
            }
        }
        else
        {
            Debug.LogError("無法切換面板：panel 是 null");
        }
    }

    /// <summary>
    /// 重置面板狀態
    /// </summary>
    private void ResetPanel()
    {
        // 取消所有選擇
        for (int i = 0; i < shipTypeBtns.Length; i++)
        {
            shipTypeBtns[i].RemoveFromClassList("selected");
        }
        selectedShipTypeIndex = -1;
        createShipBtn.SetEnabled(false);
        UpdateCostDisplay(0, 0, 0);
    }

    /// <summary>
    /// 更新資源消耗顯示
    /// </summary>
    private void UpdateCostDisplay(int gold, int oil, int cube)
    {
        goldCostLabel.text = $"金幣: {gold}";
        oilCostLabel.text = $"石油: {oil}";
        cubeCostLabel.text = $"方塊: {cube}";
    }

    private void OnBuildButtonClicked()
    {
        if (selectedShipTypeIndex >= 0)
        {
            CreateShip();
        }
        else
        {
            CreateRandomShip();
        }
    }

    /// <summary>
    /// 創建戰艦
    /// </summary>
    private void CreateShip()
    {
        if (selectedShipTypeIndex < 0) return;

        int goldCost = Mathf.Max(0, goldInputField.value);
        int oilCost = Mathf.Max(0, oilInputField.value);
        int cubeCost = Mathf.Max(0, cubeInputField.value);

        // 檢查資源是否足夠
        var gameData = GameDataController.Instance != null ? GameDataController.Instance.CurrentGameData : null;
        if (GameDataController.Instance == null)
        {
            Debug.LogError("GameDataController.Instance 為 null，無法建造船隻！");
            return;
        }
        if (gameData == null)
        {
            Debug.LogWarning("CurrentGameData 為 null，自動初始化新遊戲資料。");
            GameDataController.Instance.CurrentGameData = new GameData();
            gameData = GameDataController.Instance.CurrentGameData;
        }
        if (gameData.PlayerDatad == null)
        {
            Debug.LogWarning("PlayerDatad 為 null，自動初始化 PlayerDatad。");
            gameData.PlayerDatad = new GameData.PlayerData();
        }
        if (gameData.PlayerDatad.Gold < goldCost ||
            gameData.PlayerDatad.Oils < oilCost ||
            gameData.PlayerDatad.Cube < cubeCost)
        {
            Debug.LogWarning("資源不足，無法建造戰艦！");
            return;
        }

        // 扣除資源
        gameData.PlayerDatad.Gold -= goldCost;
        gameData.PlayerDatad.Oils -= oilCost;
        gameData.PlayerDatad.Cube -= cubeCost;
        // 觸發資源變動事件
        gameData.PlayerDatad.OnResourceChanged?.Invoke();

        // 創建戰艦實例
        InstantiateShip(selectedShipTypeIndex);

        Debug.Log("開始建造船隻");

        // 重置選擇狀態
        ResetPanel();
    }

    /// <summary>
    /// 隨機建造船隻
    /// </summary>
    private void CreateRandomShip()
    {
        var gameData = GameDataController.Instance != null ? GameDataController.Instance.CurrentGameData : null;
        if (GameDataController.Instance == null || gameData == null || gameData.PlayerDatad == null)
        {
            Debug.LogError("GameDataController 或 PlayerDatad 為 null，無法隨機建造船隻！");
            return;
        }

        // 檢查資源是否足夠最低門檻
        if (gameData.PlayerDatad.Gold < 10 || gameData.PlayerDatad.Oils < 10 || gameData.PlayerDatad.Cube < 1)
        {
            Debug.LogWarning("資源不足，無法隨機建造船隻！");
            return;
        }

        // 取得目前輸入資源
        int inputGold = Mathf.Max(0, goldInputField.value);
        int inputOil = Mathf.Max(0, oilInputField.value);
        int inputCube = Mathf.Max(0, cubeInputField.value);

        // 計算每個可負擔船型的權重（距離越小權重越高）
        System.Collections.Generic.List<int> candidates = new System.Collections.Generic.List<int>();
        System.Collections.Generic.List<int> weights = new System.Collections.Generic.List<int>();
        for (int i = 0; i < 5; i++)
        {
            if (gameData.PlayerDatad.Gold >= shipCosts[i, 0] &&
                gameData.PlayerDatad.Oils >= shipCosts[i, 1] &&
                gameData.PlayerDatad.Cube >= shipCosts[i, 2])
            {
                // 距離 = 三種資源差值的總和
                int dist = Mathf.Abs(inputGold - shipCosts[i, 0])
                         + Mathf.Abs(inputOil - shipCosts[i, 1])
                         + Mathf.Abs(inputCube - shipCosts[i, 2]);
                // 權重 = 100 - 距離（最小為1）
                int weight = Mathf.Max(1, 100 - dist);
                candidates.Add(i);
                weights.Add(weight);
            }
        }
        if (candidates.Count == 0)
        {
            Debug.LogWarning("沒有任何船型可隨機建造！");
            return;
        }

        // 加權隨機選擇
        int totalWeight = 0;
        foreach (var w in weights) totalWeight += w;
        int rand = Random.Range(0, totalWeight);
        int chosenIdx = 0;
        for (int i = 0; i < weights.Count; i++)
        {
            if (rand < weights[i])
            {
                chosenIdx = i;
                break;
            }
            rand -= weights[i];
        }
        int idx = candidates[chosenIdx];

        // 扣除資源
        gameData.PlayerDatad.Gold -= shipCosts[idx, 0];
        gameData.PlayerDatad.Oils -= shipCosts[idx, 1];
        gameData.PlayerDatad.Cube -= shipCosts[idx, 2];
        gameData.PlayerDatad.OnResourceChanged?.Invoke();

        // 建造
        InstantiateShip(idx);

        Debug.Log($"隨機建造船型 {idx + 1}");

        // 重置面板
        ResetPanel();
    }

    /// <summary>
    /// 實例化戰艦
    /// </summary>
    private void InstantiateShip(int shipTypeIdx)
    {
        // 根據 shipTypeIdx 載入不同預製件
        string[] prefabNames = { "Prefabs/Carrier", "Prefabs/Ship", "Prefabs/Cruiser", "Prefabs/Destroyer", "Prefabs/Submarine" };
        GameObject shipPrefab = Resources.Load<GameObject>(prefabNames[shipTypeIdx]);
        if (shipPrefab == null)
        {
            Debug.LogError($"找不到船隻預製件：{prefabNames[shipTypeIdx]}！");
            return;
        }

        // 使用 MapController 找到 Chinju Tile 附近的最近海洋格子
        if (mapController == null)
        {
            Debug.LogError("MapController 未設置！");
            return;
        }

        Vector3 chinjuTilePosition = mapController.GetChinjuTileWorldPosition();
        if (chinjuTilePosition == Vector3.zero)
        {
            Debug.LogError("找不到 Chinju Tile 的位置！");
            return;
        }

        Vector3 spawnPosition = mapController.FindNearestOceanTile(chinjuTilePosition);
        if (spawnPosition == Vector3.zero)
        {
            Debug.LogError("找不到 Chinju Tile 附近的最近海洋格子！");
            return;
        }

        // 固定 z 座標為 -1
        spawnPosition.z = -1;

        // 在場景中生成戰艦
        GameObject battleShip = Instantiate(shipPrefab, spawnPosition, Quaternion.identity);
        if (battleShip != null)
        {
            Debug.Log("戰艦實例化成功！");

            // 儲存船隻資料到 GameData
            var gameData = GameDataController.Instance != null ? GameDataController.Instance.CurrentGameData : null;
            if (gameData != null && gameData.PlayerDatad != null)
            {
                var shipData = new GameData.ShipData
                {
                    Name = "戰艦",
                    Health = 100,
                    AttackPower = 20,
                    Defense = 10,
                    Position = spawnPosition,
                    Fuel = 100,
                    Speed = 5,
                    Rotation = 0
                };
                gameData.PlayerDatad.Ships.Add(shipData);
                Debug.Log("已將新戰艦資料存入 GameData");
            }
            else
            {
                Debug.LogWarning("無法儲存船隻資料到 GameData，PlayerDatad 為 null");
            }
        }
        else
        {
            Debug.LogError("戰艦實例化失敗！");
        }
    }
}