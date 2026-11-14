using NUnit.Framework;
using UnityEngine;
using UnderTheRain.Player;

namespace UnderTheRain.Tests.EditMode
{
    public class PlayerMotorTests
    {
        private const float Dt = 0.01f;
        private PlayerMovementConfigSO _config;
        private PlayerMotor _motor;

        [SetUp]
        public void SetUp()
        {
            _config = ScriptableObject.CreateInstance<PlayerMovementConfigSO>();
            _motor = new PlayerMotor(_config);
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(_config);

        private void Run(int steps, PlayerMotorInput input, bool grounded)
        {
            for (int i = 0; i < steps; i++)
            {
                _motor.Step(Dt, input, grounded);
                input.jumpPressed = false;
                input.jumpReleased = false;
            }
        }

        [Test]
        public void Run_ReachesMaxSpeedWithinAccelerationTime()
        {
            Run(10, new PlayerMotorInput { moveX = 1f }, true);
            Assert.AreEqual(_config.maxRunSpeed, _motor.Velocity.x, 0.01f);
        }

        [Test]
        public void Stop_HasNoSlide_AfterDecelerationTime()
        {
            Run(10, new PlayerMotorInput { moveX = 1f }, true);
            Run(7, default, true);
            Assert.AreEqual(0f, _motor.Velocity.x, 0.01f);
            Assert.AreEqual(PlayerLocomotionState.Idle, _motor.State);
        }

        [Test]
        public void LockHorizontal_PreservesVelocity_SoItMustNotBeUsedToFreezeControl()
        {
            // lockHorizontal is for knockback (keep the push). Dialogue locks send moveX = 0 instead,
            // otherwise Dante keeps running through the conversation (regression found in story 5.1).
            Run(10, new PlayerMotorInput { moveX = 1f }, true);
            Run(30, new PlayerMotorInput { moveX = 0f, lockHorizontal = true }, true);
            Assert.AreEqual(_config.maxRunSpeed, _motor.Velocity.x, 0.01f);

            Run(7, new PlayerMotorInput { moveX = 0f }, true);
            Assert.AreEqual(0f, _motor.Velocity.x, 0.01f);
        }

        [Test]
        public void Jump_AppliesJumpVelocity_WhenGrounded()
        {
            _motor.Step(Dt, new PlayerMotorInput { jumpPressed = true, jumpHeld = true }, true);
            Assert.IsTrue(_motor.JumpedThisStep);
            Assert.Greater(_motor.Velocity.y, 0f);
            Assert.AreEqual(PlayerLocomotionState.JumpAscend, _motor.State);
        }

        [Test]
        public void ReleasingJumpEarly_ProducesLowerPeakThanHolding()
        {
            float held = Peak(releaseAfterSteps: -1);
            float tapped = Peak(releaseAfterSteps: 3);
            Assert.Less(tapped, held * 0.7f);
        }

        private float Peak(int releaseAfterSteps)
        {
            var motor = new PlayerMotor(_config);
            float y = 0f, peak = 0f;
            motor.Step(Dt, new PlayerMotorInput { jumpPressed = true, jumpHeld = true }, true);
            for (int i = 0; i < 400; i++)
            {
                bool release = i == releaseAfterSteps;
                motor.Step(Dt, new PlayerMotorInput { jumpHeld = !release, jumpReleased = release }, false);
                y += motor.Velocity.y * Dt;
                peak = Mathf.Max(peak, y);
            }
            return peak;
        }

        [Test]
        public void CoyoteTime_AllowsJump_ShortlyAfterLeavingLedge()
        {
            Run(1, default, true);
            Run(2, default, false); // 20ms airborne < 40ms coyote
            _motor.Step(Dt, new PlayerMotorInput { jumpPressed = true }, false);
            Assert.IsTrue(_motor.JumpedThisStep);
        }

        [Test]
        public void CoyoteTime_Expires_AfterWindow()
        {
            Run(1, default, true);
            Run(8, default, false); // 80ms airborne > 40ms coyote
            _motor.Step(Dt, new PlayerMotorInput { jumpPressed = true }, false);
            Assert.IsFalse(_motor.JumpedThisStep);
        }

        [Test]
        public void InputBuffer_JumpsOnLanding_WhenPressedJustBeforeGround()
        {
            _motor.Velocity = new Vector2(0f, -5f);
            _motor.Step(Dt, new PlayerMotorInput { jumpPressed = true }, false);
            Assert.IsFalse(_motor.JumpedThisStep);
            Run(5, default, false); // still within 120ms
            _motor.Step(Dt, default, true);
            Assert.IsTrue(_motor.JumpedThisStep);
        }

        [Test]
        public void InputBuffer_Expires_WhenPressedTooEarly()
        {
            _motor.Velocity = new Vector2(0f, -5f);
            _motor.Step(Dt, new PlayerMotorInput { jumpPressed = true }, false);
            Run(20, default, false); // 200ms > 120ms
            _motor.Step(Dt, default, true);
            Assert.IsFalse(_motor.JumpedThisStep);
        }

        [Test]
        public void Fall_ClampsToMaxFallSpeed()
        {
            Run(300, default, false);
            Assert.AreEqual(-_config.maxFallSpeed, _motor.Velocity.y, 0.001f);
            Assert.AreEqual(PlayerLocomotionState.Fall, _motor.State);
        }

        [Test]
        public void WallSlide_Blocked_WhenAbilityLocked()
        {
            _motor.Velocity = new Vector2(0f, -10f);
            Run(50, new PlayerMotorInput { moveX = 1f, wallContactDir = 1, wallJumpUnlocked = false }, false);
            Assert.AreNotEqual(PlayerLocomotionState.WallSlide, _motor.State);
            Assert.Less(_motor.Velocity.y, -_config.wallSlideMaxSpeed);
        }

        [Test]
        public void WallSlide_ClampsFallSpeed_WhenPushingIntoWallAndUnlocked()
        {
            _motor.Velocity = new Vector2(0f, -1f);
            Run(100, new PlayerMotorInput { moveX = 1f, wallContactDir = 1, wallJumpUnlocked = true }, false);
            Assert.AreEqual(PlayerLocomotionState.WallSlide, _motor.State);
            Assert.AreEqual(-_config.wallSlideMaxSpeed, _motor.Velocity.y, 0.001f);
        }

        [Test]
        public void WallJump_Blocked_WhenAbilityLocked()
        {
            _motor.Step(Dt, new PlayerMotorInput { jumpPressed = true, wallContactDir = 1, wallJumpUnlocked = false }, false);
            Assert.IsFalse(_motor.JumpedThisStep);
            Assert.IsFalse(_motor.WallJumpedThisStep);
        }

        [Test]
        public void WallJump_LaunchesAwayFromWall_WhenUnlocked()
        {
            _motor.Step(Dt, new PlayerMotorInput { jumpPressed = true, jumpHeld = true, wallContactDir = 1, wallJumpUnlocked = true }, false);
            Assert.IsTrue(_motor.WallJumpedThisStep);
            Assert.AreEqual(-_config.wallJumpVelocity.x, _motor.Velocity.x, 0.01f);
            Assert.Greater(_motor.Velocity.y, 0f);
            Assert.AreEqual(PlayerLocomotionState.WallJump, _motor.State);
        }

        [Test]
        public void WallJump_LockTime_AllowsSingleWallClimbAfterBriefPush()
        {
            _motor.Step(Dt, new PlayerMotorInput { jumpPressed = true, jumpHeld = true, wallContactDir = 1, wallJumpUnlocked = true }, false);
            // Brief push away during lock window (30ms < 50ms lock)
            Run(3, new PlayerMotorInput { moveX = 1f, jumpHeld = true, wallJumpUnlocked = true }, false);
            Assert.Less(_motor.Velocity.x, 0f);
            // After lock window expires, holding toward the wall steers back for single-wall climbing
            Run(20, new PlayerMotorInput { moveX = 1f, jumpHeld = true, wallJumpUnlocked = true }, false);
            Assert.Greater(_motor.Velocity.x, 0f);
        }

        [Test]
        public void WallJump_AllowedWithinWallCoyoteWindow()
        {
            _motor.Step(Dt, new PlayerMotorInput { moveX = 1f, wallContactDir = 1, wallJumpUnlocked = true }, false);
            Run(3, new PlayerMotorInput { wallContactDir = 0, wallJumpUnlocked = true }, false); // 30ms < 80ms wall coyote
            _motor.Step(Dt, new PlayerMotorInput { jumpPressed = true, wallJumpUnlocked = true }, false);
            Assert.IsTrue(_motor.WallJumpedThisStep);
            Assert.AreEqual(-_config.wallJumpVelocity.x, _motor.Velocity.x, 0.01f);
        }
    }
}
