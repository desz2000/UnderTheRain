using NUnit.Framework;
using UnityEngine;
using UnderTheRain.Companion;
using UnderTheRain.Enemies;

namespace UnderTheRain.Tests.EditMode
{
    public class CatInterferenceTests
    {
        private sealed class FakeAffectable : ICatAffectable
        {
            public bool IsAffectedByCat { get; }
            public bool IsHesitating { get; private set; }
            public float LastDuration { get; private set; }

            public FakeAffectable(bool affectedByCat) => IsAffectedByCat = affectedByCat;

            public bool TryApplyCatInterference(float duration)
            {
                if (!IsAffectedByCat || IsHesitating) return false;
                IsHesitating = true;
                LastDuration = duration;
                return true;
            }
        }

        private CatConfigSO _config;
        private CatInterferenceModel _model;

        [SetUp]
        public void SetUp()
        {
            _config = ScriptableObject.CreateInstance<CatConfigSO>();
            _config.interferenceRadius = 4.5f;
            _config.interferenceCooldown = 3.0f;
            _config.hesitationDuration = 1.35f;
            _model = new CatInterferenceModel(_config);
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(_config);

        [Test]
        public void TryTriggerPulse_AffectsCollectiveMachine_AndIgnoresMisanthrope()
        {
            var robot = new FakeAffectable(affectedByCat: true);
            var misanthrope = new FakeAffectable(affectedByCat: false);

            bool triggered = _model.TryTriggerPulse(new ICatAffectable[] { robot, misanthrope }, out int count);

            Assert.IsTrue(triggered);
            Assert.AreEqual(1, count);
            Assert.IsTrue(robot.IsHesitating);
            Assert.AreEqual(1.35f, robot.LastDuration, 0.001f);
            Assert.IsFalse(misanthrope.IsHesitating);
        }

        [Test]
        public void TryTriggerPulse_EnforcesCooldown_UntilElapsed()
        {
            var robot = new FakeAffectable(affectedByCat: true);
            Assert.IsTrue(_model.TryTriggerPulse(new ICatAffectable[] { robot }, out _));
            Assert.IsFalse(_model.IsReady);

            // Second call during cooldown fails.
            Assert.IsFalse(_model.TryTriggerPulse(new ICatAffectable[] { robot }, out int countDuringCooldown));
            Assert.AreEqual(0, countDuringCooldown);

            _model.Tick(3.1f);
            Assert.IsTrue(_model.IsReady);
        }
    }
}
