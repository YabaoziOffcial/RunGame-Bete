using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 专门用来保存路径
public class PathConst 
{
    public static string GetEnemyPrefabPath(string name)
    {
        return $"Prefab/Enemy/{name}";
    }

    public static string GetExPrefabPath()
    {
        return "Prefab/Ex/EX_1";
    }

    public static string GetDamageNumberPrefabPath()
    {
        return "Prefab/UI/DamageNumberText";
    }

    public static string GetWeaponConfigPath(string name)
    {
        return $"Config/WeaponConfig/{name}";
    }

    public static string GetEnemySpawnConfigPath(string name)
    {
        return $"Config/EnemySpawnConfig/{name}";
    }

    public static string GetEquipConfigPath(string name)
    {
        return $"Config/{name}";
    }
}
