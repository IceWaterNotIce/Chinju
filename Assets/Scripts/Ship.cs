using UnityEngine;
using UnityEngine.Tilemaps;
using System;
using System.Collections.Generic;
using Unity.Netcode;

public class Ship : NetworkBehaviour
{
    public string ShipId { get; set; } 


    public bool IsFollower;
    public Ship LeaderShip;


    [Header("Navigation Settings")]
    [SerializeField] private Rect m_navigationArea;
    protected float m_navigationUpdateTimer = 0f;    public Rect NavigationArea
    {
        get => m_navigationArea;
        set
        {
            Debug.Log($"[Ship] NavigationArea set to {value}");
            m_navigationArea = value;
            if (m_navigationArea != Rect.zero)
            {
                TargetSpeed = 2f;
            }
            else
            {
                Debug.LogWarning("[Ship] NavigationArea is set to Rect.zero, navigation disabled.");
            }
        }
    }

    protected List<Vector3> m_waypoints = new List<Vector3>();
    public IReadOnlyList<Vector3> Waypoints => m_waypoints.AsReadOnly();
    public void AddWaypoint(Vector3 point) => m_waypoints.Add(point);
    public void ClearWaypoints() => m_waypoints.Clear();

    #region Health & Fuel
    [Header("Health Settings")]
    [SerializeField] protected float m_maxHealth = 100f;
    [SerializeField] protected float m_health = 100f;

    [Header("Fuel Settings")]
    [SerializeField] protected float m_maxFuel = 100f;
    [SerializeField] protected float m_fuel = 100f;
    [SerializeField] protected float m_fuelConsumption = 0.1f;

    public float MaxHealth { get => m_maxHealth; set => m_maxHealth = Mathf.Max(0, value); }
    public float FuelConsumptionRate { get => m_fuelConsumption; set => m_fuelConsumption = Mathf.Max(0, value); }
    public float MaxFuel { get => m_maxFuel; set => m_maxFuel = Mathf.Max(0, value); }

    public virtual float Health
    {
        get => m_health;
        set
        {
            m_health = Mathf.Clamp(value, 0, m_maxHealth);
            OnHealthChanged?.Invoke(m_health);
            if (m_health <= 0) OnDeath();
        }
    }

    public float CurrentFuel
    {
        get => m_fuel;
        set
        {
            m_fuel = Mathf.Clamp(value, 0, m_maxFuel);
            OnFuelChanged?.Invoke(m_fuel);
            if (m_fuel <= 0) StopMovement();
        }
    }


    private Vector3 m_cachedDirection;
    private Vector3 m_cachedTarget;
    private Vector3 m_cachedCenter;
    private Vector3 m_cachedNextPosition;


    public event Action<float> OnHealthChanged;
    public event Action<float> OnFuelChanged;

    protected virtual void OnDeath()
    {
        Destroy(gameObject);
        if (this is PlayerShip && ShipDetailPanel.Instance != null)
        {
            Destroy(ShipDetailPanel.Instance.gameObject); 
            Debug.Log("[Ship] ShipDetailPanel 已銷毀");
        }
    }
    protected void StopMovement() => Speed = TargetSpeed = 0;
    #endregion

    #region Movement & Rotation
    /*
     * Unit : 每小時海里數 (knots)
     * 場景單位：1 = 1km
     * 實際移動時需將速度轉換為每秒公里數：km/s = knots * 1.852 / 3600
     */
    [Header("Movement Settings")]
    [SerializeField] protected float m_maxSpeed = 10f;
    [SerializeField] protected float m_acceleration = 2f;
    [SerializeField] protected float m_targetSpeed = 0f;
    [SerializeField] protected float m_speed = 0f;

    [Header("Rotation Settings")]
    [SerializeField] protected float m_maxRotationSpeed = 90f;
    [SerializeField] protected float m_rotationAcceleration = 45f;
    [SerializeField] protected float m_targetRotation = 0f;
    [SerializeField] protected float m_targetRotationSpeed = 0f;
    [SerializeField] protected float m_rotationSpeed = 0f;

    public float MaxSpeed { get => m_maxSpeed; set => m_maxSpeed = Mathf.Max(0, value); }
    public float Acceleration { get => m_acceleration; set => m_acceleration = Mathf.Max(0, value); }
    public float TargetSpeed { get => m_targetSpeed; set => m_targetSpeed = Mathf.Clamp(value, 0, m_maxSpeed); }
    public float Speed { get => m_speed; set => m_speed = Mathf.Clamp(value, 0, m_maxSpeed); }

    public float MaxRotationSpeed { get => m_maxRotationSpeed; set => m_maxRotationSpeed = Mathf.Max(0, value); }
    public float RotationAcceleration { get => m_rotationAcceleration; set => m_rotationAcceleration = Mathf.Max(0, value); }
    public float TargetRotation { get => m_targetRotation; set => m_targetRotation = value % 360f; }
    public float TargetRotationSpeed { get => m_targetRotationSpeed; set => m_targetRotationSpeed = Mathf.Clamp(value, -m_maxRotationSpeed, m_maxRotationSpeed); }
    public float RotationSpeed { get => m_rotationSpeed; set => m_rotationSpeed = Mathf.Clamp(value, -m_maxRotationSpeed, m_maxRotationSpeed); }
    #endregion

    #region Components
    [SerializeField] public Tilemap tilemap;
    [SerializeField] public TileBase oceanTile;
    #endregion

    [SerializeField] private float navigationBoundaryBuffer = 2f; // 可配置邊界緩衝值

    /**
     * @type {NetworkVariable<Vector3>}
     * @description 同步艦船位置
     */
    public NetworkVariable<Vector3> NetworkPosition = new NetworkVariable<Vector3>();
    /**
     * @type {NetworkVariable<float>}
     * @description 同步艦船血量
     */
    public NetworkVariable<float> NetworkHealth = new NetworkVariable<float>();
    /**
     * @type {NetworkVariable<float>}
     * @description 同步艦船速度
     */
    public NetworkVariable<float> NetworkSpeed = new NetworkVariable<float>();
    /**
     * @type {NetworkVariable<float>}
     * @description 同步艦船旋轉角度
     */
    public NetworkVariable<float> NetworkRotation = new NetworkVariable<float>();

    public  void Start()
    {
        tilemap = FindFirstObjectByType<Tilemap>();
        oceanTile = Resources.Load<TileBase>("Tiles/OceanTile");
        if (tilemap == null || oceanTile == null)
            Debug.LogError("Tilemap or Ocean Tile not found!", this);
    }

    public virtual void Update()
    {
        // 網路同步：只有 Owner 可以寫入，其他 Client 只讀取
        if (IsOwner)
        {
            NetworkPosition.Value = transform.position;
            NetworkHealth.Value = Health;
            NetworkSpeed.Value = Speed;
            NetworkRotation.Value = transform.eulerAngles.z;
        }
        else
        {
            transform.position = NetworkPosition.Value;
            Health = NetworkHealth.Value;
            Speed = NetworkSpeed.Value;
            var rot = transform.eulerAngles;
            rot.z = NetworkRotation.Value;
            transform.eulerAngles = rot;
        }
    }

    protected virtual void Rotate()
    {
        if (Mathf.Abs(m_targetRotationSpeed) > 0.01f)
        {
            m_rotationSpeed = Mathf.MoveTowards(m_rotationSpeed, m_targetRotationSpeed, m_rotationAcceleration * Time.deltaTime);
            transform.Rotate(0, 0, m_rotationSpeed * Time.deltaTime);
        }
        else
        {
            float delta = Mathf.DeltaAngle(transform.eulerAngles.z, m_targetRotation);
            if (Mathf.Abs(delta) > 0.1f)
            {
                float step = Mathf.Clamp(delta, -m_maxRotationSpeed * Time.deltaTime, m_maxRotationSpeed * Time.deltaTime);
                transform.Rotate(0, 0, step);
            }
        }
    }

    protected virtual void Move()
    {

        float updateInterval = (Speed < 0.1f) ? 1.0f : 0.2f;
        m_navigationUpdateTimer += Time.deltaTime;

        if (!IsFollower && Time.frameCount % 10 == GetInstanceID() % 10)
        {
            if (m_navigationUpdateTimer >= updateInterval)
            {
                m_navigationUpdateTimer = 0f;
                if (m_waypoints.Count > 0)
                {
                    NavigateToWaypoint();
                }

                if (NavigationArea != Rect.zero)
                {
                    HandleNavigationArea();
                }
            }
        }

        if (CurrentFuel <= 0) return;

        m_speed = Mathf.MoveTowards(m_speed, m_targetSpeed, m_acceleration * Time.deltaTime);

        // 將速度從每小時海里數(knots)轉換為每秒公里數(km/s)
        float kmPerSecond = m_speed * 1.852f / 3600f;
        Vector3 newPosition = transform.position + transform.right * kmPerSecond * Time.deltaTime * GameManager.RealGameTimeScale;

        transform.position = newPosition;
        GameDataController.Instance?.UpdateShipPosition(ShipId, newPosition);
        CurrentFuel -= FuelConsumptionRate * m_speed * Time.deltaTime;
        GameDataController.Instance?.UpdateShipFuel(ShipId, CurrentFuel);
    }


    protected virtual void NavigateToWaypoint()
    {
        m_cachedTarget = m_waypoints[0];
        m_cachedDirection = m_cachedTarget;
        m_cachedDirection -= transform.position;
        m_cachedDirection.Normalize();
        SetNavigation(m_cachedDirection, 2f);

        if ((transform.position - m_cachedTarget).sqrMagnitude < 0.01f)
        {
            m_waypoints.RemoveAt(0);
        }
    }

    protected virtual void HandleNavigationArea()
    {
        if (CurrentFuel <= 0)
        {
            Debug.LogWarning("[Ship] Out of fuel, cannot navigate.");
            TargetSpeed = 0f;
            return;
        }

        if (IsOutOfNavigationBounds())
        {
            m_cachedDirection = CalculateDirection(GetNavigationAreaCenter());
            SetNavigation(m_cachedDirection, 2f);
        }
        else if (tilemap != null && oceanTile != null && !IsNextPositionOceanTile())
        {
            m_cachedDirection = CalculateDirection(transform.position - GetNextPosition());
            SetNavigation(m_cachedDirection, 2f);
        }
        else
        {
            TargetSpeed = 2f;
        }

        CurrentFuel -= FuelConsumptionRate * TargetSpeed * Time.deltaTime;
    }

    protected virtual bool IsOutOfNavigationBounds()
    {
        return transform.position.x < NavigationArea.xMin + navigationBoundaryBuffer ||
               transform.position.x > NavigationArea.xMax - navigationBoundaryBuffer ||
               transform.position.y < NavigationArea.yMin + navigationBoundaryBuffer ||
               transform.position.y > NavigationArea.yMax - navigationBoundaryBuffer;
    }

    private Vector3 CalculateDirection(Vector3 target)
    {
        return (target - transform.position).normalized;
    }

    protected virtual Vector3 GetNavigationAreaCenter()
    {
        return new Vector3((NavigationArea.xMin + NavigationArea.xMax) / 2, (NavigationArea.yMin + NavigationArea.yMax) / 2, 0);
    }

    protected virtual Vector3 GetNextPosition()
    {
        return transform.position + transform.right * Speed * Time.deltaTime;
    }

    protected virtual bool IsNextPositionOceanTile()
    {
        TileBase tile = tilemap.GetTile(tilemap.WorldToCell(GetNextPosition()));
        return tile == oceanTile;
    }

    protected virtual void SetNavigation(Vector3 direction, float speed)
    {
        TargetRotation = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        TargetSpeed = speed;
        Debug.Log($"[Ship] Adjusted TargetSpeed: {TargetSpeed}, TargetRotation: {TargetRotation}");
    }

    public virtual GameData.ShipData SaveShipData()
    {
        return new GameData.ShipData
        {


            Name = this.name,
            Position = transform.position,
            Health = (int)Health,
            FuelConsumptionRate = (int)FuelConsumptionRate,
            Speed = Speed,
            Rotation = transform.rotation.eulerAngles.z,
            Level = 1,
            Experience = 0,
            PrefabName = gameObject.name.Replace("(Clone)", "").Trim(),
            MaxFuel = MaxFuel,
            CurrentFuel = m_fuel
        };
    }
    public virtual void LoadShipData(GameData.ShipData shipData)
    {
        this.name = shipData.Name;
        this.transform.position = shipData.Position;
        this.transform.rotation = Quaternion.Euler(0, 0, shipData.Rotation);
        this.MaxFuel = shipData.MaxFuel;
        this.CurrentFuel = shipData.CurrentFuel;
        this.Health = shipData.Health;
        this.FuelConsumptionRate = shipData.FuelConsumptionRate;

    }
    #region Debug
    private void OnValidate()
    {
        Health = m_health; TargetSpeed = m_targetSpeed;
        TargetRotationSpeed = m_targetRotationSpeed;
        CurrentFuel = m_fuel;
    }
    #endregion

    public bool IsInFleet()
    {
        return IsFollower || LeaderShip != null;
    }
}