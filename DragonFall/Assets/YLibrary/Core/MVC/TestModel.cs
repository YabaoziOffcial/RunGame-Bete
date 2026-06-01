using System;

// 模型层
public class TestModel
{
    private readonly TestData m_TestData;
    public TestData TestData => m_TestData;

    public TestModel()
    {
        // 
        m_TestData = GameDataManager.Instance.GetData<TestData>(TextConst.TestData);
    }
}

/// <summary>
/// MVC 示例存档模块：走 GameDataManager 版本化 Load/Save，不再直接 ES3.Save。
/// </summary>
[Serializable]
public class TestData : DataBase, IVersionedGameData
{
    public static class Keys
    {
        public const string TestDataKey = "TestDataKey";
    }

    public string ModuleKey => TextConst.TestData;
    public int CurrentSchemaVersion => 1;

    public string TestDataValue
    {
        get => GetValue<string>(Keys.TestDataKey, "");
        set => SetValue(Keys.TestDataKey, value);
    }

    public void UpgradeFrom(int fromSchemaVersion) { }

    public void OnAfterLoaded() { }

    public void OnBeforeSave() { }

    public override bool Save()
    {
        GameDataManager.Instance.SaveData(TextConst.TestData, this);
        GameDataManager.Instance.Store.Commit();
        return true;
    }
}
