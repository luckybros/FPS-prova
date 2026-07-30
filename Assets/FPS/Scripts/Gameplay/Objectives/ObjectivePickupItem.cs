using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.Gameplay
{
    public class ObjectivePickupItem : Objective
    {
        [Tooltip("Item to pickup to complete the objective")]
        public GameObject ItemToPickup;

        protected override void Start()
        {
            base.Start();

            // Subscribe to the local environment's PickupEvent only.
            m_EventManager.AddListener<PickupEvent>(OnPickupEvent);
        }

        void OnPickupEvent(PickupEvent evt)
        {
            if (IsCompleted || ItemToPickup != evt.Pickup)
                return;

            CompleteObjective(string.Empty, string.Empty, "Objective complete : " + Title);

            if (gameObject)
                Destroy(gameObject);
        }

        void OnDestroy()
        {
            if (m_EventManager != null)
                m_EventManager.RemoveListener<PickupEvent>(OnPickupEvent);
        }
    }
}
