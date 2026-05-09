using System;
using UnityEngine;

/// <summary>
/// 子类应该集成这个
/// this item on demand.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class RecyclingListViewItem : MonoBehaviour
{
    private RecyclingListView parentList;
    public RecyclingListView ParentList
    {
        get => parentList;
    }

    private int currentRow; // 当前所处的位置
    public int CurrentRow
    {
        get => currentRow;
    }

    private RectTransform rectTransform;
    public RectTransform RectTransform
    {
        get
        {
            if(rectTransform == null)
                rectTransform = GetComponent<RectTransform>();
            return rectTransform;
        }
    }

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    // 通知当前任务
    public void NotifyCurrentAssignment(RecyclingListView v, int row)
    {
        parentList = v;
        currentRow = row;
    }

    public virtual float GetPerferenceHeight()
    {
        return 0;
    }
}
