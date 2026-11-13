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

        /// <summary>
        /// Hive scout robot: patrol + Kitty interference, contact damage, health and parts drop.
        /// The body is not solid (Dante passes through and takes contact damage, Hollow Knight style).
        /// </summary>
        public static GameObject CreateScoutRobot(string name, Transform parent, Sprite sprite, Vector2 worldPos)
        {
            int eHit = LayerMask.NameToLayer("EnemyHitbox");
            int eHurt = LayerMask.NameToLayer("EnemyHurtbox");

            var robotGo = new GameObject(name);
            robotGo.transform.SetParent(parent, false);
            robotGo.transform.position = worldPos;
            robotGo.transform.localScale = new Vector3(0.9f, 1.4f, 1f);

            var sr = robotGo.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = new Color(0.45f, 0.75f, 0.95f, 1f);

            // Kinematic body so trigger contacts register reliably while the patrol moves the transform.
            var rb = robotGo.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;

            var health = robotGo.AddComponent<HealthComponent>();
            var healthSo = new SerializedObject(health);
            healthSo.FindProperty("maxHealth").intValue = 4;
            healthSo.FindProperty("invulnerabilityDuration").floatValue = 0.1f;
            healthSo.ApplyModifiedPropertiesWithoutUndo();

            var patrol = robotGo.AddComponent<PatrolInterferenceTarget>();
            var patrolSo = new SerializedObject(patrol);
            patrolSo.FindProperty("faction").enumValueIndex = (int)EnemyFaction.CollectiveMachine;
            patrolSo.FindProperty("bodyRenderer").objectReferenceValue = sr;
            patrolSo.ApplyModifiedPropertiesWithoutUndo();

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
            enemySo.FindProperty("partsPickupPrefab").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<PartsPickup>(PartsPickupPrefabPath);
            enemySo.ApplyModifiedPropertiesWithoutUndo();

            return robotGo;
        }
    }
}
