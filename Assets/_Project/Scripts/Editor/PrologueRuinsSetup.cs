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
    /// Scaffolds the short Prologue / Tutorial zone in the rubble between the sewers and the surface:
    /// teaches movement, jump, Kitty's interference, and leads to the Domus Corp terminal discovery.
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

            // Room 1: Rubble Entry (Movement & Jump tutorial under the drizzle)
            var room1 = CreateRoom("Prologue_01_RubbleEntry", "prologue_rubble_entry", Vector2.zero, new Vector2(28f, 12f));
            BuildRubbleEntryRoom(room1.transform, sprite, ground);

            // Room 2: Collapsed Archive (Kitty interference + Domus Terminal + descent to Sewers)
            var room2 = CreateRoom("Prologue_02_CollapsedArchive", "prologue_collapsed_archive", new Vector2(38f, 0f), new Vector2(32f, 12f));
            BuildCollapsedArchiveRoom(room2.transform, sprite, ground, reader, narrativeState, dialogueRunner, doc, terminalDialogue);

            // Wire Room Exits
            var exitToArchive = CreateRoomExit("Exit_ToCollapsedArchive", room1.transform, new Vector2(13.2f, 1.5f), new Vector2(1.4f, 3f));
            exitToArchive.Configure(room2, "from_entry");

            var exitToEntry = CreateRoomExit("Exit_ToRubbleEntry", room2.transform, new Vector2(22.8f, 1.5f), new Vector2(1.4f, 3f));
            exitToEntry.Configure(room1, "from_archive");

            var exitToSewers = CreateRoomExit("Exit_ToSewersShelter", room2.transform, new Vector2(53.0f, 1.5f), new Vector2(1.4f, 3f));
            exitToSewers.ConfigureScene("Act1_Sewers");

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

        private static void BuildRubbleEntryRoom(Transform parent, Sprite sprite, int groundLayer)
        {
            CreateBlock("Floor", parent, sprite, groundLayer, new Vector2(0f, -1f), new Vector2(28f, 2f), new Color(0.18f, 0.21f, 0.25f));
            CreateBlock("Wall_Left", parent, sprite, groundLayer, new Vector2(-13.5f, 2.5f), new Vector2(1f, 9f), new Color(0.15f, 0.17f, 0.21f));
            CreateBlock("Wall_RightTop", parent, sprite, groundLayer, new Vector2(13.5f, 4.7f), new Vector2(1f, 4.4f), new Color(0.15f, 0.17f, 0.21f));

            // Rubble steps teaching variable jump
            CreateBlock("Rubble_Step1", parent, sprite, groundLayer, new Vector2(-3.5f, 0.8f), new Vector2(3.5f, 1.6f), new Color(0.24f, 0.27f, 0.32f));
            CreateBlock("Rubble_Step2", parent, sprite, groundLayer, new Vector2(2.5f, 2.0f), new Vector2(4.0f, 0.6f), new Color(0.28f, 0.32f, 0.38f));
            CreateBlock("Rubble_HighLedge", parent, sprite, groundLayer, new Vector2(7.5f, 3.4f), new Vector2(3.5f, 0.5f), new Color(0.28f, 0.32f, 0.38f));

            CreateSpawnPoint("Spawn_Default", parent, "default", new Vector2(-10f, 1f));
            CreateSpawnPoint("Spawn_FromArchive", parent, "from_archive", new Vector2(11.4f, 1f));

            CreateSignpost("Hint_MoveJump", parent, new Vector2(-8f, 0.5f), "[A/D] Mover   |   [Espaço] Pular (segure para pular mais alto)");

            var partsPrefab = AssetDatabase.LoadAssetAtPath<PartsPickup>(PartsPickupPrefabPath);
            if (partsPrefab != null)
            {
                var pickup = (PartsPickup)PrefabUtility.InstantiatePrefab(partsPrefab, parent);
                pickup.transform.position = new Vector3(7.5f, 4.1f, 0f);
            }

            CreateRainAmbience("PrologueDrizzle_Entry", parent, new Vector2(0f, 6.5f));
        }

        private static void BuildCollapsedArchiveRoom(
            Transform parent,
            Sprite sprite,
            int groundLayer,
            GameInputReader reader,
            NarrativeState narrativeState,
            DialogueRunner dialogueRunner,
            DocumentDataSO doc,
            DialogueSequenceSO terminalDialogue)
        {
            CreateBlock("Floor", parent, sprite, groundLayer, new Vector2(38f, -1f), new Vector2(32f, 2f), new Color(0.18f, 0.21f, 0.25f));
            CreateBlock("Wall_LeftTop", parent, sprite, groundLayer, new Vector2(22.5f, 4.7f), new Vector2(1f, 4.4f), new Color(0.15f, 0.17f, 0.21f));
            CreateBlock("Wall_RightTop", parent, sprite, groundLayer, new Vector2(53.5f, 4.7f), new Vector2(1f, 4.4f), new Color(0.15f, 0.17f, 0.21f));

            // Collapsed archive ledges
            CreateBlock("Archive_Ledge", parent, sprite, groundLayer, new Vector2(43f, 2.0f), new Vector2(6.0f, 0.55f), new Color(0.26f, 0.30f, 0.36f));

            CreateSpawnPoint("Spawn_FromEntry", parent, "from_entry", new Vector2(24.5f, 1f));

            CreateSignpost("Hint_KittyAndCombat", parent, new Vector2(29f, 0.5f), "[R] Kitty: Interferir no robô   |   [Clique Esq.] Bastão   |   [Q] Kit");

            // Scout robot guarding the ruins (affected by Kitty)
            var robotGo = new GameObject("RuinsScoutRobot");
            robotGo.transform.SetParent(parent, false);
            robotGo.transform.position = new Vector3(34.5f, 0.7f, 0f);
            robotGo.transform.localScale = new Vector3(0.9f, 1.4f, 1f);
            var robotSr = robotGo.AddComponent<SpriteRenderer>();
            robotSr.sprite = sprite;
            robotSr.color = new Color(0.45f, 0.75f, 0.95f, 1f);
            robotGo.AddComponent<BoxCollider2D>().size = Vector2.one;
            var target = robotGo.AddComponent<PatrolInterferenceTarget>();
            var tSo = new SerializedObject(target);
            tSo.FindProperty("faction").enumValueIndex = (int)EnemyFaction.CollectiveMachine;
            tSo.FindProperty("spriteRenderer").objectReferenceValue = robotSr;
            tSo.ApplyModifiedPropertiesWithoutUndo();

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

            CreateSignpost("Hint_ReturnToSewers", parent, new Vector2(50.5f, 0.5f), "Descer para o Abrigo nos Esgotos →");

            CreateRainAmbience("PrologueDrizzle_Archive", parent, new Vector2(38f, 6.5f));
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

        private static void CreateRainAmbience(string name, Transform parent, Vector2 worldPos)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = worldPos;
            go.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            go.AddComponent<SewerRainAmbience>();
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
                asset.lines = new List<DialogueLine>
                {
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
            }
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
