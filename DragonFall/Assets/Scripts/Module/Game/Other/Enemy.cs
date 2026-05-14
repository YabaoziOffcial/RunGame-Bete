using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 敌人移动、受伤、死亡和经验掉落逻辑
public class Enemy : MonoBehaviour
{
    // 自动补碰撞体时使用的默认半径
    private const float ColliderRadius = 0.5f;

    // 敌人基础属性数据
    [SerializeField] private EnemyData m_EnemyData = new EnemyData() { hp = 10f, attack = 10f, speed = 2f};
    public EnemyData EnemyData { get => m_EnemyData; private set => m_EnemyData = value; }
    
    // 对象池复用时用于重置生命值
    private float m_MaxHp;
    // 防止死亡逻辑重复触发
    private bool m_IsDead;
    private Rigidbody2D m_Rigidbody2D;

    // 初始化物理组件和默认碰撞体
    private void Awake()
    {
        m_MaxHp = m_EnemyData.hp;
        m_Rigidbody2D = GetComponent<Rigidbody2D>();
        if (m_Rigidbody2D == null)
        {
            m_Rigidbody2D = gameObject.AddComponent<Rigidbody2D>();
        }
        m_Rigidbody2D.gravityScale = 0f;

        if (GetComponent<Collider2D>() != null) return;

        CircleCollider2D collider2d = gameObject.AddComponent<CircleCollider2D>();
        collider2d.radius = ColliderRadius;
    }

    // 从对象池取出时重置状态
    private void OnEnable()
    {
        if (m_MaxHp > 0f)
        {
            m_EnemyData.hp = m_MaxHp;
        }
        m_IsDead = false;
    }


    public void Update()
    {
        if (m_IsDead) return;
        MoveToPlayer();
    }


    // 设置敌人属性，并同步最大生命值
    public void SetEnemyData(EnemyData enemyData)
    {
        m_EnemyData = enemyData;
        m_MaxHp = m_EnemyData.hp;
    }

    // 承受伤害，生命值归零后结算击杀和掉落
    public void TakeDamage(float damage)
    {
        if (m_IsDead) return;
        if (damage <= 0f) return;

        m_EnemyData.hp -= damage;
        if (m_EnemyData.hp <= 0f)
        {
            m_IsDead = true;
            if (m_Rigidbody2D != null)
            {
                m_Rigidbody2D.velocity = Vector2.zero;
            }
            GameController.Instance.Model.AddKillEnemyCount();
            TryDropEx();
            ObjectPool.PushObj(gameObject);
        }
    }

    // 根据玩家经验掉落率在死亡位置生成 EX
    private void TryDropEx()
    {
        Player player = GameController.Instance.Player;
        if (player == null) return;
        if (Random.value > player.PlayerData.ExDropRate) return;

        GameObject exPrefab = GameConst.GetExPrefab();
        if (exPrefab == null) return;

        GameObject ex = ObjectPool.GetObj(exPrefab);
        ex.transform.position = transform.position;
    }

    // 持续朝玩家方向移动
    private void MoveToPlayer()
    {
        Player player = GameController.Instance.Player;
        if (player == null || m_Rigidbody2D == null) return;

        Vector2 direction = player.transform.position - transform.position;
        if (direction.sqrMagnitude <= 0f)
        {
            m_Rigidbody2D.velocity = Vector2.zero;
            return;
        }

        m_Rigidbody2D.velocity = direction.normalized * m_EnemyData.speed;
    }
}


[System.Serializable]
public class EnemyData
{
    public float hp; // 生命值
    public float speed; // 移动速度
    public float attack; // 攻击力
    public float defense; // 防御力
    public float attackInterval; // 攻击间隔
}
