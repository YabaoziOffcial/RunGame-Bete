using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestView : ViewBase
{
    public override void Show()
    {
        base.Show();
        Debug.Log("TestView Show");
    }

    public override void Close()
    {
        base.Close();
        Debug.Log("TestView Close");
    }
}
