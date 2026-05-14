using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 基础自动射击武器
public class Weapon_1 : EquipBase
{
    // 未配置武器数据时使用的默认子弹 Resources 路径
    private const string DefaultBulletResPath = "Prefab/Weapon/Weapon_1_Bullet";
    // 自动索敌使用的敌人 Tag
    private const string EnemyTag = "Enemy";

    // 当前开火冷却计时
    private float m_FireTimer;
    // 武器配置
    private WeaponConfigSO m_Config;
    // 当前等级参数
    private WeaponLevelData m_LevelData = WeaponLevelData.Default;
    // 子弹预制体缓存
    private GameObject m_BulletPrefab;

    // 装备进入时初始化冷却和子弹资源
    public override void OnEquipEnter(Player player)
    {
        m_FireTimer = 0f;
        ApplyLevelData();
        LoadBulletPrefab();
    }

    // 装备每帧更新：冷却结束后自动攻击最近敌人
    public override void OnEquipUpdate(Player player)
    {
        if (player == null) return;
        m_FireTimer -= Time.deltaTime;
        if (m_FireTimer > 0f) return;
        Transform enemy = FindNearestEnemy(player.transform.position);
        if (enemy == null) return;

        Fire(player.transform.position, enemy.position);
        m_FireTimer = Mathf.Max(0.01f, m_LevelData.fireInterval);
    }
    
    // 固定帧更新入口，当前武器暂不使用
    public override void OnEquipFixedUpdate(Player player)
    {
    }

    // 装备离开时清理运行时状态
    public override void OnEquipExit(Player player)
    {
        m_FireTimer = 0f;
        m_Config = null;
        m_LevelData = WeaponLevelData.Default;
        m_BulletPrefab = null;
    }

    // 升级后刷新当前等级参数
    protected override void OnEquipLevelUp(Player player)
    {
        ApplyLevelData();
    }

    // 应用配置中的当前等级参数
    private void ApplyLevelData()
    {
        m_Config = EquipData != null ? EquipData.weaponConfig : null;
        m_LevelData = m_Config != null ? m_Config.GetLevelData(Level) : WeaponLevelData.Default;
    }

    // 加载或读取配置中的子弹预制体
    private void LoadBulletPrefab()
    {
        if (m_Config != null && m_Config.bulletPrefab != null)
        {
            m_BulletPrefab = m_Config.bulletPrefab;
            return;
        }

        string bulletPath = m_Config != null && !string.IsNullOrEmpty(m_Config.bulletPrefabPath)
            ? m_Config.bulletPrefabPath
            : DefaultBulletResPath;
        m_BulletPrefab = ResourceManager.Instance.LoadRes<GameObject>(bulletPath);
    }

    // 查找最近的敌人
    private Transform FindNearestEnemy(Vector3 origin)
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(EnemyTag);
        Transform nearestEnemy = null;
        float nearestDistance = float.MaxValue;

        for (int i = 0; i < enemies.Length; i++)
        {
            float distance = (enemies[i].transform.position - origin).sqrMagnitude;
            if (distance >= nearestDistance) continue;

            nearestDistance = distance;
            nearestEnemy = enemies[i].transform;
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

        int count = Mathf.Max(1, m_LevelData.bulletCount);
        float startAngle = count == 1 ? 0f : -m_LevelData.spreadAngle * (count - 1) * 0.5f;
        for (int i = 0; i < count; i++)
        {
            Vector2 bulletDirection = Quaternion.Euler(0f, 0f, startAngle + m_LevelData.spreadAngle * i) * direction;
            GameObject bullet = ObjectPool.GetObj(m_BulletPrefab);
            bullet.transform.position = firePosition;
            bullet.transform.rotation = Quaternion.identity;
            bullet.transform.right = bulletDirection;
            LaunchBullet(bullet, bulletDirection);
        }
    }

    // 发射子弹
    private void LaunchBullet(GameObject bullet, Vector2 direction)
    {
        Weapon_1_Bullet bulletMove = bullet.GetComponent<Weapon_1_Bullet>();
        if (bulletMove == null)
        {
            bulletMove = bullet.AddComponent<Weapon_1_Bullet>();
        }
        bulletMove.Init(direction, m_LevelData.bulletSpeed, m_LevelData.bulletLifeTime, m_LevelData.damage);
    }
}


