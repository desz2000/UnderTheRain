using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UnderTheRain.Core.Input
{
    /// <summary>
    /// Centralized wrapper around the InputSystem_Actions asset, exposing
    /// clean Gameplay/UI switching and typed events for player systems.
    /// </summary>
    public sealed class GameInputReader : MonoBehaviour
    {
        private const string PlayerMapName = "Player";
        private const string UiMapName = "UI";

        [SerializeField] private InputActionAsset inputActions;

        private InputActionAsset _runtimeActions;
        private InputActionMap _playerMap;
        private InputActionMap _uiMap;

        private InputAction _moveAction;
        private InputAction _jumpAction;
        private InputAction _attackAction;
        private InputAction _interactAction;
        private InputAction _crouchAction;
        private InputAction _sprintAction;

        public Vector2 MoveInput => _moveAction != null ? _moveAction.ReadValue<Vector2>() : Vector2.zero;
        public bool IsJumpHeld => _jumpAction != null && _jumpAction.IsPressed();
        public bool IsCrouchHeld => _crouchAction != null && _crouchAction.IsPressed();

        public event Action OnJumpPressed;
        public event Action OnJumpReleased;
        public event Action OnAttackPressed;
        public event Action OnInteractPressed;
        public event Action OnDashPressed;

        private void Awake()
        {
            if (inputActions == null)
            {
                Debug.LogError("[GameInputReader] InputActionAsset is not assigned.", this);
                return;
            }

            _runtimeActions = Instantiate(inputActions);
            _playerMap = _runtimeActions.FindActionMap(PlayerMapName, throwIfNotFound: true);
            _uiMap = _runtimeActions.FindActionMap(UiMapName, throwIfNotFound: true);

            _moveAction = _playerMap.FindAction("Move", throwIfNotFound: true);
            _jumpAction = _playerMap.FindAction("Jump", throwIfNotFound: true);
            _attackAction = _playerMap.FindAction("Attack", throwIfNotFound: true);
            _interactAction = _playerMap.FindAction("Interact", throwIfNotFound: true);
            _crouchAction = _playerMap.FindAction("Crouch", throwIfNotFound: true);
            _sprintAction = _playerMap.FindAction("Sprint", throwIfNotFound: true);
        }

        private void OnEnable()
        {
            if (_playerMap == null) return;

            _jumpAction.performed += HandleJumpPerformed;
            _jumpAction.canceled += HandleJumpCanceled;
            _attackAction.performed += HandleAttackPerformed;
            _interactAction.performed += HandleInteractPerformed;
            _sprintAction.performed += HandleDashPerformed;

            EnableGameplay();
        }

        private void OnDisable()
        {
            if (_playerMap == null) return;

            _jumpAction.performed -= HandleJumpPerformed;
            _jumpAction.canceled -= HandleJumpCanceled;
            _attackAction.performed -= HandleAttackPerformed;
            _interactAction.performed -= HandleInteractPerformed;
            _sprintAction.performed -= HandleDashPerformed;

            _playerMap.Disable();
            _uiMap?.Disable();
        }

        private void OnDestroy()
        {
            if (_runtimeActions != null)
            {
                Destroy(_runtimeActions);
                _runtimeActions = null;
            }
        }

        public void EnableGameplay()
        {
            _uiMap?.Disable();
            _playerMap?.Enable();
        }

        public void EnableUI()
        {
            _playerMap?.Disable();
            _uiMap?.Enable();
        }

        private void HandleJumpPerformed(InputAction.CallbackContext ctx) => OnJumpPressed?.Invoke();
        private void HandleJumpCanceled(InputAction.CallbackContext ctx) => OnJumpReleased?.Invoke();
        private void HandleAttackPerformed(InputAction.CallbackContext ctx) => OnAttackPressed?.Invoke();
        private void HandleInteractPerformed(InputAction.CallbackContext ctx) => OnInteractPressed?.Invoke();
        private void HandleDashPerformed(InputAction.CallbackContext ctx) => OnDashPressed?.Invoke();
    }
}
