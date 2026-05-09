using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

/// <summary>
/// UI面板的基础类
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class Y_PanelBase : ViewBase
{
    public Transform allUI;
    [SerializeField]
    protected CanvasGroup m_CanvasGroup;    // 实际上这个也是非必要的

    public Animator animator;
    public override void Show()
    {
        Load();
        base.Show();
        ShowAnima();
    }

    protected virtual void ShowAnima()
    {
        gameObject.SetActive(true);
    }

    public override void Close() 
    {
        UnLoad();
        base.Close();
        HideAnima();
    }
    protected virtual void HideAnima()
    {
        gameObject.SetActive(false);
    }

#pragma warning disable 0114    // 忽略警告
    /// <summary>
    /// 检视
    /// </summary>
    /// 
    protected void OnValidate()
    {
        m_CanvasGroup ??= GetComponent<CanvasGroup>();
        if (!allUI)
        {
            if (transform.Find("AllUI"))
            {
                allUI = transform.Find("AllUI");
            }
            else
            {
                allUI = this.transform;
            }
        }
        animator ??= GetComponent<Animator>();
    }

#pragma warning restore 0114
}