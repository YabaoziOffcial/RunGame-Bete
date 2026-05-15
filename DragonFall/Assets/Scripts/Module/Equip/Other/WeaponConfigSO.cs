using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "DragonFall/Equip/Weapon Config")]
public class WeaponConfigSO : ScriptableObject
{
    [Header("Equip")]
    // 装备唯一 id，后续可用于存档或表格索引
    public int id;
    // 对应的 EquipBase 子类名，例如 Weapon_1
    public string className = "Weapon_1";
    public int quality;
    public int rarity;
    public int icon;
    // UI 直接显示用的武器图标
    public Sprite iconSprite;
    // 未直接配置 Sprite 时，从 Resources/YooAsset 使用该路径加载
    public string iconPath;
    // Inspector 和 UI 中显示的武器名
    public string weaponName = "Weapon_1";
    [TextArea]
    public string description;

    [Header("Prefab")]
    // 优先使用直接引用的子弹预制体
    public GameObject bulletPrefab;
    // 未直接引用预制体时，从 ResourceManager 使用该路径加载
    public string bulletPrefabPath = "Prefab/Weapon/Weapon_1_Bullet";

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
            quality = quality,
            rarity = rarity,
            icon = icon,
            iconSprite = iconSprite,
            iconPath = iconPath,
            name = weaponName,
            description = description,
            weaponConfig = this
        };
    }

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

[System.Serializable]
public class WeaponLevelData
{
    // 开火间隔，数值越小射速越快
    public float fireInterval = 1f;
    // 单次开火生成的子弹数量
    public int bulletCount = 1;
    // 子弹飞行速度
    public float bulletSpeed = 4f;
    // 子弹存在时长
    public float bulletLifeTime = 5f;
    // 多发子弹之间的角度间隔
    public float spreadAngle = 8f;
    // 单颗子弹造成的伤害
    public float damage = 10f;

    // 没有配置资产时的默认等级参数
    public static WeaponLevelData Default => new WeaponLevelData();
}
