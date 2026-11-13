using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnderTheRain.Core.Input;
using UnderTheRain.Enemies;
using UnderTheRain.Player;

namespace UnderTheRain.Companion
{
    /// <summary>
    /// Active interference command for the metal cat: emits a short-range neural pulse
    /// on command, stalling CollectiveMachine enemies while MisanthropeHumans ignore it.
    /// </summary>
    public sealed class CatInterference : MonoBehaviour
    {
        [SerializeField] private GameInputReader input;
        [SerializeField] private CatConfigSO config;
        [SerializeField] private BatteryComponent battery;
        [SerializeField] private SpriteRenderer pulseVisual;
        [SerializeField] private Color pulseColor = new Color(0.306f, 0.804f, 0.769f, 0.55f);

        private CatInterferenceModel _model;
        private Coroutine _pulseRoutine;
        private readonly HashSet<ICatAffectable> _uniqueTargets = new HashSet<ICatAffectable>();

        public bool IsReady => _model != null && _model.IsReady;
        public float CooldownRemaining => _model != null ? _model.CooldownRemaining : 0f;
        public float CooldownNormalized => _model != null ? _model.CooldownNormalized : 0f;

        public event Action<int> OnInterferenceTriggered;

        private void Awake()
        {
            if (config == null && TryGetComponent(out CatController controller))
            {
                config = controller.Config;
            }
            if (config == null)
            {
                Debug.LogError("[CatInterference] CatConfigSO is not assigned.", this);
                return;
            }

            _model = new CatInterferenceModel(config);
            _model.OnPulseTriggered += HandlePulseTriggered;

            if (pulseVisual != null)
            {
                pulseVisual.enabled = false;
            }
        }

        private void OnEnable()
        {
            if (input != null) input.OnCatCommandPressed += HandleCommandPressed;
        }

        private void OnDisable()
        {
            if (input != null) input.OnCatCommandPressed -= HandleCommandPressed;
        }

        private void Update()
        {
            _model?.Tick(Time.deltaTime);
        }

        private void HandleCommandPressed()
        {
            TryTrigger();
        }

        public bool TryTrigger()
        {
            if (_model == null || config == null || !_model.IsReady) return false;

            if (battery != null && config.interferenceBatteryCost > 0f && !battery.TryConsume(config.interferenceBatteryCost))
            {
                Debug.Log("[CatInterference] Out of battery!");
                return false;
            }

            _uniqueTargets.Clear();
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, config.interferenceRadius);
            for (int i = 0; i < hits.Length; i++)
            {
                ICatAffectable target = hits[i].GetComponentInParent<ICatAffectable>();
                if (target != null)
                {
                    _uniqueTargets.Add(target);
                }
            }

            return _model.TryTriggerPulse(_uniqueTargets, out _);
        }

        private void HandlePulseTriggered(int affectedCount)
        {
            if (pulseVisual != null)
            {
                if (_pulseRoutine != null) StopCoroutine(_pulseRoutine);
                _pulseRoutine = StartCoroutine(PulseVisualRoutine());
            }

            OnInterferenceTriggered?.Invoke(affectedCount);
        }

        private IEnumerator PulseVisualRoutine()
        {
            pulseVisual.enabled = true;
            float duration = 0.25f;
            float elapsed = 0f;
            float targetDiameter = config.interferenceRadius * 2f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float scale = Mathf.Lerp(0.4f, targetDiameter, t);
                pulseVisual.transform.localScale = new Vector3(scale, scale, 1f);
                pulseVisual.color = new Color(pulseColor.r, pulseColor.g, pulseColor.b, pulseColor.a * (1f - t));
                yield return null;
            }

            pulseVisual.enabled = false;
            _pulseRoutine = null;
        }

        private void OnDrawGizmosSelected()
        {
            if (config == null) return;
            Gizmos.color = new Color(0.306f, 0.804f, 0.769f, 0.4f);
            Gizmos.DrawWireSphere(transform.position, config.interferenceRadius);
        }
    }
}
