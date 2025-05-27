using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;

public class FleetListPanelController : MonoBehaviour
{
    public VisualTreeAsset fleetListUXML;
    public StyleSheet fleetListUSS;

    private VisualElement root;
    private ListView fleetListView;

    void Awake()
    {


        // 註冊 FleetPanel 到 PopupManager
        PopupManager.Instance.RegisterPopup("FleetPanel", gameObject);



    }

    void OnEnable()
    {

        var uiDocument = GetComponent<UIDocument>();
        root = uiDocument.rootVisualElement;
        // 綁定 ListView
        fleetListView = root.Q<ListView>("FleetListView");
        if (fleetListView != null)
        {
            // 取得所有玩家艦隊
            var fleets = FleetManager.Instance.GetAllPlayerFleets();

            // 準備顯示資料：每個艦隊一行，底下列出所有艦名
            var displayList = new System.Collections.Generic.List<string>();
            for (int i = 0; i < fleets.Length; i++)
            {
                var fleet = fleets[i];
                displayList.Add($"艦隊 {i + 1}（{fleet.followers.Count} 艘）");
                foreach (var ship in fleet.followers)
                {
                    displayList.Add($"　- {ship.name}");
                }
            }

            fleetListView.itemsSource = displayList;
            fleetListView.makeItem = () => new Label();
            fleetListView.bindItem = (e, i) => (e as Label).text = displayList[i];
        }
    }
}
