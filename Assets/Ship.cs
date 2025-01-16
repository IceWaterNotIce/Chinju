using UnityEngine;
using UnityEngine.EventSystems;
public class Ship : MonoBehaviour, IPointerClickHandler {
    public void OnPointerClick (PointerEventData eventData)
    {
        Debug.Log ("clicked");
    }
    private float m_health;
    public float Health
    {
        get { return m_health; }
        set { m_health = value; }
    }
    private float m_maxspeed;
    public float MaxSpeed
    {
        get { return m_maxspeed; }
        set { m_maxspeed = value; }
    }
    private float m_speed;
    public float Speed
    {
        get { return m_speed; }
        set { m_speed = value; }
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

    }

   
}
