using Unity.FPS.Game;
using Unity.FPS.Gameplay;
using UnityEngine;

namespace Unity.FPS.UI
{
    public class NotificationHUDManager : MonoBehaviour
    {
        [Tooltip("UI panel containing the layoutGroup for displaying notifications")]
        public RectTransform NotificationPanel;

        [Tooltip("Prefab for the notifications")]
        public GameObject NotificationPrefab;

        EventManager m_EventManager;

        void Awake()
        {
            PlayerWeaponsManager playerWeaponsManager = transform.root.GetComponentInChildren<PlayerWeaponsManager>();
            DebugUtility.HandleErrorIfNullFindObject<PlayerWeaponsManager, NotificationHUDManager>(playerWeaponsManager,
                this);
            playerWeaponsManager.OnAddedWeapon += OnPickupWeapon;

            Jetpack jetpack = transform.root.GetComponentInChildren<Jetpack>();
            DebugUtility.HandleErrorIfNullFindObject<Jetpack, NotificationHUDManager>(jetpack, this);
            jetpack.OnUnlockJetpack += OnUnlockJetpack;

            m_EventManager = transform.root.GetComponentInChildren<EventManager>();
            DebugUtility.HandleErrorIfNullFindObject<EventManager, NotificationHUDManager>(m_EventManager, this);
            m_EventManager.AddListener<ObjectiveUpdateEvent>(OnObjectiveUpdateEvent);
        }

        void OnObjectiveUpdateEvent(ObjectiveUpdateEvent evt)
        {
            if (!string.IsNullOrEmpty(evt.NotificationText))
                CreateNotification(evt.NotificationText);
        }

        void OnPickupWeapon(WeaponController weaponController, int index)
        {
            if (index != 0)
                CreateNotification("Picked up weapon : " + weaponController.WeaponName);
        }

        void OnUnlockJetpack(bool unlock)
        {
            CreateNotification("Jetpack unlocked");
        }

        public void CreateNotification(string text)
        {
            GameObject notificationInstance = Instantiate(NotificationPrefab, NotificationPanel);
            notificationInstance.transform.SetSiblingIndex(0);

            NotificationToast toast = notificationInstance.GetComponent<NotificationToast>();
            if (toast)
                toast.Initialize(text);
        }

        void OnDestroy()
        {
            if (m_EventManager != null)
                m_EventManager.RemoveListener<ObjectiveUpdateEvent>(OnObjectiveUpdateEvent);
        }
    }
}