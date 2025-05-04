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

   
}