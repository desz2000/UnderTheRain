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
        private InputActionMap _fallbackMap;
        private InputActionMap _uiMap;

        private InputAction _moveAction;
        private InputAction _jumpAction;
        private InputAction _attackAction;
        private InputAction _interactAction;
        private InputAction _crouchAction;
        private InputAction _sprintAction;
        private InputAction _shootAction;
        private InputAction _healAction;
        private InputAction _craftAction;
        private InputAction _catCommandAction;

        public Vector2 MoveInput => _moveAction != null ? _moveAction.ReadValue<Vector2>() : Vector2.zero;
        public bool IsJumpHeld => _jumpAction != null && _jumpAction.IsPressed();
        public bool IsCrouchHeld => _crouchAction != null && _crouchAction.IsPressed();

        public event Action OnJumpPressed;
        public event Action OnJumpReleased;
        public event Action OnAttackPressed;
        public event Action OnShootPressed;
        public event Action OnHealPressed;
        public event Action OnCraftPressed;
        public event Action OnInteractPressed;
        public event Action OnDashPressed;
        public event Action OnCatCommandPressed;

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
            _fallbackMap = new InputActionMap("PlayerFallbacks");

            _moveAction = _playerMap.FindAction("Move", throwIfNotFound: true);
            _jumpAction = _playerMap.FindAction("Jump", throwIfNotFound: true);
            _attackAction = _playerMap.FindAction("Attack", throwIfNotFound: true);
            _interactAction = _playerMap.FindAction("Interact", throwIfNotFound: true);
            _crouchAction = _playerMap.FindAction("Crouch", throwIfNotFound: true);
            _sprintAction = _playerMap.FindAction("Sprint", throwIfNotFound: true);

            // Keep runtime fallbacks in a separate standalone map so we never mutate the cloned asset's internal action state.
            _interactFallbackAction = CreateFallbackAction("InteractFallback", "<Gamepad>/buttonNorth");
            _shootAction = _playerMap.FindAction("Shoot", throwIfNotFound: false)
                           ?? CreateFallbackAction("Shoot", "<Mouse>/rightButton", "<Keyboard>/f", "<Gamepad>/rightTrigger");
            _healAction = _playerMap.FindAction("Heal", throwIfNotFound: false)
                          ?? CreateFallbackAction("Heal", "<Keyboard>/q", "<Gamepad>/leftShoulder");
            _craftAction = _playerMap.FindAction("Craft", throwIfNotFound: false)
                           ?? CreateFallbackAction("Craft", "<Keyboard>/c", "<Gamepad>/dpad/down");
            _catCommandAction = _playerMap.FindAction("CatCommand", throwIfNotFound: false)
                                ?? CreateFallbackAction("CatCommand", "<Keyboard>/r", "<Gamepad>/rightShoulder");
        }

        private InputAction _interactFallbackAction;

        private InputAction CreateFallbackAction(string actionName, params string[] bindings)
        {
            var action = _fallbackMap.AddAction(actionName, InputActionType.Button);
            foreach (string binding in bindings)
            {
                action.AddBinding(binding);
            }
            return action;
        }

        private void OnEnable()
        {
            if (_playerMap == null) return;

            _jumpAction.performed += HandleJumpPerformed;
            _jumpAction.canceled += HandleJumpCanceled;
            _attackAction.performed += HandleAttackPerformed;
            _shootAction.performed += HandleShootPerformed;
            _healAction.performed += HandleHealPerformed;
            _craftAction.performed += HandleCraftPerformed;
            _interactAction.performed += HandleInteractPerformed;
            if (_interactFallbackAction != null) _interactFallbackAction.performed += HandleInteractPerformed;
            _sprintAction.performed += HandleDashPerformed;
            _catCommandAction.performed += HandleCatCommandPerformed;

            EnableGameplay();
        }

        private void OnDisable()
        {
            if (_playerMap == null) return;

            _jumpAction.performed -= HandleJumpPerformed;
            _jumpAction.canceled -= HandleJumpCanceled;
            _attackAction.performed -= HandleAttackPerformed;
            _shootAction.performed -= HandleShootPerformed;
            _healAction.performed -= HandleHealPerformed;
            _craftAction.performed -= HandleCraftPerformed;
            _interactAction.performed -= HandleInteractPerformed;
            if (_interactFallbackAction != null) _interactFallbackAction.performed -= HandleInteractPerformed;
            _sprintAction.performed -= HandleDashPerformed;
            _catCommandAction.performed -= HandleCatCommandPerformed;

            _playerMap.Disable();
            _fallbackMap?.Disable();
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
            _fallbackMap?.Enable();
        }

        /// <summary>
        /// While true, combat/ability actions (attack, shoot, heal, craft, dash, Kitty command) are swallowed.
        /// Interact keeps firing so dialogues can advance; movement/jump are gated by PlayerController.SetControlLocked.
        /// </summary>
        public bool ActionsSuppressed { get; private set; }

        public void SetActionsSuppressed(bool suppressed)
        {
            ActionsSuppressed = suppressed;
        }

        public void EnableUI()
        {
            _playerMap?.Disable();
            _fallbackMap?.Disable();
            _uiMap?.Enable();
        }

        private void HandleJumpPerformed(InputAction.CallbackContext ctx) => OnJumpPressed?.Invoke();
        private void HandleJumpCanceled(InputAction.CallbackContext ctx) => OnJumpReleased?.Invoke();
        private void HandleAttackPerformed(InputAction.CallbackContext ctx) { if (!ActionsSuppressed) OnAttackPressed?.Invoke(); }
        private void HandleShootPerformed(InputAction.CallbackContext ctx) { if (!ActionsSuppressed) OnShootPressed?.Invoke(); }
        private void HandleHealPerformed(InputAction.CallbackContext ctx) { if (!ActionsSuppressed) OnHealPressed?.Invoke(); }
        private void HandleCraftPerformed(InputAction.CallbackContext ctx) { if (!ActionsSuppressed) OnCraftPressed?.Invoke(); }
        private void HandleInteractPerformed(InputAction.CallbackContext ctx) => OnInteractPressed?.Invoke();
        private void HandleDashPerformed(InputAction.CallbackContext ctx) { if (!ActionsSuppressed) OnDashPressed?.Invoke(); }
        private void HandleCatCommandPerformed(InputAction.CallbackContext ctx) { if (!ActionsSuppressed) OnCatCommandPressed?.Invoke(); }
    }
}
