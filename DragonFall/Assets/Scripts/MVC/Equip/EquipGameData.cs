using System;

// 装备在本局游戏中的运行时记录
[Serializable]
public class EquipGameData
{
    // 装备加入玩家装备栏时的 Unix 毫秒时间戳
    public long AddTimestamp { get; private set; }
    // 该装备本局累计造成的伤害
    public float totalDamage;
    // 当前等级已应用到 PlayerStats 的成长类修正（用于升级/卸下时回滚）
    public PlayerStatModifiers AppliedPlayerBonuses = new PlayerStatModifiers();

    public EquipGameData(long addTimestamp)
    {
        AddTimestamp = addTimestamp;
    }

    public void AddDamage(float damage)
    {
        if (damage <= 0f) return;
        totalDamage += damage;
    }
}
