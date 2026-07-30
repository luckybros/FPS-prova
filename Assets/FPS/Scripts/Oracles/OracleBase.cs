using UnityEngine;
using System.Collections.Generic;

namespace Unity.FPS.Gameplay
{
    /// <summary>
    /// Classe base astratta per TUTTI gli oracoli (crash, hang, stuck, logici).
    /// Fornisce: accesso al singleton OracleManager, report con cooldown.
    /// </summary>
    public abstract class OracleBase : MonoBehaviour
    {
        // Cooldown per non spammare lo stesso errore
        private Dictionary<string, float> errorTimestamps = new Dictionary<string, float>();

        [Tooltip("Secondi di cooldown prima di poter risegnalare lo stesso tipo di errore")]
        public float reportCooldown = 5f;

        /// <summary>Nome dell'oracolo, usato nei log e nel CSV.</summary>
        protected abstract string OracleName { get; }

        /// <summary>
        /// Segnala un bug via OracleManager singleton. Non chiude il gioco.
        /// </summary>
        protected void ReportBug(string bugType, string message)
        {
            // Cooldown: non segnalare lo stesso bug troppo spesso
            if (errorTimestamps.ContainsKey(bugType))
            {
                if (Time.time - errorTimestamps[bugType] < reportCooldown)
                    return;
            }
            errorTimestamps[bugType] = Time.time;

            // int currentStep = Unity.MLAgents.Academy.Instance.StepCount;

            Debug.LogWarning($"[{OracleName}] {message}");

            if (OracleManager.Instance != null)
            {
                OracleManager.Instance.ReportGameLogicBug(OracleName, bugType, message);
            }
        }

        /// <summary>
        /// Invia un messaggio raw a Python (per ALIVE, ERROR, ecc.)
        /// </summary>
        protected void SendToPython(string message)
        {
            if (OracleManager.Instance != null)
            {
                OracleManager.Instance.SendToPython(message);
            }
        }
    }
}