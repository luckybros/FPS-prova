using System.Collections.Generic;
using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.AI
{
    public class EnemyManager : MonoBehaviour
    {
        public List<EnemyController> Enemies { get; private set; }
        public int NumberOfEnemiesTotal { get; private set; }
        public int NumberOfEnemiesRemaining => Enemies.Count;

        EventManager m_EventManager;

        void Awake()
        {
            Enemies = new List<EnemyController>();
            m_EventManager = transform.root.GetComponentInChildren<EventManager>();
            DebugUtility.HandleErrorIfNullFindObject<EventManager, EnemyManager>(m_EventManager, this);
        }

        public void RegisterEnemy(EnemyController enemy)
        {
            Enemies.Add(enemy);
            NumberOfEnemiesTotal++;
        }

        public void UnregisterEnemy(EnemyController enemyKilled)
        {
            int enemiesRemainingNotification = NumberOfEnemiesRemaining - 1;

            EnemyKillEvent evt = Events.EnemyKillEvent;
            evt.Enemy = enemyKilled.gameObject;
            evt.RemainingEnemyCount = enemiesRemainingNotification;

            // Broadcast only to listeners within this environment.
            m_EventManager.Broadcast(evt);

            Enemies.Remove(enemyKilled);
        }

        /// <summary>Clears the enemy list and resets the total count before RL episode re-registration.</summary>
        public void ResetEnemyList()
        {
            Enemies.Clear();
            NumberOfEnemiesTotal = 0;
        }
    }
}