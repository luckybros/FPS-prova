using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.FPS.Game
{
    public class GameEvent { }

    /// <summary>
    /// Per-environment event bus. Place this component on the root GameObject of each environment
    /// so that all child components can locate it via transform.root.GetComponentInChildren.
    /// Replaces the previous static implementation to isolate events between parallel RL environments.
    /// </summary>
    public class EventManager : MonoBehaviour
    {
        readonly Dictionary<Type, Action<GameEvent>> m_Events = new Dictionary<Type, Action<GameEvent>>();
        readonly Dictionary<Delegate, Action<GameEvent>> m_EventLookups = new Dictionary<Delegate, Action<GameEvent>>();

        /// <summary>Registers a typed listener on this environment's event bus.</summary>
        public void AddListener<T>(Action<T> evt) where T : GameEvent
        {
            if (!m_EventLookups.ContainsKey(evt))
            {
                Action<GameEvent> newAction = (e) => evt((T)e);
                m_EventLookups[evt] = newAction;

                if (m_Events.TryGetValue(typeof(T), out Action<GameEvent> internalAction))
                    m_Events[typeof(T)] = internalAction += newAction;
                else
                    m_Events[typeof(T)] = newAction;
            }
        }

        /// <summary>Removes a previously registered listener.</summary>
        public void RemoveListener<T>(Action<T> evt) where T : GameEvent
        {
            if (m_EventLookups.TryGetValue(evt, out var action))
            {
                if (m_Events.TryGetValue(typeof(T), out var tempAction))
                {
                    tempAction -= action;
                    if (tempAction == null)
                        m_Events.Remove(typeof(T));
                    else
                        m_Events[typeof(T)] = tempAction;
                }

                m_EventLookups.Remove(evt);
            }
        }

        /// <summary>Broadcasts an event only to listeners registered on this environment's bus.</summary>
        public void Broadcast(GameEvent evt)
        {
            if (m_Events.TryGetValue(evt.GetType(), out var action))
                action.Invoke(evt);
        }

        /// <summary>Removes all listeners. Call between episodes if needed.</summary>
        public void Clear()
        {
            m_Events.Clear();
            m_EventLookups.Clear();
        }
    }
}
