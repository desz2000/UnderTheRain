using UnityEngine;

namespace UnderTheRain.Player
{
    public struct PlayerMotorInput
    {
        public float moveX;
        public bool jumpPressed;
        public bool jumpHeld;
        public bool jumpReleased;
    }

    /// <summary>
    /// Pure (engine-agnostic) locomotion logic: run acceleration, variable jump,
    /// coyote time, input buffering and the locomotion state. Driven by
    /// <see cref="PlayerController"/>; fully unit-testable in EditMode.
    /// </summary>
    public sealed class PlayerMotor
    {
        private readonly PlayerMovementConfigSO _config;
        private float _coyoteTimer;
        private float _bufferTimer;

        public Vector2 Velocity;
        public PlayerLocomotionState State { get; private set; }
        public bool JumpedThisStep { get; private set; }

        public PlayerMotor(PlayerMovementConfigSO config)
        {
            _config = config;
        }

        public void Step(float dt, PlayerMotorInput input, bool grounded)
        {
            JumpedThisStep = false;

            // Horizontal: reach max speed in accelerationTime, stop in decelerationTime.
            float target = Mathf.Clamp(input.moveX, -1f, 1f) * _config.maxRunSpeed;
            bool braking = Mathf.Approximately(target, 0f) || Mathf.Sign(target) != Mathf.Sign(Velocity.x) && !Mathf.Approximately(Velocity.x, 0f);
            float rate = _config.maxRunSpeed / (braking ? _config.decelerationTime : _config.accelerationTime);
            if (!grounded) rate *= _config.airControlMultiplier;
            Velocity.x = Mathf.MoveTowards(Velocity.x, target, rate * dt);

            // Timers
            if (grounded && Velocity.y <= 0f) _coyoteTimer = _config.coyoteTime;
            else _coyoteTimer = Mathf.Max(0f, _coyoteTimer - dt);

            if (input.jumpPressed) _bufferTimer = _config.jumpBufferTime;
            else _bufferTimer = Mathf.Max(0f, _bufferTimer - dt);

            // Jump (buffered press + coyote window)
            if (_bufferTimer > 0f && _coyoteTimer > 0f)
            {
                Velocity.y = _config.JumpVelocity;
                _bufferTimer = 0f;
                _coyoteTimer = 0f;
                JumpedThisStep = true;
                grounded = false;
            }

            // Variable height: releasing early cuts the ascent.
            if (input.jumpReleased && Velocity.y > 0f)
                Velocity.y *= _config.jumpCutMultiplier;

            // Gravity
            if (grounded && Velocity.y <= 0f)
            {
                Velocity.y = 0f;
            }
            else
            {
                float g = Velocity.y > 0f ? _config.riseGravity : _config.riseGravity * _config.fallGravityMultiplier;
                Velocity.y = Mathf.Max(Velocity.y - g * dt, -_config.maxFallSpeed);
            }

            State = ResolveState(grounded);
        }

        /// <summary>Call when the head hits a ceiling so the ascent stops immediately.</summary>
        public void OnCeilingHit()
        {
            if (Velocity.y > 0f) Velocity.y = 0f;
        }

        private PlayerLocomotionState ResolveState(bool grounded)
        {
            if (!grounded) return Velocity.y > 0f ? PlayerLocomotionState.JumpAscend : PlayerLocomotionState.Fall;
            return Mathf.Abs(Velocity.x) > 0.01f ? PlayerLocomotionState.Run : PlayerLocomotionState.Idle;
        }
    }
}
