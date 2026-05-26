using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 飞镖武器
public class Weapon_Dart : EquipBase
{
    // 当前发射冷却计时
    private float m_FireTimer;
    // 武器配置
    private WeaponConfigSO m_Config;
    // 当前等级参数
    private WeaponLevelData m_WeaponData = WeaponLevelData.Default;
    // 飞镖预制体缓存
    private GameObject m_BulletPrefab;
    // 当前发射方向，默认向左
    private Vector2 m_FireDirection = Vector2.right;

    // 装备进入时初始化冷却和飞镖资源
    public override void Enter(Player player)
    {
        m_FireTimer = 0f;
        m_FireDirection = Vector2.left;
        ApplyLevelData();
        LoadBulletPrefab();
    }

    // 每隔一段时间按当前记录的八方向发射飞镖
    public override void Update(Player player)
    {
        if (player == null) return;

        UpdateFireDirectionByInput();
        m_FireTimer -= Time.deltaTime;
        if (m_FireTimer > 0f) return;

        Fire(player.transform.position, m_FireDirection);
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
        m_FireDirection = Vector2.left;
    }

    // 升级后刷新当前等级参数
    public override void LevelUp(Player player)
    {
        ApplyLevelData();
    }

    // 应用配置中的当前等级参数
    private void ApplyLevelData()
    {
        m_Config = ResourceManager.Instance.LoadRes<WeaponConfigSO>(PathConst.GetWeaponConfigPath("WeaponDartConfig"));
        if (m_Config == null && EquipData != null)
        {
            m_Config = EquipData.weaponConfig;
        }
        m_WeaponData = m_Config != null ? m_Config.GetLevelData(Level) : WeaponLevelData.Default;
    }

    // 加载或读取配置中的飞镖预制体
    private void LoadBulletPrefab()
    {
        if (m_Config != null && m_Config.bulletPrefab != null)
        {
            m_BulletPrefab = m_Config.bulletPrefab;
            return;
        }
        m_BulletPrefab = null;
    }

    // 接收到输入时更新为平面八方向；无输入时保留上一次方向
    private void UpdateFireDirectionByInput()
    {
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (input.sqrMagnitude <= 0f) return;

        m_FireDirection = new Vector2(Mathf.Sign(input.x), Mathf.Sign(input.y));
        if (Mathf.Approximately(input.x, 0f))
        {
            m_FireDirection.x = 0f;
        }
        if (Mathf.Approximately(input.y, 0f))
        {
            m_FireDirection.y = 0f;
        }
        m_FireDirection.Normalize();
    }

    // 以当前移动方向为中心扇形发射飞镖
    private void Fire(Vector3 firePosition, Vector2 direction)
    {
        if (m_BulletPrefab == null)
        {
            LoadBulletPrefab();
        }
        if (m_BulletPrefab == null) return;

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

    // 发射飞镖
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
