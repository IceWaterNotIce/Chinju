using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using System;
using System.Collections.Generic;
using System.Linq; // 新增引用

public class Ship : MonoBehaviour, IPointerClickHandler
{
    #region Core Systems
    private ShipUI UI => ShipUI.Instance;
    #endregion

    #region Health & Fuel
    [Header("Health Settings")]
    [SerializeField] private float m_maxHealth = 100f;
    [SerializeField] private float m_health = 100f;
    [SerializeField] private float m_fuelConsumption = 0.1f;

    public float MaxHealth
    {
        get => m_maxHealth;
        set => m_maxHealth = Mathf.Max(0, value);
    }

    public virtual float Health
    {
        get => m_health;
        set
        {
            m_health = Mathf.Clamp(value, 0, m_maxHealth);
            OnHealthChanged?.Invoke(m_health);

            if (m_health <= 0)
            {
                OnDeath();
            }
        }
    }

    private void OnDeath()
    {
        Debug.Log($"[Ship] {name} 被摧毀！");
        
        // 獲得經驗值給擊殺者
        if (!IsPlayerShip)
        {
            var playerShip = GameObject.FindObjectsByType<Ship>(FindObjectsSortMode.None)
                .FirstOrDefault(ship => ship.IsPlayerShip);
            if (playerShip != null)
            {
                playerShip.AddExperience(10f); // 擊殺敵方船隻獲得固定經驗值
            }
        }

        // 從 GameData 中移除該船隻數據
        if (GameDataController.Instance != null && GameDataController.Instance.CurrentGameData != null)
        {
            var gameData = GameDataController.Instance.CurrentGameData;

            if (IsPlayerShip)
            {
                gameData.playerData.Ships.RemoveAll(ship => ship.Name == name);
                Debug.Log($"[Ship] 玩家船隻 {name} 已從 GameData 中移除。");
            }
            else
            {
                gameData.enemyShips.RemoveAll(ship => ship.Name == name);
                Debug.Log($"[Ship] 敵方船隻 {name} 已從 GameData 中移除。");
            }
        }

        // 銷毀遊戲物件
        Destroy(gameObject);
    }

    public float FuelConsumption
    {
        get => m_fuelConsumption;
        set => m_fuelConsumption = Mathf.Max(0, value);
    }

    public event Action<float> OnHealthChanged;
    #endregion

    #region Movement
    [Header("Movement Settings")]
    [SerializeField] private float m_maxSpeed = 10f;
    [SerializeField] private float m_acceleration = 2f;
    [SerializeField] private float m_targetSpeed = 0f;
    [SerializeField] private float m_speed = 0f;

    public float MaxSpeed
    {
        get => m_maxSpeed;
        set => m_maxSpeed = Mathf.Max(0, value);
    }

    public float Acceleration
    {
        get => m_acceleration;
        set => m_acceleration = Mathf.Max(0, value);
    }

    public float TargetSpeed
    {
        get => m_targetSpeed;
        set => m_targetSpeed = Mathf.Clamp(value, 0, m_maxSpeed);
    }

    public float Speed
    {
        get => m_speed;
        set => m_speed = Mathf.Clamp(value, 0, m_maxSpeed);
    }
    #endregion

    #region Rotation
    [Header("Rotation Settings")]
    [SerializeField] private float m_maxRotationSpeed = 90f;
    [SerializeField] private float m_rotationAcceleration = 45f;
    [SerializeField] private float m_targetRotation = 0f;
    [SerializeField] private float m_targetRotationSpeed = 0f;
    [SerializeField] private float m_rotationSpeed = 0f;

    public float MaxRotationSpeed
    {
        get => m_maxRotationSpeed;
        set => m_maxRotationSpeed = Mathf.Max(0, value);
    }

    public float RotationAcceleration
    {
        get => m_rotationAcceleration;
        set => m_rotationAcceleration = Mathf.Max(0, value);
    }

    public float TargetRotation
    {
        get => m_targetRotation;
        set => m_targetRotation = value % 360f; // Normalize angle
    }

    public float TargetRotationSpeed
    {
        get => m_targetRotationSpeed;
        set => m_targetRotationSpeed = Mathf.Clamp(value, -m_maxRotationSpeed, m_maxRotationSpeed);
    }

    public float RotationSpeed
    {
        get => m_rotationSpeed;
        set => m_rotationSpeed = Mathf.Clamp(value, -m_maxRotationSpeed, m_maxRotationSpeed);
    }
    #endregion

    #region Combat & Detection
    [Header("Combat Settings")]
    [SerializeField] private float m_detectionDistance = 50f;
    [SerializeField] private bool m_combatMode = false;

    [Header("Visible Area")]
    [SerializeField] private float m_visibleRadius = 10f; // 玩家可見半徑

    public float DetectionDistance
    {
        get => m_detectionDistance;
        set => m_detectionDistance = Mathf.Max(0, value);
    }

    public bool CombatMode
    {
        get => m_combatMode;
        set => m_combatMode = value;
    }

    public float VisibleRadius
    {
        get => m_visibleRadius;
        set => m_visibleRadius = Mathf.Max(0, value);
    }
    #endregion

    #region Experience
    [Header("Experience Settings")]
    [SerializeField] private int level = 1;
    [SerializeField] private float experience = 0f;

    public int Level
    {
        get => level;
        private set => level = Mathf.Max(1, value);
    }

    public float Experience
    {
        get => experience;
        private set => experience = Mathf.Max(0, value);
    }

    public void AddExperience(float exp)
    {
        Experience += exp;
        Debug.Log($"[Ship] 獲得經驗值: {exp}，當前經驗值: {Experience}");

        float upgradeNeed = Level * 10f;
        while (Experience >= upgradeNeed)
        {
            Experience -= upgradeNeed;
            Level += 1;
            Debug.Log($"[Ship] 升級！當前等級: {Level}");
            upgradeNeed = Level * 10f;
        }
    }
    #endregion

    #region Waypoints
    private List<Vector3> m_waypoints = new List<Vector3>();
    public IReadOnlyList<Vector3> Waypoints => m_waypoints.AsReadOnly();

    public void AddWaypoint(Vector3 point) => m_waypoints.Add(point);
    public void ClearWaypoints() => m_waypoints.Clear();
    #endregion

    #region Unity Events
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Ship clicked", this);
        
        if (UI == null)
        {
            Debug.LogError("[Ship] ShipUI.Instance 為 null，無法初始化 UI！");
            return;
        }

        UI.Initial(this);
    }
    #endregion

    [SerializeField] public Tilemap tilemap; // 引用地圖的 Tilemap，改為 public
    [SerializeField] public TileBase oceanTile; // 引用海洋 Tile，改為 public

    // 新增：武器數量上限
    [Header("Weapon Settings")]
    [SerializeField] private int intWeaponLimit = 2;
    public int IntWeaponLimit
    {
        get => intWeaponLimit;
        set => intWeaponLimit = Mathf.Max(0, value);
    }

    // public Weapon weapon;
    public List<Weapon> weapons = new List<Weapon>();

    [Header("Ship Type")]
    public bool IsPlayerShip = true; // 新增：標記是否為玩家船隻

    public void Start()
    {
        tilemap = FindFirstObjectByType<Tilemap>();
        if (tilemap == null)
        {
            Debug.LogError("Tilemap not found in the scene!", this);
            return;
        }
        // Resource/Tilemap/OceanTile
        oceanTile = Resources.Load<TileBase>("Tilemap/OceanTile");
        if (oceanTile == null)
        {
            Debug.LogError("Ocean Tile not found in Resources!", this);
            return;
        }
    }

    public void Update()
    {
        Rotate();
        Move();
        DetectAndAttackTarget();
    }

    private void DetectAndAttackTarget()
    {
        Ship nearestTarget = null;
        float nearestDistance = float.MaxValue;

        // 獲取所有船隻
        var allShips = GameObject.FindObjectsByType<Ship>(FindObjectsSortMode.None);

        foreach (var ship in allShips)
        {
            // 忽略自己和同類型的船隻
            if (ship == this || ship.IsPlayerShip == this.IsPlayerShip) continue;

            // 計算與目標船隻的距離
            float distance = Vector3.Distance(transform.position, ship.transform.position);

            // 如果進入偵測範圍，並且距離比當前最近的目標更近
            if (distance <= DetectionDistance && distance < nearestDistance)
            {
                nearestTarget = ship;
                nearestDistance = distance;
            }
        }

        // 如果找到最近的目標，並且進入武器最大攻擊距離，開始攻擊
        if (nearestTarget != null && nearestDistance <= MaxWeaponAttackDistance())
        {
            Debug.Log($"[Ship] 更新目標為最近的船隻: {nearestTarget.name}，距離: {nearestDistance}");
            AttackTarget(nearestTarget.gameObject);
        }
    }

    private float MaxWeaponAttackDistance()
    {
        // 返回當前武器的最大攻擊距離
        float maxDistance = 0f;
        foreach (var weapon in weapons)
        {
            if (weapon != null)
            {
                maxDistance = Mathf.Max(maxDistance, weapon.MaxAttackDistance);
            }
        }
        return maxDistance;
    }

    void Rotate()
    {
        if (Mathf.Abs(m_targetRotationSpeed) > 0.01f)
        {
            // 以速度控制旋轉
            m_rotationSpeed = Mathf.MoveTowards(m_rotationSpeed, m_targetRotationSpeed, m_rotationAcceleration * Time.deltaTime);
            transform.rotation = Quaternion.Euler(0, 0, transform.rotation.eulerAngles.z + m_rotationSpeed * Time.deltaTime);
        }
        else
        {
            // 自動補間到 TargetRotation
            float currentZ = transform.rotation.eulerAngles.z;
            float delta = Mathf.DeltaAngle(currentZ, m_targetRotation);

            if (Mathf.Abs(delta) < 0.1f)
            {
                transform.rotation = Quaternion.Euler(0, 0, m_targetRotation);
                m_rotationSpeed = 0f;
            }
            else
            {
                float maxStep = m_maxRotationSpeed * Time.deltaTime;
                float step = Mathf.Clamp(delta, -maxStep, maxStep);
                transform.rotation = Quaternion.Euler(0, 0, currentZ + step);
                m_rotationSpeed = step / Time.deltaTime;
            }
        }
    }

    void Move()
    {
        m_speed = Mathf.MoveTowards(m_speed, m_targetSpeed, m_acceleration * Time.deltaTime);
        Vector3 newPosition = transform.position + transform.right * Speed * Time.deltaTime;

        // 檢查新位置是否為海洋 Tile
        Vector3Int tilePosition = tilemap.WorldToCell(newPosition);
        if (tilemap.GetTile(tilePosition) == oceanTile)
        {
            transform.position = newPosition;
        }
        else
        {
            Debug.LogWarning("無法移動到非海洋 Tile 的位置！");
        }
    }

    public void AttackTarget(GameObject target)
    {
        foreach (var weapon in weapons)
        {
            if (weapon != null)
            {
                weapon.StartAttack(target);
            }
        }
    }

    public void StopAttack()
    {
        foreach (var weapon in weapons)
        {
            if (weapon != null)
            {
                weapon.StopAttack();
            }
        }
    }

    public GameData.ShipData SaveShipData()
    {
        GameData.ShipData shipData = new GameData.ShipData
        {
            Name = this.name, // 保存船隻名稱
            Position = transform.position,
            Health = (int)Health,
            Fuel = (int)FuelConsumption, // 顯式轉換
            Speed = Speed,
            Rotation = transform.rotation.eulerAngles.z,
            WeaponLimit = IntWeaponLimit, // 保存武器數量上限
            Level = Level, // 保存等級
            Experience = Experience, // 保存經驗值
            Weapons = new List<GameData.WeaponData>() // 保存武器數據
        };

        foreach (var weapon in weapons)
        {
            if (weapon != null)
            {
                shipData.Weapons.Add(weapon.SaveWeaponData()); // 使用 SaveWeaponData 方法
            }
        }

        return shipData;
    }

    public void LoadShipData(GameData.ShipData shipData)
    {
        // 根據 shipData 初始化船隻屬性
        this.name = shipData.Name; // 載入船隻名稱
        this.transform.position = shipData.Position;
        this.transform.rotation = Quaternion.Euler(0, 0, shipData.Rotation);
        this.IntWeaponLimit = shipData.WeaponLimit; // 載入武器數量上限
        this.Level = shipData.Level; // 載入等級
        this.Experience = shipData.Experience; // 載入經驗值

        // 清空現有武器
        foreach (var weapon in weapons)
        {
            if (weapon != null)
            {
                Destroy(weapon.gameObject);
            }
        }
        weapons.Clear();

        // 載入武器數據
        foreach (var weaponData in shipData.Weapons)
        {
            var weaponPrefab = Resources.Load<GameObject>($"Prefabs/Weapon/{weaponData.PrefabName}");
            if (weaponPrefab != null)
            {
                var weaponObj = Instantiate(weaponPrefab, transform);
                var weaponComp = weaponObj.GetComponent<Weapon>();
                if (weaponComp != null)
                {
                    weaponComp.name = weaponData.Name;
                    weaponComp.Damage = weaponData.Damage;
                    weaponComp.MaxAttackDistance = weaponData.MaxAttackDistance;
                    weaponComp.MinAttackDistance = weaponData.MinAttackDistance;
                    weaponComp.AttackSpeed = weaponData.AttackSpeed;
                    weaponComp.CooldownTime = weaponData.CooldownTime;
                    weapons.Add(weaponComp);
                }
            }
            else
            {
                Debug.LogWarning($"[Ship] 找不到武器預製物: {weaponData.PrefabName}");
            }
        }

        Debug.Log($"[Ship] 已載入船隻數據: {shipData.Name}");
    }

    public void AddRandomWeapon()
    {
        if (weapons.Count >= IntWeaponLimit)
        {
            Debug.LogWarning("[Ship] 武器數量已達上限，無法添加新武器！");
            return;
        }

        var weaponPrefabs = Resources.LoadAll<Weapon>("Prefabs/Weapon");
        if (weaponPrefabs.Length == 0)
        {
            Debug.LogError("[Ship] 無法找到任何武器預製體！");
            return;
        }

        var randomWeaponPrefab = weaponPrefabs[UnityEngine.Random.Range(0, weaponPrefabs.Length)]; // 明確使用 UnityEngine.Random
        var newWeapon = Instantiate(randomWeaponPrefab, transform);
        weapons.Add(newWeapon);

        Debug.Log($"[Ship] 隨機生成武器：{newWeapon.Name}");
    }

    public void AddWeapon(Weapon weapon)
    {
        // 將武器添加到船隻的武器列表或其他結構中
        Debug.Log($"[Ship] 添加武器: {weapon.name}");
    }

    #region Debug
    private void OnValidate()
    {
        // Auto-clamp values in Inspector
        Health = m_health;
        TargetSpeed = m_targetSpeed;
        TargetRotationSpeed = m_targetRotationSpeed;
        m_visibleRadius = Mathf.Max(0, m_visibleRadius);
    }

    private void OnDrawGizmosSelected()
    {
        // 在 Scene 視窗中繪製可見半徑
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, m_visibleRadius);
    }
    #endregion
}