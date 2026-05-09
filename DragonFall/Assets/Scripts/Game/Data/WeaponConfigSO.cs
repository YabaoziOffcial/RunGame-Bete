using UnityEngine;

namespace Dragonfall.Data
{
    public enum WeaponType
    {
        Projectile,
        MeleeCone,
        DirectionalShot
    }

    [CreateAssetMenu(menuName = "Dragonfall/Weapon Config", fileName = "WeaponConfig")]
    public class WeaponConfigSO : ScriptableObject
    {
        [Header("Identity")]
        public string weaponName = "Magic Missile";
        public string description = "Auto-targets nearest enemy";
        public WeaponType weaponType = WeaponType.Projectile;
        public Sprite icon;

        [Header("Stats")]
        public float damage = 10f;
        public float cooldown = 1f;
        public float projectileSpeed = 6f;
        public float range = 8f;
        public float lifetime = 2f;

        [Header("Pattern")]
        public int projectileCount = 1;
        public float spreadAngle = 0f;
        public bool autoAim = true;

        [Header("Visual")]
        public Color projectileColor = Color.cyan;
        public float projectileSize = 0.25f;
    }
}
