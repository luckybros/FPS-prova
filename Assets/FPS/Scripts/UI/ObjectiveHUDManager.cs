using System.Collections.Generic;
using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.UI
{
    public class ObjectiveHUDManager : MonoBehaviour
    {
        [Tooltip("UI panel containing the layoutGroup for displaying objectives")]
        public RectTransform ObjectivePanel;

        [Tooltip("Prefab for the primary objectives")]
        public GameObject PrimaryObjectivePrefab;

        [Tooltip("Prefab for the primary objectives")]
        public GameObject SecondaryObjectivePrefab;

        Dictionary<Objective, ObjectiveToast> m_ObjectivesDictionnary;
        ObjectiveManager m_ObjectiveManager;
        EventManager m_EventManager;

        void Awake()
        {
            m_ObjectivesDictionnary = new Dictionary<Objective, ObjectiveToast>();

            m_EventManager = transform.root.GetComponentInChildren<EventManager>();
            DebugUtility.HandleErrorIfNullFindObject<EventManager, ObjectiveHUDManager>(m_EventManager, this);
            m_EventManager.AddListener<ObjectiveUpdateEvent>(OnUpdateObjective);

            // Subscribe to instance events on the local ObjectiveManager instead of global static events.
            m_ObjectiveManager = transform.root.GetComponentInChildren<ObjectiveManager>();
            DebugUtility.HandleErrorIfNullFindObject<ObjectiveManager, ObjectiveHUDManager>(m_ObjectiveManager, this);
            m_ObjectiveManager.OnObjectiveCreated += RegisterObjective;
            m_ObjectiveManager.OnObjectiveCompleted += UnregisterObjective;
        }

        public void RegisterObjective(Objective objective)
        {
            GameObject objectiveUIInstance =
                Instantiate(objective.IsOptional ? SecondaryObjectivePrefab : PrimaryObjectivePrefab, ObjectivePanel);

            if (!objective.IsOptional)
                objectiveUIInstance.transform.SetSiblingIndex(0);

            ObjectiveToast toast = objectiveUIInstance.GetComponent<ObjectiveToast>();
            DebugUtility.HandleErrorIfNullGetComponent<ObjectiveToast, ObjectiveHUDManager>(toast, this,
                objectiveUIInstance.gameObject);

            toast.Initialize(objective.Title, objective.Description, "", objective.IsOptional, objective.DelayVisible);

            m_ObjectivesDictionnary.Add(objective, toast);

            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(ObjectivePanel);
        }

        public void UnregisterObjective(Objective objective)
        {
            if (m_ObjectivesDictionnary.TryGetValue(objective, out ObjectiveToast toast) && toast != null)
                toast.Complete();

            m_ObjectivesDictionnary.Remove(objective);
        }

        void OnUpdateObjective(ObjectiveUpdateEvent evt)
        {
            if (m_ObjectivesDictionnary.TryGetValue(evt.Objective, out ObjectiveToast toast) && toast != null)
            {
                Canvas.ForceUpdateCanvases();
                if (!string.IsNullOrEmpty(evt.DescriptionText))
                    toast.DescriptionTextContent.text = evt.DescriptionText;

                if (!string.IsNullOrEmpty(evt.CounterText))
                    toast.CounterTextContent.text = evt.CounterText;

                if (toast.GetComponent<RectTransform>())
                    UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(toast.GetComponent<RectTransform>());
            }
        }

        void OnDestroy()
        {
            if (m_EventManager != null)
                m_EventManager.RemoveListener<ObjectiveUpdateEvent>(OnUpdateObjective);

            if (m_ObjectiveManager != null)
            {
                m_ObjectiveManager.OnObjectiveCreated -= RegisterObjective;
                m_ObjectiveManager.OnObjectiveCompleted -= UnregisterObjective;
            }
        }
    }
}
