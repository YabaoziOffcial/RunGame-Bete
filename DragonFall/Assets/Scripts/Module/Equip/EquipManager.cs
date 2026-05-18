using System;
using System.Collections.Generic;
using UnityEngine;
using YBZ.Design;

// 玩家身上的装备运行时管理器
public class EquipManager : Singleton<EquipManager>
{
    // 装备所属玩家，所有装备生命周期都围绕它执行
    private Player m_Owner;

    // 当前玩家已拥有的运行时装备实例
    public List<EquipBase> CurrentEquips { get; private set; } = new List<EquipBase>();
    // 装备列表变化时通知 UI 刷新
    public event Action CurrentEquipsChanged;

    // 兼容外部只读查询，避免外部必须改调用点
    public IReadOnlyList<EquipBase> Equips => CurrentEquips;

    public void Init(Player owner)
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
        CurrentEquips.Add(equip);
        equip.Enter(m_Owner, equipData);
        NotifyCurrentEquipsChanged();
        return equip;
    }

    // 升级指定类名的第一件装备
    public bool UpgradeEquip(string className)
    {
        EquipBase equip = GetEquip(className);
        if (equip == null) return false;

        equip.LevelUp();
        NotifyCurrentEquipsChanged();
        return true;
    }

    // 每帧驱动所有装备
    public void UpdateAll()
    {
        for (int i = 0; i < CurrentEquips.Count; i++)
        {
            CurrentEquips[i].Tick(m_Owner);
        }
    }

    // 固定帧驱动所有装备
    public void FixedUpdateAll()
    {
        for (int i = 0; i < CurrentEquips.Count; i++)
        {
            CurrentEquips[i].FixedTick(m_Owner);
        }
    }

    // 移除单件装备，并触发退出清理
    public bool RemoveEquip(EquipBase equip)
    {
        if (equip == null || !CurrentEquips.Remove(equip)) return false;

        equip.Exit(m_Owner);
        NotifyCurrentEquipsChanged();
        return true;
    }

    // 清空装备列表，通常用于玩家销毁或重开一局
    public void Clear()
    {
        for (int i = CurrentEquips.Count - 1; i >= 0; i--)
        {
            CurrentEquips[i].Exit(m_Owner);
        }

        CurrentEquips.Clear();
        NotifyCurrentEquipsChanged();
    }

    // 按类名查找当前已装备的装备
    public EquipBase GetEquip(string className)
    {
        for (int i = 0; i < CurrentEquips.Count; i++)
        {
            if (CurrentEquips[i].EquipData != null && CurrentEquips[i].EquipData.className == className)
            {
                return CurrentEquips[i];
            }
        }

        return null;
    }

    private void NotifyCurrentEquipsChanged()
    {
        CurrentEquipsChanged?.Invoke();
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
