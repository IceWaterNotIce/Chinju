using UnityEngine;
using TMPro;

public class StatusBar : MonoBehaviour
{
    public TMP_Text goldText;
    public TMP_Text oilsText;
    public TMP_Text cubeText;

    private GameData.PlayerData playerData;

    void Start()
    {
        // Initialize player data (replace with actual data source)
        playerData = new GameData.PlayerData
        {
            Gold = 1000,
            Oils = 500,
            Cube = 10
        };

        UpdateUI();
    }

    public void UpdateUI()
    {
        goldText.text = $"Gold: {playerData.Gold}";
        oilsText.text = $"Oils: {playerData.Oils}";
        cubeText.text = $"Cube: {playerData.Cube}";
    }
}
