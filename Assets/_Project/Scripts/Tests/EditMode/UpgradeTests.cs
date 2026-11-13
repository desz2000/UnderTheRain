using NUnit.Framework;
using UnderTheRain.Combat;
using UnderTheRain.Player;

namespace UnderTheRain.Tests.EditMode
{
    public class UpgradeTests
    {
        [SetUp]
        public void SetUp() => PlayerUpgradeSession.Reset();

        [TearDown]
        public void TearDown() => PlayerUpgradeSession.Reset();

        [Test]
        public void BatteryModel_IncreaseMax_RaisesCapacityAndCharge()
        {
            var battery = new BatteryModel(100f);
            battery.TryConsume(40f);
            battery.IncreaseMax(25f);

            Assert.AreEqual(125f, battery.MaxCharge, 0.001f);
            Assert.AreEqual(85f, battery.CurrentCharge, 0.001f);
        }

        [Test]
        public void HealthModel_IncreaseMax_AddsCellAndHealsIt()
        {
            var health = new HealthModel(5);
            health.TryTakeDamage(new DamageContext { amount = 2 });
            health.IncreaseMax(1);

            Assert.AreEqual(6, health.MaxHealth);
            Assert.AreEqual(4, health.CurrentHealth);
        }

        [Test]
        public void UpgradeSession_CollectsEachIdOnce()
        {
            Assert.IsTrue(PlayerUpgradeSession.TryCollect("archive_battery", UpgradeType.BatteryCapacity, 25f));
            Assert.IsFalse(PlayerUpgradeSession.TryCollect("archive_battery", UpgradeType.BatteryCapacity, 25f));
            Assert.IsTrue(PlayerUpgradeSession.TryCollect("chimney_cell", UpgradeType.HealthCell, 1f));

            Assert.AreEqual(25f, PlayerUpgradeSession.BatteryBonus, 0.001f);
            Assert.AreEqual(1, PlayerUpgradeSession.HealthBonus);
            Assert.IsTrue(PlayerUpgradeSession.IsCollected("archive_battery"));
        }
    }
}
