using NUnit.Framework;
using UnderTheRain.Companion;

namespace UnderTheRain.Tests.EditMode
{
    public class CatExplorationTests
    {
        [Test]
        public void TryBegin_SucceedsOnce_WhenPointAvailableAndIncomplete()
        {
            var model = new CatExplorationModel();

            Assert.IsTrue(model.TryBegin(pointAvailable: true, pointAlreadyCompleted: false));
            Assert.IsTrue(model.IsExploring);

            // Concurrent trigger while exploring must be rejected.
            Assert.IsFalse(model.TryBegin(pointAvailable: true, pointAlreadyCompleted: false));

            model.Finish();
            Assert.IsFalse(model.IsExploring);
        }

        [Test]
        public void TryBegin_Rejects_WhenNoPointOrAlreadyCompleted()
        {
            var model = new CatExplorationModel();

            Assert.IsFalse(model.TryBegin(pointAvailable: false, pointAlreadyCompleted: false));
            Assert.IsFalse(model.TryBegin(pointAvailable: true, pointAlreadyCompleted: true));
            Assert.IsFalse(model.IsExploring);
        }
    }
}
