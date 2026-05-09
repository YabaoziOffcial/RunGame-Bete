using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 子物体数据类
public class ListItemData
{
    public ItemType itemType;
    public string title;
    public string description;
    public Sprite image;
    
    public ListItemData(ItemType type, string title, string desc, Sprite image = null)
    {
        this.itemType = type;
        this.title = title;
        this.description = desc;
        this.image = image;
    }
}

// 子物体类型枚举
public enum ItemType
{
    TypeA,
    TypeB,
    TypeC
}

public class RecyclableListItem : MonoBehaviour
{
    public int Index { get; set; }

    // 设置数据的抽象方法，由子类实现
    public virtual void SetData(ListItemData data)
    {
        // 子类应实现此方法来更新UI
    }
}