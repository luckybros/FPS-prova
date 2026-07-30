using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.Gameplay
{
    [RequireComponent(typeof(Rigidbody), typeof(Collider))]
    public class Pickup : MonoBehaviour
    {
        [Tooltip("Frequency at which the item will move up and down")]
        public float VerticalBobFrequency = 1f;

        [Tooltip("Distance the item will move up and down")]
        public float BobbingAmount = 1f;

        [Tooltip("Rotation angle per second")] public float RotatingSpeed = 360f;

        [Tooltip("Sound played on pickup")] public AudioClip PickupSfx;
        [Tooltip("VFX spawned on pickup")] public GameObject PickupVfxPrefab;

        public Rigidbody PickupRigidbody { get; private set; }

        Collider m_Collider;
        Vector3 m_StartPosition;
        bool m_HasPlayedFeedback;
        EnvironmentResetManager m_ResetManager;

        /// <summary>Local environment event bus. Available to subclasses for broadcasting pickup events.</summary>
        protected EventManager m_EventManager;

        protected virtual void Start()
        {
            PickupRigidbody = GetComponent<Rigidbody>();
            DebugUtility.HandleErrorIfNullGetComponent<Rigidbody, Pickup>(PickupRigidbody, this, gameObject);
            m_Collider = GetComponent<Collider>();
            DebugUtility.HandleErrorIfNullGetComponent<Collider, Pickup>(m_Collider, this, gameObject);

            PickupRigidbody.isKinematic = true;
            m_Collider.isTrigger = true;

            m_StartPosition = transform.position;
            m_ResetManager = transform.root.GetComponentInChildren<EnvironmentResetManager>();
            m_EventManager = transform.root.GetComponentInChildren<EventManager>();
            DebugUtility.HandleErrorIfNullFindObject<EventManager, Pickup>(m_EventManager, this);
        }

        void Update()
        {
            float bobbingAnimationPhase = ((Mathf.Sin(Time.time * VerticalBobFrequency) * 0.5f) + 0.5f) * BobbingAmount;
            transform.position = m_StartPosition + Vector3.up * bobbingAnimationPhase;

            transform.Rotate(Vector3.up, RotatingSpeed * Time.deltaTime, Space.Self);
        }

        void OnTriggerEnter(Collider other)
        {
            PlayerCharacterController pickingPlayer = other.GetComponent<PlayerCharacterController>();

            if (pickingPlayer != null)
            {
                OnPicked(pickingPlayer);

                PickupEvent evt = Events.PickupEvent;
                evt.Pickup = gameObject;
                m_EventManager.Broadcast(evt);
            }
        }

        protected virtual void OnPicked(PlayerCharacterController playerController)
        {
            PlayPickupFeedback();
        }

        public void PlayPickupFeedback()
        {
            if (m_HasPlayedFeedback)
                return;

            if (PickupSfx)
                AudioUtility.CreateSFX(PickupSfx, transform.position, AudioUtility.AudioGroups.Pickup, 0f);

            if (PickupVfxPrefab)
                Instantiate(PickupVfxPrefab, transform.position, Quaternion.identity);

            m_HasPlayedFeedback = true;
        }

        /// <summary>Disables the pickup in RL mode instead of destroying it, so it can be respawned next episode.</summary>
        protected void HandlePickedUp()
        {
            if (m_ResetManager != null)
                gameObject.SetActive(false);
            else
                Destroy(gameObject);
        }

        /// <summary>Restores the pickup to its initial position and state for RL episode reset.</summary>
        public void ResetPickup(Vector3 initialPosition)
        {
            m_HasPlayedFeedback = false;
            m_StartPosition = initialPosition;
            transform.position = initialPosition;
            gameObject.SetActive(true);
        }
    }
}
