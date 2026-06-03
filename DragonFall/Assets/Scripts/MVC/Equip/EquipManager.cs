using System;
using System.Collections.Generic;
using UnityEngine;
using YBZ.Design;

// 装备控制器：局内装备增删与 Update 驱动（由 GameRoot 调用 Update / FixedUpdate）
public class EquipManager : Singleton<EquipManager>
{
    private Player m_Owner;
    private bool m_IsSessionActive;

    public Dictionary<EquipBase, EquipGameData> CurrentEquips { get; private set; } = new Dictionary<EquipBase, EquipGameData>();
    private readonly List<EquipBase> m_EquipOrder = new List<EquipBase>();
    private readonly List<EquipBase> m_AvailableEquipsBuffer = new List<EquipBase>();
    private readonly Dictionary<string, WeaponConfig> m_WeaponCatalog = new Dictionary<string, WeaponConfig>();

    private EquipConfig m_Config;

    public IReadOnlyList<EquipBase> Equips => m_EquipOrder;
    public bool IsSessionActive => m_IsSessionActive;

    /// <summary>读取 EquipConfig，建立 className → WeaponConfigSO 索引。</summary>
    public void Init()
    {
        m_Config = ResourceManager.Instance.LoadRes<EquipConfig>(PathConst.GetEquipConfigPath("EquipConfig"));
        m_WeaponCatalog.Clear();

        if (m_Config?.equips == null) return;

        for (int i = 0; i < m_Config.equips.Count; i++)
        {
            WeaponConfig weaponConfig = m_Config.equips[i];
            if (weaponConfig == null || string.IsNullOrEmpty(weaponConfig.className)) continue;
            m_WeaponCatalog[weaponConfig.className] = weaponConfig;
        }
    }

    /// <summary>开局：绑定玩家并装备初始武器。</summary>
    public void StartSession(Player owner)
    {
        if (owner == null) return;
        EndSession();
        m_Owner = owner;
        m_IsSessionActive = true;
        AddEquip("Weapon_Sword");
    }

    /// <summary>结束本局装备会话（GameOver 等时机调用）。</summary>
    public void EndSession()
    {
        m_IsSessionActive = false;
        Clear();
        m_Owner = null;
    }

    /// <summary>由 GameRoot.Update 驱动。</summary>
    public void Update()
    {
        if (!m_IsSessionActive || m_Owner == null) return;
        if (GameController.Instance != null && GameController.Instance.IsGameOver) return;

        for (int i = 0; i < m_EquipOrder.Count; i++)
        {
            m_EquipOrder[i].Update(m_Owner);
        }
    }

    /// <summary>由 GameRoot.FixedUpdate 驱动。</summary>
    public void FixedUpdate()
    {
        if (!m_IsSessionActive || m_Owner == null) return;
        if (GameController.Instance != null && GameController.Instance.IsGameOver) return;

        for (int i = 0; i < m_EquipOrder.Count; i++)
        {
            m_EquipOrder[i].FixedUpdate(m_Owner);
        }
    }

    /// <summary>按类名添加装备（优先从 EquipConfig 取 WeaponConfigSO）。</summary>
    public EquipBase AddEquip(string className)
    {
        m_WeaponCatalog.TryGetValue(className, out WeaponConfig config);
        return AddEquip(className, config);
    }

    /// <summary>通过 WeaponConfigSO 添加装备。</summary>
    public EquipBase AddEquip(WeaponConfig config)
    {
        if (config == null)
        {
            Debug.LogError("装备配置为空");
            return null;
        }

        return AddEquip(config.className, config);
    }

    /// <summary>反射创建装备实例，Enter 后广播 EquipListChanged。</summary>
    public EquipBase AddEquip(string className, WeaponConfig config)
    {
        if (string.IsNullOrEmpty(className))
        {
            Debug.LogError("装备类名为空");
            return null;
        }

        Type equipType = typeof(EquipBase).Assembly.GetType(className);
        if (equipType == null || equipType.IsAbstract || !typeof(EquipBase).IsAssignableFrom(equipType))
        {
            Debug.LogError($"未找到装备类: {className}");
            return null;
        }

        EquipBase equip = (EquipBase)Activator.CreateInstance(equipType);
        EquipData equipData = CreateEquipData(className, config);

        // 赋值
        equip.EquipData = equipData;
        equip.Owner = m_Owner;
        CurrentEquips.Add(equip, CreateEquipGameData());
        m_EquipOrder.Add(equip);
        equip.Enter(m_Owner);
        RefreshEquipPlayerStats(equip);
        NotifyCurrentEquipsChanged();
        return equip;
    }

    /// <summary>升级当前第一件同名装备。</summary>
    public bool UpgradeEquip(string className)
    {
        EquipBase equip = GetEquip(className);
        if (equip == null || IsMaxLevel(equip)) return false;

        equip.EquipData.level++;
        equip.LevelUp(m_Owner as Player);
        RefreshEquipPlayerStats(equip);
        NotifyCurrentEquipsChanged();
        return true;
    }

    /// <summary>移除单件装备并 Exit 清理。</summary>
    public bool RemoveEquip(EquipBase equip)
    {
        if (equip == null || !CurrentEquips.Remove(equip)) return false;

        RemoveEquipPlayerStats(equip);
        equip.Exit(m_Owner);
        m_EquipOrder.Remove(equip);
        NotifyCurrentEquipsChanged();
        return true;
    }

    /// <summary>清空全部装备（EndSession / 重开一局）。</summary>
    public void Clear()
    {
        for (int i = m_EquipOrder.Count - 1; i >= 0; i--)
        {
            RemoveEquipPlayerStats(m_EquipOrder[i]);
            m_EquipOrder[i].Exit(m_Owner);
        }

        CurrentEquips.Clear();
        m_EquipOrder.Clear();
        NotifyCurrentEquipsChanged();
    }

    /// <summary>按 className 查找已装备实例。</summary>
    public EquipBase GetEquip(string className)
    {
        for (int i = 0; i < m_EquipOrder.Count; i++)
        {
            if (m_EquipOrder[i].EquipData != null && m_EquipOrder[i].EquipData.className == className)
            {
                return m_EquipOrder[i];
            }
        }

        return null;
    }

    // 按 className 判断玩家是否已拥有该装备
    public bool HasEquip(string className) => GetEquip(className) != null;

    // 按配置判断玩家是否已拥有该装备
    public bool HasEquip(WeaponConfig config) =>
        config != null && HasEquip(config.className);

    // 获取已装备武器的当前等级，未拥有时返回 0
    public int GetEquipLevel(string className) => GetEquip(className)?.Level ?? 0;

    // 生成已拥有装备的升级预览：Lv.当前 -> Lv.下一级，下一级即满级时 -> Max
    public string GetEquipLevelPreview(string className)
    {
        EquipBase equip = GetEquip(className);
        if (equip == null) return string.Empty;

        WeaponConfig config = ResolveWeaponConfig(equip);
        int currentLevel = equip.Level;
        int maxLevel = config != null ? config.MaxLevel : 1;
        int nextLevel = currentLevel + 1;

        if (nextLevel >= maxLevel)
            return $"Lv.{currentLevel} -> Max";

        return $"Lv.{currentLevel} -> Lv.{nextLevel}";
    }

    // 按配置生成升级预览文案
    public string GetEquipLevelPreview(WeaponConfig config) =>
        config != null ? GetEquipLevelPreview(config.className) : string.Empty;

    // 获取选中该装备后将展示的等级效果描述
    public string GetEquipChoiceDescription(WeaponConfig config)
    {
        if (config == null) return string.Empty;

        int previewLevel = HasEquip(config) ? GetEquipLevel(config.className) + 1 : 1;
        return config.GetLevelData(previewLevel).description;
    }

    /// <summary>子弹/武器命中时累计该装备本局伤害统计。</summary>
    public void AddDamage(EquipBase equip, float damage)
    {
        if (equip == null || damage <= 0f) return;
        if (!CurrentEquips.TryGetValue(equip, out EquipGameData equipGameData)) return;

        equipGameData.AddDamage(damage);
    }

    // 装备列表变化时通知 HUD 刷新图标
    private void NotifyCurrentEquipsChanged()
    {
        GameEvents.RaiseEquipListChanged();
    }

    /// <summary>已装备且未满级的武器（升级三选一候选）。</summary>
    public List<EquipBase> GetAvailableEquips()
    {
        m_AvailableEquipsBuffer.Clear();

        for (int i = 0; i < m_EquipOrder.Count; i++)
        {
            EquipBase equip = m_EquipOrder[i];
            if (!IsMaxLevel(equip))
            {
                m_AvailableEquipsBuffer.Add(equip);
            }
        }

        return m_AvailableEquipsBuffer;
    }

    /// <summary>EquipConfig 中尚未获得、可新选的武器。</summary>
    public List<WeaponConfig> GetUndiscoveredWeaponConfigs()
    {
        List<WeaponConfig> result = new List<WeaponConfig>();
        if (m_Config?.equips == null) return result;

        for (int i = 0; i < m_Config.equips.Count; i++)
        {
            WeaponConfig weaponConfig = m_Config.equips[i];
            if (weaponConfig == null || string.IsNullOrEmpty(weaponConfig.className)) continue;
            if (GetEquip(weaponConfig.className) != null) continue;

            result.Add(weaponConfig);
        }

        return result;
    }

    private bool IsMaxLevel(EquipBase equip)
    {
        WeaponConfig config = ResolveWeaponConfig(equip);
        if (config == null) return equip.Level >= 1;

        return config.IsMaxLevel(equip.Level);
    }

    private WeaponConfig ResolveWeaponConfig(EquipBase equip)
    {
        if (equip?.EquipData == null) return null;
        if (equip.EquipData.weaponConfig != null) return equip.EquipData.weaponConfig;

        if (string.IsNullOrEmpty(equip.EquipData.className)) return null;
        m_WeaponCatalog.TryGetValue(equip.EquipData.className, out WeaponConfig config);
        return config;
    }

    private EquipGameData CreateEquipGameData()
    {
        long addTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        return new EquipGameData(addTimestamp);
    }

    // 按当前等级把配置中的成长属性应用到 PlayerStats
    private void RefreshEquipPlayerStats(EquipBase equip)
    {
        if (equip == null || !CurrentEquips.TryGetValue(equip, out EquipGameData gameData)) return;

        PlayerStats stats = GameController.Instance?.Model?.Stats;
        if (stats == null) return;

        stats.RemoveModifiers(gameData.AppliedPlayerBonuses);

        WeaponConfig config = ResolveWeaponConfig(equip);
        PlayerStatModifiers bonuses = config != null
            ? config.GetLevelData(equip.Level)
            : PlayerStatModifiers.Zero;

        stats.AddModifiers(bonuses);
        gameData.AppliedPlayerBonuses = bonuses.Clone();
    }

    // 卸下装备时回滚已应用的成长属性
    private void RemoveEquipPlayerStats(EquipBase equip)
    {
        if (equip == null || !CurrentEquips.TryGetValue(equip, out EquipGameData gameData)) return;

        PlayerStats stats = GameController.Instance?.Model?.Stats;
        stats?.RemoveModifiers(gameData.AppliedPlayerBonuses);
        gameData.AppliedPlayerBonuses = PlayerStatModifiers.Zero;
    }

    // 将配置资产转换成运行时装备数据
    private EquipData CreateEquipData(string className, WeaponConfig config)
    {
        if (config != null)
        {
            return config.CreateEquipData();
        }

        return new EquipData
        {
            className = className,
            level = 1,
            name = className
        };
    }
}
