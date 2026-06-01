// 游戏运行时数据模型：局进度 + PlayerStats，不含 UI 回调。

/// <summary>经验/等级 UI 刷新用只读快照（由 Controller 经 EventManager 传给 View）。</summary>
public readonly struct GameExpSnapshot
{
    public int Exp { get; }
    public int Level { get; }
    public int LevelUpExp { get; }

    public GameExpSnapshot(int exp, int level, int levelUpExp)
    {
        Exp = exp;
        Level = level;
        LevelUpExp = levelUpExp;
    }

    public static GameExpSnapshot FromModel(GameModel model)
    {
        return new GameExpSnapshot(model.Exp, model.Level, model.LevelUpExp);
    }
}

/// <summary>本局进度数据：击杀、经验等级、开局时间；不含 UI 回调。</summary>
public class GameModel
{
    public PlayerStats Stats { get; } = new PlayerStats();

    public int KillEnemyCount { get; private set; }
    public float StartGameTime { get; private set; }
    public long StartTime { get; private set; }
    public int Level { get; private set; }
    public int Exp { get; private set; }
    public int LevelUpExp { get; private set; }

    public GameModel()
    {
        ResetData();
    }

    /// <summary>击杀 +1（由 GameController.OnEnemyKilled 调用）。</summary>
    public void AddKillEnemyCount()
    {
        KillEnemyCount++;
    }

    /// <summary>增加经验并处理连升；返回本次升了几级。</summary>
    public int AddExp(int exp)
    {
        if (exp <= 0) return 0;

        Exp += exp;
        int levelsGained = 0;
        while (Exp >= LevelUpExp)
        {
            Exp -= LevelUpExp;
            Level++;
            LevelUpExp += GameConst.BaseLevelUpExp;
            levelsGained++;
        }

        return levelsGained;
    }

    /// <summary>重置局进度与 PlayerStats。</summary>
    public void ResetData()
    {
        KillEnemyCount = 0;
        StartGameTime = 0f;
        StartTime = 0L;
        Level = 1;
        Exp = 0;
        LevelUpExp = GameConst.BaseLevelUpExp;
        Stats.ResetToDefault();
    }

    public void SetStartGameTime(float startGameTime)
    {
        StartGameTime = startGameTime;
    }

    public void SetStartTime(long startTime)
    {
        StartTime = startTime;
    }
}
