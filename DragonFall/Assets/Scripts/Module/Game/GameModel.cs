// 游戏运行时数据模型
using System;

public class GameModel
{
    // 已击杀敌人数量
    public int KillEnemyCount { get; private set; }
    // 本局游戏开始时间
    public float StartGameTime { get; private set; }
    // 本局游戏开始时的 Unix 毫秒时间戳
    public long StartTime { get; private set; }
    // 玩家当前等级
    public int Level { get; private set; }
    // 当前等级已获得经验
    public int Exp { get; private set; }
    // 升到下一级所需经验
    public int LevelUpExp { get; private set; }

    // UI 刷新标记
    public bool KillEnemyCountChanged { get; set; }
    public bool ExpChanged { get; set; }
    public bool LevelChanged { get; set; }

    // 玩家属性数据 生命上限，回复，吸血，防御，移动速度
    public float MaxHp, Heal, Vampire, Defense, MoveSpeed;

    // 玩家技能数据 力量，弹幕速度，弹幕持续时间，攻击范围
    public float Strength, BarrageSpeed, BarrageDuration, AttackRange;

    // 技能冷却时间，弹幕数量，复活次数，心灵传动
    public float BarrageCD, BarrageCount, ReliveNumber, Telekinesis;

    // 幸运，成长，贪婪，诅咒
    public float Luck, Growth, Greed, Curse;

    // 重选，跳过，排除
    public float Reselect, Skip, Exclude;


    public Action LevelUpCallBack;
    // 初始化一局游戏的默认数据
    public GameModel()
    {
        KillEnemyCount = 0;
        StartGameTime = 0f;
        StartTime = 0L;
        Level = 1;
        Exp = 0;
        LevelUpExp = GameConst.BaseLevelUpExp;
        KillEnemyCountChanged = false;
        ExpChanged = false;
        LevelChanged = false;
    }

    // 增加击杀数量
    public void AddKillEnemyCount()
    {
        KillEnemyCount++;
        KillEnemyCountChanged = true;
    }

    // 增加经验，并处理连续升级
    public void AddExp(int exp)
    {
        if (exp <= 0) return;

        Exp += exp;
        ExpChanged = true;
        while (Exp >= LevelUpExp)
        {
            Exp -= LevelUpExp;
            Level++;
            LevelUpExp += GameConst.BaseLevelUpExp;
            LevelChanged = true;

            LevelUpCallBack?.Invoke();
        }
    }

    // 重置本局统计和成长数据
    public void ResetData()
    {
        KillEnemyCount = 0;
        Level = 1;
        Exp = 0;
        LevelUpExp = GameConst.BaseLevelUpExp;
        KillEnemyCountChanged = true;
        ExpChanged = true;
        LevelChanged = true;
    }

    // 记录本局开始时间，用于计算游戏时长
    public void SetStartGameTime(float startGameTime)
    {
        StartGameTime = startGameTime;
    }

    public void SetStartTime(long startTime)
    {
        StartTime = startTime;
    }
}
