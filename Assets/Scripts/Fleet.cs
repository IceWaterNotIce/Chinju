using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Fleet : MonoBehaviour
{
    public enum FormationType
    {
        SingleLineAhead,
        DoubleLineAhead,
        LineAbreast,
        CircularFormation,
        EchelonFormation
    }

    public List<Ship> followers = new List<Ship>(); // List of follower ships
    public float distanceBetweenFollowers = 1.0f; // Distance between followers
    public FormationType formation = FormationType.SingleLineAhead;
    public float circleRadius = 3.0f; // For CircularFormation
    public float echelonAngle = 30f; // For EchelonFormation (degrees)

    void Update()
    {
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
}