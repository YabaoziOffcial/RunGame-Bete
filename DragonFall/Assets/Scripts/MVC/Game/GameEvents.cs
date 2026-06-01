// 局内 UI 事件统一入口：仅 Controller / EquipManager 发送，View 订阅。
// 底层仍使用 YLibrary EventManager，避免多套 event Action 并存。

/// <summary>经验/等级变化（刷新经验条、等级文本）。</summary>
public readonly struct PlayerProgressChangedEvent
{
    public GameExpSnapshot Snapshot { get; }

    public PlayerProgressChangedEvent(GameExpSnapshot snapshot)
    {
        Snapshot = snapshot;
    }
}

/// <summary>升级（在 PlayerProgressChanged 之外，额外驱动选技能等 UI）。</summary>
public readonly struct LevelUpEvent
{
    public GameExpSnapshot Snapshot { get; }

    public LevelUpEvent(GameExpSnapshot snapshot)
    {
        Snapshot = snapshot;
    }
}

/// <summary>击杀数变化。</summary>
public readonly struct KillCountChangedEvent
{
    public int KillCount { get; }

    public KillCountChangedEvent(int killCount)
    {
        KillCount = killCount;
    }
}

/// <summary>局内 UI 事件：仅 Controller / EquipManager 调用 Raise*，由 GameController 订阅并驱动 View。</summary>
public static class GameEvents
{
    /// <summary>EventManager 事件名，与 GameConst 旧常量兼容。</summary>
    public static class Id
    {
        public const string PlayerProgressChanged = "PlayerProgressChanged";
        public const string LevelUp = "LevelUp";
        public const string KillCountChanged = "KillCountChanged";
        public const string EquipListChanged = "EquipListChanged";
    }

    /// <summary>经验/等级变化（刷新经验条，不含选技能）。</summary>
    public static void RaisePlayerProgressChanged(GameModel model)
    {
        if (model == null) return;
        RaisePlayerProgressChanged(GameExpSnapshot.FromModel(model));
    }

    public static void RaisePlayerProgressChanged(GameExpSnapshot snapshot)
    {
        EventManager.SendEvent(Id.PlayerProgressChanged, new PlayerProgressChangedEvent(snapshot));
    }

    /// <summary>升级一次发一次；连升会连续触发，用于打开 SelectView。</summary>
    public static void RaiseLevelUp(GameModel model)
    {
        if (model == null) return;
        RaiseLevelUp(GameExpSnapshot.FromModel(model));
    }

    public static void RaiseLevelUp(GameExpSnapshot snapshot)
    {
        EventManager.SendEvent(Id.LevelUp, new LevelUpEvent(snapshot));
    }

    /// <summary>击杀数变化。</summary>
    public static void RaiseKillCountChanged(int killCount)
    {
        EventManager.SendEvent(Id.KillCountChanged, new KillCountChangedEvent(killCount));
    }

    /// <summary>装备增删/升级后刷新 HUD 装备栏。</summary>
    public static void RaiseEquipListChanged()
    {
        EventManager.SendEvent(Id.EquipListChanged);
    }

    /// <summary>从 EventManager 回调参数解析经验快照（兼容旧 GameExpSnapshot 直传）。</summary>
    public static bool TryGetPlayerProgressChanged(object[] args, out GameExpSnapshot snapshot)
    {
        snapshot = default;
        if (args == null || args.Length == 0) return false;
        if (args[0] is PlayerProgressChangedEvent e)
        {
            snapshot = e.Snapshot;
            return true;
        }
        if (args[0] is GameExpSnapshot direct)
        {
            snapshot = direct;
            return true;
        }
        return false;
    }

    /// <summary>从回调参数解析升级事件载荷。</summary>
    public static bool TryGetLevelUp(object[] args, out GameExpSnapshot snapshot)
    {
        snapshot = default;
        if (args == null || args.Length == 0) return false;
        if (args[0] is LevelUpEvent e)
        {
            snapshot = e.Snapshot;
            return true;
        }
        if (args[0] is GameExpSnapshot direct)
        {
            snapshot = direct;
            return true;
        }
        return false;
    }

    /// <summary>从回调参数解析击杀数。</summary>
    public static bool TryGetKillCount(object[] args, out int killCount)
    {
        killCount = 0;
        if (args == null || args.Length == 0) return false;
        if (args[0] is KillCountChangedEvent e)
        {
            killCount = e.KillCount;
            return true;
        }
        if (args[0] is int direct)
        {
            killCount = direct;
            return true;
        }
        return false;
    }
}
