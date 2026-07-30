using UnityEngine;

namespace Unity.FPS.Gameplay
{
    public class StuckBugger : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                other.GetComponent<PlayerInputHandler>().SetSpeedBugMultiplier(0f);
            }
        }
    }
}