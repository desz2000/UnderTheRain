using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnderTheRain.Combat;
using UnderTheRain.Enemies;
using UnderTheRain.Narrative;
using UnderTheRain.Player;
using UnderTheRain.World;

namespace UnderTheRain.Tests.PlayMode
{
    /// <summary>
    /// Story 5.6 — larger, layered prologue: secrets, arena, rest point, one-way tunnel, upgrade, return gate.
    /// </summary>
    public class Story56PrologueScaleTests
    {
        private const string PrologueScene = "Prologue_Ruins";
        private const string SewersScene = "Act1_Sewers";
        private const float ScreenArea = 19.2f * 10.8f;

        private static RoomTransitionController Transitions => Object.FindFirstObjectByType<RoomTransitionController>();
        private static GameObject Player => GameObject.FindGameObjectWithTag("Player");

        private static RoomBounds Activate(float x, float y = 1f)
        {
            var room = Story54PrologueTests.RoomAt(x, y);
            Assert.IsNotNull(room, $"No room at ({x}, {y}).");
            Transitions.SetActiveRoomImmediate(room);
            return room;
        }

        [UnityTest]
        public IEnumerator MainRooms_AreAtLeastTwoScreens()
        {
            yield return Story54PrologueTests.LoadScene(PrologueScene);

            foreach (var (id, x) in Story54PrologueTests.Rooms)
            {
                var col = Story54PrologueTests.RoomAt(x).GetComponent<BoxCollider2D>();
                float area = col.size.x * col.size.y;
                Assert.GreaterOrEqual(area, 2f * ScreenArea, $"{id} is smaller than two screens ({col.size}).");
            }
        }

        [UnityTest]
        public IEnumerator Prologue_HasAtLeastFiveSecretsAndTheyPersist()
        {
            yield return Story54PrologueTests.LoadScene(PrologueScene);

            int secrets = 0;
            foreach (var (_, x) in Story54PrologueTests.Rooms)
            {
                secrets += Story54PrologueTests.RoomAt(x, x > 280f ? -10f : 1f).GetComponentsInChildren<SecretWallReveal>(true).Length;
            }
            Assert.GreaterOrEqual(secrets, 5, "Each room (except the quiet P4) hides a hinted secret.");

            // Walk into the P1 ticket booth: the fake front fades and stays revealed after a reload.
            var booth = GameObject.Find("Secret_TicketBooth").GetComponent<SecretWallReveal>();
            Story54PrologueTests.TeleportPlayer(booth.transform.position);
            yield return new WaitForSeconds(0.6f);
            Assert.IsTrue(booth.IsRevealed);
            Assert.Less(GameObject.Find("TicketBooth_Front").GetComponent<SpriteRenderer>().color.a, 0.5f);

            yield return Story54PrologueTests.LoadScene(PrologueScene);
            Assert.IsTrue(GameObject.Find("Secret_TicketBooth").GetComponent<SecretWallReveal>().IsRevealed);
        }

        [UnityTest]
        public IEnumerator LabArena_RobotsWakeLocksCloseThenReopenWhenCleared()
        {
            ArenaEncounter.ResetSession();
            yield return Story54PrologueTests.LoadScene(PrologueScene);
            var lab = Activate(232f);
            yield return null;

            var arena = lab.GetComponentInChildren<ArenaEncounter>(true);
            Assert.IsNotNull(arena, "P5 needs an arena encounter.");
            var robots = new[] { "Arena_Scout_A", "Arena_Scout_B", "Arena_Worker" };
            foreach (var name in robots)
            {
                Assert.IsTrue(GameObject.Find(name).GetComponent<PatrolInterferenceTarget>().IsDormant, $"{name} should start dormant.");
            }

            Story54PrologueTests.TeleportPlayer(new Vector3(232f + 16.5f, 1f, 0f));
            yield return new WaitForSeconds(0.3f);

            Assert.IsTrue(arena.IsActive, "Landing in the test chamber starts the encounter.");
            Assert.IsTrue(GameObject.Find("Arena_Gate").activeSelf, "Gate to the server room closes during the fight.");
            Assert.IsFalse(GameObject.Find("Arena_Worker").GetComponent<PatrolInterferenceTarget>().IsDormant, "Robots wake up.");

            foreach (var name in robots)
            {
                GameObject.Find(name).GetComponent<HealthComponent>().TryTakeDamage(new DamageContext { amount = 999 });
            }
            yield return new WaitForSeconds(0.3f);

            Assert.IsTrue(arena.IsCleared);
            Assert.IsFalse(arena.IsActive);
            Assert.IsNull(GameObject.Find("Arena_Gate"), "Gate reopens (inactive) once every robot falls.");
        }

        [UnityTest]
        public IEnumerator LabArena_FallingThroughTheHole_EndsInsideWithShutterClosedAbove()
        {
            // Regression (playtest): the shutter appeared inside Dante mid-fall and pushed him on top of it,
            // leaving the robots below and the gate closed (soft-lock).
            ArenaEncounter.ResetSession();
            yield return Story54PrologueTests.LoadScene(PrologueScene);
            var lab = Activate(232f);
            yield return null;
            var arena = lab.GetComponentInChildren<ArenaEncounter>(true);

            Story54PrologueTests.TeleportPlayer(new Vector3(232f + 16.5f, 7.0f, 0f)); // inside the hole, at shutter height
            yield return new WaitForSeconds(1.0f);

            Assert.IsTrue(arena.IsActive, "The fight starts once Dante drops in.");
            Assert.Less(Player.transform.position.y, 6.5f, "Dante must end up inside the test chamber, not on the shutter.");
            Assert.IsTrue(GameObject.Find("Arena_Shutter").activeSelf, "The shutter closes above him once he is clear of it.");
        }

        [UnityTest]
        public IEnumerator LabArena_LeavingTheArenaWhileActive_ReopensEverything()
        {
            ArenaEncounter.ResetSession();
            yield return Story54PrologueTests.LoadScene(PrologueScene);
            var lab = Activate(232f);
            yield return null;
            var arena = lab.GetComponentInChildren<ArenaEncounter>(true);

            Story54PrologueTests.TeleportPlayer(new Vector3(232f + 2f, 1f, 0f));
            yield return new WaitForSeconds(0.3f);
            Assert.IsTrue(arena.IsActive);

            Story54PrologueTests.TeleportPlayer(new Vector3(232f - 5f, 8.2f, 0f)); // up on the deck, outside the chamber
            yield return new WaitForSeconds(1.2f);

            Assert.IsFalse(arena.IsActive, "Safety net: being outside the arena aborts the fight.");
            Assert.IsNull(GameObject.Find("Arena_Gate"), "Gate reopens so the path is never sealed.");
            Assert.IsNull(GameObject.Find("Arena_Shutter"), "Shutter reopens too.");
        }

        [UnityTest]
        public IEnumerator RestPointInP3_BecomesTheRespawnRoom()
        {
            yield return Story54PrologueTests.LoadScene(PrologueScene);
            var debris = Activate(118f);
            yield return null;

            var rest = debris.GetComponentInChildren<ShelterRestSpot>(true);
            Assert.IsNotNull(rest, "P3 needs the emergency generator rest point.");

            // Simulate resting (ShelterRestSpot calls SetRespawnPoint on Interact), then die in P5.
            var player = Player;
            player.GetComponent<PlayerDeathHandler>().SetRespawnPoint(rest.transform.position);
            Activate(232f);
            Story54PrologueTests.TeleportPlayer(new Vector3(232f - 25f, 8.2f, 0f));
            yield return null;

            player.GetComponent<HealthComponent>().TryTakeDamage(new DamageContext { amount = 999 });
            yield return new WaitForSeconds(1.6f);

            Assert.AreEqual("prologue_debris", Transitions.CurrentRoom.RoomId, "Dante respawns at the P3 generator.");
        }

        [UnityTest]
        public IEnumerator ServiceTunnel_OpensOnlyFromP4_AndLeadsBackToP1()
        {
            yield return Story54PrologueTests.LoadScene(PrologueScene);
            Activate(174f);
            yield return null;

            Assert.IsTrue(GameObject.Find("Hatch_Cover").activeSelf, "Hatch starts closed.");
            Assert.IsTrue(GameObject.Find("Armored_Door").activeSelf, "Armored door to P5 starts closed.");
            GameObject.Find("OfflinePanel").GetComponent<CatExplorationPoint>().Complete();
            Assert.IsNull(GameObject.Find("Hatch_Cover"), "Kitty's panel opens the hatch...");
            Assert.IsNull(GameObject.Find("Armored_Door"), "...and the armored door.");

            var hatch = GameObject.Find("Hatch_ToTunnel");
            Story54PrologueTests.TeleportPlayer(hatch.transform.position);
            yield return new WaitForSeconds(0.9f);
            Assert.AreEqual("prologue_service_tunnel", Transitions.CurrentRoom.RoomId);

            var toP1 = Story54PrologueTests.ExitOnSide(Transitions.CurrentRoom, rightSide: false);
            Story54PrologueTests.TeleportPlayer(toP1.transform.position);
            yield return new WaitForSeconds(0.9f);
            Assert.AreEqual("prologue_metro_platform", Transitions.CurrentRoom.RoomId);

            // P1 has no way into the tunnel: the shortcut is one-way.
            foreach (var exit in Transitions.CurrentRoom.GetComponentsInChildren<RoomExitTrigger>(true))
            {
                Assert.IsFalse(exit.name.Contains("Tunnel"), "P1 must not lead into the tunnel.");
            }
        }

        [UnityTest]
        public IEnumerator ArchiveUpgrade_RaisesBatteryAndPersistsAcrossScenes()
        {
            yield return Story54PrologueTests.LoadScene(PrologueScene);
            Activate(280f, 13f);
            yield return null;

            var battery = Player.GetComponent<BatteryComponent>();
            float before = battery.MaxCharge;
            var upgrade = GameObject.Find("BatteryCapacityUpgrade");
            Assert.IsNotNull(upgrade, "Archive holds the battery capacity upgrade.");
            Story54PrologueTests.TeleportPlayer(upgrade.transform.position);
            yield return new WaitForSeconds(0.3f);

            Assert.AreEqual(before + 25f, battery.MaxCharge, 0.01f);

            yield return Story54PrologueTests.LoadScene(SewersScene);
            yield return null;
            Assert.AreEqual(before + 25f, Player.GetComponent<BatteryComponent>().MaxCharge, 0.01f, "Upgrade survives the scene change.");
        }

        [UnityTest]
        public IEnumerator FatherSketch_SitsAboveTheChimney_OutOfReachWithoutWallJump()
        {
            yield return Story54PrologueTests.LoadScene(PrologueScene);
            var duct = Activate(174f);
            yield return null;

            NarrativeTerminal sketch = null;
            foreach (var t in duct.GetComponentsInChildren<NarrativeTerminal>(true))
            {
                if (t.name == "FatherSketchTerminal") sketch = t;
            }
            Assert.IsNotNull(sketch, "P4 hides the first father sketch at the top of the chimney.");

            const float corridorFloor = 7.2f;
            float ledgeTop = sketch.transform.position.y - 0.7f;
            Assert.Greater(ledgeTop - corridorFloor, 3.2f, "The chimney ledge must be above a normal jump (return gate).");
        }
    }
}
