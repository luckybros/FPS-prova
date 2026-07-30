using UnityEngine;

namespace Unity.FPS.Gameplay
{
    public class FaultManager : MonoBehaviour
    {
        public static FaultManager Instance { get; private set; }

        [SerializeField] private FaultConfig config = new FaultConfig();

        public FaultConfig Config => config;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
}