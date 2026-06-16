using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 自动瞄准魔法武器：自动锁定范围内最近敌人发射魔法弹
public class Weapon_Magic : EquipBase
{
    private const string EnemyTag = "Enemy";

    private float m_FireTimer;
    private WeaponConfig m_Config;
    private WeaponLevelData m_WeaponData = WeaponLevelData.Default;
    private GameObject m_BulletPrefab;

    // 物理检测缓存，避免每帧分配
    private readonly Collider2D[] m_EnemyHitBuffer = new Collider2D[32];

    public override void Enter(Player player)
    {
        m_FireTimer = 0f;
        ApplyLevelData();
        LoadBulletPrefab();
    }

    public override void Update(Player player)
    {
        if (player == null) return;

        m_FireTimer -= Time.deltaTime;
        if (m_FireTimer > 0f) return;

        Transform enemy = FindNearestEnemy(player.transform.position);
        if (enemy == null)
        {
            m_FireTimer = m_WeaponData.AttackRate;
            return;
        }

        Fire(player.transform.position, enemy.position);
        m_FireTimer = m_WeaponData.AttackRate;
    }

    public override void FixedUpdate(Player player) { }

    public override void Exit(Player player)
    {
        m_FireTimer = 0f;
        m_Config = null;
        m_WeaponData = WeaponLevelData.Default;
        m_BulletPrefab = null;
    }

    public override void LevelUp(Player player) => ApplyLevelData();

    private void ApplyLevelData()
    {
        m_Config = ResourceManager.Instance.LoadRes<WeaponConfig>(PathConst.GetWeaponConfigPath("WeaponMagicConfig"));
        if (m_Config == null && EquipData != null)
        {
            m_Config = EquipData.weaponConfig;
        }
        m_WeaponData = m_Config != null ? m_Config.GetLevelData(Level) : WeaponLevelData.Default;
    }

    private void LoadBulletPrefab()
    {
        if (m_Config != null && m_Config.bulletPrefab != null)
        {
            m_BulletPrefab = m_Config.bulletPrefab;
        }
    }

    private Transform FindNearestEnemy(Vector3 origin)
    {
        float searchRadius = m_WeaponData.AttackRange > 0f ? m_WeaponData.AttackRange : 8f;

        int count = Physics2D.OverlapCircleNonAlloc(origin, searchRadius, m_EnemyHitBuffer);

        Transform nearest = null;
        float nearestDist = float.MaxValue;
        for (int i = 0; i < count; i++)
        {
            Collider2D col = m_EnemyHitBuffer[i];
            if (col == null || !col.CompareTag(EnemyTag)) continue;

            float dist = (col.transform.position - origin).sqrMagnitude;
            if (dist < nearestDist)
            {
                nearestDist = dist;
                nearest = col.transform;
            }
        }
        return nearest;
    }

    private void Fire(Vector3 firePosition, Vector3 targetPosition)
    {
        if (m_BulletPrefab == null)
        {
            LoadBulletPrefab();
        }
        if (m_BulletPrefab == null) return;

        Vector2 direction = targetPosition - firePosition;
        if (direction.sqrMagnitude <= 0f) direction = Vector2.right;
        direction.Normalize();

        int count = Mathf.Max(1, Mathf.RoundToInt(m_WeaponData.BarrageCount));
        float startAngle = count == 1 ? 0f : -m_WeaponData.AttackRange * (count - 1) * 0.5f;
        for (int i = 0; i < count; i++)
        {
            Vector2 bulletDirection = Quaternion.Euler(0f, 0f, startAngle + m_WeaponData.AttackRange * i) * direction;
            GameObject bullet = ObjectPool.GetObj(m_BulletPrefab, Owner.BulletPool);
            bullet.transform.position = firePosition;
            bullet.transform.rotation = Quaternion.identity;
            bullet.transform.right = bulletDirection;
            LaunchBullet(bullet, bulletDirection);
        }
    }

    private void LaunchBullet(GameObject bullet, Vector2 direction)
    {
        WeaponCommonBullet bulletMove = bullet.GetComponent<WeaponCommonBullet>();
        if (bulletMove == null)
        {
            bulletMove = bullet.AddComponent<WeaponCommonBullet>();
        }
        bulletMove.Init(direction, m_WeaponData.BarrageSpeed, m_WeaponData.BarrageDuration, m_WeaponData.Strength, this);
    }
}


