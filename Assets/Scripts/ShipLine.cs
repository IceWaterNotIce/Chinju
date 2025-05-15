using System.Collections.Generic;
using UnityEngine;

public class ShipLine : MonoBehaviour
{
    public List<Ship> followers = new List<Ship>(); // List of follower ships
    public float followSpeed = 1.0f; // Speed at which followers adjust their target speed
    public float distanceBetweenFollowers = 1.0f; // Distance between followers

    void Update()
    {
        for (int i = 1; i < followers.Count; i++)
        {
            // 根據前一艘船的位置和方向計算目標旋轉與速度
            Vector3 directionToTarget = (followers[i - 1].transform.position - followers[i].transform.position).normalized;
            float targetRotation = Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg;

            followers[i].TargetRotation = targetRotation;
            followers[i].TargetSpeed = followSpeed;
            Debug.Log($"[ShipLine] Adjusted TargetSpeed: {followers[i].TargetSpeed}, TargetRotation: {followers[i].TargetRotation}");
        }
    }
}