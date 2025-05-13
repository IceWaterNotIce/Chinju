using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;


public class PlayerShip : Warship, IPointerClickHandler
{
    #region UI References
    private ShipUI UI => ShipUI.Instance;
    #endregion

    [Header("Player Settings")]
    [SerializeField] private Rect m_navigationArea;
    
    public Rect NavigationArea 
    {
        get => m_navigationArea;
        set 
        {
            m_navigationArea = value;
            if (m_navigationArea != Rect.zero)
            {
                TargetSpeed = 2f; // 自動導航速度
            }
        }
    }
    
#region Waypoints
    private List<Vector3> m_waypoints = new List<Vector3>();
    public IReadOnlyList<Vector3> Waypoints => m_waypoints.AsReadOnly();
    public void AddWaypoint(Vector3 point) => m_waypoints.Add(point);
    public void ClearWaypoints() => m_waypoints.Clear();
    #endregion

    protected override void Move()
    {
        if (NavigationArea != Rect.zero)
        {
            HandleAreaNavigation();
        }
        else
        {
            base.Move();
        }
    }

    private void HandleAreaNavigation()
    {
        if (CurrentFuel <= 0) return;

        Vector3 newPosition = transform.position + transform.right * Speed * Time.deltaTime;
        
        // 檢查邊界碰撞
        if (!NavigationArea.Contains(newPosition) || 
            tilemap.GetTile(tilemap.WorldToCell(newPosition)) != oceanTile)
        {
            // 隨機改變方向
            TargetRotation = Random.Range(0, 360f);
            return;
        }

        transform.position = newPosition;
        CurrentFuel -= FuelConsumptionRate * Speed * Time.deltaTime;
    }

    // UI 互動邏輯
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
}