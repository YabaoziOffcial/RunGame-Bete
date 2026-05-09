using UnityEngine;

namespace Dragonfall.Data
{
    [CreateAssetMenu(menuName = "Dragonfall/Character Config", fileName = "CharacterConfig")]
    public class CharacterConfigSO : ScriptableObject
    {
        [Header("Basic")]
        public string characterName = "Elara";
        public string description = "Arcane Mage";
        public Color characterColor = new Color(0.3f, 0.3f, 0.9f);

        [Header("Stats")]
        public float maxHP = 70f;
        public float moveSpeed = 3.5f;
        public float baseDamage = 10f;
        public float pickUpRange = 2f;
        public float bodySize = 0.6f;

        [Header("Starting Weapon")]
        public WeaponConfigSO startingWeapon;
    }
}
