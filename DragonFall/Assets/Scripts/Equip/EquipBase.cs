using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 装备基类
public abstract class EquipBase
{
    // 装备静态和成长数据
    public EquipData EquipData{get; set;} 

    // 装备被添加到玩家时调用
    public abstract void OnEquipEnter(Player player);

    // 装备随玩家每帧更新时调用
    public abstract void OnEquipUpdate(Player player);

    // 装备随玩家固定帧更新时调用
    public abstract void OnEquipFixedUpdate(Player player);

    // 装备从玩家身上移除时调用
    public abstract void OnEquipExit(Player player);
}

// 装备配置数据
public class EquipData
{
    // 装备 id
    public int id;
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
}
