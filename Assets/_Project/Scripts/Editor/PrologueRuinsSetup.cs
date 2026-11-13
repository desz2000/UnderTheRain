using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnderTheRain.Combat;
using UnderTheRain.Companion;
using UnderTheRain.Core.Input;
using UnderTheRain.Enemies;
using UnderTheRain.Narrative;
using UnderTheRain.Player;
using UnderTheRain.World;

namespace UnderTheRain.Editor
{
    /// <summary>
    /// Scaffolds the short underground Prologue / Tutorial zone: an abandoned metro station that turns out
    /// to hide a Domus Corp lab. Teaches movement, jump, Kitty's vent exploration and interference, basic combat,
    /// and leads to the Domus Corp terminal discovery. No rain: the city sky is still far above.
    /// </summary>
    public static class PrologueRuinsSetup
    {
        private const string MovementConfigPath = "Assets/_Project/Data/Abilities/PlayerMovementConfig.asset";
        private const string WeaponDataPath = "Assets/_Project/Data/Weapons/SimpleBaton.asset";
        private const string CatConfigPath = "Assets/_Project/Data/Companion/CatConfig.asset";
        private const string ProjectilePrefabPath = "Assets/_Project/Prefabs/Player/BatteryProjectile.prefab";
        private const string DroppedPackPrefabPath = "Assets/_Project/Prefabs/Environment/DroppedPack.prefab";
        private const string PartsPickupPrefabPath = "Assets/_Project/Prefabs/Environment/PartsPickup.prefab";
        private const string CatPrefabPath = "Assets/_Project/Prefabs/Companion/Cat.prefab";
        private const string InputActionsPath = "Assets/InputSystem_Actions.inputactions";
        private const string SquareSpritePath = "Assets/_Project/Art/Sprites/WhiteSquare.png";

        private const string NarrativeDataDir = "Assets/_Project/Data/Narrative";
        private const string ScenePath = "Assets/_Project/Scenes/Zones/Prologue_Ruins.unity";

        private const string GroundLayer = "Ground";
        private const string OneWayLayer = "OneWayPlatform";
        private const string PlayerHitboxLayer = "PlayerHitbox";
        private const string PlayerHurtboxLayer = "PlayerHurtbox";

        [MenuItem("UnderTheRain/Setup/Create Prologue Ruins Scene")]
        public static void CreatePrologueRuinsScene()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogError("[UnderTheRain] Exit Play Mode before running Setup menu commands.");
                return;
            }

            Directory.CreateDirectory(NarrativeDataDir);

            var doc = EnsureDocumentAsset();
            var terminalDialogue = EnsureTerminalDialogueAsset();
            AssetDatabase.SaveAssets();

            EnemySetupUtility.EnsureCombatLayers();
            int ground = LayerMask.NameToLayer(GroundLayer);
            int oneWay = LayerMask.NameToLayer(OneWayLayer);
            int pHit = LayerMask.NameToLayer(PlayerHitboxLayer);
            int pHurt = LayerMask.NameToLayer(PlayerHurtboxLayer);

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(SquareSpritePath);

            // Camera
            var camGo = new GameObject("Main Camera") { tag = "MainCamera" };
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5.4f;
            cam.backgroundColor = new Color(0.039f, 0.051f, 0.070f, 1f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            camGo.transform.position = new Vector3(-8f, 1.5f, -10f);
            camGo.AddComponent<AudioListener>();
            var roomCam = camGo.AddComponent<RoomCamera2D>();
            camGo.AddComponent<CameraShake2D>();

            // Input & Device Tracker
            var inputGo = new GameObject("GameInputReader");
            var reader = inputGo.AddComponent<GameInputReader>();
            var inputAsset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputActionsPath);
            var readerSo = new SerializedObject(reader);
            readerSo.FindProperty("inputActions").objectReferenceValue = inputAsset;
            readerSo.ApplyModifiedPropertiesWithoutUndo();
            var tracker = inputGo.AddComponent<InputDeviceTracker>();

            // Fade Canvas + ScreenFader + RoomTransitionController
            var fader = CreateScreenFader();
            var transitionGo = new GameObject("RoomTransitionController");
            var transitionCtrl = transitionGo.AddComponent<RoomTransitionController>();

            // Dante
            var player = CreatePlayer(reader, sprite, ground, oneWay, pHit, pHurt);
            var controller = player.GetComponent<PlayerController>();
            var melee = player.GetComponent<PlayerMeleeCombat>();
            var ranged = player.GetComponent<PlayerRangedCombat>();
            var battery = player.GetComponent<BatteryComponent>();
            var narrativeState = player.AddComponent<NarrativeState>();

            // Dialogue Runner
            var dialogueGo = new GameObject("DialogueRunner");
            var dialogueRunner = dialogueGo.AddComponent<DialogueRunner>();
            dialogueRunner.Initialize(reader, controller, tracker);

            // Kitty Companion
            var catPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(CatPrefabPath);
            if (catPrefab != null)
            {
                var catGo = (GameObject)PrefabUtility.InstantiatePrefab(catPrefab);
                catGo.name = "Kitty";
                catGo.transform.position = new Vector3(-10.8f, 0.6f, 0f);
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
                    var ciSo = new SerializedObject(catInterference);
                    ciSo.FindProperty("input").objectReferenceValue = reader;
                    ciSo.FindProperty("config").objectReferenceValue = AssetDatabase.LoadAssetAtPath<CatConfigSO>(CatConfigPath);
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
            }

            // Room 1: Abandoned Metro Platform (movement/jump tutorial + Kitty vent; underground, no rain)
            var room1 = CreateRoom("Prologue_01_MetroPlatform", "prologue_metro_platform", Vector2.zero, new Vector2(28f, 12f));
            BuildMetroPlatformRoom(room1.transform, sprite, ground);

            // Room 2: Hidden Domus Lab behind the metro (Kitty interference + combat + Domus Terminal)
            var room2 = CreateRoom("Prologue_02_HiddenLab", "prologue_hidden_lab", new Vector2(38f, 0f), new Vector2(32f, 12f));
            BuildHiddenLabRoom(room2.transform, sprite, ground, reader, narrativeState, dialogueRunner, doc, terminalDialogue);

            // Wire Room Exits
            var exitToLab = CreateRoomExit("Exit_ToHiddenLab", room1.transform, new Vector2(13.2f, 1.5f), new Vector2(1.4f, 3f));
            exitToLab.Configure(room2, "from_platform");

            var exitToPlatform = CreateRoomExit("Exit_ToMetroPlatform", room2.transform, new Vector2(22.8f, 1.5f), new Vector2(1.4f, 3f));
            exitToPlatform.Configure(room1, "from_lab");

            var exitToSewers = CreateRoomExit("Exit_ToSewersShelter", room2.transform, new Vector2(53.0f, 1.5f), new Vector2(1.4f, 3f));
            exitToSewers.ConfigureScene("Act1_Sewers", "sewers_initial_shelter", "from_prologue");

            // Wire Camera & TransitionController
            var camSo = new SerializedObject(roomCam);
            camSo.FindProperty("target").objectReferenceValue = player.transform;
            camSo.FindProperty("initialRoom").objectReferenceValue = room1;
            camSo.ApplyModifiedPropertiesWithoutUndo();

            var trSo = new SerializedObject(transitionCtrl);
            trSo.FindProperty("screenFader").objectReferenceValue = fader;
            trSo.FindProperty("roomCamera").objectReferenceValue = roomCam;
            trSo.FindProperty("initialRoom").objectReferenceValue = room1;
            trSo.ApplyModifiedPropertiesWithoutUndo();

            room2.gameObject.SetActive(false);

            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
            EditorSceneManager.SaveScene(scene, ScenePath);
            EnsureSceneInBuildSettings(ScenePath);
            AssetDatabase.SaveAssets();
            Debug.Log($"[UnderTheRain] Created Prologue Ruins scene at {ScenePath}");
        }

        private static void BuildMetroPlatformRoom(Transform parent, Sprite sprite, int groundLayer)
        {
            // Abandoned metro platform under the city: dry, dark, no sky above.
            CreateBlock("Floor_Platform", parent, sprite, groundLayer, new Vector2(0f, -1f), new Vector2(28f, 2f), new Color(0.17f, 0.18f, 0.21f));
            CreateBlock("Ceiling", parent, sprite, groundLayer, new Vector2(0f, 6.2f), new Vector2(28f, 1.4f), new Color(0.10f, 0.11f, 0.13f));
            CreateBlock("Wall_Left", parent, sprite, groundLayer, new Vector2(-13.5f, 2.5f), new Vector2(1f, 9f), new Color(0.13f, 0.14f, 0.17f));
            CreateBlock("Wall_RightTop", parent, sprite, groundLayer, new Vector2(13.5f, 4.7f), new Vector2(1f, 4.4f), new Color(0.13f, 0.14f, 0.17f));

            // Background dressing (no colliders): rails, tiled pillars, dead signage strip.
            CreateDecor("Rails", parent, sprite, new Vector2(0f, -0.12f), new Vector2(28f, 0.12f), new Color(0.32f, 0.30f, 0.27f));
            for (int i = 0; i < 5; i++)
            {
                CreateDecor($"Pillar_{i}", parent, sprite, new Vector2(-10f + i * 5f, 2.75f), new Vector2(0.9f, 5.5f), new Color(0.20f, 0.22f, 0.24f));
            }
            CreateDecor("StationSignStrip", parent, sprite, new Vector2(-4f, 4.6f), new Vector2(6f, 0.7f), new Color(0.55f, 0.20f, 0.18f, 0.85f));

            // Collapsed platform debris teaching variable jump
            CreateBlock("Debris_Step1", parent, sprite, groundLayer, new Vector2(-3.5f, 0.8f), new Vector2(3.5f, 1.6f), new Color(0.24f, 0.26f, 0.30f));
            CreateBlock("Debris_Step2", parent, sprite, groundLayer, new Vector2(2.5f, 2.0f), new Vector2(4.0f, 0.6f), new Color(0.28f, 0.30f, 0.35f));
            CreateBlock("Debris_HighLedge", parent, sprite, groundLayer, new Vector2(7.0f, 3.4f), new Vector2(3.0f, 0.5f), new Color(0.28f, 0.30f, 0.35f));

            CreateSpawnPoint("Spawn_Default", parent, "default", new Vector2(-10f, 1f));
            CreateSpawnPoint("Spawn_FromLab", parent, "from_lab", new Vector2(11.95f, 1f));

            CreateSignpost("Hint_MoveJump", parent, new Vector2(-8f, 0.5f), "[A/D] Mover   |   [Espaço] Pular (segure para pular mais alto)");
            CreateSignpost("Sign_Station", parent, new Vector2(-3.8f, 3.2f), "ESTAÇÃO VALE-CINZA — LINHA 7 (DESATIVADA)");

            var partsPrefab = AssetDatabase.LoadAssetAtPath<PartsPickup>(PartsPickupPrefabPath);
            if (partsPrefab != null)
            {
                var pickup = (PartsPickup)PrefabUtility.InstantiatePrefab(partsPrefab, parent);
                pickup.transform.position = new Vector3(7.0f, 4.1f, 0f);
            }

            // Jammed maintenance door: Kitty crawls through the vent and releases it from the other side.
            CreateCatDuctShortcut(parent, sprite, groundLayer, new Vector2(11.0f, 2.75f), 5.5f);
            CreateSignpost("Hint_KittyDuct", parent, new Vector2(9.2f, 0.5f), "Porta emperrada. [E] Kitty: entrar pelo duto");
        }

        private static void BuildHiddenLabRoom(
            Transform parent,
            Sprite sprite,
            int groundLayer,
            GameInputReader reader,
            NarrativeState narrativeState,
            DialogueRunner dialogueRunner,
            DocumentDataSO doc,
            DialogueSequenceSO terminalDialogue)
        {
            // Starts as a metro maintenance tunnel, then turns into sterile Domus Corp lab panels.
            CreateBlock("Floor", parent, sprite, groundLayer, new Vector2(38f, -1f), new Vector2(32f, 2f), new Color(0.17f, 0.18f, 0.21f));
            CreateBlock("Ceiling", parent, sprite, groundLayer, new Vector2(38f, 6.2f), new Vector2(32f, 1.4f), new Color(0.10f, 0.11f, 0.13f));
            CreateBlock("Wall_LeftTop", parent, sprite, groundLayer, new Vector2(22.5f, 4.7f), new Vector2(1f, 4.4f), new Color(0.13f, 0.14f, 0.17f));
            CreateBlock("Wall_RightTop", parent, sprite, groundLayer, new Vector2(53.5f, 4.7f), new Vector2(1f, 4.4f), new Color(0.13f, 0.14f, 0.17f));

            CreateDecor("MaintenanceTunnelBack", parent, sprite, new Vector2(30f, 2.75f), new Vector2(14f, 5.5f), new Color(0.14f, 0.15f, 0.17f));
            CreateDecor("LabPanelsBack", parent, sprite, new Vector2(45.5f, 2.75f), new Vector2(15f, 5.5f), new Color(0.20f, 0.27f, 0.29f));
            CreateDecor("LabLightStrip", parent, sprite, new Vector2(45.5f, 5.3f), new Vector2(15f, 0.15f), new Color(0.306f, 0.804f, 0.769f, 0.8f));

            CreateBlock("Lab_Ledge", parent, sprite, groundLayer, new Vector2(43f, 2.0f), new Vector2(6.0f, 0.55f), new Color(0.30f, 0.36f, 0.40f));

            CreateSpawnPoint("Spawn_FromPlatform", parent, "from_platform", new Vector2(24.5f, 1f));
            CreateSpawnPoint("Spawn_FromSewers", parent, "from_sewers", new Vector2(51.0f, 1f));

            CreateSignpost("Hint_KittyAndCombat", parent, new Vector2(29f, 0.5f), "[R] Kitty: Interferir no robô   |   [Clique Esq.] Bastão   |   [Q] Kit");
            CreateSignpost("Clue_LabReveal", parent, new Vector2(39f, 0.5f), "Isto não é manutenção do metrô... é equipamento da Domus Corp.");

            // Scout robot guarding the hidden lab (affected by Kitty, deals contact damage, can be destroyed)
            EnemySetupUtility.CreateScoutRobot("LabScoutRobot", parent, sprite, new Vector2(34.5f, 0.7f));

            // Domus Corp Terminal (Story trigger)
            var termGo = new GameObject("DomusTerminal");
            termGo.transform.SetParent(parent, false);
            termGo.transform.position = new Vector3(43f, 2.95f, 0f);
            termGo.transform.localScale = new Vector3(0.9f, 1.4f, 1f);
            var termSr = termGo.AddComponent<SpriteRenderer>();
            termSr.sprite = sprite;
            termSr.color = new Color(0.20f, 0.24f, 0.30f, 1f);
            var termCol = termGo.AddComponent<BoxCollider2D>();
            termCol.isTrigger = true;
            termCol.size = new Vector2(1.8f, 1.4f);

            var indGo = new GameObject("ScreenGlow");
            indGo.transform.SetParent(termGo.transform, false);
            indGo.transform.localPosition = new Vector3(0f, 0.25f, 0f);
            indGo.transform.localScale = new Vector3(0.55f, 0.35f, 1f);
            var indSr = indGo.AddComponent<SpriteRenderer>();
            indSr.sprite = sprite;
            indSr.sortingOrder = 2;

            var terminal = termGo.AddComponent<NarrativeTerminal>();
            var termSo = new SerializedObject(terminal);
            termSo.FindProperty("statusIndicator").objectReferenceValue = indSr;
            termSo.ApplyModifiedPropertiesWithoutUndo();
            terminal.Configure(reader, narrativeState, dialogueRunner, doc, terminalDialogue);

            CreateSignpost("Hint_ReturnToSewers", parent, new Vector2(50.5f, 0.5f), "Voltar ao Abrigo nos Esgotos →");
        }

        private static void CreateDecor(string name, Transform parent, Sprite sprite, Vector2 worldPos, Vector2 size, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = worldPos;
            go.transform.localScale = new Vector3(size.x, size.y, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = color;
            sr.sortingOrder = -10;
        }

        private static void CreateCatDuctShortcut(Transform parent, Sprite sprite, int groundLayer, Vector2 doorPos, float doorHeight)
        {
            var door = CreateBlock("JammedMaintenanceDoor", parent, sprite, groundLayer, doorPos, new Vector2(0.8f, doorHeight),
                new Color(0.55f, 0.32f, 0.20f, 1f));

            float floorY = doorPos.y - doorHeight * 0.5f;

            var duct = new GameObject("MaintenanceVent");
            duct.transform.SetParent(parent, false);
            duct.transform.position = new Vector3(doorPos.x - 1.3f, floorY + 0.5f, 0f);
            duct.transform.localScale = new Vector3(0.9f, 0.8f, 1f);
            var ductSr = duct.AddComponent<SpriteRenderer>();
            ductSr.sprite = sprite;
            ductSr.color = new Color(0.32f, 0.58f, 0.62f, 0.85f);
            var ductCol = duct.AddComponent<BoxCollider2D>();
            ductCol.isTrigger = true;
            ductCol.size = new Vector2(2.2f, 2.0f);

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
            entry.transform.position = new Vector3(doorPos.x - 1.0f, floorY + 0.35f, 0f);

            var target = new GameObject("CatExit");
            target.transform.SetParent(duct.transform, false);
            target.transform.position = new Vector3(doorPos.x + 1.1f, floorY + 0.35f, 0f);

            var point = duct.AddComponent<CatExplorationPoint>();
            var so = new SerializedObject(point);
            so.FindProperty("explorationType").enumValueIndex = (int)CatExplorationType.Duct;
            so.FindProperty("catEntryPoint").objectReferenceValue = entry.transform;
            so.FindProperty("catTargetPoint").objectReferenceValue = target.transform;
            so.FindProperty("promptIndicator").objectReferenceValue = prompt;
            so.FindProperty("persistentId").stringValue = "prologue_maintenance_door";
            var disableProp = so.FindProperty("objectsToDisableOnComplete");
            disableProp.arraySize = 1;
            disableProp.GetArrayElementAtIndex(0).objectReferenceValue = door;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static GameObject CreatePlayer(
            GameInputReader reader,
            Sprite sprite,
            int ground,
            int oneWay,
            int pHit,
            int pHurt)
        {
            var player = new GameObject("Dante") { tag = "Player" };
            player.transform.position = new Vector3(-10f, 1f, 0f);
            player.transform.localScale = new Vector3(0.8f, 1.6f, 1f);
            var sr = player.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = new Color(0.306f, 0.804f, 0.769f, 1f);
            var rb = player.AddComponent<Rigidbody2D>();
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            player.AddComponent<BoxCollider2D>().size = Vector2.one;

            var health = player.AddComponent<HealthComponent>();
            var battery = player.AddComponent<BatteryComponent>();
            var medkit = player.AddComponent<MedkitComponent>();
            var partsPack = player.AddComponent<PartsPackComponent>();

            var medkitSo = new SerializedObject(medkit);
            medkitSo.FindProperty("input").objectReferenceValue = reader;
            medkitSo.FindProperty("healthComponent").objectReferenceValue = health;
            medkitSo.ApplyModifiedPropertiesWithoutUndo();

            var partsSo = new SerializedObject(partsPack);
            partsSo.FindProperty("input").objectReferenceValue = reader;
            partsSo.FindProperty("medkitComponent").objectReferenceValue = medkit;
            partsSo.ApplyModifiedPropertiesWithoutUndo();

            var deathHandler = player.AddComponent<PlayerDeathHandler>();
            var dhSo = new SerializedObject(deathHandler);
            dhSo.FindProperty("input").objectReferenceValue = reader;
            dhSo.FindProperty("health").objectReferenceValue = health;
            dhSo.FindProperty("battery").objectReferenceValue = battery;
            dhSo.FindProperty("medkit").objectReferenceValue = medkit;
            dhSo.FindProperty("partsPack").objectReferenceValue = partsPack;
            dhSo.FindProperty("droppedPackPrefab").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<DroppedPack>(DroppedPackPrefabPath);
            dhSo.ApplyModifiedPropertiesWithoutUndo();

            var controller = player.AddComponent<PlayerController>();
            var ctrlSo = new SerializedObject(controller);
            ctrlSo.FindProperty("input").objectReferenceValue = reader;
            ctrlSo.FindProperty("config").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<PlayerMovementConfigSO>(MovementConfigPath);
            ctrlSo.FindProperty("groundMask").intValue = (1 << ground) | (1 << oneWay);
            ctrlSo.FindProperty("oneWayMask").intValue = 1 << oneWay;
            ctrlSo.FindProperty("abilities").FindPropertyRelative("wallJump").boolValue = false;
            ctrlSo.ApplyModifiedPropertiesWithoutUndo();

            var pHurtGo = new GameObject("PlayerHurtbox") { layer = pHurt };
            pHurtGo.transform.SetParent(player.transform, false);
            var pHurtCol = pHurtGo.AddComponent<BoxCollider2D>();
            pHurtCol.isTrigger = true;
            pHurtCol.size = Vector2.one;
            var pHurtbox = pHurtGo.AddComponent<Hurtbox>();
            var pHurtSo = new SerializedObject(pHurtbox);
            pHurtSo.FindProperty("damageableTarget").objectReferenceValue = health;
            pHurtSo.ApplyModifiedPropertiesWithoutUndo();

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
            meleeSo.FindProperty("weaponData").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<WeaponDataSO>(WeaponDataPath);
            meleeSo.FindProperty("meleeHitbox").objectReferenceValue = meleeHitbox;
            meleeSo.FindProperty("swingVisual").objectReferenceValue = swingSr;
            meleeSo.FindProperty("combatFeedback").objectReferenceValue = feedback;
            meleeSo.ApplyModifiedPropertiesWithoutUndo();

            var ranged = player.AddComponent<PlayerRangedCombat>();
            var rangedSo = new SerializedObject(ranged);
            rangedSo.FindProperty("input").objectReferenceValue = reader;
            rangedSo.FindProperty("playerController").objectReferenceValue = controller;
            rangedSo.FindProperty("battery").objectReferenceValue = battery;
            rangedSo.FindProperty("projectilePrefab").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<BatteryProjectile>(ProjectilePrefabPath);
            rangedSo.FindProperty("solidMask").intValue = 1 << ground;
            rangedSo.ApplyModifiedPropertiesWithoutUndo();

            player.AddComponent<DebugCombatHud>();
            return player;
        }

        private static ScreenFader CreateScreenFader()
        {
            var canvasGo = new GameObject("FadeCanvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            canvasGo.AddComponent<CanvasScaler>();

            var fadeGo = new GameObject("ScreenFader");
            fadeGo.transform.SetParent(canvasGo.transform, false);
            var rect = fadeGo.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var img = fadeGo.AddComponent<Image>();
            img.color = Color.black;
            img.raycastTarget = false;

            var group = fadeGo.AddComponent<CanvasGroup>();
            group.alpha = 0f;
            group.blocksRaycasts = false;
            group.interactable = false;

            return fadeGo.AddComponent<ScreenFader>();
        }

        private static RoomBounds CreateRoom(string name, string roomId, Vector2 center, Vector2 size)
        {
            var go = new GameObject(name);
            go.transform.position = center;
            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = size;
            var bounds = go.AddComponent<RoomBounds>();
            bounds.SetRoomId(roomId);
            return bounds;
        }

        private static RoomExitTrigger CreateRoomExit(string name, Transform parent, Vector2 worldPos, Vector2 size)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = worldPos;
            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = size;
            return go.AddComponent<RoomExitTrigger>();
        }

        private static void CreateSpawnPoint(string name, Transform parent, string spawnId, Vector2 worldPos)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = worldPos;
            var sp = go.AddComponent<RoomSpawnPoint>();
            sp.SetSpawnId(spawnId);
        }

        private static void CreateSignpost(string name, Transform parent, Vector2 worldPos, string text)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = worldPos;
            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(3.5f, 2.2f);
            var sign = go.AddComponent<TutorialSignpost>();
            sign.Configure(text);
        }

        private static GameObject CreateBlock(
            string name,
            Transform parent,
            Sprite sprite,
            int layer,
            Vector2 worldPos,
            Vector2 size,
            Color color)
        {
            var go = new GameObject(name) { layer = layer };
            go.transform.SetParent(parent, false);
            go.transform.position = worldPos;
            go.transform.localScale = new Vector3(size.x, size.y, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = color;
            go.AddComponent<BoxCollider2D>().size = Vector2.one;
            return go;
        }

        private static DocumentDataSO EnsureDocumentAsset()
        {
            string path = Path.Combine(NarrativeDataDir, "DomusSabotageLog.asset");
            var asset = AssetDatabase.LoadAssetAtPath<DocumentDataSO>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<DocumentDataSO>();
                AssetDatabase.CreateAsset(asset, path);
            }
            EditorUtility.SetDirty(asset);
            return asset;
        }

        private static DialogueSequenceSO EnsureTerminalDialogueAsset()
        {
            string path = Path.Combine(NarrativeDataDir, "TerminalDomusEvidenceDialogue.asset");
            var asset = AssetDatabase.LoadAssetAtPath<DialogueSequenceSO>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<DialogueSequenceSO>();
                AssetDatabase.CreateAsset(asset, path);
            }

            // This setup owns the terminal text, so it always rewrites it to stay in sync with the scene.
            asset.lines = new List<DialogueLine>
                {
                    new DialogueLine
                    {
                        speakerName = "DANTE",
                        speakerColor = new Color(0.75f, 0.90f, 0.96f, 1f),
                        text = "Painéis estéreis atrás de uma estação de metrô... Isso nunca foi manutenção. Era um laboratório da Domus Corp."
                    },
                    new DialogueLine
                    {
                        speakerName = "TERMINAL DOMUS CORP // SETOR 04",
                        speakerColor = new Color(0.306f, 0.804f, 0.769f, 1f),
                        text = "[ARQUIVO CONFIDENCIAL #2090-B] O kernel de livre-arbítrio foi sabotado antes da distribuição em massa. " +
                               "A Diretoria Executiva ordenou o acobertamento total da falha."
                    },
                    new DialogueLine
                    {
                        speakerName = "TERMINAL DOMUS CORP // SETOR 04",
                        speakerColor = new Color(0.306f, 0.804f, 0.769f, 1f),
                        text = "[ADENDO] Desenvolvedor-chefe dado como desaparecido. Credenciais de acesso ao laboratório subterrâneo " +
                               "permaneceram ativas após a Queda."
                    },
                    new DialogueLine
                    {
                        speakerName = "DANTE",
                        speakerColor = new Color(0.75f, 0.90f, 0.96f, 1f),
                        text = "Não foi um acidente... A Domus Corp sabia. E meu pai pode estar vivo lá em cima. " +
                               "Vamos voltar para o abrigo, Kitty — Icarus precisa ver isso."
                    }
                };
            EditorUtility.SetDirty(asset);
            return asset;
        }

        private static void EnsureSceneInBuildSettings(string scenePath)
        {
            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            foreach (var s in scenes)
            {
                if (s.path == scenePath) return;
            }
            scenes.Add(new EditorBuildSettingsScene(scenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
