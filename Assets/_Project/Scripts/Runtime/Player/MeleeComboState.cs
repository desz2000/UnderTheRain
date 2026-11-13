using System;
using UnityEngine;

namespace UnderTheRain.Player
{
    /// <summary>
    /// Pure 2-3 hit melee combo state machine, unit-testable in EditMode.
    /// </summary>
    public sealed class MeleeComboState
    {
        private readonly WeaponDataSO _weapon;
        private float _activeTimer;
        private float _comboWindowTimer;
        private float _cooldownTimer;

        public int CurrentStep { get; private set; }
        public bool IsAttacking => _activeTimer > 0f;
        public bool InComboWindow => !IsAttacking && _comboWindowTimer > 0f && CurrentStep > 0 && CurrentStep < _weapon.comboSteps;

        public event Action<int> OnAttackStarted;
        public event Action OnAttackEnded;

        public MeleeComboState(WeaponDataSO weapon)
        {
            _weapon = weapon;
        }

        public void Tick(float dt)
        {
            if (_cooldownTimer > 0f)
            {
                _cooldownTimer = Mathf.Max(0f, _cooldownTimer - dt);
            }

            if (_activeTimer > 0f)
            {
                _activeTimer = Mathf.Max(0f, _activeTimer - dt);
                if (_activeTimer <= 0f)
                {
                    OnAttackEnded?.Invoke();
                    if (CurrentStep >= _weapon.comboSteps)
                    {
                        CurrentStep = 0;
                        _comboWindowTimer = 0f;
                        _cooldownTimer = _weapon.comboResetCooldown;
                    }
                    else
                    {
                        _comboWindowTimer = _weapon.comboWindow;
                    }
                }
            }
            else if (_comboWindowTimer > 0f)
            {
                _comboWindowTimer = Mathf.Max(0f, _comboWindowTimer - dt);
                if (_comboWindowTimer <= 0f)
                {
                    CurrentStep = 0;
                }
            }
        }

        public bool TryAttack()
        {
            if (IsAttacking || _cooldownTimer > 0f) return false;

            CurrentStep = (_comboWindowTimer > 0f && CurrentStep < _weapon.comboSteps)
                ? CurrentStep + 1
                : 1;

            _comboWindowTimer = 0f;
            _activeTimer = _weapon.activeDuration;
            OnAttackStarted?.Invoke(CurrentStep);
            return true;
        }
    }
}
