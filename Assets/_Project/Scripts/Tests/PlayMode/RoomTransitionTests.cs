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
    }
}
