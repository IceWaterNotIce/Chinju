using System;

public class Ship {
    // ...existing code...

    public event Action OnShipDataChanged;

    private int health;
    public int Health {
        get => health;
        set {
            if (health != value) {
                health = value;
                OnShipDataChanged?.Invoke();
            }
        }
    }

    private int fuel;
    public int Fuel {
        get => fuel;
        set {
            if (fuel != value) {
                fuel = value;
                OnShipDataChanged?.Invoke();
            }
        }
    }

    // ...existing code...
}