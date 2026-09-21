using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.SideChannels;
using System.Text;
using System;

namespace Unity.FPS.Gameplay
{
    public class OracleSideChannel : SideChannel
    {
        public static event Action<int> OnResetReceived;
        public OracleSideChannel()
        {
            ChannelId = new Guid("621f0a70-4f87-11ea-a6bf-784f4387d1f7");
        }

        protected override void OnMessageReceived(IncomingMessage msg)
        {
            var receivedString = msg.ReadString();
            Debug.Log("From Python : " + receivedString);

            if (receivedString.StartsWith("RESET"))
            {
                int envId = 0;
                var parts = receivedString.Split(':');
                if (parts.Length > 1)
                    int.TryParse(parts[1], out envId);

                Debug.Log($"Comando di reset ricevuto per environment {envId}.");
                OnResetReceived?.Invoke(envId);
            }
        }

        public void SendStringToPython(string msg)
        {
            // Debug.Log($"Sending to Python {msg}");
            var stringToSend = msg;
            using (var msgOut = new OutgoingMessage())
            {
                msgOut.WriteString(stringToSend);
                QueueMessageToSend(msgOut);
            }
        }
    }
}