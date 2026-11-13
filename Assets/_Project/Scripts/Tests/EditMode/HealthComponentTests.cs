using NUnit.Framework;
using UnityEngine;
using UnderTheRain.Combat;

namespace UnderTheRain.Tests.EditMode
{
    public class HealthComponentTests
    {
        [Test]
        public void TryTakeDamage_ReducesHealth_AndStartsInvulnerability()
        {
            var health = new HealthModel(5, 0.5f);
            bool hit = health.TryTakeDamage(new DamageContext { amount = 1 });

            Assert.IsTrue(hit);
            Assert.AreEqual(4, health.CurrentHealth);
            Assert.IsTrue(health.IsInvulnerable);
        }

        [Test]
        public void TryTakeDamage_IgnoredDuringInvulnerabilityWindow()
        {
            var health = new HealthModel(5, 0.5f);
            Assert.IsTrue(health.TryTakeDamage(new DamageContext { amount = 1 }));

            health.Tick(0.3f);
            Assert.IsFalse(health.TryTakeDamage(new DamageContext { amount = 1 }));
            Assert.AreEqual(4, health.CurrentHealth);

            health.Tick(0.3f); // total 0.6s > 0.5s
            Assert.IsTrue(health.TryTakeDamage(new DamageContext { amount = 1 }));
            Assert.AreEqual(3, health.CurrentHealth);
        }

        [Test]
        public void TryTakeDamage_FiresOnDied_WhenHealthReachesZero()
        {
            var health = new HealthModel(2, 0.1f);
            bool died = false;
            health.OnDied += () => died = true;

            health.TryTakeDamage(new DamageContext { amount = 2 });
            Assert.IsTrue(health.IsDead);
            Assert.IsTrue(died);
        }

        [Test]
        public void Heal_ClampsAtMaxHealth()
        {
            var health = new HealthModel(5, 0f);
            health.TryTakeDamage(new DamageContext { amount = 2 });
            health.Heal(10);
            Assert.AreEqual(5, health.CurrentHealth);
        }
    }
}
