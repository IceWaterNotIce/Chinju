using System;
using System.Collections.Generic;
using System.Linq; // 新增：解決 LINQ 擴充方法錯誤
using UnityEngine;

[System.Serializable]
public class GameData
{
    public const int SaveDataVersion = 1; // 新增：存檔版本常數

    [SerializeField] public int version = 1; // 新增：存檔版本號
    [SerializeField] public float gameTime;
    [SerializeField] public string lastPlayedTime; // 新增：保存最後遊玩時間
    [SerializeField] public MapData mapData; // 修正命名
    [SerializeField] public List<PlayerData> players = new List<PlayerData>(); // 新增：玩家列表
    [SerializeField] public EnemyData enemyData = new EnemyData(); // 新增：敵方數據
    [SerializeField] public List<Vector3> ammoStates = new List<Vector3>(); // 新增：保存彈藥位置
    [SerializeField] public bool isServer = false; // 新增：是否為伺服器

    public GameData UpgradeSaveData(GameData data)
    {
        if (data.version < SaveDataVersion)
        {
            Debug.Log($"[GameData] 升級存檔版本：{data.version} -> {SaveDataVersion}");

            if (data.version == 1)
            {
                // Example upgrade logic for version 1 to 2
                foreach (var weapon in data.players.SelectMany(p => p.Weapons))
                {
                    if (weapon.Damage < 0) weapon.Damage = 0; // 確保武器傷害值有效
                }
            }

            data.version = SaveDataVersion;
        }
        return data;
    }

    private bool ValidateData()
    {
        // Example validation logic
        return players.All(p => p.Oils >= 0 && p.Gold >= 0 && p.Cube >= 0) &&
               players.SelectMany(p => p.Weapons).All(w => w.Damage >= 0 && w.MaxAttackDistance > 0);
    }

    #region Map
    [System.Serializable]
    public class MapData
    {
        [SerializeField] public int Seed;
        [SerializeField] public int Width;
        [SerializeField] public int Height;
        [SerializeField] public float IslandDensity;
        [SerializeField] public List<Vector3Int> ChinjuTiles = new List<Vector3Int>(); // chinjuTile的座標
    }
    #endregion

    #region Player
    [System.Serializable]
    public class PlayerData
    {
        [SerializeField] public string PlayerId = Guid.NewGuid().ToString();
        [SerializeField] public string PlayerName = "Player";
        [SerializeField] public Color PlayerColor = Color.white;
        [SerializeField] public string Avatar;
        [SerializeField] public int Level = 1;
        [SerializeField] public float Exp = 0;
        public void TryLevelUp()
        {
            while (Exp >= Level * 10)
            {
                Exp -= Level * 10;
                Level++;
                // 可在此加入升級時的額外處理
            }
        }

        [SerializeField] public float Oils = 0;
        [SerializeField] public int Gold = 0;
        [SerializeField] public int Cube = 0;
        [System.NonSerialized] public System.Action OnResourceChanged = delegate { };

        [SerializeField] public List<FleetData> Fleets = new List<FleetData>(); // 新增：玩家艦隊數據
        [SerializeField] public List<ShipData> Ships = new List<ShipData>();
        [SerializeField] public List<WeaponData> Weapons = new List<WeaponData>(); // 確保初始化

        public PlayerData()
        {
            OnResourceChanged = delegate { };
            // 初始化時設置默認值
            Oils = 0;
            Gold = 0;
            Cube = 0;

            Level = 1;
            Exp = 0;
        }
    }
    #endregion

    #region Enemy
    [System.Serializable]
    public class EnemyData
    {
        [SerializeField] public List<ShipData> EnemyShips = new List<ShipData>(); // 敵方船隻列表
        [SerializeField] public List<FleetData> EnemyFleets = new List<FleetData>(); // 敵方艦隊列表
    }
    #endregion

    #region Fleet
    [System.Serializable]
    public class FleetData
    {
        [SerializeField] public string FleetId; // 唯一識別碼
        [SerializeField] public string Name; // 艦隊名稱
        [SerializeField] public List<string> ShipIds = new List<string>(); // 艦隊中的船隻ID列表
        [SerializeField] public Vector3 Position; // 艦隊位置
        [SerializeField] public float Speed; // 艦隊速度
        [SerializeField] public string FlagshipId; // 旗艦ID
    }
    #endregion

    #region Ship
    [System.Serializable]
    public class ShipData
    {
        [SerializeField] public string ShipId = Guid.NewGuid().ToString();
        [SerializeField] public string FleetId; 
        [SerializeField] public string Name;

        [SerializeField] public int Level;
        [SerializeField] public float Experience;

        [SerializeField] public float Health;
        [SerializeField] public int AttackPower;
        [SerializeField] public int Defense;
        public enum CombatMode { Peaceful, Defensive, Aggressive }
        [SerializeField] public CombatMode Mode; 

        [SerializeField] public Vector3 Position;
        [SerializeField] public float Speed;
        [SerializeField] public float Rotation;
        [SerializeField] public Rect NavigationArea;
        public bool CanMove() => CurrentFuel > 0 && Health > 0;

        [SerializeField] public float MaxFuel;
        [SerializeField] public float CurrentFuel;
        [SerializeField] public float FuelConsumptionRate;
        public float FuelPercent => MaxFuel > 0 ? CurrentFuel / MaxFuel : 0;
        
        [SerializeField] public int WeaponLimit;
        [SerializeField] public List<WeaponData> Weapons = new List<WeaponData>();
        [SerializeField] public string PrefabName;
    }
    #endregion

    #region Weapon
    [System.Serializable]
    public class WeaponData
    {
        [SerializeField] public string WeaponId = Guid.NewGuid().ToString(); // 新增唯一標識
        public enum WeaponType { Primary, Secondary, Special }
        [SerializeField] public WeaponType Type;
        [SerializeField] public string Name;

        [SerializeField] public int Damage;
        [SerializeField] public float MaxAttackDistance;
        [SerializeField] public float MinAttackDistance;
        [SerializeField] public float AttackSpeed;
        [SerializeField] public float CooldownTime;
        [SerializeField] public string PrefabName;
        [SerializeField] public int AmmoPerShot;
        [SerializeField] public int MaxAmmo;
        [SerializeField] private int _currentAmmo;
        public int CurrentAmmo
        {
            get => _currentAmmo;
            set => _currentAmmo = Mathf.Clamp(value, 0, MaxAmmo);
        }
    }
    #endregion
}