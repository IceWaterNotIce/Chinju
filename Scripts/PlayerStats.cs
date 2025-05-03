using System;

public class PlayerStats {
    public event Action OnStatsChanged;

    private int score;
    public int Score {
        get => score;
        set {
            if (score != value) {
                score = value;
                OnStatsChanged?.Invoke();
            }
        }
    }

    private int level;
    public int Level {
        get => level;
        set {
            if (level != value) {
                level = value;
                OnStatsChanged?.Invoke();
            }
        }
    }
}