using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnderTheRain.World;

namespace UnderTheRain.Editor
{
    /// <summary>
    /// Renders game-view stills of the Prologue rooms to PNG files, so zone art can be reviewed
    /// without entering Play Mode (also works from the command line).
    /// The scene is opened, photographed and left unsaved.
    /// </summary>
    internal static class ArtPreviewRenderer
    {
        private const string ScenePath = "Assets/_Project/Scenes/Zones/Prologue_Ruins.unity";
        private const string OutputDir = "docs/art-review/prologue-unity";
        private const int Width = 1280;
        private const int Height = 720;

        private static readonly (string file, string room, Vector2 center)[] Shots =
        {
            ("p1_platform_left", "Prologue_P1_MetroPlatform", new Vector2(-13f, 3.4f)),
            ("p1_wagon_and_booth", "Prologue_P1_MetroPlatform", new Vector2(9f, 3.4f)),
            ("p2_gaps", "Prologue_P2_Gap", new Vector2(54f, 2.6f)),
            ("p3_slab_and_office", "Prologue_P3_Debris", new Vector2(118f, 4.4f)),
            ("p3_bridge_and_generator", "Prologue_P3_Debris", new Vector2(138f, 2.6f)),
            ("p4_ducts", "Prologue_P4_Duct", new Vector2(176f, 4.6f)),
            ("p5_lab_arena", "Prologue_P5_HiddenLab", new Vector2(238f, 3.6f)),
            ("p5_lab_deck", "Prologue_P5_HiddenLab", new Vector2(226f, 10f)),
            ("p5a_archive", "Prologue_P5a_LabArchive", new Vector2(280f, 17f)),
            ("p6_descent", "Prologue_P6_Descent", new Vector2(282f, -14f)),
            ("tunnel", "Prologue_T_ServiceTunnel", new Vector2(100f, -10.2f))
        };

        [MenuItem("UnderTheRain/Art/Render Prologue Previews")]
        public static void RenderProloguePreviews()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogError("[UnderTheRain] Exit Play Mode before rendering previews.");
                return;
            }

            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Camera cam = null;
            foreach (var root in scene.GetRootGameObjects())
            {
                cam = root.GetComponentInChildren<Camera>(true);
                if (cam != null) break;
            }
            if (cam == null)
            {
                Debug.LogError("[UnderTheRain] No camera in the Prologue scene.");
                return;
            }

            Directory.CreateDirectory(OutputDir);
            var target = new RenderTexture(Width, Height, 24);
            var readback = new Texture2D(Width, Height, TextureFormat.RGB24, false);
            cam.targetTexture = target;

            foreach (var (file, roomName, center) in Shots)
            {
                foreach (var root in scene.GetRootGameObjects())
                {
                    if (root.GetComponent<RoomBounds>() != null) root.SetActive(root.name == roomName);
                }

                cam.transform.position = new Vector3(center.x, center.y, -10f);
                cam.Render();

                RenderTexture.active = target;
                readback.ReadPixels(new Rect(0, 0, Width, Height), 0, 0);
                readback.Apply();
                RenderTexture.active = null;
                File.WriteAllBytes($"{OutputDir}/{file}.png", readback.EncodeToPNG());
            }

            cam.targetTexture = null;
            Object.DestroyImmediate(target);
            Object.DestroyImmediate(readback);
            Debug.Log($"[UnderTheRain] Rendered {Shots.Length} previews to {OutputDir}");

            // Leave the scene asset untouched: reopen it clean.
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        }
    }
}
