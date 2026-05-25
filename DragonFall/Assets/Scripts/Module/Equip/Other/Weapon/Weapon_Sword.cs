using UnityEngine;

// 长剑武器：按玩家移动方向定期释放静止刀光
public class Weapon_Sword : EquipBase
{
    private const string configPath = "Config/WeaponSwordConfig";
    private float m_FireTimer;
    private WeaponConfigSO m_Config;
    private WeaponLevelData m_WeaponData = WeaponLevelData.Default;
    private GameObject m_SlashPrefab;
    private Vector2 m_FireDirection = Vector2.right;

    public override void Enter(Player player)
    {
        m_FireTimer = 0f;
        m_FireDirection = Vector2.right;
        ApplyLevelData();
        LoadSlashPrefab();
    }

    public override void Update(Player player)
    {
        if (player == null) return;

        UpdateFireDirectionByInput();
        m_FireTimer -= Time.deltaTime;
        if (m_FireTimer > 0f) return;

        Fire(player.transform.position, m_FireDirection);
        m_FireTimer = m_WeaponData.fireInterval;
    }

    public override void FixedUpdate(Player player)
    {
    }

    public override void Exit(Player player)
    {
        m_FireTimer = 0f;
        m_Config = null;
        m_WeaponData = WeaponLevelData.Default;
        m_SlashPrefab = null;
        m_FireDirection = Vector2.right;
    }

    public override void LevelUp(Player player)
    {
        ApplyLevelData();
    }

    private void ApplyLevelData()
    {
        m_Config = ResourceManager.Instance.LoadRes<WeaponConfigSO>(configPath);
        m_WeaponData = m_Config.GetLevelData(Level);
    }

    private void LoadSlashPrefab()
    {
        if (m_Config != null && m_Config.bulletPrefab != null)
        {
            m_SlashPrefab = m_Config.bulletPrefab;
            return;
        }
        m_SlashPrefab = m_Config.bulletPrefab;
    }

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

    private void Fire(Vector3 firePosition, Vector2 direction)
    {
        if (m_SlashPrefab == null)
        {
            LoadSlashPrefab();
        }
        if (m_SlashPrefab == null) return;

        int count = Mathf.Max(1, m_WeaponData.bulletCount);
        float startAngle = count == 1 ? 0f : -m_WeaponData.spreadAngle * (count - 1) * 0.5f;
        for (int i = 0; i < count; i++)
        {
            Vector2 slashDirection = Quaternion.Euler(0f, 0f, startAngle + m_WeaponData.spreadAngle * i) * direction;
            GameObject slash = ObjectPool.GetObj(m_SlashPrefab, m_Config.isPlayerChild ? Owner.BulletPool : null);
            slash.transform.position = firePosition;
            slash.transform.rotation = Quaternion.identity;
            slash.transform.right = slashDirection;

            Weapon_Sword_Slash swordSlash = slash.GetComponent<Weapon_Sword_Slash>();
            if (swordSlash == null)
            {
                swordSlash = slash.AddComponent<Weapon_Sword_Slash>();
            }
            swordSlash.Init(m_WeaponData.bulletLifeTime, m_WeaponData.damage);
        }
    }
}
