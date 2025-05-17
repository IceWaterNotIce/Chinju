using UnityEngine;
using UnityEngine.UIElements;

public class FleetUIController : MonoBehaviour
{
    public Fleet fleet;
    private VisualElement root;
    private VisualElement followerListContainer;
    private DropdownField formationDropdown;

    void Start()
    {
        var uiDoc = GetComponent<UIDocument>();
        if (uiDoc == null)
        {
            Debug.LogError("[FleetUIController] 無法取得 UIDocument");
            return;
        }
        root = uiDoc.rootVisualElement;
        followerListContainer = root.Q<VisualElement>("followerListContainer");
        formationDropdown = root.Q<DropdownField>("formationDropdown");

        if (formationDropdown != null)
        {
            formationDropdown.choices = new System.Collections.Generic.List<string>
            {
                "單縱隊", "雙縱隊", "橫隊", "圓形", "斜隊"
            };
            formationDropdown.RegisterValueChangedCallback(OnFormationChanged);
        }

        RefreshFollowerList();
    }

    public void SetFleet(Fleet f)
    {
        fleet = f;
        RefreshFollowerList();
        if (formationDropdown != null)
            formationDropdown.index = (int)fleet.formation;
    }

    private void OnFormationChanged(ChangeEvent<string> evt)
    {
        if (fleet == null) return;
        switch (evt.newValue)
        {
            case "單縱隊": fleet.formation = Fleet.FormationType.SingleLineAhead; break;
            case "雙縱隊": fleet.formation = Fleet.FormationType.DoubleLineAhead; break;
            case "橫隊": fleet.formation = Fleet.FormationType.LineAbreast; break;
            case "圓形": fleet.formation = Fleet.FormationType.CircularFormation; break;
            case "斜隊": fleet.formation = Fleet.FormationType.EchelonFormation; break;
        }
    }

    private void RefreshFollowerList()
    {
        if (followerListContainer == null || fleet == null) return;
        followerListContainer.Clear();
        for (int i = 0; i < fleet.followers.Count; i++)
        {
            var ship = fleet.followers[i];
            var label = new Label($"{i + 1}. {ship.name}");
            followerListContainer.Add(label);
        }
    }
}
