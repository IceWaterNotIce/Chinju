using System.Collections.Generic;
using UnityEngine;

public class EnemyShipPool : MonoBehaviour
{
    public static EnemyShipPool Instance { get; private set; }

    private List<GameObject> enemyShipPrefabs = new List<GameObject>(); // 自動載入
    [SerializeField] private int initialPoolSize = 10;

    private Queue<GameObject> pool = new Queue<GameObject>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            LoadEnemyShipPrefabs();
            InitializePool();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void LoadEnemyShipPrefabs()
    {
        // 從 Resources/Prefabs/Ship/Enemys 載入所有 prefab
        var loadedPrefabs = Resources.LoadAll<GameObject>("Prefabs/Ships/Enemies");
        enemyShipPrefabs.Clear();
        enemyShipPrefabs.AddRange(loadedPrefabs);
        if (enemyShipPrefabs.Count == 0)
        {
            Debug.LogError("[EnemyShipPool] 無法從 Resources/Prefabs/Ships/Enemies 載入任何敵艦預製物！");
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
