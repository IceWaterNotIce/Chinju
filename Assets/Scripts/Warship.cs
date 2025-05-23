using UnityEngine;
using System.Linq;
using System;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

// 全域定義 CombatMode 枚舉
public enum CombatMode { Peaceful, Defensive, Aggressive }

public class Warship : Ship
{
    // 新增：標記是否為跟隨者
    public bool IsFollower;
    public PlayerShip LeaderShip; // 新增：領隊船參考

    #region Combat Settings
    [Header("Combat Settings")]
    [SerializeField] private float m_detectionDistance = 50f;
    [SerializeField] private CombatMode m_combatMode = CombatMode.Peaceful;
    [SerializeField] private int m_weaponLimit = 2;
    [SerializeField] public List<Weapon> weapons = new List<Weapon>();

    public float DetectionDistance { get => m_detectionDistance; set => m_detectionDistance = Mathf.Max(0, value); }
    public CombatMode Mode { get => m_combatMode; set => m_combatMode = value; }
    public int WeaponLimit { get => m_weaponLimit; set => m_weaponLimit = Mathf.Max(0, value); }
    #endregion

    #region Experience System
    [Header("Experience Settings")]
    [SerializeField] private int m_level = 1;
    [SerializeField] private float m_experience = 0f;

    public event Action<float> OnExperienceChanged;
    public event Action<int> OnLevelChanged;

    public int Level { get => m_level; protected set => m_level = Mathf.Max(1, value); }
    public float Experience { get => m_experience; private set => m_experience = Mathf.Max(0, value); }

    public void AddExperience(float exp)
    {
        Experience += exp;
        float upgradeNeed = Level * 10f;
        while (Experience >= upgradeNeed)
        {
            Experience -= upgradeNeed;
            Level++;
            upgradeNeed = Level * 10f;
        }
        OnExperienceChanged?.Invoke(Experience);
        OnLevelChanged?.Invoke(Level);
    }
    #endregion

    public override void Update()
    {
        base.Update();
        if (m_combatMode != CombatMode.Peaceful) DetectAndAttackTarget();
    }

    private void DetectAndAttackTarget()
    {
        var target = FindNearestEnemy();
        if (target != null && Vector3.Distance(transform.position, target.transform.position) <= GetMaxWeaponRange())
        {
            AttackTarget(target.gameObject);
        }
        else
        {
            StopAttack();
        }
    }

    private Ship FindNearestEnemy()
    {
        // get this ship tag
        var thisTag = gameObject.tag;
        // if the tag is "Player" then the enemy tag is "Enemy"
        var enemyTag = thisTag == "Player" ? "Enemy" : "Player";
        // find all ships with the enemy tag
        var enemies = FindObjectsByType<Ship>(FindObjectsSortMode.None)
            .Where(s => s.gameObject.tag == enemyTag)
            .ToList();
        // find the nearest enemy ship
        Ship nearestEnemy = null;
        float minDistance = float.MaxValue;
        foreach (var enemy in enemies)
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestEnemy = enemy;
            }
        }
        Debug.Log($"[Warship] Nearest enemy: {nearestEnemy?.name}, Distance: {minDistance}");
        return nearestEnemy;
    }

    private float GetMaxWeaponRange() => weapons.Count > 0 ? weapons.Max(w => w.MaxAttackDistance) : 0f;

    public void AttackTarget(GameObject target)
    {
        weapons.ForEach(weapon =>
        {
            if (weapon != null && target != null) weapon.StartAttack(target);
        });
    }

    public void StopAttack() => weapons.ForEach(weapon => weapon?.StopAttack());

    public void AddWeapon(Weapon weapon)
    {
        if (weapons.Count >= WeaponLimit) return;
        weapons.Add(weapon);
    }

    public void AddRandomWeapon()
    {
        if (weapons.Count >= WeaponLimit) return;

        var availableWeapons = Resources.LoadAll<Weapon>("Prefabs/Weapons");
        if (availableWeapons.Length == 0) return;

        var newWeapon = Instantiate(    
            availableWeapons[UnityEngine.Random.Range(0, availableWeapons.Length)],
            transform
        );
        weapons.Add(newWeapon);
    }

    public override GameData.ShipData SaveShipData()
    {
        var data = base.SaveShipData();
        data.Experience = Experience;
        data.Level = Level;
        // 儲存為枚舉
        data.Mode = (GameData.ShipData.CombatMode)m_combatMode;
        data.WeaponLimit = WeaponLimit;
        return data;
    }

}