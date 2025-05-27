using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class MapControllerTests
{
    private class DummyMapController : ScriptableObject
    {
        public int seed;
        protected Dictionary<Vector2Int, float> _noiseCache = new Dictionary<Vector2Int, float>();
        public float islandDensity = 0.1f;

        public TileType TestGetTileTypeAt(int x, int y)
        {
            if (x == 0 && y == 0)
                return TileType.Chinju;

            int gx = x / 2;
            int gy = y / 2;
            // 使用 double 精度來避免 PerlinNoise 單精度導致的碰撞
            double noiseSeed = (double)seed;
            double noiseValue = Mathf.PerlinNoise((float)(gx * 0.1 + noiseSeed * 0.1), (float)(gy * 0.1 + noiseSeed * 0.1));
            if (noiseValue > 1f - islandDensity)
            {
                double oilNoise = Mathf.PerlinNoise((float)((gx + noiseSeed) * 0.2), (float)((gy + noiseSeed) * 0.2));
                if (oilNoise > 0.7f)
                    return TileType.Oil;
                return TileType.Grass;
            }
            return TileType.Ocean;
        }

        protected float GetCachedNoise(int x, int y)
        {
            var key = new Vector2Int(x, y);
            if (!_noiseCache.TryGetValue(key, out float value))
            {
                value = Mathf.PerlinNoise(x * 0.1f + seed, y * 0.1f + seed);
                _noiseCache[key] = value;
            }
            return _noiseCache[key];
        }

        public void SetSeed(int s) { seed = s; _noiseCache.Clear(); }
        public void ClearNoiseCache() => _noiseCache.Clear();
    }

    [Test]
    public void SameSeed_GeneratesSameMap()
    {
        var ctrl1 = ScriptableObject.CreateInstance<DummyMapController>();
        var ctrl2 = ScriptableObject.CreateInstance<DummyMapController>();
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
        var ctrl1 = ScriptableObject.CreateInstance<DummyMapController>();
        var ctrl2 = ScriptableObject.CreateInstance<DummyMapController>();
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
        var ctrl = ScriptableObject.CreateInstance<DummyMapController>();
        Assert.AreEqual(TileType.Chinju, ctrl.TestGetTileTypeAt(0, 0), "原點必須為神獸 Tile");
    }

    [Test]
    public void TileType_OnlyValidTypes()
    {
        var ctrl = ScriptableObject.CreateInstance<DummyMapController>();
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
