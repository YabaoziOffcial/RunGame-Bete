using UnityEngine;

namespace Dragonfall.Data
{
    [CreateAssetMenu(menuName = "Dragonfall/Enemy Config", fileName = "EnemyConfig")]
    public class EnemyConfigSO : ScriptableObject
    {
        [Header("Identity")]
        public string enemyName = "Slime";
        public int tier = 1;
        public bool isBoss = false;

        [Header("Stats")]
        public float maxHP = 5f;
        public float moveSpeed = 1.5f;
        public float damage = 5f;
        public float contactCooldown = 0.5f;

        [Header("Rewards")]
        public int xpValue = 5;
        public int goldValue = 1;

        [Header("Visual")]
        public Color color = Color.green;
        public float size = 0.5f;
    }
}
