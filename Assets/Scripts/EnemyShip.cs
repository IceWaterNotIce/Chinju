using UnityEngine;

public class EnemyShip : Ship 
{
    #region Enemy-Specific Properties
    public float AttackRange { get; set; } = 5f;  // 預設攻擊範圍
    public float AttackCooldown { get; set; } = 2f;
    private float m_lastAttackTime;
    public int Damage { get; set; } = 10;
    public Transform PlayerTarget { get; set; }  // 玩家參考目標
    #endregion

    // 覆寫父類 Update 加入敵艦邏輯
    new void Update()
    {
        base.Update();  // 呼叫父類的 Update 方法
        if (PlayerTarget != null)
        {
            HandleAIBehavior();
        }
    }

    private void HandleAIBehavior()
    {
        // 計算與玩家的距離
        float distanceToPlayer = Vector3.Distance(transform.position, PlayerTarget.position);

        // 行為決策樹
        if (distanceToPlayer <= AttackRange)
        {
            EngageCombat();
        }
        else if (distanceToPlayer <= DetectionDistance)
        {
            ChasePlayer();
        }
    }

    private void ChasePlayer()
    {
        // 計算朝向玩家的方向
        Vector2 direction = (PlayerTarget.position - transform.position).normalized;
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // 設定移動參數
        TargetRotation = targetAngle;
        TargetSpeed = MaxSpeed * 0.8f;  // 追擊時保持80%速度
    }

    private void EngageCombat()
    {
        // 冷卻檢查
        if (Time.time - m_lastAttackTime >= AttackCooldown)
        {
            Attack();
            m_lastAttackTime = Time.time;
        }

        // 保持戰鬥距離
        TargetSpeed = MaxSpeed * 0.3f;
    }

    private void Attack()
    {
        // 實作攻擊邏輯（範例：發射投射物）
        Debug.Log($"Enemy attacked! Damage: {Damage}");

        /* 實際遊戲中可能包含：
        1. 生成砲彈預製件
        2. 播放攻擊動畫
        3. 觸發音效
        */
    }

    // 強化被擊中效果
    public override float Health
    {
        get => base.Health;
        set 
        {
            base.Health = value;
            if (base.Health <= 0) 
            {
                OnDeath();
            }
            else
            {
                Debug.Log("Enemy ship damaged!");
            }
        }
    }

    private void OnDeath()
    {
        Debug.Log("Enemy ship destroyed!");
        // 實作死亡效果（爆炸動畫、掉落物品等）
        Destroy(gameObject, 0.5f);  // 延遲0.5秒銷毀
    }
}