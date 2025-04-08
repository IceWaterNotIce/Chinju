using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
public class Ship : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("clicked");

        if (ShipUI.Instance == null)
        {
            Debug.LogError("ShipUI.Instance is null. Ensure ShipUI is properly initialized.");
            return;
        }

        ShipUI.Instance.Initial(this);
    }

    private List<Vector3> m_waypoints = new List<Vector3>();
    private float m_maxhealth;
    public float MaxHealth
    {
        get { return m_maxhealth; }
        set { m_maxhealth = value; }
    }
    private float m_health;
    public float Health
    {
        get { return m_health; }
        set { m_health = value; }
    }
    private float m_maxSpeed;
    public float MaxSpeed
    {
        get { return m_maxSpeed; }
        set { m_maxSpeed = value; }
    }
    private float m_targetSpeed;
    public float TargetSpeed
    {
        get { return m_targetSpeed; }
        set { m_targetSpeed = value; }
    }
    public float m_acceleration;
    public float Acceleration
    {
        get { return m_acceleration; }
        set { m_acceleration = value; }
    }
    private float m_speed;
    public float Speed
    {
        get { return m_speed; }
        set { m_speed = value; }
    }
    private float m_maxRotationSpeed;
    public float MaxRotationSpeed
    {
        get { return m_maxRotationSpeed; }
        set { m_maxRotationSpeed = value; }
    }
    private float m_targetRotation;
    public float TargetRotation
    {
        get { return m_targetRotation; }
        set { m_targetRotation = value; }
    }
    private float m_targetRotationSpeed;
    public float TargetRotationSpeed
    {
        get { return m_targetRotationSpeed; }
        set { m_targetRotationSpeed = value; }
    }
    private float m_rotationSpeed;


    public float RotationSpeed
    {
        get { return m_rotationSpeed; }
        set { m_rotationSpeed = value; }
    }

    private float m_rotationAcceleration;
    public float RotationAcceleration
    {
        get { return m_rotationAcceleration; }
        set { m_rotationAcceleration = value; }
    }


    private float m_detectionDistance;
    public float DetectionDistance
    {
        get { return m_detectionDistance; }
        set { m_detectionDistance = value; }
    }
    private float m_fuelConsumption;
    public float FuelConsumption
    {
        get { return m_fuelConsumption; }
        set { m_fuelConsumption = value; }
    }
    private bool m_mode;
    public bool Mode
    {
        get { return m_mode; }
        set { m_mode = value; }
    }



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
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

    public void AddWaypoint(Vector3 waypoint)
    {
        m_waypoints.Add(waypoint);
    }

    public void DeleteWaypoint(Vector3 waypoint)
    {
        m_waypoints.Remove(waypoint);
    }

    public void ClearWaypoints()
    {
        m_waypoints.Clear();
    }


}
