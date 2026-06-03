using UnityEngine;

/// <summary>本局玩家战斗/成长属性，由 GameModel.Stats 持有；实体与装备只读/经 Controller 修改。</summary>
public class PlayerStats
{
    public float CurrentHp { get; private set; }
    public float MaxHp { get; private set; }
    public float Heal { get; private set; }
    public float Vampire { get; private set; }
    public float Defense { get; private set; }
    public float MoveSpeed { get; private set; }

    public float Strength { get; private set; }
    public float AttackRate { get; private set; }
    public float BarrageSpeed { get; private set; }
    public float BarrageDuration { get; private set; }
    public float AttackRange { get; private set; }

    public float BarrageCD { get; private set; }
    public float BarrageCount { get; private set; }
    public float ReliveNumber { get; private set; }
    public float PickupRange { get; private set; }

    public float Luck { get; private set; }
    public float Growth { get; private set; }
    /// <summary>经验/道具掉落相关（原 PlayerData.ExDropRate）。</summary>
    public float Greed { get; private set; }
    public float Curse { get; private set; }

    public float Reselect { get; private set; }
    public float Skip { get; private set; }
    public float Exclude { get; private set; }

    public bool IsAlive => CurrentHp > 0f;

    /// <summary>新局或 BindPlayer 时恢复默认数值。</summary>
    public void ResetToDefault()
    {
        MaxHp = 100f;
        CurrentHp = MaxHp;
        MoveSpeed = 1f;
        Strength = 10f;
        AttackRate = 1f;
        Defense = 10f;
        Greed = 0.3f;

        Heal = 0f;
        Vampire = 0f;
        BarrageSpeed = 0f;
        BarrageDuration = 0f;
        AttackRange = 0f;
        BarrageCD = 0f;
        BarrageCount = 0f;
        ReliveNumber = 0f;
        PickupRange = 0f;
        Luck = 0f;
        Growth = 0f;
        Curse = 0f;
        Reselect = 0f;
        Skip = 0f;
        Exclude = 0f;
    }

    /// <summary>扣血并钳制到 0；不负责 GameOver（由 GameController 判断）。</summary>
    public void ApplyDamage(float damage)
    {
        if (damage <= 0f) return;
        CurrentHp = Mathf.Max(0f, CurrentHp - damage);
    }

    public void SetMaxHp(float value) => MaxHp = value;
    public void SetCurrentHp(float value) => CurrentHp = value;
    public void SetHeal(float value) => Heal = value;
    public void SetVampire(float value) => Vampire = value;
    public void SetDefense(float value) => Defense = value;
    public void SetMoveSpeed(float value) => MoveSpeed = value;
    public void SetStrength(float value) => Strength = value;
    public void SetAttackRate(float value) => AttackRate = value;
    public void SetBarrageSpeed(float value) => BarrageSpeed = value;
    public void SetBarrageDuration(float value) => BarrageDuration = value;
    public void SetAttackRange(float value) => AttackRange = value;
    public void SetBarrageCD(float value) => BarrageCD = value;
    public void SetBarrageCount(float value) => BarrageCount = value;
    public void SetReliveNumber(float value) => ReliveNumber = value;
    public void SetPickupRange(float value) => PickupRange = value;
    public void SetLuck(float value) => Luck = value;
    public void SetGrowth(float value) => Growth = value;
    public void SetGreed(float value) => Greed = value;
    public void SetCurse(float value) => Curse = value;
    public void SetReselect(float value) => Reselect = value;
    public void SetSkip(float value) => Skip = value;
    public void SetExclude(float value) => Exclude = value;

    public void AddMaxHp(float delta) => MaxHp += delta;
    public void AddCurrentHp(float delta) => CurrentHp = Mathf.Max(0f, CurrentHp + delta);
    public void AddHeal(float delta) => Heal += delta;
    public void AddVampire(float delta) => Vampire += delta;
    public void AddDefense(float delta) => Defense += delta;
    public void AddMoveSpeed(float delta) => MoveSpeed += delta;
    public void AddStrength(float delta) => Strength += delta;
    public void AddAttackRate(float delta) => AttackRate += delta;
    public void AddBarrageSpeed(float delta) => BarrageSpeed += delta;
    public void AddBarrageDuration(float delta) => BarrageDuration += delta;
    public void AddAttackRange(float delta) => AttackRange += delta;
    public void AddBarrageCD(float delta) => BarrageCD += delta;
    public void AddBarrageCount(float delta) => BarrageCount += delta;
    public void AddReliveNumber(float delta) => ReliveNumber += delta;
    public void AddPickupRange(float delta) => PickupRange += delta;
    public void AddLuck(float delta) => Luck += delta;
    public void AddGrowth(float delta) => Growth += delta;
    public void AddGreed(float delta) => Greed += delta;
    public void AddCurse(float delta) => Curse += delta;
    public void AddReselect(float delta) => Reselect += delta;
    public void AddSkip(float delta) => Skip += delta;
    public void AddExclude(float delta) => Exclude += delta;

    // 应用装备/道具对玩家的成长类属性修正
    public void AddModifiers(PlayerStatModifiers modifiers) => modifiers?.ApplyPlayerBonusesTo(this, 1);

    // 移除已应用的成长类属性修正
    public void RemoveModifiers(PlayerStatModifiers modifiers) => modifiers?.ApplyPlayerBonusesTo(this, -1);
}
