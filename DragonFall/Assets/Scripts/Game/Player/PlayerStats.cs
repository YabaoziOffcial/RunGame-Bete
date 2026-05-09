using UnityEngine;

namespace Dragonfall.Player
{
    public class PlayerStats : MonoBehaviour
    {
        [Header("Base Stats")]
        public float maxHP = 70f;
        public float moveSpeed = 3.5f;
        public float damageMultiplier = 1f;
        public float cooldownReduction = 0f;
        public float pickUpRange = 2f;
        public int extraProjectiles = 0;
        public float damageReduction = 0f;
        public float hpRegenPerSec = 0f;

        [Header("Runtime")]
        [SerializeField] private float currentHP;
        public float CurrentHP
        {
            get => currentHP;
            private set
            {
                currentHP = Mathf.Clamp(value, 0f, maxHP);
                if (currentHP <= 0f)
                    OnDeath();
            }
        }

        public bool IsDead { get; private set; }

        public void Init(float hp, float spd, float dmg, float range)
        {
            maxHP = hp;
            moveSpeed = spd;
            damageMultiplier = dmg / 10f; // baseDamage / 10 as multiplier
            pickUpRange = range;
            currentHP = maxHP;
            IsDead = false;
            damageMultiplier = 1f;
            cooldownReduction = 0f;
            extraProjectiles = 0;
            damageReduction = 0f;
            hpRegenPerSec = 0f;
        }

        public void TakeDamage(float rawDamage)
        {
            if (IsDead) return;
            float dmg = rawDamage * (1f - damageReduction);
            CurrentHP -= dmg;
            Core.EventManager.SendEvent(Core.GameEvents.OnPlayerDamaged, dmg, CurrentHP, maxHP);
        }

        public void Heal(float amount)
        {
            if (IsDead) return;
            CurrentHP += amount;
        }

        public void HealPercent(float percent)
        {
            if (IsDead) return;
            CurrentHP += maxHP * percent;
        }

        private void OnDeath()
        {
            IsDead = true;
            Core.EventManager.SendEvent(Core.GameEvents.OnPlayerDeath);
        }

        private void Update()
        {
            if (IsDead) return;
            if (hpRegenPerSec > 0f)
            {
                CurrentHP += hpRegenPerSec * Time.unscaledDeltaTime;
            }
        }

        public void ApplyPassive(Data.PassiveStatType statType, float value, bool isPercentage)
        {
            float applied = isPercentage ? value : value;

            switch (statType)
            {
                case Data.PassiveStatType.MaxHP:
                    float hpRatio = CurrentHP / maxHP;
                    maxHP += isPercentage ? maxHP * value : value;
                    currentHP = maxHP * hpRatio;
                    break;
                case Data.PassiveStatType.MoveSpeed:
                    moveSpeed += isPercentage ? moveSpeed * value : value;
                    break;
                case Data.PassiveStatType.Damage:
                    damageMultiplier += isPercentage ? damageMultiplier * value : value;
                    break;
                case Data.PassiveStatType.Cooldown:
                    cooldownReduction += value; // non-percentage: 0.1 = 10% CDR
                    break;
                case Data.PassiveStatType.PickUpRange:
                    pickUpRange += isPercentage ? pickUpRange * value : value;
                    break;
                case Data.PassiveStatType.ProjectileCount:
                    extraProjectiles += Mathf.RoundToInt(value);
                    break;
                case Data.PassiveStatType.HPRegen:
                    hpRegenPerSec += isPercentage ? maxHP * value : value;
                    break;
            }

            Core.EventManager.SendEvent(Core.GameEvents.OnStatChanged);
        }
    }
}
