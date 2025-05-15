using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;


public class PlayerShip : Warship, IPointerClickHandler
{
    #region UI References
    private ShipUI UI => ShipUI.Instance;
    #endregion

    #region Player Settings
    [Header("Player Settings")]
    [SerializeField] private Rect m_navigationArea;
    private float m_healthRegenTimer = 0f; // 用於計時的變數
    #endregion

    #region Properties
    public Rect NavigationArea
    {
        get => m_navigationArea;
        set
        {
            Debug.Log($"[PlayerShip] NavigationArea set to {value}");
            m_navigationArea = value;

            // 確保 NavigationArea 有效
            if (m_navigationArea != Rect.zero)
            {
                TargetSpeed = 2f; // 自動導航速度
            }
            else
            {
                Debug.LogWarning("[PlayerShip] NavigationArea is set to Rect.zero, navigation disabled.");
            }
        }
    }
    #endregion

    #region Waypoints
    private List<Vector3> m_waypoints = new List<Vector3>();
    public IReadOnlyList<Vector3> Waypoints => m_waypoints.AsReadOnly();
    public void AddWaypoint(Vector3 point) => m_waypoints.Add(point);
    public void ClearWaypoints() => m_waypoints.Clear();
    #endregion

    public PlayerShip LeaderShip;
    public bool IsFollower;

    #region Movement Logic
    protected override void Move()
    {
        if (!IsFollower)
        {
            if (m_waypoints.Count > 0)
            {
                NavigateToWaypoint();
            }

            if (NavigationArea != Rect.zero)
            {
                HandleNavigationArea();
            }
        }
        base.Move(); // 使用基類的移動邏輯
    }

    private void NavigateToWaypoint()
    {
        Vector3 target = m_waypoints[0];
        Vector3 direction = (target - transform.position).normalized;
        SetNavigation(direction, 2f);

        if ((transform.position - target).sqrMagnitude < 0.01f) // 使用平方距離比較
        {
            m_waypoints.RemoveAt(0); // 移除已到達的路徑點
        }
    }

    private void HandleNavigationArea()
    {
        if (CurrentFuel <= 0)
        {
            Debug.LogWarning("[PlayerShip] Out of fuel, cannot navigate.");
            TargetSpeed = 0f; // 停止移動
            return;
        }

        if (IsOutOfNavigationBounds())
        {
            Vector3 center = GetNavigationAreaCenter();
            Vector3 direction = (center - transform.position).normalized;
            SetNavigation(direction, 2f);
        }
        else if (tilemap != null && oceanTile != null && !IsNextPositionOceanTile())
        {
            Vector3 direction = (transform.position - GetNextPosition()).normalized;
            SetNavigation(direction, 2f);
        }
        else
        {
            TargetSpeed = 2f; // 設置導航速度
        }

        CurrentFuel -= FuelConsumptionRate * TargetSpeed * Time.deltaTime;
        //Debug.Log($"[PlayerShip] Adjusted TargetSpeed: {TargetSpeed}, TargetRotation: {TargetRotation}, Remaining Fuel: {CurrentFuel}");
    }

    private bool IsOutOfNavigationBounds()
    {
        return transform.position.x < NavigationArea.xMin + 2 || transform.position.x > NavigationArea.xMax - 2 ||
               transform.position.y < NavigationArea.yMin + 2 || transform.position.y > NavigationArea.yMax - 2;
    }

    private Vector3 GetNavigationAreaCenter()
    {
        return new Vector3((NavigationArea.xMin + NavigationArea.xMax) / 2, (NavigationArea.yMin + NavigationArea.yMax) / 2, 0);
    }

    private Vector3 GetNextPosition()
    {
        return transform.position + transform.right * Speed * Time.deltaTime;
    }

    private bool IsNextPositionOceanTile()
    {
        TileBase tile = tilemap.GetTile(tilemap.WorldToCell(GetNextPosition()));
        return tile == oceanTile;
    }

    private void SetNavigation(Vector3 direction, float speed)
    {
        TargetRotation = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        TargetSpeed = speed;
        Debug.Log($"[PlayerShip] Adjusted TargetSpeed: {TargetSpeed}, TargetRotation: {TargetRotation}");
    }
    #endregion

    #region Health Logic
    public override void Update()
    {
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
            Debug.LogError("ShipUI.Instance is null!");
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
        if (GetComponent<ShipLine>() != null)
        {
            // remove this ship component 
            Destroy(GetComponent<ShipLine>());
            Debug.Log($"[PlayerShip] ShipLine component removed from {gameObject.name}");
        }

        // if this ship is follower, call the ship line component to remove this ship
        if (IsFollower)
        {
           LeaderShip?.GetComponent<ShipLine>()?.RemoveFollower(this);
            Debug.Log($"[PlayerShip] Follower {gameObject.name} removed from Leader {LeaderShip.name}");
        }
    }
}