using System.Collections.Generic;
using UnityEngine;

public class EnemyShipPool : MonoBehaviour
{
    public static EnemyShipPool Instance { get; private set; }

    [SerializeField] private List<GameObject> enemyShipPrefabs; // 支持多個預製物
    [SerializeField] private int initialPoolSize = 10;

    private Queue<GameObject> pool = new Queue<GameObject>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializePool();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializePool()
    {
        foreach (var prefab in enemyShipPrefabs)
        {
            for (int i = 0; i < initialPoolSize; i++)
            {
                var enemyShip = Instantiate(prefab);

                // 設置為 EnemyShipPool 的子物件
                enemyShip.transform.SetParent(this.transform);

                enemyShip.SetActive(false);
                pool.Enqueue(enemyShip);
            }
        }
    }

    public GameObject GetEnemyShip()
    {
        if (pool.Count > 0)
        {
            var enemyShip = pool.Dequeue();
            enemyShip.SetActive(true);

            // 設置為 EnemyShipPool 的子物件
            enemyShip.transform.SetParent(this.transform);

            return enemyShip;
        }
        else
        {
            // 隨機選擇一個預製物
            var prefab = enemyShipPrefabs[Random.Range(0, enemyShipPrefabs.Count)];
            var enemyShip = Instantiate(prefab);

            // 設置為 EnemyShipPool 的子物件
            enemyShip.transform.SetParent(this.transform);

            return enemyShip;
        }
    }

    public void ReturnEnemyShip(GameObject enemyShip)
    {
        enemyShip.SetActive(false);

        // 確保設置為 EnemyShipPool 的子物件
        enemyShip.transform.SetParent(this.transform);

        pool.Enqueue(enemyShip);
    }
}
