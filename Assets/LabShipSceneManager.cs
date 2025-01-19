using UnityEngine;

public class LabShipSceneManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CreateShip(new Vector2(0, 0), 100, 10, 10, 1, true);
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
