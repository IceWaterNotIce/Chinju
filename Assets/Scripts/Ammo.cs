using UnityEngine;

public class Ammo : MonoBehaviour
{
    public float Damage = 10f;
    public float Speed = 10f;
    private Vector3 direction;
    private GameObject owner; // 新增：記錄發射彈藥的船隻

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
            if (ship.gameObject == owner)
            {
                Debug.Log("[Ammo] 彈藥撞擊到發射它的船隻，忽略傷害。");
                return;
            }

            ship.Health -= Damage;
        }
        Destroy(gameObject);
    }
}
