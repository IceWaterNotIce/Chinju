using UnityEngine;

public class LabShipSceneManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CreateShip(
            SpawnPosition: new Vector2(0, 0),
            maxHealth: 100,
            acceleration: 1,
            maxSpeed: 5,
            MaxRotationSpeed: 100,
            RotationAcceleration: 1,
            detectionDistance: 5,
            fuelConsumption: 1,
            mode: true
        );
    }

    // Update is called once per frame
    void Update()
    {

    }

    public GameObject ShipPrefab;
    void CreateShip(Vector2 SpawnPosition, float maxHealth, float acceleration, float maxSpeed, float MaxRotationSpeed, float RotationAcceleration, float detectionDistance, float fuelConsumption, bool mode)
    {
        GameObject ship = Instantiate(ShipPrefab, SpawnPosition, Quaternion.identity);
        Ship shipScript = ship.GetComponent<Ship>();
        shipScript.MaxHealth = maxHealth;
        shipScript.MaxSpeed = maxSpeed;
        shipScript.MaxRotationSpeed = MaxRotationSpeed;
        shipScript.Acceleration = acceleration;
        shipScript.RotationAcceleration = RotationAcceleration;
        shipScript.DetectionDistance = detectionDistance;
        shipScript.FuelConsumption = fuelConsumption;
        shipScript.CombatMode = mode;
    }
}
