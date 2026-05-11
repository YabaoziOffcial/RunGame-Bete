using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 装备基类
public abstract class EquipBase
{
    public EquipData EquipData{get; set;} 
    public abstract void OnEquipEnter(Player player);

    public abstract void OnEquipUpdate(Player player);

    public abstract void OnEquipFixedUpdate(Player player);

    public abstract void OnEquipExit(Player player);
}

public class EquipData
{
    public int id;
    public int level;
    public int quality;
    public int rarity;
    public int icon;
    public string name;
    public string description;
}
