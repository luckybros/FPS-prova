using UnityEngine;
using Unity.FPS.Game;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

namespace Unity.FPS.Gameplay
{
    public class PlayerAgent : Agent
    {
        private Vector3 startPosition;
        
        void Start()
        {
            startPosition = transform.localPosition;    
        }

        public override void OnEpisodeBegin()
        {
            transform.localPosition = startPosition;
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            sensor.AddObservation(transform.localPosition);
        }
    }
}
