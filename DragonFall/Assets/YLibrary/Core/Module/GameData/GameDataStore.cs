using System;
using UnityEngine;

/// <summary>
/// ES3 存档读写封装：用一个文件保存多个模块（Key 分区），并支持批量提交。
/// 
/// 为什么要封装：
/// - ES3 支持按 key 读写，但业务代码不应到处散落 ES3.Save/Load
/// - 用 ES3File 做缓存，最后一次性 Sync()，减少 IO 次数
/// </summary>
/// 
/// sealed 防止被继承，导致无法控制存档的读写
public sealed class GameDataStore
{
    public const string DefaultFileName = "GameData.es3"; // 默认文件名, 可以通过这个更换这个实现本地 不同账号的切换

    private readonly ES3Settings settings;
    private readonly ES3File file;

    public GameDataStore(string fileName = DefaultFileName, ES3Settings overrideSettings = null)
    {
        // 统一使用持久化目录
        var s = overrideSettings != null ? (ES3Settings)overrideSettings.Clone() : new ES3Settings();
        s.location = ES3.Location.File;
        s.directory = ES3.Directory.PersistentDataPath;
        s.path = string.IsNullOrEmpty(fileName) ? DefaultFileName : fileName;
        // JSON 更利于调试（也更利于以后做云同步/排障）
        s.format = ES3.Format.JSON;
        s.prettyPrint = true;
        settings = s;

        // syncWithFile=true：构建时就把文件内容读到 cache，后续 Save/Load 都走内存缓存
        file = new ES3File(settings, syncWithFile: true);
    }

    public ES3Settings Settings => settings;

    public bool KeyExists(string key) => file.KeyExists(key);

    public void DeleteKey(string key)
    {
        if (!file.KeyExists(key)) return;
        file.DeleteKey(key);
    }

    public T Load<T>(string key, T defaultValue = default)
    {
        return file.Load(key, defaultValue);
    }

    public void LoadInto<T>(string key, T obj) where T : class
    {
        file.LoadInto(key, obj);
    }

    public void Save<T>(string key, T value)
    {
        // 统一写入 ES3File 缓存，配合 Commit() 一次性落盘，避免覆盖/回滚问题
        file.Save(key, value);
    }

    /// <summary>把缓存写入磁盘（一次性提交，减少 IO）</summary>
    public void Commit()
    {
        file.Sync(settings);
    }

    // -------------------- Key helpers --------------------

    public static string ModuleSchemaKey(string moduleKey) => $"module/{moduleKey}/schemaVersion";
    public static string ModuleDataKey(string moduleKey) => $"module/{moduleKey}/data";

    public static string MakeSafeModuleKey(string moduleKey)
    {
        if (string.IsNullOrEmpty(moduleKey)) return "unknown";
        // 防止 key 中出现路径分隔导致混乱
        return moduleKey.Replace("\\", "_").Replace("/", "_").Trim();
    }
}

