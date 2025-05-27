using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class FleetManager : Singleton<FleetManager>
{

    public void CreateFleet(Warship[] warships)
    {
        GameObject fleetParent = new GameObject("FleetGroup");
        // get the first ship parent
        var firstShip = warships.FirstOrDefault();
        var parentTransform = firstShip != null ? firstShip.transform.parent : null;
        if (parentTransform != null)
        {
            fleetParent.transform.SetParent(parentTransform);
        }
        else
        {
            fleetParent.transform.position = Vector3.zero; // 如果沒有父物件，則放在世界原點
        }


        foreach (var ship in warships)
        {
            ship.transform.SetParent(fleetParent.transform);
        }

        var fleet = fleetParent.AddComponent<Fleet>();

        for (int i = 0; i < warships.Length; i++)
        {
            var ship = warships[i];
            fleet.followers.Add(ship);
            if (i == 0)
            {
                ship.IsFollower = false;
                ship.LeaderShip = null;
            }
            else
            {
                ship.IsFollower = true;
                ship.LeaderShip = warships[0] as PlayerShip;
            }
        }

        Debug.Log($"[FleetManager] Created fleet with {warships.Length} ships.");
    }

    public void AddShipToFleet(Warship ship, Fleet fleet)
    {
        if (fleet == null || ship == null) return;

        fleet.followers.Add(ship);
        ship.IsFollower = true;
        ship.LeaderShip = fleet.followers[0] as PlayerShip; // 假設第一艦是領導者
        ship.transform.SetParent(fleet.transform);
    }

    public bool IsFleetLeader(Warship ship)
    {
        Fleet fleet = ship.transform.parent != null ? ship.transform.parent.GetComponent<Fleet>() : null;
        if (fleet == null || ship == null) return false;
        return fleet.followers.Count > 0 && fleet.followers[0] == ship;
    }

    // a void to get all player fleets
    public Fleet[] GetAllPlayerFleets()
    {
        return FindObjectsByType<Fleet>(FindObjectsSortMode.None).Where(f => f.followers.Count > 0 && f.followers[0] is PlayerShip).ToArray();
    }

    // ...可擴充更多管理功能...
}
