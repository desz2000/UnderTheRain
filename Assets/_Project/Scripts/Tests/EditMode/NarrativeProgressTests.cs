using NUnit.Framework;
using UnderTheRain.Narrative;

namespace UnderTheRain.Tests.EditMode
{
    public class NarrativeProgressTests
    {
        [Test]
        public void TryCollectDocument_AddsOnceAndFiresEvent()
        {
            var model = new NarrativeProgressModel();
            string lastDoc = null;
            model.OnDocumentCollected += id => lastDoc = id;

            Assert.IsTrue(model.TryCollectDocument("doc_domus_sabotage"));
            Assert.IsTrue(model.HasDocument("doc_domus_sabotage"));
            Assert.AreEqual(1, model.CollectedDocumentCount);
            Assert.AreEqual("doc_domus_sabotage", lastDoc);

            // Duplicate collection must be ignored.
            Assert.IsFalse(model.TryCollectDocument("doc_domus_sabotage"));
            Assert.AreEqual(1, model.CollectedDocumentCount);
        }

        [Test]
        public void SetFlag_TracksUniqueFlagsAndRejectsEmpty()
        {
            var model = new NarrativeProgressModel();
            int eventCount = 0;
            model.OnFlagSet += _ => eventCount++;

            Assert.IsFalse(model.SetFlag(""));
            Assert.IsFalse(model.SetFlag(null));
            Assert.IsTrue(model.SetFlag(NarrativeState.FlagDomusEvidenceFound));
            Assert.IsFalse(model.SetFlag(NarrativeState.FlagDomusEvidenceFound));

            Assert.IsTrue(model.HasFlag(NarrativeState.FlagDomusEvidenceFound));
            Assert.AreEqual(1, eventCount);
        }

        [Test]
        public void Act1SewersFlow_EvidenceBeforeIcarusUnlocksExit()
        {
            var model = new NarrativeProgressModel();

            Assert.IsFalse(model.HasFlag(NarrativeState.FlagSewerExitUnlocked));

            model.TryCollectDocument("doc_domus_sabotage");
            model.SetFlag(NarrativeState.FlagDomusEvidenceFound);
            Assert.IsTrue(model.HasFlag(NarrativeState.FlagDomusEvidenceFound));

            model.SetFlag(NarrativeState.FlagIcarusConfronted);
            model.SetFlag(NarrativeState.FlagSewerExitUnlocked);

            Assert.IsTrue(model.HasFlag(NarrativeState.FlagIcarusConfronted));
            Assert.IsTrue(model.HasFlag(NarrativeState.FlagSewerExitUnlocked));
        }
    }
}
