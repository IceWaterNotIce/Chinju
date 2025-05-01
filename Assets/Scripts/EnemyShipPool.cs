using System.Collections.Generic;
using UnityEngine;

public class EnemyShipPool : MonoBehaviour
{
    public static EnemyShipPool Instance { get; private set; }

    [SerializeField] private GameObject enemyShipPrefab;
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
        for (int i = 0; i < initialPoolSize; i++)
        {
            var enemyShip = Instantiate(enemyShipPrefab);
            enemyShip.SetActive(false);
            pool.Enqueue(enemyShip);
        }
    }

    public GameObject GetEnemyShip()
    {
        if (pool.Count > 0)
        {
            var enemyShip = pool.Dequeue();
            enemyShip.SetActive(true);
            return enemyShip;
        }
        else
        {
            var enemyShip = Instantiate(enemyShipPrefab);
            return enemyShip;
        }
    }

    public void ReturnEnemyShip(GameObject enemyShip)
    {
        enemyShip.SetActive(false);
        pool.Enqueue(enemyShip);
    }
}
