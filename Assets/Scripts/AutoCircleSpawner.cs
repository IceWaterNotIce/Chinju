using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class CircleData
{
    public Vector2 center = new Vector2(0.5f, 0.5f); // UV 座標 [0,1]
    [Range(0, 0.5f)]
    public float radius = 0.1f;
}

public class AutoCircleSpawner : MonoBehaviour
{
    public Material circleMaterial; // 使用 MultiTransparentCircles Shader 的材質
    public List<CircleData> circles = new List<CircleData>(100);
    public float spawnInterval = 2.0f; // 每 2 秒新增一個
    public float circleRadius = 0.1f; // 圓形半徑
    private float timer = 0f;

    void Update()
    {
        timer += Time.deltaTime;

        // 每 2 秒新增一個圓形
        if (timer >= spawnInterval)
        {
            timer = 0f;
            AddRandomCircle();
        }
    }

    void AddRandomCircle()
    {
        // 隨機生成圓心位置 (UV 座標範圍 [0,1])
        Vector2 randomCenter = new Vector2(
            Random.Range(0.1f, 0.9f),
            Random.Range(0.1f, 0.9f)
        );

        // 添加到列表
        circles.Add(new CircleData
        {
            center = randomCenter,
            radius = circleRadius
        });

        // 更新 Shader 參數
        UpdateShader();
    }

    void UpdateShader()
    {
        circleMaterial.SetInt("_CircleCount", circles.Count);
        circleMaterial.SetVectorArray("_Circles", GetCircleDataArray());
    }

    Vector4[] GetCircleDataArray()
    {
        Vector4[] circleDataArray = new Vector4[circles.Count];
        for (int i = 0; i < circles.Count; i++)
        {
            circleDataArray[i] = new Vector4(circles[i].center.x, circles[i].center.y, circles[i].radius, 0);
        }
        return circleDataArray;
    }

    // 可選：在 Inspector 顯示當前圓形數量
    public int CircleCount => circles.Count;
}