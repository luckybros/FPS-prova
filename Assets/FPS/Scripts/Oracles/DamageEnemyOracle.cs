using UnityEngine;
using Unity.FPS.Game;

namespace Unity.FPS.Gameplay
{
    public class DamageEnemyOracle : OracleBase
    {
        protected override string OracleName => "DAMAGE ENEMY ORACLE";

        private float maxHealth = 100f;

        private void OnEnable()
        {
            Health.OnDamagedStatic += OnDamageApplied;
        }

        private void OnDisable()
        {
            Health.OnDamagedStatic -= OnDamageApplied;
        }

        private void OnDamageApplied(float healthBefore, float healthAfter, float damageAmount)
        {
            if (healthAfter < healthBefore - damageAmount)
            {
                ReportBug("multiple_damage",
                    $"Enemy took more damage than the original one! " +
                    $"Before: {healthBefore:F2}, After: {healthAfter:F2}" +
                    $"Supposed damage: {damageAmount:F2}.");
            }
        }
    }
}