using UnityEngine;
using UnderTheRain.Player;

namespace UnderTheRain.Companion
{
    /// <summary>
    /// Controls the metal cat companion in the scene: follows closely behind Dante on the ground,
    /// perches on Dante's shoulder during wall climbs, combat, or jumping/high traversal,
    /// and stays perched for a configurable hold duration after Dante lands.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
    public sealed class CatController : MonoBehaviour
    {
        private const float GroundProbeDistance = 0.06f;

        [SerializeField] private CatConfigSO config;
        [SerializeField] private PlayerController playerController;
        [SerializeField] private PlayerMeleeCombat meleeCombat;
        [SerializeField] private PlayerRangedCombat rangedCombat;
        [SerializeField] private LayerMask groundMask;
        [SerializeField] private LayerMask oneWayMask;
        [SerializeField] private SpriteRenderer spriteRenderer;

        private Rigidbody2D _body;
        private BoxCollider2D _collider;
        private CatFollowModel _model;
        private float _combatHoldTimer;
        private float _traversalHoldTimer;
        private float _highGroundTimer;
        private Vector3 _baseScale;
        private bool _externalControlActive;

        public CatConfigSO Config => config;
        public CatCompanionState State => _model != null ? _model.State : CatCompanionState.Follow;
        public bool IsPerched => _model != null && _model.IsPerchedOrPerching;
        public bool IsExternalControlActive => _externalControlActive;

        public void SetExternalControl(bool active)
        {
            _externalControlActive = active;
            if (_body == null || _collider == null) return;

            if (active)
            {
                _collider.enabled = false;
                _body.bodyType = RigidbodyType2D.Kinematic;
                _body.linearVelocity = Vector2.zero;
            }
            else
            {
                _body.bodyType = RigidbodyType2D.Dynamic;
                _collider.enabled = true;
                if (_model != null)
                {
                    _model.Position = _body.position;
                    _model.Velocity = Vector2.zero;
                }
            }
        }

        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
            _collider = GetComponent<BoxCollider2D>();
            if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            _baseScale = transform.localScale;

            _body.gravityScale = 0f;
            _body.freezeRotation = true;
            _body.interpolation = RigidbodyInterpolation2D.Interpolate;

            if (config == null)
            {
                Debug.LogError("[CatController] CatConfigSO is not assigned.", this);
                return;
            }

            _model = new CatFollowModel(config, transform.position);
        }

        private void OnEnable()
        {
            if (rangedCombat != null)
            {
                rangedCombat.OnShotFired += HandleCombatActivity;
            }
        }

        private void OnDisable()
        {
            if (rangedCombat != null)
            {
                rangedCombat.OnShotFired -= HandleCombatActivity;
            }
        }

        private void HandleCombatActivity()
        {
            if (config != null)
            {
                _combatHoldTimer = config.combatPerchHoldDuration;
            }
        }

        private void FixedUpdate()
        {
            if (_externalControlActive || _model == null || config == null || playerController == null) return;

            if (meleeCombat != null && meleeCombat.IsAttacking)
            {
                _combatHoldTimer = config.combatPerchHoldDuration;
            }
            else if (_combatHoldTimer > 0f)
            {
                _combatHoldTimer = Mathf.Max(0f, _combatHoldTimer - Time.fixedDeltaTime);
            }

            PlayerLocomotionState danteState = playerController.State;
            bool isClimbing = danteState == PlayerLocomotionState.WallSlide || danteState == PlayerLocomotionState.WallJump;
            bool isAirborne = danteState == PlayerLocomotionState.JumpAscend || danteState == PlayerLocomotionState.Fall;

            float heightAboveCat = playerController.transform.position.y - _body.position.y;
            if (!IsPerched && (isAirborne || heightAboveCat > config.highGroundPerchThresholdY))
            {
                _highGroundTimer += Time.fixedDeltaTime;
            }
            else if (!isAirborne && heightAboveCat <= config.highGroundPerchThresholdY)
            {
                _highGroundTimer = 0f;
            }

            if (isClimbing || (isAirborne && _highGroundTimer >= config.highGroundPerchDelay) || (!IsPerched && _highGroundTimer >= config.highGroundPerchDelay))
            {
                _traversalHoldTimer = config.traversalPerchHoldDuration;
            }
            else if (_traversalHoldTimer > 0f)
            {
                _traversalHoldTimer = Mathf.Max(0f, _traversalHoldTimer - Time.fixedDeltaTime);
            }

            bool shouldPerch = isClimbing || _combatHoldTimer > 0f || _traversalHoldTimer > 0f;

            bool wasPerched = _model.IsPerchedOrPerching;
            if (!wasPerched)
            {
                _model.Position = _body.position;
                _model.Velocity = _body.linearVelocity;
            }

            bool grounded = !shouldPerch && CheckGround();
            var input = new CatFollowInput
            {
                dantePosition = playerController.transform.position,
                facingDirection = playerController.FacingDirection,
                shouldPerch = shouldPerch,
                grounded = grounded
            };

            _model.Step(Time.fixedDeltaTime, input);

            if (_model.IsPerchedOrPerching)
            {
                if (!wasPerched)
                {
                    _collider.enabled = false;
                    _body.bodyType = RigidbodyType2D.Kinematic;
                }
                _body.linearVelocity = Vector2.zero;
                _body.MovePosition(_model.Position);
            }
            else
            {
                if (wasPerched)
                {
                    _body.bodyType = RigidbodyType2D.Dynamic;
                    _collider.enabled = true;
                }

                // If the model triggered a safety teleport, snap the Rigidbody2D immediately.
                if (Vector2.Distance(_body.position, _model.Position) > 1.5f)
                {
                    _body.position = _model.Position;
                }
                _body.linearVelocity = _model.Velocity;
            }

            UpdateVisualFacing();
        }

        private bool CheckGround()
        {
            if (!_collider.enabled) return false;
            Bounds b = _collider.bounds;
            RaycastHit2D[] hits = Physics2D.BoxCastAll(
                b.center,
                new Vector2(b.size.x * 0.9f, b.size.y),
                0f,
                Vector2.down,
                GroundProbeDistance,
                groundMask);

            for (int i = 0; i < hits.Length; i++)
            {
                RaycastHit2D hit = hits[i];
                if (hit.collider == null || hit.normal.y <= 0.5f) continue;

                bool isOneWay = (oneWayMask.value & (1 << hit.collider.gameObject.layer)) != 0;
                if (isOneWay)
                {
                    if (_body.linearVelocity.y > 0.05f) continue;
                    if (b.min.y < hit.collider.bounds.max.y - 0.08f) continue;
                }

                return true;
            }
            return false;
        }

        private void UpdateVisualFacing()
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = _model.FacingDirection < 0;
            }
            else
            {
                transform.localScale = new Vector3(
                    Mathf.Abs(_baseScale.x) * _model.FacingDirection,
                    _baseScale.y,
                    _baseScale.z);
            }
        }
    }
}
