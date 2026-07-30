using Unity.FPS.Game;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Unity.FPS.Gameplay
{
    public class PlayerInputHandler : MonoBehaviour
    {
        [Tooltip("Sensitivity multiplier for moving the camera around")]
        public float LookSensitivity = 1f;

        [Tooltip("Additional sensitivity multiplier for WebGL")]
        public float WebglLookSensitivityMultiplier = 0.25f;

        [Tooltip("Limit to consider an input when using a trigger on a controller")]
        public float TriggerAxisThreshold = 0.4f;

        [Tooltip("Used to flip the vertical input axis")]
        public bool InvertYAxis = false;

        [Tooltip("Used to flip the horizontal input axis")]
        public bool InvertXAxis = false;

        GameFlowManager m_GameFlowManager;
        PlayerCharacterController m_PlayerCharacterController;
        bool m_FireInputWasHeld;

        private InputAction m_MoveAction;
        private PlayerInput m_PlayerInput;

        [HideInInspector] public int m_MoveForward;
        [HideInInspector] public int m_MoveSideways;
        public bool IsMoving() => Mathf.Abs(m_MoveForward) > 0.1f || Mathf.Abs(m_MoveSideways) > 0.1;
        [HideInInspector] public int m_Turn;
        public bool IsTurning() => Mathf.Abs(m_Turn) > 0.1f;
        [HideInInspector] public bool m_Shoot;
        public float horizontalTurnSpeed;
        private float stuckBugMultiplier = 1f;

        void Start()
        {
            m_PlayerCharacterController = GetComponent<PlayerCharacterController>();
            DebugUtility.HandleErrorIfNullGetComponent<PlayerCharacterController, PlayerInputHandler>(
                m_PlayerCharacterController, this, gameObject);
            m_GameFlowManager = FindFirstObjectByType<GameFlowManager>();
            DebugUtility.HandleErrorIfNullFindObject<GameFlowManager, PlayerInputHandler>(m_GameFlowManager, this);

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            m_MoveForward = 0;
            m_MoveSideways = 0;
        }

        void LateUpdate()
        {
            m_FireInputWasHeld = GetFireInputHeld();
        }

        public bool CanProcessInput()
        {
            return Cursor.lockState == CursorLockMode.Locked && !m_GameFlowManager.GameIsEnding;
        }

        public Vector3 GetMoveInput()
        {
            if (CanProcessInput())
            {
                // var input = m_MoveAction.ReadValue<Vector2>();
                Vector3 move = new Vector3(m_MoveSideways, 0f, m_MoveForward);

                if (FaultManager.Instance != null && FaultManager.Instance.Config.stuckBugs)
                {
                    move *= stuckBugMultiplier;
                }

                // constrain move input to a maximum magnitude of 1, otherwise diagonal movement might exceed the max move speed defined
                move = Vector3.ClampMagnitude(move, 1);

                return move;
            }

            return Vector3.zero;
        }

        public float GetLookInputsHorizontal()
        {
            if (!CanProcessInput())
                return 0.0f;

            float input = m_Turn / horizontalTurnSpeed;

            return input;
        }

        public bool GetFireInputDown()
        {
            return GetFireInputHeld() && !m_FireInputWasHeld;
        }

        public bool GetFireInputReleased()
        {
            return !GetFireInputHeld() && m_FireInputWasHeld;
        }

        public bool GetFireInputHeld()
        {
            if (CanProcessInput())
            {
                return m_Shoot;
            }

            return false;
        }

        public float GetLookInputsVertical()   => 0.0f;

        public bool GetJumpInputDown() => false;
        public bool GetJumpInputHeld() => false;

        public bool GetAimInputHeld()      => false; 

        public bool GetSprintInputHeld()   => false;
        public bool GetCrouchInputDown()   => false;
        public bool GetCrouchInputReleased() => false;
        public bool GetReloadButtonDown()  => false;
        public int GetSwitchWeaponInput()  => 0;
        public int GetSelectWeaponInput()  => 0;

        public void SetSpeedBugMultiplier(float value)
        {
            stuckBugMultiplier = value;
        }
    }
}