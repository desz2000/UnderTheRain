using NUnit.Framework;
using UnityEngine;
using UnderTheRain.Player;

namespace UnderTheRain.Tests.EditMode
{
    public class MeleeComboTests
    {
        private WeaponDataSO _weapon;
        private MeleeComboState _combo;

        [SetUp]
        public void SetUp()
        {
            _weapon = ScriptableObject.CreateInstance<WeaponDataSO>();
            _weapon.comboSteps = 3;
            _weapon.activeDuration = 0.14f;
            _weapon.comboWindow = 0.35f;
            _weapon.comboResetCooldown = 0.22f;
            _combo = new MeleeComboState(_weapon);
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(_weapon);

        [Test]
        public void TryAttack_ChainsSteps1To3_WithinComboWindow()
        {
            Assert.IsTrue(_combo.TryAttack());
            Assert.AreEqual(1, _combo.CurrentStep);
            Assert.IsFalse(_combo.TryAttack()); // still active

            _combo.Tick(0.15f); // attack ends, combo window open
            Assert.IsTrue(_combo.InComboWindow);

            Assert.IsTrue(_combo.TryAttack());
            Assert.AreEqual(2, _combo.CurrentStep);

            _combo.Tick(0.15f);
            Assert.IsTrue(_combo.TryAttack());
            Assert.AreEqual(3, _combo.CurrentStep);
        }

        [Test]
        public void TryAttack_ResetsToStep1_WhenComboWindowExpires()
        {
            _combo.TryAttack();
            _combo.Tick(0.15f); // ends attack
            _combo.Tick(0.40f); // > 0.35s combo window

            Assert.IsTrue(_combo.TryAttack());
            Assert.AreEqual(1, _combo.CurrentStep);
        }

        [Test]
        public void TryAttack_EnforcesCooldownAfterFinalComboStep()
        {
            _combo.TryAttack();
            _combo.Tick(0.15f);
            _combo.TryAttack();
            _combo.Tick(0.15f);
            _combo.TryAttack();
            _combo.Tick(0.15f); // step 3 ends -> cooldown starts

            Assert.IsFalse(_combo.TryAttack());
            _combo.Tick(0.25f); // > 0.22s cooldown
            Assert.IsTrue(_combo.TryAttack());
            Assert.AreEqual(1, _combo.CurrentStep);
        }

        [Test]
        public void GetDamageForStep_AppliesBonusOnFinalComboStep()
        {
            _weapon.damage = 1;
            _weapon.finalComboDamageBonus = 1;

            Assert.AreEqual(1, _weapon.GetDamageForStep(1));
            Assert.AreEqual(1, _weapon.GetDamageForStep(2));
            Assert.AreEqual(2, _weapon.GetDamageForStep(3));
        }
    }
}
