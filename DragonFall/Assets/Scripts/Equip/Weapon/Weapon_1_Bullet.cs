using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon_1_Bullet : MonoBehaviour
{
    private const float ColliderRadius = 0.1f;

    private Vector2 m_Direction;
    private float m_Speed;
    private float m_LifeTimer;
    private float m_Damage;

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

        CircleCollider2D collider2d = GetComponent<CircleCollider2D>();
        if (collider2d == null)
        {
            collider2d = gameObject.AddComponent<CircleCollider2D>();
        }
        collider2d.isTrigger = true;
        collider2d.radius = ColliderRadius;
    }

    public void Init(Vector2 direction, float speed, float lifeTime, float damage)
    {
        m_Direction = direction.sqrMagnitude > 0f ? direction.normalized : Vector2.right;
        m_Speed = speed;
        m_LifeTimer = lifeTime;
        m_Damage = damage;
    }

    private void Update()
    {
        transform.position += (Vector3)(m_Direction * m_Speed * Time.deltaTime);
        m_LifeTimer -= Time.deltaTime;
        if (m_LifeTimer <= 0f)
        {
            Recycle();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        HitEnemy(other.GetComponent<Enemy>());
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HitEnemy(collision.collider.GetComponent<Enemy>());
    }

    private void HitEnemy(Enemy enemy)
    {
        if (enemy == null) return;

        enemy.TakeDamage(m_Damage);
        Recycle();
    }

    private void Recycle()
    {
        ObjectPool.PushObj(gameObject);
    }
}