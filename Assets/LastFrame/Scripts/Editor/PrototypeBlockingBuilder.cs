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
    /// Builds the 5 level-blocking prototypes in Assets/LastFrame/Scenes/Prototypes (Prototype 1..5).
    /// Theme: an old cartoon and TV studio. Each level is a set of studio rooms dressed with primitives,
    /// and adds Senses mechanics on top of the previous ones.
    ///
    /// Enemies come in two behaviours (shy/static and patrolling) and carry a melee or a ranged weapon.
    /// Patrol routes, weapons, the player's planned kills (melee from behind, ranged ink shot) and pickups
    /// are drawn under "--- Plan (ocultable) ---": visual only, no logic. Hide that object to see the bare level.
    ///
    /// Working Senses objects (guards, cameras, dog, grass, river...) are cloned from the Senses test room,
    /// where they already work with the shared player, and their references are remapped to the clones.
    /// Menu: Tools > Last Frame > Build Prototype Blocking. Safe to run again (scenes are overwritten).
    /// </summary>
    public static class PrototypeBlockingBuilder
    {
        const string k_folder = "Assets/LastFrame/Scenes/Prototypes";
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

        const float k_wallHeight = 3f;
        const float k_wallThickness = 0.3f;
        const float k_doorWidth = 2.4f;

        enum Kind { See, Hear, NightVision, Binoculars }
        enum Style { Studio, Concrete, Fence }

        static Scene s_source;
        static Transform s_sourceRoot;
        static Dictionary<Object, Object> s_globalMap; // test room player/camera -> this scene's
        static readonly Dictionary<string, Material> s_materials = new Dictionary<string, Material>();
        static Transform s_walls, s_floors, s_props, s_enemies, s_plan, s_labels, s_lights;

        [MenuItem("Tools/Last Frame/Build Prototype Blocking")]
        public static void BuildFromMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            string _report = BuildAll();
            EditorUtility.DisplayDialog("Last Frame", _report, "OK");
        }

        public static string BuildAll()
        {
            var _log = new System.Text.StringBuilder();
            string _previous = SceneManager.GetActiveScene().path;
            createMaterials();

            EditorSceneManager.OpenScene(_previous.Length > 0 ? _previous : k_sourceScene, OpenSceneMode.Single);
            s_source = EditorSceneManager.OpenScene(k_sourceScene, OpenSceneMode.Additive);
            s_sourceRoot = s_source.GetRootGameObjects().First(g => g.name == "--- STATIONS ---").transform;

            try
            {
                System.Action[] _builders = { buildPrototype1, buildPrototype2, buildPrototype3, buildPrototype4, buildPrototype5 };
                for (int i = 0; i < _builders.Length; i++)
                {
                    string _path = $"{k_folder}/Prototype {i + 1}.unity";
                    Scene _scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
                    SceneManager.SetActiveScene(_scene);
                    _builders[i]();
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
            enemy("Recepcionista de seguridad", Kind.See, false, false, V(0f, 10.8f), 180f);

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
            setupScene(false, "PROTOTYPE 3 · ALMACÉN DE ATREZO", V(0f, 0.5f));

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
            Transform _tank = enemy("Guardia del acuario", Kind.See, false, false, V(-5f, 23.2f), 180f);
            killMelee(_tank);

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
            Transform _gardener = enemy("Jardinero vigilante", Kind.See, false, true, V(0f, 52f), 180f);
            killRanged(V(7f, 40f), _gardener);
            memoryTape(V(-12.8f, 56.8f));
            exitDoor(V(-11.5f, 57.8f), "A SONIDO");
        }

        // =====================================================================
        // PROTOTYPE 4 · Estudio de sonido: foley, control y grabación
        // Senses: hearing guards (HearSense), wood x1.5 / carpet x0.5, noise traps (cans), run vs crouch.
        // Enemies: static melee hearing (foley), static melee hearing (control), patrol ranged, static ranged.
        // =====================================================================
        static void buildPrototype4()
        {
            setupScene(false, "PROTOTYPE 4 · ESTUDIO DE SONIDO", V(0f, 0.5f));

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
            Transform _producer = enemy("Productor vigilante", Kind.See, false, true, V(8.5f, 43f), 200f);
            enemy("Músico nervioso", Kind.See, true, true, V(3f, 33f), 90f, V(3f, 33f), V(10.5f, 33f), V(10.5f, 38.5f), V(3f, 38.5f));
            killRanged(V(2.2f, 41f), _producer);
            exitDoor(V(6.5f, 51.8f), "AL BACKLOT");
        }

        // =====================================================================
        // PROTOTYPE 5 · Backlot exterior y archivo de cintas
        // Senses: scent dog (SmellSense), river set (cuts the trail), trash (masks it),
        // binocular guard (SeeBooster, long range). Combines everything.
        // Enemies: dog (patrol), patrol melee (street), static ranged binoculars, static melee hearing.
        // =====================================================================
        static void buildPrototype5()
        {
            setupScene(false, "PROTOTYPE 5 · BACKLOT Y ARCHIVO", V(0f, 0.5f));

            // ---------- Patio trasero ----------
            floor(-14f, -2f, 14f, 33f, "Dirt");
            wallX(-2f, -14f, 14f, Style.Fence);
            wallZ(-14f, -2f, 54f, Style.Fence);
            wallZ(14f, -2f, 54f, Style.Fence);
            roomLabel("PATIO TRASERO", V(0f, 3f));
            trailer(V(-9.5f, 5f), 90f);
            truck(V(8f, 7f), 10f);
            dumpster(V(-12.4f, 12f), 90f);
            clone("Basura 1 (enmascara el olor)", s_props, V(-11.4f, 15f), 90f, k_trash1);
            clone("Basura 2 (enmascara el olor)", s_props, V(-11.4f, 16.8f), 90f, k_trash2);
            waterTower(V(11f, 15.5f));
            leaningFlat(V(-13.2f, 18.5f), 90f, "WallCream"); leaningFlat(V(-13.2f, 21f), 90f, "Red");
            crate(V(8f, 19f), 1.2f, 0f); crate(V(9.3f, 19.3f), 1f, 20f);
            lampPost(V(-3f, 22f)); lampPost(V(4f, 11f));
            grassPatch(V(-4.5f, 14f), 2, 2);

            Transform _dog = clone("Perro guardián (SmellSense)", s_enemies, V(0f, 16f), 0f, k_dog);
            placeDogWaypoints(_dog, new[] { V(-6f, 9f), V(0f, 7f), V(5f, 13f), V(4f, 20f), V(-2f, 22f), V(-7f, 18f) });
            enemyTag(_dog.GetComponentInChildren<ScentFollower>(true).transform.position, "PERRO · PATRULLA · SIGUE EL RASTRO", true);
            patrolRoute(new[] { V(-6f, 9f), V(0f, 7f), V(5f, 13f), V(4f, 20f), V(-2f, 22f), V(-7f, 18f) });

            Transform _river = clone("Set del río (corta el rastro)", s_props, new Vector3(0f, 0.03f, 28f), 67f, k_river); // rotated so the river crosses the yard roughly straight
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
            exitPad(V(0f, 70.8f), "LA ÚLTIMA CINTA");
        }

        // =====================================================================
        // Scene setup
        // =====================================================================

        static void setupScene(bool _night, string _title, Vector3 _start)
        {
            s_globalMap = new Dictionary<Object, Object>();
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
            _sun.intensity = _night ? 0.05f : 2.2f;
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

            Transform _volume = _sourceSystems.Find("Global Volume");
            if (_volume != null)
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
            _player.name = "Player";
            addHierarchyMap(_sourcePlayer, _player, s_globalMap);
            remap(_player, s_globalMap);
            _follow.Target = _player.transform;
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
            Transform _g;
            switch (_kind)
            {
                case Kind.Hear: _g = clone(_name, s_enemies, _pos, _yaw, k_guardHear); break;
                case Kind.NightVision: _g = clone(_name, s_enemies, _pos, _yaw, k_guardNvg, k_nvgHelmet); break;
                case Kind.Binoculars: _g = clone(_name, s_enemies, _pos, _yaw, k_guardBinoculars, k_binoculars); break;
                default: _g = clone(_name, s_enemies, _pos, _yaw, k_guardSee); break;
            }

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

            string _sense = _kind == Kind.Hear ? "OÍDO" : _kind == Kind.NightVision ? "VISIÓN NOCTURNA" : _kind == Kind.Binoculars ? "PRISMÁTICOS" : "VISTA";
            enemyTag(_pos, $"{(_patrol ? "PATRULLA" : "TÍMIDO · ESTÁTICO")} · {(_ranged ? "DISTANCIA" : "MELÉ")} · {_sense}", _patrol);
            if (_patrol && _route != null && _route.Length > 1) patrolRoute(_route);
            return _g;
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
            label("CÁMARA · NO SE PUEDE DESTRUIR", _pos + new Vector3(0f, 0f, -1f), 0.3f, new Color(0.95f, 0.9f, 0.8f), 7f);
            return _c;
        }

        static void killMelee(Transform _guard)
        {
            Vector3 _behind = _guard.position - _guard.forward * 1.4f;
            planDisc("KILL · Melé por detrás", _behind, 1.2f, "KillMelee");
            label("TU MELÉ POR DETRÁS (Q)", _behind + new Vector3(0f, 0f, -0.9f), 0.32f, new Color(0.82f, 0.39f, 0.38f), 7f);
        }

        static void killRanged(Vector3 _from, Transform _target)
        {
            planDisc("KILL · Disparo (posición)", _from, 1.1f, "KillRanged");
            line(s_plan, _from, _target.position, 0.1f, "KillRanged").name = "KILL · Disparo (línea de tiro)";
            label("TU DISPARO (clic izq., 1 tinta)", _from + new Vector3(0f, 0f, -0.9f), 0.32f, new Color(0.91f, 0.59f, 0.29f), 7f);
        }

        static void ammo(Vector3 _pos)
        {
            var _go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            _go.name = "PLAN · Bote de tinta +1";
            _go.transform.SetParent(s_plan);
            _go.transform.position = _pos + Vector3.up * 0.3f;
            _go.transform.localScale = new Vector3(0.35f, 0.3f, 0.35f);
            setupPlanRenderer(_go, "Ammo");
            label("+1 TINTA", _pos + new Vector3(0f, 0f, -0.7f), 0.3f, new Color(0.61f, 0.42f, 0.83f), 4f);
        }

        static void memoryTape(Vector3 _pos)
        {
            planBox("PLAN · Cinta de memoria", _pos + Vector3.up * 0.3f, new Vector3(0.6f, 0.2f, 0.4f), "Memory");
            label("CINTA DE MEMORIA", _pos + new Vector3(0f, 0f, -0.7f), 0.3f, new Color(0.89f, 0.74f, 0.45f), 5f);
        }

        static void exitDoor(Vector3 _doorPos, string _to)
        {
            onAirSign(_doorPos, 180f);
            exitPad(_doorPos + new Vector3(0f, 0f, -1.2f), _to);
        }

        static void exitPad(Vector3 _pos, string _to)
        {
            planBox("SetDoor (salida)", _pos, new Vector3(2.2f, 0.03f, 1.4f), "Exit");
            label("SALIDA · " + _to, _pos + new Vector3(0f, 0f, -1.1f), 0.4f, new Color(0.36f, 0.56f, 0.85f), 8f);
        }

        static void roomLabel(string _text, Vector3 _pos, float _yaw = 0f) =>
            label(_text, _pos, 0.5f, new Color(0.95f, 0.9f, 0.8f, 0.8f), 12f, _yaw);

        // =====================================================================
        // Senses clones and prefabs
        // =====================================================================

        static int s_grassIndex;
        static void grassPatch(Vector3 _center, int _cols, int _rows)
        {
            Transform _folder = s_sourceRoot.Find(k_grassFolder);
            Transform[] _grass = _folder.Cast<Transform>().Where(t => t.GetComponent<StealthZone>() != null).ToArray();
            var _patch = child(s_props, "Arbustos de atrezo (StealthZone)");
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

        static void can(Vector3 _pos, int _variant) => instantiatePrefab($"(P) Can 0{_variant % 5 + 1}", s_props, _pos, _variant * 47f);

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
            string _path = $"{k_materialsFolder}/Checker/M_Block_{_key}.mat";
            if (!AssetDatabase.IsValidFolder($"{k_materialsFolder}/Checker")) AssetDatabase.CreateFolder(k_materialsFolder, "Checker");
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
        }

        static void bakeNavMesh(int _index)
        {
            var _navGo = new GameObject("NavMeshSurface");
            var _surface = _navGo.AddComponent<NavMeshSurface>();
            _surface.collectObjects = CollectObjects.All;
            _surface.useGeometry = UnityEngine.AI.NavMeshCollectGeometry.PhysicsColliders;
            _surface.layerMask = ~(1 << 5);
            _surface.BuildNavMesh();

            string _folder = $"{k_folder}/Prototype {_index}";
            if (!AssetDatabase.IsValidFolder(_folder)) AssetDatabase.CreateFolder(k_folder, $"Prototype {_index}");
            string _path = $"{_folder}/NavMesh-Prototype {_index}.asset";
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
