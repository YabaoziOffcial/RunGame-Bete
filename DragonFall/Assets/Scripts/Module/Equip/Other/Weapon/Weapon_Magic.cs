using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 基础自动射击武器
public class Weapon_Magic : EquipBase
{
    // 自动索敌使用的敌人 Tag
    private const string EnemyTag = "Enemy";
    // 当前开火冷却计时
    private float m_FireTimer;
    // 武器配置
    private WeaponConfigSO m_Config;
    // 当前等级参数
    private WeaponLevelData m_WeaponData = WeaponLevelData.Default;
    // 子弹预制体缓存
    private GameObject m_BulletPrefab;

    // 装备进入时初始化冷却和子弹资源
    public override void Enter(Player player)
    {
        m_FireTimer = 0f;
        ApplyLevelData();
        LoadBulletPrefab();
    }

    // 装备每帧更新：冷却结束后自动攻击最近敌人
    public override void Update(Player player)
    {
        if (player == null) return;

        m_FireTimer -= Time.deltaTime;
        if (m_FireTimer > 0f) return;

        Transform enemy = FindNearestEnemy(player.transform.position);
        if (enemy == null)
        {
            m_FireTimer = m_WeaponData.fireInterval;
            return;
        }

        Fire(player.transform.position, enemy.position);
        m_FireTimer = m_WeaponData.fireInterval;
    }
    
    // 固定帧更新入口，当前武器暂不使用
    public override void FixedUpdate(Player player)
    {
    }

    // 装备离开时清理运行时状态
    public override void Exit(Player player)
    {
        m_FireTimer = 0f;
        m_Config = null;
        m_WeaponData = WeaponLevelData.Default;
        m_BulletPrefab = null;
    }

    // 升级后刷新当前等级参数
    public override void LevelUp(Player player)
    {
        ApplyLevelData();
    }

    // 应用配置中的当前等级参数
    private void ApplyLevelData()
    {
        m_Config = ResourceManager.Instance.LoadRes<WeaponConfigSO>(PathConst.GetWeaponConfigPath("WeaponMagicConfig"));
        if (m_Config == null && EquipData != null)
        {
            m_Config = EquipData.weaponConfig;
        }
        m_WeaponData = m_Config != null ? m_Config.GetLevelData(Level) : WeaponLevelData.Default;
    }

    // 加载或读取配置中的子弹预制体
    private void LoadBulletPrefab()
    {
        if (m_Config != null && m_Config.bulletPrefab != null)
        {
            m_BulletPrefab = m_Config.bulletPrefab;
            return;
        }
        m_BulletPrefab = null;
    }

    private Transform[] enmeyTransforms = new Transform[10];
    private Collider2D[] m_EnemyColliders = new Collider2D[10];

    // 查找最近的敌人
    private Transform FindNearestEnemy(Vector3 origin)
    {
        var player = GameController.Instance.Player;
        if (player == null) return null;

        Collider2D playerCollider = player.GetComponent<Collider2D>();
        if (playerCollider == null) return null;

        for (int i = 0; i < enmeyTransforms.Length; i++)
        {
            enmeyTransforms[i] = null;
            m_EnemyColliders[i] = null;
        }

        ContactFilter2D filter = new ContactFilter2D();
        filter.useTriggers = true;
        int enemyCount = playerCollider.OverlapCollider(filter, m_EnemyColliders);

        Transform nearestEnemy = null;
        float nearestDistance = float.MaxValue;
        for (int i = 0; i < enemyCount; i++)
        {
            Collider2D enemyCollider = m_EnemyColliders[i];
            if (enemyCollider == null || !enemyCollider.CompareTag(EnemyTag)) continue;

            Transform enemyTransform = enemyCollider.transform;
            enmeyTransforms[i] = enemyTransform;

            float distance = (enemyTransform.position - origin).sqrMagnitude;
            if (distance >= nearestDistance) continue;

            nearestDistance = distance;
            nearestEnemy = enemyTransform;
        }

        return nearestEnemy;
    }

    // 开火
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

        int count = Mathf.Max(1, m_WeaponData.bulletCount);
        float startAngle = count == 1 ? 0f : -m_WeaponData.spreadAngle * (count - 1) * 0.5f;
        for (int i = 0; i < count; i++)
        {
            Vector2 bulletDirection = Quaternion.Euler(0f, 0f, startAngle + m_WeaponData.spreadAngle * i) * direction;
            GameObject bullet = ObjectPool.GetObj(m_BulletPrefab, m_Config != null && m_Config.isPlayerChild ? Owner.BulletPool : null);
            bullet.transform.position = firePosition;
            bullet.transform.rotation = Quaternion.identity;
            bullet.transform.right = bulletDirection;
            LaunchBullet(bullet, bulletDirection);
        }
    }

    // 发射子弹
    private void LaunchBullet(GameObject bullet, Vector2 direction)
    {
        WeaponCommonBullet bulletMove = bullet.GetComponent<WeaponCommonBullet>();
        if (bulletMove == null)
        {
            bulletMove = bullet.AddComponent<WeaponCommonBullet>();
        }
        bulletMove.Init(direction, m_WeaponData.bulletSpeed, m_WeaponData.bulletLifeTime, m_WeaponData.damage);
    }
}


