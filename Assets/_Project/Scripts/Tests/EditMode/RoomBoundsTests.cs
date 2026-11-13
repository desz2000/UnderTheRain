using NUnit.Framework;
using UnityEngine;
using UnderTheRain.World;

namespace UnderTheRain.Tests.EditMode
{
    public class RoomBoundsTests
    {
        private GameObject _roomGo;

        [TearDown]
        public void TearDown()
        {
            if (_roomGo != null) Object.DestroyImmediate(_roomGo);
        }

        private RoomBounds CreateRoom(Vector2 center, Vector2 size)
        {
            _roomGo = new GameObject("TestRoom");
            _roomGo.transform.position = center;
            var col = _roomGo.AddComponent<BoxCollider2D>();
            col.size = size;
            return _roomGo.AddComponent<RoomBounds>();
        }

        [Test]
        public void ContainsPoint_InsideAndOutside()
        {
            var room = CreateRoom(new Vector2(38f, 0f), new Vector2(32f, 12f));

            Assert.IsTrue(room.ContainsPoint(new Vector2(51f, 1f)));
            Assert.IsFalse(room.ContainsPoint(new Vector2(-10f, 1f)));
            Assert.IsFalse(room.ContainsPoint(new Vector2(38f, 7f)));
        }

        [Test]
        public void ContainsPoint_WorksWhileRoomIsInactive()
        {
            // Respawn must find the shelter room even though inactive rooms report empty Collider2D.bounds.
            var room = CreateRoom(Vector2.zero, new Vector2(32f, 12f));
            _roomGo.SetActive(false);

            Assert.IsTrue(room.ContainsPoint(new Vector2(-9f, 0.9f)));
            Assert.IsFalse(room.ContainsPoint(new Vector2(44f, 1f)));
        }
    }
}
