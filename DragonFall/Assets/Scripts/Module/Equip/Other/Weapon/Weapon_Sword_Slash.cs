using System.Collections.Generic;
using UnityEngine;

// 长剑刀光：静止范围伤害，动画由预制体自身播放
public class Weapon_Sword_Slash : MonoBehaviour
{
    private float m_Damage;
    private EquipBase m_SourceEquip;
    private readonly HashSet<Enemy> m_HitEnemies = new HashSet<Enemy>();

    public void Init(float lifeTime, float damage, EquipBase sourceEquip = null)
    {
        m_Damage = damage;
        m_SourceEquip = sourceEquip;
        m_HitEnemies.Clear();
        CancelInvoke(nameof(Recycle));
        Invoke(nameof(Recycle), lifeTime);
    }

    private void Recycle()
    {
        m_HitEnemies.Clear();
        ObjectPool.PushObj(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy == null || m_HitEnemies.Contains(enemy)) return;

        m_HitEnemies.Add(enemy);
        Vector3 hitPosition = other.ClosestPoint(transform.position);
        float actualDamage = enemy.TakeDamage(m_Damage, hitPosition);
        EquipManager.Instance.AddDamage(m_SourceEquip, actualDamage);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Enemy enemy = collision.collider.GetComponent<Enemy>();
        if (enemy == null || m_HitEnemies.Contains(enemy)) return;

        m_HitEnemies.Add(enemy);
        Vector3 hitPosition = collision.contactCount > 0 ? collision.GetContact(0).point : transform.position;
        float actualDamage = enemy.TakeDamage(m_Damage, hitPosition);
        EquipManager.Instance.AddDamage(m_SourceEquip, actualDamage);
    }
}
