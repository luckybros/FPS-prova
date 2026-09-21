using System.Collections;
using System.Collections.Generic;
using Unity.FPS.AI;
using Unity.FPS.Gameplay;
using UnityEngine;

namespace Unity.FPS.Game
{
    /// <summary>
    /// Performs a full in-place environment reset on player death, avoiding scene reloads.
    /// All discovery is scoped to transform.root, so multiple instances of the environment
    /// prefab can run in parallel without cross-contamination.
    /// The reset is deferred by one frame so that PlayerCharacterController.OnDie() fully
    /// completes (IsDead = true, weapon lowered) before the reset overrides that state.
    /// </summary>
    public class EnvironmentResetManager : MonoBehaviour
    {
        [Tooltip("List of possible spawn points for the player. If empty, the player's initial position is used.")]
        public Transform[] SpawnPoints;

        struct EnemySpawnData
        {
            public EnemyController Enemy;
            public Vector3 InitialPosition;
            public Quaternion InitialRotation;
        }

        struct PickupSpawnData
        {
            public Pickup Pickup;
            public Vector3 InitialPosition;
        }

        EventManager m_EventManager;
        EnemyManager m_EnemyManager;
        ObjectiveManager m_ObjectiveManager;
        PlayerCharacterController m_Player;
        Health m_PlayerHealth;
        Vector3 m_PlayerSpawnPosition;
        Quaternion m_PlayerSpawnRotation;

        readonly List<EnemySpawnData> m_EnemySpawnData = new List<EnemySpawnData>();
        readonly List<PickupSpawnData> m_PickupSpawnData = new List<PickupSpawnData>();

        [SerializeField] int m_EnvironmentId = 0;

        void Awake()
        {
            Transform root = transform.root;

            m_EventManager = root.GetComponentInChildren<EventManager>();
            DebugUtility.HandleErrorIfNullFindObject<EventManager, GameFlowManager>(m_EventManager, this);

            m_EnemyManager = root.GetComponentInChildren<EnemyManager>();
            DebugUtility.HandleErrorIfNullFindObject<EnemyManager, EnvironmentResetManager>(m_EnemyManager, this);

            m_ObjectiveManager = root.GetComponentInChildren<ObjectiveManager>();
            DebugUtility.HandleErrorIfNullFindObject<ObjectiveManager, EnvironmentResetManager>(m_ObjectiveManager, this);

            m_Player = root.GetComponentInChildren<PlayerCharacterController>();
            DebugUtility.HandleErrorIfNullFindObject<PlayerCharacterController, EnvironmentResetManager>(m_Player, this);

            m_PlayerHealth = m_Player.GetComponent<Health>();
            DebugUtility.HandleErrorIfNullFindObject<Health, EnvironmentResetManager>(m_PlayerHealth, this);

            m_PlayerSpawnPosition = m_Player.transform.position;
            m_PlayerSpawnRotation = m_Player.transform.rotation;

            // Auto-populate SpawnPoints from a child container named "SpawnPoints" if not assigned manually.
            if (SpawnPoints == null || SpawnPoints.Length == 0)
            {
                Transform container = transform.Find("SpawnPoints");
                if (container != null && container.childCount > 0)
                {
                    SpawnPoints = new Transform[container.childCount];
                    for (int i = 0; i < container.childCount; i++)
                        SpawnPoints[i] = container.GetChild(i);
                }
            }

            m_EventManager.AddListener<AllObjectivesCompletedEvent>(OnPlayerWin);
            m_EventManager.AddListener<PlayerDeathEvent>(OnPlayerLose);

            OracleSideChannel.OnResetReceived += OnResetSignalReceived;

            foreach (var enemy in root.GetComponentsInChildren<EnemyController>(true))
            {
                m_EnemySpawnData.Add(new EnemySpawnData
                {
                    Enemy = enemy,
                    InitialPosition = enemy.transform.position,
                    InitialRotation = enemy.transform.rotation
                });
            }

            foreach (var pickup in root.GetComponentsInChildren<Pickup>(true))
            {
                m_PickupSpawnData.Add(new PickupSpawnData
                {
                    Pickup = pickup,
                    InitialPosition = pickup.transform.position
                });
            }
        }

        void OnPlayerWin(AllObjectivesCompletedEvent evt)
        {
            Debug.Log("OOOOOO ho vinto");
            StartCoroutine(DeferredReset());
        }

        void OnPlayerLose(PlayerDeathEvent evt)
        {
            Debug.Log("OOOOOO ho perso");
            StartCoroutine(DeferredReset());
        }

        void OnResetSignalReceived(int envId)
        {
            if (envId == m_EnvironmentId)
                StartCoroutine(DeferredReset());
        }

        IEnumerator DeferredReset()
        {
            // Wait one frame: PlayerCharacterController.OnDie() sets IsDead=true
            // and lowers the weapon. Only after that the reset can correctly override.
            yield return null;
            ResetEnvironment();
        }

        /// <summary>
        /// Resets player, enemies and pickups to their initial state.
        /// Called automatically one frame after this environment's player dies.
        /// </summary>
        public void ResetEnvironment()
        {
            m_ObjectiveManager.ResetObjectives();

            m_EnemyManager.ResetEnemyList();

            foreach (var data in m_EnemySpawnData)
            {
                if (data.Enemy != null)
                    data.Enemy.ResetEnemy(data.InitialPosition, data.InitialRotation);
            }

            foreach (var data in m_PickupSpawnData)
            {
                if (data.Pickup != null)
                    data.Pickup.ResetPickup(data.InitialPosition);
            }

            Vector3 spawnPos = m_PlayerSpawnPosition;
            Quaternion spawnRot = m_PlayerSpawnRotation;

            if (SpawnPoints != null && SpawnPoints.Length > 0)
            {
                Transform chosen = SpawnPoints[Random.Range(0, SpawnPoints.Length)];
                spawnPos = chosen.position;
                spawnRot = chosen.rotation;
            }

            m_Player.ResetPlayer(spawnPos, spawnRot);
        }

        void OnDestroy()
        {
            if (m_EventManager != null)
            {
                m_EventManager.RemoveListener<AllObjectivesCompletedEvent>(OnPlayerWin);
                m_EventManager.RemoveListener<PlayerDeathEvent>(OnPlayerLose);
            }
        }
    }
}
