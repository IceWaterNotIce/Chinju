using System.Collections.Generic;
using UnityEngine;

public class ShipManager : Singleton<ShipManager>
{
    private Dictionary<string, Ship> ships = new Dictionary<string, Ship>();

    public Ship GetShipById(string shipId)
    {
        ships.TryGetValue(shipId, out var ship);
        return ship;
    }

    public void RegisterShip(Ship ship)
    {
        if (!ships.ContainsKey(ship.ShipId)) // 修正：確保 ShipId 屬性存在於 Ship 類別
        {
            ships.Add(ship.ShipId, ship);
            Debug.Log($"[ShipManager] Registered ship: {ship.name} with ID: {ship.ShipId}");
        }
    }

    public void UnregisterShip(Ship ship)
    {
        if (ships.ContainsKey(ship.ShipId)) // 修正：確保 ShipId 屬性存在於 Ship 類別
        {
            ships.Remove(ship.ShipId);
            Debug.Log($"[ShipManager] Unregistered ship: {ship.name} with ID: {ship.ShipId}");
        }
    }
}
