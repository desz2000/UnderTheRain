using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnderTheRain.World;

namespace UnderTheRain.Tests.PlayMode
{
    public class RoomTransitionTests
    {
        [UnityTest]
        public IEnumerator TransitionToRoom_MovesTravelerAndSwitchesActiveRoom()
        {
            var camGo = new GameObject("TestCamera");
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5.4f;
            var roomCam = camGo.AddComponent<RoomCamera2D>();

            var faderGo = new GameObject("Fader");
            var cg = faderGo.AddComponent<CanvasGroup>();
            cg.alpha = 0f;
            var fader = faderGo.AddComponent<ScreenFader>();

            var roomAGo = new GameObject("RoomA");
            var boxA = roomAGo.AddComponent<BoxCollider2D>();
            boxA.size = new Vector2(30f, 16f);
            var roomA = roomAGo.AddComponent<RoomBounds>();
            roomA.SetRoomId("room_a");

            var roomBGo = new GameObject("RoomB");
            roomBGo.transform.position = new Vector3(100f, 0f, 0f);
            var boxB = roomBGo.AddComponent<BoxCollider2D>();
            boxB.size = new Vector2(30f, 16f);
            var roomB = roomBGo.AddComponent<RoomBounds>();
            roomB.SetRoomId("room_b");

            var spawnGo = new GameObject("SpawnFromA");
            spawnGo.transform.SetParent(roomBGo.transform);
            spawnGo.transform.position = new Vector3(95f, 2f, 0f);
            var spawn = spawnGo.AddComponent<RoomSpawnPoint>();
            spawn.SetSpawnId("from_a");
            roomB.CacheSpawnPoints();
            roomBGo.SetActive(false);

            var travelerGo = new GameObject("Traveler");
            travelerGo.transform.position = Vector3.zero;
            roomCam.SetTarget(travelerGo.transform);

            var ctrlGo = new GameObject("TransitionCtrl");
            var ctrl = ctrlGo.AddComponent<RoomTransitionController>();
            ctrl.Initialize(fader, roomCam, roomA);

            ctrl.TransitionToRoom(travelerGo.transform, roomB, "from_a");

            yield return new WaitForSecondsRealtime(0.7f);

            Assert.IsFalse(roomAGo.activeSelf);
            Assert.IsTrue(roomBGo.activeSelf);
            Assert.AreEqual(new Vector3(95f, 2f, 0f), travelerGo.transform.position);

            Object.Destroy(camGo);
            Object.Destroy(faderGo);
            Object.Destroy(roomAGo);
            Object.Destroy(roomBGo);
            Object.Destroy(travelerGo);
            Object.Destroy(ctrlGo);
        }

        /// <summary>Simulates PlayerController's corner correction: writes body.position every physics step.</summary>
        private sealed class BodyNudger : MonoBehaviour
        {
            private Rigidbody2D _body;
            private void Awake() => _body = GetComponent<Rigidbody2D>();
            private void FixedUpdate() => _body.position += Vector2.right * 0.001f;
        }

        [UnityTest]
        public IEnumerator TransitionToRoom_MovesInterpolatedRigidbodyMidJump()
        {
            // Regression: jumping into an exit (interpolated body + corner correction) left Dante in the old room.
            var camGo = new GameObject("TestCamera");
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            var roomCam = camGo.AddComponent<RoomCamera2D>();

            var faderGo = new GameObject("Fader");
            faderGo.AddComponent<CanvasGroup>().alpha = 0f;
            var fader = faderGo.AddComponent<ScreenFader>();

            var roomAGo = new GameObject("RoomA");
            roomAGo.AddComponent<BoxCollider2D>().size = new Vector2(30f, 16f);
            var roomA = roomAGo.AddComponent<RoomBounds>();

            var roomBGo = new GameObject("RoomB");
            roomBGo.transform.position = new Vector3(100f, 0f, 0f);
            roomBGo.AddComponent<BoxCollider2D>().size = new Vector2(30f, 16f);
            var roomB = roomBGo.AddComponent<RoomBounds>();
            var spawnGo = new GameObject("Spawn");
            spawnGo.transform.SetParent(roomBGo.transform);
            spawnGo.transform.position = new Vector3(95f, 2f, 0f);
            spawnGo.AddComponent<RoomSpawnPoint>().SetSpawnId("from_a");
            roomB.CacheSpawnPoints();
            roomBGo.SetActive(false);

            var travelerGo = new GameObject("Traveler");
            var body = travelerGo.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            body.linearVelocity = new Vector2(8f, 15f); // mid-jump toward the exit
            travelerGo.AddComponent<BoxCollider2D>();
            travelerGo.AddComponent<BodyNudger>();
            roomCam.SetTarget(travelerGo.transform);

            var ctrlGo = new GameObject("TransitionCtrl");
            var ctrl = ctrlGo.AddComponent<RoomTransitionController>();
            ctrl.Initialize(fader, roomCam, roomA);

            ctrl.TransitionToRoom(travelerGo.transform, roomB, "from_a");
            yield return new WaitForSecondsRealtime(0.8f);

            Assert.IsTrue(roomBGo.activeSelf);
            Assert.AreEqual(95f, body.position.x, 0.5f, "Rigidbody must end up at the destination spawn.");
            Assert.AreEqual(2f, body.position.y, 0.5f);
            Assert.AreEqual(95f, travelerGo.transform.position.x, 0.5f);

            Object.Destroy(camGo);
            Object.Destroy(faderGo);
            Object.Destroy(roomAGo);
            Object.Destroy(roomBGo);
            Object.Destroy(travelerGo);
            Object.Destroy(ctrlGo);
        }
    }
}
