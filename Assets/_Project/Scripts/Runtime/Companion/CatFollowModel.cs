using UnityEngine;

namespace UnderTheRain.Companion
{
    public enum CatCompanionState
    {
        Follow,
        Perching,
        Perched
    }

    public struct CatFollowInput
    {
        public Vector2 dantePosition;
        public int facingDirection;
        public bool shouldPerch;
        public bool grounded;
    }

    /// <summary>
    /// Pure simulation model for the metal cat companion: follows closely behind Dante on the ground
    /// (staying calm in idle when Dante is nearby), teleports if stranded, and smoothly perches
    /// on Dante's shoulder during wall climbs, combat, or jumping/platform traversal.
    /// </summary>
    public sealed class CatFollowModel
    {
        private readonly CatConfigSO _config;

        private Vector2 _perchStartPosition;
        private float _perchElapsed;

        public CatCompanionState State { get; private set; } = CatCompanionState.Follow;
        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; }
        public int FacingDirection { get; private set; } = 1;
        public bool IsPerchedOrPerching => State == CatCompanionState.Perching || State == CatCompanionState.Perched;

        public CatFollowModel(CatConfigSO config, Vector2 initialPosition)
        {
            _config = config;
            Position = initialPosition;
        }

        public Vector2 GetFollowTarget(Vector2 dantePosition, int facingDirection)
        {
            int dir = facingDirection >= 0 ? 1 : -1;
            return dantePosition + new Vector2(_config.followOffset.x * dir, _config.followOffset.y);
        }

        public Vector2 GetShoulderTarget(Vector2 dantePosition, int facingDirection)
        {
            int dir = facingDirection >= 0 ? 1 : -1;
            return dantePosition + new Vector2(_config.shoulderOffset.x * dir, _config.shoulderOffset.y);
        }

        public void Step(float deltaTime, CatFollowInput input)
        {
            int dir = input.facingDirection >= 0 ? 1 : -1;
            Vector2 followTarget = GetFollowTarget(input.dantePosition, dir);
            Vector2 shoulderTarget = GetShoulderTarget(input.dantePosition, dir);

            if (input.shouldPerch)
            {
                StepPerch(deltaTime, shoulderTarget, dir);
                return;
            }

            if (State != CatCompanionState.Follow)
            {
                // Dismount calmly from shoulder without hopping.
                State = CatCompanionState.Follow;
                Velocity = Vector2.zero;
            }

            if (Vector2.Distance(Position, followTarget) > _config.teleportDistance)
            {
                Position = followTarget;
                Velocity = Vector2.zero;
                FacingDirection = dir;
                return;
            }

            float dx = followTarget.x - Position.x;
            float vx = Velocity.x;
            if (Mathf.Abs(dx) > _config.stopDistance)
            {
                float targetVx = Mathf.Sign(dx) * _config.followSpeed;
                vx = Mathf.MoveTowards(vx, targetVx, _config.acceleration * deltaTime);
                FacingDirection = dx > 0f ? 1 : -1;
            }
            else
            {
                vx = Mathf.MoveTowards(vx, 0f, _config.deceleration * deltaTime);
                FacingDirection = dir;
            }

            float vy = Velocity.y;
            if (input.grounded)
            {
                if (vy < 0f) vy = 0f;
            }
            else
            {
                vy = Mathf.Max(-_config.maxFallSpeed, vy - _config.gravity * deltaTime);
            }

            Velocity = new Vector2(vx, vy);
        }

        private void StepPerch(float deltaTime, Vector2 shoulderTarget, int facingDir)
        {
            FacingDirection = facingDir;
            Velocity = Vector2.zero;

            if (State == CatCompanionState.Follow)
            {
                State = CatCompanionState.Perching;
                _perchStartPosition = Position;
                _perchElapsed = 0f;
            }

            if (State == CatCompanionState.Perching)
            {
                _perchElapsed += deltaTime;
                float t = _config.perchTransitionDuration > 0f
                    ? Mathf.Clamp01(_perchElapsed / _config.perchTransitionDuration)
                    : 1f;
                // Smoothstep easing for a crisp hop onto Dante's shoulder.
                float eased = t * t * (3f - 2f * t);
                Position = Vector2.Lerp(_perchStartPosition, shoulderTarget, eased);

                if (t >= 1f)
                {
                    State = CatCompanionState.Perched;
                    Position = shoulderTarget;
                }
                return;
            }

            Position = shoulderTarget;
        }
    }
}
