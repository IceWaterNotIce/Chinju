using UnityEngine;

public class FleetManager : Singleton<FleetManager>
{

    public void CreateFleet(Warship[] warships)
    {
        GameObject fleetParent = new GameObject("FleetGroup");
        fleetParent.transform.SetParent(ShipCreationManager.Instance.transform);

        foreach (var ship in warships)
        {
            ship.transform.SetParent(fleetParent.transform);
        }

        var fleet = fleetParent.AddComponent<Fleet>();
        foreach (var ship in warships)
        {
            fleet.followers.Add(ship);
            ship.IsFollower = true;
            ship.LeaderShip = warships[0] as PlayerShip;
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

    // ...可擴充更多管理功能...
}
