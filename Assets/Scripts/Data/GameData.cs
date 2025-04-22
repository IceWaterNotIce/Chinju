using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData
{
    public PlayerData PlayerDatad { get; set; } // 修正名稱
    public MapData MapDatad { get; set; } // 修正名稱

    public class PlayerData 
    {
        public float Oils { get; set; }
        public float Gold { get; set; }
        public int Cube { get; set; }

        // 玩家擁有的船隻數據
        public List<ShipData> Ships { get; set; } = new List<ShipData>();
    }

    public class MapData
    {
        public int Seed { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public float IslandDensity { get; set; }

        // chinjuTile的座標
        public List<Vector3Int> ChinjuTiles { get; set; } = new List<Vector3Int>();
    }

    [System.Serializable]
    public class ShipData
    {
        public string Name { get; set; }
        public int Health { get; set; }
        public int AttackPower { get; set; }
        public int Defense { get; set; }
        public Vector3 Position { get; set; }
        public float Fuel { get; set; } // Add Fuel
        public float Speed { get; set; } // Add Speed
        public float Rotation { get; set; } // Add Rotation
    }
}
