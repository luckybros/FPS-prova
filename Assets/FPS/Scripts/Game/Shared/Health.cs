using UnityEngine;
using UnityEngine.Events;
using Unity.FPS.Gameplay;

namespace Unity.FPS.Game
{
    public class Health : MonoBehaviour
    {
        [Tooltip("Maximum amount of health")] public float MaxHealth = 10f;

        [Tooltip("Health ratio at which the critical health vignette starts appearing")]
        public float CriticalHealthRatio = 0.3f;

        public UnityAction<float, GameObject> OnDamaged;
        public UnityAction<float> OnHealed;

        public static UnityAction<float, float, float> OnDamagedStatic;
        public static UnityAction<float, float, float> OnHealedStatic;

        public UnityAction OnDie;

        [field: SerializeField]
        public float CurrentHealth { get; set; }
        public bool Invincible { get; set; }
        public bool CanPickup() => true;

        public float GetRatio() => CurrentHealth / MaxHealth;
        public bool IsCritical() => GetRatio() <= CriticalHealthRatio;

        bool m_IsDead;

        void Start()
        {
            CurrentHealth = MaxHealth;
        }

        public void Heal(float healAmount)
        {
            float healthBefore = CurrentHealth;

            // Heal over max
            if (FaultManager.Instance != null && FaultManager.Instance.Config.logicalHealOverMax)
            {
                CurrentHealth += healAmount;
            }
            else
            {
                CurrentHealth += healAmount;
                CurrentHealth = Mathf.Clamp(CurrentHealth, 0f, MaxHealth);
            }

            // No healing if health in a range
            if (FaultManager.Instance != null && FaultManager.Instance.Config.logicalNoHealing)
            {
                if (healthBefore <= 60f && healthBefore >= 50f)
                {
                    CurrentHealth -= healAmount;
                }
            }

            // call OnHeal action
            float trueHealAmount = CurrentHealth - healthBefore;
            OnHealed?.Invoke(trueHealAmount);
            OnHealedStatic?.Invoke(healthBefore, CurrentHealth, trueHealAmount);
        }

        public void TakeDamage(float damage, GameObject damageSource)
        {
            if (Invincible)
                return;

            if (FaultManager.Instance != null && FaultManager.Instance.Config.crashDamageDistance)
            {
                if (damageSource.CompareTag("Player"))
                {
                    Vector3 distanceBetweenEnemyAndPlayer = damageSource.transform.position - transform.position;
                    float distanceBetweenEnemyAndPlayerMagnitude = distanceBetweenEnemyAndPlayer.magnitude - 2;
                    int distInt = Mathf.FloorToInt(distanceBetweenEnemyAndPlayerMagnitude);
                    int bonusDamage = Mathf.FloorToInt(damage) / distInt;
                }
            }


            float healthBefore = CurrentHealth;
            CurrentHealth -= damage;
            CurrentHealth = Mathf.Clamp(CurrentHealth, 0f, MaxHealth);

            if (FaultManager.Instance != null && FaultManager.Instance.Config.logicalDamageTwoTimesToEnemy)
            {
                if (damageSource.CompareTag("Player"))
                {
                    if (CurrentHealth <= 40f && CurrentHealth >= 20f)
                    {
                        CurrentHealth -= damage;
                    }
                }
            }

            // call OnDamage action
            float trueDamageAmount = healthBefore - CurrentHealth;
            if (trueDamageAmount > 0f)
            {
                OnDamaged?.Invoke(trueDamageAmount, damageSource);
                OnDamagedStatic?.Invoke(healthBefore, CurrentHealth, damage);
            }

            HandleDeath();
        }

        public void Kill()
        {
            CurrentHealth = 0f;

            // call OnDamage action
            OnDamaged?.Invoke(MaxHealth, null);

            HandleDeath();
        }

        void HandleDeath()
        {
            if (m_IsDead)
                return;

            // call OnDie action
            if (CurrentHealth <= 0f)
            {
                m_IsDead = true;
                OnDie?.Invoke();
            }
        }

        /// <summary>Restores health to max and clears the dead state. Used for RL environment reset.</summary>
        public void ResetHealth()
        {
            CurrentHealth = MaxHealth;
            m_IsDead = false;
        }
    }
}