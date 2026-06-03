using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 装备基类
public abstract class EquipBase
{
    // 装备静态和成长数据
    public EquipData EquipData { get; set; } = new EquipData();
    // 当前装备所属玩家，便于装备升级或退出时访问宿主
    public Player Owner { get; set; }
    // 对外统一暴露的装备等级，最低为 1 级
    public int Level => EquipData != null ? Mathf.Max(1, EquipData.level) : 1;
    
    // 装备被添加到玩家时调用
    public abstract void Enter(Player player);

    // 装备随玩家每帧更新时调用
    public abstract void Update(Player player);

    // 装备随玩家固定帧更新时调用
    public abstract void FixedUpdate(Player player);

    // 装备从玩家身上移除时调用
    public abstract void Exit(Player player);

    // 装备升级时调用
    public abstract void LevelUp (Player player);
    
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
    // UI 直接显示用的图标
    public Sprite iconSprite;
    // 未直接配置 Sprite 时，从 ResourceManager 加载的图标路径
    public string iconPath;
    // 装备名称
    public string name;
    // 装备描述
    public string description;
    // 武器配置数据
    public WeaponConfig weaponConfig;
}
