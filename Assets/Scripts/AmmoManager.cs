using System.Collections.Generic;
using UnityEngine;

public class AmmoManager : MonoBehaviour
{
    public static AmmoManager Instance;

    public GameObject AmmoPrefab;
    public int PoolSize = 20;

    private Queue<GameObject> ammoPool = new Queue<GameObject>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InitializePool();
    }

    private void InitializePool()
    {
        for (int i = 0; i < PoolSize; i++)
        {
            GameObject ammo = Instantiate(AmmoPrefab, transform); // 設置為 AmmoManager 的子物件
            ammo.SetActive(false);
            ammoPool.Enqueue(ammo);
        }
    }

    public GameObject GetAmmo()
    {
        while (ammoPool.Count > 0)
        {
            GameObject ammo = ammoPool.Dequeue();
            if (ammo != null)
            {
                ammo.SetActive(true);
                ammo.transform.SetParent(transform); // 確保彈藥仍然是 AmmoManager 的子物件
                return ammo;
            }
        }

        Debug.LogWarning("[AmmoManager] 彈藥池已空或包含無效物件，動態生成新彈藥！");
        GameObject newAmmo = Instantiate(AmmoPrefab, transform); // 動態生成的彈藥也設置為子物件
        return newAmmo;
    }

    public void ReturnAmmo(GameObject ammo)
    {
        if (ammo == null)
        {
            Debug.LogWarning("[AmmoManager] 試圖回收一個已被銷毀的彈藥物件！");
            return;
        }

        ammo.SetActive(false);
        ammo.transform.SetParent(transform); // 確保回收的彈藥設置為 AmmoManager 的子物件
        ammoPool.Enqueue(ammo);
    }
}
