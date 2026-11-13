using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnderTheRain.Combat;
using UnderTheRain.World;

namespace UnderTheRain.Tests.PlayMode
{
    /// <summary>
    /// Smoke tests over the generated Story 5.1 scenes (Prologue_Ruins / Act1_Sewers).
    /// Requires both scenes in Build Settings (added by their Setup menus).
    /// </summary>
    public class Story51SceneFlowTests
    {
        private const string PrologueScene = "Prologue_Ruins";
        private const string SewersScene = "Act1_Sewers";

        private static IEnumerator LoadScene(string sceneName)
        {
            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
            yield return WaitForScene(sceneName);
        }

        private static IEnumerator WaitForScene(string sceneName, float timeout = 5f)
        {
            float t = 0f;
            while (SceneManager.GetActiveScene().name != sceneName && t < timeout)
            {
                t += Time.unscaledDeltaTime;
                yield return null;
            }
            Assert.AreEqual(sceneName, SceneManager.GetActiveScene().name, $"Scene {sceneName} did not load.");
            // Let Awake/Start (pending entry, initial room) run.
            yield return null;
            yield return null;
        }

        private static GameObject Player => GameObject.FindGameObjectWithTag("Player");
        private static RoomTransitionController Transitions => Object.FindFirstObjectByType<RoomTransitionController>();

        private static void ActivateRoomAt(Vector3 point)
        {
            var room = Transitions.FindRoomContaining(point);
            Assert.IsNotNull(room, $"No room contains {point}.");
            Transitions.SetActiveRoomImmediate(room);
        }

        [UnityTest]
        public IEnumerator Prologue_LoadsInMetroPlatform()
        {
            yield return LoadScene(PrologueScene);

            Assert.IsNotNull(Player, "Dante not found.");
            Assert.IsNotNull(Transitions, "RoomTransitionController not found.");
            Assert.AreEqual("prologue_metro_platform", Transitions.CurrentRoom.RoomId);
        }

        [UnityTest]
        public IEnumerator Prologue_DeathInLab_RespawnsInActivePlatformRoom()
        {
            yield return LoadScene(PrologueScene);
            var transitions = Transitions;
            var player = Player;

            RoomBounds lab = transitions.FindRoomContaining(new Vector3(232f, 1f, 0f));
            Assert.IsNotNull(lab, "Lab room not found.");
            player.transform.position = new Vector3(232f, 1f, 0f);
            transitions.SetActiveRoomImmediate(lab);
            yield return null;

            var health = player.GetComponent<HealthComponent>();
            health.TryTakeDamage(new DamageContext { amount = 999 });

            yield return new WaitForSeconds(1.6f);

            Assert.AreEqual("prologue_metro_platform", transitions.CurrentRoom.RoomId, "Respawn did not activate the platform room.");
            Assert.IsTrue(transitions.CurrentRoom.gameObject.activeInHierarchy);
            Assert.IsFalse(lab.gameObject.activeSelf, "Lab room should be inactive after respawning in the platform.");
            Assert.Less(player.transform.position.x, 0f, "Dante should respawn at the platform start.");
            Assert.IsFalse(health.IsDead, "Health should be restored on respawn.");
        }

        [UnityTest]
        public IEnumerator CrossScene_EntersCorrectRoomAndSpawn_BothWays()
        {
            yield return LoadScene(PrologueScene);

            Transitions.TransitionToScene(SewersScene, "sewers_initial_shelter", "from_prologue");
            yield return WaitForScene(SewersScene);
            yield return new WaitForSeconds(0.1f);

            Assert.AreEqual("sewers_initial_shelter", Transitions.CurrentRoom.RoomId);
            Assert.AreEqual(-12.6f, Player.transform.position.x, 0.5f, "Dante should arrive at the shelter's west side.");

            Transitions.TransitionToScene(PrologueScene, "prologue_descent", "from_sewers");
            yield return WaitForScene(PrologueScene);
            yield return new WaitForSeconds(0.1f);

            Assert.AreEqual("prologue_descent", Transitions.CurrentRoom.RoomId);
            Assert.AreEqual(293.4f, Player.transform.position.x, 0.5f, "Dante should arrive at the bottom of the descent shaft (P6).");
        }

        [UnityTest]
        public IEnumerator KittyDoor_StaysOpenAfterSceneReload()
        {
            yield return LoadScene(PrologueScene);
            ActivateRoomAt(new Vector3(174f, 1f, 0f));
            yield return null;

            var point = GameObject.Find("MaintenanceVent").GetComponent<CatExplorationPoint>();
            Assert.IsNotNull(point, "Maintenance vent not found in P4.");
            point.Complete();

            yield return LoadScene(PrologueScene);
            ActivateRoomAt(new Vector3(174f, 1f, 0f));
            yield return null;

            var reloaded = GameObject.Find("MaintenanceVent").GetComponent<CatExplorationPoint>();
            Assert.IsNotNull(reloaded);
            Assert.IsTrue(reloaded.IsCompleted, "The door Kitty opened should stay open in the same session.");
        }
    }
}
