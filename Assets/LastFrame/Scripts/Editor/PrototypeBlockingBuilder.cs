using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using Senses2026;
using SensesTestRoom;

namespace LastFrame.EditorTools
{
    /// <summary>
    /// Builds the 5 level-blocking prototypes in Assets/LastFrame/Scenes/Prototypes/v{n} (Prototype 1..5).
    /// Theme: an old cartoon and TV studio. Each level is a set of studio rooms dressed with primitives,
    /// and adds Senses mechanics on top of the previous ones.
    ///
    /// Enemies come in two behaviours (shy/static and patrolling) and carry a melee or a ranged weapon.
    /// Patrol routes, weapons, the player's planned kills (melee from behind, ranged ink shot) and pickups
    /// are drawn under "--- Plan (ocultable) ---": visual only, no logic. Hide that object to see the bare level.
    ///
    /// Working Senses objects (guards, cameras, dog, grass, river...) are cloned from the Senses test room,
    /// where they already work with the shared player, and their references are remapped to the clones.
    /// Versions (all of them stay in the project as a record of the design process):
    /// - v1 (Tools > Last Frame > Build Prototype Blocking v1): first pass, human-scale studio, security guards.
    ///   Writes Prototypes/v1/Prototype N.unity.
    /// - v2 (Tools > Last Frame > Build Prototype Blocking v2): aligned with the GDD. Ollie is smaller than the
    ///   world (player x0.7, furniture x1.2, taller walls), enemies are AI cartoons (otter = patrols,
    ///   capybara = static) with an eraser (melee) or an eraser launcher (ranged), the memory tape opens the
    ///   exit, and the non-GDD Senses pieces are re-themed without removing any mechanic.
    ///   Writes Prototypes/v2/Prototype N v2.unity. Never touches the v1 scenes.
    /// - v3 (Tools > Last Frame > Build Prototype Blocking v3): v2 + every mechanic, once taught, comes back in the
    ///   later levels (light, night vision, stealth zones, cameras, half cover, hearing, floors, noise traps).
    ///   P2 gets more enemies, P3 more cameras and patrols, P4 a new foley room, P5 is at night with lamps,
    ///   floodlights and more cameras. Writes Prototypes/v3/Prototype N v3.unity.
    /// - v4 (Tools > Last Frame > Build Prototype Blocking v4): the v3 layout rendered with Flat Kit: every blocking
    ///   material and every opaque Lit material on the cloned Senses objects becomes a FlatKit/Stylized Surface
    ///   (one hard cel step, shadows tinted toward the studio ink, ink outlines). Writes Prototypes/v4/Prototype N v4.unity.
    /// - v10 (Tools > Last Frame > Build Prototype Blocking v10 (P1, abandonado)): v9 abandoned for real: daylight through
    ///   dusty windows, a projector left running, dead tubes and failing bulbs, dust in every shaft, only the AI's lights
    ///   clean; props from the concept art (projector, film reels, easels, leaning flats, drawings on the floor).
    ///   P2-P5 get the same finish on their own layout (dressLevelV10). Writes Prototypes/v10/Prototype N v10.unity.
    /// - v9 (Tools > Last Frame > Build Prototype Blocking v9 (P1, estilo GDD)): the v8 light plan with the art direction of
    ///   the GDD concept art and its official palette: a warm wooden studio (plank floors, walnut, brick, ivory), plum
    ///   shadows, furniture under dust sheets, warm afternoon light shafts through dusty windows. Writes Prototypes/v9/Prototype 1 v9.unity.
    /// - v8 (Tools > Last Frame > Build Prototype Blocking v8 (P1, luz motivada)): motivated lighting on P1, in three
    ///   variants built on v7b (v8a), v7c (v8b) and v5 (v8c). No sun inside the studio: every light comes from a visible
    ///   fixture (AI security floodlights, desk lamps, failing tubes, a bare bulb, the TV, emergency and EXIT lights),
    ///   draws its cone and floor pool, and is measured by Senses, so the pools are the danger zones of the level.
    ///   v8d sits halfway between v8b and v8c (light, color, shading and post averaged; palette mixed 50/50).
    ///   Writes Prototypes/v8/Prototype 1 v8a..v8d.unity.
    /// - v7 (Tools > Last Frame > Build Prototype Blocking v7 (P1, 4 variantes)): four look variations of P1, one scene each:
    ///   v7a pencil and paper, v7b soul vs algorithm, v7c pop dusk, v7d flat TV cartoon. Characters get an ink outline
    ///   and a colored drop shadow. Writes Prototypes/v7/Prototype 1 v7a..v7d.unity. Each look is a LookProfile below.
    /// - v6 (Tools > Last Frame > Build Prototype Blocking v6 (P1, cartoon)): v5 made brighter and more cartoon, on P1 first:
    ///   a warm, lit studio, saturated 80s palette, light lavender cel shadows with a soft rim light, a lighter post
    ///   (more saturation, almost no vignette or grain). Writes Prototypes/v6/Prototype 1 v6.unity.
    /// - v5 (Tools > Last Frame > Build Prototype Blocking v5 (look)): the v4 levels with a new look. A studio at night lit by
    ///   warm practical lamps, purple cel shadows, Flat Kit's screen-space ink outline (the "Look" renderer of URP_LastFrame),
    ///   film grain and vignette, a camera a bit steeper, and Ollie and the enemies as 2D placeholder sprites over the
    ///   working Senses objects. Blue is kept for Ollie, red for the enemies. Writes Prototypes/v5/Prototype N v5.unity.
    /// </summary>
    public static class PrototypeBlockingBuilder
    {
        const string k_rootFolder = "Assets/LastFrame/Scenes/Prototypes";
        static string k_folder => $"{k_rootFolder}/v{s_version}";
        static string sceneName(int _index) => s_version >= 2 ? $"Prototype {_index} v{s_version}{s_variant}" : $"Prototype {_index}";
        static string s_variant = ""; // v7: "a".."d", one look variation each

        // v2 scale: Ollie is a small cartoon in a giant studio
        const float k_playerScaleV2 = 0.7f;
        const float k_propScaleV2 = 1.2f;
        static readonly string[] k_unscaledProps = { "Telón", "Fachada", "Puerta", "Ascensor", "Persiana", "Luz ON AIR", "Ventana", "Paneles", "Cartel", "Disco de oro", "Estrella", "Pizarra" };
        static int s_version = 1;
        static bool s_v2 => s_version >= 2; // v2 and later
        static bool s_v3 => s_version >= 3; // v3 and later
        static bool s_flat => s_version >= 4; // v4 and later: Flat Kit look
        static bool s_v5 => s_version >= 5; // v5: night studio look, screen outline, 2D sprites
        /// <summary>Picks the v1 or v2+ text (names, labels) so every version builds from the same layout.</summary>
        static string T(string _v1, string _v2) => s_v2 ? _v2 : _v1;
        const string k_sourceScene = "Assets/LastFrame/Scenes/TestRoom/Scenes/SensesTestRoom.unity";
        const string k_materialsFolder = "Assets/LastFrame/Art/Materials/Blocking";

        // Sources inside the test room
        const string k_guardSee = "Station 03 · Vista vs Luz (Lumen)/(P) Sentry - Dumy_1_2";
        const string k_guardHear = "Station 11 · Trampas de Ruido y Superficies/(P) Sentry - Dumy_1_1";
        const string k_guardBinoculars = "Station 07 · See Booster: Prismáticos y Gafas/- Example 6.1/(P) Sentry - Dumy_1_1";
        const string k_binoculars = "Station 07 · See Booster: Prismáticos y Gafas/- Example 6.1/Binoculars";
        const string k_guardNvg = "Station 08 · See Booster: Visión Nocturna/- Example 7.2/(P) Sentry Dumy_1_1";
        const string k_nvgHelmet = "Station 08 · See Booster: Visión Nocturna/- Example 7.2/Helmet_NVG";
        const string k_camera = "Station 04 · Zonas de Sigilo (hierba)/(P) CameraPole #1";
        const string k_grassFolder = "Station 04 · Zonas de Sigilo (hierba)/- Enviroment";
        const string k_fishTank = "Station 01 · Vista vs Obstáculos/Example 1.2/FishTank";
        const string k_fishTankBase = "Station 01 · Vista vs Obstáculos/Example 1.2/FishTank Base";
        const string k_dog = "Station 12 · Olfato (rastro y distractores)/- Scent Folower Example";
        const string k_river = "Station 12 · Olfato (rastro y distractores)/- Scent Blockers/-- River - as Scent Blocker";
        const string k_trash1 = "Station 12 · Olfato (rastro y distractores)/- Scent Reducers/-- Trash Bag 1 - as Scent Reducer";
        const string k_trash2 = "Station 12 · Olfato (rastro y distractores)/- Scent Reducers/-- Trash Bag 2 - as Scent Reducer";

        static float k_wallHeight => s_v2 ? 3.75f : 3f;
        const float k_wallThickness = 0.3f;
        const float k_doorWidth = 2.4f;

        enum Kind { See, Hear, NightVision, Binoculars }
        enum Style { Studio, Concrete, Fence }

        static Scene s_source;
        static Transform s_sourceRoot;
        static Dictionary<Object, Object> s_globalMap; // test room player/camera -> this scene's
        static readonly Dictionary<string, Material> s_materials = new Dictionary<string, Material>();
        static Transform s_player;
        static Transform s_walls, s_floors, s_props, s_enemies, s_plan, s_labels, s_lights;

        [MenuItem("Tools/Last Frame/Build Prototype Blocking v1 (archivo)")]
        public static void BuildV1FromMenu() => buildFromMenu(1);

        [MenuItem("Tools/Last Frame/Build Prototype Blocking v2 (archivo)")]
        public static void BuildV2FromMenu() => buildFromMenu(2);

        [MenuItem("Tools/Last Frame/Build Prototype Blocking v3 (archivo)")]
        public static void BuildV3FromMenu() => buildFromMenu(3);

        [MenuItem("Tools/Last Frame/Build Prototype Blocking v4 (archivo)")]
        public static void BuildV4FromMenu() => buildFromMenu(4);

        [MenuItem("Tools/Last Frame/Build Prototype Blocking v10 (abandonado)")]
        public static void BuildV10FromMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            EditorUtility.DisplayDialog("Last Frame", BuildV10(), "OK");
        }

        [MenuItem("Tools/Last Frame/Build Prototype Blocking v9 (P1, estilo GDD)")]
        public static void BuildV9FromMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            EditorUtility.DisplayDialog("Last Frame", BuildV9(), "OK");
        }

        [MenuItem("Tools/Last Frame/Build Prototype Blocking v8 (P1, luz motivada)")]
        public static void BuildV8FromMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            EditorUtility.DisplayDialog("Last Frame", BuildV8(), "OK");
        }

        [MenuItem("Tools/Last Frame/Build Prototype Blocking v7 (P1, 4 variantes)")]
        public static void BuildV7FromMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            EditorUtility.DisplayDialog("Last Frame", BuildV7(), "OK");
        }

        [MenuItem("Tools/Last Frame/Build Prototype Blocking v6 (P1, cartoon)")]
        public static void BuildV6FromMenu() => buildFromMenu(6);

        [MenuItem("Tools/Last Frame/Build Prototype Blocking v5 (look)")]
        public static void BuildV5FromMenu() => buildFromMenu(5);

        static void buildFromMenu(int _version)
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            string _report = build(_version);
            EditorUtility.DisplayDialog("Last Frame", _report, "OK");
        }

        /// <summary>Builds v1 (the archived first pass).</summary>
        public static string BuildAll() => build(1);

        /// <summary>Builds v2 (aligned with the GDD, archived) into Prototypes/v2.</summary>
        public static string BuildV2() => build(2);

        /// <summary>Builds v3 (cumulative mechanics, archived) into Prototypes/v3.</summary>
        public static string BuildV3() => build(3);

        /// <summary>Builds v4 (v3 with the Flat Kit look) into Prototypes/v4.</summary>
        public static string BuildV4() => build(4);

        /// <summary>Builds v5 (night studio look, 2D sprites) into Prototypes/v5.</summary>
        public static string BuildV5() => build(5);

        /// <summary>Builds v6 (brighter cartoon look, P1 only for now) into Prototypes/v6.</summary>
        public static string BuildV6() => build(6);

        /// <summary>Builds the v8 motivated-lighting variants of P1 (v8a..v8d) into Prototypes/v8.</summary>
        /// <summary>Builds v9 (GDD concept-art style on the v8 light plan, P1 only) into Prototypes/v9.</summary>
        /// <summary>Builds v10 (v9 abandoned: windows, projector, dust, concept-art props; P1 only) into Prototypes/v10.</summary>
        public static string BuildV10()
        {
            LastFrameArtSetup.SetupRendering();
            LastFrameArtSetup.BuildPlaceholderSprites();
            LastFrameArtSetup.BuildLightPoolTexture();
            LastFrameArtSetup.BuildWoodPlanks();
            LastFrameArtSetup.BuildWindowCookie();
            return build(10);
        }

        public static string BuildV9()
        {
            LastFrameArtSetup.SetupRendering();
            LastFrameArtSetup.BuildPlaceholderSprites();
            LastFrameArtSetup.BuildLightPoolTexture();
            LastFrameArtSetup.BuildWoodPlanks();
            return build(9);
        }

        public static string BuildV8()
        {
            LastFrameArtSetup.SetupRendering();
            LastFrameArtSetup.BuildPlaceholderSprites();
            LastFrameArtSetup.BuildLightPoolTexture();
            var _log = new System.Text.StringBuilder();
            try
            {
                foreach (string _variant in new[] { "a", "b", "c", "d" })
                {
                    s_variant = _variant;
                    _log.Append(build(8));
                }
            }
            finally { s_variant = ""; }
            return _log.ToString();
        }

        /// <summary>Builds the four v7 look variations of P1 (v7a..v7d) into Prototypes/v7.</summary>
        public static string BuildV7()
        {
            LastFrameArtSetup.SetupRendering();
            LastFrameArtSetup.BuildPlaceholderSprites();
            LastFrameArtSetup.BuildPaperGrain();
            var _log = new System.Text.StringBuilder();
            try
            {
                foreach (string _variant in new[] { "a", "b", "c", "d" })
                {
                    s_variant = _variant;
                    _log.Append(build(7));
                }
            }
            finally { s_variant = ""; }
            return _log.ToString();
        }

        static string build(int _version)
        {
            s_version = _version;
            var _log = new System.Text.StringBuilder();
            if (!AssetDatabase.IsValidFolder(k_folder)) AssetDatabase.CreateFolder(k_rootFolder, $"v{s_version}");
            string _previous = SceneManager.GetActiveScene().path;
            // Never keep open a scene this build is about to overwrite (Unity refuses to save over an open scene).
            if (_previous.Length > 0 && System.IO.Path.GetDirectoryName(_previous).Replace('\\', '/') == k_folder) _previous = "";
            createMaterials();

            EditorSceneManager.OpenScene(_previous.Length > 0 ? _previous : k_sourceScene, OpenSceneMode.Single);
            s_source = EditorSceneManager.OpenScene(k_sourceScene, OpenSceneMode.Additive);
            s_sourceRoot = s_source.GetRootGameObjects().First(g => g.name == "--- STATIONS ---").transform;

            try
            {
                System.Action[] _builders = { buildPrototype1, buildPrototype2, buildPrototype3, buildPrototype4, buildPrototype5 };
                int _count = s_version >= 6 && s_version != 10 ? 1 : _builders.Length; // v6-v9 are tried on P1 only; v10 covers the 5 levels
                for (int i = 0; i < _count; i++)
                {
                    s_index = i + 1;
                    string _path = $"{k_folder}/{sceneName(i + 1)}.unity";
                    Scene _scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
                    SceneManager.SetActiveScene(_scene);
                    s_buildScene = _scene;
                    _builders[i]();
                    if (s_version >= 10 && i > 0) dressLevelV10();
                    if (s_flat) convertToFlatKit(_scene);
                    if (s_v5) applyLookV5();
                    if (s_version >= 10) roomMoodsV10();
                    bakeNavMesh(i + 1);
                    EditorSceneManager.SaveScene(_scene, _path);
                    EditorSceneManager.CloseScene(_scene, true);
                    addToBuildSettings(_path);
                    _log.AppendLine("Saved " + _path);
                }
            }
            finally
            {
                EditorSceneManager.CloseScene(s_source, true); // never saved: the test room stays as it is
                s_version = 1;
            }

            AssetDatabase.SaveAssets();
            Debug.Log("[LastFrame] Prototype blocking\n" + _log);
            return _log.ToString();
        }

        static Vector3 V(float _x, float _z) => new Vector3(_x, 0f, _z);

        // =====================================================================
        // PROTOTYPE 1 · Pasillos del estudio (recepción, animación, guiones, camerinos)
        // Senses: crouch (Stealth), vision guards (SeeSense), low/high cover.
        // Enemies: static melee x2, patrol melee x1.
        // =====================================================================
        static void buildPrototype1()
        {
            setupScene(false, "PROTOTYPE 1 · PASILLOS DEL ESTUDIO", V(0f, 0.5f));

            // ---------- Recepción ----------
            floor(-8f, -2f, 8f, 14f, "Checker");
            floor(-3f, 1f, 3f, 7f, "CarpetRed", 0.012f);
            wallX(-2f, -8f, 8f, Style.Studio);
            wallZ(-8f, -2f, 55f, Style.Studio);
            wallZ(8f, -2f, 42.5f, Style.Studio);
            wallX(14f, -4f, 8f, Style.Studio);
            glassDoors(V(0f, -1.8f), 0f);
            roomLabel("RECEPCIÓN", V(-2.5f, 2f));

            Transform _counter = prop("Mostrador de recepción", V(0f, 9f), 0f);
            box(_counter, V(0f, 0f), new Vector3(6f, 1.1f, 0.9f), "Wood");
            box(_counter, new Vector3(0f, 1.1f, 0f), new Vector3(6.2f, 0.06f, 1f), "Brass", false);
            crt(_counter, new Vector3(-1.8f, 1.16f, 0.1f), 0.5f);
            box(_counter, new Vector3(1.5f, 1.16f, 0f), new Vector3(0.3f, 0.12f, 0.2f), "Red", false);
            chair(V(0f, 10f), 180f);
            if (s_version < 10) enemy("Recepcionista de seguridad", Kind.See, false, false, V(0f, 10.8f), 180f); // v10: the entrance has no guard

            sofa(V(-5.5f, 4.5f), 90f); sofa(V(-5.5f, 8f), 90f);
            sofa(V(5.5f, 4.5f), -90f); sofa(V(5.5f, 8f), -90f);
            table(V(-3.9f, 6.2f), 0f, 0.9f, 1.6f, 0.45f);
            table(V(3.9f, 6.2f), 0f, 0.9f, 1.6f, 0.45f);
            plant(V(-7.3f, -1.3f)); plant(V(7.3f, -1.3f)); plant(V(7.3f, 13.3f));
            waterCooler(V(7.4f, 11f));
            shelf(V(-3.5f, 12.5f), 0f, 3f); // hides the corridor entrance from the receptionist
            poster(V(-6f, 13.8f), 180f, "Yellow"); poster(V(3f, -1.8f), 0f, "Red"); poster(V(-3f, -1.8f), 0f, "Sky");
            elevator(V(4.5f, 13.8f), 180f);
            crate(V(4.2f, 12.6f), 1f, 10f); crate(V(5.6f, 12.8f), 0.8f, -15f);

            // ---------- Pasillo de oficinas (patrulla) ----------
            floor(-8f, 14f, -4f, 47.5f, "CarpetRed");
            wallZ(-4f, 14f, 42.5f, Style.Studio, 21f, 34f);
            poster(V(-7.8f, 20f), 90f, "Sky"); poster(V(-7.8f, 28f), 90f, "Yellow"); poster(V(-7.8f, 38f), 90f, "Red");
            bench(V(-7.4f, 31f), 90f);
            plant(V(-4.7f, 15f));
            roomLabel("PASILLO DE OFICINAS", V(-6f, 24f), 90f);
            enemy("Vigilante del pasillo", Kind.See, true, false, V(-6f, 42f), 180f, V(-6f, 42f), V(-6f, 16f));

            // ---------- Departamento de animación ----------
            floor(-4f, 14f, 8f, 27.5f, "WoodLight");
            wallX(27.5f, -4f, 8f, Style.Studio);
            roomLabel("ANIMACIÓN", V(2f, 15.5f));
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    animationDesk(V(0.5f + i * 4f, 19f + j * 4f), 0f);
                    chair(V(0.5f + i * 4f, 17.9f + j * 4f), 0f);
                }
            }
            shelf(V(7.5f, 21f), 90f, 6f);
            celRack(V(-2.5f, 26.6f), 0f);
            plant(V(-3.3f, 26.8f));
            poster(V(3f, 27.3f), 180f, "Sky");
            memoryTape(V(6.8f, 26.3f));

            // ---------- Sala de guiones (guardia distraído viendo dibujos) ----------
            floor(-4f, 27.5f, 8f, 42.5f, "Checker");
            wallX(42.5f, -4f, 14f, Style.Studio);
            roomLabel("SALA DE GUIONES", V(1.5f, 29f));
            table(V(1.5f, 32.5f), 0f, 5f, 2f, 0.75f);
            for (int i = 0; i < 3; i++) { chair(V(-0.5f + i * 2f, 31f), 0f); chair(V(-0.5f + i * 2f, 34f), 180f); }
            chalkboard(V(1.5f, 42.3f), 180f);
            shelf(V(-3.6f, 38f), 90f, 4f);
            oldTv(V(5.4f, 39.6f), 200f);
            armchair(V(4.4f, 37.2f), 20f);
            Transform _watcher = enemy("Guardia viendo dibujos", Kind.See, false, false, V(4.2f, 36.6f), 20f);
            killMelee(_watcher);

            // ---------- Pasillo de camerinos ----------
            floor(-8f, 42.5f, 14f, 47.5f, "Checker");
            wallZ(14f, 42.5f, 60f, Style.Studio);
            wallX(47.5f, -8f, 14f, Style.Studio, -3f, 3.5f, 10.5f);
            plant(V(13.3f, 43.2f));
            poster(V(1f, 42.7f), 0f, "Yellow"); poster(V(10f, 42.7f), 0f, "Red");
            starSign(V(-3f, 47.3f)); starSign(V(3.5f, 47.3f));
            roomLabel("PASILLO DE CAMERINOS", V(4f, 45f));

            // ---------- Camerinos + acceso al plató ----------
            floor(-8f, 47.5f, 0f, 55f, "CarpetRed");
            floor(0f, 47.5f, 7f, 55f, "WoodLight");
            floor(7f, 47.5f, 14f, 60f, "Concrete");
            wallZ(0f, 47.5f, 55f, Style.Studio);
            wallX(55f, -8f, 7f, Style.Studio);
            wallZ(7f, 47.5f, 60f, Style.Studio);
            wallX(60f, 7f, 14f, Style.Studio, 10.5f);
            vanity(V(-6f, 54.6f), 180f); vanity(V(-3f, 54.6f), 180f);
            costumeRack(V(-7f, 50.5f), 90f);
            sofa(V(-0.8f, 51f), -90f);
            vanity(V(2f, 54.6f), 180f); vanity(V(5f, 54.6f), 180f);
            costumeRack(V(3.5f, 49.2f), 0f);
            crate(V(8.2f, 52f), 1.1f, 0f); crate(V(13f, 56f), 1.2f, 20f); crate(V(13f, 54.6f), 0.9f, 0f);
            roomLabel("CAMERINOS", V(-4f, 49f));
            exitDoor(V(10.5f, 59.8f), "AL PLATÓ B");
            if (s_version >= 10) p1LightsV10();
            else if (s_version >= 8) p1LightsV8();
            else if (s_v5) p1LampsV5();
        }

        // =====================================================================
        // PROTOTYPE 2 · Plató B de noche: el set del bosque encantado
        // Senses: Lumen (studio spotlights expose you), night-vision guard (SeeBooster NVG).
        // Enemies: static ranged (under the spotlights), patrol melee (backstage), static ranged NVG.
        // =====================================================================
        static void buildPrototype2()
        {
            setupScene(true, "PROTOTYPE 2 · PLATÓ B DE NOCHE", V(0f, 0.5f));

            // ---------- Muelle de carga ----------
            floor(-6f, -2f, 6f, 10f, "Concrete");
            wallX(-2f, -6f, 6f, Style.Concrete);
            wallZ(-6f, -2f, 10f, Style.Concrete);
            wallZ(6f, -2f, 10f, Style.Concrete);
            rollUpDoor(V(0f, -1.8f), 0f, 5f);
            roomLabel("MUELLE DE CARGA", V(0f, 3f));
            crate(V(-4.6f, 0.8f), 1.2f, 0f); crate(V(-4.6f, 2.1f), 1.2f, 5f); crate(V(-3.3f, 0.9f), 1f, -10f);
            forklift(V(4f, 3.5f), -30f);
            inkBarrel(V(-5.2f, 8.4f)); inkBarrel(V(-4.4f, 9f)); inkBarrel(V(-5.2f, 9.3f));
            ammo(V(-3.6f, 7.6f));
            workLight(V(4.5f, 8.5f));

            // ---------- Plató B ----------
            floor(-14f, 10f, 14f, 54f, "ConcreteDark");
            wallX(10f, -14f, 14f, Style.Concrete, -1f, 1f);
            wallZ(-14f, 10f, 54f, Style.Concrete);
            wallZ(14f, 10f, 54f, Style.Concrete);
            wallX(54f, -14f, 14f, Style.Concrete, 10f);
            roomLabel("PLATÓ B · SET DEL BOSQUE ENCANTADO", V(0f, 21f));

            // Audience side: bleachers, TV cameras, director
            bleachers(V(-10f, 11.2f), 0f, 6f, 3);
            bleachers(V(9.5f, 11.2f), 0f, 6f, 3);
            tvCamera(V(-2.5f, 17.5f), 0f); tvCamera(V(2.5f, 17.5f), -10f);
            directorChair(V(6.2f, 16f), -20f);
            cable(V(-2.5f, 17f), V(-2.5f, 11f)); cable(V(2.5f, 17f), V(4f, 11f));

            // The set: painted night backdrop, tree flats, cottage, tombstones and a fence
            floor(-9f, 22f, 9f, 42f, "Wood", 0.012f);
            Transform _backdrop = prop("Telón pintado: noche", V(0f, 42.2f), 0f);
            box(_backdrop, V(0f, 0f), new Vector3(18f, 4.5f, 0.15f), "SkyNight");
            cylinder(_backdrop, new Vector3(4f, 3.4f, -0.1f), 1.6f, 0.05f, "Bulb", false, new Vector3(90f, 0f, 0f));
            for (int i = 0; i < 6; i++) sphere(_backdrop, new Vector3(-7f + i * 2.3f, 3.2f + (i % 2) * 0.7f, -0.1f), 0.15f, "Bulb");
            box(_backdrop, new Vector3(-8.5f, 0f, 0.7f), new Vector3(0.1f, 3f, 1.3f), "Wood", false);
            box(_backdrop, new Vector3(8.5f, 0f, 0.7f), new Vector3(0.1f, 3f, 1.3f), "Wood", false);
            treeFlat(V(-7f, 27f), 0f); treeFlat(V(-5f, 34f), 10f); treeFlat(V(-8f, 39f), -5f); treeFlat(V(8f, 27f), 0f); treeFlat(V(7.5f, 40f), 5f);
            cottageFacade(V(4.5f, 36.5f), 180f, 4.5f);
            tombstone(V(-2f, 29f)); tombstone(V(0.3f, 31f)); tombstone(V(2.5f, 28.5f)); tombstone(V(-3.5f, 32.5f));
            picketFence(V(0f, 24f), 0f, 6f);

            // Spotlights light the set; the edges and the backstage stay dark
            spotlightStand(V(-6f, 21f), 25f, true);
            spotlightStand(V(6f, 21f), -25f, true);
            spotlightStand(V(-12.5f, 32f), 90f, true);
            spotlightStand(V(12.5f, 32f), -90f, true);
            lumenProbe(V(-12f, 48f)); lumenProbe(V(12f, 14f)); lumenProbe(V(0f, 32f));

            Transform _actor = enemy("Guardia en el set", Kind.See, false, true, V(0f, 33.5f), 180f);
            killRanged(V(-11.5f, 23.5f), _actor);
            if (s_v2) memoryTape(V(5f, 39.2f)); // v2: behind the cottage, at the back of the lit set (the tape opens the exit)
            if (s_v3)
            {
                // v3: more pressure. A patrol across the audience side, a static watcher on the dark west side
                // (so the safe flank is no longer free, and is a melee lesson) and a patrol on the east flank.
                enemy("Regidor del público", Kind.See, true, false, V(-11f, 19.5f), 90f, V(-11f, 19.5f), V(11f, 19.5f));
                Transform _flank = enemy("Utilero del lateral", Kind.See, false, false, V(-11.5f, 39f), 90f);
                killMelee(_flank);
                enemy("Técnico de luces", Kind.See, true, true, V(12f, 14f), 0f, V(12f, 14f), V(12f, 44f));
            }

            // ---------- Bastidores ----------
            roomLabel("BASTIDORES", V(-4f, 44.2f));
            leaningFlat(V(-12.8f, 46f), 90f, "Sky"); leaningFlat(V(-12.8f, 48.5f), 90f, "Green");
            costumeRack(V(-6f, 52.6f), 0f);
            crate(V(1.5f, 52.5f), 1.2f, 0f); crate(V(2.8f, 52.7f), 1f, 15f);
            inkBarrel(V(-12.8f, 52.6f)); inkBarrel(V(-12f, 53.1f));
            ammo(V(-11.5f, 51f));
            catwalkStairs(V(12f, 44.5f), 0f);
            enemy("Tramoyista vigilante", Kind.See, true, false, V(-10f, 47f), 90f, V(-10f, 47f), V(10f, 47f), V(10f, 51f), V(-10f, 51f));
            Transform _nvg = enemy("Guardia con visión nocturna", Kind.NightVision, false, true, V(6f, 51.8f), 220f);
            killMelee(_nvg);
            exitDoor(V(10f, 53.8f), "A CONTROL");
        }

        // =====================================================================
        // PROTOTYPE 3 · Almacén de atrezo y set del jardín
        // Senses: stealth zones (fake bushes on astroturf), security cameras (cannot be destroyed),
        // Obstacle (the prop aquarium only half blocks the view).
        // Enemies: patrol ranged, static melee (behind the aquarium), static ranged (garden).
        // =====================================================================
        static void buildPrototype3()
        {
            setupScene(false, "PROTOTYPE 3 · ALMACÉN DE ATREZO", V(0f, 0.5f), s_v3); // v3: lights low

            // ---------- Almacén de atrezo ----------
            floor(-12f, -2f, 12f, 26f, "Concrete");
            wallX(-2f, -12f, 12f, Style.Concrete);
            wallZ(-12f, -2f, 26f, Style.Concrete);
            wallZ(12f, -2f, 26f, Style.Concrete);
            wallX(26f, -14f, 14f, Style.Concrete, 6f);
            roomLabel("ALMACÉN DE ATREZO", V(0f, 2f));
            foreach (float _x in new[] { -7.5f, -2.5f, 2.5f, 7.5f })
            {
                shelf(V(_x, 7.25f), 90f, 6.5f);
                shelf(V(_x, 16.75f), 90f, 6.5f);
            }
            cartoonBomb(V(-10f, 1.5f));
            giantPencil(V(1.5f, 22.8f), 0f);
            anvil(V(9.8f, 23.5f));
            costumeRack(V(-10.6f, 23.5f), 90f);
            crate(V(10.6f, 1f), 1.2f, 0f); crate(V(10.6f, 2.4f), 1.2f, 10f); crate(V(9.3f, 0.9f), 1f, -5f);
            ammo(V(10.8f, 8f));
            securityCamera("Cámara del almacén (no se puede destruir)", V(11.6f, -1.6f), -45f);
            enemy("Guardia de inventario", Kind.See, true, true, V(-11f, 12f), 90f, V(-11f, 12f), V(11f, 12f));

            clone("Acuario de atrezo (Obstacle)", s_props, V(-5f, 21f), 0f, k_fishTank, k_fishTankBase);
            Transform _tank = enemy(s_v3 ? "Guardia del acuario con visor nocturno" : "Guardia del acuario", s_v3 ? Kind.NightVision : Kind.See, false, false, V(-5f, 23.2f), 180f); // v3: night vision from P2 again
            killMelee(_tank);
            if (s_v3)
            {
                // v3: a patrol along an aisle crosses the inventory patrol, a second camera covers the way to the
                // garden, and work lights make pools of light between the shelves (light from P2).
                enemy("Mozo de almacén", Kind.See, true, false, V(5f, 4f), 0f, V(5f, 4f), V(5f, 20f));
                securityCamera("Cámara del pasillo norte", V(-11.6f, 25.6f), 135f);
                workLight(V(-10.5f, 12f)); workLight(V(10.5f, 17f)); workLight(V(0f, 24.5f));
                lumenProbe(V(0f, 12f));
            }

            // ---------- Set del jardín ----------
            floor(-14f, 26f, 14f, 58f, "Astro");
            wallZ(-14f, 26f, 58f, Style.Concrete);
            wallZ(14f, 26f, 58f, Style.Concrete);
            wallX(58f, -14f, 14f, Style.Concrete, -11.5f);
            roomLabel("PLATÓ C · SET DEL JARDÍN", V(0f, 28f));

            Transform _sky = prop("Telón pintado: cielo", V(1.5f, 56.5f), 0f);
            box(_sky, V(0f, 0f), new Vector3(23f, 5f, 0.15f), "Sky");
            cylinder(_sky, new Vector3(-6f, 3.5f, -0.1f), 2f, 0.05f, "Yellow", false, new Vector3(90f, 0f, 0f));
            foreach (Vector3 _c in new[] { new Vector3(2f, 3.6f, -0.1f), new Vector3(3f, 3.9f, -0.1f), new Vector3(7f, 3.3f, -0.1f), new Vector3(8f, 3.5f, -0.1f) })
                sphere(_sky, _c, 1.3f, "Paper");

            grassPatch(V(-9f, 31f), 3, 2);
            grassPatch(V(-2f, 35.5f), 3, 2);
            grassPatch(V(7f, 40f), 2, 3);
            grassPatch(V(-8f, 46.5f), 2, 2);
            grassPatch(V(5.5f, 49.5f), 3, 1);
            picketFence(V(-4f, 30f), 0f, 6f);
            picketFence(V(4.5f, 44f), 0f, 5f);
            picketFence(V(-11.5f, 40f), 90f, 5f);
            well(V(-4.5f, 43f));
            cottageFacade(V(9f, 54.5f), 180f, 5f);
            Vector3[] _flowers = { V(-12f, 28f), V(-10f, 36f), V(1f, 31f), V(3f, 32f), V(10f, 30f), V(11f, 35f), V(-1f, 47f), V(1f, 48f), V(-12f, 52f), V(12f, 47f) };
            string[] _colors = { "Red", "Yellow", "Paper", "Sky" };
            for (int i = 0; i < _flowers.Length; i++) flower(_flowers[i], _colors[i % _colors.Length]);
            securityCamera("Cámara del plató (no se puede destruir)", V(13.6f, 41f), -90f);
            if (s_v3)
            {
                // v3: the bushes earn their place. Two more cameras overlap with the first one, a patrol loops
                // between the bushes, a watcher guards the tape at the well, and stage spotlights light the set.
                securityCamera("Cámara del jardín oeste", V(-13.6f, 33f), 90f);
                securityCamera("Cámara del telón", V(-6f, 55.8f), 160f);
                enemy("Jardinero ayudante", Kind.See, true, false, V(-10f, 38f), 90f, V(-10f, 38f), V(8f, 36f), V(10f, 47f), V(-6f, 50f));
                Transform _wellWatcher = enemy("Vigía del pozo", Kind.See, false, false, V(-1.8f, 44.6f), 90f);
                killMelee(_wellWatcher);
                spotlightStand(V(-12.5f, 50f), 90f, true);
                spotlightStand(V(12.5f, 34f), -90f, true);
                lumenProbe(V(-8f, 50f)); lumenProbe(V(8f, 34f));
            }
            Transform _gardener = enemy("Jardinero vigilante", Kind.See, false, true, V(0f, 52f), 180f);
            killRanged(V(7f, 40f), _gardener);
            memoryTape(s_v2 ? V(-4.5f, 44.6f) : V(-12.8f, 56.8f)); // v2: at the well, in the gardener's view
            exitDoor(V(-11.5f, 57.8f), "A SONIDO");
        }

        // =====================================================================
        // PROTOTYPE 4 · Estudio de sonido: foley, control y grabación
        // Senses: hearing guards (HearSense), wood x1.5 / carpet x0.5, noise traps (cans), run vs crouch.
        // Enemies: static melee hearing (foley), static melee hearing (control), patrol ranged, static ranged.
        // =====================================================================
        static void buildPrototype4()
        {
            setupScene(false, "PROTOTYPE 4 · ESTUDIO DE SONIDO", V(0f, 0.5f), s_v3); // v3: lights low

            // ---------- Pasillo de grabación ----------
            floor(-3f, -2f, 3f, 10f, "CarpetRed");
            wallX(-2f, -3f, 3f, Style.Studio);
            wallZ(-3f, -2f, 10f, Style.Studio);
            wallZ(3f, -2f, 10f, Style.Studio);
            onAirSign(V(0f, 9.8f), 180f);
            recordPoster(V(-2.8f, 3f), 90f); recordPoster(V(-2.8f, 6f), 90f); recordPoster(V(2.8f, 4.5f), -90f);
            bench(V(2.4f, 7.5f), 90f);

            // ---------- Sala de foley ----------
            floor(-12f, 10f, 12f, 30f, "ConcreteDark");
            wallX(10f, -12f, 12f, Style.Studio, 0f);
            wallZ(-12f, 10f, 52f, Style.Studio);
            wallZ(12f, 10f, 52f, Style.Studio);
            wallX(30f, -12f, 12f, Style.Studio, -8f, 9f);
            roomLabel("SALA DE FOLEY", V(-3f, 11.5f));
            if (s_v3) foleyRoomV3();
            else
            {
                woodFloor(V(0f, 20f), new Vector2(12f, 16f));
                carpet(V(-9.5f, 20f), new Vector2(3f, 17f));
                gravelPit(V(9f, 14f), "Concrete"); gravelPit(V(9f, 19f), "Yellow");
                waterTub(V(9f, 24.5f));
                foleyDoor(V(4.5f, 27.5f), 0f);
                Transform _props = prop("Mesa de efectos", V(-3.5f, 27.5f), 0f);
                box(_props, V(0f, 0f), new Vector3(2.4f, 0.8f, 1f), "Wood");
                for (int i = 0; i < 4; i++) sphere(_props, new Vector3(-0.8f + i * 0.5f, 0.92f, 0f), 0.22f, i % 2 == 0 ? "Wood" : "Metal");
                for (int i = 0; i < 6; i++) can(new Vector3(-4f + (i % 3) * 3.6f, 0f, 14f + (i / 3) * 6.5f + (i % 2)), i);
                enemy("Técnico de foley", Kind.Hear, false, false, V(0f, 21.5f), 180f);
            }

            // ---------- Sala de control ----------
            floor(-12f, 30f, 1f, 44f, "Checker");
            wallZ(1f, 30f, 52f, Style.Studio, 41f);
            wallX(44f, -12f, 1f, Style.Studio, -5f);
            studioWindow(V(1f, 36f), 90f, 5f);
            roomLabel("CONTROL", V(-7f, 31.5f));
            mixingConsole(V(-2.5f, 37f), 90f, 4f);
            chair(V(-4.1f, 36.2f), 90f);
            crtWall(V(-11.6f, 37f), 90f, 3);
            tapeMachine(V(-9f, 43.5f), 180f); tapeMachine(V(-7f, 43.5f), 180f);
            ammo(V(-11f, 31.5f));
            Transform _engineer = enemy("Ingeniero de sonido", Kind.Hear, false, false, V(-4f, 37.8f), 90f);
            killMelee(_engineer);

            // ---------- Archivo de discos ----------
            floor(-12f, 44f, 1f, 52f, "WoodLight");
            wallX(52f, -12f, 12f, Style.Studio, 6.5f);
            roomLabel("DISCOTECA", V(-6f, 45.3f));
            shelf(V(-7f, 47.5f), 0f, 7f); shelf(V(-7f, 50.5f), 0f, 7f);
            memoryTape(V(-0.5f, 51.2f));

            // ---------- Estudio de grabación ----------
            carpet(V(6.5f, 41f), new Vector2(10.6f, 21.6f));
            roomLabel("ESTUDIO A · GRABANDO", V(6.5f, 31.2f));
            acousticPanels(V(11.8f, 41f), -90f, 8);
            drumKit(V(9f, 48f));
            piano(V(3.2f, 47.5f), 90f);
            micStand(V(5f, 44f)); micStand(V(7f, 46f)); micStand(V(5.5f, 37f));
            amp(V(10.8f, 44.5f), -90f); amp(V(10.8f, 51f), 180f);
            Transform _producer = s_v3
                ? enemy("Productor con visor nocturno", Kind.NightVision, false, true, V(8.5f, 43f), 200f) // v3: night vision again in the dim studio
                : enemy("Productor vigilante", Kind.See, false, true, V(8.5f, 43f), 200f);
            if (s_v3)
            {
                // v3: half cover (from P3) and a stage spotlight (light from P2) in the recording studio.
                clone("Pecera del productor (tapa a medias, Obstacle)", s_props, V(11.1f, 34.5f), 90f, k_fishTank, k_fishTankBase);
                spotlightStand(V(1.8f, 50.6f), 150f, true);
                lumenProbe(V(6.5f, 46f));
            }
            enemy("Músico nervioso", Kind.See, true, true, V(3f, 33f), 90f, V(3f, 33f), V(10.5f, 33f), V(10.5f, 38.5f), V(3f, 38.5f));
            killRanged(V(2.2f, 41f), _producer);
            exitDoor(V(6.5f, 51.8f), "AL BACKLOT");
        }

        /// <summary>
        /// v3 foley room (x -12..12, z 10..30): three lanes split by low booth partitions (1.2 m, they hide a
        /// crouching Ollie), joined by one gap in the middle. Each lane trades noise, light and watchers:
        /// - West, quiet: carpet (x0.5) but film cans as traps, a basket stealth zone and a camera watching it.
        /// - Centre, loud: wood (x1.5) under a recording spotlight, with the foley technician listening.
        /// - East, normal floor: foley pits and a water tub as cover, and an otter patrolling it by ear.
        /// The west lane leads to the control door (-8, 30), the east lane to studio A (9, 30).
        /// </summary>
        static void foleyRoomV3()
        {
            // Booth partitions at x = -5 and x = 5, from z 13 to 27, with a crossing gap at z 19.5..21.5
            Transform _booths = prop("Cabinas de foley (tabiques bajos)", Vector3.zero, 0f);
            _booths.localScale = Vector3.one;
            foreach (float _x in new[] { -5f, 5f })
            {
                box(_booths, new Vector3(_x, 0f, 16.25f), new Vector3(0.2f, 1.2f, 6.5f), "Wood");
                box(_booths, new Vector3(_x, 0f, 24.25f), new Vector3(0.2f, 1.2f, 5.5f), "Wood");
            }

            // West lane: quiet carpet, but traps, a stealth zone and a camera
            carpet(V(-8.5f, 20f), new Vector2(6.6f, 16f));
            foreach (Vector3 _c in new[] { V(-9.5f, 15.5f), V(-7f, 18.5f), V(-10f, 23f), V(-7.5f, 26f) }) can(_c, (int)(_c.z * 3f));
            grassPatch(V(-10.3f, 20.5f), 1, 2, "Cestas de material de foley");
            foleyDoor(V(-6.2f, 27.6f), 90f);
            securityCamera("Cámara de foley", V(-11.6f, 10.4f), 30f);
            label("CARRIL SILENCIOSO · ALFOMBRA x0,5 · LATAS · CÁMARA", V(-8.5f, 13.2f), 0.3f, new Color(0.95f, 0.9f, 0.8f), 6f);

            // Centre lane: loud wood under a spotlight, the technician listens
            woodFloor(V(0f, 20f), new Vector2(9.6f, 17f));
            spotlightStand(V(0f, 28.9f), 180f, true);
            lumenProbe(V(0f, 18f));
            Transform _props = prop("Mesa de efectos", V(-2.8f, 27.8f), 0f);
            box(_props, V(0f, 0f), new Vector3(2.4f, 0.8f, 1f), "Wood");
            for (int i = 0; i < 4; i++) sphere(_props, new Vector3(-0.8f + i * 0.5f, 0.92f, 0f), 0.22f, i % 2 == 0 ? "Wood" : "Metal");
            Transform _tech = enemy("Técnico de foley", Kind.Hear, false, false, V(0f, 23.5f), 180f);
            killMelee(_tech);
            label("CARRIL DE MADERA · RUIDO x1,5 · FOCO", V(0f, 14f), 0.3f, new Color(0.95f, 0.9f, 0.8f), 6f);

            // East lane: normal floor, cover, a patrol that hears
            gravelPit(V(10.6f, 14.5f), "Concrete"); gravelPit(V(10.6f, 22f), "Yellow");
            waterTub(V(7.2f, 17.5f));
            Transform _shelf = prop("Estante de efectos de foley", V(10.4f, 26.5f), 0f);
            box(_shelf, V(0f, 0f), new Vector3(2.2f, 1.4f, 0.8f), "Wood");
            enemy("Ayudante de foley", Kind.Hear, true, false, V(8.5f, 12f), 0f, V(8.5f, 12f), V(8.5f, 28f));
            label("CARRIL NORMAL · COBERTURA · PATRULLA QUE OYE", V(8.5f, 13.2f), 0.3f, new Color(0.95f, 0.9f, 0.8f), 6f);
        }

        // =====================================================================
        // PROTOTYPE 5 · Backlot exterior y archivo de cintas
        // Senses: scent dog (SmellSense), river set (cuts the trail), trash (masks it),
        // binocular guard (SeeBooster, long range). Combines everything.
        // Enemies: dog (patrol), patrol melee (street), static ranged binoculars, static melee hearing.
        // =====================================================================
        static void buildPrototype5()
        {
            setupScene(s_v3, "PROTOTYPE 5 · BACKLOT Y ARCHIVO", V(0f, 0.5f)); // v3: at night, so the light mechanic comes back

            // ---------- Patio trasero ----------
            floor(-14f, -2f, 14f, 33f, "Dirt");
            wallX(-2f, -14f, 14f, Style.Fence);
            wallZ(-14f, -2f, 54f, Style.Fence);
            wallZ(14f, -2f, 54f, Style.Fence);
            roomLabel("PATIO TRASERO", V(0f, 3f));
            trailer(V(-9.5f, 5f), 90f);
            truck(V(8f, 7f), 10f);
            dumpster(V(-12.4f, 12f), 90f);
            clone(T("Basura 1 (enmascara el olor)", "Papelera de bocetos 1 (tapa el rastro)"), s_props, V(-11.4f, 15f), 90f, k_trash1);
            clone(T("Basura 2 (enmascara el olor)", "Papelera de bocetos 2 (tapa el rastro)"), s_props, V(-11.4f, 16.8f), 90f, k_trash2);
            waterTower(V(11f, 15.5f));
            leaningFlat(V(-13.2f, 18.5f), 90f, "WallCream"); leaningFlat(V(-13.2f, 21f), 90f, "Red");
            crate(V(8f, 19f), 1.2f, 0f); crate(V(9.3f, 19.3f), 1f, 20f);
            lampPost(V(-3f, 22f)); lampPost(V(4f, 11f));
            grassPatch(V(-4.5f, 14f), 2, 2);

            Transform _dog = clone(T("Perro guardián (SmellSense)", "Sabueso IA (sigue tu rastro, SmellSense)"), s_enemies, V(0f, 16f), 0f, k_dog);
            placeDogWaypoints(_dog, new[] { V(-6f, 9f), V(0f, 7f), V(5f, 13f), V(4f, 20f), V(-2f, 22f), V(-7f, 18f) });
            enemyTag(_dog.GetComponentInChildren<ScentFollower>(true).transform.position, T("PERRO · PATRULLA · SIGUE EL RASTRO", "SABUESO IA · PATRULLA · SIGUE TU RASTRO"), true);
            patrolRoute(new[] { V(-6f, 9f), V(0f, 7f), V(5f, 13f), V(4f, 20f), V(-2f, 22f), V(-7f, 18f) });

            Transform _river = clone(T("Set del río (corta el rastro)", "Río de utilería (el agua lava el rastro)"), s_props, new Vector3(0f, 0.03f, 28f), 67f, k_river); // rotated so the river crosses the yard roughly straight
            _ = _river;
            for (int i = 0; i < 8; i++) reeds(V(-12f + i * 3.3f, i % 2 == 0 ? 22f : 30f));
            roomLabel("SET DEL RÍO", V(-9f, 31f));

            // ---------- Calle de fachadas ----------
            floor(-14f, 33f, 14f, 54f, "Concrete");
            roomLabel("CALLE DEL BACKLOT", V(0f, 34.5f));
            string[] _westMats = { "WallCream", "Red", "Sky" }, _eastMats = { "Yellow", "WallCream", "Red" };
            for (int i = 0; i < 3; i++)
            {
                facade(V(-11f, 37f + i * 6.5f), 90f, 6f, 5f, _westMats[i]);
                facade(V(11f, 37f + i * 6.5f), -90f, 6f, 5f, _eastMats[i]);
            }
            grassPatch(V(-8.6f, 40f), 2, 2);
            grassPatch(V(8.6f, 46.5f), 2, 2);
            grassPatch(V(-8.6f, 50f), 2, 1);
            lampPost(V(-7f, 35f)); lampPost(V(7f, 52f));
            securityCamera("Cámara de la calle (no se puede destruir)", V(5f, 53.4f), 200f);
            enemy("Guardia del backlot", Kind.See, true, false, V(0f, 36f), 0f, V(0f, 36f), V(0f, 51f));
            ammo(V(-12.8f, 44f)); // in the alley behind the facades

            // ---------- Archivo de cintas ----------
            floor(-14f, 54f, 14f, 72f, "Checker");
            wallX(54f, -14f, 14f, Style.Studio, 0f);
            wallZ(-14f, 54f, 72f, Style.Studio);
            wallZ(14f, 54f, 72f, Style.Studio);
            wallX(72f, -14f, 14f, Style.Studio);
            roomLabel("ARCHIVO DE CINTAS", V(-7f, 55.5f));
            foreach (float _z in new[] { 58f, 61.5f, 65f })
            {
                shelf(V(-6f, _z), 0f, 8f);
                shelf(V(6f, _z), 0f, 8f);
            }
            woodFloor(V(12f, 63f), new Vector2(3.6f, 14f));
            desk(V(-12f, 56.6f), 90f, true);
            vaultDoor(V(0f, 71.8f), 180f);
            Transform _archivist = enemy("Archivero con prismáticos", Kind.Binoculars, false, true, V(0f, 69f), 180f);
            killMelee(_archivist);
            Transform _keeper = enemy("Guardia del archivo", Kind.Hear, false, false, V(12f, 68.5f), 180f);
            killRanged(V(12f, 57.5f), _keeper);
            memoryTape(V(-12.6f, 70.6f));

            if (s_v3)
            {
                // v3: the backlot is at night. Lamp posts (lit in lampPost) and two floodlights make pools of light
                // in the yard and the street (light from P2), with probes so Senses can read them.
                spotlightStand(V(-12.5f, 1f), 45f, true);
                spotlightStand(V(12.5f, 22f), -120f, true);
                lumenProbe(V(-6f, 10f)); lumenProbe(V(6f, 20f)); lumenProbe(V(0f, 44f));
                // More cameras (from P3): one over the yard, one on the water tower, one inside the archive
                securityCamera("Cámara del patio", V(13.6f, 0.5f), -45f);
                securityCamera("Cámara del depósito de agua", V(9.4f, 14f), 230f);
                securityCamera("Cámara del archivo", V(-13.6f, 56f), 60f);
                // Night vision (from P2): a watcher looks across the river from the street side
                Transform _riverWatch = enemy("Vigía del río con visor nocturno", Kind.NightVision, false, true, V(9f, 33.8f), 180f);
                killRanged(V(-4f, 24f), _riverWatch);
                // The archive is lit inside, the yard is not
                foreach (Vector3 _lamp in new[] { V(-6f, 63f), V(6f, 63f), V(0f, 57.5f) })
                {
                    Transform _l = prop("Lámpara del archivo", _lamp, 0f);
                    pointLight(_l, new Vector3(0f, 3.3f, 0f), new Color(1f, 0.92f, 0.75f), 8f, 9f);
                }
                // Half cover (from P3) and noise traps (from P4) in the archive
                clone("Vitrina de cintas (tapa a medias, Obstacle)", s_props, V(-12.3f, 63f), 90f, k_fishTank, k_fishTankBase);
                can(V(10.3f, 60.5f), 1); can(V(12.6f, 64.5f), 2); can(V(9.8f, 66.8f), 3);
            }
            exitPad(V(0f, 70.8f), "LA ÚLTIMA CINTA");
        }

        // =====================================================================
        // Scene setup
        // =====================================================================

        /// <param name="_dim">v3: a closed studio with the lights low, so spotlights and work lights make light pools.</param>
        static void setupScene(bool _night, string _title, Vector3 _start, bool _dim = false)
        {
            s_globalMap = new Dictionary<Object, Object>();
            if (s_v2) _title = System.Text.RegularExpressions.Regex.Replace(_title, @"^PROTOTYPE (\d)", $"PROTOTYPE $1 v{s_version}{s_variant}");
            Transform _sourceSystems = s_source.GetRootGameObjects().First(g => g.name == "--- TEST ROOM ---").transform;

            var _managers = new GameObject("--- Managers ---").transform;
            var _env = new GameObject("--- Environment ---").transform;
            s_floors = child(_env, "Floors");
            s_walls = child(_env, "Walls");
            s_props = child(_env, "Props");
            var _gameplay = new GameObject("--- Gameplay ---").transform;
            s_enemies = child(_gameplay, "Enemies");
            var _lighting = new GameObject("--- Lighting ---").transform;
            s_lights = child(_lighting, "Studio Lights");
            s_plan = new GameObject("--- Plan (ocultable) ---").transform;
            s_labels = child(s_plan, "Labels");

            // Lighting
            var _sunGo = new GameObject("Directional Light");
            _sunGo.transform.SetParent(_lighting);
            _sunGo.transform.rotation = Quaternion.Euler(50f, 330f, 0f);
            var _sun = _sunGo.AddComponent<Light>();
            _sun.type = LightType.Directional;
            _sun.shadows = LightShadows.Soft;
            _sun.intensity = _night ? 0.05f : _dim ? 0.35f : 2.2f;
            _sun.color = _night ? new Color(0.64f, 0.86f, 0.96f) : new Color(1f, 0.94f, 0.8f);
            _sunGo.AddComponent<UniversalAdditionalLightData>();
            RenderSettings.sun = _sun;
            RenderSettings.skybox = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Skybox.mat");
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;
            if (_night)
            {
                RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
                RenderSettings.ambientLight = new Color(0.14f, 0.16f, 0.26f);
                RenderSettings.skybox = null;
                RenderSettings.defaultReflectionMode = UnityEngine.Rendering.DefaultReflectionMode.Custom;
            }
            else if (_dim)
            {
                RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
                RenderSettings.ambientLight = new Color(0.3f, 0.29f, 0.34f);
                RenderSettings.skybox = null;
            }

            if (s_v5)
            {
                // A studio at night: cool, dim moonlight and a dark purple ambient; practical lamps do the rest.
                // Levels that teach light (night / dim) stay darker so their light pools still matter.
                _sun.intensity = _night ? s_look.SunNight : _dim ? s_look.SunDim : s_look.SunDay;
                _sun.color = s_look.SunColor;
                // v10: Flat Kit paints whatever faces the main light in full base color, however weak the light is.
                // Lit from below, every surface rests on its shaded side: only lamps and windows bring color back.
                if (s_version >= 10) _sunGo.transform.rotation = Quaternion.Euler(-55f, 330f, 0f);
                RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
                RenderSettings.ambientLight = _night ? s_look.AmbientNight : _dim ? s_look.AmbientDim : s_look.AmbientDay;
                RenderSettings.skybox = null;
            }

            Transform _volume = _sourceSystems.Find("Global Volume");
            if (s_v5) lookVolumeV5(_lighting);
            else if (_volume != null)
            {
                GameObject _v = Object.Instantiate(_volume.gameObject, _lighting);
                _v.name = "Global Volume";
            }

            // Managers
            var _factions = new GameObject("SceneFactionManager");
            _factions.transform.SetParent(_managers);
            _factions.AddComponent<SceneFactionManager>().FactionSettings =
                Resources.Load<FactionSettings>("FactionSettings (SO)/DefaultFactionSettings");

            // Camera (same follow camera as the test room / P scenes)
            var _camGo = new GameObject("Main Camera") { tag = "MainCamera" };
            _camGo.transform.SetParent(_gameplay);
            var _cam = _camGo.AddComponent<Camera>();
            _cam.fieldOfView = 50f;
            _cam.farClipPlane = 300f;
            _cam.clearFlags = CameraClearFlags.SolidColor;
            _cam.backgroundColor = _night ? new Color(0.02f, 0.03f, 0.06f) : new Color32(0x25, 0x1E, 0x23, 0xFF);
            _camGo.AddComponent<AudioListener>();
            _camGo.AddComponent<UniversalAdditionalCameraData>();
            var _follow = _camGo.AddComponent<TestRoomCamera>();
            Transform _sourceCam = _sourceSystems.Find("Main Camera");
            if (_sourceCam != null) s_globalMap[_sourceCam] = _camGo.transform;

            // Player (the test room one: Stealth with Lumen/Zoner add-ons, Noise, Scent, faction 1)
            GameObject _sourcePlayer = s_source.GetRootGameObjects().First(g => g.name == "Player");
            GameObject _player = Object.Instantiate(_sourcePlayer, _start + Vector3.up * 0.3f, Quaternion.identity, _gameplay);
            _player.name = s_v2 ? "Player (Ollie)" : "Player";
            s_player = _player.transform;
            if (s_version >= 10)
            {
                // GDD: Ollie walks, runs and crouches; she does not jump. The Senses test player jumps 1.2 m and steps 0.3,
                // which let her climb onto furniture. Low step (only floor seams) and a 30° slope limit (no riding up
                // round props). Collider kept to Ollie's sprite: 1.4 m tall, 0.35 m radius (local values x0.7).
                var _cc = _player.GetComponent<CharacterController>();
                if (_cc != null) { _cc.stepOffset = 0.08f; _cc.slopeLimit = 30f; _cc.height = 2f; _cc.radius = 0.5f; _cc.center = new Vector3(0f, 1f, 0f); }
                var _mover = _player.GetComponent<PlayerMover>();
                if (_mover != null) _mover.JumpHeight = 0f;
            }
            if (s_v2) _player.transform.localScale = Vector3.one * k_playerScaleV2; // Ollie is smaller than the world
            addHierarchyMap(_sourcePlayer, _player, s_globalMap);
            remap(_player, s_globalMap);
            _follow.Target = _player.transform;
            if (s_v5)
            {
                // Steeper camera (GDD: high angle, not fully top-down) and the "Look" renderer (screen ink outline)
                _follow.Pitch = 57f;
                _follow.Distance = 14f;
                _cam.backgroundColor = s_look.Background;
                var _camData = _camGo.GetComponent<UniversalAdditionalCameraData>();
                _camData.SetRenderer(LastFrameArtSetup.LookRendererIndex(s_look.RendererName));
                if (s_version >= 10) _camData.volumeTrigger = _player.transform; // room volumes follow Ollie, not the high camera
                _camData.renderPostProcessing = true;
            }
            _follow.Snap();

            // Ground (layer 2 + tag Ground, like the Senses demos: the PlayerMover ground check uses it)
            var _ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            _ground.name = "Ground";
            _ground.tag = "Ground";
            _ground.layer = 2;
            _ground.isStatic = true;
            _ground.transform.SetParent(_env);
            _ground.transform.position = new Vector3(0f, 0f, 36f);
            _ground.transform.localScale = new Vector3(4f, 1f, 9f);
            _ground.GetComponent<MeshRenderer>().sharedMaterial = s_materials["Ink"];

            // Start + level title (plan only)
            planBox("Start", _start, new Vector3(2f, 0.02f, 2f), "Start");
            label(_title, _start + new Vector3(0f, 0f, -1.6f), 0.55f, new Color(1f, 0.85f, 0.35f), 16f);
        }

        static Transform child(Transform _parent, string _name)
        {
            var _t = new GameObject(_name).transform;
            _t.SetParent(_parent);
            return _t;
        }

        // =====================================================================
        // Enemies (Senses guards cloned from the test room) + plan markers
        // =====================================================================

        /// <summary>
        /// Shy enemies stay put (static); the others patrol a route. Both carry a melee or a ranged weapon.
        /// The guard itself is the working Senses sentry; behaviour, weapon and route are drawn in the plan.
        /// </summary>
        static Transform enemy(string _name, Kind _kind, bool _patrol, bool _ranged, Vector3 _pos, float _yaw, params Vector3[] _route)
        {
            // v2: the enemies are AI cartoons. The design sets the behaviour: the otter patrols, the capybara stays put.
            string _design = _patrol ? "Nutria IA" : "Capibara IA";
            if (s_v2) _name = $"{_design} · {_name}";
            Transform _g;
            switch (_kind)
            {
                case Kind.Hear: _g = clone(_name, s_enemies, _pos, _yaw, k_guardHear); break;
                case Kind.NightVision: _g = clone(_name, s_enemies, _pos, _yaw, k_guardNvg, k_nvgHelmet); break;
                case Kind.Binoculars: _g = clone(_name, s_enemies, _pos, _yaw, k_guardBinoculars, k_binoculars); break;
                default: _g = clone(_name, s_enemies, _pos, _yaw, k_guardSee); break;
            }

            if (s_v2) aiCartoonGear(_g, _patrol, _ranged);
            else
            {
                // Weapon placeholder in the right hand
                var _weapon = new GameObject(_ranged ? "Arma: pistola de tinta" : "Arma: porra").transform;
                _weapon.SetParent(_g, false);
                if (_ranged)
                {
                    box(_weapon, new Vector3(0.38f, 1.0f, 0.3f), new Vector3(0.12f, 0.14f, 0.42f), "InkPurple", false);
                    cylinder(_weapon, new Vector3(0.38f, 1.2f, 0.2f), 0.14f, 0.18f, "Brass", false);
                }
                else
                {
                    cylinder(_weapon, new Vector3(0.4f, 1.0f, 0.3f), 0.08f, 0.7f, "Wood", false, new Vector3(60f, 0f, 0f));
                }
            }

            string _sense = _kind == Kind.Hear ? "OÍDO"
                : _kind == Kind.NightVision ? T("VISIÓN NOCTURNA", "VISOR NOCTURNO")
                : _kind == Kind.Binoculars ? T("PRISMÁTICOS", "TELEOBJETIVO") : "VISTA";
            string _tag = s_v2
                ? $"{_design.ToUpper()} · {(_patrol ? "PATRULLA" : "ESTÁTICO")} · {(_ranged ? "DISTANCIA (LANZA GOMAS)" : "MELÉ (BORRADOR)")} · {_sense}"
                : $"{(_patrol ? "PATRULLA" : "TÍMIDO · ESTÁTICO")} · {(_ranged ? "DISTANCIA" : "MELÉ")} · {_sense}";
            enemyTag(_pos, _tag, _patrol);
            if (_patrol && _route != null && _route.Length > 1) patrolRoute(_route);
            return _g;
        }

        /// <summary>
        /// v2 stand-ins for the AI cartoon designs (final sprites replace them): a glossy ring over the head
        /// (otter = cyan, capybara = yellow) and the weapon in the right hand. No firearms (GDD): melee is a giant
        /// eraser, ranged is a slingshot that launches erasers.
        /// </summary>
        static void aiCartoonGear(Transform _guard, bool _patrol, bool _ranged)
        {
            var _design = new GameObject(_patrol ? "Diseño: Nutria IA (placeholder)" : "Diseño: Capibara IA (placeholder)").transform;
            _design.SetParent(_guard, false);
            cylinder(_design, new Vector3(0f, 2.15f, 0f), 0.7f, 0.06f, _patrol ? "Patrol" : "Yellow", false);
            sphere(_design, new Vector3(0f, 2.3f, 0f), 0.22f, "Screen");

            var _weapon = new GameObject(_ranged ? "Arma: lanza gomas (distancia)" : "Arma: borrador gigante (melé)").transform;
            _weapon.SetParent(_guard, false);
            if (_ranged)
            {
                cylinder(_weapon, new Vector3(0.4f, 0.85f, 0.3f), 0.07f, 0.35f, "Wood", false);
                cylinder(_weapon, new Vector3(0.3f, 1.25f, 0.3f), 0.06f, 0.3f, "Wood", false, new Vector3(0f, 0f, 25f));
                cylinder(_weapon, new Vector3(0.5f, 1.25f, 0.3f), 0.06f, 0.3f, "Wood", false, new Vector3(0f, 0f, -25f));
                box(_weapon, new Vector3(0.4f, 1.3f, 0.2f), new Vector3(0.16f, 0.1f, 0.1f), "Red", false);
            }
            else
            {
                box(_weapon, new Vector3(0.42f, 0.9f, 0.35f), new Vector3(0.22f, 0.5f, 0.32f), "Red", false);
                box(_weapon, new Vector3(0.42f, 1.15f, 0.35f), new Vector3(0.23f, 0.22f, 0.33f), "Sky", false);
            }
        }

        static void enemyTag(Vector3 _pos, string _text, bool _patrol)
        {
            planDisc("Enemigo (" + (_patrol ? "patrulla" : "estático") + ")", _pos, 1.1f, _patrol ? "Patrol" : "Static");
            label(_text, _pos + new Vector3(0f, 0f, -0.95f), 0.3f, _patrol ? new Color(0.31f, 0.76f, 0.85f) : new Color(0.95f, 0.9f, 0.8f), 7f);
        }

        static void patrolRoute(Vector3[] _points)
        {
            var _root = child(s_plan, "Ruta de patrulla");
            for (int i = 0; i < _points.Length; i++)
            {
                var _post = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                _post.name = "Waypoint " + (i + 1);
                _post.transform.SetParent(_root);
                _post.transform.position = _points[i] + Vector3.up * 0.25f;
                _post.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
                setupPlanRenderer(_post, "Patrol");

                bool _loop = _points.Length > 2;
                if (i == _points.Length - 1 && !_loop) break;
                Vector3 _a = _points[i], _b = _points[(i + 1) % _points.Length];
                line(_root, _a, _b, 0.08f, "Patrol");
            }
        }

        static Transform securityCamera(string _name, Vector3 _pos, float _yaw)
        {
            Transform _c = clone(_name, s_enemies, _pos, _yaw, k_camera);
            label(T("CÁMARA · NO SE PUEDE DESTRUIR", "CÁMARA · ALARMA AL ENEMIGO MÁS CERCANO · LA TINTA LA CIEGA 2-3 s"), _pos + new Vector3(0f, 0f, -1f), 0.3f, new Color(0.95f, 0.9f, 0.8f), 7f);
            return _c;
        }

        static void killMelee(Transform _guard)
        {
            Vector3 _behind = _guard.position - _guard.forward * 1.4f;
            planDisc("KILL · Melé por detrás", _behind, 1.2f, "KillMelee");
            label(T("TU MELÉ POR DETRÁS (Q)", "TU CORRECTOR POR DETRÁS, EN SIGILO (Q)"), _behind + new Vector3(0f, 0f, -0.9f), 0.32f, new Color(0.82f, 0.39f, 0.38f), 7f);
        }

        static void killRanged(Vector3 _from, Transform _target)
        {
            planDisc("KILL · Disparo (posición)", _from, 1.1f, "KillRanged");
            line(s_plan, _from, _target.position, 0.1f, "KillRanged").name = "KILL · Disparo (línea de tiro)";
            label(T("TU DISPARO (clic izq., 1 tinta)", "TU PLUMA DE TINTA (clic izq., 1 tinta)"), _from + new Vector3(0f, 0f, -0.9f), 0.32f, new Color(0.91f, 0.59f, 0.29f), 7f);
        }

        static void ammo(Vector3 _pos)
        {
            var _go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            _go.name = T("PLAN · Bote de tinta +1", "PLAN · Tintero +1");
            _go.transform.SetParent(s_plan);
            _go.transform.position = _pos + Vector3.up * 0.3f;
            _go.transform.localScale = new Vector3(0.35f, 0.3f, 0.35f);
            setupPlanRenderer(_go, "Ammo");
            label(T("+1 TINTA", "TINTERO +1"), _pos + new Vector3(0f, 0f, -0.7f), 0.3f, new Color(0.61f, 0.42f, 0.83f), 4f);
        }

        static void memoryTape(Vector3 _pos)
        {
            planBox("PLAN · Cinta de memoria", _pos + Vector3.up * 0.3f, new Vector3(0.6f, 0.2f, 0.4f), "Memory");
            label(T("CINTA DE MEMORIA", "CINTA (OBLIGATORIA: ABRE LA SALIDA)"), _pos + new Vector3(0f, 0f, -0.7f), 0.3f, new Color(0.89f, 0.74f, 0.45f), 5f);
        }

        static void exitDoor(Vector3 _doorPos, string _to)
        {
            onAirSign(_doorPos, 180f);
            exitPad(_doorPos + new Vector3(0f, 0f, -1.2f), _to);
        }

        static void exitPad(Vector3 _pos, string _to)
        {
            planBox("SetDoor (salida)", _pos, new Vector3(2.2f, 0.03f, 1.4f), "Exit");
            label("SALIDA · " + _to + T("", " · NECESITA LA CINTA"), _pos + new Vector3(0f, 0f, -1.1f), 0.4f, new Color(0.36f, 0.56f, 0.85f), 8f);
        }

        static void roomLabel(string _text, Vector3 _pos, float _yaw = 0f) =>
            label(_text, _pos, 0.5f, new Color(0.95f, 0.9f, 0.8f, 0.8f), 12f, _yaw);

        // =====================================================================
        // Senses clones and prefabs
        // =====================================================================

        static int s_grassIndex;
        static void grassPatch(Vector3 _center, int _cols, int _rows, string _name = "Arbustos de atrezo")
        {
            Transform _folder = s_sourceRoot.Find(k_grassFolder);
            Transform[] _grass = _folder.Cast<Transform>().Where(t => t.GetComponent<StealthZone>() != null).ToArray();
            var _patch = child(s_props, _name + " (StealthZone)");
            _patch.position = _center;
            const float _spacing = 1.5f;
            box(_patch, Vector3.zero, new Vector3(_cols * _spacing + 0.6f, 0.02f, _rows * _spacing + 0.6f), "Bush", false).transform.position = _center + Vector3.up * 0.01f;
            for (int x = 0; x < _cols; x++)
            {
                for (int z = 0; z < _rows; z++)
                {
                    Transform _src = _grass[s_grassIndex++ % _grass.Length];
                    Vector3 _pos = _center + new Vector3((x - (_cols - 1) * 0.5f) * _spacing, 0f, (z - (_rows - 1) * 0.5f) * _spacing);
                    clone(_src.name, _patch, _pos, (x * 37 + z * 71) % 360, relativePath(_src));
                }
            }
        }

        static void placeDogWaypoints(Transform _dogGroup, Vector3[] _points)
        {
            ScentFollower _follower = _dogGroup.GetComponentInChildren<ScentFollower>(true);
            var _so = new SerializedObject(_follower);
            SerializedProperty _list = _so.FindProperty("m_waypoints");
            for (int i = 0; i < _list.arraySize && i < _points.Length; i++)
            {
                var _wp = _list.GetArrayElementAtIndex(i).objectReferenceValue as Transform;
                if (_wp != null) _wp.position = _points[i];
            }
            _follower.transform.position = _points[0];
        }

        static void lumenProbe(Vector3 _pos) => instantiatePrefab("LumenProbe", s_lights, _pos + Vector3.up * 2.5f, 0f);

        static void woodFloor(Vector3 _center, Vector2 _size)
        {
            GameObject _wood = instantiatePrefab("(P) WoodenFloor", s_floors, _center + Vector3.up * 0.03f, 0f);
            if (_wood != null) _wood.transform.localScale = new Vector3(_size.x, 1f, _size.y);
        }

        static void carpet(Vector3 _center, Vector2 _size)
        {
            GameObject _carpet = instantiatePrefab("(P) Carpet", s_floors, _center + Vector3.up * 0.01f, 0f);
            if (_carpet != null) _carpet.transform.localScale = new Vector3(_size.x / 4.89f, 1f, _size.y / 3.24f);
        }

        static void can(Vector3 _pos, int _variant)
        {
            GameObject _can = instantiatePrefab($"(P) Can 0{_variant % 5 + 1}", s_props, _pos, _variant * 47f);
            if (s_v2 && _can != null) _can.name = $"Lata de película {_variant + 1} (hace ruido)";
        }

        static GameObject instantiatePrefab(string _name, Transform _parent, Vector3 _pos, float _yaw)
        {
            GameObject _prefab = null;
            foreach (string _guid in AssetDatabase.FindAssets($"\"{_name}\" t:Prefab"))
            {
                var _go = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(_guid));
                if (_go != null && _go.name == _name) { _prefab = _go; break; }
            }
            if (_prefab == null) { Debug.LogError("[LastFrame] Prefab not found: " + _name); return null; }
            var _instance = (GameObject)PrefabUtility.InstantiatePrefab(_prefab, _parent);
            _instance.transform.SetPositionAndRotation(_pos, Quaternion.Euler(0f, _yaw, 0f));
            return _instance;
        }

        /// <summary>
        /// Clones a group of test room objects (plus the FOV objects their Senses use), keeps every
        /// internal reference pointing to the clones and references to the test room player/camera
        /// pointing to this scene's. The first source is the anchor placed at _pos/_yaw.
        /// </summary>
        static Transform clone(string _name, Transform _parent, Vector3 _pos, float _yaw, params string[] _paths)
        {
            var _sources = new List<Transform>();
            foreach (string _p in _paths)
            {
                Transform _t = s_sourceRoot.Find(_p);
                if (_t == null) { Debug.LogError("[LastFrame] Source not found: " + _p); continue; }
                _sources.Add(_t);
            }
            if (_sources.Count == 0) return null;

            foreach (Senses _senses in _sources.SelectMany(s => s.GetComponentsInChildren<Senses>(true)).ToArray())
            {
                var _fov = new SerializedObject(_senses).FindProperty("m_fov").objectReferenceValue as FOV;
                if (_fov != null && !_sources.Any(s => _fov.transform.IsChildOf(s))) _sources.Add(_fov.transform);
            }

            Transform _anchor = _sources[0];
            var _group = new GameObject(_name).transform;
            _group.SetPositionAndRotation(_anchor.position, Quaternion.Euler(0f, _anchor.eulerAngles.y, 0f));

            var _map = new Dictionary<Object, Object>(s_globalMap);
            var _clones = new List<GameObject>();
            foreach (Transform _src in _sources)
            {
                GameObject _c = Object.Instantiate(_src.gameObject, _src.position, _src.rotation);
                _c.name = _src.name;
                _c.transform.localScale = _src.lossyScale;
                _c.SetActive(true);
                _c.transform.SetParent(_group, true);
                addHierarchyMap(_src.gameObject, _c, _map);
                _clones.Add(_c);
            }
            foreach (GameObject _c in _clones) remap(_c, _map);

            // Keep the anchor's height above the floor (the river and the fish tank sit above y = 0 in the test room).
            _group.SetPositionAndRotation(_pos + Vector3.up * _anchor.position.y, Quaternion.Euler(0f, _yaw, 0f));
            _group.SetParent(_parent, true);
            return _group;
        }

        static void addHierarchyMap(GameObject _src, GameObject _dst, Dictionary<Object, Object> _map)
        {
            Transform[] _a = _src.GetComponentsInChildren<Transform>(true);
            Transform[] _b = _dst.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < _a.Length && i < _b.Length; i++)
            {
                _map[_a[i].gameObject] = _b[i].gameObject;
                Component[] _ca = _a[i].GetComponents<Component>();
                Component[] _cb = _b[i].GetComponents<Component>();
                for (int j = 0; j < _ca.Length && j < _cb.Length; j++)
                {
                    if (_ca[j] != null && _cb[j] != null) _map[_ca[j]] = _cb[j];
                }
            }
        }

        static void remap(GameObject _root, Dictionary<Object, Object> _map)
        {
            foreach (Component _c in _root.GetComponentsInChildren<Component>(true))
            {
                if (_c == null || _c is Transform) continue;
                var _so = new SerializedObject(_c);
                SerializedProperty _it = _so.GetIterator();
                bool _changed = false;
                while (_it.Next(true))
                {
                    if (_it.propertyType != SerializedPropertyType.ObjectReference) continue;
                    Object _value = _it.objectReferenceValue;
                    if (_value == null || AssetDatabase.Contains(_value)) continue;
                    if (_map.TryGetValue(_value, out Object _replacement))
                    {
                        _it.objectReferenceValue = _replacement;
                        _changed = true;
                    }
                    else if (_value is Component _vc && _vc.gameObject.scene == s_source || _value is GameObject _vg && _vg.scene == s_source)
                    {
                        _it.objectReferenceValue = null; // never keep a cross-scene reference
                        _changed = true;
                    }
                }
                if (_changed) _so.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        static string relativePath(Transform _t)
        {
            var _parts = new List<string>();
            while (_t != null && _t != s_sourceRoot)
            {
                _parts.Insert(0, _t.name);
                _t = _t.parent;
            }
            return string.Join("/", _parts);
        }

        // =====================================================================
        // Architecture
        // =====================================================================

        /// <summary>Wall along X at _z, from _x0 to _x1, with a door gap centered on each value of _doors.</summary>
        static void wallX(float _z, float _x0, float _x1, Style _style, params float[] _doors)
        {
            foreach (Vector2 _s in segments(_x0, _x1, _doors))
                wallPiece(new Vector3((_s.x + _s.y) * 0.5f, 0f, _z), new Vector3(_s.y - _s.x, 0f, k_wallThickness), _style);
        }

        /// <summary>Wall along Z at _x, from _z0 to _z1, with a door gap centered on each value of _doors.</summary>
        static void wallZ(float _x, float _z0, float _z1, Style _style, params float[] _doors)
        {
            foreach (Vector2 _s in segments(_z0, _z1, _doors))
                wallPiece(new Vector3(_x, 0f, (_s.x + _s.y) * 0.5f), new Vector3(k_wallThickness, 0f, _s.y - _s.x), _style);
        }

        static IEnumerable<Vector2> segments(float _a, float _b, float[] _doors)
        {
            float _cursor = _a;
            foreach (float _d in _doors.OrderBy(d => d))
            {
                float _gapStart = _d - k_doorWidth * 0.5f;
                if (_gapStart > _cursor + 0.05f) yield return new Vector2(_cursor, _gapStart);
                _cursor = Mathf.Max(_cursor, _d + k_doorWidth * 0.5f);
            }
            if (_b > _cursor + 0.05f) yield return new Vector2(_cursor, _b);
        }

        static void wallPiece(Vector3 _base, Vector3 _footprint, Style _style)
        {
            var _wall = child(s_walls, "Wall");
            _wall.position = _base;
            switch (_style)
            {
                case Style.Studio: // old studio corridor: dark wainscot, cream wall, brass trim
                    box(_wall, Vector3.zero, new Vector3(_footprint.x, 1.1f, _footprint.z), "Wainscot");
                    box(_wall, new Vector3(0f, 1.1f, 0f), new Vector3(_footprint.x, k_wallHeight - 1.1f, _footprint.z), "WallCream");
                    box(_wall, new Vector3(0f, 1.08f, 0f), new Vector3(_footprint.x + 0.04f, 0.07f, _footprint.z + 0.04f), "Brass", false);
                    break;
                case Style.Concrete:
                    box(_wall, Vector3.zero, new Vector3(_footprint.x, k_wallHeight + 0.5f, _footprint.z), "Concrete");
                    break;
                case Style.Fence:
                    box(_wall, Vector3.zero, new Vector3(_footprint.x, 2.6f, _footprint.z), "Wood");
                    box(_wall, new Vector3(0f, 2.6f, 0f), new Vector3(_footprint.x + 0.02f, 0.08f, _footprint.z + 0.02f), "Ink", false);
                    break;
            }
        }

        static void floor(float _x0, float _z0, float _x1, float _z1, string _material, float _y = 0.006f)
        {
            float _w = _x1 - _x0, _d = _z1 - _z0;
            GameObject _f = GameObject.CreatePrimitive(PrimitiveType.Cube);
            _f.name = "Floor · " + _material;
            _f.transform.SetParent(s_floors);
            _f.transform.position = new Vector3((_x0 + _x1) * 0.5f, _y - 0.005f, (_z0 + _z1) * 0.5f);
            _f.transform.localScale = new Vector3(_w, 0.01f, _d);
            _f.isStatic = true;
            Object.DestroyImmediate(_f.GetComponent<Collider>()); // the walkable Ground plane is underneath
            var _r = _f.GetComponent<MeshRenderer>();
            _r.sharedMaterial = s_materials[_material];
            if (_material == "Checker") _r.sharedMaterial = checkerFor(_w, _d);
        }

        /// <summary>
        /// Checker material tiled for a floor of this size (0.5 m tiles). Property blocks are not saved
        /// with the scene, so each floor size gets its own small material asset.
        /// </summary>
        static Material checkerFor(float _w, float _d)
        {
            string _key = $"Checker_{_w:0.#}x{_d:0.#}";
            if (s_materials.TryGetValue(_key, out Material _cached)) return _cached;
            string _folder = s_flat ? flatFolder : k_materialsFolder;
            string _path = $"{_folder}/Checker/{(s_flat ? flatPrefix : "M_Block")}_{_key}.mat";
            if (!AssetDatabase.IsValidFolder($"{_folder}/Checker")) AssetDatabase.CreateFolder(_folder, "Checker");
            var _mat = AssetDatabase.LoadAssetAtPath<Material>(_path);
            if (_mat == null)
            {
                _mat = new Material(s_materials["Checker"]);
                AssetDatabase.CreateAsset(_mat, _path);
            }
            _mat.CopyPropertiesFromMaterial(s_materials["Checker"]);
            _mat.SetTextureScale("_BaseMap", new Vector2(_w, _d));
            EditorUtility.SetDirty(_mat);
            s_materials[_key] = _mat;
            return _mat;
        }

        // =====================================================================
        // Props (studio set dressing built from primitives)
        // =====================================================================

        static Transform prop(string _name, Vector3 _pos, float _yaw)
        {
            var _t = child(s_props, _name);
            _t.SetPositionAndRotation(_pos, Quaternion.Euler(0f, _yaw, 0f));
            // v2: furniture is bigger than Ollie. Backdrops, facades and wall fixtures keep their size so the
            // routes stay open and nothing pokes through the walls.
            if (s_v2 && !k_unscaledProps.Any(_name.StartsWith)) _t.localScale = Vector3.one * k_propScaleV2;
            return _t;
        }

        static void crt(Transform _p, Vector3 _base, float _size)
        {
            box(_p, _base, new Vector3(_size * 1.1f, _size * 0.9f, _size), "Metal", false);
            box(_p, _base + new Vector3(0f, _size * 0.1f, -_size * 0.51f), new Vector3(_size * 0.85f, _size * 0.62f, 0.02f), "Screen", false);
        }

        static void chair(Vector3 _pos, float _yaw)
        {
            Transform _t = prop("Silla", _pos, _yaw);
            box(_t, Vector3.zero, new Vector3(0.5f, 0.45f, 0.5f), "Ink");
            box(_t, new Vector3(0f, 0.45f, 0.22f), new Vector3(0.5f, 0.5f, 0.06f), "Ink", false);
        }

        static void armchair(Vector3 _pos, float _yaw)
        {
            Transform _t = prop("Sillón", _pos, _yaw);
            box(_t, Vector3.zero, new Vector3(1f, 0.45f, 0.9f), "Red");
            box(_t, new Vector3(0f, 0.45f, 0.35f), new Vector3(1f, 0.55f, 0.2f), "Red", false);
        }

        static void sofa(Vector3 _pos, float _yaw)
        {
            Transform _t = prop("Sofá", _pos, _yaw);
            box(_t, Vector3.zero, new Vector3(2f, 0.45f, 0.85f), "Red");
            box(_t, new Vector3(0f, 0.45f, 0.33f), new Vector3(2f, 0.5f, 0.2f), "Red");
            box(_t, new Vector3(-0.95f, 0.45f, 0f), new Vector3(0.12f, 0.2f, 0.85f), "Red", false);
            box(_t, new Vector3(0.95f, 0.45f, 0f), new Vector3(0.12f, 0.2f, 0.85f), "Red", false);
        }

        static void table(Vector3 _pos, float _yaw, float _w, float _d, float _h)
        {
            Transform _t = prop("Mesa", _pos, _yaw);
            box(_t, Vector3.zero, new Vector3(_w, _h, _d), "Wood");
        }

        static void desk(Vector3 _pos, float _yaw, bool _withCrt)
        {
            Transform _t = prop("Escritorio", _pos, _yaw);
            box(_t, Vector3.zero, new Vector3(1.6f, 0.75f, 0.8f), "Wood");
            if (_withCrt) crt(_t, new Vector3(0.3f, 0.75f, 0.05f), 0.5f);
            box(_t, new Vector3(-0.4f, 0.75f, 0f), new Vector3(0.4f, 0.05f, 0.3f), "Paper", false);
        }

        static void animationDesk(Vector3 _pos, float _yaw)
        {
            Transform _t = prop("Mesa de animador", _pos, _yaw);
            box(_t, Vector3.zero, new Vector3(1.6f, 0.75f, 0.9f), "Wood");
            box(_t, new Vector3(0f, 0.95f, 0.05f), new Vector3(0.9f, 0.05f, 0.7f), "Screen", false, new Vector3(-25f, 0f, 0f));
            box(_t, new Vector3(0.6f, 0.75f, 0.2f), new Vector3(0.25f, 0.15f, 0.35f), "Paper", false);
            cylinder(_t, new Vector3(-0.65f, 0.75f, 0.3f), 0.06f, 0.6f, "Metal", false);
        }

        static void shelf(Vector3 _pos, float _yaw, float _len)
        {
            Transform _t = prop("Estantería", _pos, _yaw);
            box(_t, Vector3.zero, new Vector3(_len, 2.2f, 0.55f), "Wood");
            int _n = Mathf.FloorToInt(_len / 0.45f);
            for (int i = 0; i < _n; i++)
            {
                float _x = -_len * 0.5f + 0.25f + i * 0.45f;
                cylinder(_t, new Vector3(_x, 2.2f, 0f), 0.38f, 0.08f + (i % 3) * 0.08f, i % 4 == 0 ? "Red" : "Metal", false);
            }
        }

        static void plant(Vector3 _pos)
        {
            Transform _t = prop("Planta", _pos, 0f);
            cylinder(_t, Vector3.zero, 0.5f, 0.5f, "Brass");
            sphere(_t, new Vector3(0f, 0.95f, 0f), 0.9f, "Green");
        }

        static void waterCooler(Vector3 _pos)
        {
            Transform _t = prop("Fuente de agua", _pos, 0f);
            box(_t, Vector3.zero, new Vector3(0.4f, 1f, 0.4f), "Paper");
            cylinder(_t, new Vector3(0f, 1f, 0f), 0.3f, 0.45f, "Glass", false);
        }

        static void poster(Vector3 _pos, float _yaw, string _material)
        {
            Transform _t = prop("Cartel", _pos, _yaw);
            box(_t, new Vector3(0f, 1.1f, 0f), new Vector3(0.9f, 1.25f, 0.04f), "Brass", false);
            box(_t, new Vector3(0f, 1.15f, 0.01f), new Vector3(0.78f, 1.13f, 0.05f), _material, false);
        }

        static void recordPoster(Vector3 _pos, float _yaw)
        {
            Transform _t = prop("Disco de oro", _pos, _yaw);
            box(_t, new Vector3(0f, 1.2f, 0f), new Vector3(0.8f, 0.9f, 0.04f), "Ink", false);
            cylinder(_t, new Vector3(0f, 1.65f, 0.03f), 0.6f, 0.02f, "Brass", false, new Vector3(90f, 0f, 0f));
        }

        static void starSign(Vector3 _pos)
        {
            Transform _t = prop("Estrella del camerino", _pos, 0f);
            box(_t, new Vector3(0f, 2.2f, 0f), new Vector3(0.45f, 0.45f, 0.05f), "Yellow", false, new Vector3(0f, 0f, 45f));
        }

        static void glassDoors(Vector3 _pos, float _yaw)
        {
            Transform _t = prop("Puerta principal", _pos, _yaw);
            box(_t, new Vector3(-0.62f, 0f, 0f), new Vector3(1.2f, 2.5f, 0.08f), "Glass", false);
            box(_t, new Vector3(0.62f, 0f, 0f), new Vector3(1.2f, 2.5f, 0.08f), "Glass", false);
            box(_t, new Vector3(0f, 2.5f, 0f), new Vector3(2.6f, 0.15f, 0.1f), "Brass", false);
        }

        static void elevator(Vector3 _pos, float _yaw)
        {
            Transform _t = prop("Ascensor (fuera de servicio)", _pos, _yaw);
            box(_t, new Vector3(-0.52f, 0f, 0f), new Vector3(1f, 2.4f, 0.08f), "Metal", false);
            box(_t, new Vector3(0.52f, 0f, 0f), new Vector3(1f, 2.4f, 0.08f), "Metal", false);
            box(_t, new Vector3(0f, 2.5f, 0f), new Vector3(0.7f, 0.2f, 0.08f), "Bulb", false);
        }

        static void crate(Vector3 _pos, float _size, float _yaw)
        {
            Transform _t = prop("Caja", _pos, _yaw);
            box(_t, Vector3.zero, Vector3.one * _size, "WoodLight");
            box(_t, new Vector3(0f, _size * 0.45f, 0f), new Vector3(_size + 0.02f, _size * 0.1f, _size + 0.02f), "Wood", false);
        }

        static void bench(Vector3 _pos, float _yaw)
        {
            Transform _t = prop("Banco", _pos, _yaw);
            box(_t, Vector3.zero, new Vector3(2f, 0.45f, 0.5f), "Wood");
        }

        static void celRack(Vector3 _pos, float _yaw)
        {
            Transform _t = prop("Secadero de celuloides", _pos, _yaw);
            box(_t, Vector3.zero, new Vector3(2f, 1.5f, 0.4f), "Metal");
            for (int i = 0; i < 5; i++) box(_t, new Vector3(-0.8f + i * 0.4f, 1.5f, 0f), new Vector3(0.3f, 0.35f, 0.02f), "Glass", false);
        }

        static void chalkboard(Vector3 _pos, float _yaw)
        {
            Transform _t = prop("Pizarra de guiones", _pos, _yaw);
            box(_t, new Vector3(0f, 0.9f, 0f), new Vector3(2.6f, 1.3f, 0.06f), "Board", false);
            box(_t, new Vector3(0f, 0.85f, 0f), new Vector3(2.7f, 0.06f, 0.12f), "Wood", false);
        }

        static void oldTv(Vector3 _pos, float _yaw)
        {
            Transform _t = prop("Televisor antiguo (dibujos en bucle)", _pos, _yaw);
            box(_t, Vector3.zero, new Vector3(1.1f, 0.9f, 0.7f), "Wood");
            box(_t, new Vector3(0f, 0.2f, -0.36f), new Vector3(0.75f, 0.55f, 0.02f), "Screen", false);
            cylinder(_t, new Vector3(-0.2f, 1.15f, 0f), 0.03f, 0.6f, "Metal", false, new Vector3(0f, 0f, 25f));
            cylinder(_t, new Vector3(0.2f, 1.15f, 0f), 0.03f, 0.6f, "Metal", false, new Vector3(0f, 0f, -25f));
        }

        static void vanity(Vector3 _pos, float _yaw)
        {
            Transform _t = prop("Tocador con bombillas", _pos, _yaw);
            box(_t, Vector3.zero, new Vector3(1.2f, 0.75f, 0.5f), "Wood");
            box(_t, new Vector3(0f, 0.8f, 0.2f), new Vector3(0.9f, 0.9f, 0.05f), "Glass", false);
            for (int i = 0; i < 5; i++) sphere(_t, new Vector3(-0.5f + i * 0.25f, 1.78f, 0.2f), 0.12f, "Bulb");
        }

        static void costumeRack(Vector3 _pos, float _yaw)
        {
            Transform _t = prop("Perchero de vestuario", _pos, _yaw);
            box(_t, new Vector3(0f, 1.6f, 0f), new Vector3(1.7f, 0.05f, 0.05f), "Metal", false);
            box(_t, new Vector3(-0.85f, 0f, 0f), new Vector3(0.05f, 1.6f, 0.5f), "Metal", false);
            box(_t, new Vector3(0.85f, 0f, 0f), new Vector3(0.05f, 1.6f, 0.5f), "Metal", false);
            string[] _cloth = { "Red", "Sky", "Yellow", "Green", "InkPurple" };
            for (int i = 0; i < 5; i++) box(_t, new Vector3(-0.6f + i * 0.3f, 0.6f, 0f), new Vector3(0.22f, 0.95f, 0.5f), _cloth[i]);
        }

        static void rollUpDoor(Vector3 _pos, float _yaw, float _width)
        {
            Transform _t = prop("Persiana del muelle", _pos, _yaw);
            for (int i = 0; i < 10; i++) box(_t, new Vector3(0f, i * 0.32f, 0f), new Vector3(_width, 0.28f, 0.08f), "Metal", false);
        }

        static void forklift(Vector3 _pos, float _yaw)
        {
            Transform _t = prop("Carretilla elevadora", _pos, _yaw);
            box(_t, Vector3.zero, new Vector3(1.2f, 1.2f, 2f), "Yellow");
            box(_t, new Vector3(0f, 1.2f, -0.3f), new Vector3(1.1f, 0.08f, 1f), "Ink", false);
            box(_t, new Vector3(-0.45f, 0f, 1.1f), new Vector3(0.1f, 2.4f, 0.1f), "Metal", false);
            box(_t, new Vector3(0.45f, 0f, 1.1f), new Vector3(0.1f, 2.4f, 0.1f), "Metal", false);
            box(_t, new Vector3(0f, 0.1f, 1.7f), new Vector3(0.9f, 0.06f, 1f), "Metal", false);
        }

        static void inkBarrel(Vector3 _pos)
        {
            Transform _t = prop("Barril de tinta", _pos, 0f);
            cylinder(_t, Vector3.zero, 0.7f, 1f, "InkPurple");
            cylinder(_t, new Vector3(0f, 1f, 0f), 0.72f, 0.04f, "Ink", false);
        }

        static void workLight(Vector3 _pos)
        {
            Transform _t = prop("Foco de trabajo", _pos, 0f);
            cylinder(_t, Vector3.zero, 0.08f, 2f, "Metal", false);
            sphere(_t, new Vector3(0f, 2.1f, 0f), 0.3f, "Bulb");
            pointLight(_t, new Vector3(0f, 2.1f, 0f), new Color(1f, 0.8f, 0.55f), 6f, 6f);
        }

        static void bleachers(Vector3 _pos, float _yaw, float _width, int _rows)
        {
            Transform _t = prop("Gradas del público", _pos, _yaw);
            for (int i = 0; i < _rows; i++)
                box(_t, new Vector3(0f, 0f, -i * 0.8f), new Vector3(_width, 0.45f * (i + 1), 0.8f), i % 2 == 0 ? "Wood" : "WoodLight");
        }

        static void tvCamera(Vector3 _pos, float _yaw)
        {
            Transform _t = prop("Cámara de TV antigua", _pos, _yaw);
            cylinder(_t, Vector3.zero, 0.9f, 0.15f, "Ink");
            cylinder(_t, new Vector3(0f, 0.15f, 0f), 0.15f, 1.1f, "Metal", false);
            box(_t, new Vector3(0f, 1.25f, 0f), new Vector3(0.5f, 0.55f, 0.8f), "Ink", false);
            cylinder(_t, new Vector3(0f, 1.52f, 0.6f), 0.25f, 0.4f, "Metal", false, new Vector3(90f, 0f, 0f));
            sphere(_t, new Vector3(0f, 1.9f, -0.2f), 0.1f, "OnAir");
        }

        static void directorChair(Vector3 _pos, float _yaw)
        {
            Transform _t = prop("Silla del director", _pos, _yaw);
            box(_t, Vector3.zero, new Vector3(0.6f, 0.75f, 0.55f), "Wood");
            box(_t, new Vector3(0f, 0.75f, 0.25f), new Vector3(0.6f, 0.5f, 0.04f), "Red", false);
        }

        static void cable(Vector3 _a, Vector3 _b) => line(s_props, _a, _b, 0.06f, "Ink").name = "Cable";

        static void treeFlat(Vector3 _pos, float _yaw)
        {
            Transform _t = prop("Árbol de cartón", _pos, _yaw);
            box(_t, Vector3.zero, new Vector3(0.5f, 2.6f, 0.15f), "Wood");
            cylinder(_t, new Vector3(0f, 3.2f, 0f), 2.4f, 0.12f, "Green", false, new Vector3(90f, 0f, 0f));
            box(_t, new Vector3(0f, 0f, 0.6f), new Vector3(0.1f, 2f, 1.1f), "WoodLight", false);
        }

        static void cottageFacade(Vector3 _pos, float _yaw, float _width)
        {
            Transform _t = prop("Fachada de cabaña (decorado)", _pos, _yaw);
            box(_t, Vector3.zero, new Vector3(_width, 3.2f, 0.15f), "WoodLight");
            box(_t, new Vector3(0f, 3.2f, 0f), new Vector3(_width * 0.75f, _width * 0.75f, 0.14f), "Red", false, new Vector3(0f, 0f, 45f));
            box(_t, new Vector3(0f, 0f, -0.05f), new Vector3(0.9f, 1.9f, 0.1f), "Ink", false);
            box(_t, new Vector3(-_width * 0.3f, 1.4f, -0.05f), new Vector3(0.7f, 0.7f, 0.1f), "Bulb", false);
            box(_t, new Vector3(_width * 0.3f, 1.4f, -0.05f), new Vector3(0.7f, 0.7f, 0.1f), "Bulb", false);
            box(_t, new Vector3(-_width * 0.4f, 0f, 0.6f), new Vector3(0.1f, 2.4f, 1.1f), "Wood", false);
            box(_t, new Vector3(_width * 0.4f, 0f, 0.6f), new Vector3(0.1f, 2.4f, 1.1f), "Wood", false);
        }

        static void tombstone(Vector3 _pos)
        {
            Transform _t = prop("Lápida de cartón", _pos, (_pos.x * 13f) % 20f);
            box(_t, Vector3.zero, new Vector3(0.8f, 0.9f, 0.25f), "Concrete");
            cylinder(_t, new Vector3(0f, 0.9f, 0f), 0.8f, 0.25f, "Concrete", false, new Vector3(90f, 0f, 0f));
        }

        static void picketFence(Vector3 _pos, float _yaw, float _len)
        {
            Transform _t = prop("Valla de madera", _pos, _yaw);
            box(_t, new Vector3(0f, 0.35f, 0f), new Vector3(_len, 0.12f, 0.06f), "Paper");
            box(_t, Vector3.zero, new Vector3(_len, 0.9f, 0.06f), "Paper", true).GetComponent<MeshRenderer>().enabled = false; // collider only
            int _n = Mathf.FloorToInt(_len / 0.35f);
            for (int i = 0; i <= _n; i++)
                box(_t, new Vector3(-_len * 0.5f + i * 0.35f, 0f, 0f), new Vector3(0.12f, 1f, 0.04f), "Paper", false);
        }

        static void spotlightStand(Vector3 _pos, float _yaw, bool _lit)
        {
            Transform _t = prop("Foco de estudio", _pos, _yaw);
            cylinder(_t, Vector3.zero, 0.7f, 0.05f, "Ink", false);
            cylinder(_t, Vector3.zero, 0.08f, 1.9f, "Metal", false);
            box(_t, new Vector3(0f, 1.95f, 0f), new Vector3(0.5f, 0.5f, 0.6f), "Ink", false, new Vector3(30f, 0f, 0f));
            if (!_lit) return;
            var _go = new GameObject("Spot Light");
            _go.transform.SetParent(_t, false);
            _go.transform.localPosition = new Vector3(0f, 2.2f, 0.3f);
            _go.transform.localRotation = Quaternion.Euler(28f, 0f, 0f);
            var _light = _go.AddComponent<Light>();
            _light.type = LightType.Spot;
            _light.range = 16f;
            _light.spotAngle = 55f;
            _light.intensity = 40f;
            _light.color = new Color(1f, 0.9f, 0.7f);
            _light.shadows = LightShadows.Soft;
            _go.AddComponent<UniversalAdditionalLightData>();
        }

        static void leaningFlat(Vector3 _pos, float _yaw, string _material)
        {
            Transform _t = prop("Decorado apoyado", _pos, _yaw);
            box(_t, new Vector3(0f, 1.5f, 0.3f), new Vector3(2.2f, 3f, 0.1f), _material, true, new Vector3(-10f, 0f, 0f));
        }

        static void catwalkStairs(Vector3 _pos, float _yaw)
        {
            Transform _t = prop("Escalera a la pasarela", _pos, _yaw);
            for (int i = 0; i < 6; i++) box(_t, new Vector3(0f, 0f, i * 0.5f), new Vector3(1.4f, 0.3f * (i + 1), 0.5f), "Metal");
        }

        static void cartoonBomb(Vector3 _pos)
        {
            Transform _t = prop("Bomba de caricatura gigante", _pos, 0f);
            sphere(_t, new Vector3(0f, 0.8f, 0f), 1.6f, "Ink", true);
            cylinder(_t, new Vector3(0f, 1.6f, 0f), 0.35f, 0.25f, "Metal", false);
            cylinder(_t, new Vector3(0.15f, 2f, 0f), 0.05f, 0.5f, "Wood", false, new Vector3(0f, 0f, -30f));
        }

        static void giantPencil(Vector3 _pos, float _yaw)
        {
            Transform _t = prop("Lápiz gigante", _pos, _yaw);
            cylinder(_t, new Vector3(0f, 0.35f, 0f), 0.7f, 5f, "Yellow", true, new Vector3(0f, 0f, 90f));
            cylinder(_t, new Vector3(2.75f, 0.35f, 0f), 0.5f, 0.5f, "WoodLight", false, new Vector3(0f, 0f, 90f));
            cylinder(_t, new Vector3(-2.65f, 0.35f, 0f), 0.72f, 0.3f, "Red", false, new Vector3(0f, 0f, 90f));
        }

        static void anvil(Vector3 _pos)
        {
            Transform _t = prop("Yunque de atrezo", _pos, 15f);
            box(_t, Vector3.zero, new Vector3(0.8f, 0.5f, 0.6f), "Ink");
            box(_t, new Vector3(0f, 0.5f, 0f), new Vector3(1.4f, 0.35f, 0.6f), "Ink");
        }

        static void well(Vector3 _pos)
        {
            Transform _t = prop("Pozo de atrezo", _pos, 0f);
            cylinder(_t, Vector3.zero, 1.5f, 0.9f, "Concrete");
            box(_t, new Vector3(-0.7f, 0.9f, 0f), new Vector3(0.1f, 1.3f, 0.1f), "Wood", false);
            box(_t, new Vector3(0.7f, 0.9f, 0f), new Vector3(0.1f, 1.3f, 0.1f), "Wood", false);
            box(_t, new Vector3(0f, 2.2f, 0f), new Vector3(1.8f, 0.1f, 1.2f), "Red", false);
        }

        static void flower(Vector3 _pos, string _material)
        {
            Transform _t = prop("Flor de atrezo", _pos, 0f);
            cylinder(_t, Vector3.zero, 0.06f, 0.7f, "Green", false);
            sphere(_t, new Vector3(0f, 0.8f, 0f), 0.4f, _material);
        }

        static void onAirSign(Vector3 _pos, float _yaw)
        {
            Transform _t = prop("Luz ON AIR", _pos, _yaw);
            box(_t, new Vector3(0f, 2.55f, 0f), new Vector3(1.1f, 0.35f, 0.2f), "OnAir", false);
        }

        static void studioWindow(Vector3 _pos, float _yaw, float _width)
        {
            Transform _t = prop("Ventana de control", _pos, _yaw);
            box(_t, new Vector3(0f, 1.15f, 0f), new Vector3(_width, 1.3f, 0.36f), "Glass", false);
        }

        static void mixingConsole(Vector3 _pos, float _yaw, float _width)
        {
            Transform _t = prop("Mesa de mezclas", _pos, _yaw);
            box(_t, Vector3.zero, new Vector3(_width, 0.8f, 1.1f), "Ink");
            box(_t, new Vector3(0f, 0.85f, 0f), new Vector3(_width, 0.08f, 1f), "Metal", false, new Vector3(-12f, 0f, 0f));
            for (int i = 0; i < 8; i++) box(_t, new Vector3(-_width * 0.45f + i * _width * 0.12f, 0.92f, 0.1f), new Vector3(0.08f, 0.03f, 0.5f), i % 3 == 0 ? "OnAir" : "Screen", false);
        }

        static void crtWall(Vector3 _pos, float _yaw, int _columns)
        {
            Transform _t = prop("Muro de monitores", _pos, _yaw);
            for (int c = 0; c < _columns; c++)
                for (int r = 0; r < 2; r++)
                    crt(_t, new Vector3((c - (_columns - 1) * 0.5f) * 0.75f, 0.8f + r * 0.65f, 0f), 0.6f);
            box(_t, Vector3.zero, new Vector3(_columns * 0.75f, 0.8f, 0.6f), "Ink");
        }

        static void tapeMachine(Vector3 _pos, float _yaw)
        {
            Transform _t = prop("Magnetófono", _pos, _yaw);
            box(_t, Vector3.zero, new Vector3(1f, 1.3f, 0.6f), "Metal");
            cylinder(_t, new Vector3(-0.25f, 1.05f, -0.31f), 0.35f, 0.03f, "Ink", false, new Vector3(90f, 0f, 0f));
            cylinder(_t, new Vector3(0.25f, 1.05f, -0.31f), 0.35f, 0.03f, "Ink", false, new Vector3(90f, 0f, 0f));
        }

        static void gravelPit(Vector3 _pos, string _fill)
        {
            Transform _t = prop("Foso de foley", _pos, 0f);
            box(_t, Vector3.zero, new Vector3(2.4f, 0.25f, 2.4f), "Wood");
            box(_t, new Vector3(0f, 0.25f, 0f), new Vector3(2.1f, 0.02f, 2.1f), _fill, false);
        }

        static void waterTub(Vector3 _pos)
        {
            Transform _t = prop("Tina de agua (foley)", _pos, 0f);
            cylinder(_t, Vector3.zero, 1.6f, 0.6f, "Metal");
            cylinder(_t, new Vector3(0f, 0.6f, 0f), 1.45f, 0.02f, "Sky", false);
        }

        static void foleyDoor(Vector3 _pos, float _yaw)
        {
            Transform _t = prop("Puerta de foley", _pos, _yaw);
            box(_t, Vector3.zero, new Vector3(1.2f, 0.1f, 0.8f), "Wood");
            box(_t, new Vector3(0f, 0.1f, 0f), new Vector3(1f, 2.1f, 0.08f), "WoodLight");
            sphere(_t, new Vector3(0.35f, 1.1f, -0.06f), 0.08f, "Brass");
        }

        static void acousticPanels(Vector3 _pos, float _yaw, int _count)
        {
            Transform _t = prop("Paneles acústicos", _pos, _yaw);
            for (int i = 0; i < _count; i++)
                for (int r = 0; r < 2; r++)
                    box(_t, new Vector3((i - (_count - 1) * 0.5f) * 1.1f, 0.9f + r * 1.05f, 0f), new Vector3(0.95f, 0.95f, 0.1f), (i + r) % 2 == 0 ? "Ink" : "Wainscot", false);
        }

        static void drumKit(Vector3 _pos)
        {
            Transform _t = prop("Batería", _pos, 0f);
            cylinder(_t, new Vector3(0f, 0.3f, 0f), 0.6f, 0.5f, "Red", true, new Vector3(90f, 0f, 0f));
            cylinder(_t, new Vector3(-0.6f, 0f, -0.3f), 0.4f, 0.6f, "Red");
            cylinder(_t, new Vector3(0.6f, 0f, -0.3f), 0.45f, 0.5f, "Red");
            cylinder(_t, new Vector3(-0.7f, 1f, 0.2f), 0.5f, 0.02f, "Brass", false);
            cylinder(_t, new Vector3(0.8f, 1.1f, 0.1f), 0.55f, 0.02f, "Brass", false);
        }

        static void piano(Vector3 _pos, float _yaw)
        {
            Transform _t = prop("Piano vertical", _pos, _yaw);
            box(_t, Vector3.zero, new Vector3(1.6f, 1.25f, 0.6f), "Ink");
            box(_t, new Vector3(0f, 0.75f, -0.4f), new Vector3(1.5f, 0.06f, 0.25f), "Paper", false);
        }

        static void micStand(Vector3 _pos)
        {
            Transform _t = prop("Micrófono", _pos, 0f);
            cylinder(_t, Vector3.zero, 0.4f, 0.03f, "Ink", false);
            cylinder(_t, Vector3.zero, 0.04f, 1.5f, "Metal", false);
            sphere(_t, new Vector3(0f, 1.55f, 0f), 0.14f, "Metal");
        }

        static void amp(Vector3 _pos, float _yaw)
        {
            Transform _t = prop("Amplificador", _pos, _yaw);
            box(_t, Vector3.zero, new Vector3(0.8f, 0.9f, 0.45f), "Ink");
            box(_t, new Vector3(0f, 0.1f, -0.23f), new Vector3(0.6f, 0.6f, 0.02f), "Wainscot", false);
        }

        static void trailer(Vector3 _pos, float _yaw)
        {
            Transform _t = prop("Caravana de estrellas", _pos, _yaw);
            box(_t, new Vector3(0f, 0.4f, 0f), new Vector3(6f, 2.4f, 2.4f), "Paper");
            box(_t, new Vector3(0f, 1.3f, -1.21f), new Vector3(6f, 0.25f, 0.02f), "Red", false);
            box(_t, new Vector3(1.5f, 0.4f, -1.22f), new Vector3(0.8f, 1.9f, 0.04f), "Wood", false);
            box(_t, new Vector3(1.5f, 0f, -1.6f), new Vector3(1f, 0.4f, 0.6f), "Metal");
            starSign(_pos);
        }

        static void truck(Vector3 _pos, float _yaw)
        {
            Transform _t = prop("Camión del estudio", _pos, _yaw);
            box(_t, new Vector3(0f, 0.5f, -0.8f), new Vector3(2.3f, 2.4f, 4.2f), "Paper");
            box(_t, new Vector3(0f, 0.5f, 2.1f), new Vector3(2.2f, 1.7f, 1.5f), "Red");
            box(_t, new Vector3(0f, 1.6f, 2.86f), new Vector3(1.8f, 0.5f, 0.02f), "Glass", false);
            foreach (float _z in new[] { -2f, 2f })
                foreach (float _x in new[] { -1.15f, 1.15f })
                    cylinder(_t, new Vector3(_x, 0.45f, _z), 0.9f, 0.3f, "Ink", false, new Vector3(0f, 0f, 90f));
        }

        static void dumpster(Vector3 _pos, float _yaw)
        {
            Transform _t = prop("Contenedor", _pos, _yaw);
            box(_t, Vector3.zero, new Vector3(2f, 1.3f, 1.2f), "Green");
            box(_t, new Vector3(0f, 1.3f, 0f), new Vector3(2.05f, 0.08f, 1.25f), "Ink", false);
        }

        static void waterTower(Vector3 _pos)
        {
            Transform _t = prop("Depósito de agua del estudio", _pos, 0f);
            foreach (float _x in new[] { -1f, 1f })
                foreach (float _z in new[] { -1f, 1f })
                    cylinder(_t, new Vector3(_x, 0f, _z), 0.2f, 4f, "Wood");
            cylinder(_t, new Vector3(0f, 4f, 0f), 3f, 2.5f, "WoodLight", false);
            cylinder(_t, new Vector3(0f, 6.5f, 0f), 3.2f, 0.3f, "Red", false);
        }

        static void lampPost(Vector3 _pos)
        {
            Transform _t = prop("Farola del backlot", _pos, 0f);
            cylinder(_t, Vector3.zero, 0.15f, 4f, "Ink");
            sphere(_t, new Vector3(0f, 4.1f, 0f), 0.45f, "Bulb");
            if (s_v3) pointLight(_t, new Vector3(0f, 3.8f, 0f), new Color(1f, 0.85f, 0.6f), 14f, 9f); // v3: a pool of light at night
        }

        static void reeds(Vector3 _pos)
        {
            Transform _t = prop("Juncos de atrezo", _pos, 0f);
            for (int i = 0; i < 4; i++) cylinder(_t, new Vector3(i * 0.2f - 0.3f, 0f, (i % 2) * 0.2f), 0.06f, 1f + (i % 3) * 0.3f, "Green", false);
        }

        static void facade(Vector3 _pos, float _yaw, float _width, float _height, string _material)
        {
            Transform _t = prop("Fachada del backlot", _pos, _yaw);
            box(_t, Vector3.zero, new Vector3(_width, _height, 0.2f), _material);
            box(_t, new Vector3(0f, 0f, -0.08f), new Vector3(1.1f, 2.2f, 0.1f), "Ink", false);
            box(_t, new Vector3(-_width * 0.3f, 2.8f, -0.08f), new Vector3(1f, 1f, 0.1f), "Glass", false);
            box(_t, new Vector3(_width * 0.3f, 2.8f, -0.08f), new Vector3(1f, 1f, 0.1f), "Glass", false);
            box(_t, new Vector3(0f, _height - 0.9f, -0.15f), new Vector3(_width * 0.7f, 0.6f, 0.1f), "Yellow", false);
            box(_t, new Vector3(-_width * 0.35f, 0f, 0.8f), new Vector3(0.1f, _height * 0.7f, 1.4f), "Wood", false);
            box(_t, new Vector3(_width * 0.35f, 0f, 0.8f), new Vector3(0.1f, _height * 0.7f, 1.4f), "Wood", false);
        }

        static void vaultDoor(Vector3 _pos, float _yaw)
        {
            Transform _t = prop("Puerta de la bóveda", _pos, _yaw);
            cylinder(_t, new Vector3(0f, 1.5f, 0f), 2.8f, 0.3f, "Metal", false, new Vector3(90f, 0f, 0f));
            cylinder(_t, new Vector3(0f, 1.5f, 0.2f), 0.8f, 0.1f, "Brass", false, new Vector3(90f, 0f, 0f));
        }

        // =====================================================================
        // Primitive helpers
        // =====================================================================

        /// <summary>Box in the parent's local space. Without rotation _p is the base center; with rotation, the center.</summary>
        static GameObject box(Transform _parent, Vector3 _p, Vector3 _size, string _material, bool _collider = true, Vector3? _euler = null)
        {
            var _go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            _go.name = _material;
            _go.transform.SetParent(_parent, false);
            _go.transform.localPosition = _euler.HasValue ? _p : _p + Vector3.up * _size.y * 0.5f;
            if (_euler.HasValue) _go.transform.localRotation = Quaternion.Euler(_euler.Value);
            _go.transform.localScale = _size;
            _go.isStatic = true;
            _go.GetComponent<MeshRenderer>().sharedMaterial = s_materials[_material];
            if (!_collider) Object.DestroyImmediate(_go.GetComponent<Collider>());
            return _go;
        }

        static GameObject cylinder(Transform _parent, Vector3 _p, float _diameter, float _height, string _material, bool _collider = true, Vector3? _euler = null)
        {
            var _go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            _go.name = _material;
            _go.transform.SetParent(_parent, false);
            _go.transform.localPosition = _euler.HasValue ? _p : _p + Vector3.up * _height * 0.5f;
            if (_euler.HasValue) _go.transform.localRotation = Quaternion.Euler(_euler.Value);
            _go.transform.localScale = new Vector3(_diameter, _height * 0.5f, _diameter);
            _go.isStatic = true;
            _go.GetComponent<MeshRenderer>().sharedMaterial = s_materials[_material];
            if (!_collider) Object.DestroyImmediate(_go.GetComponent<Collider>());
            return _go;
        }

        static GameObject sphere(Transform _parent, Vector3 _center, float _diameter, string _material, bool _collider = false)
        {
            var _go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            _go.name = _material;
            _go.transform.SetParent(_parent, false);
            _go.transform.localPosition = _center;
            _go.transform.localScale = Vector3.one * _diameter;
            _go.isStatic = true;
            _go.GetComponent<MeshRenderer>().sharedMaterial = s_materials[_material];
            if (!_collider) Object.DestroyImmediate(_go.GetComponent<Collider>());
            return _go;
        }

        static GameObject line(Transform _parent, Vector3 _a, Vector3 _b, float _width, string _material)
        {
            Vector3 _dir = _b - _a;
            _dir.y = 0f;
            var _go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            _go.name = "Line";
            _go.transform.SetParent(_parent);
            _go.transform.position = (_a + _b) * 0.5f + Vector3.up * 0.09f;
            _go.transform.rotation = Quaternion.LookRotation(_dir);
            _go.transform.localScale = new Vector3(_width, 0.02f, _dir.magnitude);
            _go.GetComponent<MeshRenderer>().sharedMaterial = s_materials[_material];
            Object.DestroyImmediate(_go.GetComponent<Collider>());
            return _go;
        }

        static void pointLight(Transform _parent, Vector3 _local, Color _color, float _intensity, float _range)
        {
            var _go = new GameObject("Point Light");
            _go.transform.SetParent(_parent, false);
            _go.transform.localPosition = _local;
            var _light = _go.AddComponent<Light>();
            _light.type = LightType.Point;
            _light.color = _color;
            _light.intensity = _intensity;
            _light.range = _range;
            _go.AddComponent<UniversalAdditionalLightData>();
        }

        // ---------- Plan-only primitives (no colliders) ----------

        static void planBox(string _name, Vector3 _base, Vector3 _size, string _material)
        {
            var _go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            _go.name = _name;
            _go.transform.SetParent(s_plan);
            _go.transform.position = _base + Vector3.up * (_size.y * 0.5f + 0.06f);
            _go.transform.localScale = _size;
            setupPlanRenderer(_go, _material);
        }

        static void planDisc(string _name, Vector3 _pos, float _diameter, string _material)
        {
            var _go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            _go.name = _name;
            _go.transform.SetParent(s_plan);
            _go.transform.position = _pos + Vector3.up * 0.08f; // above wood/carpet
            _go.transform.localScale = new Vector3(_diameter, 0.01f, _diameter);
            setupPlanRenderer(_go, _material);
        }

        static void setupPlanRenderer(GameObject _go, string _material)
        {
            _go.GetComponent<MeshRenderer>().sharedMaterial = s_materials[_material];
            _go.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            Object.DestroyImmediate(_go.GetComponent<Collider>());
        }

        static void label(string _text, Vector3 _pos, float _fontSize, Color _color, float _width, float _yaw = 0f)
        {
            var _go = new GameObject("Label · " + _text);
            _go.transform.SetParent(s_labels);
            _go.transform.SetPositionAndRotation(_pos + Vector3.up * 0.1f, Quaternion.Euler(90f, _yaw, 0f));
            var _tmp = _go.AddComponent<TextMeshPro>();
            _tmp.text = _text;
            _tmp.fontSize = _fontSize * 10f;
            _tmp.alignment = TextAlignmentOptions.Center;
            _tmp.color = _color;
            _tmp.textWrappingMode = TextWrappingModes.Normal;
            _tmp.rectTransform.sizeDelta = new Vector2(_width, 1.5f);
        }

        // =====================================================================
        // Assets
        // =====================================================================

        static void createMaterials()
        {
            s_materials.Clear(); // each build (and each v7/v8 variant) starts clean: no materials left over from another look
            if (!AssetDatabase.IsValidFolder(k_materialsFolder)) AssetDatabase.CreateFolder("Assets/LastFrame/Art/Materials", "Blocking");
            var _defs = new (string Name, Color Color, bool Emissive)[]
            {
                // Studio palette (GDD: café tinta, latón, rojo apagado, verde humo)
                ("Ink", new Color32(0x25, 0x1E, 0x23, 0xFF), false),
                ("Wainscot", new Color32(0x4A, 0x35, 0x30, 0xFF), false),
                ("WallCream", new Color32(0xE6, 0xD5, 0xAE, 0xFF), false),
                ("Brass", new Color32(0xD5, 0xA3, 0x4F, 0xFF), false),
                ("Concrete", new Color32(0x7A, 0x76, 0x74, 0xFF), false),
                ("ConcreteDark", new Color32(0x3F, 0x3C, 0x3E, 0xFF), false),
                ("Wood", new Color32(0x8A, 0x5A, 0x3B, 0xFF), false),
                ("WoodLight", new Color32(0xB9, 0x87, 0x5A, 0xFF), false),
                ("Red", new Color32(0x9E, 0x2B, 0x2B, 0xFF), false),
                ("CarpetRed", new Color32(0x5A, 0x22, 0x28, 0xFF), false),
                ("Green", new Color32(0x5E, 0x8C, 0x4A, 0xFF), false),
                ("Bush", new Color32(0x3E, 0x5A, 0x3C, 0xFF), false),
                ("Astro", new Color32(0x4F, 0x7F, 0x3A, 0xFF), false),
                ("Dirt", new Color32(0x6B, 0x55, 0x40, 0xFF), false),
                ("Sky", new Color32(0x8F, 0xB8, 0xD8, 0xFF), false),
                ("SkyNight", new Color32(0x22, 0x2E, 0x52, 0xFF), false),
                ("Paper", new Color32(0xF2, 0xE6, 0xC9, 0xFF), false),
                ("Yellow", new Color32(0xE2, 0xBD, 0x73, 0xFF), false),
                ("Metal", new Color32(0x8C, 0x8C, 0x91, 0xFF), false),
                ("Glass", new Color32(0xA9, 0xCF, 0xD8, 0xFF), false),
                ("Board", new Color32(0x2F, 0x4A, 0x3A, 0xFF), false),
                ("InkPurple", new Color32(0x59, 0x40, 0x5C, 0xFF), false),
                ("Screen", new Color32(0x9F, 0xE3, 0xC2, 0xFF), true),
                ("Bulb", new Color32(0xFF, 0xE4, 0xA0, 0xFF), true),
                ("OnAir", new Color32(0xFF, 0x3B, 0x30, 0xFF), true),
                // Plan markers
                ("Start", new Color32(0x66, 0x73, 0x6A, 0xFF), true),
                ("Exit", new Color32(0x5B, 0x8F, 0xD9, 0xFF), true),
                ("KillMelee", new Color32(0xD2, 0x63, 0x61, 0xFF), true),
                ("KillRanged", new Color32(0xE8, 0x96, 0x4A, 0xFF), true),
                ("Ammo", new Color32(0x9B, 0x6B, 0xD3, 0xFF), true),
                ("Memory", new Color32(0xE2, 0xBD, 0x73, 0xFF), true),
                ("Patrol", new Color32(0x4F, 0xC3, 0xD9, 0xFF), true),
                ("Static", new Color32(0xC9, 0xC2, 0xB8, 0xFF), true),
            };
            Shader _lit = Shader.Find("Universal Render Pipeline/Lit");
            foreach (var _def in _defs)
            {
                string _path = $"{k_materialsFolder}/M_Block_{_def.Name}.mat";
                var _mat = AssetDatabase.LoadAssetAtPath<Material>(_path);
                if (_mat == null)
                {
                    _mat = new Material(_lit);
                    AssetDatabase.CreateAsset(_mat, _path);
                }
                _mat.SetColor("_BaseColor", _def.Color);
                _mat.SetFloat("_Smoothness", 0.15f);
                if (_def.Emissive)
                {
                    _mat.EnableKeyword("_EMISSION");
                    _mat.SetColor("_EmissionColor", _def.Color * 0.8f);
                    _mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
                }
                EditorUtility.SetDirty(_mat);
                s_materials[_def.Name] = _mat;
            }

            // Checkered studio floor (black and cream, 0.5 m tiles via per-renderer tiling)
            string _checkerPath = $"{k_materialsFolder}/M_Block_Checker.mat";
            string _texPath = $"{k_materialsFolder}/T_Block_Checker.asset";
            var _tex = AssetDatabase.LoadAssetAtPath<Texture2D>(_texPath);
            if (_tex == null)
            {
                _tex = new Texture2D(2, 2, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Repeat };
                Color _a = new Color32(0xEC, 0xE0, 0xC4, 0xFF), _b = new Color32(0x2B, 0x24, 0x28, 0xFF);
                _tex.SetPixels(new[] { _a, _b, _b, _a });
                _tex.Apply();
                AssetDatabase.CreateAsset(_tex, _texPath);
            }
            var _checker = AssetDatabase.LoadAssetAtPath<Material>(_checkerPath);
            if (_checker == null)
            {
                _checker = new Material(_lit);
                AssetDatabase.CreateAsset(_checker, _checkerPath);
            }
            _checker.SetTexture("_BaseMap", _tex);
            _checker.SetColor("_BaseColor", Color.white);
            _checker.SetFloat("_Smoothness", 0.35f);
            EditorUtility.SetDirty(_checker);
            s_materials["Checker"] = _checker;

            // Old materials from the first blocking pass are no longer used.
            foreach (string _old in new[] { "Wall", "Cover", "Tall", "Floor", "Zone" })
                AssetDatabase.DeleteAsset($"{k_materialsFolder}/M_Block_{_old}.mat");

            // v4: the same palette as Flat Kit Stylized Surface materials (the Lit ones above stay for v1-v3).
            if (s_flat)
            {
                if (!AssetDatabase.IsValidFolder(flatFolder)) AssetDatabase.CreateFolder(k_materialsFolder, flatFolder.Substring(k_materialsFolder.Length + 1));
                foreach (string _name in s_materials.Keys.ToList())
                {
                    bool _emissive = _defs.Any(d => d.Name == _name && d.Emissive);
                    Color? _v5Color = s_look != null && s_look.Palette.TryGetValue(_name, out Color _c) ? _c : (Color?)null;
                    if (s_look != null && s_look.PaletteMixA != null && _defs.Any(d => d.Name == _name))
                    {
                        Color _base = _defs.First(d => d.Name == _name).Color;
                        Color _a = s_look.PaletteMixA.Palette.TryGetValue(_name, out Color _ca) ? _ca : _base;
                        Color _b = s_look.PaletteMixB.Palette.TryGetValue(_name, out Color _cb) ? _cb : _base;
                        _v5Color = Color.Lerp(_a, _b, s_look.PaletteMixT);
                    }
                    // v5 draws its outline in screen space (renderer feature), so no per-object outline
                    s_materials[_name] = flatFrom(s_materials[_name], $"{flatFolder}/{flatPrefix}_{_name}.mat", !s_v5 && !_emissive && _name != "Glass", _v5Color);
                }
                if (s_look != null && s_look.WoodFloors)
                {
                    // the "Checker" floors become walnut planks (1 texture tile = 1 m, so floor tiling keeps working)
                    Material _floor = s_materials["Checker"];
                    _floor.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(LastFrameArtSetup.k_woodPlanks));
                    _floor.SetColor("_BaseColor", Color.white);
                    EditorUtility.SetDirty(_floor);
                }
            }
        }

        // =====================================================================
        // Flat Kit (v4)
        // =====================================================================

        const string k_flatFolder = k_materialsFolder + "/FlatKit";
        static string flatFolder => s_look != null ? $"{k_materialsFolder}/FlatKit_{s_look.Id}" : k_flatFolder;
        static string flatPrefix => s_look != null ? "MAT_" + s_look.Id.ToUpper() : "M_Flat";
        static readonly Color k_shadowLavender = new Color32(0x8A, 0x78, 0xB0, 0xFF);
        /// <summary>v6 palette: a brighter, more saturated 80s cartoon studio (blue still kept for Ollie).</summary>
        static readonly Dictionary<string, Color> k_paletteV6 = new Dictionary<string, Color>
        {
            { "WallCream", new Color32(0xF4, 0xDF, 0xB0, 0xFF) },
            { "Wainscot", new Color32(0x8A, 0x4E, 0x3A, 0xFF) },
            { "CarpetRed", new Color32(0xB8, 0x3A, 0x45, 0xFF) },
            { "Wood", new Color32(0xB0, 0x6E, 0x42, 0xFF) },
            { "WoodLight", new Color32(0xE0, 0xA8, 0x6A, 0xFF) },
            { "Red", new Color32(0xD8, 0x3E, 0x3E, 0xFF) },
            { "Green", new Color32(0x72, 0xB8, 0x55, 0xFF) },
            { "Bush", new Color32(0x4F, 0x94, 0x4C, 0xFF) },
            { "Astro", new Color32(0x6A, 0xB0, 0x4A, 0xFF) },
            { "Sky", new Color32(0x5F, 0xC0, 0xA5, 0xFF) },
            { "Glass", new Color32(0x9F, 0xD8, 0xC8, 0xFF) },
            { "Screen", new Color32(0xC8, 0xF2, 0xD8, 0xFF) },
            { "Yellow", new Color32(0xF6, 0xC4, 0x4E, 0xFF) },
            { "Brass", new Color32(0xEC, 0xB4, 0x4C, 0xFF) },
            { "Paper", new Color32(0xFF, 0xF2, 0xD6, 0xFF) },
            { "InkPurple", new Color32(0x86, 0x5E, 0x96, 0xFF) },
            { "Concrete", new Color32(0xA8, 0x9E, 0x98, 0xFF) },
            { "Board", new Color32(0x3E, 0x6E, 0x55, 0xFF) },
        };
        static readonly Color k_shadowPurple = new Color32(0x3B, 0x2B, 0x52, 0xFF);
        /// <summary>v5 palette changes: blue is kept for Ollie's jacket, so the studio's blues become smoke green.</summary>
        static readonly Dictionary<string, Color> k_paletteV5 = new Dictionary<string, Color>
        {
            { "Sky", new Color32(0x6F, 0x8F, 0x80, 0xFF) },
            { "Glass", new Color32(0x8F, 0xB3, 0xA8, 0xFF) },
            { "Screen", new Color32(0xB8, 0xE6, 0xC8, 0xFF) },
            { "WallCream", new Color32(0xDD, 0xC6, 0x9A, 0xFF) },
            { "Wainscot", new Color32(0x56, 0x36, 0x2E, 0xFF) },
        };
        const string k_flatShader = "FlatKit/Stylized Surface";
        static readonly Color k_inkColor = new Color32(0x25, 0x1E, 0x23, 0xFF);
        static readonly string[] k_litShaders = { "Universal Render Pipeline/Lit", "Universal Render Pipeline/Simple Lit", "Universal Render Pipeline/Complex Lit" };

        /// <summary>
        /// Flat Kit copy of a Lit material: same color, texture and emission, one hard cel step with the shaded
        /// side darkened toward the studio ink, Unity shadows multiplied in, and an ink outline.
        /// </summary>
        static Material flatFrom(Material _src, string _path, bool _outline, Color? _override = null)
        {
            Shader _shader = Shader.Find(k_flatShader);
            var _mat = AssetDatabase.LoadAssetAtPath<Material>(_path);
            if (_mat == null)
            {
                _mat = new Material(_shader);
                AssetDatabase.CreateAsset(_mat, _path);
            }
            _mat.shader = _shader;
            _mat.shaderKeywords = new string[0];

            Color _color = _override ?? (_src.HasProperty("_BaseColor") ? _src.GetColor("_BaseColor") : _src.HasProperty("_Color") ? _src.GetColor("_Color") : Color.white);
            _color.a = 1f;
            if (s_v5 && _override == null)
            {
                // Blue is Ollie's color: strong blues elsewhere (the river, cloned props) become a dark teal
                Color.RGBToHSV(_color, out float _bh, out float _bs, out float _bv);
                if (_bh > 0.53f && _bh < 0.72f && _bs > 0.35f) _color = Color.HSVToRGB(0.47f, _bs * 0.6f, _bv * 0.8f);
            }
            _mat.SetColor("_BaseColor", _color);
            Texture _tex = _src.HasProperty("_BaseMap") ? _src.GetTexture("_BaseMap") : null;
            _mat.SetTexture("_BaseMap", _tex);
            if (_tex != null)
            {
                _mat.SetTextureScale("_BaseMap", _src.GetTextureScale("_BaseMap"));
                _mat.SetTextureOffset("_BaseMap", _src.GetTextureOffset("_BaseMap"));
            }

            // Cel shading: one hard step, the shaded side darker and tinted (v5+: by the look profile)
            bool _flatShading = s_look != null && s_look.FlatShading;
            if (!_flatShading)
            {
                _mat.SetFloat("_CelPrimaryMode", 1f);
                _mat.EnableKeyword("_CELPRIMARYMODE_SINGLE");
            }
            else _mat.SetFloat("_CelPrimaryMode", 0f); // completely flat color, like a TV cartoon background
            Color.RGBToHSV(_color, out float _h, out float _s, out float _v);
            Color _dim = s_look != null
                ? Color.Lerp(Color.HSVToRGB(_h, Mathf.Clamp01(_s * s_look.DimSaturation), _v * s_look.DimValue), s_look.ShadowTint, s_look.DimTint)
                : Color.Lerp(Color.HSVToRGB(_h, Mathf.Clamp01(_s * 1.15f), _v * 0.62f), k_inkColor, 0.18f);
            _dim.a = 1f;
            _mat.SetColor("_ColorDim", _dim);
            _mat.SetFloat("_SelfShadingSize", 0.45f);
            _mat.SetFloat("_ShadowEdgeSize", 0.02f);
            _mat.SetFloat("_Flatness", 0.85f);

            // Shadows from the sun and the studio lights, and spotlights tint the surface
            if (_flatShading)
            {
                _mat.SetFloat("_UnityShadowMode", 0f); // no shadows at all
            }
            else if (s_look != null)
            {
                _mat.SetFloat("_UnityShadowMode", 2f);
                _mat.EnableKeyword("_UNITYSHADOWMODE_COLOR");
                _mat.SetColor("_UnityShadowColor", Color.Lerp(_dim, s_look.ShadowTint, s_look.ShadowMix));
                if (s_look.Rim)
                {
                    // a soft rim, like the highlight line of a painted cel
                    _mat.SetFloat("_RimEnabled", 1f);
                    _mat.EnableKeyword("DR_RIM_ON");
                    _mat.SetColor("_FlatRimColor", s_look.RimColor);
                    _mat.SetFloat("_FlatRimLightAlign", 0.3f);
                    _mat.SetFloat("_FlatRimSize", 0.3f);
                    _mat.SetFloat("_FlatRimEdgeSmoothness", 0.1f);
                }
            }
            else
            {
                _mat.SetFloat("_UnityShadowMode", 1f);
                _mat.EnableKeyword("_UNITYSHADOWMODE_MULTIPLY");
            }
            _mat.SetFloat("_UnityShadowPower", 0.75f);
            _mat.SetFloat("_UnityShadowSharpness", 6f);
            _mat.SetFloat("_LightContribution", _flatShading ? 0f : 0.5f);
            _mat.SetFloat("_LightFalloffSize", 0.3f);

            if (_src.IsKeywordEnabled("_EMISSION"))
            {
                _mat.EnableKeyword("_EMISSION");
                _mat.SetColor("_EmissionColor", _src.GetColor("_EmissionColor"));
                if (_src.HasProperty("_EmissionMap")) _mat.SetTexture("_EmissionMap", _src.GetTexture("_EmissionMap"));
                _mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
            }

            _mat.SetFloat("_OutlineEnabled", _outline ? 1f : 0f);
            if (_outline)
            {
                _mat.EnableKeyword("DR_OUTLINE_ON");
                _mat.SetFloat("_OutlineWidth", 2.2f);
                _mat.SetColor("_OutlineColor", k_inkColor);
                _mat.SetFloat("_OutlineDepthOffset", 0f);
            }
            // Same as Flat Kit's material inspector: only active outline passes are drawn (keeps the SRP Batcher)
            _mat.SetShaderPassEnabled("Outline", _outline);
            _mat.SetShaderPassEnabled("SRPDEFAULTUNLIT", false);
            registerOutline(_mat, _outline);

            EditorUtility.SetDirty(_mat);
            return _mat;
        }

        /// <summary>
        /// Flat Kit draws per-object outlines with its "Per Object Outline" renderer feature. Its editor utility adds
        /// that feature to the active renderer on demand and registers the material, like ticking "Enable Outline".
        /// Called by reflection because Flat Kit's editor code is not referenced by this assembly.
        /// </summary>
        static void registerOutline(Material _mat, bool _active)
        {
            if (s_registerOutline == null)
            {
                System.Type _utils = System.AppDomain.CurrentDomain.GetAssemblies()
                    .Select(a => a.GetType("ObjectOutlineEditorUtils")).FirstOrDefault(t => t != null);
                s_registerOutline = _utils?.GetMethod("SetActive", new[] { typeof(Material), typeof(bool) });
                if (s_registerOutline == null) { Debug.LogWarning("[LastFrame] Flat Kit ObjectOutlineEditorUtils not found: no outlines."); return; }
            }
            s_registerOutline.Invoke(null, new object[] { _mat, _active });
        }
        static System.Reflection.MethodInfo s_registerOutline;

        /// <summary>
        /// v4: every opaque Lit material left in the scene (guards, cameras, props cloned from the test room) gets a
        /// Flat Kit copy in Materials/Blocking/FlatKit/Converted. Transparent ones (glass, water) stay as they are.
        /// </summary>
        static void convertToFlatKit(Scene _scene)
        {
            string _folder = flatFolder + "/Converted";
            if (!AssetDatabase.IsValidFolder(_folder)) AssetDatabase.CreateFolder(flatFolder, "Converted");
            var _cache = new Dictionary<Material, Material>();
            foreach (GameObject _root in _scene.GetRootGameObjects())
            {
                foreach (Renderer _r in _root.GetComponentsInChildren<Renderer>(true))
                {
                    if (_r is ParticleSystemRenderer || _r is LineRenderer || _r is TrailRenderer) continue;
                    if (isUnderLumenProbe(_r.transform)) continue; // Senses measures light with these: keep their materials
                    Material[] _mats = _r.sharedMaterials;
                    bool _changed = false;
                    for (int i = 0; i < _mats.Length; i++)
                    {
                        Material _m = _mats[i];
                        if (_m == null || !k_litShaders.Contains(_m.shader.name)) continue;
                        if (_m.HasProperty("_Surface") && _m.GetFloat("_Surface") > 0.5f) continue; // transparent
                        if (!_cache.TryGetValue(_m, out Material _flat))
                        {
                            string _safe = string.Concat(_m.name.Split(System.IO.Path.GetInvalidFileNameChars()));
                            _flat = flatFrom(_m, $"{_folder}/{flatPrefix}_{_safe}.mat", !s_v5);
                            _cache[_m] = _flat;
                        }
                        _mats[i] = _flat;
                        _changed = true;
                    }
                    if (_changed) _r.sharedMaterials = _mats;
                }
            }
        }

        // =====================================================================
        // v5 look (P1 first): night studio, practical lamps, grain, 2D sprites
        // =====================================================================

        static int s_index; // 1-based index of the prototype being built

        /// <summary>Scene volume: soft tonemapping, a bit more contrast, film grain and a dark vignette.</summary>
        static void lookVolumeV5(Transform _parent)
        {
            string _folder = $"{k_folder}/{sceneName(s_index)}";
            if (!AssetDatabase.IsValidFolder(_folder)) AssetDatabase.CreateFolder(k_folder, sceneName(s_index));
            string _path = $"{_folder}/VP_{sceneName(s_index).Replace(" ", "")}.asset";
            AssetDatabase.DeleteAsset(_path);
            var _profile = ScriptableObject.CreateInstance<UnityEngine.Rendering.VolumeProfile>();
            AssetDatabase.CreateAsset(_profile, _path);

            var _tone = _profile.Add<Tonemapping>(true);
            _tone.mode.Override(TonemappingMode.Neutral);
            var _color = _profile.Add<ColorAdjustments>(true);
            _color.postExposure.Override(s_look.Exposure);
            _color.contrast.Override(s_look.Contrast);
            _color.saturation.Override(s_look.Saturation);
            _color.colorFilter.Override(s_look.ColorFilter);
            var _bloom = _profile.Add<Bloom>(true);
            _bloom.threshold.Override(0.95f);
            _bloom.intensity.Override(s_look.Bloom);
            _bloom.scatter.Override(0.6f);
            var _vignette = _profile.Add<Vignette>(true);
            _vignette.color.Override(s_look.VignetteColor);
            _vignette.intensity.Override(s_look.Vignette);
            _vignette.smoothness.Override(0.45f);
            var _grain = _profile.Add<FilmGrain>(true);
            _grain.type.Override(FilmGrainLookup.Medium3);
            var _paper = s_look.PaperGrain ? AssetDatabase.LoadAssetAtPath<Texture2D>(LastFrameArtSetup.k_paperGrain) : null;
            if (_paper != null)
            {
                _grain.type.Override(FilmGrainLookup.Custom); // paper fibers instead of film noise
                _grain.texture.Override(_paper);
            }
            _grain.intensity.Override(s_look.Grain);
            _grain.response.Override(0.75f);
            foreach (var _component in _profile.components) AssetDatabase.AddObjectToAsset(_component, _profile);
            EditorUtility.SetDirty(_profile);

            var _go = new GameObject($"Global Volume (look {s_look.Id})");
            _go.transform.SetParent(_parent);
            var _volume = _go.AddComponent<UnityEngine.Rendering.Volume>();
            _volume.isGlobal = true;
            _volume.sharedProfile = _profile;
        }

        /// <summary>
        /// P1 practical lamps: warm pools where the guards watch (so Lumen still exposes Ollie there) and darker gaps
        /// between them, a cold TV glow in the script room and a red light at the exit.
        /// </summary>
        static void p1LampsV5()
        {
            s_lampIndex = 0;
            Color _warm = new Color(1f, 0.78f, 0.5f);
            // Recepción: the receptionist's desk is the brightest spot
            ceilingLamp(V(-4f, 4f), _warm, 4f, 7f);
            ceilingLamp(V(4f, 4f), _warm, 4f, 7f);
            ceilingLamp(V(0f, 9.6f), _warm, 6f, 8f);
            // Pasillo de oficinas: sconces with dark gaps for the patrol
            ceilingLamp(V(-6f, 18f), _warm, 4.5f, 6f);
            ceilingLamp(V(-6f, 30f), _warm, 4.5f, 6f);
            ceilingLamp(V(-6f, 41f), _warm, 4.5f, 6f);
            // Animación: lamps over the desks
            ceilingLamp(V(0.5f, 20.5f), _warm, 4f, 6.5f);
            ceilingLamp(V(4.5f, 20.5f), _warm, 4f, 6.5f);
            ceilingLamp(V(6.5f, 25.5f), new Color(1f, 0.86f, 0.55f), 3f, 4f); // the tape corner
            // Sala de guiones: lamp over the table, cold TV glow on the watcher
            ceilingLamp(V(1.5f, 32.5f), _warm, 5f, 7f);
            pointLight(s_lights, new Vector3(5.1f, 1.2f, 38.9f), new Color(0.65f, 0.95f, 0.85f), 3f, 4.5f);
            // Camerinos
            ceilingLamp(V(-2f, 45f), _warm, 3.5f, 6f);
            ceilingLamp(V(9f, 45f), _warm, 3.5f, 6f);
            ceilingLamp(V(-4.5f, 52.5f), new Color(1f, 0.85f, 0.65f), 4f, 6f);
            ceilingLamp(V(3.5f, 52.5f), new Color(1f, 0.85f, 0.65f), 4f, 6f);
            ceilingLamp(V(10.5f, 52f), _warm, 3.5f, 6f);
            pointLight(s_lights, new Vector3(10.5f, 2.6f, 58.6f), new Color(1f, 0.25f, 0.2f), 4f, 5f); // ON AIR at the exit

            lumenProbe(V(0f, 6f)); lumenProbe(V(-6f, 24f)); lumenProbe(V(1.5f, 33f)); lumenProbe(V(10.5f, 52f));
        }

        /// <summary>A ceiling light with no visible fixture: from the high camera a hanging lamp only hides the floor.</summary>
        static void ceilingLamp(Vector3 _pos, Color _color, float _intensity, float _range)
        {
            if (s_look.LampScale <= 0f) return;
            Transform _t = child(s_lights, "Luz de techo");
            _t.position = _pos;
            if (s_look.LampColors != null) _color = s_look.LampColors[s_lampIndex++ % s_look.LampColors.Length];
            pointLight(_t, new Vector3(0f, 2.9f, 0f), _color, _intensity * s_look.LampScale, _range * 1.15f);
        }

        /// <summary>
        /// 2D placeholder sprites over the working Senses objects: the 3D meshes are hidden (colliders and senses
        /// stay), a sprite that faces the camera stands at the feet, with a flat drop shadow (red for enemies).
        /// </summary>
        static void applyLookV5()
        {
            bool _outlined = s_look.OutlinedSprites;
            var _ollie = AssetDatabase.LoadAssetAtPath<Sprite>(_outlined ? LastFrameArtSetup.k_spriteOllieOutlined : LastFrameArtSetup.k_spriteOllie);
            var _otter = AssetDatabase.LoadAssetAtPath<Sprite>(_outlined ? LastFrameArtSetup.k_spriteOtterOutlined : LastFrameArtSetup.k_spriteOtter);
            var _capybara = AssetDatabase.LoadAssetAtPath<Sprite>(_outlined ? LastFrameArtSetup.k_spriteCapybaraOutlined : LastFrameArtSetup.k_spriteCapybara);
            if (_ollie == null || _otter == null || _capybara == null)
            {
                Debug.LogWarning("[LastFrame] Placeholder sprites missing: run Tools > Last Frame > Build Placeholder Sprites.");
                return;
            }
            // P2-P5 keep their v3 lights; in the darker v5 studio they need a bit more reach
            if (s_index > 1 && s_version < 10) // v10: no boost, darkness is wanted
            {
                foreach (Light _l in s_lights.GetComponentsInChildren<Light>(true).Concat(s_props.GetComponentsInChildren<Light>(true)))
                {
                    if (_l.type == LightType.Directional) continue;
                    _l.intensity *= 1.4f;
                    _l.range *= 1.15f;
                }
            }

            // The prop river uses the Senses pack's palette texture (bright blue): give it its own dark teal water
            Material _water = null;
            foreach (Renderer _r in s_props.GetComponentsInChildren<Renderer>(true).Where(r => r.transform.parent != null && r.transform.parent.name.Contains("River")))
            {
                if (_water == null) _water = flatFrom(s_materials["Glass"], $"{flatFolder}/{flatPrefix}_Water.mat", false, new Color32(0x2C, 0x55, 0x55, 0xFF));
                _r.sharedMaterials = Enumerable.Repeat(_water, _r.sharedMaterials.Length).ToArray();
            }

            // Colored drop shadows (the look decides: ink, lavender, neon...)
            Material _ollieShadow = flatFrom(s_materials["Ink"], $"{flatFolder}/{flatPrefix}_ShadowOllie.mat", false, s_look.OllieShadow);
            Material _enemyShadow = flatFrom(s_materials["Red"], $"{flatFolder}/{flatPrefix}_ShadowEnemy.mat", false, s_look.EnemyShadow);
            if (s_look.EnemyShadowGlow)
            {
                _enemyShadow.EnableKeyword("_EMISSION");
                _enemyShadow.SetColor("_EmissionColor", s_look.EnemyShadow * 1.6f);
                _enemyShadow.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
                EditorUtility.SetDirty(_enemyShadow);
            }

            if (s_player != null) characterSprite(s_player, _ollie, 1.45f, _ollieShadow, false, false);
            foreach (Transform _enemy in s_enemies)
            {
                bool _otterDesign = _enemy.name.StartsWith("Nutria");
                characterSprite(_enemy, _otterDesign ? _otter : _capybara, 2.1f, _enemyShadow, _otterDesign, true);
                // v7b: the AI cartoons bring their own cold neon light into the warm studio
                // (v8: small, so standing next to an enemy does not light Ollie up)
                if (s_look.NeonAI) pointLight(_enemy, new Vector3(0f, 1.6f, 0f), s_look.EnemyShadow, s_version >= 8 ? 2f : 9f, s_version >= 8 ? 2.2f : 5.5f);
            }
            if (s_look.SketchProps) sketchProps();
            if (s_look.SheetProps) sheetProps();
        }

        static void characterSprite(Transform _owner, Sprite _sprite, float _height, Material _shadowMaterial, bool _flipWithFacing, bool _artFacesLeft)
        {
            Renderer[] _renderers = _owner.GetComponentsInChildren<Renderer>(true)
                .Where(r => r is MeshRenderer || r is SkinnedMeshRenderer).ToArray();
            Vector3 _feet = _owner.position;
            if (_renderers.Length > 0)
            {
                Bounds _b = _renderers[0].bounds;
                foreach (Renderer _r in _renderers) _b.Encapsulate(_r.bounds);
                _feet = new Vector3(_owner.position.x, _b.min.y, _owner.position.z);
            }
            foreach (Renderer _r in _renderers) _r.enabled = false;

            var _go = new GameObject("SPR " + _sprite.name.Replace("SPR_", "").Replace("_Placeholder", "") + " (placeholder)");
            _go.transform.SetParent(_owner, false);
            _go.transform.position = _feet;
            _go.transform.rotation = Quaternion.Euler(57f, 0f, 0f);
            _go.transform.localScale = Vector3.one * (_height / Mathf.Max(0.01f, _owner.lossyScale.y));
            var _sr = _go.AddComponent<SpriteRenderer>();
            _sr.sprite = _sprite;
            _sr.flipX = _artFacesLeft;
            var _face = _go.AddComponent<LastFrame.Visual.FaceCameraSprite>();
            var _so = new SerializedObject(_face);
            _so.FindProperty("m_flipWithFacing").boolValue = _flipWithFacing;
            _so.FindProperty("m_artFacesLeft").boolValue = _artFacesLeft;
            _so.ApplyModifiedPropertiesWithoutUndo();

            var _shadow = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            _shadow.name = "Sombra (sprite)";
            Object.DestroyImmediate(_shadow.GetComponent<Collider>());
            _shadow.transform.SetParent(_owner, false);
            _shadow.transform.position = new Vector3(_feet.x, 0.015f, _feet.z);
            Vector3 _ls = _owner.lossyScale;
            _shadow.transform.localScale = new Vector3(_height * 0.45f / _ls.x, 0.002f / _ls.y, _height * 0.28f / _ls.z);
            var _mr = _shadow.GetComponent<MeshRenderer>();
            _mr.sharedMaterial = _shadowMaterial;
            _mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        // =====================================================================
        // Look profiles (v5+): everything a version or variant changes in light, color, shading and post
        // =====================================================================

        sealed class LookProfile
        {
            public string Id;
            public float SunDay = 0.45f, SunDim = 0.3f, SunNight = 0.25f;
            public Color SunColor = new Color(0.72f, 0.68f, 0.95f);
            public Color AmbientDay = new Color(0.2f, 0.17f, 0.26f), AmbientDim = new Color(0.18f, 0.15f, 0.24f), AmbientNight = new Color(0.17f, 0.15f, 0.25f);
            public Color Background = new Color32(0x1C, 0x14, 0x1A, 0xFF);
            public Dictionary<string, Color> Palette = new Dictionary<string, Color>();
            /// <summary>No cel step, no shadows, lights ignored: a flat TV cartoon background.</summary>
            public bool FlatShading;
            public float DimValue = 0.55f, DimSaturation = 1.1f, DimTint = 0.38f, ShadowMix = 0.5f;
            public Color ShadowTint = new Color32(0x3B, 0x2B, 0x52, 0xFF);
            public bool Rim;
            public Color RimColor;
            public float LampScale = 1.7f;
            public Color[] LampColors; // null = each lamp keeps its own warm color
            public string RendererName = "Look";
            public float Exposure = 0.15f, Contrast = 12f, Saturation = 8f, Bloom = 0.6f, Vignette = 0.32f, Grain = 0.35f;
            public Color VignetteColor = new Color32(0x1C, 0x14, 0x1A, 0xFF), ColorFilter = new Color(1f, 0.96f, 0.9f);
            public bool PaperGrain, SketchProps, NeonAI, OutlinedSprites, EnemyShadowGlow;
            /// <summary>v9: plank floors instead of checkers, furniture under dust sheets, light through windows.</summary>
            public bool WoodFloors, SheetProps, Windows;
            public Color OllieShadow = new Color32(0x25, 0x1E, 0x23, 0xFF), EnemyShadow = new Color32(0x9E, 0x2B, 0x2B, 0xFF);
            // v8 motivated lighting: old tungsten lamps, the AI's security lights, fluorescent tubes
            public Color WarmLight = new Color(1f, 0.78f, 0.5f), AiLight = new Color(0.55f, 0.95f, 1f), TubeLight = new Color(0.9f, 0.96f, 0.78f);
            /// <summary>If set, each palette color is Lerp(MixA, MixB, MixT) (a missing entry uses the base color).</summary>
            public LookProfile PaletteMixA, PaletteMixB;
            public float PaletteMixT = 0.5f;
        }

        static int s_lampIndex;

        static LookProfile s_look
        {
            get
            {
                if (s_version < 5) return null;
                if (s_version == 5) return k_lookV5;
                if (s_version == 6) return k_lookV6;
                if (s_version == 10) return k_lookV10;
                if (s_version == 9) return k_lookV9;
                if (s_version == 8) return s_variant == "a" ? k_lookV8Soul : s_variant == "b" ? k_lookV8Pop : s_variant == "c" ? k_lookV8Night : k_lookV8Between;
                switch (s_variant)
                {
                    case "a": return k_lookPencil;
                    case "b": return k_lookSoul;
                    case "c": return k_lookPopDusk;
                    default: return k_lookFlatTv;
                }
            }
        }

        // v5: studio at night, warm practical lamps, purple shadows, grain and vignette
        static readonly LookProfile k_lookV5 = new LookProfile { Id = "v5", Palette = k_paletteV5 };

        // v6: lit studio, brighter 80s palette, light lavender shadows with a rim, light post
        static readonly LookProfile k_lookV6 = new LookProfile
        {
            Id = "v6", SunDay = 1.3f, SunColor = new Color(1f, 0.93f, 0.8f), AmbientDay = new Color(0.62f, 0.56f, 0.6f),
            Background = new Color32(0x3B, 0x27, 0x33, 0xFF), Palette = k_paletteV6,
            DimValue = 0.78f, DimSaturation = 1.05f, DimTint = 0.3f, ShadowTint = new Color32(0x8A, 0x78, 0xB0, 0xFF), ShadowMix = 0.25f,
            Rim = true, RimColor = new Color(1f, 0.95f, 0.85f) * 0.35f, LampScale = 0.6f,
            Exposure = 0.1f, Contrast = 6f, Saturation = 22f, Bloom = 0.35f, Vignette = 0.12f, VignetteColor = new Color32(0x3B, 0x27, 0x33, 0xFF), Grain = 0.12f,
            EnemyShadow = new Color32(0xD8, 0x3E, 0x3E, 0xFF),
        };

        // v7a: pencil and paper. A hand-painted animation background: soft paper colors, almost no shading,
        // graphite lines (also between color areas), paper-fiber grain and some props left as uncolored sketches.
        static readonly LookProfile k_lookPencil = new LookProfile
        {
            Id = "v7a", SunDay = 0.9f, SunColor = new Color(1f, 0.95f, 0.86f), AmbientDay = new Color(0.52f, 0.49f, 0.47f),
            Background = new Color32(0xE6, 0xDA, 0xC2, 0xFF),
            Palette = new Dictionary<string, Color>
            {
                { "WallCream", new Color32(0xF1, 0xE4, 0xC6, 0xFF) }, { "Wainscot", new Color32(0xA8, 0x78, 0x60, 0xFF) },
                { "CarpetRed", new Color32(0xC4, 0x68, 0x60, 0xFF) }, { "Wood", new Color32(0xC0, 0x8C, 0x60, 0xFF) },
                { "WoodLight", new Color32(0xE4, 0xC0, 0x8E, 0xFF) }, { "Red", new Color32(0xD2, 0x66, 0x5C, 0xFF) },
                { "Green", new Color32(0x92, 0xB8, 0x74, 0xFF) }, { "Bush", new Color32(0x78, 0x9C, 0x68, 0xFF) },
                { "Sky", new Color32(0x92, 0xC8, 0xB8, 0xFF) }, { "Glass", new Color32(0xB8, 0xDA, 0xD0, 0xFF) },
                { "Yellow", new Color32(0xEE, 0xD0, 0x80, 0xFF) }, { "Brass", new Color32(0xE0, 0xBA, 0x6E, 0xFF) },
                { "Paper", new Color32(0xFB, 0xF4, 0xE4, 0xFF) }, { "InkPurple", new Color32(0x9A, 0x80, 0xA4, 0xFF) },
                { "Concrete", new Color32(0xB4, 0xAC, 0xA4, 0xFF) }, { "Board", new Color32(0x5E, 0x80, 0x6C, 0xFF) },
                { "Screen", new Color32(0xD0, 0xEE, 0xDA, 0xFF) }, { "Ink", new Color32(0x4A, 0x40, 0x46, 0xFF) },
            },
            DimValue = 0.88f, DimSaturation = 0.95f, DimTint = 0.2f, ShadowTint = new Color32(0x9A, 0x8C, 0xA8, 0xFF), ShadowMix = 0.3f,
            LampScale = 0.4f, RendererName = "Pencil",
            Exposure = -0.35f, Contrast = 10f, Saturation = 4f, Bloom = 0.1f, Vignette = 0.22f, VignetteColor = new Color32(0x8A, 0x76, 0x5E, 0xFF),
            ColorFilter = new Color(1f, 0.97f, 0.9f), Grain = 0.9f, PaperGrain = true, SketchProps = true, OutlinedSprites = true,
            OllieShadow = new Color32(0x9A, 0x8A, 0xA6, 0xFF), EnemyShadow = new Color32(0xB8, 0x5A, 0x50, 0xFF),
        };

        // v7b: soul vs algorithm. The classic studio is warm and drawn; the AI cartoons bring cold, glossy neon.
        static readonly LookProfile k_lookSoul = new LookProfile
        {
            Id = "v7b", SunDay = 0.65f, SunColor = new Color(1f, 0.86f, 0.68f), AmbientDay = new Color(0.36f, 0.31f, 0.33f),
            Background = new Color32(0x2A, 0x1E, 0x26, 0xFF), Palette = k_paletteV6,
            DimValue = 0.7f, DimSaturation = 1.05f, DimTint = 0.3f, ShadowTint = new Color32(0x7A, 0x62, 0x8A, 0xFF), ShadowMix = 0.35f,
            Rim = true, RimColor = new Color(1f, 0.9f, 0.75f) * 0.25f, LampScale = 1.0f,
            Exposure = 0.1f, Contrast = 8f, Saturation = 0f, Bloom = 1.2f, Vignette = 0.22f, VignetteColor = new Color32(0x2A, 0x1E, 0x26, 0xFF), Grain = 0.2f,
            ColorFilter = new Color(1f, 0.93f, 0.82f), // the hand-made studio leans sepia, so the AI neon is the only cold color
            NeonAI = true, OutlinedSprites = true, EnemyShadowGlow = true,
            OllieShadow = new Color32(0x5A, 0x48, 0x5E, 0xFF), EnemyShadow = new Color32(0x30, 0xE0, 0xFF, 0xFF),
        };

        // v7c: pop dusk. Between v5 and v6: half light with saturated colored lamps (magenta, teal, amber), spy-pop mood.
        static readonly LookProfile k_lookPopDusk = new LookProfile
        {
            Id = "v7c", SunDay = 0.5f, SunColor = new Color(1f, 0.62f, 0.78f), AmbientDay = new Color(0.33f, 0.25f, 0.41f),
            Background = new Color32(0x24, 0x18, 0x2E, 0xFF),
            Palette = new Dictionary<string, Color>
            {
                { "WallCream", new Color32(0xEA, 0xCC, 0xA4, 0xFF) }, { "Wainscot", new Color32(0x6E, 0x36, 0x56, 0xFF) },
                { "CarpetRed", new Color32(0xC8, 0x30, 0x5E, 0xFF) }, { "Wood", new Color32(0xAC, 0x62, 0x3C, 0xFF) },
                { "WoodLight", new Color32(0xE0, 0x9C, 0x5C, 0xFF) }, { "Red", new Color32(0xE2, 0x34, 0x4E, 0xFF) },
                { "Green", new Color32(0x4C, 0xB4, 0x6E, 0xFF) }, { "Sky", new Color32(0x2C, 0xB8, 0xA6, 0xFF) },
                { "Glass", new Color32(0x8C, 0xDA, 0xCC, 0xFF) }, { "Yellow", new Color32(0xFF, 0xC4, 0x34, 0xFF) },
                { "Brass", new Color32(0xF0, 0xAE, 0x3E, 0xFF) }, { "InkPurple", new Color32(0x7C, 0x4A, 0x9C, 0xFF) },
                { "Screen", new Color32(0xB0, 0xFF, 0xE0, 0xFF) },
            },
            DimValue = 0.6f, DimSaturation = 1.2f, DimTint = 0.35f, ShadowTint = new Color32(0x4A, 0x2A, 0x6A, 0xFF), ShadowMix = 0.45f,
            Rim = true, RimColor = new Color(1f, 0.6f, 0.85f) * 0.3f, LampScale = 1.4f,
            LampColors = new[] { new Color(1f, 0.32f, 0.72f), new Color(0.2f, 0.9f, 0.85f), new Color(1f, 0.72f, 0.3f) },
            Exposure = 0.15f, Contrast = 10f, Saturation = 25f, Bloom = 0.8f, Vignette = 0.22f, VignetteColor = new Color32(0x24, 0x18, 0x2E, 0xFF),
            ColorFilter = new Color(1f, 0.95f, 1f), Grain = 0.15f, OutlinedSprites = true, EnemyShadowGlow = true,
            OllieShadow = new Color32(0x3A, 0x24, 0x4A, 0xFF), EnemyShadow = new Color32(0xFF, 0x3A, 0x6A, 0xFF),
        };

        // v7d: flat TV cartoon. Saturday-morning background: flat saturated colors, thick black ink, no shadows.
        static readonly LookProfile k_lookFlatTv = new LookProfile
        {
            Id = "v7d", FlatShading = true, SunDay = 1f, SunColor = Color.white, AmbientDay = new Color(0.5f, 0.5f, 0.5f),
            Background = new Color32(0x2A, 0x24, 0x38, 0xFF),
            Palette = new Dictionary<string, Color>
            {
                { "WallCream", new Color32(0xFF, 0xE4, 0xA4, 0xFF) }, { "Wainscot", new Color32(0xE0, 0x62, 0x3C, 0xFF) },
                { "CarpetRed", new Color32(0xE8, 0x38, 0x50, 0xFF) }, { "Wood", new Color32(0xD8, 0x88, 0x40, 0xFF) },
                { "WoodLight", new Color32(0xF6, 0xB8, 0x5A, 0xFF) }, { "Red", new Color32(0xF2, 0x38, 0x3C, 0xFF) },
                { "Green", new Color32(0x5C, 0xC4, 0x4A, 0xFF) }, { "Bush", new Color32(0x3E, 0xA8, 0x44, 0xFF) },
                { "Sky", new Color32(0x40, 0xD2, 0xB0, 0xFF) }, { "Glass", new Color32(0xA8, 0xEC, 0xDC, 0xFF) },
                { "Yellow", new Color32(0xFF, 0xD4, 0x40, 0xFF) }, { "Brass", new Color32(0xFF, 0xBA, 0x3A, 0xFF) },
                { "Paper", new Color32(0xFF, 0xFF, 0xFF, 0xFF) }, { "Concrete", new Color32(0xBC, 0xB2, 0xAC, 0xFF) },
                { "Board", new Color32(0x2E, 0x8C, 0x58, 0xFF) }, { "InkPurple", new Color32(0x9C, 0x5C, 0xC4, 0xFF) },
                { "ConcreteDark", new Color32(0x6E, 0x66, 0x70, 0xFF) },
            },
            LampScale = 0f, RendererName = "FlatTV",
            Exposure = 0f, Contrast = 4f, Saturation = 15f, Bloom = 0.1f, Vignette = 0f, Grain = 0f, ColorFilter = Color.white, OutlinedSprites = true,
            OllieShadow = new Color32(0x2A, 0x2A, 0x40, 0xFF), EnemyShadow = new Color32(0xE8, 0x38, 0x3A, 0xFF),
        };

        static bool isUnderLumenProbe(Transform _t)
        {
            for (; _t != null; _t = _t.parent)
                if (_t.name.Contains("LumenProbe")) return true;
            return false;
        }

        /// <summary>
        /// v7a: one prop in four is left as an uncolored sketch (paper white), like an unfinished background drawing
        /// (GDD: "some sketches finished and others half done"). Doors, facades and signs stay painted.
        /// </summary>
        /// <summary>v9: one prop in three is covered by a dust sheet (ivory), like the abandoned studio of the concept art.</summary>
        /// <summary>Props that carry gameplay (Senses objects or trigger zones) are never covered or recolored.</summary>
        static bool isGameplayProp(Transform _prop) =>
            _prop.GetComponentsInChildren<Collider>(true).Any(c => c.isTrigger) ||
            _prop.GetComponentsInChildren<MonoBehaviour>(true).Any(m => m != null && m.GetType().Namespace != null && m.GetType().Namespace.StartsWith("Senses"));

        static void sheetProps()
        {
            Material _sheet = flatFrom(s_materials["Paper"], $"{flatFolder}/{flatPrefix}_Sheet.mat", false, new Color32(0xE6, 0xD4, 0xB6, 0xFF));
            int i = 0;
            foreach (Transform _prop in s_props)
            {
                if (k_unscaledProps.Any(n => _prop.name.StartsWith(n))) continue;
                if (isGameplayProp(_prop)) continue; // grass, river, trash, fish tanks...: they must stay readable
                if (i++ % 3 != 1) continue;
                foreach (Renderer _r in _prop.GetComponentsInChildren<Renderer>(true))
                {
                    if (!(_r is MeshRenderer) || isUnderLumenProbe(_r.transform)) continue;
                    if (_r.sharedMaterials.Any(m => m != null && m.IsKeywordEnabled("_EMISSION"))) continue;
                    _r.sharedMaterials = Enumerable.Repeat(_sheet, _r.sharedMaterials.Length).ToArray();
                }
            }
        }

        /// <summary>A window in a wall: frame, glowing dusty panes and a warm shaft (spot from outside) onto the floor.</summary>
        static void windowLight(Vector3 _wallPos, Vector3 _target)
        {
            Transform _t = lightRig("Ventana polvorienta", _wallPos);
            bool _eastWall = _wallPos.x > 0f;
            Quaternion _face = Quaternion.Euler(0f, _eastWall ? -90f : 90f, 0f);
            var _frame = new GameObject("Marco").transform;
            _frame.SetParent(_t, false);
            _frame.localRotation = _face;
            box(_frame, Vector3.zero, new Vector3(1.9f, 1.5f, 0.08f), "Wood", false);
            box(_frame, new Vector3(0f, 0f, 0.02f), new Vector3(1.7f, 1.3f, 0.06f), glowName("Window", new Color(1f, 0.88f, 0.62f)), false);
            box(_frame, new Vector3(0f, 0f, 0.06f), new Vector3(0.07f, 1.3f, 0.04f), "Wood", false);
            box(_frame, new Vector3(0f, 0f, 0.06f), new Vector3(1.7f, 0.07f, 0.04f), "Wood", false);
            Color _sun = new Color(1f, 0.82f, 0.55f);
            Light _light = spot(_t, new Vector3(_eastWall ? 1.5f : -1.5f, 1.2f, 0f), _target, _sun, 14f, 9f, 34f, true);
            lightVisuals(_light, _sun, 0.3f, false); // the cone would start outside the wall: pool only
        }

        static void sketchProps()
        {
            Material _sketch = flatFrom(s_materials["Paper"], $"{flatFolder}/{flatPrefix}_Sketch.mat", false, new Color32(0xFA, 0xF4, 0xE6, 0xFF));
            int i = 0;
            foreach (Transform _prop in s_props)
            {
                if (k_unscaledProps.Any(n => _prop.name.StartsWith(n))) continue;
                if (i++ % 4 != 1) continue;
                foreach (Renderer _r in _prop.GetComponentsInChildren<Renderer>(true))
                {
                    if (!(_r is MeshRenderer) || isUnderLumenProbe(_r.transform)) continue;
                    if (_r.sharedMaterials.Any(m => m != null && m.IsKeywordEnabled("_EMISSION"))) continue;
                    _r.sharedMaterials = Enumerable.Repeat(_sketch, _r.sharedMaterials.Length).ToArray();
                }
            }
        }

        // =====================================================================
        // v8: motivated lighting (P1 first). The studio is abandoned and the AI has switched some areas back on.
        // Every light comes from a visible fixture, draws its cone and floor pool, and is a real light that Senses
        // (Lumen) measures: lit = exposed, dark = hidden. Some old lights fail and flicker: timing windows.
        // =====================================================================

        // v8a: v7b base (warm sepia studio, the AI brings cold cyan)
        static readonly LookProfile k_lookV8Soul = new LookProfile
        {
            Id = "v8a", SunDay = 0.12f, SunColor = new Color(0.6f, 0.65f, 0.9f), AmbientDay = new Color(0.11f, 0.09f, 0.12f),
            Background = new Color32(0x1E, 0x16, 0x1C, 0xFF), Palette = k_paletteV6,
            DimValue = 0.7f, DimSaturation = 1.05f, DimTint = 0.3f, ShadowTint = new Color32(0x7A, 0x62, 0x8A, 0xFF), ShadowMix = 0.35f,
            Rim = true, RimColor = new Color(1f, 0.9f, 0.75f) * 0.2f,
            Exposure = 0.25f, Contrast = 8f, Saturation = 0f, Bloom = 1.1f, Vignette = 0.25f, VignetteColor = new Color32(0x1E, 0x16, 0x1C, 0xFF),
            ColorFilter = new Color(1f, 0.93f, 0.82f), Grain = 0.2f,
            NeonAI = true, OutlinedSprites = true, EnemyShadowGlow = true,
            OllieShadow = new Color32(0x5A, 0x48, 0x5E, 0xFF), EnemyShadow = new Color32(0x30, 0xE0, 0xFF, 0xFF),
            WarmLight = new Color(1f, 0.72f, 0.42f), AiLight = new Color(0.72f, 0.95f, 1f), TubeLight = new Color(0.86f, 0.95f, 0.7f), // pale: lights every floor color (Senses measures floor brightness)
        };

        // v8b: v7c base (spy-pop: amber old lamps, magenta AI, teal tubes)
        static readonly LookProfile k_lookV8Pop = new LookProfile
        {
            Id = "v8b", SunDay = 0.15f, SunColor = new Color(0.8f, 0.5f, 0.9f), AmbientDay = new Color(0.14f, 0.1f, 0.17f),
            Background = new Color32(0x1A, 0x12, 0x22, 0xFF), Palette = k_lookPopDusk.Palette,
            DimValue = 0.6f, DimSaturation = 1.2f, DimTint = 0.35f, ShadowTint = new Color32(0x4A, 0x2A, 0x6A, 0xFF), ShadowMix = 0.45f,
            Rim = true, RimColor = new Color(1f, 0.6f, 0.85f) * 0.25f,
            Exposure = 0.25f, Contrast = 10f, Saturation = 22f, Bloom = 0.9f, Vignette = 0.25f, VignetteColor = new Color32(0x1A, 0x12, 0x22, 0xFF),
            ColorFilter = new Color(1f, 0.95f, 1f), Grain = 0.15f,
            NeonAI = true, OutlinedSprites = true, EnemyShadowGlow = true,
            OllieShadow = new Color32(0x3A, 0x24, 0x4A, 0xFF), EnemyShadow = new Color32(0xFF, 0x3A, 0x8A, 0xFF),
            WarmLight = new Color(1f, 0.68f, 0.3f), AiLight = new Color(1f, 0.62f, 0.88f), TubeLight = new Color(0.62f, 0.98f, 0.9f),
        };

        // v8c: v5 base (night studio, warm tungsten, cold white AI)
        static readonly LookProfile k_lookV8Night = new LookProfile
        {
            Id = "v8c", SunDay = 0.1f, SunColor = new Color(0.6f, 0.62f, 0.9f), AmbientDay = new Color(0.09f, 0.08f, 0.12f),
            Background = new Color32(0x12, 0x0D, 0x11, 0xFF), Palette = k_paletteV5,
            Exposure = 0.3f, Contrast = 12f, Saturation = 6f, Bloom = 0.7f, Vignette = 0.3f, Grain = 0.3f,
            NeonAI = true, OutlinedSprites = true, EnemyShadowGlow = true,
            EnemyShadow = new Color32(0xD8, 0xFF, 0xF0, 0xFF),
            WarmLight = new Color(1f, 0.75f, 0.45f), AiLight = new Color(0.85f, 1f, 0.95f), TubeLight = new Color(0.8f, 0.9f, 0.75f),
        };

        // v8d: halfway between v8b (pop dusk) and v8c (night): a bit of the pop color and mood, a bit of the night's
        // restraint. Every value is the average of the two; the palette is mixed 50/50.
        static readonly LookProfile k_lookV8Between = new LookProfile
        {
            Id = "v8d", SunDay = 0.125f, SunColor = new Color(0.7f, 0.56f, 0.9f), AmbientDay = new Color(0.115f, 0.09f, 0.145f),
            Background = new Color32(0x16, 0x0F, 0x19, 0xFF), PaletteMixA = k_lookV8Pop, PaletteMixB = k_lookV8Night,
            DimValue = 0.575f, DimSaturation = 1.15f, DimTint = 0.365f, ShadowTint = new Color32(0x42, 0x2A, 0x5E, 0xFF), ShadowMix = 0.475f,
            Rim = true, RimColor = new Color(1f, 0.6f, 0.85f) * 0.12f,
            Exposure = 0.275f, Contrast = 11f, Saturation = 14f, Bloom = 0.8f, Vignette = 0.275f, VignetteColor = new Color32(0x1A, 0x12, 0x1E, 0xFF),
            ColorFilter = new Color(1f, 0.955f, 0.95f), Grain = 0.22f,
            NeonAI = true, OutlinedSprites = true, EnemyShadowGlow = true,
            OllieShadow = new Color32(0x30, 0x21, 0x36, 0xFF), EnemyShadow = new Color32(0xEC, 0x9C, 0xBD, 0xFF),
            WarmLight = new Color(1f, 0.715f, 0.375f), AiLight = new Color(0.925f, 0.81f, 0.915f), TubeLight = new Color(0.71f, 0.94f, 0.825f),
        };

        // v9: the GDD concept art and its official palette (Paleta de Colores). Warm wooden studio, plum shadows,
        // ivory dust sheets, amber/brass light, muted red for alerts and enemies, slate blue kept for Ollie.
        static readonly LookProfile k_lookV9 = new LookProfile
        {
            Id = "v9", SunDay = 0.15f, SunColor = new Color(1f, 0.85f, 0.65f), AmbientDay = new Color(0.27f, 0.21f, 0.21f),
            Background = new Color32(0x25, 0x1E, 0x23, 0xFF), // café tinta
            Palette = new Dictionary<string, Color>
            {
                { "Ink", new Color32(0x25, 0x1E, 0x23, 0xFF) },          // café tinta
                { "WallCream", new Color32(0x8E, 0x6A, 0x52, 0xFF) },    // warm wood panels (nogal, lighter)
                { "Wainscot", new Color32(0x65, 0x48, 0x3D, 0xFF) },     // nogal
                { "Wood", new Color32(0x65, 0x48, 0x3D, 0xFF) },         // nogal
                { "WoodLight", new Color32(0x8C, 0x68, 0x4E, 0xFF) },
                { "CarpetRed", new Color32(0xA6, 0x5F, 0x45, 0xFF) },    // ladrillo apagado
                { "Red", new Color32(0xD2, 0x63, 0x61, 0xFF) },          // rojo apagado
                { "Green", new Color32(0x66, 0x73, 0x6A, 0xFF) },        // verde humo
                { "Bush", new Color32(0x58, 0x66, 0x5C, 0xFF) },
                { "Astro", new Color32(0x66, 0x73, 0x6A, 0xFF) },
                { "Board", new Color32(0x4E, 0x5A, 0x52, 0xFF) },
                { "Sky", new Color32(0x66, 0x73, 0x6A, 0xFF) },          // painted backdrops
                { "Glass", new Color32(0x8F, 0xA0, 0x96, 0xFF) },
                { "Screen", new Color32(0xF1, 0xDF, 0xC0, 0xFF) },       // marfil
                { "Yellow", new Color32(0xD5, 0xA3, 0x4F, 0xFF) },       // mostaza
                { "Brass", new Color32(0xE2, 0xBD, 0x73, 0xFF) },        // latón claro
                { "Paper", new Color32(0xF1, 0xDF, 0xC0, 0xFF) },        // marfil
                { "InkPurple", new Color32(0x59, 0x40, 0x5C, 0xFF) },    // berenjena
                { "Concrete", new Color32(0x7C, 0x6C, 0x62, 0xFF) },
                { "ConcreteDark", new Color32(0x30, 0x2A, 0x38, 0xFF) }, // ciruela oscura
                { "Metal", new Color32(0x72, 0x68, 0x6A, 0xFF) },
                { "Dirt", new Color32(0x65, 0x48, 0x3D, 0xFF) },
            },
            DimValue = 0.62f, DimSaturation = 1.05f, DimTint = 0.4f, ShadowTint = new Color32(0x30, 0x2A, 0x38, 0xFF), ShadowMix = 0.45f, // ciruela oscura
            Rim = true, RimColor = new Color(1f, 0.85f, 0.6f) * 0.18f,
            Exposure = 0.2f, Contrast = 6f, Saturation = 0f, Bloom = 0.6f, Vignette = 0.25f, VignetteColor = new Color32(0x25, 0x1E, 0x23, 0xFF),
            ColorFilter = new Color(1f, 0.94f, 0.84f), Grain = 0.15f,
            OutlinedSprites = true, EnemyShadowGlow = false,
            OllieShadow = new Color32(0x30, 0x2A, 0x38, 0xFF), EnemyShadow = new Color32(0xD2, 0x63, 0x61, 0xFF),
            WarmLight = new Color(1f, 0.74f, 0.42f), AiLight = new Color(0.88f, 0.95f, 0.88f), TubeLight = new Color(0.96f, 0.9f, 0.74f),
            WoodFloors = true, SheetProps = true, Windows = true,
        };

        static readonly List<Mesh> s_coneMeshes = new List<Mesh>();
        static readonly Color k_emergencyRed = new Color(1f, 0.22f, 0.16f);
        static readonly Color k_exitGreen = new Color(0.3f, 1f, 0.45f);

        /// <summary>
        /// P1 light plan. Reception and the exit are areas the AI switched back on (cold security floodlights);
        /// the rest is the abandoned classic wing (desk lamps, a bare bulb, failing tubes, the TV, emergency lights).
        /// Lit pools are where guards see Ollie; the dark lanes between them are the stealth routes.
        /// </summary>
        static void p1LightsV8()
        {
            s_coneMeshes.Clear();
            LookProfile _l = s_look;

            // Recepción (IA): a floodlight behind the receptionist covers the entrance and the carpet.
            // The sofa lanes on both sides stay dark: the safe way in, like the v1 design (behind the sofas).
            aiFlood(V(1.8f, 12.9f), V(0f, 3.8f), 42f, 18f, 140f);
            deskLamp(new Vector3(-2.2f, 1.34f, 9f), V(-0.6f, 10.4f), _l.WarmLight); // old desk lamp on the counter

            // Pasillo de oficinas (abandonado): five wall tubes, two dead and one failing.
            // Dark gaps at z 24 and 36 let Ollie wait while the patrol passes; the tube at z 30 opens a window when it fails.
            wallTube(new Vector3(-7.85f, 2.6f, 18f), "on");
            wallTube(new Vector3(-7.85f, 2.6f, 24f), "dead");
            wallTube(new Vector3(-7.85f, 2.6f, 30f), "flicker");
            wallTube(new Vector3(-7.85f, 2.6f, 36f), "dead");
            wallTube(new Vector3(-7.85f, 2.6f, 41f), "on");

            // Animación (abandonado): two desk lamps left on, in diagonal, and a work light on the memory tape.
            deskLamp(new Vector3(0f, 1.0f, 19.4f), V(0.6f, 18.2f), _l.WarmLight);
            deskLamp(new Vector3(4f, 1.0f, 23.4f), V(4.6f, 22.2f), _l.WarmLight);
            workLightV8(V(5.9f, 25.2f), V(6.8f, 26.3f), _l.WarmLight); // the reward is lit: go get it, but you'll be seen

            // Sala de guiones (abandonado): a bare bulb over the table and the TV that keeps the guard busy.
            // The TV lights the guard's face and the armchair; behind it (the melee kill) is dark.
            bareBulb(new Vector3(1.5f, 2.8f, 32.5f), _l.WarmLight);
            tvGlow(new Vector3(5.2f, 0.95f, 39.2f), V(4.3f, 37.0f), new Color(0.7f, 0.95f, 0.85f));

            // Pasillo de camerinos (abandonado): emergency lights only.
            emergencyLamp(new Vector3(-3f, 2.5f, 47.3f));
            emergencyLamp(new Vector3(9f, 2.5f, 42.7f));

            // Camerinos (abandonado): mirror bulbs, the left ones failing.
            vanityBulbs(-4.5f, true);
            vanityBulbs(3.5f, false);

            // Salida (IA): the AI lit the way out to the set. EXIT sign over the door.
            aiFlood(V(8.2f, 49f), V(11f, 57.2f), 38f, 16f, 110f);
            exitSign(new Vector3(10.5f, 2.95f, 59.55f));

            // Light probes for Senses in lit and dark spots
            lumenProbe(V(0f, 5f)); lumenProbe(V(6.2f, 6f)); lumenProbe(V(-6f, 24f)); lumenProbe(V(-6f, 30f));
            lumenProbe(V(1.5f, 32.5f)); lumenProbe(V(4.3f, 37f)); lumenProbe(V(-4.5f, 53.6f)); lumenProbe(V(11f, 56f));

            if (s_look.Windows)
            {
                // dusty windows: warm afternoon shafts. Lit pools on the floor, so they count for stealth too
                windowLight(new Vector3(7.85f, 2.1f, 17.5f), V(4.6f, 17f));   // animation room, away from the tape
                windowLight(new Vector3(-7.85f, 2.1f, 51f), V(-4.8f, 51.5f)); // left dressing room
            }
            saveConeMeshes();
        }

        // ---------- fixtures ----------

        static Transform lightRig(string _name, Vector3 _pos)
        {
            Transform _t = child(s_lights, _name);
            _t.position = _pos;
            return _t;
        }

        /// <summary>A security floodlight the AI switched back on: tripod, head with a cold glowing face, shadowed spot.</summary>
        static void aiFlood(Vector3 _base, Vector3 _target, float _angle, float _range, float _intensity)
        {
            Transform _t = lightRig("Foco de seguridad IA (reactivado)", _base);
            cylinder(_t, Vector3.zero, 0.7f, 0.05f, "Ink", false);
            cylinder(_t, Vector3.zero, 0.08f, 2.5f, "Metal", false);
            Vector3 _headPos = _base + Vector3.up * 2.6f;
            var _head = new GameObject("Cabezal").transform;
            _head.SetParent(_t, false);
            _head.position = _headPos;
            _head.rotation = Quaternion.LookRotation(_target - _headPos);
            box(_head, Vector3.zero, new Vector3(0.65f, 0.45f, 0.5f), "Ink", false);
            box(_head, new Vector3(0f, 0f, 0.26f), new Vector3(0.52f, 0.34f, 0.03f), glowName("AI", s_look.AiLight), false);
            Light _light = spot(_head, new Vector3(0f, 0f, 0.32f), _target, s_look.AiLight, _intensity, _range, _angle, true);
            lightVisuals(_light, s_look.AiLight, 0.32f, true);
        }

        /// <summary>An old desk lamp (flexo): small base, glowing head, a narrow warm spot.</summary>
        static void deskLamp(Vector3 _pos, Vector3 _target, Color _color)
        {
            Transform _t = lightRig("Flexo", _pos);
            cylinder(_t, Vector3.zero, 0.22f, 0.04f, "Ink", false);
            cylinder(_t, new Vector3(0f, 0.04f, 0f), 0.04f, 0.4f, "Metal", false);
            sphere(_t, new Vector3(0f, 0.46f, 0f), 0.16f, glowName("Warm", _color));
            Light _light = spot(_t, new Vector3(0f, 0.46f, 0f), _target, _color, 12f, 4.5f, 75f, false);
            lightVisuals(_light, _color, 0.3f, false);
        }

        /// <summary>A work light on a stand pointing at something (the memory tape).</summary>
        static void workLightV8(Vector3 _base, Vector3 _target, Color _color)
        {
            Transform _t = lightRig("Lámpara de trabajo", _base);
            cylinder(_t, Vector3.zero, 0.4f, 0.04f, "Ink", false);
            cylinder(_t, Vector3.zero, 0.05f, 1.6f, "Metal", false);
            sphere(_t, new Vector3(0f, 1.65f, 0f), 0.22f, glowName("Warm", _color));
            Light _light = spot(_t, new Vector3(0f, 1.65f, 0f), _target, _color, 16f, 5f, 60f, false);
            lightVisuals(_light, _color, 0.32f, true);
        }

        /// <summary>A fluorescent tube on the west corridor wall: "on", "dead" (no light) or "flicker" (failing).</summary>
        static void wallTube(Vector3 _pos, string _state)
        {
            bool _dead = _state == "dead";
            Transform _t = lightRig(_dead ? "Fluorescente fundido" : _state == "flicker" ? "Fluorescente que falla" : "Fluorescente", _pos);
            box(_t, Vector3.zero, new Vector3(0.14f, 0.12f, 1.4f), "Metal", false);
            var _tube = box(_t, new Vector3(0.08f, -0.02f, 0f), new Vector3(0.06f, 0.07f, 1.25f), _dead ? "ConcreteDark" : glowName("Tube", s_look.TubeLight), false);
            if (_dead) return;
            Light _light = point(_t, new Vector3(0.7f, -0.3f, 0f), s_look.TubeLight, 6.5f, 5.5f);
            Renderer _pool = lightPool(new Vector3(_pos.x + 1.85f, 0f, _pos.z), 2.1f, 1.5f, s_look.TubeLight, 0.3f);
            if (_state == "flicker") flicker(_t, _light, _pool, _tube.GetComponent<Renderer>());
        }

        /// <summary>A bare bulb hanging on its cord.</summary>
        static void bareBulb(Vector3 _pos, Color _color)
        {
            Transform _t = lightRig("Bombilla desnuda", _pos);
            cylinder(_t, Vector3.zero, 0.025f, k_wallHeight - _pos.y, "Ink", false);
            sphere(_t, new Vector3(0f, -0.08f, 0f), 0.2f, glowName("Warm", _color));
            point(_t, new Vector3(0f, -0.2f, 0f), _color, 12f, 6.5f);
            lightPool(new Vector3(_pos.x, 0f, _pos.z), 2.5f, 1f, _color, 0.3f);
        }

        /// <summary>The TV's glow: a wide, short, cold spot from the screen.</summary>
        static void tvGlow(Vector3 _screen, Vector3 _target, Color _color)
        {
            Transform _t = lightRig("Brillo de la tele", _screen);
            Light _light = spot(_t, Vector3.zero, _target, _color, 10f, 5.5f, 80f, true);
            lightVisuals(_light, _color, 0.28f, true);
        }

        static void emergencyLamp(Vector3 _pos)
        {
            Transform _t = lightRig("Luz de emergencia", _pos);
            box(_t, Vector3.zero, new Vector3(0.35f, 0.18f, 0.12f), glowName("Emergency", k_emergencyRed), false);
            point(_t, new Vector3(0f, -0.3f, 0f), k_emergencyRed, 4f, 3.5f);
            lightPool(new Vector3(_pos.x, 0f, _pos.z + (_pos.z > 45f ? -0.9f : 0.9f)), 1.4f, 1f, k_emergencyRed, 0.28f);
        }

        /// <summary>Dressing-room mirror bulbs over two vanities (centered on _x), optionally failing.</summary>
        static void vanityBulbs(float _x, bool _flicker)
        {
            Transform _t = lightRig(_flicker ? "Bombillas de espejo (fallan)" : "Bombillas de espejo", new Vector3(_x, 2.05f, 54.75f));
            var _bulbs = new List<Renderer>();
            for (int i = 0; i < 6; i++)
                _bulbs.Add(sphere(_t, new Vector3(-1.75f + i * 0.7f, 0f, 0f), 0.13f, glowName("Warm", s_look.WarmLight)).GetComponent<Renderer>());
            Light _light = point(_t, new Vector3(0f, -0.25f, -0.6f), s_look.WarmLight, 5f, 4f);
            _bulbs.Add(lightPool(new Vector3(_x, 0f, 53.5f), 1.9f, 1f, s_look.WarmLight, 0.28f));
            if (_flicker) flicker(_t, _light, _bulbs.ToArray());
        }

        static void exitSign(Vector3 _pos)
        {
            Transform _t = lightRig("Cartel de salida", _pos);
            box(_t, Vector3.zero, new Vector3(0.75f, 0.26f, 0.06f), glowName("Exit", k_exitGreen), false);
            point(_t, new Vector3(0f, -0.3f, -0.4f), k_exitGreen, 3.5f, 3.5f);
            lightPool(new Vector3(_pos.x, 0f, _pos.z - 1.2f), 1.4f, 1f, k_exitGreen, 0.26f);
        }

        // ---------- lights, cones and pools ----------

        static Light spot(Transform _parent, Vector3 _local, Vector3 _target, Color _color, float _intensity, float _range, float _angle, bool _shadows)
        {
            var _go = new GameObject("Spot Light");
            _go.transform.SetParent(_parent, false);
            _go.transform.localPosition = _local;
            _go.transform.rotation = Quaternion.LookRotation(_target - _go.transform.position);
            var _light = _go.AddComponent<Light>();
            _light.type = LightType.Spot;
            _light.color = _color;
            _light.intensity = _intensity;
            _light.range = _range;
            _light.spotAngle = _angle;
            _light.innerSpotAngle = _angle * 0.7f; // a harder edge, closer to the cartoon pool
            _light.shadows = _shadows ? LightShadows.Soft : LightShadows.None;
            _go.AddComponent<UniversalAdditionalLightData>();
            return _light;
        }

        static Light point(Transform _parent, Vector3 _local, Color _color, float _intensity, float _range)
        {
            pointLight(_parent, _local, _color, _intensity, _range);
            return _parent.GetComponentsInChildren<Light>().Last();
        }

        /// <summary>The visible part of a spot light: its floor pool (where it really lights) and, if asked, its cone.</summary>
        static void lightVisuals(Light _spot, Color _color, float _alpha, bool _cone)
        {
            Vector3 _p = _spot.transform.position, _d = _spot.transform.forward;
            if (_d.y > -0.05f) return;
            float _t = (0.03f - _p.y) / _d.y;
            if (_t > _spot.range) return;
            Vector3 _center = _p + _d * _t;
            float _radius = _t * Mathf.Tan(_spot.spotAngle * 0.5f * 0.85f * Mathf.Deg2Rad);
            float _stretch = Mathf.Min(1f / Mathf.Max(0.35f, -_d.y), 2.5f);
            var _horizontal = new Vector3(_d.x, 0f, _d.z);
            float _yaw = _horizontal.sqrMagnitude > 0.001f ? Mathf.Atan2(_d.x, _d.z) * Mathf.Rad2Deg : 0f;
            lightPool(_center, _radius, _stretch, _color, _alpha, _yaw);
            if (_cone) lightCone(_spot, _color);
        }

        /// <summary>A flat cartoon pool of light on the floor (additive, stepped edge). No collider.</summary>
        static Renderer lightPool(Vector3 _center, float _radius, float _stretch, Color _color, float _alpha, float _yaw = 0f)
        {
            var _go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            _go.name = "Zona de luz";
            Object.DestroyImmediate(_go.GetComponent<Collider>());
            _go.transform.SetParent(s_lights);
            _go.transform.position = new Vector3(_center.x, 0.03f, _center.z);
            _go.transform.rotation = Quaternion.Euler(90f, _yaw, 0f);
            _go.transform.localScale = new Vector3(_radius * 2f, _radius * 2f * _stretch, 1f);
            var _r = _go.GetComponent<MeshRenderer>();
            _r.sharedMaterial = lightMaterial("Pool", _color, _alpha, true);
            _r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            _r.receiveShadows = false;
            return _r;
        }

        /// <summary>A soft cone from the lamp to the floor (vertex alpha fades toward the floor).</summary>
        static void lightCone(Light _spot, Color _color)
        {
            Vector3 _p = _spot.transform.position, _f = _spot.transform.forward;
            float _half = _spot.spotAngle * 0.5f * 0.85f * Mathf.Deg2Rad;
            Vector3 _right = Vector3.Cross(Vector3.up, _f);
            if (_right.sqrMagnitude < 0.001f) _right = Vector3.right;
            _right.Normalize();
            Vector3 _up = Vector3.Cross(_f, _right);
            const int n = 24;
            var _verts = new Vector3[n + 1];
            var _colors = new Color[n + 1];
            var _tris = new int[n * 3];
            _verts[0] = Vector3.zero;
            _colors[0] = new Color(1f, 1f, 1f, 1f);
            for (int i = 0; i < n; i++)
            {
                float _a = i / (float)n * Mathf.PI * 2f;
                Vector3 _dir = (_f * Mathf.Cos(_half) + (_right * Mathf.Cos(_a) + _up * Mathf.Sin(_a)) * Mathf.Sin(_half)).normalized;
                float _tc = _f.y < -0.01f ? (0.03f - _p.y) / _f.y : _spot.range * 0.5f;
                float _t = _dir.y < -0.01f ? Mathf.Min((0.03f - _p.y) / _dir.y, _spot.range, _tc * 1.5f) : _tc * 1.1f;
                _verts[i + 1] = _dir * _t;
                _colors[i + 1] = new Color(1f, 1f, 1f, 0f);
                _tris[i * 3] = 0; _tris[i * 3 + 1] = i + 1; _tris[i * 3 + 2] = (i + 1) % n + 1;
            }
            var _mesh = new Mesh { name = "Cono de luz " + (s_coneMeshes.Count + 1) };
            _mesh.vertices = _verts;
            _mesh.colors = _colors;
            _mesh.triangles = _tris;
            _mesh.RecalculateBounds();
            s_coneMeshes.Add(_mesh);

            var _go = new GameObject("Cono de luz");
            _go.transform.SetParent(_spot.transform.parent, true);
            _go.transform.SetPositionAndRotation(_p, Quaternion.identity);
            _go.transform.localScale = Vector3.one;
            if (_go.transform.lossyScale != Vector3.one) _go.transform.localScale = new Vector3(1f / _go.transform.parent.lossyScale.x, 1f / _go.transform.parent.lossyScale.y, 1f / _go.transform.parent.lossyScale.z);
            _go.AddComponent<MeshFilter>().sharedMesh = _mesh;
            var _r = _go.AddComponent<MeshRenderer>();
            _r.sharedMaterial = lightMaterial("Cone", _color, 0.22f, false);
            _r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            _r.receiveShadows = false;
        }

        /// <summary>Additive unlit material for pools and cones (URP Particles/Unlit, which uses vertex colors).</summary>
        static Material lightMaterial(string _kind, Color _color, float _alpha, bool _textured)
        {
            string _key = $"Light{_kind}_{ColorUtility.ToHtmlStringRGB(_color)}_{Mathf.RoundToInt(_alpha * 100)}";
            if (s_materials.TryGetValue(_key, out Material _cached)) return _cached;
            string _folder = flatFolder + "/Light";
            if (!AssetDatabase.IsValidFolder(_folder)) AssetDatabase.CreateFolder(flatFolder, "Light");
            string _path = $"{_folder}/{flatPrefix}_{_key}.mat";
            var _mat = AssetDatabase.LoadAssetAtPath<Material>(_path);
            Shader _shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (_mat == null)
            {
                _mat = new Material(_shader);
                AssetDatabase.CreateAsset(_mat, _path);
            }
            _mat.shader = _shader;
            _mat.SetTexture("_BaseMap", _textured ? AssetDatabase.LoadAssetAtPath<Texture2D>(LastFrameArtSetup.k_lightPool) : null);
            _mat.SetColor("_BaseColor", new Color(_color.r, _color.g, _color.b, _alpha));
            _mat.SetFloat("_Surface", 1f);
            _mat.SetFloat("_Blend", 2f); // additive
            _mat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            _mat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.One);
            _mat.SetFloat("_ZWrite", 0f);
            _mat.SetFloat("_Cull", 0f);
            _mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            _mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            EditorUtility.SetDirty(_mat);
            s_materials[_key] = _mat;
            return _mat;
        }

        /// <summary>Emissive Flat Kit material for lamp bulbs, tubes and screens, registered by name for box()/sphere().</summary>
        static string glowName(string _key, Color _color)
        {
            string _name = $"Glow{_key}_{ColorUtility.ToHtmlStringRGB(_color)}";
            if (s_materials.ContainsKey(_name)) return _name;
            Material _mat = flatFrom(s_materials["Bulb"], $"{flatFolder}/{flatPrefix}_{_name}.mat", false, _color);
            _mat.EnableKeyword("_EMISSION");
            _mat.SetColor("_EmissionColor", _color * 2f);
            _mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
            EditorUtility.SetDirty(_mat);
            s_materials[_name] = _mat;
            return _name;
        }

        static void flicker(Transform _root, Light _light, params Renderer[] _visuals)
        {
            var _flicker = _root.gameObject.AddComponent<LastFrame.Visual.LightFlicker>();
            var _so = new SerializedObject(_flicker);
            _so.FindProperty("m_light").objectReferenceValue = _light;
            SerializedProperty _list = _so.FindProperty("m_visuals");
            _list.arraySize = _visuals.Length;
            for (int i = 0; i < _visuals.Length; i++) _list.GetArrayElementAtIndex(i).objectReferenceValue = _visuals[i];
            _so.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>Cone meshes are generated, so they are saved as an asset next to the scene.</summary>
        static void saveConeMeshes()
        {
            if (s_coneMeshes.Count == 0) return;
            string _folder = $"{k_folder}/{sceneName(s_index)}";
            if (!AssetDatabase.IsValidFolder(_folder)) AssetDatabase.CreateFolder(k_folder, sceneName(s_index));
            string _path = $"{_folder}/MESH_LightCones.asset";
            AssetDatabase.DeleteAsset(_path);
            AssetDatabase.CreateAsset(s_coneMeshes[0], _path);
            for (int i = 1; i < s_coneMeshes.Count; i++) AssetDatabase.AddObjectToAsset(s_coneMeshes[i], _path);
        }

        // =====================================================================
        // v10: the v9 style, abandoned for real. The old wing is no longer lit by working lamps: daylight comes in
        // through dusty windows, a projector left running throws its beam, dead tubes and failing bulbs, dust in
        // every shaft. Only the AI's floodlights are clean. Props from the concept art: projector, film reels,
        // easels with drawings, leaning painted flats, drawings on the floor.
        // =====================================================================

        static readonly LookProfile k_lookV10 = buildLookV10();

        static LookProfile buildLookV10()
        {
            LookProfile _v9 = k_lookV9;
            return new LookProfile
            {
                Id = "v10", SunDay = 0.03f, SunColor = _v9.SunColor, AmbientDay = new Color(0.2f, 0.16f, 0.18f), // dark: only the lit pools read, the rest is shadow to hide in
                Background = _v9.Background, Palette = _v9.Palette,
                DimValue = 0.52f, DimSaturation = _v9.DimSaturation, DimTint = 0.46f, // the unlit side is really dark: Flat Kit shades it as a fixed share of the base color ShadowTint = _v9.ShadowTint, ShadowMix = 0.5f,
                Rim = true, RimColor = _v9.RimColor,
                Exposure = 0.6f, Contrast = 10f, Saturation = 6f, Bloom = 0.9f, Vignette = 0.32f, VignetteColor = _v9.VignetteColor,
                ColorFilter = new Color(1f, 0.92f, 0.8f), Grain = 0.25f,
                OutlinedSprites = true, OllieShadow = _v9.OllieShadow, EnemyShadow = _v9.EnemyShadow,
                WarmLight = _v9.WarmLight, AiLight = _v9.AiLight, TubeLight = new Color(0.9f, 0.88f, 0.72f),
                WoodFloors = true, SheetProps = true,
            };
        }

        static readonly Color k_daylight = new Color(1f, 0.8f, 0.52f);
        static readonly Color k_projector = new Color(1f, 0.76f, 0.42f);

        /// <summary>
        /// P1 abandoned light plan. Same stealth layout as v8 (lit pools = danger, dark lanes = routes), with sources
        /// that make sense in a closed, forgotten studio.
        /// </summary>
        static void p1LightsV10()
        {
            s_coneMeshes.Clear();

            // Recepción (IA): the AI's clean floodlight stays: the only modern light in the wing.
            // A window high on the west wall puts a dusty pool by the entrance; the sofa lanes stay dark.
            aiFlood(V(2.6f, -1.3f), V(0.3f, 7.8f), 40f, 18f, 150f); // by the entrance, shining in (away from the camera): carpet and counter front lit, sofa lanes dark
            dustyWindow(new Vector3(-7.75f, 2.6f, 1.5f), V(-2.4f, 1.6f), 0.7f); // the shaft passes over the dark sofa lane and lands by the door

            // Pasillo de oficinas: mostly dead tubes. Two still work, one fails: the patrol walks through pools.
            wallTube(new Vector3(-7.85f, 2.6f, 18f), "flicker");
            wallTube(new Vector3(-7.85f, 2.6f, 24f), "dead");
            wallTube(new Vector3(-7.85f, 2.6f, 30f), "on");
            wallTube(new Vector3(-7.85f, 2.6f, 36f), "dead");
            wallTube(new Vector3(-7.85f, 2.6f, 41f), "on");

            // Animación: no lamps on. Two windows light the desks; the projector left running lights the memory tape.
            dustyWindow(new Vector3(7.75f, 2.6f, 15.8f), V(4.2f, 17f), 1f);
            // (no window on the corridor wall: it is an interior wall)
            projector(V(1.6f, 25.2f), new Vector3(6.8f, 0.2f, 26.3f));

            // Sala de guiones: the bare bulb fails; the TV is still on and keeps the guard busy.
            failingBulb(new Vector3(1.5f, 2.8f, 32.5f));
            tvGlow(new Vector3(5.2f, 0.95f, 39.2f), V(4.3f, 37.0f), new Color(0.75f, 0.95f, 0.85f));

            // Pasillo de camerinos: emergency lights only.
            emergencyLamp(new Vector3(-3f, 2.5f, 47.3f));
            emergencyLamp(new Vector3(9f, 2.5f, 42.7f));

            // Camerinos: a window in the left room, the right mirror's bulbs barely working.
            dustyWindow(new Vector3(-7.75f, 2.6f, 51f), V(-4.8f, 51.5f), 0.9f);
            vanityBulbs(3.5f, true);

            // Salida (IA): floodlight and EXIT sign, as in v8.
            aiFlood(V(8.2f, 49f), V(11f, 57.2f), 38f, 16f, 110f);
            exitSign(new Vector3(10.5f, 2.95f, 59.55f));

            lumenProbe(V(0f, 5f)); lumenProbe(V(6.2f, 6f)); lumenProbe(V(-6f, 24f)); lumenProbe(V(-6f, 30f));
            lumenProbe(V(4.2f, 17f)); lumenProbe(V(1.5f, 32.5f)); lumenProbe(V(-4.5f, 53.6f)); lumenProbe(V(11f, 56f));

            p1PropsV10();
            saveConeMeshes();
        }

        /// <summary>
        /// Props from the concept art, placed so every v8 route stays open (checked with the NavMesh after the build):
        /// the dark lanes behind the reception sofas, the corridor, the doors into each room and the way to the exit.
        /// </summary>
        static void p1PropsV10()
        {
            // Recepción (keeps the lanes behind the sofas, x -7.8..-6.1 and 6.1..7.8, and the central carpet free)
            filmReels(V(3.1f, 13.2f), 3);
            leaningFlat(V(-0.6f, 13.5f), 180f, "Sky");
            floorPapers(V(1f, 11.4f), 4);
            trashBin(V(3.7f, 10.3f));
            coatRack(V(-5.4f, -1.3f));
            // Animación (door from the corridor at x -4, z 21..27.5 stays open)
            easel(V(-2.9f, 15.4f), 200f);
            easel(V(6.4f, 15.5f), 150f);
            stool(V(-1.9f, 15.7f));
            filmReels(V(2.6f, 14.7f), 4);
            paperStack(V(-3.4f, 17.2f));
            coveredFurniture(V(-1.2f, 26.8f), new Vector3(1.4f, 1.1f, 0.9f), -5f);
            projector(V(1.6f, 25.2f), new Vector3(6.8f, 0.2f, 26.3f));
            floorPapers(V(2.5f, 20.8f), 5);
            // Sala de guiones (door from the corridor at x -4, z 27.5..34 stays open)
            leaningFlat(V(7.5f, 30.5f), -90f, "Sky");
            archiveCabinet(V(7.45f, 34.2f), -90f);
            easel(V(-3.1f, 41.4f), 140f);
            trashBin(V(6.9f, 41.9f));
            floorPapers(V(1.5f, 35.5f), 4);
            // Camerinos y salida
            coveredFurniture(V(-7.1f, 48.4f), new Vector3(1.1f, 0.8f, 0.8f), 0f);
            coveredFurniture(V(5.9f, 53.2f), new Vector3(1f, 1.2f, 0.9f), 15f);
            filmReels(V(12.8f, 58.2f), 3);
            leaningFlat(V(13.5f, 45.2f), -90f, "Green");
            floorPapers(V(-3.5f, 50.5f), 3);
        }

        static void trashBin(Vector3 _pos)
        {
            Transform _t = prop("Papelera con bocetos", _pos, 0f);
            cylinder(_t, Vector3.zero, 0.42f, 0.55f, "Metal");
            sphere(_t, new Vector3(0.05f, 0.58f, 0f), 0.18f, "Paper");
            sphere(_t, new Vector3(-0.08f, 0.6f, 0.06f), 0.15f, "Paper");
            sphere(_t, new Vector3(0.35f, 0.07f, 0.2f), 0.14f, "Paper"); // one on the floor
        }

        static void coatRack(Vector3 _pos)
        {
            Transform _t = prop("Perchero", _pos, 0f);
            cylinder(_t, Vector3.zero, 0.45f, 0.05f, "Wood", false);
            cylinder(_t, Vector3.zero, 0.07f, 1.8f, "Wood");
            sphere(_t, new Vector3(0.18f, 1.65f, 0f), 0.14f, "Wood");
            box(_t, new Vector3(-0.15f, 1.35f, 0f), new Vector3(0.3f, 0.6f, 0.12f), "Red", false, new Vector3(0f, 0f, 8f)); // a forgotten coat
        }

        static void stool(Vector3 _pos)
        {
            Transform _t = prop("Taburete", _pos, 0f);
            cylinder(_t, new Vector3(0f, 0.55f, 0f), 0.4f, 0.06f, "Wood");
            cylinder(_t, Vector3.zero, 0.06f, 0.55f, "Metal", false);
        }

        static void paperStack(Vector3 _pos)
        {
            Transform _t = prop("Pila de dibujos", _pos, 12f);
            box(_t, Vector3.zero, new Vector3(0.6f, 0.5f, 0.45f), "Wood");
            for (int i = 0; i < 4; i++)
                box(_t, new Vector3(0.02f * i, 0.5f + i * 0.04f, -0.01f * i), new Vector3(0.48f, 0.035f, 0.36f), "Paper", false, new Vector3(0f, i * 9f, 0f));
        }

        static void archiveCabinet(Vector3 _pos, float _yaw)
        {
            Transform _t = prop("Archivador", _pos, _yaw);
            box(_t, Vector3.zero, new Vector3(0.9f, 1.4f, 0.6f), "Metal");
            for (int i = 0; i < 3; i++) box(_t, new Vector3(0f, 0.25f + i * 0.45f, 0.31f), new Vector3(0.3f, 0.05f, 0.03f), "Brass", false);
            box(_t, new Vector3(0f, 0.7f, 0.42f), new Vector3(0.8f, 0.4f, 0.25f), "Metal", false, new Vector3(0f, 0f, 0f)); // a drawer left open
        }

        /// <summary>A piece of furniture under a dust sheet: a box with a slightly larger, lower "cloth" on top.</summary>
        static void coveredFurniture(Vector3 _pos, Vector3 _size, float _yaw)
        {
            Transform _t = prop("Mueble tapado", _pos, _yaw);
            box(_t, Vector3.zero, _size, "Paper");
            box(_t, new Vector3(0f, _size.y * 0.25f, 0f), new Vector3(_size.x + 0.12f, _size.y * 0.5f, _size.z + 0.12f), "Paper", false);
            box(_t, new Vector3(0.1f, _size.y + 0.04f, -0.05f), new Vector3(_size.x * 0.6f, 0.08f, _size.z * 0.5f), "Paper", false, new Vector3(0f, 18f, 0f));
        }

        // ---------- v10 light sources ----------

        /// <summary>
        /// A dusty window lit by the low afternoon sun, built like real light through a window: the sun is a spot far
        /// outside with a window-pane cookie, so the real light on the floor is the window's shape (4 panes and
        /// mullions), stretched by the sun's angle. The visible parts follow that same projection: a shaft that starts
        /// at the window and fades toward the floor, the pane-shaped pool, the glowing glass, and dust in the beam.
        /// The light is real (Senses measures it): the pool is a danger zone.
        /// </summary>
        static void dustyWindow(Vector3 _wallPos, Vector3 _target, float _strength)
        {
            Transform _t = lightRig("Ventana polvorienta", _wallPos);
            Vector3 _flat = new Vector3(_target.x - _wallPos.x, 0f, _target.z - _wallPos.z);
            Vector3 _n = Mathf.Abs(_flat.x) > Mathf.Abs(_flat.z) ? new Vector3(Mathf.Sign(_flat.x), 0f, 0f) : new Vector3(0f, 0f, Mathf.Sign(_flat.z));
            Vector3 _d = (_target - _wallPos).normalized;            // sun direction into the room
            const float L = 6f, hw = 0.85f, hh = 0.55f, k_cookieX = 0.84f, k_cookieY = 0.546f;
            Vector3 _src = _wallPos - _d * L;
            Vector3 _right = Vector3.Cross(Vector3.up, _d).normalized;
            Vector3 _up = Vector3.Cross(_d, _right);

            // Real light: the sun, with the window as a cookie (inscribed in the spot's circle so no pane is cut)
            float _angle = 2f * Mathf.Atan(hw / (k_cookieX * L)) * Mathf.Rad2Deg;
            var _go = new GameObject("Sol de tarde (spot con forma de ventana)");
            _go.transform.SetParent(_t, false);
            _go.transform.SetPositionAndRotation(_src, Quaternion.LookRotation(_d, _up));
            var _light = _go.AddComponent<Light>();
            _light.type = LightType.Spot;
            _light.color = k_daylight;
            _light.intensity = 110f * _strength;
            _light.range = L + 9f;
            _light.spotAngle = _angle;
            _light.innerSpotAngle = _angle * 0.97f;
            _light.shadows = LightShadows.None; // it starts outside: the wall would block it
            _light.cookie = AssetDatabase.LoadAssetAtPath<Texture2D>(LastFrameArtSetup.k_windowCookie);
            _go.AddComponent<UniversalAdditionalLightData>();

            // Corner rays of the window frustum, in normalized cookie coordinates (u, v in -1..1)
            System.Func<float, float, Vector3> ray = (u, v) => (_d + _right * (u * hw / L) + _up * (v * hh / L)).normalized;
            System.Func<Vector3, Vector3> onWall = r => _src + r * (Vector3.Dot(_wallPos - _src, _n) / Vector3.Dot(r, _n));
            System.Func<Vector3, Vector3> onFloor = r => _src + r * ((0.035f - _src.y) / r.y);

            // The window in the wall, where the beam enters
            Vector3[] _wc = { onWall(ray(-1, -1)), onWall(ray(1, -1)), onWall(ray(1, 1)), onWall(ray(-1, 1)) };
            Vector3 _center = (_wc[0] + _wc[1] + _wc[2] + _wc[3]) / 4f;
            Vector3 _tangent = Vector3.Cross(Vector3.up, _n);
            float _w = Mathf.Abs(Vector3.Dot(_wc[1] - _wc[0], _tangent)) + 0.2f, _h = Mathf.Abs(_wc[2].y - _wc[1].y) + 0.2f;
            var _frame = new GameObject("Marco").transform;
            _frame.SetParent(_t, false);
            _frame.SetPositionAndRotation(_center, Quaternion.LookRotation(_n));
            box(_frame, Vector3.zero, new Vector3(_w + 0.12f, _h + 0.12f, 0.08f), "Wood", false);
            box(_frame, new Vector3(0f, 0f, 0.03f), new Vector3(_w - 0.06f, _h - 0.06f, 0.05f), glowName("Window", new Color(1f, 0.86f, 0.6f)), false);
            box(_frame, new Vector3(0f, 0f, 0.07f), new Vector3(0.07f, _h - 0.06f, 0.04f), "Wood", false);
            box(_frame, new Vector3(0f, 0f, 0.07f), new Vector3(_w - 0.06f, 0.07f, 0.04f), "Wood", false);
            box(_frame, new Vector3(0f, -_h * 0.5f - 0.08f, 0.1f), new Vector3(_w + 0.25f, 0.06f, 0.18f), "Wood", false); // sill

            // Shaft: from the window outline to the floor outline, bright at the window and fading out
            Vector3[] _fc = { onFloor(ray(-1, -1)), onFloor(ray(1, -1)), onFloor(ray(1, 1)), onFloor(ray(-1, 1)) };
            var _verts = new List<Vector3>();
            var _cols = new List<Color>();
            var _tris = new List<int>();
            for (int i = 0; i < 4; i++)
            {
                int j = (i + 1) % 4, b = _verts.Count;
                _verts.Add(_wc[i] - _center); _verts.Add(_wc[j] - _center); _verts.Add(_fc[j] - _center); _verts.Add(_fc[i] - _center);
                _cols.Add(new Color(1f, 1f, 1f, 1f)); _cols.Add(new Color(1f, 1f, 1f, 1f));
                _cols.Add(new Color(1f, 1f, 1f, 0.08f)); _cols.Add(new Color(1f, 1f, 1f, 0.08f));
                _tris.AddRange(new[] { b, b + 1, b + 2, b, b + 2, b + 3 });
            }
            var _mesh = new Mesh { name = "Haz de ventana " + (s_coneMeshes.Count + 1) };
            _mesh.SetVertices(_verts); _mesh.SetColors(_cols); _mesh.SetTriangles(_tris, 0);
            _mesh.RecalculateBounds();
            s_coneMeshes.Add(_mesh);
            var _shaft = new GameObject("Haz de luz");
            _shaft.transform.SetParent(_t, false);
            _shaft.transform.position = _center;
            _shaft.AddComponent<MeshFilter>().sharedMesh = _mesh;
            var _sr = _shaft.AddComponent<MeshRenderer>();
            _sr.sharedMaterial = lightMaterial("Shaft", k_daylight, 0.16f * Mathf.Clamp01(_strength + 0.2f), false);
            _sr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            _sr.receiveShadows = false;

            // Floor pool: the 4 panes, with the mullion gaps, projected like the real light
            const float g = 0.05f; // half mullion width (normalized)
            float[][] _panes = { new[] { -1f, -g, -1f, -g }, new[] { g, 1f, -1f, -g }, new[] { -1f, -g, g, 1f }, new[] { g, 1f, g, 1f } };
            var _pv = new List<Vector3>();
            var _pc = new List<Color>();
            var _pt = new List<int>();
            Vector3 _poolCenter = (_fc[0] + _fc[1] + _fc[2] + _fc[3]) / 4f;
            foreach (float[] p in _panes)
            {
                int b = _pv.Count;
                _pv.Add(onFloor(ray(p[0], p[2])) - _poolCenter); _pv.Add(onFloor(ray(p[1], p[2])) - _poolCenter);
                _pv.Add(onFloor(ray(p[1], p[3])) - _poolCenter); _pv.Add(onFloor(ray(p[0], p[3])) - _poolCenter);
                for (int i = 0; i < 4; i++) _pc.Add(Color.white);
                _pt.AddRange(new[] { b, b + 2, b + 1, b, b + 3, b + 2 });
            }
            var _poolMesh = new Mesh { name = "Luz de ventana en el suelo " + (s_coneMeshes.Count + 1) };
            _poolMesh.SetVertices(_pv); _poolMesh.SetColors(_pc); _poolMesh.SetTriangles(_pt, 0);
            _poolMesh.RecalculateBounds();
            s_coneMeshes.Add(_poolMesh);
            var _pool = new GameObject("Zona de luz (ventana)");
            _pool.transform.SetParent(_t, false);
            _pool.transform.position = _poolCenter;
            _pool.AddComponent<MeshFilter>().sharedMesh = _poolMesh;
            var _pr = _pool.AddComponent<MeshRenderer>();
            _pr.sharedMaterial = lightMaterial("WindowPool", k_daylight, 0.22f, false);
            _pr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            _pr.receiveShadows = false;

            dustMotes(_t, _center, _poolCenter, Mathf.Min(_w, 1.6f));
        }

        /// <summary>A film projector on a tripod, left running: two reels, glowing lens and an amber beam.</summary>
        static void projector(Vector3 _base, Vector3 _target)
        {
            Transform _t = prop("Proyector de cine (encendido)", _base, 0f);
            for (int i = 0; i < 3; i++)
            {
                float _a = i * 120f * Mathf.Deg2Rad;
                cylinder(_t, new Vector3(Mathf.Sin(_a) * 0.25f, 0.55f, Mathf.Cos(_a) * 0.25f), 0.04f, 1.15f, "Metal", false,
                    new Vector3(Mathf.Cos(_a) * 12f, 0f, -Mathf.Sin(_a) * 12f));
            }
            Vector3 _head = new Vector3(0f, 1.25f, 0f);
            var _body = new GameObject("Cuerpo").transform;
            _body.SetParent(_t, false);
            _body.localPosition = _head;
            Vector3 _dir = _target - (_base + _head);
            _body.rotation = Quaternion.LookRotation(new Vector3(_dir.x, 0f, _dir.z));
            box(_body, Vector3.zero, new Vector3(0.36f, 0.32f, 0.55f), "Ink", false);
            cylinder(_body, new Vector3(0f, 0.38f, -0.18f), 0.42f, 0.06f, "Brass", false, new Vector3(0f, 0f, 90f));
            cylinder(_body, new Vector3(0f, 0.38f, 0.2f), 0.42f, 0.06f, "Brass", false, new Vector3(0f, 0f, 90f));
            cylinder(_body, new Vector3(0f, 0f, 0.34f), 0.14f, 0.14f, glowName("Projector", k_projector), false, new Vector3(90f, 0f, 0f));
            Light _light = spot(_body, new Vector3(0f, 0f, 0.42f), _target, k_projector, 30f, 9f, 22f, true);
            lightVisuals(_light, k_projector, 0.34f, true);
            dustMotes(_body, _light.transform.position, _target, 0.8f);
        }

        static void failingBulb(Vector3 _pos)
        {
            Transform _t = lightRig("Bombilla que falla", _pos);
            cylinder(_t, Vector3.zero, 0.025f, k_wallHeight - _pos.y, "Ink", false);
            var _bulb = sphere(_t, new Vector3(0f, -0.08f, 0f), 0.2f, glowName("Warm", s_look.WarmLight));
            Light _light = point(_t, new Vector3(0f, -0.2f, 0f), s_look.WarmLight, 10f, 6f);
            Renderer _pool = lightPool(new Vector3(_pos.x, 0f, _pos.z), 2.4f, 1f, s_look.WarmLight, 0.3f);
            flicker(_t, _light, _pool, _bulb.GetComponent<Renderer>());
        }

        /// <summary>Dust floating in a light shaft: slow, tiny ivory motes along the line from the source to the floor.</summary>
        static void dustMotes(Transform _parent, Vector3 _from, Vector3 _to, float _width)
        {
            var _go = new GameObject("Polvo en el haz");
            _go.transform.SetParent(_parent, true);
            _go.transform.position = Vector3.Lerp(_from, _to, 0.5f);
            _go.transform.rotation = Quaternion.LookRotation(_to - _from);
            var _ps = _go.AddComponent<ParticleSystem>();
            _ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var _main = _ps.main;
            _main.loop = true;
            _main.prewarm = true;
            _main.startLifetime = new ParticleSystem.MinMaxCurve(5f, 8f);
            _main.startSpeed = new ParticleSystem.MinMaxCurve(0.02f, 0.08f);
            _main.startSize = new ParticleSystem.MinMaxCurve(0.03f, 0.07f);
            _main.startColor = new Color(1f, 0.92f, 0.75f, 0.8f);
            _main.maxParticles = 80;
            _main.simulationSpace = ParticleSystemSimulationSpace.World;
            var _emission = _ps.emission;
            _emission.rateOverTime = 10f;
            var _shape = _ps.shape;
            _shape.shapeType = ParticleSystemShapeType.Box;
            _shape.scale = new Vector3(_width, _width * 0.6f, Vector3.Distance(_from, _to) * 0.9f);
            var _noise = _ps.noise;
            _noise.enabled = true;
            _noise.strength = 0.08f;
            _noise.frequency = 0.3f;
            var _fade = _ps.colorOverLifetime;
            _fade.enabled = true;
            var _g = new Gradient();
            _g.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.25f), new GradientAlphaKey(1f, 0.75f), new GradientAlphaKey(0f, 1f) });
            _fade.color = _g;
            var _r = _go.GetComponent<ParticleSystemRenderer>();
            _r.sharedMaterial = lightMaterial("Dust", new Color(1f, 0.92f, 0.75f), 0.9f, true);
            _r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            _r.receiveShadows = false;
        }

        // ---------- v10 props ----------

        /// <summary>A stack of film reels in their round cans (decor, small collider).</summary>
        static void filmReels(Vector3 _pos, int _count)
        {
            Transform _t = prop("Rollos de película", _pos, (_pos.x * 37f + _pos.z * 11f) % 360f);
            for (int i = 0; i < _count; i++)
            {
                Vector3 _o = new Vector3((i % 2) * 0.12f - 0.06f, i * 0.09f, (i % 3) * 0.08f - 0.08f);
                cylinder(_t, _o, 0.62f, 0.08f, i % 2 == 0 ? "Metal" : "Brass", i == 0);
                cylinder(_t, _o + new Vector3(0f, 0.085f, 0f), 0.16f, 0.01f, "Ink", false);
            }
        }

        /// <summary>A painter's easel with a drawing on it (the concept art's animation desks and sketches).</summary>
        static void easel(Vector3 _pos, float _yaw)
        {
            Transform _t = prop("Caballete con dibujo", _pos, _yaw);
            box(_t, new Vector3(-0.35f, 0.85f, 0f), new Vector3(0.06f, 1.75f, 0.06f), "Wood", false, new Vector3(-8f, 0f, 8f));
            box(_t, new Vector3(0.35f, 0.85f, 0f), new Vector3(0.06f, 1.75f, 0.06f), "Wood", false, new Vector3(-8f, 0f, -8f));
            box(_t, new Vector3(0f, 0.8f, -0.35f), new Vector3(0.06f, 1.6f, 0.06f), "Wood", true, new Vector3(20f, 0f, 0f));
            box(_t, new Vector3(0f, 0.75f, 0.06f), new Vector3(0.9f, 0.06f, 0.12f), "Wood", false);
            box(_t, new Vector3(0f, 1.25f, 0.1f), new Vector3(0.8f, 0.95f, 0.03f), "Paper", false, new Vector3(-8f, 0f, 0f));
            // the sketch: a few ink strokes
            box(_t, new Vector3(-0.1f, 1.35f, 0.125f), new Vector3(0.35f, 0.03f, 0.005f), "InkPurple", false, new Vector3(-8f, 0f, 25f));
            box(_t, new Vector3(0.05f, 1.15f, 0.125f), new Vector3(0.25f, 0.03f, 0.005f), "InkPurple", false, new Vector3(-8f, 0f, -15f));
            sphere(_t, new Vector3(0f, 1.45f, 0.125f), 0.16f, "InkPurple");
        }

        /// <summary>Drawings scattered on the floor (flat ivory sheets, no collider).</summary>
        static void floorPapers(Vector3 _center, int _count)
        {
            Transform _t = prop("Dibujos en el suelo", _center, 0f);
            var _rnd = new System.Random(Mathf.RoundToInt(_center.x * 31f + _center.z * 7f));
            for (int i = 0; i < _count; i++)
            {
                Vector3 _o = new Vector3((float)_rnd.NextDouble() * 2.4f - 1.2f, 0.015f + i * 0.002f, (float)_rnd.NextDouble() * 1.6f - 0.8f);
                box(_t, _o, new Vector3(0.42f, 0.004f, 0.3f), "Paper", false, new Vector3(0f, (float)_rnd.NextDouble() * 360f, 0f));
            }
        }

        // =====================================================================
        // v10 on P2-P5: the same finish as P1, applied to each level's own layout. The level keeps its v3 light plan
        // (those lights already come from visible fixtures and were placed for stealth); this pass makes them read
        // like P1: every light gets its floor pool (and spots their cone and dust), a few old lights fail, interior
        // levels get dusty windows on exterior walls where there is room, and concept-art props are placed only in
        // spots that leave at least 2.2 m of free passage (checked with physics, then the NavMesh after the bake).
        // =====================================================================

        static Scene s_buildScene;

        static void dressLevelV10()
        {
            s_coneMeshes.Clear();
            Physics.SyncTransforms();
            bool _outdoor = s_index == 5; // backlot at night: no windows, the lamp posts and floodlights do the work

            // 1) every existing light reads like P1
            int _point = 0;
            int _pointTotal = s_lights.GetComponentsInChildren<Light>(true).Concat(s_props.GetComponentsInChildren<Light>(true)).Distinct().Count(l => l.type == LightType.Point && l.intensity > 0f);
            foreach (Light _l in s_lights.GetComponentsInChildren<Light>(true).Concat(s_props.GetComponentsInChildren<Light>(true)).Distinct().ToList())
            {
                if (_l.type == LightType.Directional || _l.intensity <= 0f) continue;
                if (_l.type == LightType.Spot)
                {
                    lightVisuals(_l, _l.color, 0.3f, false); // pool only: drawn cones of angled stage spots read as odd shapes
                    Vector3 _p = _l.transform.position, _d = _l.transform.forward;
                    if (_d.y < -0.05f) dustMotes(_l.transform, _p, _p + _d * ((0.03f - _p.y) / _d.y), 1.2f);
                }
                else if (_l.type == LightType.Point)
                {
                    Vector3 _p = _l.transform.position;
                    int _k = _point++;
                    var _glow = _l.transform.parent != null ? _l.transform.parent.GetComponentsInChildren<Renderer>().Where(r => r.sharedMaterials.Any(m => m != null && m.IsKeywordEnabled("_EMISSION"))).ToArray() : new Renderer[0];
                    if (_pointTotal >= 4 && _k % 4 == 0)
                    {
                        // abandoned: this one is dead (dark bulb, no light): more shadow to hide in
                        foreach (Renderer _g in _glow) _g.sharedMaterials = Enumerable.Repeat(s_materials["ConcreteDark"], _g.sharedMaterials.Length).ToArray();
                        Object.DestroyImmediate(_l.gameObject);
                        continue;
                    }
                    float _r = Mathf.Min(_l.range * 0.4f, 3.5f);
                    Renderer _pool = lightPool(new Vector3(_p.x, 0f, _p.z), _r, 1f, _l.color, 0.28f);
                    // abandoned: one old light in four fails (a timing window, like the P1 corridor tube)
                    if (_k % 4 == 2)
                    {
                        flicker(_l.transform, _l, _glow.Concat(new[] { _pool }).ToArray());
                    }
                }
            }

            // 2) windows on exterior walls (interior levels only)
            // only the prop warehouse (P3) has windows; a sound stage (P2) and recording rooms (P4) have none, P5 is outdoors
            if (s_index == 3) autoWindows(2, 26f);

            // 3) concept-art props in safe spots
            autoProps(10);

            saveConeMeshes();
        }

        static Bounds levelBounds()
        {
            Renderer[] _r = s_walls.GetComponentsInChildren<Renderer>().Where(r => r.bounds.size.magnitude < 80f).ToArray();
            Bounds _b = _r[0].bounds;
            foreach (Renderer r in _r) _b.Encapsulate(r.bounds);
            return _b;
        }

        static bool inLevel(Collider _c) => _c != null && _c.gameObject.scene == s_buildScene && _c.gameObject.name != "Ground";

        static bool rayHitsLevel(Vector3 _from, Vector3 _dir, float _dist, out RaycastHit _hit)
        {
            _hit = default;
            float _best = float.MaxValue;
            bool _found = false;
            foreach (RaycastHit h in Physics.RaycastAll(_from, _dir, _dist, ~0, QueryTriggerInteraction.Ignore))
            {
                if (!inLevel(h.collider) || h.distance >= _best) continue;
                _best = h.distance; _hit = h; _found = true;
            }
            return _found;
        }

        static bool spotIsFree(Vector3 _p, float _radius, bool _includeTriggers)
        {
            var _q = _includeTriggers ? QueryTriggerInteraction.Collide : QueryTriggerInteraction.Ignore;
            return !Physics.OverlapSphere(_p, _radius, ~0, _q).Any(inLevel);
        }

        static List<Vector3> keyPoints()
        {
            var _pts = new List<Vector3> { V(0f, 0.5f) };
            foreach (Transform t in s_plan.GetComponentsInChildren<Transform>(true))
                if (t.name.StartsWith("PLAN · Cinta") || t.name.StartsWith("SetDoor")) _pts.Add(new Vector3(t.position.x, 0f, t.position.z));
            foreach (Transform e in s_enemies) _pts.Add(new Vector3(e.position.x, 0f, e.position.z));
            return _pts;
        }

        /// <summary>
        /// Windows where an exterior wall has at least 2.4 m of free floor in front of it, spaced 10 m apart,
        /// alternating sides. The shaft lands about 3 m into the room.
        /// </summary>
        static void autoWindows(int _max, float _maxZ = float.MaxValue)
        {
            Bounds _b = levelBounds();
            _b.max = new Vector3(_b.max.x, _b.max.y, Mathf.Min(_b.max.z, _maxZ + 6f));
            var _placed = new List<Vector3>();
            for (float z = _b.min.z + 6f; z < _b.max.z - 6f && _placed.Count < _max; z += 3f)
            {
                foreach (int side in new[] { _placed.Count % 2 == 0 ? 1 : -1, _placed.Count % 2 == 0 ? -1 : 1 })
                {
                    if (_placed.Count >= _max) break;
                    float _xw = side > 0 ? _b.max.x - 0.16f : _b.min.x + 0.16f;
                    Vector3 _in = new Vector3(-side, 0f, 0f);
                    Vector3 _probe = new Vector3(_xw + _in.x * 3f, 1f, z);
                    if (_placed.Any(p => Mathf.Abs(p.z - z) < 10f)) continue;
                    if (!spotIsFree(_probe, 0.4f, false)) continue;
                    // the nearest thing toward the wall must be the wall itself, about 3 m away, at window height too
                    if (!rayHitsLevel(_probe, -_in, 3.6f, out RaycastHit _low) || _low.distance < 2.4f || !_low.collider.transform.IsChildOf(s_walls)) continue;
                    if (!rayHitsLevel(_probe + Vector3.up * 2.2f, -_in, 3.6f, out RaycastHit _high) || !_high.collider.transform.IsChildOf(s_walls)) continue;
                    float _wallX = _low.point.x + _in.x * 0.12f;
                    dustyWindow(new Vector3(_wallX, 2.6f, z), new Vector3(_wallX + _in.x * 3.2f, 0f, z + 0.8f), 0.9f);
                    _placed.Add(new Vector3(_wallX, 0f, z));
                }
            }
        }

        /// <summary>
        /// Props against walls or furniture, never in a passage: the spot is free, something solid is within 0.9 m
        /// on one side, the opposite side has 2.2 m clear, and it is away from the start, the tape, the exit and enemies.
        /// </summary>
        static void autoProps(int _max)
        {
            Bounds _b = levelBounds();
            List<Vector3> _keep = keyPoints();
            var _placed = new List<Vector3>();
            System.Action<Vector3, int>[] _kinds =
            {
                (p, i) => filmReels(p, 2 + i % 3),
                (p, i) => trashBin(p),
                (p, i) => paperStack(p),
                (p, i) => coveredFurniture(p, new Vector3(1f, 0.9f, 0.8f), (i * 37f) % 40f - 20f),
                (p, i) => stool(p),
                (p, i) => filmReels(p, 3),
            };
            Vector3[] _dirs = { Vector3.left, Vector3.right, Vector3.forward, Vector3.back };
            for (float z = _b.min.z + 1.2f; z < _b.max.z - 1.2f && _placed.Count < _max; z += 1.5f)
            {
                for (float x = _b.min.x + 1.2f; x < _b.max.x - 1.2f && _placed.Count < _max; x += 1.5f)
                {
                    Vector3 _p = new Vector3(x, 0f, z);
                    if (_keep.Any(k => Vector3.Distance(k, _p) < 3f) || _placed.Any(q => Vector3.Distance(q, _p) < 4.5f)) continue;
                    if (!spotIsFree(_p + Vector3.up * 0.6f, 0.55f, true)) continue;
                    bool _ok = false;
                    foreach (Vector3 d in _dirs)
                    {
                        if (!rayHitsLevel(_p + Vector3.up * 0.5f, d, 0.9f, out _)) continue;
                        if (rayHitsLevel(_p + Vector3.up * 0.5f, -d, 2.6f, out _)) continue; // keep a 2.2 m passage
                        Vector3 _side = Vector3.Cross(Vector3.up, d);
                        if (rayHitsLevel(_p + Vector3.up * 0.5f, _side, 1.2f, out _) && rayHitsLevel(_p + Vector3.up * 0.5f, -_side, 1.2f, out _)) continue; // not in a narrow gap
                        _ok = true;
                        break;
                    }
                    if (!_ok) continue;
                    int i = _placed.Count;
                    _kinds[i % _kinds.Length](_p, i);
                    if (i % 3 == 1) floorPapers(_p + new Vector3(0.9f, 0f, 0.4f), 3);
                    _placed.Add(_p);
                    Physics.SyncTransforms();
                }
            }
        }

        // =====================================================================
        // v10 room moods: not every room is dark. Each room gets a mood that serves the level's balance:
        //  - Lit: working ceiling lamps, warm color, a local volume with more exposure and saturation. Ollie is
        //    exposed here, so she plays with line of sight, cover and stealth kills instead of darkness.
        //  - Mid: one lamp still works.
        //  - Dark: no fill, a local volume a bit darker and desaturated: the places to hide and wait.
        // Room bounds come from the room's plan label, raycasting to the walls. The local volumes are evaluated at
        // Ollie's position (camera volume trigger), so the look changes as she walks from room to room.
        // =====================================================================

        enum Mood { Lit, Mid, Dark }

        static readonly Dictionary<string, Mood> k_roomMoods = new Dictionary<string, Mood>
        {
            // P1 Pasillos del estudio
            { "RECEPCIÓN", Mood.Mid }, { "PASILLO DE OFICINAS", Mood.Dark }, { "ANIMACIÓN", Mood.Lit },
            { "SALA DE GUIONES", Mood.Mid }, { "PASILLO DE CAMERINOS", Mood.Dark }, { "CAMERINOS", Mood.Lit },
            // P2 Plató B
            { "MUELLE DE CARGA", Mood.Mid }, { "PLATÓ B · SET DEL BOSQUE ENCANTADO", Mood.Lit }, { "BASTIDORES", Mood.Dark },
            // P3 Almacén y jardín
            { "ALMACÉN DE ATREZO", Mood.Lit }, { "PLATÓ C · SET DEL JARDÍN", Mood.Mid },
            // P4 Estudio de sonido
            { "SALA DE FOLEY", Mood.Mid }, { "CONTROL", Mood.Dark }, { "DISCOTECA", Mood.Lit }, { "ESTUDIO A · GRABANDO", Mood.Lit },
            // P5 Backlot y archivo
            { "PATIO TRASERO", Mood.Dark }, { "SET DEL RÍO", Mood.Mid }, { "CALLE DEL BACKLOT", Mood.Mid }, { "ARCHIVO DE CINTAS", Mood.Lit },
        };

        static readonly Dictionary<string, Color> k_roomLightColor = new Dictionary<string, Color>
        {
            { "DISCOTECA", new Color(1f, 0.55f, 0.8f) },
            { "ESTUDIO A · GRABANDO", new Color(1f, 0.62f, 0.5f) },
            { "PLATÓ C · SET DEL JARDÍN", new Color(0.85f, 0.95f, 0.75f) },
        };

        static void roomMoodsV10()
        {
            s_freshProfiles.Clear();
            Physics.SyncTransforms();
            var _root = child(s_lights, "Ambiente por sala");
            var _rooms = s_labels.GetComponentsInChildren<TMP_Text>(true).Where(l => k_roomMoods.ContainsKey(l.text.Trim()))
                .Select(l => (Name: l.text.Trim(), Center: new Vector3(l.transform.position.x, 0f, l.transform.position.z))).ToList();
            foreach (var (_name, _center) in _rooms)
            {
                Mood _mood = k_roomMoods[_name];
                Bounds _room = roomBounds(_center);
                // open levels: a room never reaches past halfway to a neighbour room that falls inside it
                foreach (var (_other, _oc) in _rooms)
                {
                    if (_other == _name || !_room.Contains(new Vector3(_oc.x, 1f, _oc.z))) continue;
                    Vector3 _min = _room.min, _max = _room.max;
                    Vector3 _d = _oc - _center;
                    if (Mathf.Abs(_d.z) >= Mathf.Abs(_d.x)) { if (_d.z > 0) _max.z = _center.z + _d.z * 0.5f; else _min.z = _center.z + _d.z * 0.5f; }
                    else { if (_d.x > 0) _max.x = _center.x + _d.x * 0.5f; else _min.x = _center.x + _d.x * 0.5f; }
                    _room.SetMinMax(_min, _max);
                }
                Color _color = k_roomLightColor.TryGetValue(_name, out Color _c) ? _c : new Color(1f, 0.8f, 0.58f);
                var _t = child(_root, $"{_name} ({(_mood == Mood.Lit ? "iluminada" : _mood == Mood.Mid ? "media luz" : "oscura")})");
                _t.position = _room.center;

                if (_mood != Mood.Dark)
                {
                    float _spacing = _mood == Mood.Lit ? 5.5f : 9f;
                    int _nx = Mathf.Max(1, Mathf.RoundToInt((_room.size.x - 2f) / _spacing));
                    int _nz = Mathf.Max(1, Mathf.RoundToInt((_room.size.z - 2f) / _spacing));
                    int _max = _mood == Mood.Lit ? 6 : 1;
                    int _n = 0;
                    for (int ix = 0; ix < _nx && _n < _max; ix++)
                        for (int iz = 0; iz < _nz && _n < _max; iz++)
                        {
                            float x = _nx == 1 ? _room.center.x : Mathf.Lerp(_room.min.x + 1.5f, _room.max.x - 1.5f, ix / (float)(_nx - 1));
                            float z = _nz == 1 ? _room.center.z : Mathf.Lerp(_room.min.z + 1.5f, _room.max.z - 1.5f, iz / (float)(_nz - 1));
                            pendantLamp(_t, new Vector3(x, 0f, z), _color, _mood == Mood.Lit ? 13f : 5f, _spacing * 1.4f);
                            _n++;
                        }
                }
                roomVolume(_t, _room, _mood);
            }
        }

        static Bounds roomBounds(Vector3 _center)
        {
            Vector3 _o = _center + Vector3.up * 1.5f;
            float d(Vector3 dir) => Physics.RaycastAll(_o, dir, 40f, ~0, QueryTriggerInteraction.Ignore)
                .Where(h => h.collider.transform.IsChildOf(s_walls)).Select(h => h.distance).DefaultIfEmpty(12f).Min();
            float _l = d(Vector3.left), _r = d(Vector3.right), _b = d(Vector3.back), _f = d(Vector3.forward);
            var _bounds = new Bounds();
            _bounds.SetMinMax(new Vector3(_center.x - _l, 0f, _center.z - _b), new Vector3(_center.x + _r, k_wallHeight, _center.z + _f));
            return _bounds;
        }

        /// <summary>A working ceiling lamp: short cord, small shade and glowing bulb (small, so it does not hide the floor).</summary>
        static void pendantLamp(Transform _parent, Vector3 _pos, Color _color, float _intensity, float _range)
        {
            var _t = new GameObject("Lámpara de techo (funciona)").transform;
            _t.SetParent(_parent, false);
            _t.position = new Vector3(_pos.x, 0f, _pos.z);
            cylinder(_t, new Vector3(0f, 3.2f, 0f), 0.02f, k_wallHeight - 3.2f, "Ink", false);
            cylinder(_t, new Vector3(0f, 3.05f, 0f), 0.42f, 0.15f, "Brass", false);
            sphere(_t, new Vector3(0f, 3.0f, 0f), 0.16f, glowName("Warm", _color));
            pointLight(_t, new Vector3(0f, 2.85f, 0f), _color, _intensity, _range);
        }

        static readonly HashSet<string> s_freshProfiles = new HashSet<string>();

        static void roomVolume(Transform _parent, Bounds _room, Mood _mood)
        {
            string _folder = $"{k_folder}/{sceneName(s_index)}";
            if (!AssetDatabase.IsValidFolder(_folder)) AssetDatabase.CreateFolder(k_folder, sceneName(s_index));
            string _path = $"{_folder}/VP_Room_{_mood}.asset";
            var _profile = AssetDatabase.LoadAssetAtPath<UnityEngine.Rendering.VolumeProfile>(_path);
            if (_profile != null && !s_freshProfiles.Contains(_path)) { AssetDatabase.DeleteAsset(_path); _profile = null; }
            s_freshProfiles.Add(_path);
            if (_profile == null)
            {
                _profile = ScriptableObject.CreateInstance<UnityEngine.Rendering.VolumeProfile>();
                AssetDatabase.CreateAsset(_profile, _path);
                var _ca = _profile.Add<ColorAdjustments>(true);
                _ca.postExposure.Override(_mood == Mood.Lit ? 0.45f : _mood == Mood.Mid ? 0.15f : -0.15f);
                _ca.saturation.Override(_mood == Mood.Lit ? 22f : _mood == Mood.Mid ? 8f : -12f);
                _ca.contrast.Override(_mood == Mood.Dark ? 14f : 6f);
                if (_mood == Mood.Dark) _ca.colorFilter.Override(new Color(0.85f, 0.85f, 1f));
                var _vg = _profile.Add<Vignette>(true);
                _vg.intensity.Override(_mood == Mood.Dark ? 0.42f : _mood == Mood.Mid ? 0.28f : 0.18f);
                foreach (var _comp in _profile.components) AssetDatabase.AddObjectToAsset(_comp, _profile);
                EditorUtility.SetDirty(_profile);
            }
            var _go = new GameObject("Volumen local");
            _go.transform.SetParent(_parent, false);
            _go.transform.position = _room.center + Vector3.up * (_room.size.y * 0.5f);
            _go.layer = 2; // ignore raycast: never blocks Senses or the player
            var _box = _go.AddComponent<BoxCollider>();
            _box.isTrigger = true;
            _box.size = new Vector3(_room.size.x, _room.size.y + 2f, _room.size.z);
            var _vol = _go.AddComponent<UnityEngine.Rendering.Volume>();
            _vol.isGlobal = false;
            _vol.blendDistance = 2f;
            _vol.priority = 1f;
            _vol.sharedProfile = _profile;
        }

        static void bakeNavMesh(int _index)
        {
            var _navGo = new GameObject("NavMeshSurface");
            var _surface = _navGo.AddComponent<NavMeshSurface>();
            _surface.collectObjects = CollectObjects.All;
            _surface.useGeometry = UnityEngine.AI.NavMeshCollectGeometry.PhysicsColliders;
            _surface.layerMask = ~(1 << 5);
            _surface.BuildNavMesh();

            string _folder = $"{k_folder}/{sceneName(_index)}";
            if (!AssetDatabase.IsValidFolder(_folder)) AssetDatabase.CreateFolder(k_folder, sceneName(_index));
            string _path = $"{_folder}/NavMesh-{sceneName(_index)}.asset";
            AssetDatabase.DeleteAsset(_path);
            if (_surface.navMeshData != null) AssetDatabase.CreateAsset(_surface.navMeshData, _path);
        }

        static void addToBuildSettings(string _path)
        {
            var _scenes = EditorBuildSettings.scenes.ToList();
            if (_scenes.Any(s => s.path == _path)) return;
            _scenes.Add(new EditorBuildSettingsScene(_path, false)); // prototypes never ship
            EditorBuildSettings.scenes = _scenes.ToArray();
        }
    }
}
