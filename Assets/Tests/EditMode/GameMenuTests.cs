using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

/// <summary>
/// 遊戲選單按鈕測試類
/// </summary>
public class GameMenuTests
{
    private GameManager gameManager;
    private UIDocument uiDocument;

    [SetUp]
    public void Setup()
    {
        // 創建測試用的 GameObject 並添加必要組件
        GameObject gameObject = new GameObject("GameManager");
        gameManager = gameObject.AddComponent<GameManager>();
        uiDocument = gameObject.AddComponent<UIDocument>();
    }

    [TearDown]
    public void Teardown()
    {
        // 清理測試物件
        Object.DestroyImmediate(gameManager.gameObject);
    }

    /// <summary>
    /// 測試顯示和隱藏選單功能
    /// </summary>
    [Test]
    public void TestMenuVisibility()
    {
        // 顯示選單
        gameManager.ShowGameMenu();
        Assert.IsTrue(gameManager.IsMenuVisible());

        // 隱藏選單
        gameManager.HideGameMenu();
        Assert.IsFalse(gameManager.IsMenuVisible());
    }

    /// <summary>
    /// 測試儲存遊戲按鈕功能
    /// </summary>
    [Test]
    public void TestSaveGameButton()
    {
        gameManager.OnSaveGameButtonClicked();
        // 驗證是否有保存檔案
        string saveFilePath = System.IO.Path.Combine(Application.persistentDataPath, "savegame.json");
        Assert.IsTrue(System.IO.File.Exists(saveFilePath));
    }

    /// <summary>
    /// 測試載入遊戲按鈕功能
    /// </summary>
    [Test]
    public void TestLoadGameButton()
    {
        // 先保存一些測試數據
        GameData testData = new GameData();
        testData.PlayerDatad.Gold = 1000;
        gameManager.SaveGame(testData);

        // 測試載入功能
        gameManager.OnLoadGameButtonClicked();
        Assert.AreEqual(1000, gameManager.currentGameData.PlayerDatad.Gold);
    }

    /// <summary>
    /// 測試繼續遊戲按鈕功能
    /// </summary>
    [Test]
    public void TestContinueButton()
    {
        gameManager.ShowGameMenu();
        gameManager.OnContinueButtonClicked();
        Assert.IsFalse(gameManager.IsMenuVisible());
    }
} 