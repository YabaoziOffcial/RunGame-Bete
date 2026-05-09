/// <summary>
/// 版本化的模块化存档接口（方案三：分域存档 + 各模块独立版本与迁移）
/// 
/// 设计目标：
/// - 每个模块自己维护 schemaVersion
/// - 新版本读取旧存档时，能在 Upgrade(fromVersion) 中补字段/修数据
/// - GameDataManager 负责统一调用 Load/Save，不需要业务侧到处写 ES3
/// </summary>
public interface IVersionedGameData : IGameData
{
    /// <summary>模块唯一 Key（建议稳定不变，用于存档定位）</summary>
    public string ModuleKey { get; }

    /// <summary>当前数据结构版本号（每次结构变化就 +1）</summary>
    public int CurrentSchemaVersion { get; }

    /// <summary>
    /// 当检测到存档版本小于 CurrentSchemaVersion 时，会调用此方法进行升级迁移。
    /// 注意：只做“向前迁移”，不做回退。
    /// </summary>A
    public void UpgradeFrom(int fromSchemaVersion);

    /// <summary>可选：加载完成（含升级）后的回调</summary>
    public void OnAfterLoaded();

    /// <summary>可选：保存前回调（做数据清理/校验）</summary>
    public void OnBeforeSave();
}

public interface IGameData
{
    
}