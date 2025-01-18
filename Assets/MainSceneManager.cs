using UnityEngine;
using UnityEngine.Rendering;

public class MainSceneManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public GameObject ShipPrefab;
    void CreateShip(Vector2 SpawnPosition, float maxHealth, float maxSpeed, float detectionDistance, float fuelConsumption, bool mode)
    {
        GameObject ship = Instantiate(ShipPrefab, SpawnPosition, Quaternion.identity);
        Ship shipScript = ship.GetComponent<Ship>();
        shipScript.MaxHealth = maxHealth;
        shipScript.MaxSpeed = maxSpeed;
        shipScript.DetectionDistance = detectionDistance;
        shipScript.FuelConsumption = fuelConsumption;
        shipScript.Mode = mode;
    }
}
