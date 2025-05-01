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
        var ship = collision.gameObject.GetComponent<Ship>();
        if (ship != null)
        {
            // 確保彈藥不會對發射它的船隻造成傷害
            if (owner != null && ship.gameObject == owner)
            {
                Debug.Log("[Ammo] 彈藥穿過發射它的船隻，忽略碰撞。");
                return; 
            }

            ship.Health -= Damage;
            Debug.Log($"[Ammo] 彈藥命中 {ship.name}，造成 {Damage} 傷害。");
        }
        Destroy(gameObject);
    }
}
