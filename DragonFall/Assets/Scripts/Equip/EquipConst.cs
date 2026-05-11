using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipConst
{
    public GameObject GetWeaponPrefab(string name)
    {
        string path = $"Weapon/{name}";
        return ResourceManager.Instance.LoadRes<GameObject>(path);
    }
}
