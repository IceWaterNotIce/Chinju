using System.Collections.Generic;
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

    public Fleet InstantiateFleetFromData(GameData.FleetData fleetData)
    {
        // 根據 FleetData 創建艦隊實例
        var fleet = new GameObject(fleetData.Name).AddComponent<Fleet>();
        fleet.FleetId = fleetData.FleetId;
        fleet.transform.position = fleetData.Position;
        fleet.Speed = fleetData.Speed;
        fleet.FlagshipId = fleetData.FlagshipId;

        // 將船隻加入艦隊
        foreach (var shipId in fleetData.ShipIds)
        {
            var ship = ShipManager.Instance?.GetShipById(shipId);
            if (ship != null)
            {
                fleet.AddShip(ship);
                ship.transform.SetParent(fleet.transform); // 確保父物件正確
            }
        }

        Debug.Log($"[FleetManager] InstantiateFleetFromData: {fleetData.Name} with {fleetData.ShipIds.Count} ships.");
        return fleet;
    }
}
