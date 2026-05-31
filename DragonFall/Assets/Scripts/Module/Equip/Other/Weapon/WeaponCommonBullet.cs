using UnityEngine;

// 普通的子弹，打到人之后就会回收
public class WeaponCommonBullet : ThingBase
{
    // 飞行方向
    private Vector2 m_Direction;
    // 飞行速度
    private float m_Speed;
    // 剩余存活时间
    private float m_LifeTimer;
    // 命中造成的伤害
    private float m_Damage;
    private EquipBase m_SourceEquip;

    private void Awake()
    {
        interactable = false;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    public void Init(Vector2 direction, float speed, float lifeTime, float damage, EquipBase sourceEquip = null)
    {
        ClearCallbacks();
        m_Direction = direction.sqrMagnitude > 0f ? direction.normalized : Vector2.right;
        m_Speed = speed;
        m_LifeTimer = lifeTime;
        m_Damage = damage;
        m_SourceEquip = sourceEquip;
        RegisterCollisionCallbacks();
    }

    private void RegisterCollisionCallbacks()
    {
        OnTriggerEnter2DCallBack = OnTriggerEnterHandler;
        OnCollisionEnter2DCallBack = OnCollisionEnterHandler;
    }

    private void OnTriggerEnterHandler(ThingBase thing, Collider2D other)
    {
        Vector3 hitPosition = other.ClosestPoint(transform.position);
        HitEnemy(other.GetComponent<Enemy>(), hitPosition);
    }

    private void OnCollisionEnterHandler(ThingBase thing, Collision2D collision)
    {
        Vector3 hitPosition = collision.contactCount > 0 ? collision.GetContact(0).point : transform.position;
        HitEnemy(collision.collider.GetComponent<Enemy>(), hitPosition);
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

    private void HitEnemy(Enemy enemy, Vector3 hitPosition)
    {
        if (enemy == null) return;
        float actualDamage = enemy.TakeDamage(m_Damage, hitPosition);
        EquipManager.Instance.AddDamage(m_SourceEquip, actualDamage);
        Recycle();
    }

    private void Recycle()
    {
        ClearCallbacks();
        ObjectPool.PushObj(gameObject);
    }
}
