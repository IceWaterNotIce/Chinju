using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// 管理遊戲UI界面
/// </summary>
public class UIManager : MonoBehaviour
{
    /// <summary>
    /// 單例實例
    /// </summary>
    public static UIManager Instance { get; private set; }

    [SerializeField] private PlayerStatsUI playerStatsUI;

    void Awake()
    {
        // 設置單例實例
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    /// <summary>
    /// 更新所有資源顯示
    /// </summary>
    /// <param name="gold">金幣數量</param>
    /// <param name="oil">石油數量</param>
    /// <param name="cube">方塊數量</param>
    public void UpdateResourceDisplay(int gold, int oil, int cube)
    {
        if (playerStatsUI != null)
        {
            playerStatsUI.UpdateResourceDisplay(gold, oil, cube);
        }
    }

    /// <summary>
    /// 更新金幣數量顯示
    /// </summary>
    /// <param name="amount">金幣數量</param>
    public void UpdateGoldAmount(int amount)
    {
        if (playerStatsUI != null)
        {
            playerStatsUI.UpdateGoldAmount(amount);
        }
    }

    /// <summary>
    /// 更新石油數量顯示
    /// </summary>
    /// <param name="amount">石油數量</param>
    public void UpdateOilAmount(int amount)
    {
        if (playerStatsUI != null)
        {
            playerStatsUI.UpdateOilAmount(amount);
        }
    }

    /// <summary>
    /// 更新方塊數量顯示
    /// </summary>
    /// <param name="amount">方塊數量</param>
    public void UpdateCubeAmount(int amount)
    {
        if (playerStatsUI != null)
        {
            playerStatsUI.UpdateCubeAmount(amount);
        }
    }
} 