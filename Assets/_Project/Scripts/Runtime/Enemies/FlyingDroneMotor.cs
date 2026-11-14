using UnityEngine;
using UnderTheRain.World;

namespace UnderTheRain.Enemies
{
    /// <summary>
    /// Flight for hive drones: hovers around its post, flies to a spot above Dante once it sees him and
    /// attacks with a telegraphed swoop (wind-up tint, straight dive at where Dante was, then a cooldown).
    /// Kitty's interference freezes it and cancels the wind-up.
    /// </summary>
    [RequireComponent(typeof(PatrolInterferenceTarget), typeof(EnemyAwareness))]
    public sealed class FlyingDroneMotor : MonoBehaviour
    {
        [Header("Hover (unaware)")]
        [SerializeField, Min(0.1f)] private float hoverSpeed = 1.8f;
        [SerializeField, Min(0f)] private float hoverHalfWidth = 3f;
        [SerializeField, Min(0f)] private float bobAmplitude = 0.25f;
        [SerializeField, Min(0f)] private float bobFrequency = 2.2f;

        [Header("Chase")]
        [SerializeField, Min(0.5f)] private float chaseSpeed = 4.5f;
        [Tooltip("Height above Dante where it lines up the swoop.")]
        [SerializeField, Min(0.5f)] private float hoverHeight = 2.6f;
        [SerializeField, Min(0f)] private float hoverSideOffset = 2.2f;

        [Tooltip("Drones look down as well as ahead: half angle of their vision cone.")]
        [SerializeField, Range(5f, 180f)] private float sightHalfAngle = 125f;

        [Header("Swoop attack")]
        [SerializeField, Min(0.05f)] private float telegraphDuration = 0.45f;
        [SerializeField, Min(1f)] private float swoopSpeed = 11f;
        [SerializeField, Min(0.1f)] private float swoopMaxDuration = 0.7f;
        [SerializeField, Min(0f)] private float swoopCooldown = 1.4f;

        [Header("Collision")]
        [Tooltip("Solid layers the drone cannot fly through.")]
        [SerializeField] private LayerMask obstacleMask;
        [SerializeField, Min(0.05f)] private float bodyRadius = 0.3f;

        private enum Phase { Hover, Telegraph, Swoop }

        private const float AnchorTolerance = 0.8f;
        private const float ArriveDistance = 0.2f;

        private PatrolInterferenceTarget _core;
        private EnemyAwareness _awareness;
        private RoomBounds _room;
        private Vector2 _origin;
        private Phase _phase;
        private float _phaseTimer;
        private float _cooldown;
        private float _bobTime;
        private int _hoverDir = 1;
        private Vector2 _swoopDirection;
        private Vector2 _swoopTarget;

        public bool IsSwooping => _phase == Phase.Swoop;

        private void Awake()
        {
            _core = GetComponent<PatrolInterferenceTarget>();
            _awareness = GetComponent<EnemyAwareness>();
            _awareness.SetViewHalfAngle(sightHalfAngle);
            _room = GetComponentInParent<RoomBounds>();
            _origin = transform.position;
            if (obstacleMask.value == 0) obstacleMask = LayerMask.GetMask("Ground");
        }

        private void OnEnable()
        {
            // A re-entered room starts with the drone back at its post.
            transform.position = _origin;
            _phase = Phase.Hover;
            _cooldown = 0f;
        }

        private void Update()
        {
            float dt = Time.deltaTime;

            if (_core.IsHesitating || _core.IsDormant)
            {
                CancelAttack();
                return;
            }

            if (_cooldown > 0f) _cooldown = Mathf.Max(0f, _cooldown - dt);
            _bobTime += dt;

            var state = _awareness.State;
            if (state != EnemyAwarenessState.Chasing && _phase != Phase.Hover) CancelAttack();

            switch (state)
            {
                case EnemyAwarenessState.Alerted:
                    Face(_awareness.LastKnownPosition.x);
                    break;

                case EnemyAwarenessState.Chasing:
                    TickChase(dt);
                    break;

                case EnemyAwarenessState.Searching:
                    FlyTowards(_awareness.LastKnownPosition + Vector2.up * hoverHeight, chaseSpeed * 0.7f, dt);
                    break;

                case EnemyAwarenessState.Returning:
                    if (FlyTowards(_origin, hoverSpeed * 1.5f, dt)) _awareness.NotifyReturned();
                    break;

                default:
                    TickHover(dt);
                    break;
            }
        }

        private void TickHover(float dt)
        {
            Vector2 pos = transform.position;
            float targetX = pos.x + _hoverDir * hoverSpeed * dt;
            if (targetX >= _origin.x + hoverHalfWidth)
            {
                targetX = _origin.x + hoverHalfWidth;
                _hoverDir = -1;
            }
            else if (targetX <= _origin.x - hoverHalfWidth)
            {
                targetX = _origin.x - hoverHalfWidth;
                _hoverDir = 1;
            }

            float targetY = _origin.y + Mathf.Sin(_bobTime * bobFrequency) * bobAmplitude;
            if (!Step(new Vector2(targetX, targetY) - pos)) _hoverDir = -_hoverDir;
            _core.SetFacing(_hoverDir);
        }

        private void TickChase(float dt)
        {
            Vector2 pos = transform.position;
            Vector2 target = _awareness.LastKnownPosition;

            switch (_phase)
            {
                case Phase.Hover:
                    // Stays on the side it is already on, so it does not cross over Dante's head.
                    float side = pos.x >= target.x ? 1f : -1f;
                    Vector2 anchor = target + new Vector2(side * hoverSideOffset, hoverHeight);
                    FlyTowards(anchor, chaseSpeed, dt);
                    Face(target.x);

                    if (_cooldown <= 0f && _awareness.HasTargetInSight
                        && ((Vector2)transform.position - anchor).sqrMagnitude <= AnchorTolerance * AnchorTolerance)
                    {
                        _phase = Phase.Telegraph;
                        _phaseTimer = telegraphDuration;
                        _swoopTarget = target;
                        _core.SetTelegraphing(true);
                    }
                    break;

                case Phase.Telegraph:
                    Face(_swoopTarget.x);
                    _phaseTimer -= dt;
                    if (_phaseTimer <= 0f)
                    {
                        _core.SetTelegraphing(false);
                        Vector2 toTarget = _swoopTarget - pos;
                        _swoopDirection = toTarget.sqrMagnitude > 0.0001f ? toTarget.normalized : Vector2.down;
                        _phase = Phase.Swoop;
                        _phaseTimer = swoopMaxDuration;
                    }
                    break;

                case Phase.Swoop:
                    _phaseTimer -= dt;
                    bool moved = Step(_swoopDirection * (swoopSpeed * dt));
                    bool passedTarget = Vector2.Dot(_swoopTarget - (Vector2)transform.position, _swoopDirection) <= 0f;
                    if (!moved || passedTarget || _phaseTimer <= 0f)
                    {
                        _phase = Phase.Hover;
                        _cooldown = swoopCooldown;
                    }
                    break;
            }
        }

        private void CancelAttack()
        {
            if (_phase == Phase.Hover) return;
            _phase = Phase.Hover;
            _cooldown = swoopCooldown;
            _core.SetTelegraphing(false);
        }

        private void Face(float targetX)
        {
            float dx = targetX - transform.position.x;
            if (Mathf.Abs(dx) > 0.05f) _core.SetFacing(dx > 0f ? 1 : -1);
        }

        /// <summary>Flies towards a point. Returns true once it has arrived.</summary>
        private bool FlyTowards(Vector2 destination, float speed, float dt)
        {
            Vector2 toDestination = destination - (Vector2)transform.position;
            float distance = toDestination.magnitude;
            if (distance <= ArriveDistance) return true;

            Step(toDestination / distance * Mathf.Min(speed * dt, distance));
            return false;
        }

        /// <summary>Moves by delta, sliding along solid ground and the room edge. Returns false if fully blocked.</summary>
        private bool Step(Vector2 delta)
        {
            Vector2 pos = transform.position;
            if (IsFree(pos + delta))
            {
                transform.position = pos + delta;
                return true;
            }

            var slideX = new Vector2(delta.x, 0f);
            if (Mathf.Abs(delta.x) > 0.0001f && IsFree(pos + slideX))
            {
                transform.position = pos + slideX;
                return false;
            }

            var slideY = new Vector2(0f, delta.y);
            if (Mathf.Abs(delta.y) > 0.0001f && IsFree(pos + slideY))
            {
                transform.position = pos + slideY;
            }
            return false;
        }

        private bool IsFree(Vector2 point)
        {
            if (_room != null && !_room.ContainsPoint(point)) return false;
            return Physics2D.OverlapCircle(point, bodyRadius, obstacleMask) == null;
        }
    }
}
