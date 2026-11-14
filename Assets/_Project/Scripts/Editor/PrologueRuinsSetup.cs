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

        private const string ZoneArtDir = "Assets/_Project/Art/Tilesets/Prologue";
        private const string GroundLayer = "Ground";
        private const string OneWayLayer = "OneWayPlatform";
        private const string PlayerHitboxLayer = "PlayerHitbox";
        private const string PlayerHurtboxLayer = "PlayerHurtbox";

        // Room origins (local floor line at y = 0). Rooms do not overlap; P6 extends downward.
        // Exit triggers: narrow and flush with the room edge (past the wall face), so a transition only
        // fires once Dante actually walks into the doorway. Spawns stay 2.6 inside (>= 1.6 clear of the trigger).
        private const float ExitInset = 0.3f;
        private const float ExitWidth = 0.6f;
        // Doorways are 2.5 tall (wall tops start at y 2.5), so the trigger covers exactly the doorway.
        private const float DoorHeight = 2.5f;
        private const float ExitCenterY = DoorHeight * 0.5f;
        private static readonly Vector2 ExitSize = new Vector2(ExitWidth, DoorHeight);

        private static readonly Vector2 P1Origin = new Vector2(0f, 0f);
        private static readonly Vector2 P2Origin = new Vector2(56f, 0f);
        private static readonly Vector2 P3Origin = new Vector2(118f, 0f);
        private static readonly Vector2 P4Origin = new Vector2(174f, 0f);
        private static readonly Vector2 P5Origin = new Vector2(232f, 0f);
        private static readonly Vector2 P6Origin = new Vector2(282f, 0f);
        // Archive sits above P6 (no overlap); the tunnel runs under P1–P4 (below their bounds).
        private static readonly Vector2 ArchiveOrigin = new Vector2(280f, 12f);
        private static readonly Vector2 TunnelOrigin = new Vector2(88f, -13f);

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
            // Zone art: every texture comes from one colour ramp, so backdrop, masses and edges belong together.
            ZoneArtKit.EnsureKit(ZoneArtDir, ZoneArtKit.ProloguePalette);
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
            cam.backgroundColor = new Color(0.024f, 0.043f, 0.055f, 1f);
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
                catGo.transform.position = new Vector3(-21f, 0.6f, 0f);
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

            // ── Story 5.6: six large tutorial rooms + optional Archive (P5a) + one-way Service Tunnel. ──
            // Every room is built in local coordinates (origin = main floor line, y = 0).
            s_sprite = sprite;
            s_ground = ground;
            s_oneWay = oneWay;
            s_reader = reader;
            s_state = narrativeState;
            s_runner = dialogueRunner;
            var fatherDoc = EnsureDocument("FatherSketch01.asset", "doc_father_sketch_01", "Rabisco técnico (à mão)", "—",
                "Esquema de um circuito de isolamento desenhado à mão, cheio de setas e correções apressadas.");
            var fatherDialogue = EnsureDialogue("FatherSketchDialogue.asset", new List<DialogueLine>
            {
                new DialogueLine { speakerName = "RABISCO À MÃO", speakerColor = new Color(0.95f, 0.78f, 0.45f, 1f),
                    text = "Um papel dobrado, preso na grade: o esquema de um circuito de isolamento, cheio de setas e correções apressadas." },
                new DialogueLine { speakerName = "DANTE", speakerColor = new Color(0.75f, 0.90f, 0.96f, 1f),
                    text = "Essa letra... é do meu pai. Ele esteve aqui." }
            });

            var p1 = Room("Prologue_P1_MetroPlatform", "prologue_metro_platform", P1Origin, -24f, -4f, 24f, 12f);
            var p2 = Room("Prologue_P2_Gap", "prologue_gap", P2Origin, -28f, -4f, 28f, 12f);
            var p3 = Room("Prologue_P3_Debris", "prologue_debris", P3Origin, -30f, -4f, 30f, 14f);
            var p4 = Room("Prologue_P4_Duct", "prologue_duct", P4Origin, -22f, -4f, 22f, 16f);
            var p5 = Room("Prologue_P5_HiddenLab", "prologue_hidden_lab", P5Origin, -32f, -4f, 32f, 18f);
            var p5a = Room("Prologue_P5a_LabArchive", "prologue_lab_archive", ArchiveOrigin, -12f, -1f, 12f, 13f);
            var p6 = Room("Prologue_P6_Descent", "prologue_descent", P6Origin, -14f, -38f, 14f, 6f);
            var tunnel = Room("Prologue_T_ServiceTunnel", "prologue_service_tunnel", TunnelOrigin, -72f, -1f, 72f, 7f);

            BuildP1(p1.transform, P1Origin);
            BuildP2(p2.transform, P2Origin);
            BuildP3(p3.transform, P3Origin);
            BuildP4(p4.transform, P4Origin, fatherDoc, fatherDialogue);
            BuildP5(p5.transform, P5Origin, doc, terminalDialogue);
            BuildArchive(p5a.transform, ArchiveOrigin);
            BuildP6(p6.transform, P6Origin);
            BuildTunnel(tunnel.transform, TunnelOrigin);

            // Doors (low = floor y 0; high doors sit on mezzanines/walkways).
            LinkDoor("p1p2_low", p1, P1Origin, 24f, 0f, p2, P2Origin, -28f, 0f);
            LinkDoor("p1p2_high", p1, P1Origin, 24f, 5f, p2, P2Origin, -28f, 5f);
            LinkDoor("p2p3_low", p2, P2Origin, 28f, 0f, p3, P3Origin, -30f, 0f);
            LinkDoor("p2p3_high", p2, P2Origin, 28f, 5f, p3, P3Origin, -30f, 8f);
            LinkDoor("p3p4", p3, P3Origin, 30f, 0f, p4, P4Origin, -22f, 0f);
            LinkDoor("p4p5", p4, P4Origin, 22f, 7.2f, p5, P5Origin, -32f, 7.2f);
            LinkDoor("p5p6", p5, P5Origin, 32f, 0f, p6, P6Origin, -14f, 0f);
            LinkDoor("p5archive", p5, P5Origin, 32f, 12.8f, p5a, ArchiveOrigin, -12f, 0f);

            // One-way shortcut: P4 hatch (opened by Kitty's panel) → tunnel → P1. P1's hatch never opens from above.
            var hatchExit = CreateRoomExit("Hatch_ToTunnel", p4.transform, P4Origin + new Vector2(-15f, -3.7f), new Vector2(2f, 0.6f));
            hatchExit.Configure(tunnel, "from_p4");
            // Safety floor under the hatch: if a transition is busy, Dante lands on the trigger and it retries
            // instead of falling out of the world.
            Rect("Hatch_SafetyFloor", p4.transform, P4Origin, -16f, -4.5f, -14f, -4f, FloorColor);
            var hatchSo = new SerializedObject(hatchExit);
            hatchSo.FindProperty("retryWhileInside").boolValue = true;
            hatchSo.ApplyModifiedPropertiesWithoutUndo();
            CreateSpawnPoint("Spawn_from_p4", tunnel.transform, "from_p4", TunnelOrigin + new Vector2(69f, 1f));
            Exit("Exit_TunnelToP1", tunnel, TunnelOrigin, -72f, -1, 0f).Configure(p1, "from_tunnel");
            CreateSpawnPoint("Spawn_from_tunnel", p1.transform, "from_tunnel", P1Origin + new Vector2(15.5f, 1f));

            // P6 bottom → Sewers shelter (Act 1).
            Exit("Exit_ToSewersShelter", p6, P6Origin, 14f, +1, -33.6f).ConfigureScene("Act1_Sewers", "sewers_initial_shelter", "from_prologue");
            CreateSpawnPoint("Spawn_from_sewers", p6.transform, "from_sewers", P6Origin + new Vector2(14f - 2.6f, -32.6f));

            // Wire Camera & TransitionController
            var camSo = new SerializedObject(roomCam);
            camSo.FindProperty("target").objectReferenceValue = player.transform;
            camSo.FindProperty("initialRoom").objectReferenceValue = p1;
            camSo.ApplyModifiedPropertiesWithoutUndo();

            var trSo = new SerializedObject(transitionCtrl);
            trSo.FindProperty("screenFader").objectReferenceValue = fader;
            trSo.FindProperty("roomCamera").objectReferenceValue = roomCam;
            trSo.FindProperty("initialRoom").objectReferenceValue = p1;
            trSo.ApplyModifiedPropertiesWithoutUndo();

            // Dress the solids only now: caps and edges depend on which neighbours cover them.
            ZoneArtKit.SkinSolids();

            foreach (var room in new[] { p2, p3, p4, p5, p5a, p6, tunnel })
            {
                room.gameObject.SetActive(false);
            }

            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
            EditorSceneManager.SaveScene(scene, ScenePath);
            EnsureSceneInBuildSettings(ScenePath);
            AssetDatabase.SaveAssets();
            Debug.Log($"[UnderTheRain] Created Prologue Ruins scene at {ScenePath}");
        }

        // ───────────────────────────── build context & palette ─────────────────────────────
        private static Sprite s_sprite;
        private static int s_ground;
        private static int s_oneWay;
        private static GameInputReader s_reader;
        private static NarrativeState s_state;
        private static DialogueRunner s_runner;

        private static readonly Color FloorColor = new Color(0.17f, 0.18f, 0.21f);
        private static readonly Color WallColor = new Color(0.13f, 0.14f, 0.17f);
        private static readonly Color CeilColor = new Color(0.10f, 0.11f, 0.13f);
        private static readonly Color LedgeColor = new Color(0.28f, 0.30f, 0.35f);
        private static readonly Color PitColor = new Color(0.12f, 0.13f, 0.15f);
        private static readonly Color GraffitiColor = new Color(0.78f, 0.22f, 0.20f, 0.85f);
        private static readonly Color CyanLight = new Color(0.306f, 0.804f, 0.769f, 0.8f);
        private static readonly Color WarmLight = new Color(0.98f, 0.80f, 0.35f, 0.9f);

        // ───────────────────────────── P1 — metro platform (48×16) ─────────────────────────────
        // Two floors from the start: platform + overturned wagon over a safe track bed, escalator to a
        // mezzanine (graffiti + parts), a 4.0 gap to the ticket booth (secret battery cell) and the high door.
        private static void BuildP1(Transform r, Vector2 o)
        {
            Rect("Ceiling", r, o, -24f, 11f, 24f, 12f, CeilColor);
            Rect("Wall_Left", r, o, -24f, -4f, -23f, 11f, WallColor);
            WallWithDoors("Wall_Right", r, o, 23f, 24f, 0f, 11f, 0f, 5f);

            Rect("Platform_Left", r, o, -23f, -4f, -4f, 0f, FloorColor);
            Rect("Track_Bed", r, o, -4f, -4f, 11f, -2.5f, PitColor);
            Rect("Platform_Right", r, o, 11f, -4f, 24f, 0f, FloorColor);
            Rect("Track_Crate", r, o, -3.5f, -2.5f, -2f, -1f, LedgeColor);
            Rect("Overturned_Wagon", r, o, -1f, -2.5f, 8f, 1f, new Color(0.30f, 0.24f, 0.20f));
            Rect("Escalator_Step1", r, o, -14f, 0f, -12f, 1.4f, LedgeColor);
            Rect("Escalator_Step2", r, o, -12f, 0f, -10f, 2.8f, LedgeColor);
            Rect("Mezzanine", r, o, -10f, 4.5f, 6f, 5f, LedgeColor);
            Rect("TicketBooth_Platform", r, o, 10f, 4.5f, 23f, 5f, LedgeColor);

            DecorRect("Rails", r, o, -4f, -2.55f, 11f, -2.45f, new Color(0.32f, 0.30f, 0.27f));
            DecorRect("StationSignStrip", r, o, -21f, 8.5f, -14f, 9.2f, new Color(0.55f, 0.20f, 0.18f, 0.85f));
            Graffiti("Graffiti_Mezzanine", r, o, -6f, 6.2f);

            CreateSpawnPoint("Spawn_Default", r, "default", o + new Vector2(-20f, 1f));
            CreateSignpost("Hint_MoveJump", r, o + new Vector2(-19f, 0.5f), "[A/D] Mover   |   [Espaço] Pular");
            CreateSignpost("Sign_Station", r, o + new Vector2(-7f, 0.5f), "ESTAÇÃO VALE-CINZA — LINHA 7 (DESATIVADA)");
            CreatePartsPickup(r, o + new Vector2(0f, 5.6f));

            // Secret: the ticket booth front is decor without collision; a flickering light behind cracked glass hints it.
            var booth = DecorRect("TicketBooth_Front", r, o, 14f, 5f, 18.5f, 8.5f, new Color(0.22f, 0.24f, 0.27f), 5);
            DecorRect("TicketBooth_Flicker", r, o, 15.6f, 7.3f, 16.1f, 7.8f, WarmLight, 6);
            CreateBatteryCell(r, o + new Vector2(17.2f, 5.5f));
            SecretWall("Secret_TicketBooth", r, o, 14f, 5f, 18.5f, 7.5f, "p1_ticket_booth", booth);

            // Service hatch: visibly locked from this side (the tunnel opens only from P4).
            DecorRect("ServiceHatch_Locked", r, o, 17f, -0.15f, 20f, 0.05f, new Color(0.40f, 0.36f, 0.30f), 2);
            CreateSignpost("Sign_Hatch", r, o + new Vector2(18.5f, 0.5f), "Escotilha de serviço — trancada, sem energia");
        }

        // ───────────────────────────── P2 — gap (56×16) ─────────────────────────────
        // No damage. Teach (3.0) → test (4.0 / 4.5 / 5.0, narrowing pillars) → twist (+2 ledges that punish
        // over-holding). Optional cable walkway (5.3 / 5.5 precision gaps) leads to P3's high door.
        private static void BuildP2(Transform r, Vector2 o)
        {
            Rect("Ceiling", r, o, -28f, 11f, 28f, 12f, CeilColor);
            WallWithDoors("Wall_Left", r, o, -28f, -27f, 0f, 11f, 0f, 5f);
            WallWithDoors("Wall_Right", r, o, 27f, 28f, 0f, 11f, 0f, 5f);
            Rect("Wall_Right_Base", r, o, 27f, -4f, 28f, 0f, WallColor);

            Rect("Floor_Start", r, o, -28f, -4f, -16f, 0f, FloorColor);
            Rect("Trench_Bottom", r, o, -16f, -4f, 10f, -2.5f, PitColor);
            Rect("Pillar_A", r, o, -13f, -4f, -9f, 0f, FloorColor);
            Rect("Pillar_B", r, o, -5f, -4f, -2f, 0f, FloorColor);
            Rect("Pillar_C", r, o, 2.5f, -4f, 5f, 0f, FloorColor);
            Rect("Floor_Right_Top", r, o, 10f, -0.5f, 27f, 0f, FloorColor);
            Rect("Floor_Right_Under", r, o, 10f, -4f, 27f, -2.5f, PitColor);
            DecorRect("Trench_Rails", r, o, -16f, -2.55f, 10f, -2.45f, new Color(0.32f, 0.30f, 0.27f));

            Rect("Ledge_1", r, o, 14f, 1.5f, 16f, 2f, LedgeColor);
            Rect("Ledge_2", r, o, 19.5f, 3.5f, 21.5f, 4f, LedgeColor);
            CreateSignpost("Sign_Line7", r, o + new Vector2(20.5f, 4.5f), "LINHA 7 INTERDITADA POR ORDEM DA DOMUS. ACESSO RESTRITO AO SUBNÍVEL.");

            Rect("CableWalk_A", r, o, -27f, 4.5f, -18f, 5f, LedgeColor);
            Rect("CableWalk_B", r, o, -12.7f, 4.5f, -6.7f, 5f, LedgeColor);
            Rect("CableWalk_C", r, o, -1.2f, 4.5f, 12f, 5f, LedgeColor);
            Rect("CableWalk_D", r, o, 22f, 4.5f, 27f, 5f, LedgeColor);
            CreatePartsPickup(r, o + new Vector2(6f, 5.6f));

            CreateSignpost("Hint_HoldJump", r, o + new Vector2(-21f, 0.5f), "Segure [Espaço] para pular mais longe");

            // Secret pocket under the right floor: a lit grate hints it from above; the trench wall is fake.
            DecorRect("Grate_Light", r, o, 23f, -0.55f, 25f, 0.02f, WarmLight, 6);
            Graffiti("Graffiti_Pocket", r, o, 24.2f, -1.9f);
            CreatePartsPickup(r, o + new Vector2(25.8f, -1.8f));
            var pocketCover = DecorRect("Pocket_FakeWall", r, o, 10f, -2.5f, 27f, -0.5f, FloorColor, 5);
            SecretWall("Secret_TrenchPocket", r, o, 10f, -2.5f, 12f, -0.5f, "p2_trench_pocket", pocketCover);
        }

        // ───────────────────────────── P3 — debris (60×18) ─────────────────────────────
        // Baton on flat ground → two Workers on two heights → battery cell + shot target out of baton reach →
        // Worker on a narrow bridge over a safe pit. Optional upper office skips the bridge. Rest point at the end.
        private static void BuildP3(Transform r, Vector2 o)
        {
            Rect("Ceiling", r, o, -30f, 13f, 30f, 14f, CeilColor);
            WallWithDoors("Wall_Left", r, o, -30f, -29f, 0f, 13f, 0f, 8f);
            WallWithDoors("Wall_Right", r, o, 29f, 30f, 0f, 13f, 0f);

            Rect("Floor_Main", r, o, -30f, -4f, 15f, 0f, FloorColor);
            Rect("Pit_Bottom", r, o, 15f, -4f, 25f, -2.5f, PitColor);
            Rect("Bridge", r, o, 17f, -0.4f, 23f, 0f, LedgeColor);
            Rect("Floor_Right", r, o, 25f, -4f, 30f, 0f, FloorColor);
            Rect("Debris_Step1", r, o, -9f, 0f, -7f, 1.4f, LedgeColor);
            Rect("Debris_Step2", r, o, -7f, 0f, -5f, 2.8f, LedgeColor);
            Rect("Slab", r, o, -2f, 2.3f, 4f, 2.8f, LedgeColor);
            Rect("Target_Ledge", r, o, 9f, 5f, 12f, 5.5f, LedgeColor);

            // Upper office route (reached from P2's walkway door or by the one-way stack).
            Rect("Office_Floor_L", r, o, -29f, 7.5f, -13f, 8f, LedgeColor);
            OneWay("Office_Step_1", r, o, -12f, -8f, 2.8f);
            OneWay("Office_Step_2", r, o, -12f, -8f, 5.6f);
            OneWay("Office_Step_3", r, o, -12f, -8f, 8f);
            Rect("Office_Floor_R", r, o, -7f, 7.5f, 25f, 8f, LedgeColor);
            Rect("Office_Floor_Vault", r, o, 27f, 7.5f, 29f, 8f, LedgeColor);
            Graffiti("Graffiti_Office", r, o, -2f, 9f);
            CreateBatteryCell(r, o + new Vector2(0f, 8.5f));
            DecorRect("LabGlow_ThroughCracks", r, o, 18f, 10f, 24f, 10.2f, CyanLight);
            DecorRect("ThickCable", r, o, 20f, 9.5f, 27f, 9.7f, new Color(0.10f, 0.10f, 0.10f), 6);

            // Secret vault behind the fake office wall, hinted by the thick cable running into it.
            CreatePartsPickup(r, o + new Vector2(28f, 8.6f));
            var vaultCover = DecorRect("Vault_FakeWall", r, o, 26.5f, 8f, 29f, 10.5f, WallColor, 5);
            SecretWall("Secret_OfficeVault", r, o, 27f, 8f, 29f, 10.5f, "p3_office_vault", vaultCover);

            EnemySetupUtility.CreateWorkerRobot("Worker_1_Flat", r, s_sprite, o + new Vector2(-20f, 0f));
            EnemySetupUtility.CreateWorkerRobot("Worker_2_Slab", r, s_sprite, o + new Vector2(1f, 2.8f));
            EnemySetupUtility.CreateWorkerRobot("Worker_3_UnderSlab", r, s_sprite, o + new Vector2(1f, 0f));
            EnemySetupUtility.CreateWorkerRobot("Worker_4_Bridge", r, s_sprite, o + new Vector2(20f, 0f));
            // Feet at 5.5: jump apex (3.2) + baton hitbox (~1.0) cannot reach it, so the shot is the tool.
            EnemySetupUtility.CreateDormantScout("DormantScoutTarget", r, s_sprite, o + new Vector2(10.5f, 5.5f));
            // First drone, right after the shot lesson: it comes for Dante, so the shot (or a timed baton hit) is tested.
            EnemySetupUtility.CreateDrone("Drone_ShotLesson", r, s_sprite, o + new Vector2(14f, 3.6f));

            CreateBatteryCell(r, o + new Vector2(6f, 0.5f));
            CreateSignpost("Hint_Baton", r, o + new Vector2(-25f, 0.5f), "[Clique Esq.] Bastão — o 3º golpe é mais forte");
            CreateSignpost("Hint_Shoot", r, o + new Vector2(7.5f, 0.5f), "[Clique Dir.] Disparo — gasta bateria");

            // Station emergency generator: rest point (restores and becomes the respawn point).
            CreateRestSpot(r, o + new Vector2(26.5f, 0.9f));
            CreateSignpost("Hint_Rest", r, o + new Vector2(25.5f, 0.5f), "Gerador de emergência da estação — [E] Descansar");
        }

        // ───────────────────────────── P4 — Kitty's ducts (44×20) ─────────────────────────────
        // Quiet room after P3's fights: duct 1 opens the jammed door, duct 2 drops the grate (Dante must carry
        // Kitty up the crates first), the offline panel opens the armored door to P5 AND the tunnel hatch.
        // Optional duct 3 opens a locker. The chimney (Wall Jump) is the prologue's visible return gate.
        private static void BuildP4(Transform r, Vector2 o, DocumentDataSO fatherDoc, DialogueSequenceSO fatherDialogue)
        {
            Rect("Ceiling", r, o, -22f, 15f, 22f, 16f, CeilColor);
            WallWithDoors("Wall_Left", r, o, -22f, -21f, 0f, 15f, 0f);
            WallWithDoors("Wall_Right", r, o, 21f, 22f, 0f, 15f, 7.2f);

            Rect("Floor_West", r, o, -22f, -4f, -16f, 0f, FloorColor);
            Rect("Floor_East", r, o, -14f, -4f, 22f, 0f, FloorColor);
            var hatchCover = Rect("Hatch_Cover", r, o, -16f, -0.5f, -14f, 0f, new Color(0.40f, 0.36f, 0.30f));

            var jammedDoor = Rect("JammedMaintenanceDoor", r, o, -6.4f, 0f, -5.6f, 15f, new Color(0.55f, 0.32f, 0.20f));
            Rect("Crate_A", r, o, -5.6f, 0f, -3f, 2.4f, LedgeColor);
            Rect("Platform_B", r, o, -1f, 4.3f, 5f, 4.8f, LedgeColor);
            var grate = Rect("Fallen_Grate", r, o, 5.2f, 4.8f, 5.8f, 15f, new Color(0.35f, 0.33f, 0.30f));
            Rect("Service_Corridor", r, o, 6f, 6.7f, 21f, 7.2f, LedgeColor);
            var armoredDoor = Rect("Armored_Door", r, o, 20.4f, 7.2f, 21f, 9.7f, new Color(0.40f, 0.45f, 0.50f));

            // Locker under the corridor (duct 3).
            var lockerDoor = Rect("Locker_Door", r, o, 10f, 0f, 10.5f, 2.5f, new Color(0.45f, 0.40f, 0.32f));
            Rect("Locker_Roof", r, o, 10f, 2.5f, 14f, 3f, LedgeColor);
            Rect("Locker_Back", r, o, 13.5f, 0f, 14f, 2.5f, LedgeColor);
            CreateBatteryCell(r, o + new Vector2(11.8f, 0.5f));
            CreatePartsPickup(r, o + new Vector2(12.9f, 0.6f));

            // Chimney (3 wide, walls start 2.5 above the corridor so you can walk under them).
            Rect("Chimney_Wall_L", r, o, 8.5f, 9.7f, 9f, 13f, WallColor);
            Rect("Chimney_Wall_R", r, o, 12f, 9.7f, 12.5f, 12.5f, WallColor);
            Rect("Chimney_Ledge", r, o, 12.5f, 12f, 15f, 12.5f, LedgeColor);
            DecorRect("Chimney_Glow", r, o, 13f, 13f, 14.6f, 13.6f, WarmLight, 2);
            CreateTerminal("FatherSketchTerminal", r, o + new Vector2(14f, 13.2f), fatherDoc, fatherDialogue,
                NarrativeState.FlagFatherSketchFound, "[E] Examinar", WarmLight);

            KittyPoint("MaintenanceVent", r, CatExplorationType.Duct, o + new Vector2(-8f, 0.5f),
                o + new Vector2(-7f, 0.35f), o + new Vector2(-4.9f, 0.35f), "prologue_maintenance_door", jammedDoor);
            KittyPoint("GrateVent", r, CatExplorationType.Duct, o + new Vector2(3f, 5.3f),
                o + new Vector2(4.2f, 5.15f), o + new Vector2(6.6f, 7.55f), "prologue_p4_grate", grate);
            KittyPoint("OfflinePanel", r, CatExplorationType.Terminal, o + new Vector2(17f, 7.7f),
                o + new Vector2(16.5f, 7.55f), o + new Vector2(18.2f, 8.4f), "prologue_p4_panel", armoredDoor, hatchCover);
            KittyPoint("LockerVent", r, CatExplorationType.Duct, o + new Vector2(8f, 0.5f),
                o + new Vector2(9.4f, 0.35f), o + new Vector2(11.5f, 0.35f), "prologue_p4_locker", lockerDoor);

            CreateSignpost("Hint_KittyDuct", r, o + new Vector2(-10f, 0.5f), "Porta emperrada. [E] Kitty: entrar pelo duto");
            CreateSignpost("Hint_KittyPanel", r, o + new Vector2(15f, 7.7f), "Painel offline. [E] Kitty: tentar ligar");
        }

        // ───────────────────────────── P5 — hidden Domus lab (64×22) ─────────────────────────────
        // Tight corridor (1 scout, teaches R) → observation deck (clue, cell, war-prototype carcass) → drop through
        // the broken floor into the test chamber ARENA (2 scouts + 1 worker, dormant until you land; exits lock
        // until they fall) → server room with the Domus terminal → P6. Walkway + Kitty panel → Archive (P5a).
        private static void BuildP5(Transform r, Vector2 o, DocumentDataSO doc, DialogueSequenceSO terminalDialogue)
        {
            Rect("Ceiling", r, o, -32f, 17f, 32f, 18f, CeilColor);
            WallWithDoors("Wall_Left", r, o, -32f, -31f, 0f, 17f, 7.2f);
            WallWithDoors("Wall_Right", r, o, 31f, 32f, 0f, 17f, 0f, 12.8f);
            DecorRect("LabLightStrip", r, o, -10f, 16.3f, 31f, 16.5f, CyanLight);

            Rect("Floor_Base", r, o, -32f, -4f, 32f, 0f, FloorColor);
            Rect("UnderCorridor_Fill", r, o, -31f, 0f, -18f, 6.2f, WallColor);
            Rect("Corridor_Floor", r, o, -31f, 6.2f, -18f, 7.2f, FloorColor);
            Rect("Corridor_LowCeiling", r, o, -31f, 10.7f, -18f, 11.2f, CeilColor);
            Rect("UnderDeck_Fill", r, o, -18f, 0f, -13f, 6.7f, WallColor);
            Rect("Niche_Roof", r, o, -13f, 2.5f, -11f, 6.7f, WallColor);
            Rect("Arena_LeftWall", r, o, -11f, 2.5f, -10f, 6.7f, WallColor);
            Rect("Deck_A", r, o, -18f, 6.7f, 15f, 7.2f, LedgeColor);
            Rect("Deck_B", r, o, 18f, 6.7f, 31f, 7.2f, LedgeColor);

            // Arena (test chamber)
            OneWay("Arena_Platform_L", r, o, -6f, -2f, 2.8f);
            OneWay("Arena_Platform_R", r, o, 8f, 12f, 2.8f);
            OneWay("Arena_Ladder_1", r, o, 15f, 18.5f, 2.8f);
            OneWay("Arena_Ladder_2", r, o, 15f, 18.5f, 4.8f);
            DecorRect("WarPrototype_Carcass", r, o, 2f, 1.5f, 6f, 6.2f, new Color(0.32f, 0.30f, 0.33f), -5);
            DecorRect("Crane_Cable", r, o, 3.9f, 6.2f, 4.1f, 17f, new Color(0.08f, 0.08f, 0.08f), -4);

            // Server room
            Rect("Server_Step", r, o, 22f, 0f, 24f, 2.4f, LedgeColor);
            Rect("Server_Platform", r, o, 24f, 4.3f, 29f, 4.8f, LedgeColor);
            CreateTerminal("DomusTerminal", r, o + new Vector2(27f, 5.5f), doc, terminalDialogue,
                NarrativeState.FlagDomusEvidenceFound, "[E] Acessar Terminal", new Color(0.306f, 0.804f, 0.769f, 1f));

            // Upper walkway to the Archive (panel opened by Kitty).
            OneWay("Walkway_Step", r, o, 0f, 4f, 10f);
            Rect("Upper_Walkway", r, o, 6f, 12.3f, 31f, 12.8f, LedgeColor);
            var archiveDoor = Rect("Archive_Door", r, o, 30.4f, 12.8f, 31f, 15.3f, new Color(0.40f, 0.45f, 0.50f));
            KittyPoint("ArchivePanel", r, CatExplorationType.Terminal, o + new Vector2(26f, 13.3f),
                o + new Vector2(25.5f, 13.15f), o + new Vector2(29.6f, 13.15f), "prologue_p5_archive_panel", archiveDoor);
            CreateSignpost("Hint_ArchivePanel", r, o + new Vector2(23.5f, 13.3f), "Painel do arquivo. [E] Kitty");

            // Corridor scout (teaches R in a tight space).
            EnemySetupUtility.CreateScoutRobot("Scout_Corridor", r, s_sprite, o + new Vector2(-24f, 7.2f));
            CreateSignpost("Hint_KittyInterference", r, o + new Vector2(-28f, 7.7f), "[R] Kitty: interferir no robô (gasta bateria)");
            CreateSignpost("Clue_LabReveal", r, o + new Vector2(-5f, 7.7f), "Isto não é manutenção do metrô... é equipamento da Domus Corp.");
            CreateBatteryCell(r, o + new Vector2(8f, 7.7f));
            CreateSignpost("Hint_Medkit", r, o + new Vector2(14f, 0.5f), "[Q] Kit médico");

            // Arena encounter: robots dormant until Dante drops in; shutter + gate lock until all fall.
            var a1 = EnemySetupUtility.CreateRobot("Arena_Scout_A", r, s_sprite, o + new Vector2(-4f, 0f), EnemySetupUtility.Scout.AsDormant(6f));
            var a2 = EnemySetupUtility.CreateRobot("Arena_Scout_B", r, s_sprite, o + new Vector2(12f, 0f), EnemySetupUtility.Scout.AsDormant(6f));
            var a3 = EnemySetupUtility.CreateRobot("Arena_Worker", r, s_sprite, o + new Vector2(4f, 0f), EnemySetupUtility.Worker.AsDormant(6f));
            var shutter = Rect("Arena_Shutter", r, o, 15f, 6.7f, 18f, 7.2f, new Color(0.55f, 0.25f, 0.22f));
            var gate = Rect("Arena_Gate", r, o, 19.4f, 0f, 20f, 6.7f, new Color(0.55f, 0.25f, 0.22f));
            shutter.SetActive(false);
            gate.SetActive(false);
            // Zone starts at -13 so the crane niche (a secret reached only from the arena) counts as inside.
            CreateArena("Arena_TestChamber", r, o, -13f, 0f, 19.4f, 6.5f, "prologue_p5_arena",
                new[] { shutter, gate }, new[] { a1, a2, a3 });

            // Secret: the crane cable runs into the arena's left wall, which is fake at floor level.
            DecorRect("Crane_Cable_Niche", r, o, -10.6f, 2.5f, -10.4f, 17f, new Color(0.08f, 0.08f, 0.08f), 6);
            CreateBatteryCell(r, o + new Vector2(-12f, 0.5f));
            // The fake wall covers the whole niche (and its cell), like P6's shaft niche.
            var nicheCover = DecorRect("Niche_FakeWall", r, o, -13f, 0f, -10f, 2.5f, WallColor, 5);
            SecretWall("Secret_CraneNiche", r, o, -11f, 0f, -10.2f, 2.5f, "p5_crane_niche", nicheCover);
        }

        // ───────────────────────────── P5a — lab archive (24×14, optional) ─────────────────────────────
        private static void BuildArchive(Transform r, Vector2 o)
        {
            Rect("Ceiling", r, o, -12f, 12f, 12f, 13f, CeilColor);
            WallWithDoors("Wall_Left", r, o, -12f, -11f, 0f, 12f, 0f);
            Rect("Wall_Right", r, o, 11f, -1f, 12f, 12f, WallColor);
            Rect("Floor", r, o, -12f, -1f, 12f, 0f, FloorColor);

            OneWay("Rack_1", r, o, -9f, -4f, 2.8f);
            OneWay("Rack_2", r, o, 1f, 6f, 2.8f);
            OneWay("Rack_3", r, o, -5f, 0f, 5.6f);
            OneWay("Rack_4", r, o, 4f, 10f, 5.6f);
            OneWay("Rack_Top", r, o, -2f, 3f, 8.4f);
            for (int i = 0; i < 4; i++) DecorRect($"RackLight_{i}", r, o, -9f + i * 5.5f, 9.8f, -8.6f + i * 5.5f, 10.1f, CyanLight);

            CreateSignpost("Log_Test07", r, o + new Vector2(-8f, 0.5f), "REGISTRO DE TESTE 07 — Protótipo de combate: teste de campo interrompido. Falha de calibração.");
            CreateSignpost("Log_Test12", r, o + new Vector2(-1.5f, 0.5f), "REGISTRO DE TESTE 12 — Custos acima do previsto. A diretoria pede a suspensão do programa.");
            CreateSignpost("Log_Memo", r, o + new Vector2(5f, 0.5f), "MEMORANDO — Projeto engavetado. Laboratório lacrado. Acesso pela Linha 7 encerrado.");

            CreateBatteryCell(r, o + new Vector2(-7f, 3.4f));
            EnemySetupUtility.CreateDormantScout("Archive_DormantScout", r, s_sprite, o + new Vector2(8f, 5.6f));
            CreateUpgrade("BatteryCapacityUpgrade", r, o + new Vector2(0.5f, 9.1f), UpgradeType.BatteryCapacity, 25f, "prologue_archive_battery_upgrade");
        }

        // ───────────────────────────── P6 — descent (28×44) ─────────────────────────────
        // Teach drop-through on wide platforms → zigzag with a scout on an 8-wide one-way (drop through to
        // escape) → long guided fall past a hidden niche. Every step ≤ 2.8 so it climbs back without Wall Jump.
        private static void BuildP6(Transform r, Vector2 o)
        {
            Rect("Ceiling", r, o, -14f, 5f, 14f, 6f, CeilColor);
            WallWithDoors("Wall_Left", r, o, -14f, -13f, 0f, 5f, 0f);
            Rect("Wall_Left_Lower", r, o, -14f, -38f, -13f, 0f, WallColor);
            WallWithDoors("Wall_Right", r, o, 13f, 14f, -33.6f, 5f, -33.6f);
            Rect("Wall_Right_Base", r, o, 13f, -38f, 14f, -33.6f, WallColor);
            DecorRect("DrainPipe", r, o, 9f, -33f, 9.6f, 4f, new Color(0.22f, 0.24f, 0.22f));
            DecorRect("ShelterGlow", r, o, -13f, -33.6f, 13f, -30.5f, new Color(0.98f, 0.70f, 0.30f, 0.25f), -9);

            Rect("Entry_Ledge", r, o, -13f, -2f, -7.5f, 0f, FloorColor);
            Rect("Bottom_Floor", r, o, -13f, -38f, 13f, -33.6f, FloorColor);

            OneWay("Thin_1", r, o, -7f, 3f, -2.8f);
            OneWay("Thin_2", r, o, -7f, 3f, -5.6f);
            OneWay("Thin_3", r, o, 2f, 7f, -8.4f);
            OneWay("Thin_4", r, o, -6f, -1f, -11.2f);
            OneWay("Thin_ScoutLanding", r, o, -4f, 4f, -14f);
            OneWay("Thin_6", r, o, 3f, 8f, -16.8f);
            OneWay("Thin_7", r, o, -5f, 0f, -19.6f);
            OneWay("Thin_8", r, o, 2f, 7f, -22.4f);
            OneWay("Thin_NicheApproach", r, o, -10f, -7f, -22.4f);
            OneWay("Thin_10", r, o, -6f, -1f, -25.2f);
            OneWay("Thin_11", r, o, 1f, 6f, -28f);
            OneWay("Thin_12", r, o, -5f, 0f, -30.8f);

            EnemySetupUtility.CreateScoutRobot("Scout_Landing", r, s_sprite, o + new Vector2(0f, -14f));
            // Drone in the open shaft: pressures the descent from the air (drop through to break its line of sight).
            EnemySetupUtility.CreateDrone("Drone_Shaft", r, s_sprite, o + new Vector2(6f, -19f));

            // Hidden niche in the left wall, hinted by a flickering lamp at its mouth.
            Rect("Niche_Floor", r, o, -13f, -25f, -10f, -22.4f, WallColor);
            Rect("Niche_Roof", r, o, -13f, -19.9f, -10f, -17f, WallColor);
            DecorRect("Niche_Lamp", r, o, -10.4f, -19.7f, -9.9f, -19.3f, WarmLight, 6);
            CreateBatteryCell(r, o + new Vector2(-11.3f, -21.9f));
            CreatePartsPickup(r, o + new Vector2(-12.4f, -21.8f));
            var nicheCover = DecorRect("Niche_FakeWall", r, o, -13f, -22.4f, -10f, -19.9f, WallColor, 5);
            SecretWall("Secret_ShaftNiche", r, o, -10.8f, -22.4f, -10f, -19.9f, "p6_shaft_niche", nicheCover);

            CreateSignpost("Hint_DropThrough", r, o + new Vector2(-2f, -2.3f), "[S] + [Espaço] Descer pela plataforma fina");
            CreateSignpost("Hint_ToShelter", r, o + new Vector2(8f, -33.1f), "Abrigo da Resistência →");
        }

        // ───────────────────────────── T — service tunnel (one-way shortcut P4 → P1) ─────────────────────────────
        private static void BuildTunnel(Transform r, Vector2 o)
        {
            Rect("Ceiling", r, o, -72f, 6f, 72f, 7f, CeilColor);
            Rect("Floor", r, o, -72f, -1f, 72f, 0f, FloorColor);
            WallWithDoors("Wall_Left", r, o, -72f, -71f, 0f, 6f, 0f);
            Rect("Wall_Right", r, o, 71f, -1f, 72f, 6f, WallColor);
            DecorRect("ServiceRails", r, o, -71f, 0.05f, 71f, 0.15f, new Color(0.32f, 0.30f, 0.27f));
            for (int i = 0; i < 12; i++) DecorRect($"TunnelLight_{i}", r, o, -66f + i * 12f, 5.3f, -65.2f + i * 12f, 5.6f, WarmLight);
            CreateSignpost("Sign_Tunnel", r, o + new Vector2(64f, 0.5f), "Túnel de serviço — saída para a plataforma ←");
        }

        // ───────────────────────────── helpers ─────────────────────────────
        private static RoomBounds Room(string objName, string id, Vector2 o, float x1, float y1, float x2, float y2)
        {
            var room = CreateRoom(objName, id, o + new Vector2((x1 + x2) * 0.5f, (y1 + y2) * 0.5f), new Vector2(x2 - x1, y2 - y1));
            // The lab rooms swap the tiled wall for panels of the same ramp (same zone, same art pattern).
            ZoneArtKit.AddBackdrop(room.transform, UnityEngine.Rect.MinMaxRect(o.x + x1, o.y + y1, o.x + x2, o.y + y2), id.Contains("lab"), o.y);
            return room;
        }

        private static bool IsMassColor(Color color)
            => color == FloorColor || color == WallColor || color == CeilColor || color == PitColor;

        private static GameObject Rect(string name, Transform parent, Vector2 o, float x1, float y1, float x2, float y2, Color color)
            => CreateBlock(name, parent, s_sprite, s_ground, o + new Vector2((x1 + x2) * 0.5f, (y1 + y2) * 0.5f), new Vector2(x2 - x1, y2 - y1), color);

        private static SpriteRenderer DecorRect(string name, Transform parent, Vector2 o, float x1, float y1, float x2, float y2, Color color, int order = -10)
        {
            var worldRect = UnityEngine.Rect.MinMaxRect(o.x + x1, o.y + y1, o.x + x2, o.y + y2);

            // Fake walls look exactly like the masses they hide; other large opaque decor becomes a tinted panel.
            bool mass = IsMassColor(color);
            if (mass || (color.a >= 0.99f && worldRect.width >= 1f && worldRect.height >= 1f))
            {
                return ZoneArtKit.Tiled(name, parent, worldRect, mass ? ZoneArtKit.Mass : ZoneArtKit.Panel, order,
                    mass ? Color.white : ZoneArtKit.ZoneTint(color));
            }

            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = worldRect.center;
            go.transform.localScale = new Vector3(x2 - x1, y2 - y1, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = s_sprite;
            // Opaque flat decor (pipes, rails, cables) is pulled into the zone ramp; lights and translucent marks keep their colour.
            bool isLight = color == WarmLight || color == CyanLight;
            sr.color = !isLight && color.a >= 0.99f ? ZoneArtKit.ZoneTint(color) : color;
            sr.sortingOrder = order;

            // Every light source also lights its surroundings.
            if (color == WarmLight || color == CyanLight)
            {
                ZoneArtKit.AddGlow(name + "_Glow", parent, worldRect.center, color, 5f, order - 1);
            }
            return sr;
        }

        private static void OneWay(string name, Transform parent, Vector2 o, float x1, float x2, float topY)
        {
            var go = CreateBlock(name, parent, s_sprite, s_oneWay, o + new Vector2((x1 + x2) * 0.5f, topY - 0.15f),
                new Vector2(x2 - x1, 0.3f), new Color(0.30f, 0.33f, 0.36f));
            ZoneArtKit.SkinOneWay(go, o.x + x1, o.x + x2, o.y + topY);
            go.GetComponent<BoxCollider2D>().usedByEffector = true;
            var effector = go.AddComponent<PlatformEffector2D>();
            effector.useOneWay = true;
            effector.surfaceArc = 160f;
        }

        /// <summary>Vertical wall from yBottom to yTop with 2.5-tall doorway gaps starting at each door floor.</summary>
        private static void WallWithDoors(string name, Transform parent, Vector2 o, float x1, float x2, float yBottom, float yTop, params float[] doorFloors)
        {
            var floors = new List<float>(doorFloors);
            floors.Sort();
            float y = yBottom;
            int seg = 0;
            foreach (float door in floors)
            {
                if (door > y + 0.01f) Rect($"{name}_{seg++}", parent, o, x1, y, x2, door, WallColor);
                y = door + DoorHeight;
            }
            if (yTop > y + 0.01f) Rect($"{name}_{seg}", parent, o, x1, y, x2, yTop, WallColor);
        }

        private static RoomExitTrigger Exit(string name, RoomBounds from, Vector2 o, float edgeX, int side, float floorY)
            => CreateRoomExit(name, from.transform, o + new Vector2(edgeX - side * ExitInset, floorY + ExitCenterY), ExitSize);

        /// <summary>Two-way door between a's right edge and b's left edge (door floors may differ per side).</summary>
        private static void LinkDoor(string tag, RoomBounds a, Vector2 ao, float aRightEdge, float aFloor, RoomBounds b, Vector2 bo, float bLeftEdge, float bFloor)
        {
            Exit($"Exit_{tag}_Fwd", a, ao, aRightEdge, +1, aFloor).Configure(b, $"from_{tag}_a");
            CreateSpawnPoint($"Spawn_from_{tag}_a", b.transform, $"from_{tag}_a", bo + new Vector2(bLeftEdge + 2.6f, bFloor + 1f));
            Exit($"Exit_{tag}_Back", b, bo, bLeftEdge, -1, bFloor).Configure(a, $"from_{tag}_b");
            CreateSpawnPoint($"Spawn_from_{tag}_b", a.transform, $"from_{tag}_b", ao + new Vector2(aRightEdge - 2.6f, aFloor + 1f));
        }

        private static void Graffiti(string name, Transform parent, Vector2 o, float x, float y)
        {
            // Resistance symbol (image only): a slashed ring made of two strokes.
            DecorRect(name + "_Ring", parent, o, x - 0.6f, y - 0.6f, x + 0.6f, y + 0.6f, GraffitiColor, -3);
            DecorRect(name + "_Slash", parent, o, x - 0.12f, y - 0.9f, x + 0.12f, y + 0.9f, new Color(0.10f, 0.10f, 0.11f, 0.9f), -2);
        }

        private static void SecretWall(string name, Transform parent, Vector2 o, float x1, float y1, float x2, float y2, string persistentId, params SpriteRenderer[] covers)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = o + new Vector2((x1 + x2) * 0.5f, (y1 + y2) * 0.5f);
            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(x2 - x1, y2 - y1);
            var reveal = go.AddComponent<SecretWallReveal>();
            var so = new SerializedObject(reveal);
            var arr = so.FindProperty("covers");
            arr.arraySize = covers.Length;
            for (int i = 0; i < covers.Length; i++) arr.GetArrayElementAtIndex(i).objectReferenceValue = covers[i];
            so.FindProperty("persistentId").stringValue = persistentId;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void CreateArena(string name, Transform parent, Vector2 o, float x1, float y1, float x2, float y2, string persistentId,
            GameObject[] locks, GameObject[] robots)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = o + new Vector2((x1 + x2) * 0.5f, (y1 + y2) * 0.5f);
            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(x2 - x1, y2 - y1);
            var arena = go.AddComponent<ArenaEncounter>();
            var so = new SerializedObject(arena);
            var lockProp = so.FindProperty("lockObjects");
            lockProp.arraySize = locks.Length;
            for (int i = 0; i < locks.Length; i++) lockProp.GetArrayElementAtIndex(i).objectReferenceValue = locks[i];
            var enemyProp = so.FindProperty("enemies");
            enemyProp.arraySize = robots.Length;
            for (int i = 0; i < robots.Length; i++) enemyProp.GetArrayElementAtIndex(i).objectReferenceValue = robots[i].GetComponent<HealthComponent>();
            so.FindProperty("persistentId").stringValue = persistentId;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static CatExplorationPoint KittyPoint(string name, Transform parent, CatExplorationType type, Vector2 pos,
            Vector2 entryPos, Vector2 targetPos, string persistentId, params GameObject[] disables)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            go.transform.localScale = new Vector3(0.9f, 0.8f, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = s_sprite;
            sr.color = type == CatExplorationType.Duct ? new Color(0.32f, 0.58f, 0.62f, 0.85f) : new Color(0.20f, 0.30f, 0.34f, 1f);
            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(2.2f, 2.0f);

            var prompt = new GameObject("PromptGlyph");
            prompt.transform.SetParent(go.transform, false);
            prompt.transform.localPosition = new Vector3(0f, 0.9f, 0f);
            prompt.transform.localScale = new Vector3(0.35f, 0.35f, 1f);
            var promptSr = prompt.AddComponent<SpriteRenderer>();
            promptSr.sprite = s_sprite;
            promptSr.color = new Color(0.96f, 0.86f, 0.32f, 1f);
            promptSr.sortingOrder = 6;
            prompt.SetActive(false);

            var entry = new GameObject("CatEntry");
            entry.transform.SetParent(go.transform, false);
            entry.transform.position = entryPos;
            var target = new GameObject("CatExit");
            target.transform.SetParent(go.transform, false);
            target.transform.position = targetPos;

            var point = go.AddComponent<CatExplorationPoint>();
            var so = new SerializedObject(point);
            so.FindProperty("explorationType").enumValueIndex = (int)type;
            so.FindProperty("catEntryPoint").objectReferenceValue = entry.transform;
            so.FindProperty("catTargetPoint").objectReferenceValue = target.transform;
            so.FindProperty("promptIndicator").objectReferenceValue = prompt;
            so.FindProperty("persistentId").stringValue = persistentId;
            var disableProp = so.FindProperty("objectsToDisableOnComplete");
            disableProp.arraySize = disables.Length;
            for (int i = 0; i < disables.Length; i++) disableProp.GetArrayElementAtIndex(i).objectReferenceValue = disables[i];
            so.ApplyModifiedPropertiesWithoutUndo();
            return point;
        }

        private static void CreateTerminal(string name, Transform parent, Vector2 pos, DocumentDataSO doc, DialogueSequenceSO dialogue,
            string flag, string promptLabel, Color glow)
        {
            var termGo = new GameObject(name);
            termGo.transform.SetParent(parent, false);
            termGo.transform.position = pos;
            termGo.transform.localScale = new Vector3(0.9f, 1.4f, 1f);
            var termSr = termGo.AddComponent<SpriteRenderer>();
            termSr.sprite = s_sprite;
            termSr.color = new Color(0.20f, 0.24f, 0.30f, 1f);
            var termCol = termGo.AddComponent<BoxCollider2D>();
            termCol.isTrigger = true;
            termCol.size = new Vector2(1.8f, 1.4f);

            var indGo = new GameObject("ScreenGlow");
            indGo.transform.SetParent(termGo.transform, false);
            indGo.transform.localPosition = new Vector3(0f, 0.25f, 0f);
            indGo.transform.localScale = new Vector3(0.55f, 0.35f, 1f);
            var indSr = indGo.AddComponent<SpriteRenderer>();
            indSr.sprite = s_sprite;
            indSr.sortingOrder = 2;

            var terminal = termGo.AddComponent<NarrativeTerminal>();
            var so = new SerializedObject(terminal);
            so.FindProperty("statusIndicator").objectReferenceValue = indSr;
            so.FindProperty("flagOnRead").stringValue = flag;
            so.FindProperty("promptLabel").stringValue = promptLabel;
            so.FindProperty("unreadColor").colorValue = glow;
            so.ApplyModifiedPropertiesWithoutUndo();
            terminal.Configure(s_reader, s_state, s_runner, doc, dialogue);
        }

        private static void CreateRestSpot(Transform parent, Vector2 pos)
        {
            var go = new GameObject("EmergencyGenerator_RestSpot");
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            go.transform.localScale = new Vector3(1.6f, 1.8f, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = s_sprite;
            sr.color = new Color(0.95f, 0.65f, 0.28f, 0.45f);
            go.AddComponent<BoxCollider2D>().size = Vector2.one;
            var rest = go.AddComponent<ShelterRestSpot>();
            var so = new SerializedObject(rest);
            so.FindProperty("input").objectReferenceValue = s_reader;
            so.FindProperty("isDefaultShelter").boolValue = false;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void CreateUpgrade(string name, Transform parent, Vector2 pos, UpgradeType type, float amount, string persistentId)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            go.transform.localScale = new Vector3(0.55f, 0.55f, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = s_sprite;
            sr.color = new Color(0.75f, 1f, 0.95f, 1f);
            sr.sortingOrder = 4;
            go.AddComponent<BoxCollider2D>().isTrigger = true;
            var up = go.AddComponent<UpgradePickup>();
            var so = new SerializedObject(up);
            so.FindProperty("upgradeType").enumValueIndex = (int)type;
            so.FindProperty("amount").floatValue = amount;
            so.FindProperty("persistentId").stringValue = persistentId;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void CreatePartsPickup(Transform parent, Vector2 worldPos)
        {
            var partsPrefab = AssetDatabase.LoadAssetAtPath<PartsPickup>(PartsPickupPrefabPath);
            if (partsPrefab == null) return;
            var pickup = (PartsPickup)PrefabUtility.InstantiatePrefab(partsPrefab, parent);
            pickup.transform.position = worldPos;
        }

        private static void CreateBatteryCell(Transform parent, Vector2 worldPos)
        {
            var go = new GameObject("BatteryCell");
            go.transform.SetParent(parent, false);
            go.transform.position = worldPos;
            go.transform.localScale = new Vector3(0.4f, 0.4f, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = s_sprite;
            sr.color = new Color(0.306f, 0.804f, 0.769f, 1f);
            go.AddComponent<BoxCollider2D>().isTrigger = true;
            go.AddComponent<BatteryCellPickup>();
        }

        private static DocumentDataSO EnsureDocument(string fileName, string id, string title, string author, string body)
        {
            string path = Path.Combine(NarrativeDataDir, fileName);
            var asset = AssetDatabase.LoadAssetAtPath<DocumentDataSO>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<DocumentDataSO>();
                AssetDatabase.CreateAsset(asset, path);
            }
            asset.documentId = id;
            asset.title = title;
            asset.author = author;
            asset.bodyText = body;
            EditorUtility.SetDirty(asset);
            return asset;
        }

        private static DialogueSequenceSO EnsureDialogue(string fileName, List<DialogueLine> lines)
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

        private static GameObject CreatePlayer(
            GameInputReader reader,
            Sprite sprite,
            int ground,
            int oneWay,
            int pHit,
            int pHurt)
        {
            var player = new GameObject("Dante") { tag = "Player" };
            player.transform.position = new Vector3(-20f, 1f, 0f);
            player.transform.localScale = new Vector3(0.7f, 1.3f, 1f);
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

            player.AddComponent<PlayerUpgradeApplier>();
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
            go.AddComponent<BoxCollider2D>().size = Vector2.one;

            // The block only carries the collider; ZoneArtKit adds its art as children (one-ways skin themselves).
            if (layer == s_ground)
            {
                var kind = IsMassColor(color) ? ZoneArtKit.SolidKind.Mass
                    : color == LedgeColor ? ZoneArtKit.SolidKind.Structure
                    : ZoneArtKit.SolidKind.Panel;
                ZoneArtKit.RegisterSolid(go, new UnityEngine.Rect(worldPos - size * 0.5f, size), kind, ZoneArtKit.ZoneTint(color));
            }
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
