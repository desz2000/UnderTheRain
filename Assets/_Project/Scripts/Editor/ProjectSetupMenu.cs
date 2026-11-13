using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnderTheRain.Core;
using UnderTheRain.Core.Input;
using UnderTheRain.World;

namespace UnderTheRain.Editor
{
    /// <summary>
    /// Editor utilities to scaffold the project folder hierarchy and persistent Core scene.
    /// </summary>
    public static class ProjectSetupMenu
    {
        private const string CoreScenePath = "Assets/_Project/Scenes/Core.unity";
        private const string InputActionsPath = "Assets/InputSystem_Actions.inputactions";

        private static readonly string[] RequiredFolders =
        {
            "Assets/_Project/Art/Sprites",
            "Assets/_Project/Art/Tilesets",
            "Assets/_Project/Art/Animations",
            "Assets/_Project/Audio/Music",
            "Assets/_Project/Audio/SFX",
            "Assets/_Project/Audio/Mixers",
            "Assets/_Project/Data/Weapons",
            "Assets/_Project/Data/Abilities",
            "Assets/_Project/Data/Enemies",
            "Assets/_Project/Data/Items",
            "Assets/_Project/Data/Narrative",
            "Assets/_Project/Data/World",
            "Assets/_Project/Prefabs/Player",
            "Assets/_Project/Prefabs/Companion",
            "Assets/_Project/Prefabs/Enemies",
            "Assets/_Project/Prefabs/Environment",
            "Assets/_Project/Prefabs/UI",
            "Assets/_Project/Scenes/Zones",
            "Assets/_Project/Settings"
        };

        [MenuItem("UnderTheRain/Setup/Create Folder Structure and Core Scene")]
        public static void CreateFolderStructureAndCoreScene()
        {
            EnsureFolders();
            CreateCoreSceneIfMissing();
            AssetDatabase.Refresh();
        }

        private static void EnsureFolders()
        {
            foreach (string folder in RequiredFolders)
            {
                if (!AssetDatabase.IsValidFolder(folder))
                {
                    Directory.CreateDirectory(folder);
                }
            }
        }

        private static void CreateCoreSceneIfMissing()
        {
            if (File.Exists(CoreScenePath))
            {
                Debug.Log($"[UnderTheRain] Core scene already exists at {CoreScenePath}");
                return;
            }

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var cameraGo = new GameObject("Main Camera") { tag = "MainCamera" };
            var cam = cameraGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5.4f;
            cam.backgroundColor = new Color(0.055f, 0.063f, 0.078f, 1f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            cameraGo.transform.position = new Vector3(0f, 0f, -10f);
            cameraGo.AddComponent<AudioListener>();

            var bootstrapGo = new GameObject("GameBootstrap");
            bootstrapGo.AddComponent<GameBootstrap>();

            EditorSceneManager.SaveScene(scene, CoreScenePath);
            Debug.Log($"[UnderTheRain] Created Core scene at {CoreScenePath}");
        }

        [MenuItem("UnderTheRain/Setup/Wire Input in Core Scene")]
        public static void WireInputInCoreScene()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogError("[UnderTheRain] Exit Play Mode before running Setup menu commands.");
                return;
            }
            if (!File.Exists(CoreScenePath))
            {
                Debug.LogError($"[UnderTheRain] Core scene not found at {CoreScenePath}. Run 'Create Folder Structure and Core Scene' first.");
                return;
            }

            var scene = EditorSceneManager.OpenScene(CoreScenePath, OpenSceneMode.Single);
            var bootstrap = Object.FindFirstObjectByType<GameBootstrap>();
            if (bootstrap == null)
            {
                var go = new GameObject("GameBootstrap");
                bootstrap = go.AddComponent<GameBootstrap>();
            }

            var reader = bootstrap.GetComponent<GameInputReader>();
            if (reader == null)
            {
                reader = bootstrap.gameObject.AddComponent<GameInputReader>();
            }

            var asset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputActionsPath);
            if (asset == null)
            {
                Debug.LogError($"[UnderTheRain] InputActionAsset not found at {InputActionsPath}");
                return;
            }

            var so = new SerializedObject(reader);
            so.FindProperty("inputActions").objectReferenceValue = asset;
            so.ApplyModifiedPropertiesWithoutUndo();

            if (bootstrap.GetComponent<InputDeviceTracker>() == null)
            {
                bootstrap.gameObject.AddComponent<InputDeviceTracker>();
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[UnderTheRain] Wired GameInputReader and InputDeviceTracker in Core scene.");
        }

        [MenuItem("UnderTheRain/Setup/Wire Room Transition in Core Scene")]
        public static void WireRoomTransitionInCoreScene()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogError("[UnderTheRain] Exit Play Mode before running Setup menu commands.");
                return;
            }
            if (!File.Exists(CoreScenePath))
            {
                Debug.LogError($"[UnderTheRain] Core scene not found at {CoreScenePath}. Run 'Create Folder Structure and Core Scene' first.");
                return;
            }

            var scene = EditorSceneManager.OpenScene(CoreScenePath, OpenSceneMode.Single);
            var mainCam = Camera.main;
            RoomCamera2D roomCam = null;
            if (mainCam != null)
            {
                roomCam = mainCam.GetComponent<RoomCamera2D>() ?? mainCam.gameObject.AddComponent<RoomCamera2D>();
            }

            var fader = Object.FindFirstObjectByType<ScreenFader>();
            if (fader == null)
            {
                var canvasGo = new GameObject("TransitionFadeCanvas");
                var canvas = canvasGo.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 999;

                var cg = canvasGo.AddComponent<CanvasGroup>();
                cg.alpha = 0f;
                cg.blocksRaycasts = false;

                var panelGo = new GameObject("FadeOverlay");
                panelGo.transform.SetParent(canvasGo.transform, false);
                var rect = panelGo.AddComponent<RectTransform>();
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;

                var img = panelGo.AddComponent<Image>();
                img.color = new Color(0.055f, 0.063f, 0.078f, 1f);
                img.raycastTarget = false;

                fader = canvasGo.AddComponent<ScreenFader>();
            }

            var bootstrap = Object.FindFirstObjectByType<GameBootstrap>();
            if (bootstrap != null)
            {
                var controller = bootstrap.GetComponent<RoomTransitionController>()
                                 ?? bootstrap.gameObject.AddComponent<RoomTransitionController>();
                var so = new SerializedObject(controller);
                so.FindProperty("screenFader").objectReferenceValue = fader;
                so.FindProperty("roomCamera").objectReferenceValue = roomCam;
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[UnderTheRain] Wired ScreenFader, RoomCamera2D, and RoomTransitionController in Core scene.");
        }
    }
}
