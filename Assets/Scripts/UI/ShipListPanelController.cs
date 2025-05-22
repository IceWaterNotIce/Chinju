using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;
using System.Collections.Generic;

public class ShipListPanelController : MonoBehaviour
{
    public VisualElement root;
    public VisualElement shipListContainer;
    public Button closeButton;

    [SerializeField] private VisualTreeAsset shipTemplate;

    private void Awake()
    {
        // 獲取根元素
        var uiDocument = GetComponent<UIDocument>();
        root = uiDocument.rootVisualElement;
        shipListContainer = UIHelper.InitializeElement<VisualElement>(root, "GridContainer");
        closeButton = UIHelper.InitializeElement<Button>(root, "CloseButton");

        // 註冊面板到 PopupManager
        PopupManager.Instance.RegisterPopup("ShipPanel", gameObject);

    }

    void OnEnable()
    {
        // 僅在啟用時初始化，避免重複
        StartCoroutine(WaitForGameDataControllerInitialization());
    }

    private IEnumerator WaitForGameDataControllerInitialization()
    {
        while (GameDataController.Instance == null || GameDataController.Instance.CurrentGameData == null)
        {
            yield return null; // 等待下一幀
        }

        // 重新取得UI元素（若有動態生成UI的情境）
        var uiDocument = GetComponent<UIDocument>();
        root = uiDocument.rootVisualElement;
        shipListContainer = UIHelper.InitializeElement<VisualElement>(root, "GridContainer");

        closeButton = UIHelper.InitializeElement<Button>(root, "CloseButton");
        closeButton.RegisterCallback<ClickEvent>(ev => PopupManager.Instance.HidePopup("ShipPanel"));
        InitializeShipList();
    }

    private void InitializeShipList()
    {
        shipListContainer.Clear();

        var playerData = GameDataController.Instance.CurrentGameData.playerData;
        if (playerData == null || playerData.Ships == null || playerData.Ships.Count == 0)
        {
            var noShipLabel = new Label("目前沒有船隻可顯示。");
            noShipLabel.AddToClassList("no-ship-item");
            shipListContainer.Add(noShipLabel);
            return;
        }

        foreach (var ship in playerData.Ships)
        {
            if (shipTemplate == null)
            {
                Debug.LogError("[ShipListPanelController] 找不到 ShipTemplate.uxml");
                continue;
            }
            VisualElement shipElement = shipTemplate.Instantiate();

            var nameLabel = shipElement.Q<Label>("lblName");
            var levelLabel = shipElement.Q<Label>("lblLevel");
            if (nameLabel != null) nameLabel.text = ship.Name;
            if (levelLabel != null) levelLabel.text = $"Lv.{ship.Level}";

            shipListContainer.Add(shipElement);
            shipElement.AddToClassList("ship-item");
        }
    }
}