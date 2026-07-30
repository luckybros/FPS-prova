using UnityEngine;
using Unity.FPS.Game;
using Unity.FPS.AI;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Collections.Generic;

namespace Unity.FPS.Gameplay
{
    public class PlayerAgent : Agent
    {
        const float HitReward = 0.1f;
        const float KillReward = 1.0f;

        private PlayerCharacterController playerCharacterController;
        private PlayerInputHandler playerInputHandler;
        private Health m_PlayerHealth;
        private PlayerWeaponsManager m_WeaponsManager;
        private Vector3 startPosition;

        private readonly List<Health> m_EnemyHealthComponents = new List<Health>();

        void Start()
        {
            playerCharacterController = GetComponent<PlayerCharacterController>();
            playerInputHandler = GetComponent<PlayerInputHandler>();
            startPosition = transform.localPosition;

            m_PlayerHealth = GetComponent<Health>();
            m_PlayerHealth.OnDamaged += OnDamage;
            m_PlayerHealth.OnHealed += OnHeal;

            m_WeaponsManager = GetComponent<PlayerWeaponsManager>();

            // Subscribe to each enemy's damage and death events within this environment.
            foreach (var enemy in transform.root.GetComponentsInChildren<EnemyController>(true))
            {
                var enemyHealth = enemy.GetComponent<Health>();
                if (enemyHealth == null)
                    continue;

                enemyHealth.OnDamaged += OnEnemyDamaged;
                enemyHealth.OnDie += OnEnemyDied;
                m_EnemyHealthComponents.Add(enemyHealth);
            }
        }

        void OnDestroy()
        {
            if (m_PlayerHealth != null)
            {
                m_PlayerHealth.OnDamaged -= OnDamage;
                m_PlayerHealth.OnHealed -= OnHeal;
            }

            foreach (var enemyHealth in m_EnemyHealthComponents)
            {
                if (enemyHealth == null)
                    continue;
                enemyHealth.OnDamaged -= OnEnemyDamaged;
                enemyHealth.OnDie -= OnEnemyDied;
            }
            m_EnemyHealthComponents.Clear();
        }

        public override void OnEpisodeBegin()
        {
            Debug.Log($"On Episode Begin?");
            GetComponent<PlayerInputHandler>().SetSpeedBugMultiplier(1f);
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            sensor.AddObservation(transform.localPosition.x);
            sensor.AddObservation(transform.localPosition.z);
            var activeWeapon = m_WeaponsManager.GetActiveWeapon();
            float ammoRatio = activeWeapon != null ? activeWeapon.CurrentAmmoRatio : 0f;
            sensor.AddObservation(ammoRatio);
            sensor.AddObservation(playerCharacterController.m_Health.GetRatio());
        }

        public void MoveAgent(ActionSegment<int> act)
        {
            var moveForward = act[0];
            var moveSideways = act[1];
            var turn = act[2];
            var shoot = act[3];

            switch (moveForward)
            {
                case 0:
                    playerInputHandler.m_MoveForward = 0;
                    break;
                case 1:
                    playerInputHandler.m_MoveForward = 1;
                    break;
                case 2:
                    playerInputHandler.m_MoveForward = -1;
                    break;
            }

            switch (moveSideways)
            {
                case 0:
                    playerInputHandler.m_MoveSideways = 0;
                    break;
                case 1:
                    playerInputHandler.m_MoveSideways = 1;
                    break;
                case 2:
                    playerInputHandler.m_MoveSideways = -1;
                    break;
            }

            switch (turn)
            {
                case 0:
                    playerInputHandler.m_Turn = 0;
                    break;
                case 1:
                    playerInputHandler.m_Turn = 1;
                    break;
                case 2:
                    playerInputHandler.m_Turn = -1;
                    break;
            }

            switch (shoot)
            {
                case 0:
                    playerInputHandler.m_Shoot = false;
                    break;
                case 1:
                    playerInputHandler.m_Shoot = true;
                    break;
            }
        }

        public override void OnActionReceived(ActionBuffers actionBuffers)
        {
            MoveAgent(actionBuffers.DiscreteActions);
        }

        public override void Heuristic(in ActionBuffers actionsOut)
        {
            var discreteActionsOut = actionsOut.DiscreteActions;

            if (Input.GetKey(KeyCode.W))
            {
                discreteActionsOut[0] = 1;
            }
            else if (Input.GetKey(KeyCode.S))
            {
                discreteActionsOut[0] = 2;
            }
            else 
            {
                discreteActionsOut[0] = 0;
            }

            if (Input.GetKey(KeyCode.D))
            {
                discreteActionsOut[1] = 1;
            }
            else if (Input.GetKey(KeyCode.A))
            {
                discreteActionsOut[1] = 2;
            }
            else 
            {
                discreteActionsOut[1] = 0;
            }

            if (Input.GetKey(KeyCode.E))
            {
                discreteActionsOut[2] = 1;
            }
            else if (Input.GetKey(KeyCode.Q))
            {
                discreteActionsOut[2] = 2;
            }
            else 
            {
                discreteActionsOut[2] = 0;
            }

            if (Input.GetMouseButton(0))
            {
                discreteActionsOut[3] = 1;
            }
            else
            {
                discreteActionsOut[3] = 0;
            }
        }

        private void OnDamage(float damage, GameObject damageSource)
        {
            Debug.Log("Player Damaged");
            AddReward(-0.1f);
        }

        private void OnHeal(float healAmount)
        {
            Debug.Log("Player Health");
            AddReward(0.1f);
        }

        private void OnEnemyDamaged(float damage, GameObject damageSource)
        {
            if (damageSource != null && damageSource.CompareTag("Player"))
            {
                Debug.Log("Enemy Damaged");
                AddReward(HitReward);
            }
        }

        private void OnEnemyDied()
        {
            Debug.Log("Enemy Killed");
            AddReward(KillReward);
        }

    }
}
