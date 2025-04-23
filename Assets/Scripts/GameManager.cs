using UnityEngine;
using System.IO;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using UnityEngine.UI; // For UnityEngine.UI.Button
using UnityEngine.UIElements; // For UnityEngine.UIElements.Button
using UnityEngine.InputSystem; // Import the new Input System namespace

public class GameManager : Singleton<GameManager>
{
    public const string serverUrl = "https://icewaternotice.com/games/Word Curse/";
    public const string githubUrl = "https://raw.githubusercontent.com/IceWaterNotIce/Word-Curse/main/";

    private string saveFilePath;
    public GameData currentGameData = new GameData();
    public GameObject gameMenuUI; // Reference to the Game Menu UI
    private VisualElement gameMenu;

    void Start()
    {
        saveFilePath = Path.Combine(Application.persistentDataPath, "savegame.json");

        // Load and bind the GameMenu UI
        var uiDocument = GetComponent<UIDocument>();
        if (uiDocument == null)
        {
            Debug.LogError("UIDocument component is missing on the GameObject.");
            return;
        }

        gameMenu = uiDocument.rootVisualElement;
        if (gameMenu == null)
        {
            Debug.LogError("GameMenu UI is not properly set up in the UIDocument.");
            return;
        }

        gameMenu.Q<UnityEngine.UIElements.Button>("saveGameButton").clicked += OnSaveGameButtonClicked;
        gameMenu.Q<UnityEngine.UIElements.Button>("continueButton").clicked += OnContinueButtonClicked;
        gameMenu.Q<UnityEngine.UIElements.Button>("loadGameButton").clicked += OnLoadGameButtonClicked;
        gameMenu.Q<UnityEngine.UIElements.Button>("exitGameButton").clicked += OnExitGameButtonClicked;

        gameMenu.style.display = DisplayStyle.None; // Hide menu initially
    }

    private void OnEnable()
    {
        // Subscribe to the Escape key action
        InputSystem.onActionChange += OnActionChange;
    }

    private void OnDisable()
    {
        // Unsubscribe from the Escape key action
        InputSystem.onActionChange -= OnActionChange;
    }

    private void OnActionChange(object obj, InputActionChange change)
    {
        if (obj is InputAction action && action.name == "Escape" && change == InputActionChange.ActionPerformed)
        {
            if (gameMenu.style.display == DisplayStyle.None)
            {
                ShowGameMenu();
            }
            else
            {
                HideGameMenu();
            }
        }
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

    public void ShowGameMenu()
    {
        gameMenu.style.display = DisplayStyle.Flex; // Show the game menu
    }

    public void HideGameMenu()
    {
        gameMenu.style.display = DisplayStyle.None; // Hide the game menu
    }

    public void OnSaveGameButtonClicked()
    {
        SaveGame(currentGameData);
        Debug.Log("Game saved via menu.");
    }

    public void OnContinueButtonClicked()
    {
        HideGameMenu();
        Debug.Log("Game continued.");
    }

    public void OnLoadGameButtonClicked()
    {
        GameData loadedData = LoadGame();
        if (loadedData != null)
        {
            currentGameData = loadedData;
            Debug.Log("Game loaded via menu.");
        }
    }

    public void OnExitGameButtonClicked()
    {
        Application.Quit();
        Debug.Log("Game exited via menu.");
    }
}