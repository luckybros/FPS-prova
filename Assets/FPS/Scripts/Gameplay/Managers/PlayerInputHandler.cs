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

        void Start()
        {
            m_PlayerCharacterController = GetComponent<PlayerCharacterController>();
            DebugUtility.HandleErrorIfNullGetComponent<PlayerCharacterController, PlayerInputHandler>(
                m_PlayerCharacterController, this, gameObject);
            m_GameFlowManager = FindFirstObjectByType<GameFlowManager>();
            DebugUtility.HandleErrorIfNullFindObject<GameFlowManager, PlayerInputHandler>(m_GameFlowManager, this);

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            m_PlayerInput = GetComponent<PlayerInput>();
            m_PlayerInput.currentActionMap.Enable();

            m_MoveAction = m_PlayerInput.actions.FindAction("Player/Move");
            
            m_MoveAction.Enable();
        }

        public bool CanProcessInput()
        {
            return Cursor.lockState == CursorLockMode.Locked && !m_GameFlowManager.GameIsEnding;
        }

        public Vector3 GetMoveInput()
        {
            if (CanProcessInput())
            {
                var input = m_MoveAction.ReadValue<Vector2>();
                Vector3 move = new Vector3(input.x, 0f, input.y);

                // constrain move input to a maximum magnitude of 1, otherwise diagonal movement might exceed the max move speed defined
                move = Vector3.ClampMagnitude(move, 1);

                return move;
            }

            return Vector3.zero;
        }

        public float GetLookInputsHorizontal() => 0.0f;
        public float GetLookInputsVertical()   => 0.0f;

        public bool GetJumpInputDown() => false;
        public bool GetJumpInputHeld() => false;

        public bool GetFireInputDown()     => false;
        public bool GetFireInputReleased() => false;
        public bool GetFireInputHeld()     => false;
        public bool GetAimInputHeld()      => false; 

        public bool GetSprintInputHeld()   => false;
        public bool GetCrouchInputDown()   => false;
        public bool GetCrouchInputReleased() => false;
        public bool GetReloadButtonDown()  => false;
        public int GetSwitchWeaponInput()  => 0;
        public int GetSelectWeaponInput()  => 0;
    }
}