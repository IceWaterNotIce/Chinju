using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData
{
    [SerializeField]
    public int version = 1; // 新增：存檔版本號

    [SerializeField]
    public PlayerData playerData; // 修正命名
    [SerializeField]
    public MapData mapData;       // 修正命名

    [SerializeField]
    public float gameTime;

    [SerializeField]
    public List<ShipData> enemyShips = new List<ShipData>();

    [System.Serializable]
    public class PlayerData
    {
        [SerializeField]
        public float Oils = 0;
        [SerializeField]
        public int Gold = 0;
        [SerializeField]
        public int Cube = 0;

        [SerializeField]
        public int Level = 1;
        [SerializeField]
        public float Exp = 0;

        [SerializeField]
        public List<ShipData> Ships = new List<ShipData>();

        [SerializeField]
        public List<WeaponData> Weapons = new List<WeaponData>(); // 確保初始化

        [SerializeField]
        public List<FleetData> Fleets = new List<FleetData>(); // 新增：玩家艦隊數據

        // 初始化資源變動事件
        [System.NonSerialized]
        public System.Action OnResourceChanged = delegate { };

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
        [SerializeField]
        public int Seed;
        [SerializeField]
        public int Width;
        [SerializeField]
        public int Height;
        [SerializeField]
        public float IslandDensity;

        // chinjuTile的座標
        [SerializeField]
        public List<Vector3Int> ChinjuTiles = new List<Vector3Int>();
    }

    [System.Serializable]
    public class ShipData
    {
        // 改為全域枚舉
        public enum CombatMode { Peaceful, Defensive, Aggressive }
        [SerializeField]
        public CombatMode Mode; // 使用新的枚舉

        [SerializeField]
        public string Name;
        [SerializeField]
        public int Health;
        [SerializeField]
        public int AttackPower;
        [SerializeField]
        public int Defense;
        [SerializeField]
        public Vector3 Position;
        [SerializeField]
        public float MaxFuel;
        [SerializeField]
        public float CurrentFuel;
        [SerializeField]
        public float FuelConsumptionRate;
        [SerializeField]
        public float Speed;
        [SerializeField]
        public float Rotation;
        [SerializeField]
        public int WeaponLimit;
        [SerializeField]
        public int Level;
        [SerializeField]
        public float Experience;
        [SerializeField]
        public List<WeaponData> Weapons = new List<WeaponData>();
        [SerializeField]
        public string PrefabName;
        [SerializeField]
        public Rect NavigationArea;

        [SerializeField]
        public string ShipId = Guid.NewGuid().ToString(); // 改為 GUID 確保唯一性
        [SerializeField]
        public string FleetId; // 所屬艦隊Id，可為null
        [SerializeField]
        public string ParentShipId; // 父船Id（如有母艦/僚艦關係）

        // 燃料百分比屬性
        public float FuelPercent => MaxFuel > 0 ? CurrentFuel / MaxFuel : 0;

        // 狀態檢查
        public bool CanMove() => CurrentFuel > 0 && Health > 0;
    }

    [System.Serializable]
    public class WeaponData
    {
        public enum WeaponType { Primary, Secondary, Special }
        [SerializeField]
        public WeaponType Type;

        [SerializeField]
        public string Name;
        [SerializeField]
        public int Damage;
        [SerializeField]
        public float MaxAttackDistance;
        [SerializeField]
        public float MinAttackDistance;
        [SerializeField]
        public float AttackSpeed;
        [SerializeField]
        public float CooldownTime;
        [SerializeField]
        public string PrefabName;
        [SerializeField]
        public int AmmoPerShot;
        [SerializeField]
        public int MaxAmmo;
        [SerializeField]
        private int _currentAmmo;
        public int CurrentAmmo
        {
            get => _currentAmmo;
            set => _currentAmmo = Mathf.Clamp(value, 0, MaxAmmo);
        }

        [SerializeField]
        public string WeaponId = Guid.NewGuid().ToString(); // 新增唯一標識
    }

    [System.Serializable]
    public class FleetData
    {
        [SerializeField]
        public string FleetId; // 唯一識別碼
        [SerializeField]
        public string Name; // 艦隊名稱
        [SerializeField]
        public List<string> ShipIds = new List<string>(); // 艦隊中的船隻ID列表
        [SerializeField]
        public Vector3 Position; // 艦隊位置
        [SerializeField]
        public float Speed; // 艦隊速度
        [SerializeField]
        public string FlagshipId; // 旗艦ID
    }

    [SerializeField]
    public string lastPlayedTime; // 新增：保存最後遊玩時間
    [SerializeField]
    public List<Vector3> ammoStates = new List<Vector3>(); // 新增：保存彈藥位置

    public GameData UpgradeSaveData(GameData data)
    {
        if (data.version < SaveDataVersion)
        {
            Debug.Log($"[GameData] 升級存檔版本：{data.version} -> {SaveDataVersion}");
            if (data.playerData == null)
                data.playerData = new PlayerData();
            if (data.mapData == null)
                data.mapData = new MapData();
            if (data.playerData.Ships == null)
                data.playerData.Ships = new List<ShipData>();
            if (data.enemyShips == null)
                data.enemyShips = new List<ShipData>();
            if (data.mapData.ChinjuTiles == null)
                data.mapData.ChinjuTiles = new List<Vector3Int>();
            if (data.playerData.Fleets == null)
                data.playerData.Fleets = new List<FleetData>();

            // 新增：遷移邏輯示例
            foreach (var ship in data.playerData.Ships)
            {
                if (string.IsNullOrEmpty(ship.ShipId))
                    ship.ShipId = Guid.NewGuid().ToString();
            }

            data.version = SaveDataVersion;
        }
        return data;
    }
}
