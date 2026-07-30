using UnityEngine;
using System;

namespace Unity.FPS.Gameplay
{
    public class BuggerHealthPickup : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Projectile") && FaultManager.Instance != null && FaultManager.Instance.Config.crashShootingPowerUp)
            {
                throw new Exception("Shoot a pickup!");
                return;
            }
        }
    }
}
