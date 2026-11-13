using System;
using System.Collections.Generic;
using UnityEngine;
using UnderTheRain.Enemies;

namespace UnderTheRain.Companion
{
    /// <summary>
    /// Pure cooldown and target-filtering model for the cat's active interference pulse.
    /// </summary>
    public sealed class CatInterferenceModel
    {
        private readonly CatConfigSO _config;

        public float CooldownRemaining { get; private set; }
        public bool IsReady => CooldownRemaining <= 0f;
        public float CooldownNormalized => _config.interferenceCooldown > 0f
            ? Mathf.Clamp01(CooldownRemaining / _config.interferenceCooldown)
            : 0f;

        public event Action<int> OnPulseTriggered;

        public CatInterferenceModel(CatConfigSO config)
        {
            _config = config;
        }

        public void Tick(float deltaTime)
        {
            if (CooldownRemaining > 0f)
            {
                CooldownRemaining = Mathf.Max(0f, CooldownRemaining - deltaTime);
            }
        }

        public bool TryTriggerPulse(IEnumerable<ICatAffectable> candidates, out int affectedCount)
        {
            affectedCount = 0;
            if (!IsReady) return false;

            CooldownRemaining = _config.interferenceCooldown;
            if (candidates != null)
            {
                foreach (ICatAffectable target in candidates)
                {
                    if (target == null || !target.IsAffectedByCat) continue;
                    if (target.TryApplyCatInterference(_config.hesitationDuration))
                    {
                        affectedCount++;
                    }
                }
            }

            OnPulseTriggered?.Invoke(affectedCount);
            return true;
        }
    }
}
