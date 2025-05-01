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
        if (AmmoPrefab == null)
        {
            Debug.LogError("[Weapon] AmmoPrefab 未設置，無法生成彈藥！");
            return;
        }

        if (target == null) return;

        Vector3 direction = (target.transform.position - transform.position).normalized;

        // 計算彈藥的生成位置，設置為船隻與目標之間的位置
        Vector3 spawnPosition = transform.position + direction * 1.5f; // 1.5f 為偏移距離，可根據需求調整

        GameObject ammoObj = Instantiate(AmmoPrefab, spawnPosition, Quaternion.identity);

        if (ammoObj != null)
        {
            ammoObj.transform.SetParent(null); // 確保彈藥生成在場景的根層級
            Ammo ammo = ammoObj.GetComponent<Ammo>();
            if (ammo != null)
            {
                ammo.SetDirection(direction);

                // 設置彈藥的擁有者為武器所屬的船隻
                var ownerShip = GetComponentInParent<Ship>();
                if (ownerShip != null)
                {
                    ammo.SetOwner(ownerShip.gameObject);
                    Debug.Log($"[Weapon] 彈藥生成成功，設置擁有者為 {ownerShip.name}");
                }
                else
                {
                    Debug.LogWarning("[Weapon] 無法找到武器的擁有者船隻，請檢查武器的層級結構！");
                }
            }
            else
            {
                Debug.LogError("[Weapon] 無法找到 Ammo 組件，請檢查 AmmoPrefab 是否正確設置！");
            }
        }
        else
        {
            Debug.LogError("[Weapon] 無法生成 AmmoPrefab，請檢查資源是否正確設置！");
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
