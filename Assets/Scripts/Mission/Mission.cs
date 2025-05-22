using UnityEngine;

public abstract class Mission : ScriptableObject
{
    [SerializeField] protected string id;
    [SerializeField] protected string title;
    [TextArea]
    [SerializeField] protected string description;
    [SerializeField] protected bool isCompleted;
    [SerializeField] protected int rewardGold;
    [SerializeField] protected int rewardOil;
    [SerializeField] protected int rewardCube;
    [SerializeField] protected int rewardExp;

    public string Id => id;
    public string Title => title;
    public string Description => description;
    public bool IsCompleted => isCompleted;
    public int RewardGold => rewardGold;
    public int RewardOil => rewardOil;
    public int RewardCube => rewardCube;
    public int RewardExp => rewardExp;

    public virtual void Complete()
    {
        if (!isCompleted)
        {
            isCompleted = true;
            var playerData = GameDataController.Instance.CurrentGameData.playerData;
            playerData.Gold += rewardGold;
            playerData.Oils += rewardOil;
            playerData.Cube += rewardCube;
            playerData.Exp += rewardExp; // 如果有經驗值欄位
            GameDataController.Instance.TriggerResourceChanged();
            Debug.Log($"獲得金幣: {rewardGold}, 油: {rewardOil}, 魔方: {rewardCube}, 經驗: {rewardExp}");
        }
    }

    public virtual void ResetMission()
    {
        isCompleted = false;
    }
}

[CreateAssetMenu(fileName = "DailyMission", menuName = "Game/Mission/DailyMission", order = 1)]
public class DailyMission : Mission
{
    // 可擴充每日任務專屬屬性
}

[CreateAssetMenu(fileName = "LevelMission", menuName = "Game/Mission/LevelMission", order = 2)]
public class LevelMission : Mission
{
    [SerializeField] private int levelRequired;
    public int LevelRequired => levelRequired;
    // 可擴充關卡任務專屬屬性
}