using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon_1 : EquipBase
{
    private const string BulletResPath = "Weapon_1_Bullet";
    private const string EnemyTag = "Enemy";

    public float FireInterval = 1f;
    public int BulletCount = 1;
    public float BulletSpeed = 10f;
    public float BulletLifeTime = 5f;
    public float SpreadAngle = 8f;

    private float m_FireTimer;
    private GameObject m_BulletPrefab;

    public override void OnEquipEnter(Player player)
    {
        m_FireTimer = 0f;
        m_BulletPrefab = ResourceManager.Instance.LoadRes<GameObject>(BulletResPath);
    }

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
    
    public override void OnEquipFixedUpdate(Player player)
    {
    }

    public override void OnEquipExit(Player player)
    {
        m_FireTimer = 0f;
        m_BulletPrefab = null;
    }

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
            GameObject bullet = Object.Instantiate(m_BulletPrefab, firePosition, Quaternion.identity);
            bullet.transform.right = bulletDirection;
            LaunchBullet(bullet, bulletDirection);
        }
    }

    private void LaunchBullet(GameObject bullet, Vector2 direction)
    {
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = direction.normalized * BulletSpeed;
            return;
        }

        Weapon_1_Bullet bulletMove = bullet.GetComponent<Weapon_1_Bullet>();
        if (bulletMove == null)
        {
            bulletMove = bullet.AddComponent<Weapon_1_Bullet>();
        }
        bulletMove.Init(direction, BulletSpeed, BulletLifeTime);
    }
}


