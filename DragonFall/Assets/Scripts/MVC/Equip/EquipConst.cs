using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 装备模块资源入口
public class EquipConst
{
    // 根据武器名称加载武器预制体
    public static GameObject GetWeaponPrefab(string name)
    {
        string path = $"Weapon/{name}";
        return ResourceManager.Instance.LoadRes<GameObject>(path);
    }

    public static Sprite GetWeaponIconPath(string name)
    {
        return ResourceManager.Instance.LoadRes<Sprite>($"Sprite/Icon/{name}_Icon");
    }

    public static Sprite DefaultIcon
    {
        get
        {
            if (m_DefaultIcon == null)
            {
                m_DefaultIcon = ResourceManager.Instance.LoadRes<Sprite>("Sprite/Icon/default_icon");
            }
            return m_DefaultIcon;
        }
    }
    private static Sprite m_DefaultIcon;
}
