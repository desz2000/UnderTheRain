using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnderTheRain.Core;

namespace UnderTheRain.Editor
{
    /// <summary>
    /// Editor utilities to scaffold the project folder hierarchy and persistent Core scene.
    /// </summary>
    public static class ProjectSetupMenu
    {
        private const string CoreScenePath = "Assets/_Project/Scenes/Core.unity";

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
    }
}
