using UnityEngine;

namespace Unity.FPS.Gameplay
{
    /// <summary>
    /// Send heathbeat "ALIVE N" to Python
    /// If Unity gets stuck in an infinite loop,
    /// the ALVE message stop get sent.
    /// </summary>

    public class HangOracle : OracleBase
    {
        protected override string OracleName => "HANG ORACLE";

        [Tooltip("Time interval between an heartbeat and another one")]
        public float interval;
        
        private float timer = 0f;

        private void Update()
        {
            timer += Time.deltaTime;

            if (timer >= interval)
            {
                int currentStep = Unity.MLAgents.Academy.Instance.StepCount;
                SendToPython($"[ALIVE]|{currentStep}");
                timer = 0f;
            }
        }
    }
}