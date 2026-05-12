using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    private const float ColliderRadius = 0.5f;

    [SerializeField] private EnemyData m_EnemyData = new EnemyData() { hp = 10f, attack = 10f, speed = 2f};
    public EnemyData EnemyData { get => m_EnemyData; private set => m_EnemyData = value; }
    
    private float m_MaxHp;
    private bool m_IsDead;
    private Rigidbody2D m_Rigidbody2D;

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


    public void SetEnemyData(EnemyData enemyData)
    {
        m_EnemyData = enemyData;
        m_MaxHp = m_EnemyData.hp;
    }

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
            ObjectPool.PushObj(gameObject);
        }
    }

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
