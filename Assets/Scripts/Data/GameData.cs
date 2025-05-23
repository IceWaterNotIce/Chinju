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
        // 數值型欄位默認值
        private float _oils = 0;
        public float Oils
        {
            get => _oils;
            set
            {
                _oils = Mathf.Max(0, value); // 確保不為負
                OnResourceChanged();
            }
        }

        private float _gold = 0;
        public float Gold
        {
            get => _gold;
            set
            {
                _gold = Mathf.Max(0, value); // 確保不為負
                OnResourceChanged();
            }
        }

        private int _cube = 0;
        public int Cube
        {
            get => _cube;
            set
            {
                _cube = Mathf.Max(0, value); // 確保不為負
                OnResourceChanged();
            }
        }

        // 默認從1級開始
        public int Level = 1;
        public float Exp = 0;

        // 玩家擁有的船隻數據
        public List<ShipData> Ships = new List<ShipData>();

        public List<WeaponData> Weapons = new List<WeaponData>(); // 確保初始化

        // 初始化資源變動事件
        [System.NonSerialized]
        public System.Action OnResourceChanged = delegate { };

        public PlayerData()
        {
            OnResourceChanged = delegate { };
            _oils = 0;
            _gold = 0;
            _cube = 0;
            Level = 1;
            Exp = 0;
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
        // 改為全域枚舉
        public enum CombatMode { Peaceful, Defensive, Aggressive }
        public CombatMode Mode; // 使用新的枚舉

        public string Name;
        public int Health;
        public int AttackPower;
        public int Defense;
        public Vector3 Position;
        public float MaxFuel;
        public float CurrentFuel;
        public float FuelConsumptionRate;
        public float Speed;
        public float Rotation;
        public int WeaponLimit;
        public int Level;
        public float Experience;
        public List<WeaponData> Weapons = new List<WeaponData>();
        public string PrefabName;
        public Rect NavigationArea;

        // 燃料百分比屬性
        public float FuelPercent => MaxFuel > 0 ? CurrentFuel / MaxFuel : 0;

        // 狀態檢查
        public bool CanMove() => CurrentFuel > 0 && Health > 0;
    }

    [System.Serializable]
    public class WeaponData
    {
        public enum WeaponType { Primary, Secondary, Special }
        public WeaponType Type;

        public string Name;
        public int Damage;
        public float MaxAttackDistance;
        public float MinAttackDistance;
        public float AttackSpeed;
        public float CooldownTime;
        public string PrefabName;
        public int AmmoPerShot;

        public int MaxAmmo;
        private int _currentAmmo;
        public int CurrentAmmo
        {
            get => _currentAmmo;
            set => _currentAmmo = Mathf.Clamp(value, 0, MaxAmmo);
        }
    }
}
