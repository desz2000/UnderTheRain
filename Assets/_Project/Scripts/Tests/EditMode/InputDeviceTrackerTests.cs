using NUnit.Framework;
using UnityEngine.InputSystem;
using UnderTheRain.Core.Input;

namespace UnderTheRain.Tests.EditMode
{
    public class InputDeviceTrackerTests : InputTestFixture
    {
        [Test]
        public void ResolveDeviceType_ReturnsNull_WhenDeviceIsNull()
        {
            Assert.IsNull(InputDeviceTracker.ResolveDeviceType(null));
        }

        [Test]
        public void ResolveDeviceType_ReturnsKeyboardMouse_ForKeyboardAndMouse()
        {
            var keyboard = InputSystem.AddDevice<Keyboard>();
            var mouse = InputSystem.AddDevice<Mouse>();

            Assert.AreEqual(InputDeviceType.KeyboardMouse, InputDeviceTracker.ResolveDeviceType(keyboard));
            Assert.AreEqual(InputDeviceType.KeyboardMouse, InputDeviceTracker.ResolveDeviceType(mouse));
        }

        [Test]
        public void ResolveDeviceType_ReturnsGamepad_ForGamepad()
        {
            var gamepad = InputSystem.AddDevice<Gamepad>();

            Assert.AreEqual(InputDeviceType.Gamepad, InputDeviceTracker.ResolveDeviceType(gamepad));
        }
    }
}
