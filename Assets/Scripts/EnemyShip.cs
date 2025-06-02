using UnityEngine;
using System.Collections.Generic;
using System.Linq; // 新增 using 以支援 LINQ
using Unity.Netcode;

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

    private MapController mapController; // 新增

    /**
     * @type {NetworkVariable<float>}
     * @description 同步攻擊冷卻計時器
     */
    public NetworkVariable<float> NetworkLastAttackTime = new NetworkVariable<float>(0f);
    /**
     * @type {NetworkVariable<Vector3>}
     * @description 同步目標玩家位置
     */
    public NetworkVariable<Vector3> NetworkPlayerTargetPosition = new NetworkVariable<Vector3>();

    new void Start()
    {
        base.Start();
        this.ShipId = System.Guid.NewGuid().ToString(); // 新增：初始化唯一 ShipId
        GameManager.Instance?.RegisterEnemyShip(this); // 確保註冊
        // 取得 MapController 實例
        mapController = FindFirstObjectByType<MapController>();
        PickNewRandomMove();
        lastPosition = transform.position;
    }

    // 覆寫父類 Update 加入敵艦邏輯
    new void Update()
    {
        // 網路同步：只有 Server 可以寫入，其他 Client 只讀取
        if (IsServer)
        {
            NetworkLastAttackTime.Value = m_lastAttackTime;
            if (PlayerTarget != null)
                NetworkPlayerTargetPosition.Value = PlayerTarget.position;
        }
        else
        {
            m_lastAttackTime = NetworkLastAttackTime.Value;
            if (PlayerTarget != null)
                PlayerTarget.position = NetworkPlayerTargetPosition.Value;
        }
        base.Update();  // 呼叫父類的 Update 方法
        HandleAIBehavior();
        TryFormFleetWithNearbyEnemies();
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
        // 權重選擇最佳方向
        int tryCount = 8;
        float bestWeight = float.MinValue;
        float bestAngle = 0f;
        float bestSpeed = 0f;

        Vector3 curPos = transform.position;

        for (int i = 0; i < tryCount; i++)
        {
            float angle = Random.Range(0f, 360f);
            float speed = MaxSpeed * Random.Range(0.1f, 0.2f);
            float distance = Random.Range(3f, 10f);

            // 預測移動後的位置
            Vector3 dir = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad), 0);
            Vector3 targetPos = curPos + dir * distance;

            int oceanLevel = 0;
            if (mapController != null && mapController.oceanTilemap != null)
            {
                Vector3Int cell = mapController.oceanTilemap.WorldToCell(targetPos);
                mapController.oceanTileLevels.TryGetValue(cell, out oceanLevel);
            }

            // 權重：oceanLevel 越大越好，並加一點隨機性
            float weight = oceanLevel + Random.Range(0f, 0.5f);

            if (weight > bestWeight)
            {
                bestWeight = weight;
                bestAngle = angle;
                bestSpeed = speed;
            }
        }

        randomMoveDistance = Random.Range(3f, 10f);
        randomSpeed = bestSpeed;
        randomAngle = bestAngle;
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

    public void SetLevel(int newLevel)
    {
        //set the level to warship
        Level = newLevel;
        // 根據等級調整屬性（可自訂）
        damage = 10 + (Level - 1) * 5;
        base.Health = 100 + (Level - 1) * 20;
        Debug.Log($"[EnemyShip] {name} 設定為等級 {Level}，攻擊力 {damage}，生命值 {base.Health}");
    }

    // 新增：靠近時自動組成 Fleet（自動組隊優化）
    private void TryFormFleetWithNearbyEnemies()
    {
        // 若父物件已有 Fleet 則略過
        if (transform.parent != null && transform.parent.GetComponent<Fleet>() != null)
            return;

        // 找到所有敵艦
        EnemyShip[] allEnemies = GameObject.FindObjectsByType<EnemyShip>(FindObjectsSortMode.None);
        foreach (var other in allEnemies)
        {
            if (other == this) continue;

            float dist = Vector3.Distance(transform.position, other.transform.position);
            if (dist < 3f)
            {
                // 只要雙方都沒有 Fleet 組件才組隊
                if (other.transform.parent != null && other.transform.parent.GetComponent<Fleet>() != null)
                {
                    Fleet existingFleet = other.transform.parent.GetComponent<Fleet>();

                    FleetManager.Instance.AddShipToFleet(this, existingFleet);
                    break;
                }
                else
                {
                    // 決定誰當 leader（用 name 排序，或用 GetInstanceID 保證唯一）
                    EnemyShip leader, follower;
                    if (string.CompareOrdinal(this.name, other.name) < 0)
                    {
                        leader = this;
                        follower = other;
                    }
                    else if (string.CompareOrdinal(this.name, other.name) > 0)
                    {
                        leader = other;
                        follower = this;
                    }
                    else
                    {
                        // 若名稱一樣，用 InstanceID
                        leader = this.GetInstanceID() < other.GetInstanceID() ? this : other;
                        follower = leader == this ? other : this;
                    }

                    // 使用 FleetManager 建立 Fleet
                    FleetManager.Instance.CreateFleet(new Warship[] { leader, follower });

                    Debug.Log($"[EnemyShip] {leader.name} 與 {follower.name} 自動組成 Fleet");
                    break;
                }
            }
        }
    }

    public void ResetState()
    {
        Health = MaxHealth; // 重置生命值
        TargetSpeed = 0f;   // 重置速度
        TargetRotation = 0f; // 重置旋轉
        weapons.Clear();    // 清空武器列表
        Debug.Log($"[EnemyShip] {name} 已重置狀態");
    }

    public override GameData.ShipData SaveShipData() // 修正：加入 override 關鍵字
    {
        return new GameData.ShipData
        {
            ShipId = this.ShipId,
            Name = this.name,
            Health = this.Health,
            Level = this.Level,
            Speed = this.MaxSpeed,
            MaxFuel = this.MaxFuel,
            CurrentFuel = this.CurrentFuel,
            Position = this.transform.position,
            Rotation = this.transform.eulerAngles.z,
            PrefabName = this.name.Replace("(Clone)", ""), // 移除Clone後綴
            Weapons = GetWeapons().Select(w => new GameData.WeaponData
            {
                WeaponId = w.WeaponId,
                Name = w.name,
                Damage = (int)w.Damage, // 修正：明確轉型為 int
                MaxAttackDistance = w.MaxAttackDistance,
                AttackSpeed = w.AttackSpeed,
                PrefabName = w.name.Replace("(Clone)", "")
            }).ToList()
        };
    }
}