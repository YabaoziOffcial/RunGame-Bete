using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 控制器层
public class TestController : YBZ.Design.Singleton<TestController>
{
    private TestModel m_Model;
    public TestModel Model => m_Model;

    public void Init()
    {
        m_Model = new TestModel();
    }

    public void UnInit()
    {
        m_Model = null;
    }
}
