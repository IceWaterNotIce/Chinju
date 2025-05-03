using UnityEngine;

public class PlayerStatsUI : MonoBehaviour {
    // ...existing code...

    private PlayerStats playerStats;

    private void Start() {
        playerStats = FindObjectOfType<PlayerStats>();
        if (playerStats != null) {
            playerStats.OnStatsChanged += UpdateUI;
            UpdateUI(); // 初始化 UI
        }
    }

    private void OnDestroy() {
        if (playerStats != null) {
            playerStats.OnStatsChanged -= UpdateUI;
        }
    }

    private void UpdateUI() {
        // 更新 UI 元素，例如：
        // scoreText.SetText(playerStats.Score.ToString());
        // levelText.SetText(playerStats.Level.ToString());
    }

    // ...existing code...
}