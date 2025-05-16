using UnityEngine;
using System.Collections.Generic;

public class EnemyShip : Warship
{
    #region Enemy-Specific Properties
    [SerializeField] private float attackRange = 5f;  // 預設攻擊範圍
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private int damage = 10;
    [SerializeField] private Transform playerTarget;  // 玩家參考目標
    private float m_lastAttackTime;
    public float AttackRange => attackRange;  // 只讀屬性
    public float AttackCooldown => attackCooldown;
    public int Damage => damage;
    public Transform PlayerTarget
    {
        get
        {
            if (playerTarget == null)
            {
                PlayerShip[] ships = GameObject.FindObjectsByType<PlayerShip>(FindObjectsSortMode.None);

                //get the nearest player ship
                float minDistance = Mathf.Infinity;
                foreach (var ship in ships)
                {
                    float distance = Vector3.Distance(transform.position, ship.transform.position);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        playerTarget = ship.transform;
                    }
                }
            }
            return playerTarget;
        }
        set => playerTarget = value;
    }
    #endregion

    // 隨機移動參數
    private float randomMoveDistance = 0f;
    private float movedDistance = 0f;
    private float randomSpeed = 0f;
    private float randomAngle = 0f;
    private Vector2 lastPosition;

    new void Start()
    {
        base.Start();
        PickNewRandomMove();
        lastPosition = transform.position;
    }

    // 覆寫父類 Update 加入敵艦邏輯
    new void Update()
    {
        base.Update();  // 呼叫父類的 Update 方法
        HandleAIBehavior();
        TryFormShipLineWithNearbyEnemies();
    }

    private void HandleAIBehavior()
    {
        // 計算與玩家的距離
        float distanceToPlayer = PlayerTarget != null
            ? Vector3.Distance(transform.position, PlayerTarget.position)
            : Mathf.Infinity;

        // 行為決策樹
        if (distanceToPlayer <= AttackRange)
        {
            EngageCombat();
        }
        else if (distanceToPlayer <= DetectionDistance)
        {
            ChasePlayer();
        }
        else
        {
            RandomMove();
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
        if (PlayerTarget != null)
        {
            Debug.Log($"[EnemyShip] {name} 開始攻擊玩家目標: {PlayerTarget.name}");
            AttackTarget(PlayerTarget.gameObject);
        }
    }

    private void RandomMove()
    {
        // 計算移動距離
        float delta = Vector2.Distance(transform.position, lastPosition);
        movedDistance += delta;
        lastPosition = transform.position;

        // 設定隨機方向與速度，讓 Ship 的 Move/Rotate 控制移動
        TargetRotation = randomAngle;
        TargetSpeed = randomSpeed;
        Debug.Log($"[EnemyShip] Moving randomly: Angle {randomAngle}, Speed {randomSpeed}");


        // 不再直接設定 transform.rotation

        // 若已移動超過隨機距離，則重新選擇方向與速度
        if (movedDistance >= randomMoveDistance)
        {
            PickNewRandomMove();
        }
    }

    private void PickNewRandomMove()
    {
        randomMoveDistance = Random.Range(3f, 10f); // 每次隨機移動距離
        randomSpeed = MaxSpeed * Random.Range(0.1f, 0.2f); // 隨機速度
        randomAngle = Random.Range(0f, 360f); // 隨機方向
        movedDistance = 0f;
        lastPosition = transform.position;
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
                Debug.Log($"[EnemyShip] {name} 受到傷害，當前生命值: {base.Health}");
            }
        }
    }

    protected override void OnDeath()
    {
        Debug.Log("[EnemyShip] Enemy ship destroyed!");
        // 實作死亡效果（爆炸動畫、掉落物品等）
        Destroy(gameObject, 0.5f);  // 延遲0.5秒銷毀
    }

    public List<Weapon> GetWeapons()
    {
        return weapons; // 假設 `weapons` 是 Warship 類別中的武器列表
    }

    // 新增：靠近時組成 ShipLine
    private void TryFormShipLineWithNearbyEnemies()
    {
        // 若自己或父物件已有 ShipLine 則略過
        if (GetComponent<ShipLine>() != null || transform.parent != null && transform.parent.GetComponent<ShipLine>() != null)
            return;

        // 找到所有敵艦
        EnemyShip[] allEnemies = GameObject.FindObjectsByType<EnemyShip>(FindObjectsSortMode.None);
        foreach (var other in allEnemies)
        {
            if (other == this) continue;
            if (other.GetComponent<ShipLine>() != null) continue; // 已有 ShipLine 的略過

            float dist = Vector3.Distance(transform.position, other.transform.position);
            if (dist < 3f)
            {
                // 決定誰當 leader（用 name 排序）
                EnemyShip leader = string.CompareOrdinal(this.name, other.name) < 0 ? this : other;
                EnemyShip follower = leader == this ? other : this;

                // leader 建立 ShipLine
                ShipLine line = leader.gameObject.AddComponent<ShipLine>();
                line.followers.Add(leader.GetComponent<Ship>());
                line.followers.Add(follower.GetComponent<Ship>());
                // follower 設定 parent 方便管理
                follower.transform.SetParent(leader.transform);

                Debug.Log($"[EnemyShip] {leader.name} 與 {follower.name} 組成 ShipLine");
                break;
            }
        }
    }
}