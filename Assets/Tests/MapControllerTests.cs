using NUnit.Framework;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class MapControllerTests
{
    private class DummyMapController : MapController
    {
        public void SetSeed(int s) { seed = s; }
        public TileType TestGetTileTypeAt(int x, int y) => GetTileTypeAt(x, y);
        public void ClearNoiseCache() => _noiseCache.Clear();
    }

    [Test]
    public void SameSeed_GeneratesSameMap()
    {
        var ctrl1 = new DummyMapController();
        var ctrl2 = new DummyMapController();
        int testSeed = 123456;
        ctrl1.SetSeed(testSeed);
        ctrl2.SetSeed(testSeed);

        var results1 = new List<TileType>();
        var results2 = new List<TileType>();
        for (int x = -5; x <= 5; x++)
        {
            for (int y = -5; y <= 5; y++)
            {
                results1.Add(ctrl1.TestGetTileTypeAt(x, y));
                results2.Add(ctrl2.TestGetTileTypeAt(x, y));
            }
        }
        Assert.AreEqual(results1, results2, "同一 seed 下地圖生成結果應一致");
    }

    [Test]
    public void DifferentSeed_GeneratesDifferentMap()
    {
        var ctrl1 = new DummyMapController();
        var ctrl2 = new DummyMapController();
        ctrl1.SetSeed(111);
        ctrl2.SetSeed(222);

        bool anyDifferent = false;
        for (int x = -5; x <= 5 && !anyDifferent; x++)
        {
            for (int y = -5; y <= 5 && !anyDifferent; y++)
            {
                if (ctrl1.TestGetTileTypeAt(x, y) != ctrl2.TestGetTileTypeAt(x, y))
                    anyDifferent = true;
            }
        }
        Assert.IsTrue(anyDifferent, "不同 seed 下地圖生成結果應有差異");
    }

    [Test]
    public void ChinjuTile_AlwaysAtOrigin()
    {
        var ctrl = new DummyMapController();
        Assert.AreEqual(TileType.Chinju, ctrl.TestGetTileTypeAt(0, 0), "原點必須為神獸 Tile");
    }

    [Test]
    public void TileType_OnlyValidTypes()
    {
        var ctrl = new DummyMapController();
        for (int x = -3; x <= 3; x++)
        {
            for (int y = -3; y <= 3; y++)
            {
                var t = ctrl.TestGetTileTypeAt(x, y);
                Assert.IsTrue(
                    t == TileType.Ocean || t == TileType.Grass || t == TileType.Oil || t == TileType.Chinju,
                    $"TileType 不合法: {t}"
                );
            }
        }
    }
}
