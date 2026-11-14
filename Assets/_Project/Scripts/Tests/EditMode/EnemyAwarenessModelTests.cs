using NUnit.Framework;
using UnderTheRain.Enemies;

namespace UnderTheRain.Tests.EditMode
{
    /// <summary>Story 5.9 — awareness state machine of hive robots (pure logic).</summary>
    public class EnemyAwarenessModelTests
    {
        private const float Reaction = 0.35f;
        private const float Memory = 2.5f;

        private static EnemyAwarenessModel NewModel() => new EnemyAwarenessModel(Reaction, Memory);

        private static EnemyAwarenessModel NewChasingModel()
        {
            var model = NewModel();
            model.Tick(0.1f, true);
            model.Tick(Reaction + 0.01f, true);
            Assert.AreEqual(EnemyAwarenessState.Chasing, model.State);
            return model;
        }

        [Test]
        public void Idle_StaysIdle_WhileTargetIsNotSeen()
        {
            var model = NewModel();
            Assert.IsFalse(model.Tick(1f, false));
            Assert.AreEqual(EnemyAwarenessState.Idle, model.State);
            Assert.IsFalse(model.IsEngaged);
        }

        [Test]
        public void SeeingTarget_AlertsFirst_ThenChasesAfterTheReactionTime()
        {
            var model = NewModel();

            Assert.IsTrue(model.Tick(0.1f, true), "The tick that notices the target reports it (hive call).");
            Assert.AreEqual(EnemyAwarenessState.Alerted, model.State);

            Assert.IsFalse(model.Tick(Reaction * 0.5f, true));
            Assert.AreEqual(EnemyAwarenessState.Alerted, model.State, "Still in the readable reaction pause.");

            model.Tick(Reaction, true);
            Assert.AreEqual(EnemyAwarenessState.Chasing, model.State);
        }

        [Test]
        public void LosingSight_Searches_ThenReturnsWhenMemoryRunsOut()
        {
            var model = NewChasingModel();

            model.Tick(0.1f, false);
            Assert.AreEqual(EnemyAwarenessState.Searching, model.State);
            Assert.IsTrue(model.IsEngaged);

            model.Tick(Memory * 0.5f, false);
            Assert.AreEqual(EnemyAwarenessState.Searching, model.State);

            model.Tick(Memory, false);
            Assert.AreEqual(EnemyAwarenessState.Returning, model.State);
            Assert.IsFalse(model.IsEngaged);
        }

        [Test]
        public void Searching_ResumesChaseImmediately_WhenTargetIsSeenAgain()
        {
            var model = NewChasingModel();
            model.Tick(0.1f, false);

            Assert.IsFalse(model.Tick(0.1f, true), "Re-acquiring is not a new notice.");
            Assert.AreEqual(EnemyAwarenessState.Chasing, model.State);
        }

        [Test]
        public void Returning_BecomesIdleOnlyAtThePost_AndStillNoticesTheTarget()
        {
            var model = NewChasingModel();
            model.Tick(0.1f, false);
            model.Tick(Memory + 0.1f, false);
            Assert.AreEqual(EnemyAwarenessState.Returning, model.State);

            Assert.IsTrue(model.Tick(0.1f, true));
            Assert.AreEqual(EnemyAwarenessState.Alerted, model.State);

            model.Reset();
            model.NotifyReturned();
            Assert.AreEqual(EnemyAwarenessState.Idle, model.State);
        }

        [Test]
        public void NotifyReturned_OnlyAppliesWhileReturning()
        {
            var model = NewChasingModel();
            model.NotifyReturned();
            Assert.AreEqual(EnemyAwarenessState.Chasing, model.State);
        }

        [Test]
        public void ForceAlert_AlertsAnUnawareRobot_AndRenewsASearch()
        {
            var model = NewModel();
            Assert.IsTrue(model.ForceAlert());
            Assert.AreEqual(EnemyAwarenessState.Alerted, model.State);
            Assert.IsFalse(model.ForceAlert(), "Already engaged: nothing changes.");

            model.Tick(Reaction + 0.01f, false);
            model.Tick(0.1f, false);
            Assert.AreEqual(EnemyAwarenessState.Searching, model.State);

            model.Tick(Memory - 0.2f, false);
            model.ForceAlert();
            model.Tick(Memory - 0.2f, false);
            Assert.AreEqual(EnemyAwarenessState.Searching, model.State, "A fresh report renews the memory.");
        }
    }
}
