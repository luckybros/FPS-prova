using UnityEngine;

namespace Unity.FPS.Game
{
    public abstract class Objective : MonoBehaviour
    {
        [Tooltip("Name of the objective that will be shown on screen")]
        public string Title;

        [Tooltip("Short text explaining the objective that will be shown on screen")]
        public string Description;

        [Tooltip("Whether the objective is required to win or not")]
        public bool IsOptional;

        [Tooltip("Delay before the objective becomes visible")]
        public float DelayVisible;

        public bool IsCompleted { get; protected set; }
        public bool IsBlocking() => !(IsOptional || IsCompleted);

        protected EventManager m_EventManager;
        protected ObjectiveManager m_ObjectiveManager;

        protected virtual void Start()
        {
            m_EventManager = transform.root.GetComponentInChildren<EventManager>();
            DebugUtility.HandleErrorIfNullFindObject<EventManager, Objective>(m_EventManager, this);

            m_ObjectiveManager = transform.root.GetComponentInChildren<ObjectiveManager>();
            DebugUtility.HandleErrorIfNullFindObject<ObjectiveManager, Objective>(m_ObjectiveManager, this);

            // Register with the local ObjectiveManager instead of a global static event.
            m_ObjectiveManager.RegisterObjective(this);

            DisplayMessageEvent displayMessage = Events.DisplayMessageEvent;
            displayMessage.Message = Title;
            displayMessage.DelayBeforeDisplay = 0.0f;
            m_EventManager.Broadcast(displayMessage);
        }

        public void UpdateObjective(string descriptionText, string counterText, string notificationText)
        {
            ObjectiveUpdateEvent evt = Events.ObjectiveUpdateEvent;
            evt.Objective = this;
            evt.DescriptionText = descriptionText;
            evt.CounterText = counterText;
            evt.NotificationText = notificationText;
            evt.IsComplete = IsCompleted;
            m_EventManager.Broadcast(evt);
        }

        public void CompleteObjective(string descriptionText, string counterText, string notificationText)
        {
            IsCompleted = true;

            ObjectiveUpdateEvent evt = Events.ObjectiveUpdateEvent;
            evt.Objective = this;
            evt.DescriptionText = descriptionText;
            evt.CounterText = counterText;
            evt.NotificationText = notificationText;
            evt.IsComplete = IsCompleted;
            m_EventManager.Broadcast(evt);

            // Notify the local ObjectiveManager instead of a global static event.
            m_ObjectiveManager?.NotifyObjectiveCompleted(this);
        }

        /// <summary>
        /// Resets this objective to its initial uncompleted state.
        /// Override in subclasses to reset additional counters or state.
        /// </summary>
        public virtual void ResetObjective()
        {
            IsCompleted = false;
        }
    }
}
