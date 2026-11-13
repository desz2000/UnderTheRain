using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnderTheRain.Combat;
using UnderTheRain.Companion;
using UnderTheRain.Core.Input;
using UnderTheRain.Enemies;
using UnderTheRain.Player;
using UnderTheRain.World;

namespace UnderTheRain.Editor
{
    /// <summary>
    /// Scaffolds Story 4.1 cat test room: CatConfigSO asset, Companion layer + collision matrix,
    /// Cat prefab, Dante with WallJump + melee/ranged combat, platforms, climbing shaft, and a TrainingDummy.
    /// </summary>
    public static class CatTestRoomSetup
    {
        private const string MovementConfigPath = "Assets/_Project/Data/Abilities/PlayerMovementConfig.asset";
        private const string WeaponDataPath = "Assets/_Project/Data/Weapons/SimpleBaton.asset";
        private const string CatConfigPath = "Assets/_Project/Data/Companion/CatConfig.asset";
        private const string ProjectilePrefabPath = "Assets/_Project/Prefabs/Player/BatteryProjectile.prefab";
        private const string CatPrefabPath = "Assets/_Project/Prefabs/Companion/Cat.prefab";
        private const string ScenePath = "Assets/_Project/Scenes/Zones/CatTestRoom.unity";
        private const string InputActionsPath = "Assets/InputSystem_Actions.inputactions";
        private const string SquareSpritePath = "Assets/_Project/Art/Sprites/WhiteSquare.png";

        private const string GroundLayer = "Ground";
        private const string OneWayLayer = "OneWayPlatform";
        private const string CompanionLayer = "Companion";
        private const string PlayerHitboxLayer = "PlayerHitbox";
        private const string PlayerHurtboxLayer = "PlayerHurtbox";
        private const string EnemyHurtboxLayer = "EnemyHurtbox";

        [MenuItem("UnderTheRain/Setup/Create Cat Test Room")]
        public static void CreateCatTestRoom()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogError("[UnderTheRain] Exit Play Mode before running Setup menu commands.");
                return;
            }

            EnsureLayer(GroundLayer);
            EnsureLayer(OneWayLayer);
            EnsureLayer(CompanionLayer);
            EnsureLayer(PlayerHitboxLayer);
            EnsureLayer(PlayerHurtboxLayer);
            EnsureLayer(EnemyHurtboxLayer);
            ConfigureCompanionCollisionMatrix();

            int ground = LayerMask.NameToLayer(GroundLayer);
            int oneWay = LayerMask.NameToLayer(OneWayLayer);
            int companion = LayerMask.NameToLayer(CompanionLayer);
            int pHit = LayerMask.NameToLayer(PlayerHitboxLayer);
            int pHurt = LayerMask.NameToLayer(PlayerHurtboxLayer);
            int eHurt = LayerMask.NameToLayer(EnemyHurtboxLayer);

            var catConfig = EnsureCatConfigAsset();
            var catPrefab = EnsureCatPrefab(companion, ground, oneWay, catConfig);
            var baton = AssetDatabase.LoadAssetAtPath<WeaponDataSO>(WeaponDataPath);
            if (baton != null && baton.damage < 2)
            {
                baton.damage = 2;
                EditorUtility.SetDirty(baton);
                AssetDatabase.SaveAssets();
            }

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(SquareSpritePath);

            // Camera
            var camGo = new GameObject("Main Camera") { tag = "MainCamera" };
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5.4f;
            cam.backgroundColor = new Color(0.055f, 0.063f, 0.078f, 1f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            camGo.transform.position = new Vector3(0f, 2f, -10f);
            camGo.AddComponent<AudioListener>();
            var roomCam = camGo.AddComponent<RoomCamera2D>();
            var camShake = camGo.AddComponent<CameraShake2D>();

            // Input
            var inputGo = new GameObject("GameInputReader");
            var reader = inputGo.AddComponent<GameInputReader>();
            var inputAsset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputActionsPath);
            var readerSo = new SerializedObject(reader);
            readerSo.FindProperty("inputActions").objectReferenceValue = inputAsset;
            readerSo.ApplyModifiedPropertiesWithoutUndo();

            // Floor, outer walls, and climbing shaft (right side) + platforms (left/center)
            CreateBlock("Floor", sprite, ground, new Vector2(0f, -1f), new Vector2(30f, 2f), new Color(0.25f, 0.28f, 0.33f));
            CreateBlock("Wall Left", sprite, ground, new Vector2(-14.5f, 4f), new Vector2(1f, 12f), new Color(0.25f, 0.28f, 0.33f));
            CreateBlock("Wall Right", sprite, ground, new Vector2(14.5f, 4f), new Vector2(1f, 12f), new Color(0.25f, 0.28f, 0.33f));

            // Climbing shaft at far right; bottom clearance is 2.2 units so Dante (height 1.6) fits easily
            CreateBlock("Climb Wall Left", sprite, ground, new Vector2(8.0f, 5.7f), new Vector2(1f, 7f), new Color(0.28f, 0.32f, 0.38f));
            CreateBlock("Climb Wall Right", sprite, ground, new Vector2(12.2f, 4.5f), new Vector2(1f, 9f), new Color(0.28f, 0.32f, 0.38f));

            // Solid platforms for follow & platforming test (no one-way pass-through)
            CreateBlock("Platform Low", sprite, ground, new Vector2(-5.5f, 1.8f), new Vector2(4f, 0.5f), new Color(0.45f, 0.52f, 0.62f, 1f));
            CreateBlock("Platform Mid", sprite, ground, new Vector2(-1.5f, 3.6f), new Vector2(4f, 0.5f), new Color(0.45f, 0.52f, 0.62f, 1f));
            CreateBlock("Platform High", sprite, ground, new Vector2(-6.5f, 5.4f), new Vector2(4f, 0.5f), new Color(0.45f, 0.52f, 0.62f, 1f));

            // Dante
            var player = new GameObject("Dante");
            player.transform.position = new Vector3(-2f, 1f, 0f);
            player.transform.localScale = new Vector3(0.8f, 1.6f, 1f);
            var sr = player.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = new Color(0.31f, 0.8f, 0.77f);
            var rb = player.AddComponent<Rigidbody2D>();
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            player.AddComponent<BoxCollider2D>().size = Vector2.one;
            var playerHealth = player.AddComponent<HealthComponent>();
            var battery = player.AddComponent<BatteryComponent>();

            var controller = player.AddComponent<PlayerController>();
            var ctrlSo = new SerializedObject(controller);
            ctrlSo.FindProperty("input").objectReferenceValue = reader;
            ctrlSo.FindProperty("config").objectReferenceValue = AssetDatabase.LoadAssetAtPath<PlayerMovementConfigSO>(MovementConfigPath);
            ctrlSo.FindProperty("groundMask").intValue = (1 << ground) | (1 << oneWay);
            ctrlSo.FindProperty("oneWayMask").intValue = 1 << oneWay;
            ctrlSo.FindProperty("abilities").FindPropertyRelative("wallJump").boolValue = true;
            ctrlSo.ApplyModifiedPropertiesWithoutUndo();

            // Player Hurtbox
            var pHurtGo = new GameObject("PlayerHurtbox") { layer = pHurt };
            pHurtGo.transform.SetParent(player.transform, false);
            var pHurtCol = pHurtGo.AddComponent<BoxCollider2D>();
            pHurtCol.isTrigger = true;
            pHurtCol.size = Vector2.one;
            var pHurtbox = pHurtGo.AddComponent<Hurtbox>();
            var pHurtSo = new SerializedObject(pHurtbox);
            pHurtSo.FindProperty("damageableTarget").objectReferenceValue = playerHealth;
            pHurtSo.ApplyModifiedPropertiesWithoutUndo();

            // Melee Combat
            var feedback = player.AddComponent<CombatFeedback>();
            var melee = player.AddComponent<PlayerMeleeCombat>();
            var meleeHitGo = new GameObject("MeleeHitbox") { layer = pHit };
            meleeHitGo.transform.SetParent(player.transform, false);
            var meleeBox = meleeHitGo.AddComponent<BoxCollider2D>();
            meleeBox.isTrigger = true;
            meleeBox.enabled = false;
            var meleeHitbox = meleeHitGo.AddComponent<Hitbox>();

            var swingGo = new GameObject("SwingVisual");
            swingGo.transform.SetParent(player.transform, false);
            var swingSr = swingGo.AddComponent<SpriteRenderer>();
            swingSr.sprite = sprite;
            swingSr.enabled = false;

            var meleeSo = new SerializedObject(melee);
            meleeSo.FindProperty("input").objectReferenceValue = reader;
            meleeSo.FindProperty("playerController").objectReferenceValue = controller;
            meleeSo.FindProperty("weaponData").objectReferenceValue = AssetDatabase.LoadAssetAtPath<WeaponDataSO>(WeaponDataPath);
            meleeSo.FindProperty("meleeHitbox").objectReferenceValue = meleeHitbox;
            meleeSo.FindProperty("swingVisual").objectReferenceValue = swingSr;
            meleeSo.FindProperty("combatFeedback").objectReferenceValue = feedback;
            meleeSo.ApplyModifiedPropertiesWithoutUndo();

            // Ranged Combat
            var ranged = player.AddComponent<PlayerRangedCombat>();
            var projectilePrefab = AssetDatabase.LoadAssetAtPath<BatteryProjectile>(ProjectilePrefabPath);
            var rangedSo = new SerializedObject(ranged);
            rangedSo.FindProperty("input").objectReferenceValue = reader;
            rangedSo.FindProperty("playerController").objectReferenceValue = controller;
            rangedSo.FindProperty("battery").objectReferenceValue = battery;
            rangedSo.FindProperty("projectilePrefab").objectReferenceValue = projectilePrefab;
            rangedSo.FindProperty("solidMask").intValue = 1 << ground;
            rangedSo.ApplyModifiedPropertiesWithoutUndo();

            // Cat Companion
            var catGo = (GameObject)PrefabUtility.InstantiatePrefab(catPrefab);
            catGo.transform.position = new Vector3(-3.2f, 0.5f, 0f);
            var catCtrl = catGo.GetComponent<CatController>();
            var catSo = new SerializedObject(catCtrl);
            catSo.FindProperty("playerController").objectReferenceValue = controller;
            catSo.FindProperty("meleeCombat").objectReferenceValue = melee;
            catSo.FindProperty("rangedCombat").objectReferenceValue = ranged;
            catSo.FindProperty("groundMask").intValue = (1 << ground) | (1 << oneWay);
            catSo.FindProperty("oneWayMask").intValue = 1 << oneWay;
            catSo.ApplyModifiedPropertiesWithoutUndo();

            var catInterference = catGo.GetComponent<CatInterference>();
            if (catInterference != null)
            {
                var reloadedConfig = AssetDatabase.LoadAssetAtPath<CatConfigSO>(CatConfigPath);
                var ciSo = new SerializedObject(catInterference);
                ciSo.FindProperty("input").objectReferenceValue = reader;
                ciSo.FindProperty("config").objectReferenceValue = reloadedConfig;
                ciSo.FindProperty("battery").objectReferenceValue = battery;
                ciSo.ApplyModifiedPropertiesWithoutUndo();
            }

            var catExploration = catGo.GetComponent<CatExploration>();
            if (catExploration != null)
            {
                var ceSo = new SerializedObject(catExploration);
                ceSo.FindProperty("input").objectReferenceValue = reader;
                ceSo.FindProperty("catController").objectReferenceValue = catCtrl;
                ceSo.ApplyModifiedPropertiesWithoutUndo();
            }

            // Story 4.2 Interference Test Targets: CollectiveMachine vs MisanthropeHuman
            CreatePatrolTarget("ScoutRobot (Collective)", sprite, new Vector2(-8f, 0.7f),
                EnemyFaction.CollectiveMachine, new Color(0.45f, 0.75f, 0.95f, 1f));
            CreatePatrolTarget("MisanthropeGuard (Immune)", sprite, new Vector2(1.2f, 0.7f),
                EnemyFaction.MisanthropeHuman, new Color(0.85f, 0.28f, 0.35f, 1f));

            // Story 4.3 Exploration Test: NarrowDuct opening ShortcutDoor at the Wall Jump shaft entrance
            CreateDuctShortcut(sprite, ground, new Vector2(8.0f, 1.1f));

            // Training Dummy for combat perch test
            var dummyGo = new GameObject("TrainingDummy");
            dummyGo.transform.position = new Vector3(3.2f, 0.8f, 0f);
            dummyGo.transform.localScale = new Vector3(0.9f, 1.6f, 1f);
            var dummySr = dummyGo.AddComponent<SpriteRenderer>();
            dummySr.sprite = sprite;
            dummySr.color = new Color(0.85f, 0.55f, 0.25f);
            dummyGo.AddComponent<BoxCollider2D>().size = Vector2.one;
            var dummyHealth = dummyGo.AddComponent<HealthComponent>();
            var dummy = dummyGo.AddComponent<TrainingDummy>();
            var dummySo = new SerializedObject(dummy);
            dummySo.FindProperty("spriteRenderer").objectReferenceValue = dummySr;
            dummySo.ApplyModifiedPropertiesWithoutUndo();

            var dHurtGo = new GameObject("DummyHurtbox") { layer = eHurt };
            dHurtGo.transform.SetParent(dummyGo.transform, false);
            var dHurtCol = dHurtGo.AddComponent<BoxCollider2D>();
            dHurtCol.isTrigger = true;
            dHurtCol.size = Vector2.one;
            var dHurtbox = dHurtGo.AddComponent<Hurtbox>();
            var dHurtSo = new SerializedObject(dHurtbox);
            dHurtSo.FindProperty("damageableTarget").objectReferenceValue = dummyHealth;
            dHurtSo.ApplyModifiedPropertiesWithoutUndo();

            // Camera setup (unconfined in test room so it always follows Dante smoothly)
            var camSo = new SerializedObject(roomCam);
            camSo.FindProperty("target").objectReferenceValue = player.transform;
            camSo.ApplyModifiedPropertiesWithoutUndo();

            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            Debug.Log($"[UnderTheRain] Created cat test room at {ScenePath}");
        }

        private static CatConfigSO EnsureCatConfigAsset()
        {
            var existing = AssetDatabase.LoadAssetAtPath<CatConfigSO>(CatConfigPath);
            if (existing != null)
            {
                existing.followOffset = new Vector2(-0.68f, 0f);
                existing.followSpeed = 10f;
                existing.stopDistance = 0.08f;
                existing.traversalPerchHoldDuration = 2.0f;
                existing.highGroundPerchThresholdY = 0.9f;
                existing.highGroundPerchDelay = 0.15f;
                existing.interferenceRadius = 4.5f;
                existing.interferenceCooldown = 3.0f;
                existing.hesitationDuration = 1.35f;
                existing.interferenceBatteryCost = 50f;
                EditorUtility.SetDirty(existing);
                AssetDatabase.SaveAssets();
                return existing;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(CatConfigPath));
            var config = ScriptableObject.CreateInstance<CatConfigSO>();
            AssetDatabase.CreateAsset(config, CatConfigPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(CatConfigPath);
            return config;
        }

        private static GameObject EnsureCatPrefab(int companionLayer, int groundLayer, int oneWayLayer, CatConfigSO config)
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(CatPrefabPath);
            if (existing != null)
            {
                var contents = PrefabUtility.LoadPrefabContents(CatPrefabPath);
                var existingCtrl = contents.GetComponent<CatController>();
                if (existingCtrl != null)
                {
                    var existingSo = new SerializedObject(existingCtrl);
                    existingSo.FindProperty("config").objectReferenceValue = config;
                    existingSo.FindProperty("groundMask").intValue = (1 << groundLayer) | (1 << oneWayLayer);
                    existingSo.FindProperty("oneWayMask").intValue = 1 << oneWayLayer;
                    existingSo.ApplyModifiedPropertiesWithoutUndo();
                }

                var existingInterference = contents.GetComponent<CatInterference>();
                if (existingInterference == null)
                {
                    existingInterference = contents.AddComponent<CatInterference>();
                }
                Transform pulseChild = contents.transform.Find("PulseVisual");
                SpriteRenderer pulseSr = pulseChild != null ? pulseChild.GetComponent<SpriteRenderer>() : null;
                if (pulseSr == null)
                {
                    var spriteRef = AssetDatabase.LoadAssetAtPath<Sprite>(SquareSpritePath);
                    var pulseGo = new GameObject("PulseVisual");
                    pulseGo.transform.SetParent(contents.transform, false);
                    pulseSr = pulseGo.AddComponent<SpriteRenderer>();
                    pulseSr.sprite = spriteRef;
                    pulseSr.sortingOrder = 4;
                    pulseSr.enabled = false;
                }
                var existingCiSo = new SerializedObject(existingInterference);
                existingCiSo.FindProperty("config").objectReferenceValue = config;
                existingCiSo.FindProperty("pulseVisual").objectReferenceValue = pulseSr;
                existingCiSo.ApplyModifiedPropertiesWithoutUndo();

                if (contents.GetComponent<CatExploration>() == null)
                {
                    contents.AddComponent<CatExploration>();
                }

                PrefabUtility.SaveAsPrefabAsset(contents, CatPrefabPath);
                PrefabUtility.UnloadPrefabContents(contents);
                AssetDatabase.SaveAssets();
                return AssetDatabase.LoadAssetAtPath<GameObject>(CatPrefabPath);
            }

            Directory.CreateDirectory(Path.GetDirectoryName(CatPrefabPath));
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(SquareSpritePath);
            var go = new GameObject("Cat") { layer = companionLayer };
            go.transform.localScale = new Vector3(0.55f, 0.45f, 1f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = new Color(0.94f, 0.96f, 1f, 1f); // Metal-white cat
            sr.sortingOrder = 5;

            var rb = go.AddComponent<Rigidbody2D>();
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            var col = go.AddComponent<BoxCollider2D>();
            col.size = Vector2.one;

            var ctrl = go.AddComponent<CatController>();
            var so = new SerializedObject(ctrl);
            so.FindProperty("config").objectReferenceValue = config;
            so.FindProperty("groundMask").intValue = (1 << groundLayer) | (1 << oneWayLayer);
            so.FindProperty("oneWayMask").intValue = 1 << oneWayLayer;
            so.FindProperty("spriteRenderer").objectReferenceValue = sr;
            so.ApplyModifiedPropertiesWithoutUndo();

            var pulseObj = new GameObject("PulseVisual");
            pulseObj.transform.SetParent(go.transform, false);
            var pSr = pulseObj.AddComponent<SpriteRenderer>();
            pSr.sprite = sprite;
            pSr.sortingOrder = 4;
            pSr.enabled = false;

            var interference = go.AddComponent<CatInterference>();
            var ciPrefabSo = new SerializedObject(interference);
            ciPrefabSo.FindProperty("config").objectReferenceValue = config;
            ciPrefabSo.FindProperty("pulseVisual").objectReferenceValue = pSr;
            ciPrefabSo.ApplyModifiedPropertiesWithoutUndo();

            go.AddComponent<CatExploration>();

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, CatPrefabPath);
            Object.DestroyImmediate(go);
            AssetDatabase.SaveAssets();
            return prefab;
        }

        private static void ConfigureCompanionCollisionMatrix()
        {
            int companion = LayerMask.NameToLayer(CompanionLayer);
            int ground = LayerMask.NameToLayer(GroundLayer);
            int oneWay = LayerMask.NameToLayer(OneWayLayer);

            for (int i = 0; i < 32; i++)
            {
                bool shouldCollide = i == ground || i == oneWay;
                Physics2D.IgnoreLayerCollision(companion, i, !shouldCollide);
            }
        }

        private static void EnsureLayer(string layerName)
        {
            if (LayerMask.NameToLayer(layerName) != -1) return;
            var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var layers = tagManager.FindProperty("layers");
            for (int i = 8; i < layers.arraySize; i++)
            {
                var prop = layers.GetArrayElementAtIndex(i);
                if (string.IsNullOrEmpty(prop.stringValue))
                {
                    prop.stringValue = layerName;
                    tagManager.ApplyModifiedPropertiesWithoutUndo();
                    return;
                }
            }
            Debug.LogError($"[CatTestRoomSetup] No empty layer slot available for '{layerName}'.");
        }

        private static void CreateBlock(string name, Sprite sprite, int layer, Vector2 pos, Vector2 size, Color color)
        {
            var go = new GameObject(name) { layer = layer };
            go.transform.position = pos;
            go.transform.localScale = new Vector3(size.x, size.y, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = color;
            go.AddComponent<BoxCollider2D>().size = Vector2.one;
        }

        private static void CreateDuctShortcut(Sprite sprite, int groundLayer, Vector2 doorPos)
        {
            // Door blocking the 2.2-unit passage into the Wall Jump shaft until the cat traverses the duct
            var door = new GameObject("ShortcutDoor") { layer = groundLayer };
            door.transform.position = doorPos;
            door.transform.localScale = new Vector3(0.8f, 2.2f, 1f);
            var doorSr = door.AddComponent<SpriteRenderer>();
            doorSr.sprite = sprite;
            doorSr.color = new Color(0.65f, 0.35f, 0.22f, 1f);
            door.AddComponent<BoxCollider2D>().size = Vector2.one;

            // Narrow duct next to the door
            var duct = new GameObject("NarrowDuct");
            duct.transform.position = new Vector2(doorPos.x - 1.3f, 0.5f);
            duct.transform.localScale = new Vector3(0.9f, 0.8f, 1f);
            var ductSr = duct.AddComponent<SpriteRenderer>();
            ductSr.sprite = sprite;
            ductSr.color = new Color(0.32f, 0.58f, 0.62f, 0.85f);
            var ductCol = duct.AddComponent<BoxCollider2D>();
            ductCol.isTrigger = true;
            ductCol.size = new Vector2(1.4f, 1.4f);

            var prompt = new GameObject("PromptGlyph");
            prompt.transform.SetParent(duct.transform, false);
            prompt.transform.localPosition = new Vector3(0f, 0.9f, 0f);
            prompt.transform.localScale = new Vector3(0.35f, 0.35f, 1f);
            var promptSr = prompt.AddComponent<SpriteRenderer>();
            promptSr.sprite = sprite;
            promptSr.color = new Color(0.96f, 0.86f, 0.32f, 1f);
            promptSr.sortingOrder = 6;
            prompt.SetActive(false);

            var entry = new GameObject("CatEntry");
            entry.transform.SetParent(duct.transform, false);
            entry.transform.position = new Vector3(doorPos.x - 1.0f, 0.35f, 0f);

            var target = new GameObject("CatExit");
            target.transform.SetParent(duct.transform, false);
            target.transform.position = new Vector3(doorPos.x + 1.1f, 0.35f, 0f);

            var point = duct.AddComponent<CatExplorationPoint>();
            var so = new SerializedObject(point);
            so.FindProperty("explorationType").enumValueIndex = (int)CatExplorationType.Duct;
            so.FindProperty("catEntryPoint").objectReferenceValue = entry.transform;
            so.FindProperty("catTargetPoint").objectReferenceValue = target.transform;
            so.FindProperty("promptIndicator").objectReferenceValue = prompt;
            var disableProp = so.FindProperty("objectsToDisableOnComplete");
            disableProp.arraySize = 1;
            disableProp.GetArrayElementAtIndex(0).objectReferenceValue = door;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void CreatePatrolTarget(string name, Sprite sprite, Vector2 pos, EnemyFaction faction, Color color)
        {
            var go = new GameObject(name);
            go.transform.position = pos;
            go.transform.localScale = new Vector3(0.8f, 1.4f, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = color;

            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = Vector2.one;

            var glyphGo = new GameObject("HesitationGlyph");
            glyphGo.transform.SetParent(go.transform, false);
            glyphGo.transform.localPosition = new Vector3(0f, 0.85f, 0f);
            glyphGo.transform.localScale = new Vector3(0.35f, 0.25f, 1f);
            var glyphSr = glyphGo.AddComponent<SpriteRenderer>();
            glyphSr.sprite = sprite;
            glyphSr.color = new Color(1f, 0.92f, 0.25f, 1f);
            glyphSr.sortingOrder = 6;
            glyphSr.enabled = false;

            var patrol = go.AddComponent<PatrolInterferenceTarget>();
            var so = new SerializedObject(patrol);
            so.FindProperty("faction").enumValueIndex = (int)faction;
            so.FindProperty("bodyRenderer").objectReferenceValue = sr;
            so.FindProperty("hesitationGlyph").objectReferenceValue = glyphSr;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void CreateOneWayPlatform(string name, Sprite sprite, int layer, Vector2 pos, Vector2 size)
        {
            var go = new GameObject(name) { layer = layer };
            go.transform.position = pos;
            go.transform.localScale = new Vector3(size.x, size.y, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = new Color(0.45f, 0.52f, 0.62f, 1f);
            var col = go.AddComponent<BoxCollider2D>();
            col.size = Vector2.one;
            col.usedByEffector = true;
            var effector = go.AddComponent<PlatformEffector2D>();
            effector.useOneWay = true;
            effector.surfaceArc = 160f;
        }
    }
}
