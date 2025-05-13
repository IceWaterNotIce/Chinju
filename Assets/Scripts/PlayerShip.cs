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

    [Header("Player Settings")]
    [SerializeField] private Rect m_navigationArea;

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

    #region Waypoints
    private List<Vector3> m_waypoints = new List<Vector3>();
    public IReadOnlyList<Vector3> Waypoints => m_waypoints.AsReadOnly();
    public void AddWaypoint(Vector3 point) => m_waypoints.Add(point);
    public void ClearWaypoints() => m_waypoints.Clear();
    #endregion

    protected override void Move()
    {

        if (m_waypoints.Count > 0)
        {
            Vector3 target = m_waypoints[0];
            Vector3 direction = (target - transform.position).normalized;
            TargetRotation = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg; // 計算新的目標旋轉
            TargetSpeed = 2f; // 設置導航速度
            Debug.Log($"[PlayerShip] Adjusted TargetSpeed: {TargetSpeed}, TargetRotation: {TargetRotation}");
            if (Vector3.Distance(transform.position, target) < 0.1f)
            {
                m_waypoints.RemoveAt(0); // 移除已到達的路徑點
            }
        }
    
        if (NavigationArea != Rect.zero)
        {
            Debug.Log("[PlayerShip] Handling area navigation...");

            if (CurrentFuel <= 0)
            {
                Debug.LogWarning("[PlayerShip] Out of fuel, cannot navigate.");
                TargetSpeed = 0f; // 停止移動
                return;
            }

            // Navigation rect
            if (transform.position.x < NavigationArea.xMin || transform.position.x > NavigationArea.xMax ||
                transform.position.y < NavigationArea.yMin || transform.position.y > NavigationArea.yMax)
            {
                //改變方向 to rect center
                Vector3 center = new Vector3((NavigationArea.xMin + NavigationArea.xMax) / 2, (NavigationArea.yMin + NavigationArea.yMax) / 2, 0);
                Vector3 direction = (center - transform.position).normalized;
                TargetRotation = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg; // 計算新的目標旋轉
                TargetSpeed = 2f; // 設置導航速度
                Debug.Log($"[PlayerShip] Adjusted TargetSpeed: {TargetSpeed}, TargetRotation: {TargetRotation}");
            }

            // Ocean tile 
            // if keep current direction move 1 distance is not ocean tile, target to opposite direction
            else if (tilemap != null && oceanTile != null)
            {
                // the position after move 1 distance
                Vector3 nextPosition = transform.position + transform.right * Speed * Time.deltaTime;
                TileBase tile = tilemap.GetTile(tilemap.WorldToCell(nextPosition));
                if (tile != oceanTile)
                {
                    //改變方向 to opposite direction
                    Vector3 direction = (transform.position - nextPosition).normalized;
                    TargetRotation = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg; // 計算新的目標旋轉
                    TargetSpeed = 2f; // 設置導航速度
                    Debug.Log($"[PlayerShip] Adjusted TargetSpeed: {TargetSpeed}, TargetRotation: {TargetRotation}");
                    
                }
            }

            // 計算導航速度
            else
            {
                TargetSpeed = 2f; // 設置導航速度
            }

            CurrentFuel -= FuelConsumptionRate * TargetSpeed * Time.deltaTime;
            Debug.Log($"[PlayerShip] Adjusted TargetSpeed: {TargetSpeed}, TargetRotation: {TargetRotation}, Remaining Fuel: {CurrentFuel}");
        }

        base.Move(); // 使用基類的移動邏輯
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