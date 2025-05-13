using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;
using System.Collections.Generic;

public class ShipListPanelController : MonoBehaviour
{
    public VisualElement root;
    public VisualElement shipListContainer;
    public Button closeButton;

    private void Awake()
    {
        // 獲取根元素
        var uiDocument = GetComponent<UIDocument>();
        root = uiDocument.rootVisualElement;
        shipListContainer = root.Q<VisualElement>("ShipListContainer");
        closeButton = root.Q<Button>("CloseButton");

        // 註冊面板到 PopupManager
        PopupManager.Instance.RegisterPopup("ShipListPanel", gameObject);

        // 註冊關閉按鈕事件
        closeButton.RegisterCallback<ClickEvent>(ev => PopupManager.Instance.HidePopup("ShipListPanel"));
    }

    private void Start()
    {
        // 初始化船隻列表
        InitializeShipList();
    }

    void OnEnable()
    {
        Debug.Log("[ShipListPanelController] OnEnable 方法執行。");

        StartCoroutine(WaitForGameDataControllerInitialization());
    }

    private IEnumerator WaitForGameDataControllerInitialization()
    {
        while (GameDataController.Instance == null || GameDataController.Instance.CurrentGameData == null)
        {
            Debug.LogWarning("[ShipListPanelController] 等待 GameDataController 初始化...");
            yield return null; // 等待下一幀
        }

        var root = GetComponent<UIDocument>().rootVisualElement;

        shipListContainer = UIHelper.InitializeElement<VisualElement>(root, "ShipListContainer");
        if (shipListContainer == null) yield break;

        shipListContainer.Clear(); // 清空清單

        var playerData = GameDataController.Instance.CurrentGameData.playerData;
        if (playerData != null && playerData.Ships != null)
        {
            Debug.Log($"[ShipListPanelController] 玩家船隻數量: {playerData.Ships.Count}");
            if (playerData.Ships.Count > 0)
            {
                foreach (var ship in playerData.Ships)
                {
                    Debug.Log($"[ShipListPanelController] 船隻: {ship.Name}, HP: {ship.Health}, 等級: {ship.Level}");
                    var item = new Label($"{ship.Name} (HP: {ship.Health}, 等級: {ship.Level})");
                    item.AddToClassList("ship-item");
                    shipListContainer.Add(item);
                }
            }
            else
            {
                Debug.LogWarning("[ShipListPanelController] 玩家沒有船隻。");
                var noShipLabel = new Label("目前沒有船隻可顯示。");
                noShipLabel.AddToClassList("no-ship-item");
                shipListContainer.Add(noShipLabel);
            }
        }
        else
        {
            Debug.LogError("[ShipListPanelController] 玩家資料或船隻清單為空。");
        }
    }

    private void InitializeShipList()
    {
        // 清空現有的船隻列表
        shipListContainer.Clear();

        // 獲取所有船隻數據
        List<GameData.ShipData> allShips = GameDataController.Instance.CurrentGameData.playerData.Ships;

        // 創建每艘船的顯示元素
        foreach (var ship in allShips)
        {
            VisualElement shipElement = new VisualElement();
            shipElement.Add(new Label(ship.Name));
            shipElement.Add(new Label($"HP: {ship.Health}"));
            shipElement.Add(new Label($"Level: {ship.Level}"));

            shipListContainer.Add(shipElement);
            shipElement.AddToClassList("ship-item"); // 添加樣式類

            Debug.Log($"[ShipListPanelController] 船隻名稱: {ship.Name}, HP: {ship.Health}, 等級: {ship.Level}");
        }
    }
}