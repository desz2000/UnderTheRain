using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnderTheRain.Core.Input;
using UnderTheRain.Player;
using UnderTheRain.World;

namespace UnderTheRain.Editor
{
    /// <summary>Scaffolds the Story 2.1 movement test room: config asset, layers, player and platforms.</summary>
    public static class MovementTestRoomSetup
    {
        private const string ConfigPath = "Assets/_Project/Data/Abilities/PlayerMovementConfig.asset";
        private const string ScenePath = "Assets/_Project/Scenes/Zones/MovementTestRoom.unity";
        private const string InputActionsPath = "Assets/InputSystem_Actions.inputactions";
        private const string GroundLayerName = "Ground";
        private const string OneWayLayerName = "OneWayPlatform";

        [MenuItem("UnderTheRain/Setup/Create Movement Test Room")]
        public static void CreateMovementTestRoom()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogError("[UnderTheRain] Exit Play Mode before running Setup menu commands.");
                return;
            }

            EnsureLayer(GroundLayerName);
            EnsureLayer(OneWayLayerName);
            int ground = LayerMask.NameToLayer(GroundLayerName);
            int oneWay = LayerMask.NameToLayer(OneWayLayerName);

            var config = AssetDatabase.LoadAssetAtPath<PlayerMovementConfigSO>(ConfigPath);
            if (config == null)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(ConfigPath));
                config = ScriptableObject.CreateInstance<PlayerMovementConfigSO>();
                AssetDatabase.CreateAsset(config, ConfigPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.ImportAsset(ConfigPath);
                config = AssetDatabase.LoadAssetAtPath<PlayerMovementConfigSO>(ConfigPath);
            }

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var sprite = GetOrCreateSquareSprite();

            var cam = new GameObject("Main Camera") { tag = "MainCamera" };
            var camera = cam.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5.4f;
            camera.backgroundColor = new Color(0.055f, 0.063f, 0.078f, 1f);
            camera.clearFlags = CameraClearFlags.SolidColor;
            cam.transform.position = new Vector3(0f, 1f, -10f);
            cam.AddComponent<AudioListener>();
            var roomCam = cam.AddComponent<RoomCamera2D>();

            var inputGo = new GameObject("GameInputReader");
            var reader = inputGo.AddComponent<GameInputReader>();
            var inputAsset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputActionsPath);
            var readerSo = new SerializedObject(reader);
            readerSo.FindProperty("inputActions").objectReferenceValue = inputAsset;
            readerSo.ApplyModifiedPropertiesWithoutUndo();

            // Level geometry
            CreateBlock("Floor Left", sprite, ground, new Vector2(0f, -1f), new Vector2(12f, 2f));
            CreateBlock("Floor Right", sprite, ground, new Vector2(20f, -1f), new Vector2(12f, 2f)); // gap = coyote test
            CreateBlock("Ceiling Block", sprite, ground, new Vector2(3f, 3.6f), new Vector2(3f, 1f));  // corner correction test
            CreateBlock("Step Wall", sprite, ground, new Vector2(-5.5f, 2f), new Vector2(1f, 6f));
            // Vertical shaft for Story 2.2 (Wall Slide & Wall Jump)
            CreateBlock("Shaft Wall Left", sprite, ground, new Vector2(21f, 4.5f), new Vector2(1f, 7f));
            CreateBlock("Shaft Wall Right", sprite, ground, new Vector2(25.5f, 4f), new Vector2(1f, 8f));
            CreateBlock("High Reward Platform", sprite, ground, new Vector2(18.5f, 7.5f), new Vector2(4f, 1f));
            var thin = CreateBlock("Thin Platform", sprite, oneWay, new Vector2(10f, 2.5f), new Vector2(5f, 0.3f));
            var col = thin.GetComponent<BoxCollider2D>();
            col.usedByEffector = true;
            var effector = thin.AddComponent<PlatformEffector2D>();
            effector.useOneWay = true;
            effector.surfaceArc = 160f;

            // Player
            var player = new GameObject("Dante");
            player.transform.position = new Vector3(0f, 1f, 0f);
            var sr = player.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = new Color(0.31f, 0.8f, 0.77f);
            player.transform.localScale = new Vector3(0.8f, 1.6f, 1f);
            var rb = player.AddComponent<Rigidbody2D>();
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            player.AddComponent<BoxCollider2D>().size = Vector2.one;
            var controller = player.AddComponent<PlayerController>();
            var so = new SerializedObject(controller);
            so.FindProperty("input").objectReferenceValue = reader;
            // Reload after NewScene: switching scenes can unload the asset instance loaded earlier.
            so.FindProperty("config").objectReferenceValue = AssetDatabase.LoadAssetAtPath<PlayerMovementConfigSO>(ConfigPath);
            so.FindProperty("groundMask").intValue = (1 << ground) | (1 << oneWay);
            so.FindProperty("oneWayMask").intValue = 1 << oneWay;
            so.ApplyModifiedPropertiesWithoutUndo();

            var camSo = new SerializedObject(roomCam);
            camSo.FindProperty("target").objectReferenceValue = player.transform;
            camSo.ApplyModifiedPropertiesWithoutUndo();

            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            Debug.Log($"[UnderTheRain] Created movement test room at {ScenePath}");
        }

        // 1x1 world-unit white square so transform scale equals world size.
        private static Sprite GetOrCreateSquareSprite()
        {
            const string path = "Assets/_Project/Art/Sprites/WhiteSquare.png";
            if (AssetDatabase.LoadAssetAtPath<Sprite>(path) == null)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                var tex = new Texture2D(4, 4);
                var pixels = new Color[16];
                for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.white;
                tex.SetPixels(pixels);
                File.WriteAllBytes(path, tex.EncodeToPNG());
                Object.DestroyImmediate(tex);
                AssetDatabase.ImportAsset(path);
                var importer = (TextureImporter)AssetImporter.GetAtPath(path);
                importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = 4f;
                importer.filterMode = FilterMode.Point;
                importer.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static GameObject CreateBlock(string name, Sprite sprite, int layer, Vector2 pos, Vector2 size)
        {
            var go = new GameObject(name) { layer = layer };
            go.transform.position = pos;
            go.transform.localScale = new Vector3(size.x, size.y, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = new Color(0.25f, 0.28f, 0.33f);
            go.AddComponent<BoxCollider2D>().size = Vector2.one;
            return go;
        }

        private static void EnsureLayer(string layerName)
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
