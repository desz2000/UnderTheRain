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
    /// Scaffolds Story 5.1 (Act I Sewers): narrative ScriptableObjects, Initial Shelter room
    /// with Domus Corp terminal, Icarus confrontation, sealed exit gate, and Sewer Passage room.
    /// </summary>
    public static class Act1SewersSetup
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
        private const string ScenePath = "Assets/_Project/Scenes/Zones/Act1_Sewers.unity";

        private const string GroundLayer = "Ground";
        private const string OneWayLayer = "OneWayPlatform";
        private const string PlayerHitboxLayer = "PlayerHitbox";
        private const string PlayerHurtboxLayer = "PlayerHurtbox";

        [MenuItem("UnderTheRain/Setup/Create Act 1 Sewers Scene")]
        public static void CreateAct1SewersScene()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogError("[UnderTheRain] Exit Play Mode before running Setup menu commands.");
                return;
            }

            Directory.CreateDirectory(NarrativeDataDir);

            // The Domus terminal document and dialogue are owned by PrologueRuinsSetup (the terminal lives there).
            var icarusBefore = EnsureDialogueAsset(
                "IcarusBeforeEvidenceDialogue.asset",
                new List<DialogueLine>
                {
                    Line("ICARUS", new Color(0.88f, 0.72f, 0.42f, 1f),
                        "Fique abaixado, Dante. Ouça a garoa lá fora — enquanto ficarmos nos esgotos, as patrulhas não nos acham."),
                    Line("DANTE", new Color(0.75f, 0.90f, 0.96f, 1f),
                        "Se nos escondermos para sempre, já perdemos. Vou investigar a velha estação de metrô antes de decidir qualquer coisa.")
                });

            var icarusConfrontation = EnsureDialogueAsset(
                "IcarusConfrontationDialogue.asset",
                new List<DialogueLine>
                {
                    Line("DANTE", new Color(0.75f, 0.90f, 0.96f, 1f),
                        "Icarus, olha estes registros da Domus Corp que achei num laboratório escondido atrás da estação de metrô. O colapso foi sabotado e acobertado. As credenciais do meu pai ainda estão ativas!"),
                    Line("ICARUS", new Color(0.88f, 0.72f, 0.42f, 1f),
                        "Você enlouqueceu?! Trazer esses dados para cá é assinar a sentença de morte deste abrigo!"),
                    Line("ICARUS", new Color(0.88f, 0.72f, 0.42f, 1f),
                        "Seu pai morreu na Queda, garoto. Se insistir nessa obsessão suicida, está fora da resistência. Suma daqui antes que atraia os batedores!"),
                    Line("DANTE", new Color(0.75f, 0.90f, 0.96f, 1f),
                        "Então eu vou sozinho. Vamos, Kitty — a superfície nos espera.")
                });

            var icarusAfter = EnsureDialogueAsset(
                "IcarusAfterConfrontationDialogue.asset",
                new List<DialogueLine>
                {
                    Line("ICARUS", new Color(0.88f, 0.72f, 0.42f, 1f),
                        "A comporta leste está destravada. Vá se matar lá em cima se quiser — mas não volte trazendo a colmeia atrás de você.")
                });

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
            cam.backgroundColor = new Color(0.043f, 0.055f, 0.071f, 1f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            camGo.transform.position = new Vector3(-4f, 1.5f, -10f);
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

            // Cat Companion
            var catPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(CatPrefabPath);
            if (catPrefab != null)
            {
                var catGo = (GameObject)PrefabUtility.InstantiatePrefab(catPrefab);
                catGo.name = "Kitty";
                catGo.transform.position = new Vector3(-9.8f, 0.6f, 0f);
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

            // Room 1: Initial Shelter (underground, dry & sheltered from the rain above)
            var room1 = CreateRoom("Room_01_InitialShelter", "sewers_initial_shelter", Vector2.zero, new Vector2(32f, 12f));
            BuildInitialShelterRoom(room1.transform, sprite, ground, reader, narrativeState, dialogueRunner,
                icarusBefore, icarusConfrontation, icarusAfter);

            // Room 2: Sewer Passage (first ascent corridor leading toward Story 5.2)
            var room2 = CreateRoom("Room_02_SewerPassage", "sewers_passage", new Vector2(44f, 0f), new Vector2(36f, 14f));
            BuildSewerPassageRoom(room2.transform, sprite, ground);

            // Wire Room Exits between Room 1 and Room 2 (+ west exit back to Prologue Ruins)
            var exitToPrologue = CreateRoomExit("Exit_ToPrologueRuins", room1.transform, new Vector2(-14.6f, 1.5f), new Vector2(1.2f, 3f));
            exitToPrologue.ConfigureScene("Prologue_Ruins", "prologue_hidden_lab", "from_sewers");

            var exitToPassage = CreateRoomExit("Exit_ToSewerPassage", room1.transform, new Vector2(15.0f, 1.5f), new Vector2(1.4f, 3f));
            exitToPassage.Configure(room2, "from_shelter");

            var exitToShelter = CreateRoomExit("Exit_ToInitialShelter", room2.transform, new Vector2(26.8f, 1.5f), new Vector2(1.4f, 3f));
            exitToShelter.Configure(room1, "from_passage");

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
            Debug.Log($"[UnderTheRain] Created Act 1 Sewers scene at {ScenePath}");
        }

        private static void BuildInitialShelterRoom(
            Transform parent,
            Sprite sprite,
            int groundLayer,
            GameInputReader reader,
            NarrativeState narrativeState,
            DialogueRunner dialogueRunner,
            DialogueSequenceSO icarusBefore,
            DialogueSequenceSO icarusConfrontation,
            DialogueSequenceSO icarusAfter)
        {
            CreateBlock("Floor", parent, sprite, groundLayer, new Vector2(0f, -1f), new Vector2(32f, 2f), new Color(0.16f, 0.19f, 0.22f));
            CreateBlock("Ceiling", parent, sprite, groundLayer, new Vector2(0f, 6.2f), new Vector2(32f, 1.4f), new Color(0.11f, 0.13f, 0.16f));
            CreateBlock("Wall_LeftTop", parent, sprite, groundLayer, new Vector2(-15.5f, 4.7f), new Vector2(1f, 4.4f), new Color(0.14f, 0.16f, 0.19f));
            CreateBlock("Wall_RightTop", parent, sprite, groundLayer, new Vector2(15.5f, 4.7f), new Vector2(1f, 4.4f), new Color(0.14f, 0.16f, 0.19f));

            // Elevated catwalk inside shelter for verticality
            CreateBlock("ShelterPlatform", parent, sprite, groundLayer, new Vector2(-2f, 2.1f), new Vector2(5.5f, 0.45f), new Color(0.24f, 0.28f, 0.34f));

            // Spawn Points
            CreateSpawnPoint("Spawn_Default", parent, "default", new Vector2(-9f, 1f));
            CreateSpawnPoint("Spawn_FromPassage", parent, "from_passage", new Vector2(13.2f, 1f));
            CreateSpawnPoint("Spawn_FromPrologue", parent, "from_prologue", new Vector2(-12.6f, 1f));

            // Shelter Rest Spot
            var restGo = new GameObject("ShelterRestSpot");
            restGo.transform.SetParent(parent, false);
            restGo.transform.position = new Vector3(-9f, 0.9f, 0f);
            restGo.transform.localScale = new Vector3(1.6f, 1.8f, 1f);
            var restSr = restGo.AddComponent<SpriteRenderer>();
            restSr.sprite = sprite;
            restSr.color = new Color(0.95f, 0.65f, 0.28f, 0.45f);
            restGo.AddComponent<BoxCollider2D>().size = Vector2.one;
            var restSpot = restGo.AddComponent<ShelterRestSpot>();
            var restSo = new SerializedObject(restSpot);
            restSo.FindProperty("input").objectReferenceValue = reader;
            restSo.FindProperty("isDefaultShelter").boolValue = true;
            restSo.ApplyModifiedPropertiesWithoutUndo();

            // Icarus NPC
            var icarusGo = new GameObject("Icarus_NPC");
            icarusGo.transform.SetParent(parent, false);
            icarusGo.transform.position = new Vector3(5.5f, 0.85f, 0f);
            icarusGo.transform.localScale = new Vector3(0.85f, 1.7f, 1f);
            var icarusSr = icarusGo.AddComponent<SpriteRenderer>();
            icarusSr.sprite = sprite;
            icarusSr.color = new Color(0.88f, 0.72f, 0.42f, 1f);
            var icarusCol = icarusGo.AddComponent<BoxCollider2D>();
            icarusCol.isTrigger = true;
            icarusCol.size = new Vector2(2.2f, 1.4f);
            var icarus = icarusGo.AddComponent<IcarusNpc>();
            icarus.Configure(reader, narrativeState, dialogueRunner, icarusBefore, icarusConfrontation, icarusAfter);

            // Sealed East Gate (opens after Icarus confrontation)
            var gateGo = CreateBlock("SewerExitGate", parent, sprite, groundLayer, new Vector2(12.2f, 1.4f), new Vector2(0.8f, 2.8f),
                new Color(0.75f, 0.25f, 0.25f, 1f));
            var gate = gateGo.AddComponent<NarrativeGate>();
            gate.Configure(narrativeState, NarrativeState.FlagSewerExitUnlocked);
        }

        private static void BuildSewerPassageRoom(Transform parent, Sprite sprite, int groundLayer)
        {
            CreateBlock("Floor", parent, sprite, groundLayer, new Vector2(44f, -1f), new Vector2(36f, 2f), new Color(0.16f, 0.19f, 0.22f));
            CreateBlock("Ceiling", parent, sprite, groundLayer, new Vector2(44f, 7.2f), new Vector2(36f, 1.4f), new Color(0.11f, 0.13f, 0.16f));
            CreateBlock("Wall_LeftTop", parent, sprite, groundLayer, new Vector2(26.5f, 5.1f), new Vector2(1f, 5.2f), new Color(0.14f, 0.16f, 0.19f));
            CreateBlock("Wall_Right", parent, sprite, groundLayer, new Vector2(61.5f, 3.2f), new Vector2(1f, 10f), new Color(0.14f, 0.16f, 0.19f));

            // Sewer drainage platforms leading east toward the future Wall Jump shaft (Story 5.2)
            CreateBlock("PassagePlatform_A", parent, sprite, groundLayer, new Vector2(35f, 1.6f), new Vector2(4.5f, 0.5f), new Color(0.24f, 0.28f, 0.34f));
            CreateBlock("PassagePlatform_B", parent, sprite, groundLayer, new Vector2(43f, 3.1f), new Vector2(5.0f, 0.5f), new Color(0.24f, 0.28f, 0.34f));
            CreateBlock("AscentLedge_Preview", parent, sprite, groundLayer, new Vector2(56f, 3.8f), new Vector2(6.0f, 0.6f), new Color(0.28f, 0.32f, 0.38f));

            CreateSpawnPoint("Spawn_FromShelter", parent, "from_shelter", new Vector2(28.5f, 1f));

            // Parts pickup reward on elevated platform
            var partsPrefab = AssetDatabase.LoadAssetAtPath<PartsPickup>(PartsPickupPrefabPath);
            if (partsPrefab != null)
            {
                var pickup = (PartsPickup)PrefabUtility.InstantiatePrefab(partsPrefab, parent);
                pickup.transform.position = new Vector3(43f, 3.8f, 0f);
            }

            // Scout robot patrolling the passage (affected by Kitty, deals contact damage, can be destroyed)
            EnemySetupUtility.CreateScoutRobot("SewerScoutRobot", parent, sprite, new Vector2(47f, 0.7f));
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
            player.transform.position = new Vector3(-9f, 1f, 0f);
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

            // Hurtbox
            var pHurtGo = new GameObject("PlayerHurtbox") { layer = pHurt };
            pHurtGo.transform.SetParent(player.transform, false);
            var pHurtCol = pHurtGo.AddComponent<BoxCollider2D>();
            pHurtCol.isTrigger = true;
            pHurtCol.size = Vector2.one;
            var pHurtbox = pHurtGo.AddComponent<Hurtbox>();
            var pHurtSo = new SerializedObject(pHurtbox);
            pHurtSo.FindProperty("damageableTarget").objectReferenceValue = health;
            pHurtSo.ApplyModifiedPropertiesWithoutUndo();

            // Melee
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

            // Ranged
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

        private static DialogueSequenceSO EnsureDialogueAsset(string fileName, List<DialogueLine> lines)
        {
            string path = Path.Combine(NarrativeDataDir, fileName);
            var asset = AssetDatabase.LoadAssetAtPath<DialogueSequenceSO>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<DialogueSequenceSO>();
                AssetDatabase.CreateAsset(asset, path);
            }
            asset.lines = lines;
            EditorUtility.SetDirty(asset);
            return asset;
        }

        private static DialogueLine Line(string speaker, Color color, string text)
        {
            return new DialogueLine
            {
                speakerName = speaker,
                speakerColor = color,
                text = text
            };
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
