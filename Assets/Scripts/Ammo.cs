using UnityEngine;

public class Ammo : MonoBehaviour
{
    public float Damage = 10f;
    public float Speed = 10f;
    private Vector3 direction;
    private GameObject owner; // 記錄發射彈藥的船隻

    public void SetDirection(Vector3 dir)
    {
        direction = dir.normalized;
    }

    public void SetOwner(GameObject ownerShip)
    {
        owner = ownerShip; // 設置發射彈藥的船隻
    }

    void Update()
    {
        if (direction == Vector3.zero)
        {
            Debug.LogWarning("[Ammo] 未設置方向，彈藥無法移動！");
            return;
        }

        transform.position += direction * Speed * Time.deltaTime;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // 嘗試獲取 Ship 或 EnemyShip 類型的物件
        var ship = collision.gameObject.GetComponent<Ship>();
        var enemyShip = collision.gameObject.GetComponent<EnemyShip>();

        if (ship != null || enemyShip != null)
        {
            // 處理傷害
            if (ship != null)
            {
                ship.Health -= Damage;
                Debug.Log($"[Ammo] 彈藥命中玩家船隻 {ship.name}，造成 {Damage} 傷害。");
            }
            else if (enemyShip != null)
            {
                enemyShip.Health -= Damage;
                Debug.Log($"[Ammo] 彈藥命中敵方船隻 {enemyShip.name}，造成 {Damage} 傷害。");
            }
        }

        Destroy(gameObject);
    }
}
