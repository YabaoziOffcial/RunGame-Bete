using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 装备基类
public abstract class EquipBase
{
    // 装备静态和成长数据
    public EquipData EquipData { get; private set; } = new EquipData();
    // 当前装备所属玩家，便于装备升级或退出时访问宿主
    protected Player Owner { get; private set; }
    // 对外统一暴露的装备等级，最低为 1 级
    public int Level => EquipData != null ? Mathf.Max(1, EquipData.level) : 1;

    // 装备进入玩家装备栏时的统一入口
    public void Enter(Player player, EquipData equipData)
    {
        Owner = player;
        EquipData = equipData ?? new EquipData();
        if (EquipData.level <= 0) EquipData.level = 1;
        OnEquipEnter(player);
    }

    // 普通帧更新入口，由 EquipManager 统一驱动
    public void Tick(Player player)
    {
        OnEquipUpdate(player);
    }

    // 固定帧更新入口，给物理类装备预留
    public void FixedTick(Player player)
    {
        OnEquipFixedUpdate(player);
    }

    // 装备移除或玩家销毁时的统一清理入口
    public void Exit(Player player)
    {
        OnEquipExit(player);
        Owner = null;
    }

    // 装备升级入口，只修改等级并通知具体装备刷新参数
    public void LevelUp()
    {
        if (EquipData == null) EquipData = new EquipData();
        EquipData.level = Mathf.Max(1, EquipData.level + 1);
        OnEquipLevelUp(Owner);
    }

    // 装备被添加到玩家时调用
    public abstract void OnEquipEnter(Player player);

    // 装备随玩家每帧更新时调用
    public abstract void OnEquipUpdate(Player player);

    // 装备随玩家固定帧更新时调用
    public abstract void OnEquipFixedUpdate(Player player);

    // 装备从玩家身上移除时调用
    public abstract void OnEquipExit(Player player);

    // 装备升级时调用
    protected virtual void OnEquipLevelUp(Player player)
    {
    }
}

// 装备配置数据
public class EquipData
{
    // 装备 id
    public int id;
    // 装备类名
    public string className;
    // 装备等级
    public int level;
    // 装备品质
    public int quality;
    // 装备稀有度
    public int rarity;
    // 图标 id
    public int icon;
    // 装备名称
    public string name;
    // 装备描述
    public string description;
    // 武器配置数据
    public WeaponConfigSO weaponConfig;
}
