using UnityEngine;
using System.IO;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class GameManager : Singleton<GameManager>
{
    public const string serverUrl = "https://icewaternotice.com/games/Word Curse/";
    public const string githubUrl = "https://raw.githubusercontent.com/IceWaterNotIce/Word-Curse/main/";

    private string saveFilePath;
    public GameData currentGameData = new GameData();

    void Start()
    {
        saveFilePath = Path.Combine(Application.persistentDataPath, "savegame.json");
    }

    void Update()
    {
        // ...existing code...
    }

    public void SaveGame(GameData data)
    {
        string json = JsonUtility.ToJson(data);
        File.WriteAllText(saveFilePath, json);
        Debug.Log("Game saved to " + saveFilePath);
    }

    public GameData LoadGame()
    {
        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            GameData data = JsonUtility.FromJson<GameData>(json);
            Debug.Log("Game loaded from " + saveFilePath);
            return data;
        }
        else
        {
            Debug.LogWarning("Save file not found at " + saveFilePath);
            return null;
        }
    }

    public void SaveMapData(Tilemap tilemap, GameData.MapData mapData)
    {
        mapData.ChinjuTiles.Clear();
        foreach (var position in tilemap.cellBounds.allPositionsWithin)
        {
            if (tilemap.GetTile(position) != null)
            {
                mapData.ChinjuTiles.Add(position);
            }
        }
        Debug.Log("Map data saved.");
    }

    public void LoadMapData(Tilemap tilemap, GameData.MapData mapData, TileBase chinjuTile)
    {
        tilemap.ClearAllTiles();
        foreach (var position in mapData.ChinjuTiles)
        {
            tilemap.SetTile(position, chinjuTile);
        }
        Debug.Log("Map data loaded.");
    }

    public void StartNewGame()
    {
        // Reset game data
        currentGameData = new GameData
        {
            PlayerDatad = new GameData.PlayerData // Fully qualify PlayerData
            {
                Oils = 100,
                Gold = 500,
                Cube = 0,
                Ships = new List<GameData.ShipData>()
            },
            MapDatad = new GameData.MapData // Fully qualify MapData
            {
                Seed = Random.Range(0, int.MaxValue),
                Width = 100,
                Height = 100,
                IslandDensity = 0.1f,
                ChinjuTiles = new List<Vector3Int>()
            }
        };

        // Save the new game state
        SaveGame(currentGameData);

        // Notify other systems (e.g., UI, map generator) to update
        Debug.Log("New game started.");
    }

    private void OnApplicationQuit()
    {
        SaveGame(currentGameData);
        Debug.Log("Game data saved on exit.");
    }
}