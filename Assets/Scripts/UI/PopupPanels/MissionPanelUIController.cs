using UnityEngine;
using UnityEngine.UIElements;

public class MissionPanelUIController : MonoBehaviour
{
    private VisualElement missionListContainer;

    void Awake()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        missionListContainer = root.Q<VisualElement>("MissionListContainer");

        PopupManager.Instance.RegisterPopup("MissionPanel", gameObject);
    }

    void OnEnable()
    {
        RefreshMissionList();
    }

    public void RefreshMissionList()
    {
        missionListContainer.Clear();
        foreach (var mission in MissionManager.Instance.Missions)
        {
            var item = new Label($"{mission.Title} - {(mission.IsCompleted ? "已完成" : "進行中")}");
            item.AddToClassList("mission-item");
            missionListContainer.Add(item);
        }
    }
}
