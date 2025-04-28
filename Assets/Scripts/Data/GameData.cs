using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData
{
    public PlayerData PlayerDatad; // 改為 public field
    public MapData MapDatad;       // 改為 public field

    [System.Serializable]
    public class PlayerData
    {
        public float Oils;
        public float Gold;
        public int Cube;

        // 玩家擁有的船隻數據
        public List<ShipData> Ships = new List<ShipData>();

        // 新增：資源變動事件
        [System.NonSerialized]
        public System.Action OnResourceChanged;
    }

    [System.Serializable]
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
        public string Name;
        public int Health;
        public int AttackPower;
        public int Defense;
        public Vector3 Position;
        public float Fuel;
        public float Speed;
        public float Rotation;

        public int WeaponLimit;

        public List<WeaponData> Weapons = new List<WeaponData>();
    }

    [System.Serializable]
    public class WeaponData
    {
        public string Name;
        public int Damage;
        public float Range;
        public float AttackSpeed;
        public float CooldownTime;
    }
}
