using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Object = UnityEngine.Object;
using TMPro;
using System;
using System.Collections.Generic;
        
#region 组件缓存
/// <summary>
/// 组件缓存说明（尽量“低侵入”接入你现有扩展方法）：
/// - **按 Transform 实例缓存**：每个 Transform 都有一个自己的缓存字典。
/// - **按查询方式区分**：Self / InParent / InChildren 等不同查询结果分别缓存，避免互相污染。
/// - **按类型缓存**：同一个 Transform 可以缓存多种组件类型（例如 Image/Text/Slider）。
/// - **支持单个/多个**：同一种类型可以缓存“单个组件”或“组件数组”。
/// - **自动清扫**：Transform 被 Destroy 后，缓存会在访问时周期性清理；也可以手动 Clear。
///
/// 注意：如果运行时对层级结构做了大改（例如动态增删子物体/组件），
/// 你可能希望对相关 Transform 调用 `ClearComponentCache()` 以确保缓存立刻刷新。
/// </summary>
internal enum CacheQuery : byte
{
    Self = 0,
    InParent = 1,
    InChildren = 2,
    InParents = 3,
    InChildrenAll = 4,
}


// 缓存键
internal readonly struct CacheKey : IEquatable<CacheKey>
{
    public readonly Type Type;
    public readonly CacheQuery Query;
    public readonly bool IncludeInactive;
    public readonly bool IsMultiple;

    public CacheKey(Type type, CacheQuery query, bool includeInactive, bool isMultiple)
    {
        Type = type;
        Query = query;
        IncludeInactive = includeInactive;
        IsMultiple = isMultiple;
    }

    // 比较缓存键是否相等
    public bool Equals(CacheKey other)
    {
        return Type == other.Type
                && Query == other.Query
                && IncludeInactive == other.IncludeInactive
                && IsMultiple == other.IsMultiple;
    }

    public override bool Equals(object obj) => obj is CacheKey other && Equals(other);

    // 计算缓存键的哈希值
    public override int GetHashCode()
    {
        unchecked
        {
            int hash = Type != null ? Type.GetHashCode() : 0;
            hash = (hash * 397) ^ (int)Query;
            hash = (hash * 397) ^ (IncludeInactive ? 1 : 0);
            hash = (hash * 397) ^ (IsMultiple ? 1 : 0);
            return hash;
        }
    }
}


// 缓存组件，用于缓存组件
public static class StaticCacheComponent
{
    // 缓存项   
    internal sealed class CacheEntry
    {
        public WeakReference<Transform> TargetRef; // 目标引用
        public readonly Dictionary<CacheKey, object> Cache = new Dictionary<CacheKey, object>(32); // 缓存字典
    }

    // 组件缓存
    private static readonly Dictionary<int, CacheEntry> s_ComponentCache = new Dictionary<int, CacheEntry>(1024);
    private static int s_LastSweepFrame = -1;
    private const int SweepIntervalFrames = 300; // 清扫间隔帧数

    /// <summary>
    /// 清理所有 Transform 组件缓存
    /// </summary>
    public static void ClearAllComponentCache()
    {
        s_ComponentCache.Clear();
    }

    /// <summary>
    /// 清理某个 Transform 的所有组件缓存
    /// </summary>
    public static void ClearComponentCache(this Transform t)
    {
        if (t == null) return;
        s_ComponentCache.Remove(t.GetInstanceID());
    }

    /// <summary>
    /// 清理某个 Transform 下指定类型的缓存（包含不同查询方式的同类型缓存）
    /// </summary>
    public static void ClearComponentCache<T>(this Transform t) where T : Component
    {
        if (t == null) return;
        int id = t.GetInstanceID();
        if (!s_ComponentCache.TryGetValue(id, out var entry) || entry == null) return;

        var toRemove = new List<CacheKey>();
        var type = typeof(T);
        foreach (var kv in entry.Cache)
        {
            if (kv.Key.Type == type) toRemove.Add(kv.Key);
        }
        for (int i = 0; i < toRemove.Count; i++)
        {
            entry.Cache.Remove(toRemove[i]);
        }
    }

    // 清扫已销毁的缓存项
    private static void SweepDestroyedEntriesIfNeeded()
    {
        // "【StaticCacheComponent】SweepDestroyedEntriesIfNeeded ".Log();

        int frame = Time.frameCount; // 从游戏开始运行到现在的帧数
        if (s_LastSweepFrame >= 0 && frame - s_LastSweepFrame < SweepIntervalFrames) return;
        s_LastSweepFrame = frame;

        if (s_ComponentCache.Count == 0) return; // 如果缓存为空，则返回

        var deadKeys = new List<int>();
        foreach (var kv in s_ComponentCache)
        {
            var entry = kv.Value;
            if (entry == null)
            {
                deadKeys.Add(kv.Key);
                continue;
            }

            if (entry.TargetRef == null)
            {
                deadKeys.Add(kv.Key);
                continue;
            }

            if (!entry.TargetRef.TryGetTarget(out var tr) || tr == null)
            {
                deadKeys.Add(kv.Key);
            }
        }

        for (int i = 0; i < deadKeys.Count; i++)
        {
            s_ComponentCache.Remove(deadKeys[i]);
            ("清理缓存：" + deadKeys[i]).Log();
        }
    }

    // 获取或创建缓存项
    private static CacheEntry GetOrCreateEntry(Transform t)
    {
        if (t == null) return null;
        SweepDestroyedEntriesIfNeeded();

        int id = t.GetInstanceID();
        if (!s_ComponentCache.TryGetValue(id, out var entry) || entry == null)
        {
            entry = new CacheEntry { TargetRef = new WeakReference<Transform>(t) };
            s_ComponentCache[id] = entry;
            return entry;
        }

        if (entry.TargetRef == null)
        {
            entry.TargetRef = new WeakReference<Transform>(t);
            return entry;
        }

        if (!entry.TargetRef.TryGetTarget(out var target) || target == null)
        {
            entry.TargetRef.SetTarget(t);
            entry.Cache.Clear();
            return entry;
        }

        // InstanceID 复用极少见，但为了安全：如果不是同一个 Transform，重建缓存
        if (!ReferenceEquals(target, t))
        {
            entry.TargetRef.SetTarget(t);
            entry.Cache.Clear();
        }

        return entry;
    }

    // 手动把某个组件写入缓存（常用于 AddComponent 后）
    private static void CacheComponentInternal(Transform t, Component c)
    {
        if (t == null || c == null) return;
        var entry = GetOrCreateEntry(t);
        if (entry == null) return;
        entry.Cache[new CacheKey(c.GetType(), CacheQuery.Self, includeInactive: false, isMultiple: false)] = c;
    }

    /// <summary>
    /// 手动把某个组件写入缓存（常用于 AddComponent 后）
    /// </summary>
    public static void CacheComponent(this Transform t, Component c)
    {
        CacheComponentInternal(t, c);
    }

    // 获取并缓存：单个组件
    public static T GetCachedComponent<T>(this Transform t) where T : Component
    {
        return GetCachedSingle<T>(t, CacheQuery.Self, includeInactive: false);
    }
    // 尝试获取并缓存：单个组件
    public static bool TryGetCachedComponent<T>(this Transform t, out T comp) where T : Component
    {
        comp = GetCachedComponent<T>(t);
        return comp != null;
    }

    public static T GetCachedComponentInParent<T>(this Transform t, bool includeInactive = true) where T : Component
    {
        return GetCachedSingle<T>(t, CacheQuery.InParent, includeInactive);
    }

    public static T GetCachedComponentInChildren<T>(this Transform t, bool includeInactive = true) where T : Component
    {
        return GetCachedSingle<T>(t, CacheQuery.InChildren, includeInactive);
    }

    public static T[] GetCachedComponents<T>(this Transform t) where T : Component
    {
        return GetCachedMultiple<T>(t, CacheQuery.Self, includeInactive: false);
    }

    public static T[] GetCachedComponentsInParent<T>(this Transform t, bool includeInactive = true) where T : Component
    {
        return GetCachedMultiple<T>(t, CacheQuery.InParents, includeInactive);
    }

    public static T[] GetCachedComponentsInChildren<T>(this Transform t, bool includeInactive = true) where T : Component
    {
        return GetCachedMultiple<T>(t, CacheQuery.InChildrenAll, includeInactive);
    }

    /// <summary>
    /// 获取并缓存：单个组件
    /// </summary>
    private static T GetCachedSingle<T>(Transform t, CacheQuery query, bool includeInactive) where T : Component
    {
        if (t == null) return null;

        var entry = GetOrCreateEntry(t);
        if (entry != null)
        {
            var key = new CacheKey(typeof(T), query, includeInactive, isMultiple: false);
            if (entry.Cache.TryGetValue(key, out var obj))
            {
                var cached = obj as T;
                if (cached != null) return cached;
                entry.Cache.Remove(key); // 组件已销毁或类型不匹配
            }
        }

        // 缓存未命中：走 Unity 原生查询一次
        T comp = query switch
        {
            CacheQuery.Self => t.GetComponent<T>(),
            CacheQuery.InParent => t.GetComponentInParent<T>(includeInactive),
            CacheQuery.InChildren => t.GetComponentInChildren<T>(includeInactive),
            _ => t.GetComponent<T>()
        };

        if (comp != null && entry != null)
        {
            entry.Cache[new CacheKey(typeof(T), query, includeInactive, isMultiple: false)] = comp;
        }

        return comp;
    }

    /// <summary>
    /// 获取并缓存：多个组件（数组）
    /// </summary>
    private static T[] GetCachedMultiple<T>(Transform t, CacheQuery query, bool includeInactive) where T : Component
    {
        if (t == null) return null;

        var entry = GetOrCreateEntry(t);
        if (entry != null)
        {
            var key = new CacheKey(typeof(T), query, includeInactive, isMultiple: true);
            if (entry.Cache.TryGetValue(key, out var obj))
            {
                // 这里缓存的是 T[]；如果里面元素被 Destroy 了，下次再 Get 会刷新（尽量保持简单）
                if (obj is T[] arr) return arr;
                entry.Cache.Remove(key);
            }
        }

        // 缓存未命中：走 Unity 原生查询一次
        T[] comps = query switch
        {
            CacheQuery.Self => t.GetComponents<T>(),
            CacheQuery.InParents => t.GetComponentsInParent<T>(includeInactive),
            CacheQuery.InChildrenAll => t.GetComponentsInChildren<T>(includeInactive),
            _ => t.GetComponents<T>()
        };

        if (entry != null)
        {
            entry.Cache[new CacheKey(typeof(T), query, includeInactive, isMultiple: true)] = comps;
        }
        return comps;
    }
}

#endregion
