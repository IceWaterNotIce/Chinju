using UnityEngine;
using System.Collections;

public class Weapon : MonoBehaviour
{
    public string Name { get; set; }
    public int Cost { get; set; } // 新增：武器的花費
    public float Damage { get; set; }
    public float Range { get; set; }
    public float AttackSpeed { get; set; }
    public float CooldownTime { get; set; }

    public float MaxAttackDistance = 10f;
    public float MinAttackDistance = 1f;
    public GameObject AmmoPrefab;
    public float AttackCooldown = 1f;

    private bool isAttacking = false;
    private GameObject currentTarget;
    private Coroutine attackCoroutine;

    public void Attack(GameObject target)
    {
        if (AmmoPrefab == null || target == null) return;
        Vector3 dir = (target.transform.position - transform.position).normalized;
        GameObject ammoObj = Instantiate(AmmoPrefab, transform.position, Quaternion.identity);
        Ammo ammo = ammoObj.GetComponent<Ammo>();
        if (ammo != null)
        {
            ammo.SetDirection(dir);
        }
    }

    public void StartAttack(GameObject target)
    {
        if (isAttacking) return;
        isAttacking = true;
        currentTarget = target;
        attackCoroutine = StartCoroutine(AttackRoutine());
    }

    public void StopAttack()
    {
        isAttacking = false;
        if (attackCoroutine != null)
            StopCoroutine(attackCoroutine);
    }

    private IEnumerator AttackRoutine()
    {
        while (isAttacking && currentTarget != null)
        {
            float dist = Vector3.Distance(transform.position, currentTarget.transform.position);
            if (dist <= MaxAttackDistance && dist >= MinAttackDistance)
            {
                Attack(currentTarget);
            }
            yield return new WaitForSeconds(AttackCooldown);
        }
    }

    public GameData.WeaponData ToWeaponData()
    {
        return new GameData.WeaponData
        {
            Name = this.Name,
            Damage = (int)this.Damage,
            Range = this.Range,
            AttackSpeed = this.AttackSpeed,
            CooldownTime = this.CooldownTime
        };
    }
}
