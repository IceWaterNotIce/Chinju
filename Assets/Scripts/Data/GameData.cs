using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData
{
    public class PlayerData
    {
        public float Oils;
        public float Gold;
        public int Cube;

        // 玩家擁有的船隻數據
        public List<ShipData> Ships = new List<ShipData>();
    }

    public class MapData
    {
        public int Seed;
        public int Width;
        public int Height;
        public float IslandDensity;

        // chinjuTile的座標
        public List<Vector3Int> ChinjuTiles = new List<Vector3Int>();
    }

    [System.Serializable]
    public class ShipData
    {
        public Vector3 Position;
        public float Health;
        public float Fuel;
        public float Speed;
        public float Rotation;
    }
}
