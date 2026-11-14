using UnityEditor;
using UnityEngine;
using UnderTheRain.Combat;
using UnderTheRain.Enemies;
using UnderTheRain.World;

namespace UnderTheRain.Editor
{
    /// <summary>
    /// Shared scaffolding for combat-ready enemies used by the zone setup menus.
    /// </summary>
    internal static class EnemySetupUtility
    {
        private const string PartsPickupPrefabPath = "Assets/_Project/Prefabs/Environment/PartsPickup.prefab";

        /// <summary>
        /// Ensures combat layers exist and only PlayerHitbox↔EnemyHurtbox / EnemyHitbox↔PlayerHurtbox collide.
        /// </summary>
        public static void EnsureCombatLayers()
        {
            CombatTestRoomSetup.EnsureLayer("PlayerHitbox");
            CombatTestRoomSetup.EnsureLayer("PlayerHurtbox");
            CombatTestRoomSetup.EnsureLayer("EnemyHitbox");
            CombatTestRoomSetup.EnsureLayer("EnemyHurtbox");
            CombatTestRoomSetup.ConfigureCombatCollisionMatrix();
        }

        /// <summary>Tunable spec for a hive robot built by <see cref="CreateRobot"/>.</summary>
        public struct RobotSpec
        {
            public int health;
            public float patrolSpeed;
            public float patrolHalfWidth;
            public Color color;
            public Vector2 scale;
            public int partsDrop;
            public bool startDormant;
            public float wakeRadius;
            public float chaseSpeed;
            public float viewRange;
            /// <summary>Flying drone: hovers and swoops instead of walking (uses FlyingDroneMotor).</summary>
            public bool flying;

            /// <summary>Same robot, but powered down until Dante comes within <paramref name="radius"/> (0 = only damage/arena wakes it).</summary>
            public RobotSpec AsDormant(float radius)
            {
                var copy = this;
                copy.startDormant = true;
                copy.wakeRadius = radius;
                return copy;
            }
        }

        /// <summary>Agile scout (Act I): 4 HP, fast patrol.</summary>
        public static readonly RobotSpec Scout = new RobotSpec
        {
            health = 4, patrolSpeed = 2.2f, patrolHalfWidth = 2.0f,
            color = new Color(0.45f, 0.75f, 0.95f, 1f), scale = new Vector2(1.0f, 1.7f), partsDrop = 10,
            chaseSpeed = 4.4f, viewRange = 9f
        };

        /// <summary>Worker: slow and predictable; survives until the full baton combo (2+2+3) lands.</summary>
        public static readonly RobotSpec Worker = new RobotSpec
        {
            health = 6, patrolSpeed = 1.2f, patrolHalfWidth = 2.5f,
            color = new Color(0.82f, 0.56f, 0.30f, 1f), scale = new Vector2(1.4f, 1.9f), partsDrop = 15,
            chaseSpeed = 2.6f, viewRange = 7.5f
        };

        /// <summary>Powered-down scout on a high ledge: target that teaches the battery shot (2 shots).</summary>
        public static readonly RobotSpec DormantScout = new RobotSpec
        {
            health = 2, patrolSpeed = 0.5f, patrolHalfWidth = 0.2f,
            color = new Color(0.33f, 0.45f, 0.55f, 1f), scale = new Vector2(1.0f, 1.5f), partsDrop = 10,
            startDormant = true, wakeRadius = 0f, chaseSpeed = 4.4f, viewRange = 9f
        };

        /// <summary>Flying drone: fragile (one baton hit or two shots), hovers and attacks with a telegraphed swoop.</summary>
        public static readonly RobotSpec Drone = new RobotSpec
        {
            health = 2, patrolSpeed = 1.8f, patrolHalfWidth = 3.0f,
            color = new Color(0.72f, 0.52f, 0.92f, 1f), scale = new Vector2(1.0f, 0.7f), partsDrop = 5,
            chaseSpeed = 4.5f, viewRange = 10f, flying = true
        };

        public static GameObject CreateScoutRobot(string name, Transform parent, Sprite sprite, Vector2 feetPos)
            => CreateRobot(name, parent, sprite, feetPos, Scout);

        public static GameObject CreateWorkerRobot(string name, Transform parent, Sprite sprite, Vector2 feetPos)
            => CreateRobot(name, parent, sprite, feetPos, Worker);

        public static GameObject CreateDormantScout(string name, Transform parent, Sprite sprite, Vector2 feetPos)
            => CreateRobot(name, parent, sprite, feetPos, DormantScout);

        /// <summary>Flying drone. <paramref name="hoverPos"/> is the center of its hover post (not the floor).</summary>
        public static GameObject CreateDrone(string name, Transform parent, Sprite sprite, Vector2 hoverPos)
            => CreateRobot(name, parent, sprite, hoverPos, Drone);

        /// <summary>
        /// Hive robot: patrol + Kitty interference, contact damage, health and parts drop.
        /// The body is not solid (Dante passes through and takes contact damage, Hollow Knight style).
        /// Ground robots take the position of their FEET (the floor they stand on), so resizing a spec never
        /// sinks them into the ground; flying drones take the center of their hover post.
        /// Every robot gets sight (<see cref="EnemyAwareness"/>): it chases Dante while it sees him.
        /// </summary>
        public static GameObject CreateRobot(string name, Transform parent, Sprite sprite, Vector2 position, RobotSpec spec)
        {
            Vector2 worldPos = spec.flying ? position : position + new Vector2(0f, spec.scale.y * 0.5f);
            int groundLayer = LayerMask.NameToLayer("Ground");
            int oneWayLayer = LayerMask.NameToLayer("OneWayPlatform");
            int solidMask = groundLayer >= 0 ? 1 << groundLayer : 0;
            int floorMask = solidMask | (oneWayLayer >= 0 ? 1 << oneWayLayer : 0);

            int eHit = LayerMask.NameToLayer("EnemyHitbox");
            int eHurt = LayerMask.NameToLayer("EnemyHurtbox");

            var robotGo = new GameObject(name);
            robotGo.transform.SetParent(parent, false);
            robotGo.transform.position = worldPos;
            robotGo.transform.localScale = new Vector3(spec.scale.x, spec.scale.y, 1f);

            var sr = robotGo.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = spec.color;

            // Kinematic body so trigger contacts register reliably while the patrol moves the transform.
            var rb = robotGo.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;

            var health = robotGo.AddComponent<HealthComponent>();
            var healthSo = new SerializedObject(health);
            healthSo.FindProperty("maxHealth").intValue = spec.health;
            healthSo.FindProperty("invulnerabilityDuration").floatValue = 0.1f;
            healthSo.ApplyModifiedPropertiesWithoutUndo();

            // Alert glyph: readable "it saw me" beat above the head before the chase starts.
            var glyphGo = new GameObject("AlertGlyph");
            glyphGo.transform.SetParent(robotGo.transform, false);
            glyphGo.transform.localPosition = new Vector3(0f, 0.5f + 0.35f / spec.scale.y, 0f);
            glyphGo.transform.localScale = new Vector3(0.16f / spec.scale.x, 0.42f / spec.scale.y, 1f);
            var glyphSr = glyphGo.AddComponent<SpriteRenderer>();
            glyphSr.sprite = sprite;
            glyphSr.color = new Color(1f, 0.32f, 0.22f, 1f);
            glyphSr.sortingOrder = 5;
            glyphSr.enabled = false;

            // Added before the core and the motor so both find it in Awake.
            var awareness = robotGo.AddComponent<EnemyAwareness>();
            var awarenessSo = new SerializedObject(awareness);
            awarenessSo.FindProperty("viewRange").floatValue = spec.viewRange > 0f ? spec.viewRange : 9f;
            awarenessSo.FindProperty("sightBlockers").intValue = solidMask;
            awarenessSo.FindProperty("alertGlyph").objectReferenceValue = glyphSr;
            awarenessSo.ApplyModifiedPropertiesWithoutUndo();

            var patrol = robotGo.AddComponent<PatrolInterferenceTarget>();
            var patrolSo = new SerializedObject(patrol);
            patrolSo.FindProperty("chaseSpeed").floatValue = spec.chaseSpeed > 0f ? spec.chaseSpeed : 4f;
            patrolSo.FindProperty("obstacleMask").intValue = solidMask;
            patrolSo.FindProperty("floorMask").intValue = floorMask;
            patrolSo.FindProperty("faction").enumValueIndex = (int)EnemyFaction.CollectiveMachine;
            patrolSo.FindProperty("bodyRenderer").objectReferenceValue = sr;
            patrolSo.FindProperty("patrolSpeed").floatValue = spec.patrolSpeed;
            patrolSo.FindProperty("patrolHalfWidth").floatValue = spec.patrolHalfWidth;
            patrolSo.FindProperty("startDormant").boolValue = spec.startDormant;
            patrolSo.FindProperty("wakeRadius").floatValue = spec.wakeRadius;
            patrolSo.ApplyModifiedPropertiesWithoutUndo();

            if (spec.flying)
            {
                var motor = robotGo.AddComponent<FlyingDroneMotor>();
                var motorSo = new SerializedObject(motor);
                motorSo.FindProperty("hoverSpeed").floatValue = spec.patrolSpeed;
                motorSo.FindProperty("hoverHalfWidth").floatValue = spec.patrolHalfWidth;
                motorSo.FindProperty("chaseSpeed").floatValue = spec.chaseSpeed > 0f ? spec.chaseSpeed : 4.5f;
                motorSo.FindProperty("obstacleMask").intValue = solidMask;
                motorSo.ApplyModifiedPropertiesWithoutUndo();
            }

            // Hurtbox (receives melee and battery shots)
            var hurtGo = new GameObject("EnemyHurtbox") { layer = eHurt };
            hurtGo.transform.SetParent(robotGo.transform, false);
            var hurtCol = hurtGo.AddComponent<BoxCollider2D>();
            hurtCol.isTrigger = true;
            hurtCol.size = Vector2.one;
            var hurtbox = hurtGo.AddComponent<Hurtbox>();
            var hurtSo = new SerializedObject(hurtbox);
            hurtSo.FindProperty("damageableTarget").objectReferenceValue = health;
            hurtSo.ApplyModifiedPropertiesWithoutUndo();

            // Contact hitbox (hurts Dante on touch)
            var hitGo = new GameObject("ContactHitbox") { layer = eHit };
            hitGo.transform.SetParent(robotGo.transform, false);
            var hitCol = hitGo.AddComponent<BoxCollider2D>();
            hitCol.isTrigger = true;
            hitCol.size = new Vector2(0.85f, 0.9f);
            var hitbox = hitGo.AddComponent<Hitbox>();
            var hitSo = new SerializedObject(hitbox);
            hitSo.FindProperty("alwaysActive").boolValue = true;
            hitSo.ApplyModifiedPropertiesWithoutUndo();

            var enemy = robotGo.AddComponent<ScoutRobotEnemy>();
            var enemySo = new SerializedObject(enemy);
            enemySo.FindProperty("bodyRenderer").objectReferenceValue = sr;
            enemySo.FindProperty("contactHitbox").objectReferenceValue = hitGo;
            enemySo.FindProperty("partsDropAmount").intValue = spec.partsDrop;
            enemySo.FindProperty("partsPickupPrefab").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<PartsPickup>(PartsPickupPrefabPath);
            enemySo.ApplyModifiedPropertiesWithoutUndo();

            return robotGo;
        }
    }
}
