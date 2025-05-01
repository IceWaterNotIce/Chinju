using UnityEngine;
using System.Collections;

public class Weapon : MonoBehaviour
{
    public string Name; // 改為字段，允許在檢查器中編輯
    public int Cost; // 新增：武器的花費
    public float Damage;
    public float AttackSpeed = 1f;
    public float CooldownTime = 1f; // 統一命名

    public float MaxAttackDistance = 10f;
    public float MinAttackDistance = 1f;
    public GameObject AmmoPrefab;

    private bool isAttacking = false;
    private GameObject currentTarget;
    private Coroutine attackCoroutine;

    public void Attack(GameObject target)
    {
        if (AmmoPrefab == null || target == null) return;

        Vector3 direction = (target.transform.position - transform.position).normalized;
        GameObject ammoObj = Instantiate(AmmoPrefab, transform.position, Quaternion.identity);
        Ammo ammo = ammoObj.GetComponent<Ammo>();
        if (ammo != null)
        {
            ammo.SetDirection(direction);
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
            float distance = Vector3.Distance(transform.position, currentTarget.transform.position);
            if (distance <= MaxAttackDistance && distance >= MinAttackDistance)
            {
                Attack(currentTarget);
            }
            yield return new WaitForSeconds(CooldownTime); // 使用統一的 CooldownTime
        }
    }

    public GameData.WeaponData SaveWeaponData()
    {
        return new GameData.WeaponData
        {
            Name = this.Name,
            Damage = (int)this.Damage, // 顯式轉換 Damage 為 int
            MaxAttackDistance = this.MaxAttackDistance,
            MinAttackDistance = this.MinAttackDistance,
            AttackSpeed = this.AttackSpeed,
            CooldownTime = this.CooldownTime,
            PrefabName = this.gameObject.name.Replace("(Clone)", "").Trim() // 保存武器的預製物名稱
        };
    }

    public GameData.WeaponData ToWeaponData()
    {
        return SaveWeaponData(); // 使用 SaveWeaponData 方法
    }

    public void LoadWeaponData(GameData.WeaponData weaponData)
    {
        // 根據 weaponData 初始化武器屬性
        this.name = weaponData.Name;
        this.Damage = weaponData.Damage;
        this.AttackSpeed = weaponData.AttackSpeed;
        this.CooldownTime = weaponData.CooldownTime;
        this.MaxAttackDistance = weaponData.MaxAttackDistance;
        this.MinAttackDistance = weaponData.MinAttackDistance;
    }
}
