using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 普通的子弹，打到人之后就会回收
public class WeaponCommonBullet : MonoBehaviour
{
    // 自动补碰撞体时使用的默认半径
    private const float ColliderRadius = 0.1f;
    // 飞行方向
    private Vector2 m_Direction;
    // 飞行速度
    private float m_Speed;
    // 剩余存活时间
    private float m_LifeTimer;
    // 命中造成的伤害
    private float m_Damage;

    // 初始化物理和触发器碰撞
    private void Awake()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
       
    }

    // 发射前写入本次子弹参数
    public void Init(Vector2 direction, float speed, float lifeTime, float damage)
    {
        m_Direction = direction.sqrMagnitude > 0f ? direction.normalized : Vector2.right;
        m_Speed = speed;
        m_LifeTimer = lifeTime;
        m_Damage = damage;
    }

    // 推进子弹并检查生命周期
    private void Update()
    {
        transform.position += (Vector3)(m_Direction * m_Speed * Time.deltaTime);
        m_LifeTimer -= Time.deltaTime;
        if (m_LifeTimer <= 0f)
        {
            Recycle();
        }
    }

    // 触发器命中敌人
    private void OnTriggerEnter2D(Collider2D other)
    {
        Enemy enemy = other.GetComponent<Enemy>();
        Vector3 hitPosition = other.ClosestPoint(transform.position);
        HitEnemy(enemy, hitPosition);
    }

    // 碰撞器命中敌人
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Enemy enemy = collision.collider.GetComponent<Enemy>();
        Vector3 hitPosition = collision.contactCount > 0 ? collision.GetContact(0).point : transform.position;
        HitEnemy(enemy, hitPosition);
    }

    // 对敌人造成伤害后回收子弹
    private void HitEnemy(Enemy enemy, Vector3 hitPosition)
    {
        if (enemy == null) return;

        enemy.TakeDamage(m_Damage, hitPosition);
        Recycle();
    }

    // 回收到对象池
    private void Recycle()
    {
        ObjectPool.PushObj(gameObject);
    }
}