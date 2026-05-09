using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 模型层
public class TestModel
{
    public string TestText { get; set; }

    public TestModel()
    {
        // TestText = TextConst.Instance.textConst.TEST_TEXT;
    }
}

public class TestData : DataBase
{
    public static class Keys
    {
        public const string TestDataKey = "TestData";

    }

    public static readonly Dictionary<string, object> ValueType = new()
    {
        { Keys.TestDataKey, typeof(string) },
    };
    public string TestDataValue { get => GetValue<string>(Keys.TestDataKey, ""); set => SetValue(Keys.TestDataKey, value); }
    public override bool Save()
    {
        ES3.Save("TestData", this);
        return true;
    }
}