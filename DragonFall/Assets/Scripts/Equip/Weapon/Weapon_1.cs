using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 基础自动射击武器
public class Weapon_1 : EquipBase
{
    // 子弹预制体 Resources 路径
    private const string BulletResPath = "Prefab/Weapon/Weapon_1_Bullet";
    // 自动索敌使用的敌人 Tag
    private const string EnemyTag = "Enemy";

    // 开火间隔
    public float FireInterval = 1f;
    // 单次开火子弹数量
    public int BulletCount = 1;
    // 子弹移动速度
    public float BulletSpeed = 4f;
    // 子弹存活时间
    public float BulletLifeTime = 5f;
    // 多发子弹之间的散射角度
    public float SpreadAngle = 8f;
    // 单颗子弹伤害
    public float Damage = 10f;

    // 当前开火冷却计时
    private float m_FireTimer;
    // 子弹预制体缓存
    private GameObject m_BulletPrefab;

    // 装备进入时初始化冷却和子弹资源
    public override void OnEquipEnter(Player player)
    {
        m_FireTimer = 0f;
        m_BulletPrefab = ResourceManager.Instance.LoadRes<GameObject>(BulletResPath);
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
        m_FireTimer = FireInterval;
    }
    
    // 固定帧更新入口，当前武器暂不使用
    public override void OnEquipFixedUpdate(Player player)
    {
    }

    // 装备离开时清理运行时状态
    public override void OnEquipExit(Player player)
    {
        m_FireTimer = 0f;
        m_BulletPrefab = null;
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
            m_BulletPrefab = ResourceManager.Instance.LoadRes<GameObject>(BulletResPath);
        }
        if (m_BulletPrefab == null) return;

        Vector2 direction = targetPosition - firePosition;
        if (direction.sqrMagnitude <= 0f) direction = Vector2.right;
        direction.Normalize();

        int count = Mathf.Max(1, BulletCount);
        float startAngle = count == 1 ? 0f : -SpreadAngle * (count - 1) * 0.5f;
        for (int i = 0; i < count; i++)
        {
            Vector2 bulletDirection = Quaternion.Euler(0f, 0f, startAngle + SpreadAngle * i) * direction;
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
        bulletMove.Init(direction, BulletSpeed, BulletLifeTime, Damage);
    }
}


