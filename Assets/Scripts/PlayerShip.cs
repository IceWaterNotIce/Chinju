using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;
using UnityEngine.Events;
using UnityEngine.Tilemaps; // ← 新增
using Unity.Netcode;

public class PlayerShip : Warship, IPointerClickHandler
{
    #region UnityEvents for UI
    public UnityEvent<bool> OnCombatModeChanged = new UnityEvent<bool>();
    #endregion

    #region UI References
    private ShipDetailPanel UI => ShipDetailPanel.Instance;
    #endregion

    #region Player Settings
    [Header("Player Settings")]
    private float m_healthRegenTimer = 0f; // 用於計時的變數
    /**
     * @type {NetworkVariable<float>}
     * @description 同步回血計時器
     */
    public NetworkVariable<float> NetworkHealthRegenTimer = new NetworkVariable<float>(0f);
    #endregion

    #region Movement Logic
    protected override void Move()
    {
        base.Move(); // 僅呼叫基底
    }
    #endregion

    #region Health Logic
    public override void Update()
    {
        // 網路同步：只有 Owner 可以寫入，其他 Client 只讀取
        if (IsOwner)
        {
            NetworkHealthRegenTimer.Value = m_healthRegenTimer;
        }
        else
        {
            m_healthRegenTimer = NetworkHealthRegenTimer.Value;
        }
        // 每分鐘增加 1 點健康值
        m_healthRegenTimer += Time.deltaTime;
        if (m_healthRegenTimer >= 60f)
        {
            Health += 1;
            m_healthRegenTimer = 0f;
            Debug.Log($"[PlayerShip] Health increased by 1. Current Health: {Health}");
        }
        base.Update(); // 使用基類的更新邏輯
    }
    #endregion

    #region UI Interaction
    public void OnPointerClick(PointerEventData eventData)
    {
        if (UI == null)
        {
            Debug.LogError("ShipDetailPanel.Instance is null!");
            return;
        }

        UI.Initial(this);

        // 攝影機跟隨
        var cameraController = Camera.main?.GetComponent<CameraBound2D>();
        if (cameraController != null)
        {
            cameraController.FollowTarget(transform);
        }
    }
    #endregion

    #region Save/Load Logic
    public override GameData.ShipData SaveShipData()
    {
        var data = base.SaveShipData();
        data.NavigationArea = NavigationArea;
        return data;
    }

    public override void LoadShipData(GameData.ShipData data)
    {
        base.LoadShipData(data);
        NavigationArea = data.NavigationArea;
    }
    #endregion

    public void OnDisable()
    {
        // if this ship have ship line component
        if (GetComponent<Fleet>() != null)
        {
            // remove this ship component 
            Destroy(GetComponent<Fleet>());
            Debug.Log($"[PlayerShip] Fleet component removed from {gameObject.name}");
        }

        // if this ship is follower, call the ship line component to remove this ship
        if (IsFollower)
        {
           LeaderShip?.GetComponent<Fleet>()?.RemoveFollower(this);
            Debug.Log($"[PlayerShip] Follower {gameObject.name} removed from Leader {LeaderShip.name}");
        }
    }
}