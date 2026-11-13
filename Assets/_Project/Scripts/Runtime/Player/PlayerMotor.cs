using UnityEngine;

namespace UnderTheRain.Player
{
    public struct PlayerMotorInput
    {
        public float moveX;
        public bool jumpPressed;
        public bool jumpHeld;
        public bool jumpReleased;
        public int wallContactDir; // -1 = left wall, 0 = none, +1 = right wall
        public bool wallJumpUnlocked;
        public bool lockHorizontal;
    }

    /// <summary>
    /// Pure (engine-agnostic) locomotion logic: run acceleration, variable jump,
    /// coyote time, input buffering, wall slide and wall jump. Driven by
    /// <see cref="PlayerController"/>; fully unit-testable in EditMode.
    /// </summary>
    public sealed class PlayerMotor
    {
        private readonly PlayerMovementConfigSO _config;
        private float _coyoteTimer;
        private float _bufferTimer;
        private float _wallCoyoteTimer;
        private float _wallJumpLockTimer;
        private int _lastWallDir;

        public Vector2 Velocity;
        public PlayerLocomotionState State { get; private set; }
        public bool JumpedThisStep { get; private set; }
        public bool WallJumpedThisStep { get; private set; }

        public PlayerMotor(PlayerMovementConfigSO config)
        {
            _config = config;
        }

        public void Step(float dt, PlayerMotorInput input, bool grounded)
        {
            JumpedThisStep = false;
            WallJumpedThisStep = false;

            if (grounded)
            {
                _wallJumpLockTimer = 0f;
                _wallCoyoteTimer = 0f;
            }

            // Horizontal: lock steering briefly after a wall jump or damage knockback.
            if (_wallJumpLockTimer > 0f)
            {
                _wallJumpLockTimer = Mathf.Max(0f, _wallJumpLockTimer - dt);
            }
            else if (!input.lockHorizontal)
            {
                float target = Mathf.Clamp(input.moveX, -1f, 1f) * _config.maxRunSpeed;
                bool braking = Mathf.Approximately(target, 0f) || Mathf.Sign(target) != Mathf.Sign(Velocity.x) && !Mathf.Approximately(Velocity.x, 0f);
                float rate = _config.maxRunSpeed / (braking ? _config.decelerationTime : _config.accelerationTime);
                if (!grounded) rate *= _config.airControlMultiplier;
                Velocity.x = Mathf.MoveTowards(Velocity.x, target, rate * dt);
            }

            // Timers
            if (grounded && Velocity.y <= 0f) _coyoteTimer = _config.coyoteTime;
            else _coyoteTimer = Mathf.Max(0f, _coyoteTimer - dt);

            if (input.jumpPressed) _bufferTimer = _config.jumpBufferTime;
            else _bufferTimer = Mathf.Max(0f, _bufferTimer - dt);

            if (input.wallJumpUnlocked && !grounded && input.wallContactDir != 0)
            {
                _wallCoyoteTimer = _config.wallCoyoteTime;
                _lastWallDir = input.wallContactDir;
            }
            else
            {
                _wallCoyoteTimer = Mathf.Max(0f, _wallCoyoteTimer - dt);
            }

            // Ground jump (buffered press + coyote window)
            if (_bufferTimer > 0f && _coyoteTimer > 0f)
            {
                Velocity.y = _config.JumpVelocity;
                _bufferTimer = 0f;
                _coyoteTimer = 0f;
                JumpedThisStep = true;
                grounded = false;
            }
            // Wall jump (unlocked + airborne + wall contact or wall coyote)
            else if (input.wallJumpUnlocked && !grounded && _bufferTimer > 0f && (input.wallContactDir != 0 || _wallCoyoteTimer > 0f))
            {
                int wallDir = input.wallContactDir != 0 ? input.wallContactDir : _lastWallDir;
                Velocity.x = -wallDir * _config.wallJumpVelocity.x;
                Velocity.y = _config.wallJumpVelocity.y;
                _wallJumpLockTimer = _config.wallJumpLockTime;
                _wallCoyoteTimer = 0f;
                _bufferTimer = 0f;
                JumpedThisStep = true;
                WallJumpedThisStep = true;
            }

            // Variable height: releasing early cuts the ascent.
            if (input.jumpReleased && Velocity.y > 0f)
                Velocity.y *= _config.jumpCutMultiplier;

            bool isWallSliding = input.wallJumpUnlocked
                && !grounded
                && input.wallContactDir != 0
                && Velocity.y <= 0f
                && Mathf.Abs(input.moveX) > 0.1f
                && Mathf.Sign(input.moveX) == input.wallContactDir;

            // Gravity (clamped to wallSlideMaxSpeed while sliding)
            if (grounded && Velocity.y <= 0f)
            {
                Velocity.y = 0f;
            }
            else
            {
                float g = Velocity.y > 0f ? _config.riseGravity : _config.riseGravity * _config.fallGravityMultiplier;
                float maxDown = isWallSliding ? _config.wallSlideMaxSpeed : _config.maxFallSpeed;
                Velocity.y = Mathf.Max(Velocity.y - g * dt, -maxDown);
                if (isWallSliding) Velocity.x = 0f;
            }

            State = ResolveState(grounded, isWallSliding);
        }

        /// <summary>Call when the head hits a ceiling so the ascent stops immediately.</summary>
        public void OnCeilingHit()
        {
            if (Velocity.y > 0f) Velocity.y = 0f;
        }

        private PlayerLocomotionState ResolveState(bool grounded, bool isWallSliding)
        {
            if (!grounded)
            {
                if (isWallSliding) return PlayerLocomotionState.WallSlide;
                if (_wallJumpLockTimer > 0f && Velocity.y > 0f) return PlayerLocomotionState.WallJump;
                return Velocity.y > 0f ? PlayerLocomotionState.JumpAscend : PlayerLocomotionState.Fall;
            }
            return Mathf.Abs(Velocity.x) > 0.01f ? PlayerLocomotionState.Run : PlayerLocomotionState.Idle;
        }
    }
}
