using UnityEngine;
using UnityEngine.Tilemaps;
using System;
using System.Collections.Generic;

public class Ship : MonoBehaviour
{
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

    public event Action<float> OnHealthChanged;
    public event Action<float> OnFuelChanged;

    protected virtual void OnDeath()
    {
        Destroy(gameObject);
        if (this is PlayerShip && ShipUI.Instance != null)
        {
            Destroy(ShipUI.Instance.gameObject); // 銷毀 ShipUI
            Debug.Log("[Ship] ShipUI 已銷毀");
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

    public virtual void Start()
    {
        tilemap = FindFirstObjectByType<Tilemap>();
        oceanTile = Resources.Load<TileBase>("Tiles/OceanTile");
        if (tilemap == null || oceanTile == null)
            Debug.LogError("Tilemap or Ocean Tile not found!", this);
    }

    public virtual void Update()
    {
        Rotate();
        Move();
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
        if (CurrentFuel <= 0) return;

        m_speed = Mathf.MoveTowards(m_speed, m_targetSpeed, m_acceleration * Time.deltaTime);

        // 將速度從每小時海里數(knots)轉換為每秒公里數(km/s)
        float kmPerSecond = m_speed * 1.852f / 3600f;
        Vector3 newPosition = transform.position + transform.right * kmPerSecond * Time.deltaTime * GameManager.RealGameTimeScale;

        Vector3Int tilePosition = tilemap.WorldToCell(newPosition);
        if (tilemap.GetTile(tilePosition) == oceanTile)
        {
            transform.position = newPosition;
            CurrentFuel -= FuelConsumptionRate * m_speed * Time.deltaTime;
        }
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
        this.name = shipData.Name; this.transform.position = shipData.Position;
        this.transform.rotation = Quaternion.Euler(0, 0, shipData.Rotation);
        this.MaxFuel = shipData.MaxFuel; this.CurrentFuel = shipData.CurrentFuel;
    }
    #region Debug
    private void OnValidate()
    {
        Health = m_health; TargetSpeed = m_targetSpeed;
        TargetRotationSpeed = m_targetRotationSpeed;
        CurrentFuel = m_fuel;
    }
    #endregion
}