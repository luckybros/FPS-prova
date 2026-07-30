using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.SideChannels;
using System.Collections;

namespace Unity.FPS.Gameplay
{
    /// <summary>
    /// Singleton with the unique instance of OracleSideChannel
    /// All the oracles passes from here in order to send messages to Python
    /// </summary>
    public class OracleManager: MonoBehaviour
    {
        public static OracleManager Instance { get; private set;}

        private OracleSideChannel sideChannel;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            StartCoroutine(InitializeWhenReady());
        }

        private IEnumerator InitializeWhenReady()
        {
            while (!Academy.IsInitialized)
            {
                Debug.Log("[OracleManager] Waiting for Academy to inizitalize...");
                yield return new WaitForSeconds(0.1f);
            }

            yield return null;

            sideChannel = new OracleSideChannel();

            SideChannelManager.RegisterSideChannel(sideChannel);

            int currentStep = Unity.MLAgents.Academy.Instance.StepCount;

            sideChannel.SendStringToPython($"[START]|{currentStep}");
        }

        private void OnDestroy()
        {
            if (sideChannel != null && Academy.IsInitialized)
            {
                SideChannelManager.UnregisterSideChannel(sideChannel);
            }

            if (Instance == this)
            {
                Instance = null;
            }
        }

        /// <summary>
        /// Public API: all the oracles call this method in order 
        /// to send messages to Python
        /// </summary>
        public void SendToPython(string message)
        {
            if (sideChannel != null)
            {
                sideChannel.SendStringToPython(message);
            }
        }

        /// <summary>
        /// API pubblica per i game logic oracle: invia un bug report formattato.
        /// </summary>
        public void ReportGameLogicBug(string oracleName, string bugType, string message)
        {
            int currentStep = Unity.MLAgents.Academy.Instance.StepCount;
            string fullMessage = $"[GAME_BUG]|{oracleName}|{bugType}|{message}|{currentStep}";
            SendToPython(fullMessage);
        }
    }
}