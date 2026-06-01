using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 控制器层
public class TestController : YBZ.Design.Singleton<TestController>
{
    private TestModel m_Model;
    public TestModel Model => m_Model;


    private bool isTestDataChanged = false;
    public void Init()
    {
        m_Model = new TestModel();
    }

    public void UnInit()
    {
        m_Model = null;
    }

    public void Update()
    {
        FlushPendingSave();
    }

    /// <summary>立即写入尚未落盘的修改（退出/切后台前调用）。</summary>
    public void FlushPendingSave()
    {
        if (!isTestDataChanged || m_Model == null) return;
        isTestDataChanged = false;
        m_Model.TestData.Save();
    }

    public void OpenTestView()
    {
        UIManager.Instance.OpenUI<TestView>();
    }

    public void SetTestString(string testString)
    {
        m_Model.TestData.TestDataValue = testString;
        isTestDataChanged = true;
    }
}
