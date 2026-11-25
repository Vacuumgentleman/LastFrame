using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace LastFrame.EditorTools
{
    /// <summary>
    /// Creates the Last Frame project structure described in the GDD:
    /// folders under Assets/LastFrame (each empty one gets a .gitkeep so git uploads it),
    /// and one scene per screen/set with the standard hierarchy.
    /// Safe to run again: existing folders and scenes are kept.
    /// Menu: Tools > Last Frame > Create Project Structure
    /// </summary>
    public static class ProjectStructureSetup
    {
        const string k_root = "Assets/LastFrame";
        const string k_scenes = k_root + "/Scenes";

        static readonly string[] k_folders =
        {
            "Art/Animations",
            "Art/Characters/Protagonist",
            "Art/Characters/Enemies",
            "Art/Characters/Boss",
            "Art/Environment/Props",
            "Art/Environment/Sets",
            "Art/Fonts",
            "Art/Materials",
            "Art/Models",
            "Art/Sprites",
            "Art/Textures",
            "Art/UI",
            "Art/VFX",
            "Audio/Music",
            "Audio/SFX",
            "Audio/Ambience",
            "Audio/Mixers",
            "Prefabs/Characters",
            "Prefabs/Enemies",
            "Prefabs/Environment",
            "Prefabs/Managers",
            "Prefabs/Pickups",
            "Prefabs/UI",
            "Prefabs/VFX",
            "Scenes/Menus",
            "Scenes/Cinematics",
            "Scenes/Sets",
            "Scripts/Core",
            "Scripts/Player",
            "Scripts/Enemies",
            "Scripts/Weapons",
            "Scripts/Collectibles",
            "Scripts/Environment",
            "Scripts/UI",
            "Scripts/Audio",
            "Scripts/Data",
            "Scripts/Editor",
            "ScriptableObjects/Memories",
            "ScriptableObjects/Weapons",
            "Timeline",
        };

        // Standard scene hierarchy from the GDD project conventions.
        static readonly string[] k_sections =
        {
            "--- Managers ---", "--- Environment ---", "--- Gameplay ---", "--- UI ---", "--- Lighting ---", "--- Audio ---"
        };

        enum SceneKind { Menu, Cinematic, Set }

        struct SceneDef
        {
            public string Folder, Name;
            public SceneKind Kind;
            public SceneDef(string _folder, string _name, SceneKind _kind) { Folder = _folder; Name = _name; Kind = _kind; }
            public string Path => $"{k_scenes}/{Folder}/{Name}.unity";
        }

        // Build order follows the screen flow in the GDD.
        static readonly SceneDef[] k_sceneDefs =
        {
            new SceneDef("Menus", "00_Splash", SceneKind.Menu),           // rating screen + team logo
            new SceneDef("Menus", "01_MainMenu", SceneKind.Menu),         // play, options, credits, quit
            new SceneDef("Cinematics", "02_Prologue", SceneKind.Cinematic), // "the last frame"
            new SceneDef("Sets", "10_Set1_Tutorial", SceneKind.Set),      // the dark set (fixed)
            new SceneDef("Sets", "11_Set2_Props", SceneKind.Set),         // random order
            new SceneDef("Sets", "12_Set3_Iluminado", SceneKind.Set),     // random order
            new SceneDef("Sets", "13_Set4_Camaras", SceneKind.Set),       // random order (design TBD)
            new SceneDef("Sets", "14_Set5_Reemplazo", SceneKind.Set),     // boss (fixed)
            new SceneDef("Menus", "20_GameOver", SceneKind.Menu),
            new SceneDef("Cinematics", "21_Ending", SceneKind.Cinematic),
            new SceneDef("Menus", "22_Results", SceneKind.Menu),          // S to E grade
            new SceneDef("Menus", "23_Credits", SceneKind.Menu),
        };

        [MenuItem("Tools/Last Frame/Create Project Structure")]
        public static void RunFromMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            string _report = Run();
            EditorUtility.DisplayDialog("Last Frame", _report, "OK");
        }

        public static string Run()
        {
            var _log = new System.Text.StringBuilder();
            string _previousScene = SceneManager.GetActiveScene().path;

            foreach (string _folder in k_folders) EnsureFolder($"{k_root}/{_folder}");
            _log.AppendLine($"Folders: {k_folders.Length} in {k_root}");

            int _created = 0;
            foreach (SceneDef _def in k_sceneDefs)
            {
                if (File.Exists(_def.Path)) continue;
                CreateScene(_def);
                _created++;
            }
            _log.AppendLine($"Scenes created: {_created} (existing kept: {k_sceneDefs.Length - _created})");

            UpdateBuildSettings();
            _log.AppendLine("Build Settings updated");

            AddGitKeeps();
            AssetDatabase.Refresh();

            if (!string.IsNullOrEmpty(_previousScene) && File.Exists(_previousScene))
                EditorSceneManager.OpenScene(_previousScene);

            return _log.ToString();
        }

        static void CreateScene(SceneDef _def)
        {
            Scene _scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var _sections = new Dictionary<string, Transform>();
            foreach (string _name in k_sections) _sections[_name] = new GameObject(_name).transform;

            // Lighting
            var _sun = new GameObject("Directional Light");
            _sun.transform.SetParent(_sections["--- Lighting ---"]);
            _sun.transform.rotation = Quaternion.Euler(50f, 330f, 0f);
            var _light = _sun.AddComponent<Light>();
            _light.type = LightType.Directional;
            _light.shadows = LightShadows.Soft;
            _light.intensity = _def.Kind == SceneKind.Set ? 1f : 1.5f;
            _sun.AddComponent<UniversalAdditionalLightData>();

            // Camera (top-down angled view for sets, front view for menus/cinematics)
            var _camGo = new GameObject("Main Camera") { tag = "MainCamera" };
            _camGo.transform.SetParent(_sections["--- Gameplay ---"]);
            var _cam = _camGo.AddComponent<Camera>();
            _camGo.AddComponent<AudioListener>();
            _camGo.AddComponent<UniversalAdditionalCameraData>();
            if (_def.Kind == SceneKind.Set)
            {
                _camGo.transform.SetPositionAndRotation(new Vector3(0f, 12f, -10f), Quaternion.Euler(50f, 0f, 0f));
                _cam.fieldOfView = 50f;
            }
            else
            {
                _camGo.transform.position = new Vector3(0f, 1f, -10f);
                _cam.clearFlags = CameraClearFlags.SolidColor;
                _cam.backgroundColor = new Color32(0x25, 0x1E, 0x23, 0xFF); // GDD palette: café tinta
            }

            if (_def.Kind == SceneKind.Set)
            {
                // Environment placeholder floor (tag Ground is required by the player/noise systems)
                var _floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
                _floor.name = "Floor (placeholder)";
                _floor.transform.SetParent(_sections["--- Environment ---"]);
                _floor.transform.localScale = new Vector3(4f, 1f, 4f);
                _floor.isStatic = true;
                if (TagExists("Ground")) _floor.tag = "Ground";

                foreach (string _group in new[] { "Props", "LightZones" })
                    new GameObject(_group).transform.SetParent(_sections["--- Environment ---"]);

                var _spawn = new GameObject("PlayerSpawn");
                _spawn.transform.SetParent(_sections["--- Gameplay ---"]);
                foreach (string _group in new[] { "Enemies", "SecurityCameras", "Pickups", "MemoryTape", "SetDoor" })
                    new GameObject(_group).transform.SetParent(_sections["--- Gameplay ---"]);

                new GameObject("HUD").transform.SetParent(_sections["--- UI ---"]);
            }
            else if (_def.Kind == SceneKind.Cinematic)
            {
                new GameObject("Timeline").transform.SetParent(_sections["--- Gameplay ---"]);
            }
            else
            {
                new GameObject("Canvas").transform.SetParent(_sections["--- UI ---"]);
            }

            new GameObject("Music").transform.SetParent(_sections["--- Audio ---"]);

            EditorSceneManager.SaveScene(_scene, _def.Path);
        }

        static void UpdateBuildSettings()
        {
            var _scenes = k_sceneDefs.Select(d => new EditorBuildSettingsScene(d.Path, true)).ToList();

            // Keep any other scene already listed (e.g. sandbox test rooms), disabled so it never ships.
            foreach (EditorBuildSettingsScene _existing in EditorBuildSettings.scenes)
            {
                if (_scenes.Any(s => s.path == _existing.path)) continue;
                if (!File.Exists(_existing.path)) continue;
                _scenes.Add(new EditorBuildSettingsScene(_existing.path, false));
            }

            EditorBuildSettings.scenes = _scenes.ToArray();
        }

        static void EnsureFolder(string _path)
        {
            if (AssetDatabase.IsValidFolder(_path)) return;
            string _parent = Path.GetDirectoryName(_path).Replace('\\', '/');
            EnsureFolder(_parent);
            AssetDatabase.CreateFolder(_parent, Path.GetFileName(_path));
        }

        [MenuItem("Tools/Last Frame/Add .gitkeep to Empty Folders")]
        public static void AddGitKeepsFromMenu()
        {
            int _added = AddGitKeeps();
            Debug.Log($"[LastFrame] .gitkeep added to {_added} empty folder(s) under {k_root}");
        }

        // Git does not track empty folders; without a file inside, Unity would delete the folder's .meta on clone.
        static int AddGitKeeps()
        {
            int _added = 0;
            foreach (string _dir in Directory.GetDirectories(k_root, "*", SearchOption.AllDirectories))
            {
                if (Directory.EnumerateFileSystemEntries(_dir).Any()) continue;
                File.WriteAllText(Path.Combine(_dir, ".gitkeep"), string.Empty);
                _added++;
            }
            return _added;
        }

        static bool TagExists(string _tag)
        {
            return UnityEditorInternal.InternalEditorUtility.tags.Contains(_tag);
        }
    }
}
