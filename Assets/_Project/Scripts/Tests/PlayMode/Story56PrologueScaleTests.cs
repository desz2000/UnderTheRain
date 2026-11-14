using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
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
        private const float JumpHeight = 3.2f; // PlayerMovementConfig.jumpHeight (asset value)

        private static RoomTransitionController Transitions => Object.FindFirstObjectByType<RoomTransitionController>();
        private static GameObject Player => GameObject.FindGameObjectWithTag("Player");

        // Session state is static (until the Epic 8 save) and SubsystemRegistration only runs once per
        // Play Mode run, so every test starts and ends from a clean session.
        [SetUp]
        public void SetUp() => ResetSessionState();

        [TearDown]
        public void TearDown() => ResetSessionState();

        private static void ResetSessionState()
        {
            ArenaEncounter.ResetSession();
            SecretWallReveal.ResetSession();
            CatExplorationPoint.ResetSession();
            PlayerUpgradeSession.Reset();
        }

        private static RoomBounds Activate(float x, float y = 1f)
        {
            var room = Story54PrologueTests.RoomAt(x, y);
            Assert.IsNotNull(room, $"No room at ({x}, {y}).");
            Transitions.SetActiveRoomImmediate(room);
            return room;
        }

        /// <summary>Child of the room by name, including inactive ones (locks, opened doors).</summary>
        private static GameObject Child(Component root, string name)
        {
            foreach (var t in root.GetComponentsInChildren<Transform>(true))
            {
                if (t.name == name) return t.gameObject;
            }
            Assert.Fail($"'{name}' not found under {root.name}.");
            return null;
        }

        private static IEnumerator StartArena(RoomBounds lab, ArenaEncounter arena)
        {
            Story54PrologueTests.TeleportPlayer(new Vector3(232f + 16.5f, 1f, 0f));
            yield return new WaitForSeconds(0.3f);
            Assert.IsTrue(arena.IsActive, "Landing in the test chamber starts the encounter.");
            Assert.IsTrue(Child(lab, "Arena_Gate").activeSelf, "Gate to the server room closes during the fight.");
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
        public IEnumerator EveryRoomButP4_HasASecret_AndItPersists()
        {
            yield return Story54PrologueTests.LoadScene(PrologueScene);

            foreach (var (id, x) in Story54PrologueTests.Rooms)
            {
                int secrets = Story54PrologueTests.RoomAt(x, x > 280f ? -10f : 1f).GetComponentsInChildren<SecretWallReveal>(true).Length;
                if (id == "prologue_duct") Assert.AreEqual(0, secrets, "P4 is the quiet room: no secret.");
                else Assert.GreaterOrEqual(secrets, 1, $"{id} must hide a hinted secret.");
            }

            // Walk into the P1 ticket booth: the fake front fades and stays revealed after a reload.
            var p1 = Activate(0f);
            yield return null;
            var booth = Child(p1, "Secret_TicketBooth").GetComponent<SecretWallReveal>();
            Assert.IsFalse(booth.IsRevealed, "Clean session: the booth starts hidden.");
            Story54PrologueTests.TeleportPlayer(booth.transform.position);
            yield return new WaitForSeconds(0.6f);
            Assert.IsTrue(booth.IsRevealed);
            Assert.Less(Child(p1, "TicketBooth_Front").GetComponent<SpriteRenderer>().color.a, 0.5f);

            yield return Story54PrologueTests.LoadScene(PrologueScene);
            var reloaded = Child(Story54PrologueTests.RoomAt(0f), "Secret_TicketBooth").GetComponent<SecretWallReveal>();
            Assert.IsTrue(reloaded.IsRevealed);
        }

        [UnityTest]
        public IEnumerator LabArena_RobotsWakeLocksCloseThenReopenWhenCleared()
        {
            yield return Story54PrologueTests.LoadScene(PrologueScene);
            var lab = Activate(232f);
            yield return null;

            var arena = lab.GetComponentInChildren<ArenaEncounter>(true);
            Assert.IsNotNull(arena, "P5 needs an arena encounter.");
            var robots = new[] { "Arena_Scout_A", "Arena_Scout_B", "Arena_Worker" };
            foreach (var name in robots)
            {
                Assert.IsTrue(Child(lab, name).GetComponent<PatrolInterferenceTarget>().IsDormant, $"{name} should start dormant.");
            }

            yield return StartArena(lab, arena);
            Assert.IsFalse(Child(lab, "Arena_Worker").GetComponent<PatrolInterferenceTarget>().IsDormant, "Robots wake up.");

            foreach (var name in robots)
            {
                Child(lab, name).GetComponent<HealthComponent>().TryTakeDamage(new DamageContext { amount = 999 });
            }
            yield return new WaitForSeconds(0.3f);

            Assert.IsTrue(arena.IsCleared);
            Assert.IsFalse(arena.IsActive);
            Assert.IsFalse(Child(lab, "Arena_Gate").activeSelf, "Gate reopens once every robot falls.");
            Assert.IsFalse(Child(lab, "Arena_Shutter").activeSelf, "Shutter reopens too.");
        }

        [UnityTest]
        public IEnumerator LabArena_DyingMidFight_ReopensTheLocks()
        {
            yield return Story54PrologueTests.LoadScene(PrologueScene);
            var lab = Activate(232f);
            yield return null;
            var arena = lab.GetComponentInChildren<ArenaEncounter>(true);

            yield return StartArena(lab, arena);

            // Retry through any post-hit invulnerability (a robot may have touched Dante while the fight started).
            var health = Player.GetComponent<HealthComponent>();
            for (float t = 0f; !health.IsDead && t < 1f; t += Time.deltaTime)
            {
                health.TryTakeDamage(new DamageContext { amount = 999 });
                if (!health.IsDead) yield return null;
            }
            Assert.IsTrue(health.IsDead, "Precondition: Dante died.");

            Assert.IsFalse(arena.IsActive, "Death aborts the fight.");
            Assert.IsFalse(arena.IsCleared, "Aborting is not clearing.");
            Assert.IsFalse(Child(lab, "Arena_Gate").activeSelf, "Gate reopens so the path is never sealed.");
            Assert.IsFalse(Child(lab, "Arena_Shutter").activeSelf, "Shutter reopens too.");
        }

        [UnityTest]
        public IEnumerator LabArena_CraneNicheSecret_CountsAsInsideTheArena()
        {
            yield return Story54PrologueTests.LoadScene(PrologueScene);
            var lab = Activate(232f);
            yield return null;
            var arena = lab.GetComponentInChildren<ArenaEncounter>(true);

            yield return StartArena(lab, arena);

            // The niche is only reachable from the chamber: hiding in it must not abort the fight.
            Story54PrologueTests.TeleportPlayer(new Vector3(232f - 12f, 1f, 0f));
            yield return new WaitForSeconds(1.2f);

            Assert.IsTrue(arena.IsActive, "Standing in the crane niche keeps the fight running.");
            Assert.IsTrue(Child(lab, "Arena_Gate").activeSelf);
        }

        [UnityTest]
        public IEnumerator LabArena_FallingThroughTheHole_EndsInsideWithShutterClosedAbove()
        {
            // Regression (playtest): the shutter appeared inside Dante mid-fall and pushed him on top of it,
            // leaving the robots below and the gate closed (soft-lock).
            yield return Story54PrologueTests.LoadScene(PrologueScene);
            var lab = Activate(232f);
            yield return null;
            var arena = lab.GetComponentInChildren<ArenaEncounter>(true);

            Story54PrologueTests.TeleportPlayer(new Vector3(232f + 16.5f, 7.0f, 0f)); // inside the hole, at shutter height
            yield return new WaitForSeconds(1.0f);

            Assert.IsTrue(arena.IsActive, "The fight starts once Dante drops in.");
            Assert.Less(Player.transform.position.y, 6.5f, "Dante must end up inside the test chamber, not on the shutter.");
            Assert.IsTrue(Child(lab, "Arena_Shutter").activeSelf, "The shutter closes above him once he is clear of it.");
        }

        [UnityTest]
        public IEnumerator LabArena_LeavingTheArenaWhileActive_ReopensEverything()
        {
            yield return Story54PrologueTests.LoadScene(PrologueScene);
            var lab = Activate(232f);
            yield return null;
            var arena = lab.GetComponentInChildren<ArenaEncounter>(true);
            var gate = Child(lab, "Arena_Gate");
            var shutter = Child(lab, "Arena_Shutter");

            Story54PrologueTests.TeleportPlayer(new Vector3(232f + 2f, 1f, 0f));
            yield return new WaitForSeconds(0.3f);
            Assert.IsTrue(arena.IsActive);
            Assert.IsTrue(gate.activeSelf, "Precondition: the gate closed.");
            Assert.IsTrue(shutter.activeSelf, "Precondition: the shutter closed.");

            Story54PrologueTests.TeleportPlayer(new Vector3(232f - 5f, 8.2f, 0f)); // up on the deck, outside the chamber
            yield return new WaitForSeconds(1.2f);

            Assert.IsFalse(arena.IsActive, "Safety net: being outside the arena aborts the fight.");
            Assert.IsFalse(gate.activeSelf, "Gate reopens so the path is never sealed.");
            Assert.IsFalse(shutter.activeSelf, "Shutter reopens too.");
        }

        [UnityTest]
        public IEnumerator RestPointInP3_BecomesTheRespawnRoom()
        {
            yield return Story54PrologueTests.LoadScene(PrologueScene);
            var debris = Activate(118f);
            yield return null;

            var rest = debris.GetComponentInChildren<ShelterRestSpot>(true);
            Assert.IsNotNull(rest, "P3 needs the emergency generator rest point.");

            // Walk up to the generator and rest (the same path as pressing Interact in range).
            Story54PrologueTests.TeleportPlayer(rest.transform.position + Vector3.up * 0.2f);
            yield return new WaitForSeconds(0.3f);
            Assert.IsTrue(rest.TryRest(), "Dante in range of the generator can rest.");

            // Die in P5, away from the robots.
            Activate(232f);
            Story54PrologueTests.TeleportPlayer(new Vector3(232f - 5f, 8.2f, 0f));
            yield return null;
            Player.GetComponent<HealthComponent>().TryTakeDamage(new DamageContext { amount = 999 });
            yield return new WaitForSeconds(1.6f);

            Assert.AreEqual("prologue_debris", Transitions.CurrentRoom.RoomId, "Dante respawns at the P3 generator.");
        }

        [UnityTest]
        public IEnumerator ServiceTunnel_OpensOnlyFromP4_AndLeadsBackToP1()
        {
            yield return Story54PrologueTests.LoadScene(PrologueScene);
            var duct = Activate(174f);
            yield return null;

            var hatchCover = Child(duct, "Hatch_Cover");
            var armoredDoor = Child(duct, "Armored_Door");
            Assert.IsTrue(hatchCover.activeSelf, "Hatch starts closed.");
            Assert.IsTrue(armoredDoor.activeSelf, "Armored door to P5 starts closed.");
            Child(duct, "OfflinePanel").GetComponent<CatExplorationPoint>().Complete();
            Assert.IsFalse(hatchCover.activeSelf, "Kitty's panel opens the hatch...");
            Assert.IsFalse(armoredDoor.activeSelf, "...and the armored door.");

            // One-way: the hatch is the only exit in the whole scene that leads into the tunnel.
            var tunnelRoom = Story54PrologueTests.RoomAt(88f, -10f);
            Assert.IsNotNull(tunnelRoom, "Service tunnel room not found.");
            foreach (var root in SceneManager.GetSceneByName(PrologueScene).GetRootGameObjects())
            {
                foreach (var exit in root.GetComponentsInChildren<RoomExitTrigger>(true))
                {
                    if (exit.TargetRoom == tunnelRoom) Assert.AreEqual("Hatch_ToTunnel", exit.name, "Only P4's hatch leads into the tunnel.");
                }
            }

            var hatch = Child(duct, "Hatch_ToTunnel");
            Story54PrologueTests.TeleportPlayer(hatch.transform.position);
            yield return new WaitForSeconds(0.9f);
            Assert.AreEqual("prologue_service_tunnel", Transitions.CurrentRoom.RoomId);

            var toP1 = Story54PrologueTests.ExitOnSide(Transitions.CurrentRoom, rightSide: false);
            Story54PrologueTests.TeleportPlayer(toP1.transform.position);
            yield return new WaitForSeconds(0.9f);
            Assert.AreEqual("prologue_metro_platform", Transitions.CurrentRoom.RoomId);
        }

        [UnityTest]
        public IEnumerator ArchiveUpgrade_RaisesBatteryAndPersistsAcrossScenes()
        {
            yield return Story54PrologueTests.LoadScene(PrologueScene);
            var archive = Activate(280f, 13f);
            yield return null;

            var battery = Player.GetComponent<BatteryComponent>();
            float before = battery.MaxCharge;
            var upgrade = Child(archive, "BatteryCapacityUpgrade");
            Assert.IsTrue(upgrade.activeSelf, "Clean session: the archive upgrade is still there.");
            Story54PrologueTests.TeleportPlayer(upgrade.transform.position);
            yield return new WaitForSeconds(0.3f);

            Assert.AreEqual(before + 25f, battery.MaxCharge, 0.01f);
            Assert.IsFalse(upgrade.activeSelf, "Collected upgrades disappear.");

            yield return Story54PrologueTests.LoadScene(SewersScene);
            yield return null;
            Assert.AreEqual(before + 25f, Player.GetComponent<BatteryComponent>().MaxCharge, 0.01f, "Upgrade survives the scene change.");
            Assert.IsNotNull(Player.GetComponent<PlayerUpgradeApplier>(), "Dante carries the applier as a real script component.");
        }

        [UnityTest]
        public IEnumerator FatherSketch_SitsAboveTheChimney_OutOfReachWithoutWallJump()
        {
            yield return Story54PrologueTests.LoadScene(PrologueScene);
            var duct = Activate(174f);
            yield return null;

            var sketch = Child(duct, "FatherSketchTerminal").GetComponent<NarrativeTerminal>();
            Assert.IsNotNull(sketch, "P4 hides the first father sketch at the top of the chimney.");

            // Measured from the real colliders, so moving the ledge, corridor or terminal is caught.
            float corridorTop = Child(duct, "Service_Corridor").GetComponent<Collider2D>().bounds.max.y;
            float ledgeTop = Child(duct, "Chimney_Ledge").GetComponent<Collider2D>().bounds.max.y;
            float sketchBottom = sketch.GetComponent<Collider2D>().bounds.min.y;
            float danteHeight = Player.GetComponent<Collider2D>().bounds.size.y;

            Assert.Greater(ledgeTop - corridorTop, JumpHeight, "The chimney ledge must be above a normal jump (return gate).");
            Assert.Greater(sketchBottom, corridorTop + JumpHeight + danteHeight,
                "A normal jump from the corridor must not reach the sketch's interaction trigger.");
        }
    }
}
