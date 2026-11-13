using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace UnderTheRain.Core.Input
{
    /// <summary>
    /// Tracks the most recently used input device (Keyboard/Mouse vs Gamepad)
    /// so diegetic prompts can swap glyphs automatically.
    /// </summary>
    public sealed class InputDeviceTracker : MonoBehaviour
    {
        public InputDeviceType CurrentDevice { get; private set; } = InputDeviceType.KeyboardMouse;

        public event Action<InputDeviceType> OnDeviceChanged;

        private void OnEnable()
        {
            InputSystem.onEvent += HandleInputEvent;
        }

        private void OnDisable()
        {
            InputSystem.onEvent -= HandleInputEvent;
        }

        public static InputDeviceType? ResolveDeviceType(InputDevice device)
        {
            if (device == null) return null;
            if (device is Gamepad) return InputDeviceType.Gamepad;
            if (device is Keyboard || device is Mouse) return InputDeviceType.KeyboardMouse;
            return null;
        }

        private void HandleInputEvent(InputEventPtr eventPtr, InputDevice device)
        {
            if (!eventPtr.IsA<StateEvent>() && !eventPtr.IsA<DeltaStateEvent>())
            {
                return;
            }

            InputDeviceType? resolved = ResolveDeviceType(device);
            if (!resolved.HasValue || resolved.Value == CurrentDevice)
            {
                return;
            }

            CurrentDevice = resolved.Value;
            OnDeviceChanged?.Invoke(CurrentDevice);
        }
    }
}
