using System;
using UnityEngine;

namespace Unity.FPS.Gameplay
{
    [Serializable]
    public class FaultConfig
    {
        [Header("Stuck bugs")]
        public bool stuckBugs;

        [Header("Hang bugs")]
        public bool hangBugDieInVoid;

        [Header("Crash bugs")]
        public bool crashDamageDistance;
        public bool crashShootingPowerUp;
        public bool crashShootZeroAmmo;

        [Header("Logical bugs")]
        public bool logicalHealOverMax;
        public bool logicalNoHealing;
        public bool logicalDamageTwoTimesToEnemy;
    }
}

