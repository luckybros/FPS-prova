using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.SideChannels;
using System.Collections;

namespace Unity.FPS.Gameplay
{
    /// <summary>
    /// Captures errors and Unity exception (Debug.LogError, Exception)
    /// and send them to Python as "ERROR:...".
    /// </summary>
    public class CrashOracle : OracleBase
    {
        protected override string OracleName => "CRASH ORACLE";

        private void OnEnable()
        {
            // Captures Debug.LogError and exception
            Application.logMessageReceived += OnLogMessageReceived;
        }

        private void OnDisable()
        {
            Application.logMessageReceived -= OnLogMessageReceived;
        }

        private void OnLogMessageReceived(string logString, string stackTrace, LogType type)
        {
            if (type == LogType.Error || type == LogType.Exception)
            {
                string cleanLog = logString.Replace("\n", " ").Replace("\r", " ");
                string cleanStack = stackTrace.Replace("\n", " ").Replace("\r", " ");

                if (cleanStack.Length > 150)
                {
                    cleanStack = cleanStack.Substring(0, 150) + "...";
                }

                string safeMessage = $"{cleanLog} - STACK: {cleanStack}";

                if (type == LogType.Error)
                {
                    ReportBug("error", safeMessage);
                }
                else if (type == LogType.Exception)
                {
                    ReportBug("exception", safeMessage);
                }
            }
        }
    }
}