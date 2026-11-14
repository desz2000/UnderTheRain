using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnderTheRain.Enemies;
using UnderTheRain.Player;

namespace UnderTheRain.Tests.PlayMode
{
    /// <summary>
    /// Story 5.9 — robots that see Dante go after him (sight, chase, ledges, memory), flying drones with a
    /// telegraphed swoop, and Dante smaller than the ground robots.
    /// Behaviour tests run in an empty scene built by the test, so no zone layout can interfere.
    /// </summary>
    public class Story59EnemyAiTests
    {
        private const string PrologueScene = "Prologue_Ruins";
        private static int s_sceneCounter;

        private readonly List<GameObject> _spawned = new List<GameObject>();
        private Sprite _sprite;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            var scene = SceneManager.CreateScene($"Story59_AiTest_{s_sceneCounter++}");
            SceneManager.SetActiveScene(scene);
            for (int i = SceneManager.sceneCount - 1; i >= 0; i--)
            {
                var other = SceneManager.GetSceneAt(i);
                if (other != scene && other.isLoaded) yield return SceneManager.UnloadSceneAsync(other);
            }

            _sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0f, 0f, 4f, 4f), new Vector2(0.5f, 0.5f), 4f);
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var go in _spawned)
            {
                if (go != null) Object.Destroy(go);
            }
            _spawned.Clear();
        }

        private GameObject Solid(float x1, float y1, float x2, float y2)
        {
            var go = new GameObject("Solid") { layer = LayerMask.NameToLayer("Ground") };
            go.transform.position = new Vector3((x1 + x2) * 0.5f, (y1 + y2) * 0.5f, 0f);
            go.AddComponent<BoxCollider2D>().size = new Vector2(x2 - x1, y2 - y1);
            _spawned.Add(go);
            return go;
        }

        private GameObject Player(float x, float y)
        {
            var go = new GameObject("TestPlayer") { tag = "Player" };
            go.transform.position = new Vector3(x, y, 0f);
            _spawned.Add(go);
            return go;
        }

        /// <summary>Robot with the components' default tuning. Built inactive so every Awake sees all components.</summary>
        private GameObject Robot(float x, float y, bool flying = false)
        {
            var go = new GameObject(flying ? "TestDrone" : "TestRobot");
            go.SetActive(false);
            go.transform.position = new Vector3(x, y, 0f);
            go.AddComponent<SpriteRenderer>().sprite = _sprite;
            go.AddComponent<EnemyAwareness>();
            go.AddComponent<PatrolInterferenceTarget>();
            if (flying) go.AddComponent<FlyingDroneMotor>();
            go.SetActive(true);
            _spawned.Add(go);
            return go;
        }

        private static IEnumerator WaitUntil(System.Func<bool> condition, float timeout)
        {
            float elapsed = 0f;
            while (!condition() && elapsed < timeout)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        [UnityTest]
        public IEnumerator GroundRobot_ChasesDante_WhenItSeesHim()
        {
            Solid(-12f, -1f, 12f, 0f);
            var robot = Robot(0f, 0.5f);
            Player(6f, 0.5f);
            var awareness = robot.GetComponent<EnemyAwareness>();

            yield return WaitUntil(() => awareness.State == EnemyAwarenessState.Alerted, 1f);
            Assert.AreEqual(EnemyAwarenessState.Alerted, awareness.State, "Seeing Dante starts with a readable alert.");

            yield return WaitUntil(() => awareness.State == EnemyAwarenessState.Chasing, 1f);
            Assert.AreEqual(EnemyAwarenessState.Chasing, awareness.State);

            yield return new WaitForSeconds(1.2f);
            Assert.Greater(robot.transform.position.x, 4f, "The robot leaves its patrol (±2) and closes in on Dante.");
        }

        [UnityTest]
        public IEnumerator GroundRobot_DoesNotSeeThroughWalls()
        {
            Solid(-12f, -1f, 12f, 0f);
            Solid(3.5f, 0f, 4.5f, 5f);
            var robot = Robot(0f, 0.5f);
            Player(7f, 0.5f);
            var awareness = robot.GetComponent<EnemyAwareness>();

            yield return new WaitForSeconds(1.5f);
            Assert.AreEqual(EnemyAwarenessState.Idle, awareness.State, "A solid wall blocks the line of sight.");
            Assert.LessOrEqual(robot.transform.position.x, 2.05f, "It keeps patrolling its post.");
        }

        [UnityTest]
        public IEnumerator GroundRobot_StopsAtLedges_InsteadOfWalkingOff()
        {
            Solid(-12f, -1f, 3f, 0f);
            var robot = Robot(0f, 0.5f);
            Player(8f, 0.5f);
            var awareness = robot.GetComponent<EnemyAwareness>();

            yield return new WaitForSeconds(2.5f);
            Assert.AreEqual(EnemyAwarenessState.Chasing, awareness.State, "It still sees Dante across the gap.");
            Assert.Greater(robot.transform.position.x, 2.05f, "It left its patrol to chase.");
            Assert.Less(robot.transform.position.x, 3f, "It waits at the edge of the floor.");
            Assert.AreEqual(0.5f, robot.transform.position.y, 0.001f);
        }

        [UnityTest]
        public IEnumerator GroundRobot_SearchesThenReturnsToItsPost_AfterLosingDante()
        {
            Solid(-30f, -1f, 30f, 0f);
            var robot = Robot(0f, 0.5f);
            var player = Player(6f, 0.5f);
            var awareness = robot.GetComponent<EnemyAwareness>();

            yield return WaitUntil(() => awareness.State == EnemyAwarenessState.Chasing, 2f);
            Assert.AreEqual(EnemyAwarenessState.Chasing, awareness.State);

            player.transform.position = new Vector3(200f, 0.5f, 0f);
            yield return null;
            yield return null;
            Assert.AreEqual(EnemyAwarenessState.Searching, awareness.State, "Out of sight: it looks where Dante was.");

            yield return WaitUntil(() => awareness.State == EnemyAwarenessState.Idle, 9f);
            Assert.AreEqual(EnemyAwarenessState.Idle, awareness.State, "Memory runs out and it walks back.");
            Assert.LessOrEqual(Mathf.Abs(robot.transform.position.x), 2.3f, "Back around its post.");
        }

        [UnityTest]
        public IEnumerator KittyInterference_FreezesAChasingRobot()
        {
            Solid(-30f, -1f, 30f, 0f);
            var robot = Robot(0f, 0.5f);
            Player(8f, 0.5f);
            var awareness = robot.GetComponent<EnemyAwareness>();
            var core = robot.GetComponent<PatrolInterferenceTarget>();

            yield return WaitUntil(() => awareness.State == EnemyAwarenessState.Chasing, 2f);
            Assert.IsTrue(core.TryApplyCatInterference(1f));
            float frozenX = robot.transform.position.x;

            yield return new WaitForSeconds(0.5f);
            Assert.AreEqual(frozenX, robot.transform.position.x, 0.001f, "A hesitating robot does not move.");

            yield return new WaitForSeconds(1.2f);
            Assert.Greater(robot.transform.position.x, frozenX + 0.5f, "It resumes the chase after recovering.");
        }

        [UnityTest]
        public IEnumerator Damage_AlertsARobot_EvenWithoutLineOfSight()
        {
            Solid(-12f, -1f, 12f, 0f);
            Solid(3.5f, 0f, 4.5f, 5f);
            var robot = Robot(0f, 0.5f);
            Player(7f, 0.5f);
            var awareness = robot.GetComponent<EnemyAwareness>();
            yield return new WaitForSeconds(0.3f);
            Assert.AreEqual(EnemyAwarenessState.Idle, awareness.State);

            awareness.AlertToTarget();
            Assert.AreEqual(EnemyAwarenessState.Alerted, awareness.State);
            Assert.AreEqual(7f, awareness.LastKnownPosition.x, 0.001f);
        }

        [UnityTest]
        public IEnumerator HiveCall_AlertsNearbyRobots_ButNotDistantOnes()
        {
            Solid(-40f, -1f, 40f, 0f);
            // Walls hide Dante from the second and third robots: only the hive call can alert them.
            Solid(-4f, 0f, -3.5f, 5f);
            var spotter = Robot(0f, 0.5f);
            var neighbour = Robot(-6f, 0.5f);
            var distant = Robot(-25f, 0.5f);
            Player(5f, 0.5f);

            var spotterAwareness = spotter.GetComponent<EnemyAwareness>();
            yield return WaitUntil(() => spotterAwareness.IsEngaged, 2f);
            Assert.IsTrue(spotterAwareness.IsEngaged);
            yield return null;

            Assert.IsTrue(neighbour.GetComponent<EnemyAwareness>().IsEngaged, "Shared mind: the robot 6 away is alerted.");
            Assert.IsFalse(distant.GetComponent<EnemyAwareness>().IsEngaged, "The robot 25 away is out of the hive call.");
        }

        [UnityTest]
        public IEnumerator Drone_TelegraphsThenSwoopsAtDante()
        {
            Solid(-15f, -1f, 15f, 0f);
            var drone = Robot(0f, 4f, flying: true);
            Player(3f, 0.65f);
            var core = drone.GetComponent<PatrolInterferenceTarget>();
            var motor = drone.GetComponent<FlyingDroneMotor>();

            yield return WaitUntil(() => core.IsTelegraphing, 5f);
            Assert.IsTrue(core.IsTelegraphing, "The drone shows a wind-up before attacking.");
            Assert.IsFalse(motor.IsSwooping, "No dive during the wind-up.");
            float windUpY = drone.transform.position.y;

            yield return WaitUntil(() => motor.IsSwooping, 1f);
            Assert.IsTrue(motor.IsSwooping);

            float lowestY = windUpY;
            float elapsed = 0f;
            while (elapsed < 1f)
            {
                lowestY = Mathf.Min(lowestY, drone.transform.position.y);
                elapsed += Time.deltaTime;
                yield return null;
            }
            Assert.Less(lowestY, windUpY - 1.5f, "The swoop dives at Dante.");
            Assert.Greater(lowestY, 0.25f, "It never flies into the ground.");
        }

        [UnityTest]
        public IEnumerator KittyInterference_CancelsTheDroneWindUp()
        {
            Solid(-15f, -1f, 15f, 0f);
            var drone = Robot(0f, 4f, flying: true);
            Player(3f, 0.65f);
            var core = drone.GetComponent<PatrolInterferenceTarget>();
            var motor = drone.GetComponent<FlyingDroneMotor>();

            yield return WaitUntil(() => core.IsTelegraphing, 5f);
            Assert.IsTrue(core.TryApplyCatInterference(0.8f));
            Assert.IsFalse(core.IsTelegraphing, "Kitty interrupts the attack.");

            Vector3 frozen = drone.transform.position;
            yield return new WaitForSeconds(0.5f);
            Assert.IsFalse(motor.IsSwooping);
            Assert.AreEqual(frozen.x, drone.transform.position.x, 0.001f);
            Assert.AreEqual(frozen.y, drone.transform.position.y, 0.001f);
        }

        [UnityTest]
        public IEnumerator Prologue_EveryRobotHasSight_HasDrones_AndDanteIsSmallerThanGroundRobots()
        {
            yield return Story54PrologueTests.LoadScene(PrologueScene);

            var player = Object.FindFirstObjectByType<PlayerController>();
            Assert.IsNotNull(player);
            float danteHeight = player.transform.localScale.y;

            int drones = 0;
            int robots = 0;
            foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                foreach (var core in root.GetComponentsInChildren<PatrolInterferenceTarget>(true))
                {
                    robots++;
                    Assert.IsNotNull(core.GetComponent<EnemyAwareness>(), $"{core.name} has no sight.");
                    if (core.GetComponent<FlyingDroneMotor>() != null)
                    {
                        drones++;
                        continue;
                    }
                    Assert.Greater(core.transform.localScale.y, danteHeight, $"{core.name} should be taller than Dante.");
                    Assert.Greater(core.transform.localScale.x, player.transform.localScale.x, $"{core.name} should be wider than Dante.");
                }
            }

            Assert.Greater(robots, 0);
            Assert.GreaterOrEqual(drones, 2, "The prologue has flying drones.");

            // P2 stays a pure movement room (no damage): no robot may live there.
            var gap = Story54PrologueTests.RoomAt(56f);
            Assert.AreEqual(0, gap.GetComponentsInChildren<PatrolInterferenceTarget>(true).Length, "P2 must stay free of enemies.");
        }
    }
}
