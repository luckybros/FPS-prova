using UnityEngine;
using System.Collections.Generic;

namespace Unity.FPS.Gameplay
{
    /// <summary>
    /// Checks if the state of the game is stuck
    /// checking the characters positions.
    /// </summary>
    public class StuckOracle : OracleBase
    {
        protected override string OracleName => "STUCK ORACLE";
        
        [Header("Characters references")]
        [Tooltip("Characters  list to keep track of")]
        public List<Transform> characters = new List<Transform>();
        // private CoverageManager coverageManager;

        // private GlobalGameState averageState;

        [Header("Settings")]
        [Tooltip("Time interval between a sampling and another one")]
        private float interval = 1.0f;
        [Tooltip("Number of sample in history")]
        private int historySize = 20;
        [Tooltip("Threshold between states")]
        public float stuckThreshold = 0.1f;

        [Header("Raycast Settings")]
        [Tooltip("Length of the rays to check around the tank")]
        public float checkRadius = 3.0f;
        [Tooltip("Height offset from the tank base to shoot rays (prevents hitting the floor)")]
        public float rayHeightOffset = 0.5f;

        [Header("Grid Settings for Bug Report")]
        [Tooltip("Size of the cell to discretize the position in the report")]
        public float cellSize = 5.0f; // <-- Aggiunto questo per la tua cell size di 5
        private List<Queue<Vector2>> positionHistories = new List<Queue<Vector2>>();
        private List<PlayerInputHandler> playerMovements = new List <PlayerInputHandler>();
        private float timer = 0f;
        
        private void Start()
        {
            // Debug.Log($"Start Stuck oracle");
            for (int i = 0; i < characters.Count; i++)
            {
                positionHistories.Add(new Queue<Vector2>());
                playerMovements.Add(characters[i].GetComponent<PlayerInputHandler>());
            }
        }
        
        private void Update()
        {
            timer += Time.deltaTime;

            if (timer < interval) return;
            
            timer = 0f;

            // Check if every player is stuck
            for (int i = 0; i < characters.Count; i++)
            {
                if (characters[i] == null) continue;

                Vector2 currentPos = new Vector2(characters[i].localPosition.x, characters[i].localPosition.z);
                var history = positionHistories[i];

                history.Enqueue(currentPos);
                if (history.Count > historySize)
                {
                    history.Dequeue();
                }

                // check if the tank movement is taking an input, if is not moving per scelta non c'è bisogno
                bool isTryingToMove = playerMovements[i].IsMoving() || playerMovements[i].IsTurning();

                if (history.Count >= historySize && isTryingToMove && IsStuck(currentPos, history))
                {
                    Vector2Int discretePos = DiscretizePosition(currentPos);

                    Debug.Log($"Stuck!");
                    ReportBug(
                        $"stuck_{discretePos.x}_{discretePos.y}", 
                        $"Character {i} stuck: the position doesn't change. " +
                        $"Exact position: {currentPos}. " +
                        $"Discrete Cell: {discretePos}" 
                    );
                }
            }
        }

        private bool IsStuck(Vector2 currentPos, Queue<Vector2> history)
        {
            Vector2 sum = Vector2.zero;

            foreach (Vector2 pos in history)
            {
                sum += pos;
            }

            Vector2 average = sum / history.Count;

            return Vector2.Distance(currentPos, average) < stuckThreshold;
        }

        private Vector2Int DiscretizePosition(Vector2 pos)
        {
            int cellX = Mathf.FloorToInt(pos.x / 10);
            int cellY = Mathf.FloorToInt(pos.y / 10); // Ricorda che y qui deriva dalla z (localPosition.z)
            
            return new Vector2Int(cellX, cellY);
        }

        private string GetActiveBuggerName(Transform tankTransform)
        {
            int numberOfRays = 8;
            
            // Solleviamo leggermente il punto di origine così i raggi partono dal centro del carro
            // e non grattano sul pavimento
            Vector3 origin = tankTransform.position + (Vector3.up * rayHeightOffset);

            for (int i = 0; i < numberOfRays; i++)
            {
                // Calcola l'angolo per sparare a cerchio (360 gradi / 8)
                float angle = i * Mathf.PI * 2 / numberOfRays;
                Vector3 direction = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));

                // Disegna il raggio nella tab "Scene" per 2 secondi per farti fare debugging visivo
                Debug.DrawRay(origin, direction * checkRadius, Color.red, 2.0f);

                // Lancia il raggio!
                if (Physics.Raycast(origin, direction, out RaycastHit hit, checkRadius, Physics.AllLayers, QueryTriggerInteraction.Collide))
                {
                    // Se il raggio colpisce un oggetto con lo script StuckBugger
                    if (hit.collider.TryGetComponent<StuckBugger>(out StuckBugger bugger))
                    {
                        return bugger.gameObject.name;
                    }
                }
            }

            // Se nessuno degli 8 raggi ha colpito un Bugger, facciamo un ultimissimo tentativo al centro
            // casomai il tank fosse ESATTAMENTE sopra il trigger e i raggi sparano in fuori
            Collider[] hitColliders = Physics.OverlapSphere(origin, 0.5f, Physics.AllLayers, QueryTriggerInteraction.Collide);
            foreach (Collider col in hitColliders)
            {
                if (col.TryGetComponent<StuckBugger>(out StuckBugger bugger))
                {
                    return bugger.gameObject.name;
                }
            }

            return "Unknown";
        }
        
    }
}