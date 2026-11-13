using UnityEngine;

namespace UnderTheRain.World
{
    /// <summary>
    /// Orthographic 2D camera follower that confines the viewport within the active room's bounds.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public sealed class RoomCamera2D : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private RoomBounds initialRoom;
        [SerializeField] private float smoothTime = 0.08f;

        private Camera _camera;
        private RoomBounds _activeRoom;
        private Vector3 _velocity;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
            if (initialRoom != null)
            {
                SetRoom(initialRoom, snapImmediately: true);
            }
        }

        private void LateUpdate()
        {
            if (target == null) return;

            Vector3 desired = new Vector3(target.position.x, target.position.y, transform.position.z);

            if (_activeRoom != null && _camera != null)
            {
                float halfHeight = _camera.orthographicSize;
                float halfWidth = halfHeight * _camera.aspect;
                Vector2 clamped = ClampCameraPosition(desired, _activeRoom.WorldBounds, halfWidth, halfHeight);
                desired.x = clamped.x;
                desired.y = clamped.y;
            }

            transform.position = Vector3.SmoothDamp(transform.position, desired, ref _velocity, smoothTime);
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }

        public void SetRoom(RoomBounds room, bool snapImmediately = false)
        {
            _activeRoom = room;
            if (snapImmediately && target != null && _camera != null)
            {
                Vector3 pos = new Vector3(target.position.x, target.position.y, transform.position.z);
                if (_activeRoom != null)
                {
                    float halfHeight = _camera.orthographicSize;
                    float halfWidth = halfHeight * _camera.aspect;
                    Vector2 clamped = ClampCameraPosition(pos, _activeRoom.WorldBounds, halfWidth, halfHeight);
                    pos.x = clamped.x;
                    pos.y = clamped.y;
                }
                _velocity = Vector3.zero;
                transform.position = pos;
            }
        }

        /// <summary>
        /// Clamps the camera center inside roomBounds; centers on any axis where the room is narrower than the viewport.
        /// </summary>
        public static Vector2 ClampCameraPosition(Vector2 desiredPosition, Bounds roomBounds, float halfWidth, float halfHeight)
        {
            float minX = roomBounds.min.x + halfWidth;
            float maxX = roomBounds.max.x - halfWidth;
            float x = minX > maxX ? roomBounds.center.x : Mathf.Clamp(desiredPosition.x, minX, maxX);

            float minY = roomBounds.min.y + halfHeight;
            float maxY = roomBounds.max.y - halfHeight;
            float y = minY > maxY ? roomBounds.center.y : Mathf.Clamp(desiredPosition.y, minY, maxY);

            return new Vector2(x, y);
        }
    }
}
