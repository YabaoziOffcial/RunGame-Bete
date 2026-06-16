using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "DragonFall/Equip/Weapon Config")]
public class WeaponConfig : ScriptableObject
{
    [Header("Equip")]
    // 装备唯一 id，后续可用于存档或表格索引
    public int id;
    // 对应的 EquipBase 子类名，例如 Weapon_1

    [Header("武器key")]
    public string className = "Weapon_1";
    [Header("武器图标")]
    public Sprite iconSprite;
    [Header("武器名称")]
    public string weaponName = "Weapon_1";

    [Header("子弹预制体")]
    public GameObject bulletPrefab;

    [Header("Levels")]
    // 每个下标对应一级武器参数：0 表示 1 级
    public List<WeaponLevelData> levels = new List<WeaponLevelData>
    {
        new WeaponLevelData()
    };

    // 创建一份本局运行时装备数据，避免直接修改配置资产
    public EquipData CreateEquipData()
    {
        return new EquipData
        {
            id = id,
            className = className,
            level = 1,
            iconSprite = iconSprite,
            name = weaponName,
            weaponConfig = this
        };
    }

    public int MaxLevel => levels != null && levels.Count > 0 ? levels.Count : 1;

    public bool IsMaxLevel(int level) => level >= MaxLevel;

    // 获取指定等级参数，超过配置等级时使用最后一级
    public WeaponLevelData GetLevelData(int level)
    {
        if (levels == null || levels.Count == 0)
        {
            return WeaponLevelData.Default;
        }

        int index = Mathf.Clamp(level - 1, 0, levels.Count - 1);
        return levels[index];
    }
}

// 武器等级数据：字段名与 PlayerStats 一致；武器脚本读取战斗字段，成长字段通过 EquipManager 写入 PlayerStats
[System.Serializable]
public class WeaponLevelData : PlayerStatModifiers
{
    [Header("升级描述")]
    [Tooltip("选卡界面展示的效果文案")]
    public string description = "这里是升级效果";

    // AttackRate：开火间隔（秒）；BarrageCount：子弹数；AttackRange：多发扩散角（度）
    public static WeaponLevelData Default => new WeaponLevelData
    {
        AttackRate = 1f,
        BarrageCount = 1f,
        BarrageSpeed = 4f,
        BarrageDuration = 5f,
        AttackRange = 8f,
        Strength = 10f,
    };
}
