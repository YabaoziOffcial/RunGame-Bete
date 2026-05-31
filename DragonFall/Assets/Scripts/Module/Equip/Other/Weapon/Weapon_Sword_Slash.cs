using System.Collections.Generic;
using UnityEngine;

// 长剑刀光：静止范围伤害，动画由预制体自身播放
public class Weapon_Sword_Slash : ThingBase
{
    private float m_Damage;
    private EquipBase m_SourceEquip;
    private readonly HashSet<Enemy> m_HitEnemies = new HashSet<Enemy>();

    private void Awake()
    {
        interactable = false;
    }

    public void Init(float lifeTime, float damage, EquipBase sourceEquip = null)
    {
        ClearCallbacks();
        m_Damage = damage;
        m_SourceEquip = sourceEquip;
        m_HitEnemies.Clear();
        RegisterCollisionCallbacks();
        CancelInvoke(nameof(Recycle));
        Invoke(nameof(Recycle), lifeTime);
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

    private void HitEnemy(Enemy enemy, Vector3 hitPosition)
    {
        if (enemy == null || m_HitEnemies.Contains(enemy)) return;

        m_HitEnemies.Add(enemy);
        float actualDamage = enemy.TakeDamage(m_Damage, hitPosition);
        EquipManager.Instance.AddDamage(m_SourceEquip, actualDamage);
    }

    private void Recycle()
    {
        m_HitEnemies.Clear();
        CancelInvoke(nameof(Recycle));
        ClearCallbacks();
        ObjectPool.PushObj(gameObject);
    }
}
