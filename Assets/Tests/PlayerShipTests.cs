using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections.Generic;

public class PlayerShipTests
{
    private GameObject shipObj;
    private PlayerShip playerShip;

    [SetUp]
    public void SetUp()
    {
        shipObj = new GameObject();
        playerShip = shipObj.AddComponent<PlayerShip>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(shipObj);
    }

    [Test]
    public void NavigationArea_SetAndGet()
    {
        var rect = new Rect(0, 0, 10, 10);
        playerShip.NavigationArea = rect;
        Assert.AreEqual(rect, playerShip.NavigationArea);
    }

    [Test]
    public void Waypoints_AddAndClear()
    {
        playerShip.AddWaypoint(Vector3.one);
        Assert.AreEqual(1, playerShip.Waypoints.Count);
        playerShip.ClearWaypoints();
        Assert.AreEqual(0, playerShip.Waypoints.Count);
    }

    [Test]
    public void HealthRegeneration_OverTime()
    {
        float initialHealth = playerShip.Health;
        // 模擬 61 秒，需手動調整 Time.deltaTime
        for (int i = 0; i < 61; i++)
        {
            // 模擬每次 Update 都經過 1 秒
            typeof(Time).GetField("deltaTime", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)
                ?.SetValue(null, 1f);
            playerShip.Update();
        }
        Assert.GreaterOrEqual(playerShip.Health, initialHealth + 1);
    }

    [Test]
    public void SaveAndLoadShipData_PreservesNavigationArea()
    {
        var rect = new Rect(1, 2, 3, 4);
        playerShip.NavigationArea = rect;
        var data = playerShip.SaveShipData();

        var newObj = new GameObject();
        var newShip = newObj.AddComponent<PlayerShip>();
        newShip.LoadShipData(data);

        Assert.AreEqual(rect, newShip.NavigationArea);
        Object.DestroyImmediate(newObj);
    }
}
