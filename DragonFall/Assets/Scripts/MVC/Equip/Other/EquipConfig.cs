using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "DragonFall/Equip/EquipConfig")]
public class EquipConfig : ScriptableObject
{
    [SerializeField]
    public List<WeaponConfig> equips;
}
