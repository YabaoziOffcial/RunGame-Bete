using System;

/// <summary>
/// 版本化存档模块基类（可选使用）
/// 
/// 用法：
/// - 让你的数据类继承它，并实现 ModuleKey / CurrentSchemaVersion
/// - 需要迁移时实现 UpgradeFrom(fromVersion)
/// - 业务层通过 GameDataManager.GetData&lt;T&gt;(key) 获取并使用
/// 
/// 注意：
/// - 实际的 Load/Save 由 GameDataManager 统一调度并通过 ES3 持久化
/// - 这里的 Load/Save 默认实现为 no-op，避免业务侧重复写存档逻辑
/// </summary>
[Serializable]
public abstract class VersionedGameDataBase : IVersionedGameData
{
    public bool isValid { get; set; }

    public abstract string ModuleKey { get; }
    public abstract int CurrentSchemaVersion { get; }

    public virtual void UpgradeFrom(int fromSchemaVersion) { }
    public virtual void OnAfterLoaded() { }
    public virtual void OnBeforeSave() { }

    public virtual void OnRemove() { }

}

