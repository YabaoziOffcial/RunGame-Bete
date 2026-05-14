using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YBZ.Design;

/// <summary>
/// ps:
///     窗口作为子集存在,窗口同一时间只能有一个
/// </summary>
[RequireComponent(typeof(GraphicRaycast))]
public class UIManager : O_MonoSingleton<UIManager>
{
    #region Transform: canvas, Panel, PopUp, Other
    public Transform canvasTransform;
    public Transform canvasWorldTransform;
    private Transform m_PanelTransform;
    public Transform PanelTransform
    {
        get
        {
            m_PanelTransform = m_PanelTransform != null ? m_PanelTransform : canvasTransform.transform.Find("Panel");
            return m_PanelTransform;
        }
    }

    private Transform m_PopUpTransform;
    public Transform PopUpTransform
    {
        get
        {
            m_PopUpTransform = m_PopUpTransform != null ? m_PopUpTransform : canvasTransform.transform.Find("PopUp");
            return m_PopUpTransform;
        }
    }

    private Transform m_OtherTransform;
    public Transform OtherTransform
    {
        get
        {
            m_OtherTransform = m_OtherTransform != null ? m_OtherTransform : canvasTransform.transform.Find("Other");
            return m_OtherTransform;
        }
    }

    /// <summary>
    /// 快速生成一个
    /// </summary>
    [InspectorButton]
    public void InitTransform()
    {
        if (PanelTransform == null)
        {
            GameObject winGO = new("Panel");
            winGO.transform.SetParent(canvasTransform);
            winGO.transform.localPosition = Vector3.zero;
            winGO.transform.localScale = Vector3.one;
            winGO.AddComponent<RectTransform>();
        }

        if (PopUpTransform == null)
        {
            GameObject popGO = new("PopUp");
            popGO.transform.SetParent(canvasTransform);
            popGO.transform.localPosition = Vector3.zero;
            popGO.transform.localScale = Vector3.one;
            popGO.AddComponent<RectTransform>();
        }

        if (OtherTransform == null)
        {
            GameObject otherGO = new("Other");
            otherGO.transform.SetParent(canvasTransform);
            otherGO.transform.localPosition = Vector3.zero;
            otherGO.transform.localScale = Vector3.one;
            otherGO.AddComponent<RectTransform>();
        }
    }

    // 注册面板
    public void Register()
    {
        m_PanelTransform = null;
        m_PopUpTransform = null;
        m_OtherTransform = null;
    }
    #endregion

    protected override void Initialize()
    {
        Register();
        UICachas = new Dictionary<Type, ViewBase>();
    }

    private const string UI_ResourcePath = "Prefab/UI/";    // UI预制体资源路径

    public Dictionary<Type, ViewBase> UICachas;

    private ViewBase currentPanel;
    public ViewBase CurrentPanel
    {
        get => currentPanel;
        set
        {
            currentPanel = value;
        }
    }

    // 打开UI 并且返回UI实例
    public T OpenUI<T>() where T : ViewBase
    {
        var type = typeof(T);
        bool isPanel = typeof(Y_PanelBase).IsAssignableFrom(typeof(T)); // 检查传入的类型是否可以分配在这个位置（是否是派生类）
        if (!UICachas.TryGetValue(type, out var viewBase) || viewBase == null)
        {
            string path = UI_ResourcePath + type.ToString();
            var go = ResourceManager.Instance.LoadRes<GameObject>(path);
            viewBase = Instantiate(go, isPanel ? PanelTransform : PopUpTransform).GetComponent<ViewBase>();
            UICachas[type] = viewBase;
        }
        viewBase.transform.localPosition = Vector3.zero;
        viewBase.name = viewBase.name.Replace("(Clone)", "");
        if (isPanel)    // 用于维护唯一一个面板
        {
            if (CurrentPanel) CurrentPanel.Close();
            CurrentPanel = viewBase;
            Debug.Log("当前的面板是 : " + currentPanel.name);
        }
        viewBase.transform.SetAsLastSibling(); // 将UI放在最上层
        viewBase.Show();
        return viewBase as T;
    }

    public T CloseUI<T>() where T : ViewBase
    {
        var type = typeof(T);
        if (UICachas.TryGetValue(type, out var viewBase))
        {
            viewBase.Close();
            if (CurrentPanel != null && CurrentPanel == viewBase) CurrentPanel = null;
        }
        return viewBase as T;
    }

    // 删除UI缓存
    public void RemoveUI<T>() where T : ViewBase
    {
        var type = typeof(T);
        if (UICachas.TryGetValue(type, out var _))
        {
            UICachas.Remove(type);
        }
    }
}

internal class GraphicRaycast
{
}