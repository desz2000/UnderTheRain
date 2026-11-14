using UnityEngine;

namespace UnderTheRain.World
{
    /// <summary>
    /// Moves a background (or foreground) layer by a fraction of the camera's offset from the layer's anchor,
    /// so layers at different depths slide against each other as the camera pans.
    /// Factor 0 = fixed to the world; 1 = fixed to the camera; negative = closer than the playfield.
    /// </summary>
    [DefaultExecutionOrder(200)] // after the room camera (0) and the camera shake (100)
    public sealed class ParallaxLayer : MonoBehaviour
    {
        [SerializeField] private Vector2 factor = new Vector2(0.2f, 0.1f);
        [Tooltip("World point where the layer sits exactly at its authored position (the room centre).")]
        [SerializeField] private Vector2 anchor;
        [SerializeField] private Vector3 basePosition;

        private Transform _camera;

        public Vector2 Factor => factor;

        /// <summary>Editor/setup entry point: stores the authored position and the parallax strength.</summary>
        public void Configure(Vector2 parallaxFactor, Vector2 anchorPoint, Vector3 authoredPosition)
        {
            factor = parallaxFactor;
            anchor = anchorPoint;
            basePosition = authoredPosition;
        }

        /// <summary>Places the layer for a given camera position (also used by edit-time previews).</summary>
        public void ApplyFor(Vector3 cameraPosition)
        {
            Vector2 offset = Vector2.Scale((Vector2)cameraPosition - anchor, factor);
            transform.position = basePosition + new Vector3(offset.x, offset.y, 0f);
        }

        private void LateUpdate()
        {
            if (_camera == null)
            {
                var main = Camera.main;
                if (main == null) return;
                _camera = main.transform;
            }

            ApplyFor(_camera.position);
        }
    }
}
