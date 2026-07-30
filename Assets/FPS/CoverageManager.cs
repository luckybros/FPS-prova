using UnityEngine;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.FPS.Game;
using System.IO;

namespace Unity.FPS.Gameplay
{
    public class CoverageManager : MonoBehaviour
    {
        public float cellSize = 5.0f;

        [Header("Player reference")]
        public GameObject player;

        [Header("Enemies references")]
        public GameObject[] enemies;

        private static HashSet<GameState> visitedStates = new HashSet<GameState>();

        private bool isFirstFrame = true;
        private GameState lastState;

        private StatsRecorder statsRecorder;

        private static readonly object fileLock = new object();

        private static string statesPath;
        private static bool isLoaded = false;

        private void Awake()
        {
            statsRecorder = Academy.Instance.StatsRecorder;

            statesPath = Path.Combine(Application.persistentDataPath, "./GameStatesLog.txt");

            lock (fileLock)
            {
                if (!isLoaded)
                {
                    LoadStatesFromDisk();
                    isLoaded = true;
                }
            }
        }

        public GameState GetCurrentState()
        {
            return lastState;
        }

        void FixedUpdate()
        {
            GameState currentState = CaptureGameState();

            if (!isFirstFrame && currentState.Equals(lastState))
                return;

            lastState = currentState;
            if (isFirstFrame) isFirstFrame = false;

            bool newState = visitedStates.Add(lastState);

            if (newState)
            {
                lock (fileLock)
                {
                    File.AppendAllText(statesPath, lastState.ToString() + "\n");
                }

                statsRecorder.Add("Coverage/StateCoverage", visitedStates.Count);
            }
        }

        private GameState CaptureGameState()
        {
            PlayerState playerState = new PlayerState(
                player.transform,
                player.GetComponent<PlayerWeaponsManager>().GetActiveWeapon().CurrentAmmoRatio,
                player.GetComponent<Health>().GetRatio(),
                cellSize
            );

            EnemyState[] enemiesStates = new EnemyState[enemies.Length];

            for (int i = 0; i < enemies.Length; i++)
            {
                GameObject enemy = enemies[i];

                enemiesStates[i] = new EnemyState(
                    enemy.transform,
                    enemy.GetComponent<Health>().GetRatio(),
                    cellSize
                );
            }

            return new GameState(playerState, enemiesStates);
        }

        private void LoadStatesFromDisk()
        {
            if (File.Exists(statesPath))
            {
                foreach(string line in File.ReadLines(statesPath))
                {
                    if (!string.IsNullOrWhiteSpace(line)) visitedStates.Add(GameState.Parse(line, enemies.Length));
                }
            }
        }
    }
}