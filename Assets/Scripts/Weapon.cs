using UnityEngine;
using System.Collections;

public class Weapon : MonoBehaviour
{
    [Tooltip("武器名稱，可於檢查器中編輯")]
    [SerializeField] private string _name;
    public string Name
    {
        get => _name;
        set => _name = value;
    }

   

    [Tooltip("攻擊速度（每分鐘攻擊次數）")]
    [SerializeField] private float _attackSpeed = 1f;
    public float AttackSpeed
    {
        get => _attackSpeed;
        set => _attackSpeed = value;
    }

    [Tooltip("最大攻擊距離（單位：km），超過此距離無法攻擊目標")]
    [SerializeField] private float _maxAttackDistance = 10f;
    public float MaxAttackDistance
    {
        get => _maxAttackDistance;
        set
        {
            if (value < 0)
            {
                Debug.LogWarning("[Weapon] 最大攻擊距離不能為負數，設置為 0。");
                _maxAttackDistance = 0;
            }
            else
            {
                _maxAttackDistance = value;
            }
        }
    }

 [Tooltip("武器的花費")]
    [SerializeField] private int _cost;
    public int Cost
    {
        get => _cost;
        set => _cost = value;
    }

    [Tooltip("武器傷害值")]
    [SerializeField] private float _damage;
    public float Damage
    {
        get => _damage;
        set => _damage = value;
    }
    public GameObject AmmoPrefab;

    [Tooltip("每次攻擊發射的彈藥數量")]
    [SerializeField] private int _ammoPerShot = 1;
    public int AmmoPerShot
    {
        get => _ammoPerShot;
        set => _ammoPerShot = Mathf.Max(1, value);
    }

    private bool isAttacking = false;
    private GameObject currentTarget;
    private Coroutine attackCoroutine;

    public void Attack(GameObject target)
    {
        if (AmmoManager.Instance == null)
        {
            Debug.LogError("[Weapon] AmmoManager 未初始化，無法生成彈藥！");
            return;
        }

        if (target == null) return;

        Vector3 baseDirection = (target.transform.position - transform.position).normalized;

        var ownerShip = GetComponentInParent<Warship>();
        float successRate = 20f + (ownerShip != null ? Mathf.Min(ownerShip.Level / 2f, 79f) : 0f); // 最大 99%

        // 多發彈藥處理
        for (int i = 0; i < AmmoPerShot; i++)
        {
            Vector3 direction = baseDirection;

            // 計算攻擊成功率
            bool isSuccessful = Random.Range(0f, 100f) <= successRate;

            float angleOffset = 0f;
            if (!isSuccessful)
            {
                // 如果攻擊不成功，添加一個小的隨機方向偏移
                angleOffset = Random.Range(-10f, 10f);
            }
            // 多發彈藥時，每發有微小角度偏移
            float spread = Mathf.Lerp(-5f, 5f, (AmmoPerShot == 1) ? 0.5f : (float)i / (AmmoPerShot - 1));
            float totalAngle = angleOffset + spread;
            Quaternion rotation = Quaternion.Euler(0, 0, totalAngle);
            direction = rotation * baseDirection;

            Vector3 spawnPosition = transform.position;

            GameObject ammoObj = AmmoManager.Instance.GetAmmo();
            if (ammoObj == null)
            {
                Debug.LogError("[Weapon] 無法從 AmmoManager 獲取有效的彈藥物件！");
                continue;
            }

            ammoObj.transform.position = spawnPosition;
            ammoObj.transform.rotation = Quaternion.identity;
            ammoObj.transform.SetParent(AmmoManager.Instance.transform);

            Ammo ammo = ammoObj.GetComponent<Ammo>();
            if (ammo != null)
            {
                ammo.SetDirection(direction);

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
    }

    public void StartAttack(GameObject target)
    {
        if (isAttacking)
        {
            // 如果正在攻擊，更新目標為新目標
            Debug.Log($"[Weapon] {name} 更新攻擊目標為: {target.name}");
            currentTarget = target;
            return;
        }

        isAttacking = true;
        currentTarget = target;
        Debug.Log($"[Weapon] {name} 開始攻擊目標: {target.name}");
        attackCoroutine = StartCoroutine(AttackRoutine());
    }

    public void StopAttack()
    {
        isAttacking = false;
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            Debug.Log($"[Weapon] {name} 停止攻擊。");
        }
    }

    public bool IsAttackingTarget(GameObject target)
    {
        return isAttacking && currentTarget == target;
    }

    private IEnumerator AttackRoutine()
    {
        while (isAttacking && currentTarget != null)
        {
            float distance = Vector3.Distance(transform.position, currentTarget.transform.position);
            if (distance <= MaxAttackDistance)
            {
                Attack(currentTarget);
            }
            yield return new WaitForSeconds(1f / AttackSpeed); // 根據攻擊速度計算攻擊間隔
        }
    }

    public GameData.WeaponData SaveWeaponData()
    {
        return new GameData.WeaponData
        {
            Name = this.Name,
            Damage = (int)this.Damage, // 顯式轉換 Damage 為 int
            MaxAttackDistance = this.MaxAttackDistance,
   
            AttackSpeed = this.AttackSpeed,
            PrefabName = this.gameObject.name.Replace("(Clone)", "").Trim(), // 保存武器的預製物名稱
            // 若 GameData.WeaponData 有 AmmoPerShot 屬性，這裡也要保存
            AmmoPerShot = this.AmmoPerShot
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
        this._maxAttackDistance = weaponData.MaxAttackDistance;
        // 若 GameData.WeaponData 有 AmmoPerShot 屬性，這裡也要加載
        this.AmmoPerShot = weaponData.AmmoPerShot;
    }
}
