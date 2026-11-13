using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnderTheRain.Combat;
using UnderTheRain.Core.Input;
using UnderTheRain.Player;
using UnderTheRain.World;

namespace UnderTheRain.Editor
{
    /// <summary>
    /// Scaffolds Story 3.1 combat test room: weapon asset, combat layers + collision matrix,
    /// Dante with melee combo, TrainingDummy and a contact Hazard.
    /// </summary>
    public static class CombatTestRoomSetup
    {
        private const string MovementConfigPath = "Assets/_Project/Data/Abilities/PlayerMovementConfig.asset";
        private const string WeaponDataPath = "Assets/_Project/Data/Weapons/SimpleBaton.asset";
        private const string ProjectilePrefabPath = "Assets/_Project/Prefabs/Player/BatteryProjectile.prefab";
        private const string DroppedPackPrefabPath = "Assets/_Project/Prefabs/Environment/DroppedPack.prefab";
        private const string PartsPickupPrefabPath = "Assets/_Project/Prefabs/Environment/PartsPickup.prefab";
        private const string ScenePath = "Assets/_Project/Scenes/Zones/CombatTestRoom.unity";
        private const string InputActionsPath = "Assets/InputSystem_Actions.inputactions";
        private const string SquareSpritePath = "Assets/_Project/Art/Sprites/WhiteSquare.png";

        private const string GroundLayer = "Ground";
        private const string OneWayLayer = "OneWayPlatform";
        private const string PlayerHitboxLayer = "PlayerHitbox";
        private const string PlayerHurtboxLayer = "PlayerHurtbox";
        private const string EnemyHitboxLayer = "EnemyHitbox";
        private const string EnemyHurtboxLayer = "EnemyHurtbox";

        [MenuItem("UnderTheRain/Setup/Create Combat Test Room")]
        public static void CreateCombatTestRoom()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogError("[UnderTheRain] Exit Play Mode before running Setup menu commands.");
                return;
            }

            EnsureLayer(GroundLayer);
            EnsureLayer(OneWayLayer);
            EnsureLayer(PlayerHitboxLayer);
            EnsureLayer(PlayerHurtboxLayer);
            EnsureLayer(EnemyHitboxLayer);
            EnsureLayer(EnemyHurtboxLayer);
            ConfigureCombatCollisionMatrix();

            int ground = LayerMask.NameToLayer(GroundLayer);
            int oneWay = LayerMask.NameToLayer(OneWayLayer);
            int pHit = LayerMask.NameToLayer(PlayerHitboxLayer);
            int pHurt = LayerMask.NameToLayer(PlayerHurtboxLayer);
            int eHit = LayerMask.NameToLayer(EnemyHitboxLayer);
            int eHurt = LayerMask.NameToLayer(EnemyHurtboxLayer);

            EnsureSimpleBatonAsset();
            EnsureBatteryProjectilePrefab(pHit);
            EnsureDroppedPackPrefab();
            EnsurePartsPickupPrefab();

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(SquareSpritePath);

            var camGo = new GameObject("Main Camera") { tag = "MainCamera" };
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5.4f;
            cam.backgroundColor = new Color(0.055f, 0.063f, 0.078f, 1f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            camGo.transform.position = new Vector3(0f, 1f, -10f);
            camGo.AddComponent<AudioListener>();
            var roomCam = camGo.AddComponent<RoomCamera2D>();
            var camShake = camGo.AddComponent<CameraShake2D>();

            var inputGo = new GameObject("GameInputReader");
            var reader = inputGo.AddComponent<GameInputReader>();
            var inputAsset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputActionsPath);
            var readerSo = new SerializedObject(reader);
            readerSo.FindProperty("inputActions").objectReferenceValue = inputAsset;
            readerSo.ApplyModifiedPropertiesWithoutUndo();

            // Floor & walls
            CreateBlock("Floor", sprite, ground, new Vector2(0f, -1f), new Vector2(28f, 2f), new Color(0.25f, 0.28f, 0.33f));
            CreateBlock("Wall Left", sprite, ground, new Vector2(-13.5f, 3f), new Vector2(1f, 8f), new Color(0.25f, 0.28f, 0.33f));
            CreateBlock("Wall Right", sprite, ground, new Vector2(13.5f, 3f), new Vector2(1f, 8f), new Color(0.25f, 0.28f, 0.33f));

            // Dante
            var player = new GameObject("Dante");
            player.transform.position = new Vector3(0f, 1f, 0f);
            player.transform.localScale = new Vector3(0.8f, 1.6f, 1f);
            var sr = player.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = new Color(0.31f, 0.8f, 0.77f);
            var rb = player.AddComponent<Rigidbody2D>();
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            player.AddComponent<BoxCollider2D>().size = Vector2.one;
            var playerHealth = player.AddComponent<HealthComponent>();

            var controller = player.AddComponent<PlayerController>();
            var ctrlSo = new SerializedObject(controller);
            ctrlSo.FindProperty("input").objectReferenceValue = reader;
            ctrlSo.FindProperty("config").objectReferenceValue = AssetDatabase.LoadAssetAtPath<PlayerMovementConfigSO>(MovementConfigPath);
            ctrlSo.FindProperty("groundMask").intValue = (1 << ground) | (1 << oneWay);
            ctrlSo.FindProperty("oneWayMask").intValue = 1 << oneWay;
            ctrlSo.ApplyModifiedPropertiesWithoutUndo();

            // Player Hurtbox child
            var pHurtGo = new GameObject("PlayerHurtbox") { layer = pHurt };
            pHurtGo.transform.SetParent(player.transform, false);
            var pHurtCol = pHurtGo.AddComponent<BoxCollider2D>();
            pHurtCol.isTrigger = true;
            pHurtCol.size = Vector2.one;
            var pHurtbox = pHurtGo.AddComponent<Hurtbox>();
            var pHurtSo = new SerializedObject(pHurtbox);
            pHurtSo.FindProperty("damageableTarget").objectReferenceValue = playerHealth;
            pHurtSo.ApplyModifiedPropertiesWithoutUndo();

            // Player Melee Hitbox + Swing visual
            var pHitGo = new GameObject("MeleeHitbox") { layer = pHit };
            pHitGo.transform.SetParent(player.transform, false);
            var pHitCol = pHitGo.AddComponent<BoxCollider2D>();
            pHitCol.isTrigger = true;
            var meleeHitbox = pHitGo.AddComponent<Hitbox>();

            var swingGo = new GameObject("SwingVisual");
            swingGo.transform.SetParent(player.transform, false);
            var swingSr = swingGo.AddComponent<SpriteRenderer>();
            swingSr.sprite = sprite;
            swingSr.sortingOrder = 5;

            var feedback = player.AddComponent<CombatFeedback>();
            var fbSo = new SerializedObject(feedback);
            fbSo.FindProperty("cameraShake").objectReferenceValue = camShake;
            fbSo.ApplyModifiedPropertiesWithoutUndo();

            var melee = player.AddComponent<PlayerMeleeCombat>();
            var meleeSo = new SerializedObject(melee);
            meleeSo.FindProperty("input").objectReferenceValue = reader;
            meleeSo.FindProperty("playerController").objectReferenceValue = controller;
            meleeSo.FindProperty("weaponData").objectReferenceValue = AssetDatabase.LoadAssetAtPath<WeaponDataSO>(WeaponDataPath);
            meleeSo.FindProperty("meleeHitbox").objectReferenceValue = meleeHitbox;
            meleeSo.FindProperty("swingVisual").objectReferenceValue = swingSr;
            meleeSo.FindProperty("combatFeedback").objectReferenceValue = feedback;
            meleeSo.ApplyModifiedPropertiesWithoutUndo();

            var battery = player.AddComponent<BatteryComponent>();
            var medkit = player.AddComponent<MedkitComponent>();
            var medkitSo = new SerializedObject(medkit);
            medkitSo.FindProperty("input").objectReferenceValue = reader;
            medkitSo.FindProperty("healthComponent").objectReferenceValue = playerHealth;
            medkitSo.ApplyModifiedPropertiesWithoutUndo();

            var ranged = player.AddComponent<PlayerRangedCombat>();
            var rangedSo = new SerializedObject(ranged);
            rangedSo.FindProperty("input").objectReferenceValue = reader;
            rangedSo.FindProperty("playerController").objectReferenceValue = controller;
            rangedSo.FindProperty("battery").objectReferenceValue = battery;
            var projGo = AssetDatabase.LoadAssetAtPath<GameObject>(ProjectilePrefabPath);
            rangedSo.FindProperty("projectilePrefab").objectReferenceValue = projGo != null ? projGo.GetComponent<BatteryProjectile>() : null;
            rangedSo.FindProperty("solidMask").intValue = 1 << ground;
            rangedSo.ApplyModifiedPropertiesWithoutUndo();

            var partsPack = player.AddComponent<PartsPackComponent>();
            var packSo = new SerializedObject(partsPack);
            packSo.FindProperty("input").objectReferenceValue = reader;
            packSo.FindProperty("medkitComponent").objectReferenceValue = medkit;
            packSo.ApplyModifiedPropertiesWithoutUndo();

            var deathHandler = player.AddComponent<PlayerDeathHandler>();
            var deathSo = new SerializedObject(deathHandler);
            deathSo.FindProperty("input").objectReferenceValue = reader;
            deathSo.FindProperty("health").objectReferenceValue = playerHealth;
            deathSo.FindProperty("battery").objectReferenceValue = battery;
            deathSo.FindProperty("medkit").objectReferenceValue = medkit;
            deathSo.FindProperty("partsPack").objectReferenceValue = partsPack;
            var droppedGo = AssetDatabase.LoadAssetAtPath<GameObject>(DroppedPackPrefabPath);
            deathSo.FindProperty("droppedPackPrefab").objectReferenceValue = droppedGo != null ? droppedGo.GetComponent<DroppedPack>() : null;
            deathSo.ApplyModifiedPropertiesWithoutUndo();

            player.AddComponent<DebugCombatHud>();

            var camSo = new SerializedObject(roomCam);
            camSo.FindProperty("target").objectReferenceValue = player.transform;
            camSo.ApplyModifiedPropertiesWithoutUndo();

            // Training Dummy (right)
            var dummy = new GameObject("TrainingDummy") { layer = ground };
            dummy.transform.position = new Vector3(4.5f, 1f, 0f);
            dummy.transform.localScale = new Vector3(0.9f, 1.8f, 1f);
            var dummySr = dummy.AddComponent<SpriteRenderer>();
            dummySr.sprite = sprite;
            dummySr.color = new Color(0.92f, 0.72f, 0.25f);
            var dummyRb = dummy.AddComponent<Rigidbody2D>();
            dummyRb.constraints = RigidbodyConstraints2D.FreezeRotation;
            dummy.AddComponent<BoxCollider2D>().size = Vector2.one;
            var dummyHealth = dummy.AddComponent<HealthComponent>();
            var dummyHealthSo = new SerializedObject(dummyHealth);
            dummyHealthSo.FindProperty("invulnerabilityDuration").floatValue = 0.05f;
            dummyHealthSo.ApplyModifiedPropertiesWithoutUndo();
            var dummyScript = dummy.AddComponent<TrainingDummy>();
            var dummySo = new SerializedObject(dummyScript);
            var pickupGo = AssetDatabase.LoadAssetAtPath<GameObject>(PartsPickupPrefabPath);
            dummySo.FindProperty("partsPickupPrefab").objectReferenceValue = pickupGo != null ? pickupGo.GetComponent<PartsPickup>() : null;
            dummySo.ApplyModifiedPropertiesWithoutUndo();

            var dHurtGo = new GameObject("EnemyHurtbox") { layer = eHurt };
            dHurtGo.transform.SetParent(dummy.transform, false);
            var dHurtCol = dHurtGo.AddComponent<BoxCollider2D>();
            dHurtCol.isTrigger = true;
            dHurtCol.size = Vector2.one;
            var dHurtbox = dHurtGo.AddComponent<Hurtbox>();
            var dHurtSo = new SerializedObject(dHurtbox);
            dHurtSo.FindProperty("damageableTarget").objectReferenceValue = dummyHealth;
            dHurtSo.ApplyModifiedPropertiesWithoutUndo();

            // Contact Hazard (left) for testing player invulnerability
            var hazard = new GameObject("TestHazard") { layer = eHit };
            hazard.transform.position = new Vector3(-5f, 0.5f, 0f);
            hazard.transform.localScale = new Vector3(1.2f, 1f, 1f);
            var hSr = hazard.AddComponent<SpriteRenderer>();
            hSr.sprite = sprite;
            hSr.color = new Color(0.9f, 0.25f, 0.3f);
            var hCol = hazard.AddComponent<BoxCollider2D>();
            hCol.isTrigger = true;
            var hHitbox = hazard.AddComponent<Hitbox>();
            var hSo = new SerializedObject(hHitbox);
            hSo.FindProperty("alwaysActive").boolValue = true;
            hSo.ApplyModifiedPropertiesWithoutUndo();

            // Battery Cell Pickups
            CreateBatteryCell("BatteryCell_1", sprite, new Vector2(2f, 0.5f));
            CreateBatteryCell("BatteryCell_2", sprite, new Vector2(-2.5f, 0.5f));

            // Shelter Rest Spot (center-left)
            var shelter = new GameObject("ShelterRestSpot");
            shelter.transform.position = new Vector3(-9f, 1f, 0f);
            shelter.transform.localScale = new Vector3(1.6f, 2.2f, 1f);
            var shSr = shelter.AddComponent<SpriteRenderer>();
            shSr.sprite = sprite;
            shSr.color = new Color(0.25f, 0.78f, 0.52f, 0.55f);
            shelter.AddComponent<BoxCollider2D>().isTrigger = true;
            var restSpot = shelter.AddComponent<ShelterRestSpot>();
            var restSo = new SerializedObject(restSpot);
            restSo.FindProperty("input").objectReferenceValue = reader;
            restSo.ApplyModifiedPropertiesWithoutUndo();

            // Battery Charge Station (right, recharges battery only, no HP/respawn)
            var station = new GameObject("BatteryChargeStation");
            station.transform.position = new Vector3(8.5f, 1f, 0f);
            station.transform.localScale = new Vector3(1.2f, 2f, 1f);
            var stSr = station.AddComponent<SpriteRenderer>();
            stSr.sprite = sprite;
            stSr.color = new Color(0.20f, 0.68f, 0.95f, 0.55f);
            station.AddComponent<BoxCollider2D>().isTrigger = true;
            var chargeStation = station.AddComponent<BatteryChargeStation>();
            var stSo = new SerializedObject(chargeStation);
            stSo.FindProperty("input").objectReferenceValue = reader;
            stSo.ApplyModifiedPropertiesWithoutUndo();

            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            Debug.Log($"[UnderTheRain] Created combat test room at {ScenePath}");
        }

        private static void EnsureSimpleBatonAsset()
        {
            if (AssetDatabase.LoadAssetAtPath<WeaponDataSO>(WeaponDataPath) != null) return;
            Directory.CreateDirectory(Path.GetDirectoryName(WeaponDataPath));
            var baton = ScriptableObject.CreateInstance<WeaponDataSO>();
            AssetDatabase.CreateAsset(baton, WeaponDataPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(WeaponDataPath);
        }

        private static BatteryProjectile EnsureBatteryProjectilePrefab(int playerHitboxLayer)
        {
            var existing = AssetDatabase.LoadAssetAtPath<BatteryProjectile>(ProjectilePrefabPath);
            if (existing != null) return existing;

            Directory.CreateDirectory(Path.GetDirectoryName(ProjectilePrefabPath));
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(SquareSpritePath);
            var go = new GameObject("BatteryProjectile") { layer = playerHitboxLayer };
            go.transform.localScale = new Vector3(0.45f, 0.18f, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = new Color(0.306f, 0.804f, 0.769f, 1f); // #4ECDC4
            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            go.AddComponent<BatteryProjectile>();

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, ProjectilePrefabPath);
            Object.DestroyImmediate(go);
            AssetDatabase.SaveAssets();
            return prefab.GetComponent<BatteryProjectile>();
        }

        private static void EnsureDroppedPackPrefab()
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(DroppedPackPrefabPath) != null) return;

            Directory.CreateDirectory(Path.GetDirectoryName(DroppedPackPrefabPath));
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(SquareSpritePath);
            var go = new GameObject("DroppedPack");
            go.transform.localScale = new Vector3(0.75f, 0.75f, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = new Color(0.68f, 0.35f, 0.92f, 0.9f);
            go.AddComponent<BoxCollider2D>().isTrigger = true;
            go.AddComponent<DroppedPack>();

            PrefabUtility.SaveAsPrefabAsset(go, DroppedPackPrefabPath);
            Object.DestroyImmediate(go);
            AssetDatabase.SaveAssets();
        }

        private static void EnsurePartsPickupPrefab()
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(PartsPickupPrefabPath) != null) return;

            Directory.CreateDirectory(Path.GetDirectoryName(PartsPickupPrefabPath));
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(SquareSpritePath);
            var go = new GameObject("PartsPickup");
            go.transform.localScale = new Vector3(0.38f, 0.38f, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = new Color(0.96f, 0.78f, 0.22f, 1f);
            go.AddComponent<BoxCollider2D>().isTrigger = true;
            go.AddComponent<PartsPickup>();

            PrefabUtility.SaveAsPrefabAsset(go, PartsPickupPrefabPath);
            Object.DestroyImmediate(go);
            AssetDatabase.SaveAssets();
        }

        private static void CreateBatteryCell(string name, Sprite sprite, Vector2 pos)
        {
            var go = new GameObject(name);
            go.transform.position = pos;
            go.transform.localScale = new Vector3(0.4f, 0.4f, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = new Color(0.306f, 0.804f, 0.769f, 1f);
            go.AddComponent<BoxCollider2D>().isTrigger = true;
            go.AddComponent<BatteryCellPickup>();
        }

        internal static void ConfigureCombatCollisionMatrix()
        {
            int[] combatLayers =
            {
                LayerMask.NameToLayer(PlayerHitboxLayer),
                LayerMask.NameToLayer(PlayerHurtboxLayer),
                LayerMask.NameToLayer(EnemyHitboxLayer),
                LayerMask.NameToLayer(EnemyHurtboxLayer)
            };

            // Isolate combat layers from all other layers first
            for (int a = 0; a < combatLayers.Length; a++)
            {
                for (int b = 0; b < 32; b++)
                {
                    Physics2D.IgnoreLayerCollision(combatLayers[a], b, true);
                }
            }

            // Enable only PlayerHitbox <-> EnemyHurtbox and EnemyHitbox <-> PlayerHurtbox
            Physics2D.IgnoreLayerCollision(combatLayers[0], combatLayers[3], false);
            Physics2D.IgnoreLayerCollision(combatLayers[2], combatLayers[1], false);
        }

        private static GameObject CreateBlock(string name, Sprite sprite, int layer, Vector2 pos, Vector2 size, Color color)
        {
            var go = new GameObject(name) { layer = layer };
            go.transform.position = pos;
            go.transform.localScale = new Vector3(size.x, size.y, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = color;
            go.AddComponent<BoxCollider2D>().size = Vector2.one;
            return go;
        }

        internal static void EnsureLayer(string layerName)
        {
            if (LayerMask.NameToLayer(layerName) >= 0) return;

            var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty layers = tagManager.FindProperty("layers");
            for (int i = 8; i < layers.arraySize; i++)
            {
                SerializedProperty slot = layers.GetArrayElementAtIndex(i);
                if (string.IsNullOrEmpty(slot.stringValue))
                {
                    slot.stringValue = layerName;
                    tagManager.ApplyModifiedProperties();
                    return;
                }
            }
            Debug.LogError($"[UnderTheRain] No free layer slot for '{layerName}'.");
        }
    }
}
