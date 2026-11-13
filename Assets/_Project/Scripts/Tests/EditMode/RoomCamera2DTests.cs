using NUnit.Framework;
using UnityEngine;
using UnderTheRain.World;

namespace UnderTheRain.Tests.EditMode
{
    public class RoomCamera2DTests
    {
        [Test]
        public void ClampCameraPosition_ClampsInsideLargerRoom()
        {
            var bounds = new Bounds(Vector3.zero, new Vector3(40f, 20f, 0f));
            Vector2 clamped = RoomCamera2D.ClampCameraPosition(new Vector2(30f, -15f), bounds, 9.6f, 5.4f);

            Assert.AreEqual(10.4f, clamped.x, 0.001f);
            Assert.AreEqual(-4.6f, clamped.y, 0.001f);
        }

        [Test]
        public void ClampCameraPosition_CentersAxis_WhenRoomSmallerThanViewport()
        {
            var bounds = new Bounds(new Vector3(5f, 2f, 0f), new Vector3(10f, 6f, 0f));
            Vector2 clamped = RoomCamera2D.ClampCameraPosition(new Vector2(100f, 100f), bounds, 9.6f, 5.4f);

            Assert.AreEqual(5f, clamped.x, 0.001f);
            Assert.AreEqual(2f, clamped.y, 0.001f);
        }
    }
}
