using NUnit.Framework;
using UnityEngine;
using UnderTheRain.World;

namespace UnderTheRain.Tests.EditMode
{
    /// <summary>Background layers slide by a fraction of the camera offset from their anchor.</summary>
    public class ParallaxLayerTests
    {
        private GameObject _go;

        [TearDown]
        public void TearDown()
        {
            if (_go != null) Object.DestroyImmediate(_go);
        }

        private ParallaxLayer NewLayer(Vector2 factor, Vector2 anchor, Vector3 basePosition)
        {
            _go = new GameObject("Layer");
            var layer = _go.AddComponent<ParallaxLayer>();
            layer.Configure(factor, anchor, basePosition);
            return layer;
        }

        [Test]
        public void CameraAtTheAnchor_KeepsTheAuthoredPosition()
        {
            var layer = NewLayer(new Vector2(0.3f, 0.15f), new Vector2(10f, 2f), new Vector3(10f, 2f, 0f));
            layer.ApplyFor(new Vector3(10f, 2f, -10f));
            Assert.AreEqual(new Vector3(10f, 2f, 0f), layer.transform.position);
        }

        [Test]
        public void FarLayer_FollowsTheCameraByItsFactor()
        {
            var layer = NewLayer(new Vector2(0.3f, 0.15f), new Vector2(10f, 2f), new Vector3(10f, 2f, 0f));
            layer.ApplyFor(new Vector3(20f, 6f, -10f));
            Assert.AreEqual(13f, layer.transform.position.x, 0.0001f);
            Assert.AreEqual(2.6f, layer.transform.position.y, 0.0001f);
            Assert.AreEqual(0f, layer.transform.position.z, 0.0001f);
        }

        [Test]
        public void ForegroundLayer_SlidesAgainstTheCamera()
        {
            var layer = NewLayer(new Vector2(-0.2f, -0.1f), Vector2.zero, Vector3.zero);
            layer.ApplyFor(new Vector3(10f, 0f, -10f));
            Assert.AreEqual(-2f, layer.transform.position.x, 0.0001f);
        }
    }
}
