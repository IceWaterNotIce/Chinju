using UnityEngine;

public class Ammo : MonoBehaviour
{
    public float Damage = 10f;
    public float Speed = 1f;
    private GameObject owner; // 記錄發射彈藥的船隻
    private Vector3 direction;

    public void SetDirection(Vector3 dir)
    {
        Debug.Log($"[Ammo] 設置彈藥方向: {dir}");
        direction = dir.normalized;
    }

    public void SetOwner(GameObject ownerShip)
    {
        Debug.Log($"[Ammo] 設置彈藥擁有者: {ownerShip.name}");
        owner = ownerShip; // 設置發射彈藥的船隻
    }

    void Start()
    {
        Debug.Log($"[Ammo] 彈藥生成，方向: {direction}, 速度: {Speed}");
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

    void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log($"[Ammo] 彈藥與 {collision.gameObject.name} 發生碰撞。");
        // 嘗試獲取 PlayerShip 或 EnemyShip 類型的物件
        var playerShip = collision.gameObject.GetComponent<PlayerShip>();
        var enemyShip = collision.gameObject.GetComponent<EnemyShip>();

        if (playerShip != null || enemyShip != null)
        {
            // 處理傷害
            if (playerShip != null)
            {
                if ( playerShip.gameObject == owner)
                {
                    Debug.Log($"[Ammo] 彈藥命中玩家船隻 {playerShip.name}，但沒有造成傷害。");
                    return; // 如果是自己的船隻，則不造成傷害
                }
                Debug.Log($"[Ammo] 彈藥命中玩家船隻 {playerShip.name}，但沒有造成傷害。");
            }
            else if (enemyShip != null)
            {
                enemyShip.Health -= Damage;
                owner.GetComponent<Warship>().AddExperience(Damage); // 增加經驗值
                Debug.Log($"[Ammo] 彈藥命中敵方船隻 {enemyShip.name}，造成 {Damage} 傷害。");
            }
            Destroy(gameObject);
        }
        else
        {
            Debug.Log($"[Ammo] 彈藥與 {collision.gameObject.name} 發生觸發碰撞，但沒有造成傷害。");
        }
        
    }

    void OnDestroy()
    {
        Debug.Log($"[Ammo] 彈藥 {gameObject.name} 被銷毀。");
        if (AmmoManager.Instance != null)
        {
            AmmoManager.Instance.ReturnAmmo(gameObject);
        }
        else
        {
            Destroy(gameObject); // 如果 AmmoManager 不存在，直接銷毀
        }
    }
}
