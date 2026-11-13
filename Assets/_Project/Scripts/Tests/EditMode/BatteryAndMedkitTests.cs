using NUnit.Framework;
using UnderTheRain.Combat;
using UnderTheRain.Player;

namespace UnderTheRain.Tests.EditMode
{
    public class BatteryAndMedkitTests
    {
        [Test]
        public void Battery_ConsumesUntilEmpty_ThenBlocksShotsUntilRecharged()
        {
            var battery = new BatteryModel(100f);
            Assert.IsTrue(battery.TryConsume(50f));
            Assert.IsTrue(battery.TryConsume(50f));
            Assert.AreEqual(0f, battery.CurrentCharge, 0.001f);

            // Empty -> blocks new shot
            Assert.IsFalse(battery.TryConsume(25f));

            // Partial recharge allows shooting again
            battery.Recharge(35f);
            Assert.IsTrue(battery.TryConsume(25f));
            Assert.AreEqual(10f, battery.CurrentCharge, 0.001f);

            battery.RestoreFull();
            Assert.AreEqual(100f, battery.CurrentCharge, 0.001f);
        }

        [Test]
        public void Medkit_StartsWithThreeCharges_HealsDamagedHealth_AndSkipsWhenFull()
        {
            var health = new HealthModel(5, 0f);
            var medkit = new MedkitModel(maxCharges: 3, healAmount: 2);

            // Cannot waste a medkit at full health
            Assert.IsFalse(medkit.TryUse(health));
            Assert.AreEqual(3, medkit.CurrentCharges);

            health.TryTakeDamage(new DamageContext { amount = 3 });
            Assert.AreEqual(2, health.CurrentHealth);

            Assert.IsTrue(medkit.TryUse(health));
            Assert.AreEqual(4, health.CurrentHealth);
            Assert.AreEqual(2, medkit.CurrentCharges);

            medkit.RestoreFull();
            Assert.AreEqual(3, medkit.CurrentCharges);
        }

        [Test]
        public void Medkit_CannotBeUsed_WhenChargesExhausted()
        {
            var health = new HealthModel(10, 0f);
            var medkit = new MedkitModel(maxCharges: 2, healAmount: 1);

            health.TryTakeDamage(new DamageContext { amount = 5 });
            Assert.IsTrue(medkit.TryUse(health));
            Assert.IsTrue(medkit.TryUse(health));
            Assert.AreEqual(0, medkit.CurrentCharges);
            Assert.IsFalse(medkit.TryUse(health));
        }
    }
}
