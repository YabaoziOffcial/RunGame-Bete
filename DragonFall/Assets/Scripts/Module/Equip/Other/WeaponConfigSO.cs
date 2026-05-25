using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "DragonFall/Equip/Weapon Config")]
public class WeaponConfigSO : ScriptableObject
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

    [Header("是否是玩家子物体")]
    public bool isPlayerChild = false;
    // 优先使用直接引用的子弹预制体
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

    public string description = "这里是升级效果";

    // 没有配置资产时的默认等级参数
    public static WeaponLevelData Default => new WeaponLevelData();
}
