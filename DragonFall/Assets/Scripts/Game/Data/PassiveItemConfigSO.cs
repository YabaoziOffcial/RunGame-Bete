using UnityEngine;

namespace Dragonfall.Data
{
    public enum PassiveStatType
    {
        MaxHP,
        MoveSpeed,
        Damage,
        Cooldown,
        PickUpRange,
        ProjectileCount,
        Shield,
        HPRegen
    }

    [CreateAssetMenu(menuName = "Dragonfall/Passive Item Config", fileName = "PassiveItem")]
    public class PassiveItemConfigSO : ScriptableObject
    {
        [Header("Identity")]
        public string itemName = "Health Amulet";
        public string description = "+20% Max HP";
        public Sprite icon;
        public int maxLevel = 5;

        [Header("Effect")]
        public PassiveStatType statType;
        public float valuePerLevel = 0.2f;
        public bool isPercentage = true;
    }
}
