using NUnit.Framework;
using UnderTheRain.Combat;
using UnderTheRain.Player;

namespace UnderTheRain.Tests.EditMode
{
    public class PartsPackTests
    {
        [Test]
        public void AddParts_ClampsToMaxCapacity()
        {
            var pack = new PartsPackModel(maxCapacity: 50);
            Assert.AreEqual(30, pack.AddParts(30));
            Assert.AreEqual(20, pack.AddParts(40));
            Assert.AreEqual(50, pack.CurrentParts);
            Assert.AreEqual(0, pack.AddParts(10));
        }

        [Test]
        public void TryCraftMedkit_SpendsPartsAndAddsCharge_WhenMissingHealthCharge()
        {
            var health = new HealthModel(5, 0f);
            var medkit = new MedkitModel(maxCharges: 3, healAmount: 2);
            var pack = new PartsPackModel(maxCapacity: 50, initialParts: 20);

            // Already at 3/3 charges -> cannot craft
            Assert.IsFalse(pack.TryCraftMedkit(medkit, 15));
            Assert.AreEqual(20, pack.CurrentParts);

            // Use 1 charge first
            health.TryTakeDamage(new DamageContext { amount = 2 });
            Assert.IsTrue(medkit.TryUse(health));
            Assert.AreEqual(2, medkit.CurrentCharges);

            Assert.IsTrue(pack.TryCraftMedkit(medkit, 15));
            Assert.AreEqual(3, medkit.CurrentCharges);
            Assert.AreEqual(5, pack.CurrentParts);
        }

        [Test]
        public void ExtractAllOnDeath_ZeroesPackAndReturnsDroppedAmount_ThenRecoveryRestoresIt()
        {
            var pack = new PartsPackModel(maxCapacity: 50, initialParts: 35);
            int dropped = pack.ExtractAllOnDeath();

            Assert.AreEqual(35, dropped);
            Assert.AreEqual(0, pack.CurrentParts);

            int recovered = pack.AddParts(dropped);
            Assert.AreEqual(35, recovered);
            Assert.AreEqual(35, pack.CurrentParts);
        }
    }
}
