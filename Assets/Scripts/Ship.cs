using UnityEngine;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;

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
        }
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
            Debug.LogError("ShipUI instance not found!", this);
            return;
        }

        UI.Initial(this);
    }
    #endregion


    void Start()
    {
    }

    // Update is called once per frame
    public void Update()
    {
        Rotate();
        Move();
    }

    void Rotate()
    {
        m_rotationSpeed = Mathf.MoveTowards(m_rotationSpeed, m_targetRotationSpeed, m_rotationAcceleration * Time.deltaTime);
        transform.rotation = Quaternion.Euler(0, 0, transform.rotation.eulerAngles.z + m_rotationSpeed * Time.deltaTime);
    }

    void Move()
    {
        m_speed = Mathf.MoveTowards(m_speed, m_targetSpeed, m_acceleration * Time.deltaTime);
        transform.position += transform.right * Speed * Time.deltaTime;
    }

    public GameData.ShipData SaveShipData()
    {
        GameData.ShipData shipData = new GameData.ShipData
        {
            Position = transform.position,
            Health = (int)Health,
            Fuel = (int)FuelConsumption, // Explicitly cast float to int
            Speed = Speed,
            Rotation = transform.rotation.eulerAngles.z
        };

        if (shipData.Fuel > 0)
        {
            shipData.Speed += Acceleration;
            shipData.Rotation = TargetRotation;
        }

        return shipData;
    }

    public void LoadShipData(GameData.ShipData data)
    {
        transform.position = data.Position;
        Health = data.Health;
        FuelConsumption = data.Fuel;
        Speed = data.Speed;
        TargetRotation = data.Rotation;
    }

    #region Debug
    private void OnValidate()
    {
        // Auto-clamp values in Inspector
        Health = m_health;
        TargetSpeed = m_targetSpeed;
        TargetRotationSpeed = m_targetRotationSpeed;
    }
    #endregion
}