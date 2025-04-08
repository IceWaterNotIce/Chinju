using System.Collections.Generic;
using UnityEngine;

public class ShipLine : MonoBehaviour
{
    public List<Transform> followers = new List<Transform>(); // List of follower objects
    public float followSpeed = 1.0f; // Speed at which followers follow the leader
    public float distanceBetweenFollowers = 1.0f; // Distance between followers
    void Update()
    {
        for (int i = 1 ; i < followers.Count; i++)
        {
            // accroding to the first follower z rotation to calculate the target position
            Vector2 targetPosition = followers[i - 1].position - followers[i - 1].right * distanceBetweenFollowers;
            followers[i].position = Vector2.Lerp(followers[i].position, targetPosition, followSpeed * Time.deltaTime);
        }
    }
}