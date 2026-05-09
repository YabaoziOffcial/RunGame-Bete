
// <summary>
// 一个事件管理者的框架如下，主要：
// ①定义了回调函数，用delegate声明；
// ②定义了存放信息的字典；
// ③定义了添加监听关系的函数Add()；
// ④定义了发送事件的的函数SendEvent():
// ⑤定义了删除关系的相关函数；
// </summary
// 
// 事件管理器更倾向于一个全局的管理者，能够注册一个方法作为事件，允许其他对象发送事件，并通知注册的对象。
// 但是需要确保注册的事件是有效的，不能出现空事件
// 同时密切维护事件的注册与移除

/// <summary>
/// 事件管理者类型
/// </summary>

// 事件回调
using System;
using System.Collections.Generic;
using UnityEngine;

public delegate void EventCallBack(params object[] value);

public static class EventManager
{
    // 静态字典存储事件
    private static readonly Dictionary<object, EventCallBack> _eventDicts = new Dictionary<object, EventCallBack>();
    private static readonly object _lockObj = new object(); // 线程安全锁

    /// <summary>
    /// 注册事件监听
    /// </summary>
    public static void AddListener(object key, EventCallBack callBack)
    {
        if (key == null) throw new ArgumentNullException(nameof(key));
        if (callBack == null) throw new ArgumentNullException(nameof(callBack));

        lock (_lockObj)
        {
            // 确保键存在
            if (!_eventDicts.ContainsKey(key))
            {
                // 取值操作， 会自动添加新键
                // 赋值操作，会在键不存在的时候抛出异常
                _eventDicts[key] = null;
            }

            // 检查是否已注册（比较方法和目标对象）
            if (!IsCallbackRegistered(_eventDicts[key], callBack))
            {
                _eventDicts[key] += callBack;
            }
        }
    }

    /// <summary>
    /// 移除事件监听
    /// </summary>
    public static void RemoveListener(object key, EventCallBack callBack)
    {
        if (key == null) throw new ArgumentNullException(nameof(key));
        if (callBack == null) throw new ArgumentNullException(nameof(callBack));

        lock (_lockObj)
        {
            if (_eventDicts.TryGetValue(key, out EventCallBack source))
            {
                // 检查是否已注册
                if (IsCallbackRegistered(source, callBack))
                {
                    _eventDicts[key] -= callBack;
                    
                    // 清理空委托
                    if (_eventDicts[key] == null)
                    {
                        _eventDicts.Remove(key);
                    }
                }
                else
                {
                    Debug.LogWarning($"事件 {key} 未注册回调 {GetCallbackInfo(callBack)}");
                }
            }
            else
            {
                Debug.LogWarning($"事件 {key} 未注册");
            }
        }
    }

    /// <summary>
    /// 发送事件
    /// </summary>
    public static void SendEvent(object key, params object[] values)
    {
        if (key == null) throw new ArgumentNullException(nameof(key));

        EventCallBack callback = null;
        // 先获取快照，避免发送时被修改
        lock (_lockObj)
        {
            _eventDicts.TryGetValue(key, out callback);
        }
        // 外部执行，避免锁持有时间过长
        callback?.Invoke(values);
    }

    /// <summary>
    /// 清空所有事件
    /// </summary>
    public static void Clear()
    {
        lock (_lockObj)
        {
            _eventDicts.Clear();
        }
    }

    /// <summary>
    /// 判断回调是否已注册
    /// </summary>
    private static bool IsCallbackRegistered(EventCallBack source, EventCallBack target)
    {
        if (source == null) return false;

        // 遍历调用列表，比较方法和目标对象
        foreach (Delegate d in source.GetInvocationList())
        {
            // 静态方法的Target为null，实例方法的Target为对象实例
            if (d.Method == target.Method && d.Target == target.Target)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 获取回调的详细信息（用于日志）
    /// </summary>
    private static string GetCallbackInfo(EventCallBack callback)
    {
        if (callback == null) return "null";
        // 显示方法名和声明类型，方便调试
        return $"{callback.Method.DeclaringType?.Assembly}.{callback.Method.DeclaringType?.Name}.{callback.Method.Name}";
    }
}
