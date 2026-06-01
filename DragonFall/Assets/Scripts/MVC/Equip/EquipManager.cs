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

    public IReadOnlyList<EquipBase> Equips => m_EquipOrder;
    public bool IsSessionActive => m_IsSessionActive;

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

    /// <summary>按类名添加装备（无 SO 时用默认 EquipData）。</summary>
    public EquipBase AddEquip(string className)
    {
        return AddEquip(className, null);
    }

    /// <summary>通过 WeaponConfigSO 添加装备。</summary>
    public EquipBase AddEquip(WeaponConfigSO config)
    {
        if (config == null)
        {
            Debug.LogError("装备配置为空");
            return null;
        }

        return AddEquip(config.className, config);
    }

    /// <summary>反射创建装备实例，Enter 后广播 EquipListChanged。</summary>
    public EquipBase AddEquip(string className, WeaponConfigSO config)
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
        NotifyCurrentEquipsChanged();
        return equip;
    }

    /// <summary>升级当前第一件同名装备。</summary>
    public bool UpgradeEquip(string className)
    {
        EquipBase equip = GetEquip(className);
        if (equip == null) return false;
        equip.LevelUp(m_Owner as Player);
        NotifyCurrentEquipsChanged();
        return true;
    }

    /// <summary>移除单件装备并 Exit 清理。</summary>
    public bool RemoveEquip(EquipBase equip)
    {
        if (equip == null || !CurrentEquips.Remove(equip)) return false;

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

    private EquipGameData CreateEquipGameData()
    {
        long addTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        return new EquipGameData(addTimestamp);
    }

    // 将配置资产转换成运行时装备数据
    private EquipData CreateEquipData(string className, WeaponConfigSO config)
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
