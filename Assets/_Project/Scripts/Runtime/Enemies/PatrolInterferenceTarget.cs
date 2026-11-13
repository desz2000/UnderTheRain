using UnityEngine;

namespace UnderTheRain.Enemies
{
    /// <summary>
    /// Simple patrolling actor for validating cat interference behavior:
    /// CollectiveMachine targets hesitate and spark when pulsed; MisanthropeHuman targets ignore it.
    /// </summary>
    public sealed class PatrolInterferenceTarget : MonoBehaviour, ICatAffectable
    {
        [SerializeField] private EnemyFaction faction = EnemyFaction.CollectiveMachine;
        [SerializeField, Min(0.5f)] private float patrolSpeed = 2.2f;
        [SerializeField, Min(0.2f)] private float patrolHalfWidth = 2.0f;
        [SerializeField, Min(0f)] private float immunityCooldownAfterRecover = 1.5f;
        [SerializeField] private SpriteRenderer bodyRenderer;
        [SerializeField] private SpriteRenderer hesitationGlyph;

        private Vector3 _origin;
        private int _patrolDir = 1;
        private float _hesitationTimer;
        private float _immunityTimer;
        private Color _baseColor;

        public EnemyFaction Faction => faction;
        public bool IsAffectedByCat => faction == EnemyFaction.CollectiveMachine;
        public bool IsHesitating => _hesitationTimer > 0f;

        private static readonly Color HesitationColor = new Color(1f, 0.85f, 0.3f, 1f);

        /// <summary>Color the body should show right now (lets hit flashes restore the correct state).</summary>
        public Color CurrentBodyColor => IsHesitating ? HesitationColor : _baseColor;

        private void Awake()
        {
            _origin = transform.position;
            if (bodyRenderer == null) bodyRenderer = GetComponent<SpriteRenderer>();
            if (bodyRenderer != null) _baseColor = bodyRenderer.color;
            if (hesitationGlyph != null) hesitationGlyph.enabled = false;
        }

        public bool TryApplyCatInterference(float duration)
        {
            if (!IsAffectedByCat || IsHesitating || _immunityTimer > 0f) return false;

            _hesitationTimer = Mathf.Max(0.1f, duration);
            if (hesitationGlyph != null) hesitationGlyph.enabled = true;
            if (bodyRenderer != null) bodyRenderer.color = HesitationColor;
            return true;
        }

        private void Update()
        {
            if (_hesitationTimer > 0f)
            {
                _hesitationTimer = Mathf.Max(0f, _hesitationTimer - Time.deltaTime);
                if (_hesitationTimer <= 0f)
                {
                    _immunityTimer = immunityCooldownAfterRecover;
                    if (hesitationGlyph != null) hesitationGlyph.enabled = false;
                    if (bodyRenderer != null) bodyRenderer.color = _baseColor;
                }
                return;
            }

            if (_immunityTimer > 0f)
            {
                _immunityTimer = Mathf.Max(0f, _immunityTimer - Time.deltaTime);
            }

            Vector3 pos = transform.position;
            pos.x += _patrolDir * patrolSpeed * Time.deltaTime;
            if (pos.x >= _origin.x + patrolHalfWidth)
            {
                pos.x = _origin.x + patrolHalfWidth;
                _patrolDir = -1;
            }
            else if (pos.x <= _origin.x - patrolHalfWidth)
            {
                pos.x = _origin.x - patrolHalfWidth;
                _patrolDir = 1;
            }
            transform.position = pos;
        }
    }
}
