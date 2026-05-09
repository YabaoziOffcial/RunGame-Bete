using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// 绑定到一个物体物体上
public class YUID : Component
{
    public Object target;
    new public string name = "";
    public int key = 0;

    public void OnEnable()
    {
        ObjectFind.Add(this, this.gameObject);
    }

    private void OnValidate()
    {
        target ??= this;
        name = gameObject.name;
        key = GetHashCode();
    }

    public override int GetHashCode()
    {
        
        return base.GetHashCode();
    }
}


public class ObjectFind
{
    public static Dictionary<YUID, object > dict = new Dictionary<YUID, object>();
   

    public static bool TryAdd(YUID info, object obj)
    {
        return dict.TryAdd(info, obj);
    }

    public static void Add(YUID info, object obj)
    {
        dict.Add(info, obj);
    }

    public YUID Get(GameObject go)
    {
        YUID info = go.GetComponent<YUID>();
        if(info == null)
        {
            info = go.AddComponent<YUID>();
        }
        return info;
    }



    public virtual GameObject Find(string name)
    {
        GameObject go = GameObject.Find(name);
        if(go == null)
        {
            Debug.LogError("GameObject " + name + " not found!");
        }
        return go;
    }

    public virtual GameObject Find(int key)
    {
        return null;
    }
}