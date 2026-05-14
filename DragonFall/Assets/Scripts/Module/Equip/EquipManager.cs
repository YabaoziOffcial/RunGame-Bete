using System;
using System.Collections.Generic;
using UnityEngine;

// 玩家身上的装备运行时管理器
public class EquipManager
{
    // 装备所属玩家，所有装备生命周期都围绕它执行
    private readonly Player m_Owner;
    // 当前玩家已拥有的运行时装备实例
    private readonly List<EquipBase> m_Equips = new List<EquipBase>();

    // 只读暴露给外部查询，避免外部直接改列表
    public IReadOnlyList<EquipBase> Equips => m_Equips;

    public EquipManager(Player owner)
    {
        m_Owner = owner;
    }

    public EquipBase AddEquip(string className)
    {
        return AddEquip(className, null);
    }

    // 通过 ScriptableObject 配置添加装备
    public EquipBase AddEquip(WeaponConfigSO config)
    {
        if (config == null)
        {
            Debug.LogError("装备配置为空");
            return null;
        }

        return AddEquip(config.className, config);
    }

    // 根据装备类名创建运行时实例，并触发装备进入逻辑
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
        m_Equips.Add(equip);
        equip.Enter(m_Owner, equipData);
        return equip;
    }

    // 升级指定类名的第一件装备
    public bool UpgradeEquip(string className)
    {
        EquipBase equip = GetEquip(className);
        if (equip == null) return false;

        equip.LevelUp();
        return true;
    }

    // 每帧驱动所有装备
    public void UpdateAll()
    {
        for (int i = 0; i < m_Equips.Count; i++)
        {
            m_Equips[i].Tick(m_Owner);
        }
    }

    // 固定帧驱动所有装备
    public void FixedUpdateAll()
    {
        for (int i = 0; i < m_Equips.Count; i++)
        {
            m_Equips[i].FixedTick(m_Owner);
        }
    }

    // 移除单件装备，并触发退出清理
    public bool RemoveEquip(EquipBase equip)
    {
        if (equip == null || !m_Equips.Remove(equip)) return false;

        equip.Exit(m_Owner);
        return true;
    }

    // 清空装备列表，通常用于玩家销毁或重开一局
    public void Clear()
    {
        for (int i = m_Equips.Count - 1; i >= 0; i--)
        {
            m_Equips[i].Exit(m_Owner);
        }

        m_Equips.Clear();
    }

    // 按类名查找当前已装备的装备
    public EquipBase GetEquip(string className)
    {
        for (int i = 0; i < m_Equips.Count; i++)
        {
            if (m_Equips[i].EquipData != null && m_Equips[i].EquipData.className == className)
            {
                return m_Equips[i];
            }
        }

        return null;
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
