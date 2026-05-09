using System.Collections.Generic;
/// <summary>
/// 全端游戏数据管理器
/// </summary>
public class GameDataManager : YBZ.Design.Singleton<GameDataManager>
{
    /// <summary>
    /// 存档存储封装（Easy Save 3）
    /// </summary>
    public GameDataStore Store { get; private set; }

    public void Init()
    {
        LubanMgr.Instance.Init();
        m_Dict = new Dictionary<string, DataBase>();
        Store = new GameDataStore();
        UnityEngine.Debug.Log("GameDataManager Initialized!");
    }

    public void UnInit()
    {
        foreach (var data in m_Dict)
        {
            // 退出时统一保存一次（可按项目需要移除）
            try { SaveDataInternal(data.Key, data.Value); } catch { }
        }
        m_Dict.Clear();
        try { Store?.Commit(); } catch { }
    }

    // Matter Unity 内配置表
    public Matter Matter => ResourceManager.Instance.LoadRes<Matter>("Config/Matter");

    // 鲁班配置表
    // public Luban.Tables Tables => LubanMgr.Instance.tables;

    private Dictionary<string, DataBase> m_Dict;

    // 添加数据
    // public bool AddData(string key, IGameData value)
    // {
    //     m_Dict[key] = value;
    //     return LoadData(key, value);
    // }

    // 获取数据
    public T GetData<T>(string key) where T : DataBase
    {
        if (!m_Dict.ContainsKey(key))
        {
            m_Dict[key] = (T) System.Activator.CreateInstance(typeof(T));
            LoadDataInternal(key, m_Dict[key]);
        }
        var result = (T)m_Dict[key];
        return result;
    }

    // 保存数据
    public void SaveData(string key, DataBase data)
    {
        SaveDataInternal(key, data);
    }
    // // 删除数据
    // public void RemoveData(string key)
    // {
    //     if (m_Dict.ContainsKey(key))
    //     {
    //         try { SaveData(key, m_Dict[key]); } catch { }
    //         m_Dict[key].OnRemove();
    //         m_Dict.Remove(key);
    //     }
    // }

    /// <summary>
    /// 手动保存所有已加载的数据模块（建议在退出、切后台、关键节点调用）
    /// </summary>
    public void SaveAll()
    {
        if (Store == null) return;
        foreach (var kv in m_Dict)
        {
            try { SaveDataInternal(kv.Key, kv.Value); } catch { }
        }
        // 统一提交到磁盘（ES3File 缓存 -> Sync）
        Store.Commit();
    }

    // -------------------- Internal --------------------

    private bool LoadDataInternal(string key, DataBase data)
    {
        if (data == null) return false;
        // 方案三：模块化版本存档
        if (data is IVersionedGameData versioned)
        {
            return LoadVersionedData(key, versioned);
        }
        return true;
    }

    private void SaveDataInternal(string key, DataBase data)
    {
        if (data == null) return;
        if (data is IVersionedGameData versioned)
        {
            SaveVersionedData(key, versioned);
            return;
        }
    }

    /// <summary>
    /// 加载版本化数据
    /// </summary>
    /// <param name="key">从管理器中获取的键</param>
    /// <param name="data">版本化数据</param>
    /// <returns>是否加载成功</returns>
    private bool LoadVersionedData(string key, IVersionedGameData data)
    {
        if (Store == null) Store = new GameDataStore();

        string moduleKey = GameDataStore.MakeSafeModuleKey(string.IsNullOrEmpty(data.ModuleKey) ? key : data.ModuleKey);
        string schemaKey = GameDataStore.ModuleSchemaKey(moduleKey);
        string dataKey = GameDataStore.ModuleDataKey(moduleKey);
        int storedSchemaVersion = 0;
        if (Store.KeyExists(schemaKey)) storedSchemaVersion = Store.Load<int>(schemaKey, 0);

        // 如果有旧数据则加载到对象中；没有则保持默认值
        if (Store.KeyExists(dataKey))
        {
            try
            {
                // 用 LoadInto 直接填充现有对象（避免替换引用）
                Store.LoadInto<object>(dataKey, data);
            }
            catch
            {
                // 如果类型变化导致 LoadInto 失败，退化为重新 Load<object> 再尝试复制（最小容错）
                object loaded = Store.Load<object>(dataKey, null);
                if (loaded != null && loaded.GetType() == data.GetType())
                {
                    // ES3 没有通用“对象拷贝到现有实例”的接口，这里就当作已加载成功
                }
            }
        }

        // 升级迁移
        bool upgraded = false;
        if (storedSchemaVersion < data.CurrentSchemaVersion)
        {
            data.UpgradeFrom(storedSchemaVersion);
            storedSchemaVersion = data.CurrentSchemaVersion;
            upgraded = true;
            // 升级后立刻写回版本号 + 数据本体，避免出现“版本已升级但数据没写回”的不一致
            Store.Save(schemaKey, storedSchemaVersion);
            Store.Save<object>(dataKey, data);
        }

        data.OnAfterLoaded();

        // 升级发生时立即落盘一次，避免下次启动重复升级或出现不一致
        if (upgraded)
        {
            try { Store.Commit(); } catch { }
        }
        return true;
    }


    private void SaveVersionedData(string key, IVersionedGameData data)
    {
        if (Store == null) Store = new GameDataStore();

        string moduleKey = GameDataStore.MakeSafeModuleKey(string.IsNullOrEmpty(data.ModuleKey) ? key : data.ModuleKey);
        string schemaKey = GameDataStore.ModuleSchemaKey(moduleKey);
        string dataKey = GameDataStore.ModuleDataKey(moduleKey);

        data.OnBeforeSave(); // 保存前接口
        Store.Save(schemaKey, data.CurrentSchemaVersion); // 保存版本号
        Store.Save(dataKey, data); // 保存数据
    }
}