using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestView : ViewBase
{

    [SerializeField] Text m_TestText;
    [SerializeField] InputField m_TestInputField;
    [SerializeField] Button m_TestButton;

    private void Awake()
    {
        m_TestButton.onClick.AddListener(OnTestButtonClick);
    }

    private void OnTestButtonClick()
    {
        TestController.Instance.SetTestString(m_TestInputField.text);
        m_TestText.text = TestController.Instance.Model.TestData.TestDataValue;
    }

    public override void Show()
    {
        base.Show();
        Debug.Log("TestView Show");
        
        m_TestText.text = TestController.Instance.Model.TestData.TestDataValue;
    }

    public override void Close()
    {
        base.Close();
        Debug.Log("TestView Close");
    }
}
