using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class FleetManager : Singleton<FleetManager>, GameManager.IFleetManager // 修改：使用完整命名空間
{
    public void CreateFleet(Warship[] warships)
    {
        if (warships == null || warships.Length == 0)
        {
            Debug.LogWarning("[FleetManager] No ships provided to create a fleet.");
            return;
        }
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

    // 新增：清除空艦隊
    public void RemoveEmptyFleets()
    {
        var allFleets = FindObjectsByType<Fleet>(FindObjectsSortMode.None);
        foreach (var fleet in allFleets)
        {
            if (fleet.followers == null || fleet.followers.Count == 0)
            {
                Debug.Log($"[FleetManager] Removing empty fleet: {fleet.name}");
                Destroy(fleet.gameObject);
            }
        }
    }

    // 新增：驗證艦隊的追隨者
    public void ValidateFleetFollowers()
    {
        var allFleets = FindObjectsByType<Fleet>(FindObjectsSortMode.None);
        foreach (var fleet in allFleets)
        {
            fleet.followers = fleet.followers.Where(ship => ship != null).ToList();
            Debug.Log($"[FleetManager] Validated fleet: {fleet.name}, remaining followers: {fleet.followers.Count}");
        }
    }

    public Fleet InstantiateFleetFromData(GameData.FleetData fleetData)
    {
        var fleet = new GameObject(fleetData.Name).AddComponent<Fleet>();
        fleet.FleetId = fleetData.FleetId;
        fleet.transform.position = fleetData.Position;
        fleet.Speed = fleetData.Speed;
        fleet.FlagshipId = fleetData.FlagshipId;

        if (fleetData.ShipIds == null || fleetData.ShipIds.Count == 0)
        {
            Debug.LogWarning($"[FleetManager] Fleet {fleetData.Name} has no ships, skipping instantiation.");
            return null;
        }

        foreach (var shipId in fleetData.ShipIds)
        {
            var ship = ShipManager.Instance?.GetShipById(shipId);
            if (ship != null)
            {
                fleet.AddShip(ship);
                ship.transform.SetParent(fleet.transform);
            }
            else
            {
                Debug.LogWarning($"[FleetManager] Ship with ID {shipId} not found for fleet {fleetData.Name}.");
            }
        }

        Debug.Log($"[FleetManager] InstantiateFleetFromData: {fleetData.Name} with {fleetData.ShipIds.Count} ships.");
        return fleet;
    }

    // 新增：存檔前的清理方法
    public void CleanupBeforeSave()
    {
        var allFleets = FindObjectsByType<Fleet>(FindObjectsSortMode.None);
        foreach (var fleet in allFleets)
        {
            // 移除無效的追隨者
            fleet.followers = fleet.followers
                .Where(s => s != null && !string.IsNullOrEmpty(s.ShipId))
                .ToList();

            // 如果變成空艦隊則銷毀
            if (fleet.followers.Count == 0)
            {
                Destroy(fleet.gameObject);
            }
        }
    }

    public void ResetAllFleets()
    {
        var allFleets = FindObjectsByType<Fleet>(FindObjectsSortMode.None);
        foreach (var fleet in allFleets)
        {
            Destroy(fleet.gameObject); // 銷毀所有艦隊物件
        }
        Debug.Log("[FleetManager] 已重置所有艦隊");
    }
}
