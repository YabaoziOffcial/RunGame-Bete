using System;
using UnityEngine;

// 与 PlayerStats 同名的属性修正（装备/道具等级配置用，默认 0 表示不加成）
[Serializable]
public class PlayerStatModifiers
{
    [Header("最大生命")]
    [Tooltip("增加玩家 MaxHp（获得时同步增加当前生命）")]
    public float MaxHp;

    [Header("治疗")]
    [Tooltip("生命恢复相关加成")]
    public float Heal;

    [Header("吸血")]
    public float Vampire;

    [Header("防御")]
    public float Defense;

    [Header("移动速度")]
    public float MoveSpeed;

    [Header("力量/伤害")]
    [Tooltip("武器单发伤害；非 0 时写入配置")]
    public float Strength;

    [Header("攻击间隔(秒)")]
    [Tooltip("开火冷却，数值越小射速越快")]
    public float AttackRate;

    [Header("弹幕速度")]
    public float BarrageSpeed;

    [Header("弹幕持续时间")]
    public float BarrageDuration;

    [Header("攻击范围/扩散角")]
    [Tooltip("多发武器的角度间隔（度）")]
    public float AttackRange;

    [Header("弹幕冷却")]
    public float BarrageCD;

    [Header("弹幕数量")]
    public float BarrageCount;

    [Header("复活次数")]
    public float ReliveNumber;

    [Header("拾取范围")]
    public float PickupRange;

    [Header("幸运")]
    public float Luck;

    [Header("成长")]
    public float Growth;

    [Header("贪婪")]
    [Tooltip("经验/掉落相关")]
    public float Greed;

    [Header("诅咒")]
    public float Curse;

    [Header("重选次数")]
    public float Reselect;

    [Header("跳过次数")]
    public float Skip;

    [Header("排除次数")]
    public float Exclude;

    public static PlayerStatModifiers Zero => new PlayerStatModifiers();

    public PlayerStatModifiers Clone()
    {
        return (PlayerStatModifiers)MemberwiseClone();
    }

    // 叠加或扣除玩家成长类修正（sign 为 +1 或 -1）
    public void ApplyPlayerBonusesTo(PlayerStats stats, int sign)
    {
        if (stats == null || sign == 0) return;

        float s = sign;

        if (MaxHp != 0f)
        {
            stats.AddMaxHp(MaxHp * s);
            stats.AddCurrentHp(MaxHp * s);
        }

        stats.AddHeal(Heal * s);
        stats.AddVampire(Vampire * s);
        stats.AddDefense(Defense * s);
        stats.AddMoveSpeed(MoveSpeed * s);
        stats.AddBarrageCD(BarrageCD * s);
        stats.AddReliveNumber(ReliveNumber * s);
        stats.AddPickupRange(PickupRange * s);
        stats.AddLuck(Luck * s);
        stats.AddGrowth(Growth * s);
        stats.AddGreed(Greed * s);
        stats.AddCurse(Curse * s);
        stats.AddReselect(Reselect * s);
        stats.AddSkip(Skip * s);
        stats.AddExclude(Exclude * s);
    }

    // 武器战斗字段（Strength / AttackRate 等）由 WeaponLevelData 配置、武器脚本读取，不写入全局 Stats
}
