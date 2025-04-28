using UnityEngine;

public class Ammo : MonoBehaviour
{
    public float Damage = 10f;
    public float Speed = 10f;
    private Vector3 direction;

    public void SetDirection(Vector3 dir)
    {
        direction = dir.normalized;
    }

    void Update()
    {
        transform.position += direction * Speed * Time.deltaTime;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        var ship = collision.gameObject.GetComponent<Ship>();
        if (ship != null)
        {
            ship.Health -= Damage;
        }
        Destroy(gameObject);
    }
}
