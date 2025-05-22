using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData
{
    public PlayerData playerData; // 修正命名
    public MapData mapData;       // 修正命名

    // 新增：遊戲時間（秒）
    public float gameTime;

    // 新增：敵方船隻數據
    public List<ShipData> enemyShips = new List<ShipData>();

    [System.Serializable]
    public class PlayerData
    {
        public float Oils;
        public float Gold;
        public int Cube;

        // 新增：玩家等級與經驗值
        public int Level;
        public float Exp;

        // 玩家擁有的船隻數據
        public List<ShipData> Ships = new List<ShipData>();

        public List<WeaponData> Weapons = new List<WeaponData>(); // 確保初始化

        // 初始化資源變動事件
        [System.NonSerialized]
        public System.Action OnResourceChanged = delegate { };

        public PlayerData()
        {
            OnResourceChanged = delegate { };
        }

        // 嘗試升級：經驗值大於等於 Level * 10 時升級
        public void TryLevelUp()
        {
            while (Exp >= Level * 10)
            {
                Exp -= Level * 10;
                Level++;
                // 可在此加入升級時的額外處理
            }
        }
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
        public float MaxFuel; // 新增：保存船隻的最大燃料
        public float CurrentFuel;

        public float FuelConsumptionRate; // 新增：保存船隻的燃料消耗率
        public float Speed;
        public float Rotation;



        public bool CombatMode; // 新增：保存船隻的戰鬥模式
        public int WeaponLimit;

        public int Level; // 新增：保存船隻等級
        public float Experience; // 新增：保存船隻經驗值

        public List<WeaponData> Weapons = new List<WeaponData>();

        // 新增：保存船隻的預製物名稱
        public string PrefabName;

        public Rect NavigationArea; // 新增：保存矩形區域
    }

    [System.Serializable]
    public class WeaponData
    {
        public string Name;
        public int Damage;
        public float MaxAttackDistance; // 新增
        public float MinAttackDistance; // 新增
        public float AttackSpeed;
        public float CooldownTime;

        // 新增：保存武器的預製物名稱
        public string PrefabName;

        public int AmmoPerShot;


    }
}
