using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using Unity.Netcode;

public class Fleet : NetworkBehaviour
{
    public enum FormationType
    {
        SingleLineAhead,
        DoubleLineAhead,
        LineAbreast,
        CircularFormation,
        EchelonFormation
    }

    /**
     * @type {NetworkList<ulong>}
     * @description 同步艦隊成員 NetworkObjectId
     */
    public NetworkList<ulong> NetworkFollowers;
    /**
     * @type {NetworkVariable<int>}
     * @description 同步隊形類型
     */
    public NetworkVariable<int> NetworkFormation = new NetworkVariable<int>(0);
    /**
     * @type {NetworkVariable<float>}
     * @description 同步圓形隊形半徑
     */
    public NetworkVariable<float> NetworkCircleRadius = new NetworkVariable<float>(3.0f);
    /**
     * @type {NetworkVariable<float>}
     * @description 同步斜隊角度
     */
    public NetworkVariable<float> NetworkEchelonAngle = new NetworkVariable<float>(30f);

    public List<Ship> followers = new List<Ship>(); // List of follower ships
    public float distanceBetweenFollowers = 1.0f; // Distance between followers
    public FormationType formation = FormationType.SingleLineAhead;
    public float circleRadius = 3.0f; // For CircularFormation
    public float echelonAngle = 30f; // For EchelonFormation (degrees)

    public string FleetId { get; set; } // 新增：艦隊唯一識別碼
    public float Speed { get; set; } // 新增：艦隊速度
    public string FlagshipId { get; set; } // 新增：旗艦ID

    void Awake()
    {
        NetworkFollowers = new NetworkList<ulong>();
    }

    void Update()
    {
        // 網路同步：只有 Server 可以寫入，其他 Client 只讀取
        if (IsServer)
        {
            NetworkFormation.Value = (int)formation;
            NetworkCircleRadius.Value = circleRadius;
            NetworkEchelonAngle.Value = echelonAngle;
            // 同步 followers
            NetworkFollowers.Clear();
            foreach (var ship in followers)
            {
                if (ship != null && ship.TryGetComponent<NetworkObject>(out var netObj))
                    NetworkFollowers.Add(netObj.NetworkObjectId);
            }
        }
        else
        {
            formation = (FormationType)NetworkFormation.Value;
            circleRadius = NetworkCircleRadius.Value;
            echelonAngle = NetworkEchelonAngle.Value;
            // followers 由伺服器同步，這裡僅可讀 NetworkFollowers
            // followers 清單同步需額外處理（如根據 NetworkObjectId 取得 Ship 實例）
        }
        switch (formation)
        {
            case FormationType.SingleLineAhead:
                UpdateSingleLineAhead();
                break;
            case FormationType.DoubleLineAhead:
                UpdateDoubleLineAhead();
                break;
            case FormationType.LineAbreast:
                UpdateLineAbreast();
                break;
            case FormationType.CircularFormation:
                UpdateCircularFormation();
                break;
            case FormationType.EchelonFormation:
                UpdateEchelonFormation();
                break;
        }
    }

    void UpdateSingleLineAhead()
    {
        for (int i = 1; i < followers.Count; i++)
        {
            Vector3 directionToTarget = (followers[i - 1].transform.position - followers[i].transform.position).normalized;
            float targetRotation = Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg;
            followers[i].TargetRotation = targetRotation;

            if (Vector3.Distance(followers[i].transform.position, followers[i - 1].transform.position) > distanceBetweenFollowers)
                followers[i].TargetSpeed = followers[i - 1].TargetSpeed;
            else
                followers[i].TargetSpeed = followers[i - 1].TargetSpeed * 0.8f;

            Debug.Log($"[Fleet] Adjusted TargetSpeed: {followers[i].TargetSpeed}, TargetRotation: {followers[i].TargetRotation}");
        }
    }

    void UpdateDoubleLineAhead()
    {
        // 兩列縱隊，偶數在左，奇數在右
        if (followers.Count < 2) return;
        Vector3 leaderPos = followers[0].transform.position;
        Vector3 forward = followers[0].transform.right;
        Vector3 side = followers[0].transform.up;

        for (int i = 1; i < followers.Count; i++)
        {
            int col = (i - 1) % 2; // 0: left, 1: right
            int row = (i - 1) / 2 + 1;
            Vector3 offset = forward * -distanceBetweenFollowers * row;
            offset += side * distanceBetweenFollowers * (col == 0 ? -0.5f : 0.5f);
            Vector3 targetPos = leaderPos + offset;
            Vector3 directionToTarget = (targetPos - followers[i].transform.position).normalized;
            float targetRotation = Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg;
            followers[i].TargetRotation = targetRotation;
            followers[i].TargetSpeed = followers[0].TargetSpeed;
        }
    }

    void UpdateLineAbreast()
    {
        // 橫隊，所有船並排
        if (followers.Count < 2) return;
        Vector3 leaderPos = followers[0].transform.position;
        Vector3 side = followers[0].transform.up;

        int mid = followers.Count / 2;
        for (int i = 1; i < followers.Count; i++)
        {
            int offsetIndex = i - mid;
            if (followers.Count % 2 == 0 && i >= mid) offsetIndex++;
            Vector3 offset = side * distanceBetweenFollowers * offsetIndex;
            Vector3 targetPos = leaderPos + offset;
            Vector3 directionToTarget = (targetPos - followers[i].transform.position).normalized;
            float targetRotation = Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg;
            followers[i].TargetRotation = targetRotation;
            followers[i].TargetSpeed = followers[0].TargetSpeed;
        }
    }

    void UpdateCircularFormation()
    {
        // 圓形隊形，leader在圓心
        if (followers.Count < 2) return;
        Vector3 center = followers[0].transform.position;
        float angleStep = 360f / (followers.Count - 1);

        for (int i = 1; i < followers.Count; i++)
        {
            float angle = angleStep * (i - 1) * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * circleRadius;
            Vector3 targetPos = center + offset;
            Vector3 directionToTarget = (targetPos - followers[i].transform.position).normalized;
            float targetRotation = Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg;
            followers[i].TargetRotation = targetRotation;
            followers[i].TargetSpeed = followers[0].TargetSpeed;
        }
    }

    void UpdateEchelonFormation()
    {
        // 斜隊，依序向一側排列
        if (followers.Count < 2) return;
        Vector3 leaderPos = followers[0].transform.position;
        Vector3 forward = followers[0].transform.right;
        Vector3 side = followers[0].transform.up;

        float rad = echelonAngle * Mathf.Deg2Rad;
        Vector3 echelonDir = (forward * Mathf.Cos(rad) + side * Mathf.Sin(rad)).normalized;

        for (int i = 1; i < followers.Count; i++)
        {
            Vector3 offset = echelonDir * distanceBetweenFollowers * i;
            Vector3 targetPos = leaderPos + offset;
            Vector3 directionToTarget = (targetPos - followers[i].transform.position).normalized;
            float targetRotation = Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg;
            followers[i].TargetRotation = targetRotation;
            followers[i].TargetSpeed = followers[0].TargetSpeed;
        }
    }

    public void RemoveFollower(PlayerShip ship)
    {
        if (followers.Contains(ship))
        {
            followers.Remove(ship);
            Debug.Log($"[Fleet] Removed follower: {ship.name}");
        }
        else
        {
            Debug.LogWarning($"[Fleet] Attempted to remove a ship that is not a follower: {ship.name}");
        }
    }

    public void AddShip(Ship ship)
    {
        if (!followers.Contains(ship))
        {
            followers.Add(ship);
            ship.transform.SetParent(transform);
            Debug.Log($"[Fleet] Added ship: {ship.name} to fleet: {FleetId}");
        }
    }

    public GameData.FleetData SaveFleetData()
    {
        // 只保存有實際船隻的艦隊
        if (followers.Count == 0 || followers.All(s => string.IsNullOrEmpty(s.ShipId)))
        {
            Debug.LogWarning($"[Fleet] 不保存空艦隊: {gameObject.name}");
            return null;
        }

        // 確保有有效的 FleetId
        if (string.IsNullOrEmpty(FleetId))
            FleetId = Guid.NewGuid().ToString(); // 使用 Guid 生成唯一識別碼

        var validShipIds = followers
            .Where(ship => !string.IsNullOrEmpty(ship.ShipId))
            .Select(ship => ship.ShipId)
            .ToList();

        return new GameData.FleetData
        {
            FleetId = this.FleetId,
            Name = gameObject.name,
            Position = transform.position,
            Speed = this.Speed,
            FlagshipId = validShipIds.Count > 0 ? validShipIds[0] : "", // 第一個船隻作為旗艦
            ShipIds = validShipIds
        };
    }

    public bool IsPlayerFleet
    {
        get
        {
            // 判斷是否為玩家艦隊，根據艦隊中的船隻類型
            return followers.Any(ship => ship is PlayerShip);
        }
    }
}