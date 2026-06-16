#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// 在 Unity 编辑器内生成魔法/飞镖 WeaponConfigSO，避免外部写入 .asset 无法被识别
public static class WeaponConfigGenerator
{
    private const string ConfigFolder = "Assets/Resources/Config/WeaponConfig";
    private const string EquipConfigPath = "Assets/Resources/Config/EquipConfig.asset";

    [MenuItem("DragonFall/装备/修复 EquipConfig 引用")]
    public static void FixEquipConfigReferences()
    {
        WeaponConfig sword = AssetDatabase.LoadAssetAtPath<WeaponConfig>($"{ConfigFolder}/WeaponSwordConfig.asset");
        WeaponConfig magic = AssetDatabase.LoadAssetAtPath<WeaponConfig>($"{ConfigFolder}/WeaponMagicConfig.asset");
        WeaponConfig dart = AssetDatabase.LoadAssetAtPath<WeaponConfig>($"{ConfigFolder}/WeaponDartConfig.asset");

        if (magic == null)
        {
            magic = CreateOrUpdate($"{ConfigFolder}/WeaponMagicConfig.asset", BuildMagicConfig);
        }

        if (dart == null)
        {
            dart = CreateOrUpdate($"{ConfigFolder}/WeaponDartConfig.asset", BuildDartConfig);
        }

        EquipConfig equipConfig = AssetDatabase.LoadAssetAtPath<EquipConfig>(EquipConfigPath);
        if (equipConfig == null)
        {
            Debug.LogError("[DragonFall] 未找到 EquipConfig.asset");
            return;
        }

        var equips = new List<WeaponConfig>();
        if (sword != null) equips.Add(sword);
        if (magic != null) equips.Add(magic);
        if (dart != null) equips.Add(dart);
        equipConfig.equips = equips;
        EditorUtility.SetDirty(equipConfig);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[DragonFall] EquipConfig 引用已修复。");
    }

    [MenuItem("DragonFall/装备/生成属性装备配置")]
    public static void GeneratePassiveConfigs()
    {
        EnsureFolder(ConfigFolder);

        WeaponConfig sword = AssetDatabase.LoadAssetAtPath<WeaponConfig>($"{ConfigFolder}/WeaponSwordConfig.asset");
        WeaponConfig magic = AssetDatabase.LoadAssetAtPath<WeaponConfig>($"{ConfigFolder}/WeaponMagicConfig.asset");
        WeaponConfig dart = AssetDatabase.LoadAssetAtPath<WeaponConfig>($"{ConfigFolder}/WeaponDartConfig.asset");

        WeaponConfig maxHp = CreateOrUpdate($"{ConfigFolder}/EquipMaxHpConfig.asset", BuildStatEquipConfig(
            "EquipItem_MaxHp", "生命护符", "Weapon_Magic_Icon.png",
            new[] { 20f, 40f, 60f, 80f, 100f },
            new[] { "最大生命+20", "最大生命+40", "最大生命+60", "最大生命+80", "最大生命+100（满级）" },
            field: "MaxHp"
        ));

        WeaponConfig moveSpeed = CreateOrUpdate($"{ConfigFolder}/EquipMoveSpeedConfig.asset", BuildStatEquipConfig(
            "EquipItem_MoveSpeed", "疾风靴", "Weapon_Dart_Icon.png",
            new[] { 0.2f, 0.4f, 0.6f },
            new[] { "移动速度+20%", "移动速度+40%", "移动速度+60%（满级）" },
            field: "MoveSpeed"
        ));

        WeaponConfig greed = CreateOrUpdate($"{ConfigFolder}/EquipGreedConfig.asset", BuildStatEquipConfig(
            "EquipItem_Greed", "贪婪之戒", "Weapon_Sword_Icon.png",
            new[] { 0.1f, 0.2f, 0.3f, 0.4f, 0.5f },
            new[] { "掉落率+10%", "掉落率+20%", "掉落率+30%", "掉落率+40%", "掉落率+50%（满级）" },
            field: "Greed"
        ));

        WeaponConfig strength = CreateOrUpdate($"{ConfigFolder}/EquipStrengthConfig.asset", BuildStatEquipConfig(
            "EquipItem_Strength", "力量护符", "Weapon_Magic_Icon.png",
            new[] { 5f, 10f, 15f, 20f, 25f },
            new[] { "基础伤害+5", "基础伤害+10", "基础伤害+15", "基础伤害+20", "基础伤害+25（满级）" },
            field: "Strength"
        ));

        EquipConfig equipConfig = AssetDatabase.LoadAssetAtPath<EquipConfig>(EquipConfigPath);
        if (equipConfig != null)
        {
            var equips = new List<WeaponConfig>();
            if (sword != null) equips.Add(sword);
            if (magic != null) equips.Add(magic);
            if (dart != null) equips.Add(dart);
            if (maxHp != null) equips.Add(maxHp);
            if (moveSpeed != null) equips.Add(moveSpeed);
            if (greed != null) equips.Add(greed);
            if (strength != null) equips.Add(strength);
            equipConfig.equips = equips;
            EditorUtility.SetDirty(equipConfig);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[DragonFall] 已生成4个属性装备配置并更新 EquipConfig。");
    }

    private static System.Func<WeaponConfig> BuildStatEquipConfig(
        string className, string displayName, string iconFileName,
        float[] levelValues, string[] descriptions, string field)
    {
        return () =>
        {
            WeaponConfig config = ScriptableObject.CreateInstance<WeaponConfig>();
            config.className = className;
            config.weaponName = displayName;
            Sprite icon = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Resources/Sprite/Icon/{iconFileName}");
            if (icon == null)
            {
                icon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Sprite/Icon/default_icon.png");
            }
            config.iconSprite = icon;
            config.levels = new List<WeaponLevelData>();

            int count = Mathf.Min(levelValues.Length, descriptions.Length);
            for (int i = 0; i < count; i++)
            {
                var level = new WeaponLevelData { description = descriptions[i] };
                switch (field)
                {
                    case "MaxHp":      level.MaxHp = levelValues[i]; break;
                    case "MoveSpeed":  level.MoveSpeed = levelValues[i]; break;
                    case "Greed":      level.Greed = levelValues[i]; break;
                    case "Strength":   level.Strength = levelValues[i]; break;
                }
                config.levels.Add(level);
            }

            return config;
        };
    }

    [MenuItem("DragonFall/装备/重新生成魔法与飞镖配置")]
    public static void GenerateMagicAndDartConfigs()
    {
        EnsureFolder(ConfigFolder);

        WeaponConfig sword = AssetDatabase.LoadAssetAtPath<WeaponConfig>($"{ConfigFolder}/WeaponSwordConfig.asset");
        WeaponConfig magic = CreateOrUpdate($"{ConfigFolder}/WeaponMagicConfig.asset", BuildMagicConfig);
        WeaponConfig dart = CreateOrUpdate($"{ConfigFolder}/WeaponDartConfig.asset", BuildDartConfig);

        EquipConfig equipConfig = AssetDatabase.LoadAssetAtPath<EquipConfig>(EquipConfigPath);
        if (equipConfig != null)
        {
            var equips = new List<WeaponConfig>();
            if (sword != null) equips.Add(sword);
            if (magic != null) equips.Add(magic);
            if (dart != null) equips.Add(dart);
            equipConfig.equips = equips;
            EditorUtility.SetDirty(equipConfig);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[DragonFall] 已生成 WeaponMagicConfig、WeaponDartConfig，并更新 EquipConfig.equips。");
    }

    private static void EnsureFolder(string folder)
    {
        if (AssetDatabase.IsValidFolder(folder)) return;

        const string parent = "Assets/Resources/Config";
        string folderName = "WeaponConfig";
        if (!AssetDatabase.IsValidFolder(parent))
        {
            Debug.LogError($"[DragonFall] 缺少目录: {parent}");
            return;
        }

        AssetDatabase.CreateFolder(parent, folderName);
    }

    private static WeaponConfig CreateOrUpdate(string assetPath, System.Func<WeaponConfig> build)
    {
        WeaponConfig existing = AssetDatabase.LoadAssetAtPath<WeaponConfig>(assetPath);
        if (existing != null)
        {
            ApplyConfig(build(), existing);
            EditorUtility.SetDirty(existing);
            return existing;
        }

        WeaponConfig created = build();
        created.name = System.IO.Path.GetFileNameWithoutExtension(assetPath);
        AssetDatabase.CreateAsset(created, assetPath);
        return created;
    }

    private static void ApplyConfig(WeaponConfig source, WeaponConfig target)
    {
        target.id = source.id;
        target.className = source.className;
        target.weaponName = source.weaponName;
        target.iconSprite = source.iconSprite;
        target.bulletPrefab = source.bulletPrefab;
        target.levels = source.levels;
    }

    private static WeaponConfig BuildMagicConfig()
    {
        WeaponConfig         config = ScriptableObject.CreateInstance<WeaponConfig>();
        config.id = 3;
        config.className = "Weapon_Magic";
        config.weaponName = "Weapon_Magic";
        config.iconSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Sprite/Icon/Weapon_Magic_Icon.png");
        config.bulletPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/Prefab/Equip/Weapon_Magic_Bullet.prefab");
        config.levels = new List<WeaponLevelData>
        {
            Level(1.2f, 1, 6f, 3f, 8f, 8f, "魔法弹：自动锁定最近敌人"),
            Level(1.1f, 1, 6f, 3f, 8f, 12f, "伤害提升"),
            Level(1.1f, 2, 6f, 3f, 15f, 12f, "双发魔法弹"),
            Level(1f, 2, 7f, 3f, 15f, 16f, "射速提升"),
            Level(0.9f, 2, 7f, 3.5f, 15f, 20f, "攻击频率提升"),
            Level(0.9f, 3, 7f, 3.5f, 12f, 20f, "三发魔法弹"),
            Level(0.8f, 3, 8f, 4f, 12f, 26f, "伤害大幅提升"),
            Level(0.7f, 3, 8f, 4f, 12f, 32f, "魔法弹极限"),
        };
        return config;
    }

    private static WeaponConfig BuildDartConfig()
    {
        WeaponConfig         config = ScriptableObject.CreateInstance<WeaponConfig>();
        config.id = 4;
        config.className = "Weapon_Dart";
        config.weaponName = "Weapon_Dart";
        config.iconSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Sprite/Icon/Weapon_Dart_Icon.png");
        config.bulletPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/Prefab/Equip/Weapon_Dart_Bullet.prefab");
        config.levels = new List<WeaponLevelData>
        {
            Level(1.5f, 1, 8f, 2.5f, 8f, 12f, "飞镖：朝移动方向发射"),
            Level(1.4f, 1, 8f, 2.5f, 8f, 16f, "伤害提升"),
            Level(1.3f, 2, 8f, 2.5f, 12f, 16f, "双发飞镖"),
            Level(1.2f, 2, 9f, 3f, 12f, 20f, "飞行速度提升"),
            Level(1.1f, 2, 9f, 3f, 12f, 24f, "攻击力提升"),
            Level(1f, 3, 9f, 3f, 10f, 24f, "三发飞镖"),
            Level(1f, 3, 10f, 3.5f, 10f, 30f, "穿透力提升"),
            Level(0.9f, 3, 10f, 3.5f, 10f, 38f, "飞镖极限"),
        };
        return config;
    }

    private static WeaponLevelData Level(
        float attackRate,
        int barrageCount,
        float barrageSpeed,
        float barrageDuration,
        float attackRange,
        float strength,
        string description)
    {
        return new WeaponLevelData
        {
            AttackRate = attackRate,
            BarrageCount = barrageCount,
            BarrageSpeed = barrageSpeed,
            BarrageDuration = barrageDuration,
            AttackRange = attackRange,
            Strength = strength,
            description = description,
        };
    }
}
#endif
