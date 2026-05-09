using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using YBZ.Design;

// 2024.7.5 解决只加入物体时ObjectPoll空引用问题

public class ObjectPool
{
    private readonly static Dictionary<string, Queue<GameObject>> m_Pool = new();
    private static GameObject m_PoolPos = GameObject.Find("ObjectPool");

    /// <summary>
    /// 弹出一个
    /// </summary>
    /// <param name="prefab"></param>
    /// <returns></returns>
    public static GameObject GetObj(GameObject prefab, Transform parent = null, bool ZeroPosAndRotation = true)
    {
        GameObject result;
        string poolKey = prefab.name.Replace("(Clone)", string.Empty);
        // 池中物体不够
        if (!m_Pool.ContainsKey(poolKey) || m_Pool[poolKey].Count == 0)
        {
            result = GameObject.Instantiate(prefab);
            result.name = poolKey;
            if (m_PoolPos == null)
            {
                m_PoolPos = new GameObject("ObjectPool");
            }
            Transform child = m_PoolPos.transform.Find(poolKey + "Pool");
            if (child == null)
            {
                GameObject childObj = new GameObject(poolKey + "Pool");
                childObj.transform.SetParent(m_PoolPos.transform);
                child = childObj.transform;
            }
            result.transform.SetParent(child);
            if (!m_Pool.ContainsKey(poolKey))
            {
                m_Pool.Add(poolKey, new Queue<GameObject>());
            }
            m_Pool[poolKey].Enqueue(result);
        }

        result = m_Pool[poolKey].Dequeue();
        if (!result.activeSelf)
        {
            result.SetActive(true);
        }

        if (parent != null) result.transform.SetParent(parent, false);
        if (ZeroPosAndRotation)
        {
            result.transform.localPosition = Vector3.zero;
            result.transform.localScale = Vector3.one;
        }
        return result;
    }

    /// <summary>
    /// 弹入一个
    /// </summary>
    /// <param name="prefab"></param>
    public static bool PushObj(GameObject prefab, bool use_pool_pos = true)
    {
        string name = prefab.name.Replace("(Clone)", string.Empty);
        if (!m_Pool.ContainsKey(name))
        {
            m_Pool.Add(name, new Queue<GameObject>());
        }
        if (m_Pool[name].Contains(prefab))
        {
            Y_Debug.Log("物体已存在:", name);
            return false;
        }

        prefab.SetActive(false);
        m_Pool[name].Enqueue(prefab);

        if (use_pool_pos)
        {
            if (m_PoolPos == null) m_PoolPos = new GameObject("ObjectPool");
            Transform child = m_PoolPos.transform.Find(name + "Pool");
            if (child == null)
            {
                GameObject childObj = new GameObject(name + "Pool");
                childObj.transform.SetParent(m_PoolPos.transform);
                child = childObj.transform;
            }
            prefab.transform.SetParent(child);
            prefab.transform.localPosition = Vector3.zero;
        }
        return true;
    }

    /// <summary>
    /// 将父对象的所有子物体都加入对象池中
    /// </summary>
    /// <param name="prefab"></param>
    public static void PushAllChildren(GameObject prefab, bool use_pool_pos = true)
    {
        while (prefab.transform.childCount > 0)
        {
            PushObj(prefab.transform.GetChild(0).gameObject, use_pool_pos);
        }
    }

    public static void PushAllChildren(Transform parent, bool use_pool_pos = true, params GameObject[] excludeObjects)
    {
        if (parent == null) return;

        var excludeSet = excludeObjects == null ? null : new HashSet<GameObject>(excludeObjects);
        var childrenToPush = new List<GameObject>();
        for (int i = 0; i < parent.childCount; i++)
        {
            GameObject child = parent.GetChild(i).gameObject;
            if (excludeSet != null && excludeSet.Contains(child)) continue;
            childrenToPush.Add(child);
        }

        for (int i = 0; i < childrenToPush.Count; i++)
        {
            PushObj(childrenToPush[i], use_pool_pos);
        }
    }

    public static void Clear(string key)
    {
        if (m_Pool.ContainsKey(key))
        {
            m_Pool.Remove(key);
        }
    }

    /// <summary>
    /// 清空对象池
    /// </summary>
    public static void ClearAll()
    {
        m_Pool.Clear();
    }
}
