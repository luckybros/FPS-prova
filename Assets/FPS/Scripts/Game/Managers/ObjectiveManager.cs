using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.FPS.Game
{
    public class ObjectiveManager : MonoBehaviour
    {
        /// <summary>Fired when an Objective registers with this manager. Used by the local HUD.</summary>
        public event Action<Objective> OnObjectiveCreated;

        /// <summary>Fired when an Objective calls CompleteObjective. Used by the local HUD.</summary>
        public event Action<Objective> OnObjectiveCompleted;

        List<Objective> m_Objectives = new List<Objective>();
        bool m_ObjectivesCompleted = false;
        EventManager m_EventManager;

        void Awake()
        {
            m_EventManager = transform.root.GetComponentInChildren<EventManager>();
            DebugUtility.HandleErrorIfNullFindObject<EventManager, ObjectiveManager>(m_EventManager, this);
        }

        /// <summary>
        /// Called by each Objective on Start to register with this environment's manager.
        /// Also notifies the local HUD via OnObjectiveCreated.
        /// </summary>
        public void RegisterObjective(Objective objective)
        {
            m_Objectives.Add(objective);
            OnObjectiveCreated?.Invoke(objective);
        }

        /// <summary>
        /// Called by Objective.CompleteObjective to notify the local HUD.
        /// </summary>
        public void NotifyObjectiveCompleted(Objective objective)
        {
            OnObjectiveCompleted?.Invoke(objective);
        }

        /// <summary>
        /// Resets all registered objectives and clears the completion flag.
        /// Called by EnvironmentResetManager on each environment reset.
        /// </summary>
        public void ResetObjectives()
        {
            m_ObjectivesCompleted = false;
            foreach (var objective in m_Objectives)
                objective.ResetObjective();
        }

        void Update()
        {
            if (m_Objectives.Count == 0 || m_ObjectivesCompleted)
                return;

            for (int i = 0; i < m_Objectives.Count; i++)
            {
                // Stop as soon as we find one uncompleted blocking objective.
                if (m_Objectives[i].IsBlocking())
                    return;
            }

            m_ObjectivesCompleted = true;
            m_EventManager.Broadcast(Events.AllObjectivesCompletedEvent);
        }
    }
}
