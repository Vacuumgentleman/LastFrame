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

namespace SensesTestRoom.EditorTools
{
    /// <summary>
    /// Construye "Assets/Scenes/SensesTestRoom.unity": una test room que reúne todas las mecánicas
    /// del Senses Pack (las 13 escenas demo) en estaciones colocadas en fila, con un único jugador,
    /// cámara que lo sigue y HUD para saltar entre estaciones.
    ///
    /// Las escenas demo originales se abren de forma aditiva, se MUEVEN sus objetos a la escena nueva
    /// y se cierran sin guardar, así que no se modifican y todas las referencias internas se conservan.
    /// Se puede volver a ejecutar cuando quieras: Tools > Senses Test Room > Build Scene.
    /// </summary>
    public static class SensesTestRoomBuilder
    {
        const string k_demoFolder = "Assets/CatBorg AI Framework - Mega Bundle/Senses Pack/Scenes/";
        const string k_outputScene = "Assets/Scenes/SensesTestRoom.unity";
        const string k_outputFolder = "Assets/Scenes/SensesTestRoom";
        const string k_playerSourceDemo = "Demo 12 - SmellSense"; // su Player ya trae Scent, Stealth, Noise, etc.
        const float k_gap = 14f;

        static readonly string[] k_requiredTags = { "Ground", "Wood", "Carpet", "WarningTable", "ActionInteractor" };

        // Raíces de las demos que NO se copian (cada demo trae las suyas; la test room tiene unas globales).
        static readonly string[] k_discardedRootPrefixes =
        {
            "Main Camera", "- Main Camera", "Directional Light", "- Directional Light", "- Camera && Light",
            "Global Volume", "EventSystem", "(P) SceneScaner", "SceneFactionManager", "RuntimeStats", "(P) Player"
        };

        class StationDef
        {
            public string Scene, Title, Description, Controls;
            public bool Night;
            public float NightIntensity;
            public Color NightColor;
            public bool FreezePlayer;
            public bool DelayedStart; // la batalla de facciones arranca al entrar
        }

        static readonly StationDef[] k_stations =
        {
            new StationDef
            {
                Scene = "Demo 1 - SeeSense vs Obstacle", Title = "Vista vs Obstáculos",
                Description = "SeeSense + Obstacle. Las cámaras de seguridad no ven a través de un muro sólido, pero sí a través de un obstáculo con 'Obstacle' (la pecera), que solo reduce la visibilidad.",
                Controls = "Pasa por delante de cada cámara y compara cuánto tardan en detectarte."
            },
            new StationDef
            {
                Scene = "Demo 2 - SeeSense vs Stealth", Title = "Vista vs Sigilo",
                Description = "Stealth. Tres centinelas y tres objetivos: sin Stealth, con Stealth y con Stealth más fuerte. El sigilo reduce la velocidad a la que sube la 'awareness' del centinela.",
                Controls = "Observa los indicadores de alerta sobre cada centinela."
            },
            new StationDef
            {
                Scene = "Demo 3 - SeeSense vs Light", Title = "Vista vs Luz (Lumen)",
                Description = "Lumen Stealth AddOn. El jugador mide la luz que recibe (LightDetector + LumenProbes): en la sombra es casi invisible, bajo las farolas lo ven enseguida. Estación nocturna.",
                Controls = "Muévete entre las farolas y la oscuridad con Ctrl (sigilo) y mira la barra de luz.",
                Night = true, NightIntensity = 0.04f, NightColor = new Color(0.639f, 0.862f, 0.955f)
            },
            new StationDef
            {
                Scene = "Demo 4 - SeeSense vs Zooner Steath Buff", Title = "Zonas de Sigilo (hierba)",
                Description = "Zoner Stealth AddOn + StealthZone. La hierba es una zona que da un bonus de sigilo mientras estás dentro y agachado.",
                Controls = "Cruza el campo de las cámaras por la hierba con Ctrl y luego por fuera."
            },
            new StationDef
            {
                Scene = "Demo 5 - SeeSense vs Distractors", Title = "Distractores",
                Description = "Distractores visuales: un TargetSenses de facción None (la bomba de humo) atrae la mirada del centinela mientras todavía no te ha detectado.",
                Controls = "Pulsa el botón de la UI, coloca el humo con el ratón (clic izq.) y cuela por detrás."
            },
            new StationDef
            {
                Scene = "Demo 6 - SeeSense vs RigCollider", Title = "RigCollider",
                Description = "RigCollider. En vez de un único collider, el objetivo usa los huesos del rig: el centinela lo ve aunque solo asome una parte del cuerpo por detrás de la caja.",
                Controls = "Compara el ejemplo con CapsuleCollider (izq.) y con RigCollider (der.)."
            },
            new StationDef
            {
                Scene = "Demo 7 - SeeSense vs See Booster - Binocualrs", Title = "See Booster: Prismáticos y Gafas",
                Description = "SeeBooster. Objetos que modifican la vista del centinela: los prismáticos amplían el alcance, las gafas de sol lo reducen.",
                Controls = "Fíjate en el tamaño de cada cono de visión."
            },
            new StationDef
            {
                Scene = "Demo 8 - SeeSense vs See Booster - NVG", Title = "See Booster: Visión Nocturna",
                Description = "SeeBooster (NVG). De noche el centinela normal apenas ve; el que lleva gafas de visión nocturna sí. Estación nocturna.",
                Controls = "Compara los dos centinelas en la oscuridad.",
                Night = true, NightIntensity = 2f, NightColor = Color.white
            },
            new StationDef
            {
                Scene = "Demo 9 - SeeSenses vs LookFor", Title = "LookFor (tipos de búsqueda)",
                Description = "SeeSenseSearchType: el centinela decide a quién mirar (el más cercano, el más visible, el más centrado...). Aquí controlas a los maniquíes en lugar de al jugador.",
                Controls = "1/2/3 elige maniquí · WASD lo mueve · el ratón lo orienta · N para salir.",
                FreezePlayer = true
            },
            new StationDef
            {
                Scene = "Demo 10 - HearSense vs Noise - NavMesh", Title = "Oído vs Ruido (NavMesh)",
                Description = "HearSense + Noise con propagación por NavMesh: el sonido rodea las paredes siguiendo el camino real, así que llega más débil a quien está detrás de un muro.",
                Controls = "Mira qué centinelas oyen los pasos de cada maniquí."
            },
            new StationDef
            {
                Scene = "Demo 11 - HearSense vs Noise Traps", Title = "Trampas de Ruido y Superficies",
                Description = "NoiseTrap + SoundPropagationSettings. Las latas hacen ruido al golpearlas; la madera amplifica tus pasos (x1.5) y la alfombra los amortigua (x0.5).",
                Controls = "Camina, corre y agáchate sobre cada superficie; da patadas a las latas."
            },
            new StationDef
            {
                Scene = "Demo 12 - SmellSense", Title = "Olfato (rastro y distractores)",
                Description = "SmellSense + Scent. Dejas un rastro de olor que el perro sigue. El río bloquea el rastro y las bolsas de basura lo enmascaran (ScentZoneDistractor).",
                Controls = "Deja que el perro te huela y despístalo cruzando el río o pasando junto a la basura."
            },
            new StationDef
            {
                Scene = "Demo 13 - Factions (Stress Test)", Title = "Facciones (batalla)",
                Description = "Factions + rendimiento. Dos facciones con torres y minions que solo atacan a la facción enemiga. La batalla empieza al entrar por primera vez. Ojo: tú perteneces a la facción azul.",
                Controls = "Mira cómo cada bando detecta solo a sus enemigos.",
                DelayedStart = true
            },
        };

        [MenuItem("Tools/Senses Test Room/Build Scene")]
        public static void BuildFromMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Build();
            EditorUtility.DisplayDialog("Senses Test Room", "Escena creada en " + k_outputScene + ".\nDale a Play.", "OK");
        }

        public static string Build()
        {
            var _log = new System.Text.StringBuilder();
            ensureTags();

            Scene _scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            SceneManager.SetActiveScene(_scene);

            Material _daySkybox = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Skybox.mat");
            Material _nightSkybox = findAsset<Material>("SkyBox - NightScene t:Material");
            RenderSettings.skybox = _daySkybox;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;

            // ---------- Globales ----------
            var _systems = new GameObject("--- TEST ROOM ---");

            var _sunGo = new GameObject("Directional Light");
            _sunGo.transform.SetParent(_systems.transform);
            _sunGo.transform.rotation = Quaternion.Euler(50f, 330f, 0f);
            var _sun = _sunGo.AddComponent<Light>();
            _sun.type = LightType.Directional;
            _sun.intensity = 4f;
            _sun.color = new Color(1f, 0.957f, 0.839f);
            _sun.shadows = LightShadows.Soft;
            _sunGo.AddComponent<UniversalAdditionalLightData>();
            RenderSettings.sun = _sun;

            var _camGo = new GameObject("Main Camera") { tag = "MainCamera" };
            _camGo.transform.SetParent(_systems.transform);
            var _cam = _camGo.AddComponent<Camera>();
            _cam.fieldOfView = 50f;
            _cam.farClipPlane = 400f;
            _camGo.AddComponent<AudioListener>();
            _camGo.AddComponent<UniversalAdditionalCameraData>();
            _camGo.AddComponent<CameraRay_DemoScript>(); // clic en los carteles ⚠ de las demos
            var _follow = _camGo.AddComponent<TestRoomCamera>();

            var _factionGo = new GameObject("SceneFactionManager");
            _factionGo.transform.SetParent(_systems.transform);
            _factionGo.AddComponent<SceneFactionManager>().FactionSettings =
                Resources.Load<FactionSettings>("FactionSettings (SO)/DefaultFactionSettings");

            // ---------- Jugador (de la demo de olfato: trae todos los componentes) ----------
            GameObject _player = extractPlayer(_scene);
            _player.transform.SetParent(null);
            configurePlayer(_player, _camGo.transform);
            _follow.Target = _player.transform;

            var _stationsRoot = new GameObject("--- STATIONS ---");
            var _stations = new List<TestRoomStation>();
            bool _globalsTaken = false;
            float _cursorX = 0f;

            for (int i = 0; i < k_stations.Length; i++)
            {
                StationDef _def = k_stations[i];
                string _path = k_demoFolder + _def.Scene + ".unity";
                Scene _demo = EditorSceneManager.OpenScene(_path, OpenSceneMode.Additive);

                var _stationGo = new GameObject($"Station {i + 1:00} · {_def.Title}");
                _stationGo.transform.SetParent(_stationsRoot.transform);
                var _station = _stationGo.AddComponent<TestRoomStation>();
                _station.Title = _def.Title;
                _station.Description = _def.Description;
                _station.ControlsHint = _def.Controls;
                _station.FreezePlayer = _def.FreezePlayer;
                if (_def.Night)
                {
                    _station.OverrideLighting = true;
                    _station.LightIntensity = _def.NightIntensity;
                    _station.LightColor = _def.NightColor;
                    _station.Skybox = _nightSkybox;
                }

                GameObject[] _roots = _demo.GetRootGameObjects();
                var _kept = new List<GameObject>();
                var _discarded = new List<GameObject>();
                Vector3? _demoPlayerPos = null;

                foreach (GameObject _root in _roots)
                {
                    bool _discard = k_discardedRootPrefixes.Any(p => _root.name.StartsWith(p)) || _root.name == "GameObject";

                    // De la primera demo nos quedamos el EventSystem y el Global Volume como globales.
                    if (!_globalsTaken && (_root.name == "EventSystem" || _root.name == "Global Volume"))
                    {
                        SceneManager.MoveGameObjectToScene(_root, _scene);
                        _root.transform.SetParent(_systems.transform, true);
                        continue;
                    }

                    if (_root.name == "(P) Player") _demoPlayerPos = _root.transform.position;
                    (_discard ? _discarded : _kept).Add(_root);
                }
                _globalsTaken = true;

                remapReferences(_kept, _discarded, _player);

                foreach (GameObject _root in _kept)
                {
                    SceneManager.MoveGameObjectToScene(_root, _scene);
                    _root.transform.SetParent(_stationGo.transform, false);
                }

                EditorSceneManager.CloseScene(_demo, true); // sin guardar: la demo original no cambia

                // Suelos de las demos (100x100) fuera: la test room tiene un suelo común.
                foreach (Transform _t in _stationGo.GetComponentsInChildren<Transform>(true).ToArray())
                {
                    if (_t == null || _t.parent == null || !_t.parent.name.StartsWith("- Enviroment")) continue;
                    if (_t.name == "Plane" || _t.name == "-- Ground") Object.DestroyImmediate(_t.gameObject);
                }

                // El río de la demo de olfato está a ras de suelo: lo subimos un pelo para que se vea.
                Transform _blockers = _stationGo.transform.Find("- Scent Blockers");
                if (_blockers != null) _blockers.position += Vector3.up * 0.03f;

                // UI de la demo: solo visible dentro de la estación.
                var _ui = new List<GameObject>();
                foreach (GameObject _root in _kept)
                {
                    if (_root.GetComponent<Canvas>() != null) _ui.Add(_root);
                }
                _station.UiRoots = _ui.ToArray();

                // Maniquíes controlables (demo LookFor): solo responden dentro de su estación.
                var _inside = new List<Behaviour>();
                _inside.AddRange(_stationGo.GetComponentsInChildren<PlayerControler_MK1>(true));
                _station.EnabledOnlyInside = _inside.ToArray();

                // Paneles de victoria de facciones: no salir del Play Mode, solo reanudar.
                foreach (PressAnyKeyToExit _exit in _stationGo.GetComponentsInChildren<PressAnyKeyToExit>(true))
                {
                    GameObject _panel = _exit.gameObject;
                    Object.DestroyImmediate(_exit);
                    _panel.AddComponent<TestRoomResumeOnKey>();
                }

                // Colocar la estación en fila según su tamaño real.
                Bounds _bounds = computeBounds(_stationGo);
                float _offsetX = _cursorX - _bounds.min.x;
                _stationGo.transform.position = new Vector3(_offsetX, 0f, 0f);
                _station.MinX = _cursorX - k_gap * 0.5f;
                _station.MaxX = _cursorX + _bounds.size.x + k_gap * 0.5f;
                _cursorX += _bounds.size.x + k_gap;

                if (_def.DelayedStart)
                {
                    var _delayed = new List<GameObject>();
                    foreach (Transform _child in _stationGo.transform)
                    {
                        if (_child.GetComponentInChildren<MinionSpawner_DemoScript>(true) != null && _child.gameObject.activeSelf)
                        {
                            _child.gameObject.SetActive(false);
                            _delayed.Add(_child.gameObject);
                        }
                    }
                    _station.ActivateOnFirstEnter = _delayed.ToArray();
                }

                // Punto de aparición: donde estaba el jugador en la demo, o delante del contenido.
                var _spawn = new GameObject("SpawnPoint").transform;
                _spawn.SetParent(_stationGo.transform, false);
                Vector3 _localBoundsCenter = _bounds.center;
                _spawn.localPosition = _demoPlayerPos.HasValue
                    ? new Vector3(_demoPlayerPos.Value.x, 0.3f, _demoPlayerPos.Value.z)
                    : new Vector3(_localBoundsCenter.x, 0.3f, Mathf.Min(_bounds.min.z - 3f, -4f));
                _station.SpawnPoint = _spawn;

                if (_def.FreezePlayer)
                {
                    // El jugador espera a un lado (fuera de la vista de los centinelas) y la cámara enfoca el contenido.
                    _spawn.localPosition = new Vector3(_bounds.min.x - 4f, 0.3f, _bounds.min.z - 3f);
                    var _focus = new GameObject("CameraFocus").transform;
                    _focus.SetParent(_stationGo.transform, false);
                    _focus.localPosition = new Vector3(_localBoundsCenter.x, 0f, _localBoundsCenter.z - 2f);
                    _station.CameraFocus = _focus;
                }

                createSign(_stationGo.transform, $"{i + 1:00} · {_def.Title}", _spawn.localPosition + new Vector3(0f, 0.02f, -2.5f));
                _stations.Add(_station);
                _log.AppendLine($"{_stationGo.name}: x[{_station.MinX:0.0},{_station.MaxX:0.0}] kept={_kept.Count} ui={_ui.Count}");
            }

            // La primera estación empieza desde -inf y la última hasta +inf para que el jugador nunca quede "fuera".
            _stations[0].MinX = float.MinValue;
            _stations[_stations.Count - 1].MaxX = float.MaxValue;

            // ---------- Suelo común ----------
            float _minX = -20f, _maxX = _cursorX + 10f;
            var _ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            _ground.name = "Ground";
            _ground.tag = "Ground";
            _ground.isStatic = true;
            _ground.transform.SetParent(_systems.transform);
            _ground.transform.position = new Vector3((_minX + _maxX) * 0.5f, 0f, 5f);
            _ground.transform.localScale = new Vector3((_maxX - _minX) / 10f, 1f, 7f);
            Material _groundMat = findAsset<Material>("Ground t:Material", "Senses Pack/Materials");
            if (_groundMat != null) _ground.GetComponent<MeshRenderer>().sharedMaterial = _groundMat;

            // ---------- Manager ----------
            var _managerGo = new GameObject("TestRoomManager");
            _managerGo.transform.SetParent(_systems.transform);
            var _manager = _managerGo.AddComponent<TestRoomManager>();
            _manager.Stations = _stations.ToArray();
            _manager.Player = _player.transform;
            _manager.FollowCamera = _follow;
            _manager.Sun = _sun;
            _manager.DefaultSkybox = _daySkybox;

            _player.transform.position = _stations[0].SpawnPoint.position;
            _follow.Snap();

            // ---------- NavMesh (la propagación de ruido de la demo 10 lo usa) ----------
            if (!AssetDatabase.IsValidFolder(k_outputFolder)) AssetDatabase.CreateFolder("Assets/Scenes", "SensesTestRoom");
            var _navGo = new GameObject("NavMeshSurface");
            _navGo.transform.SetParent(_systems.transform);
            var _surface = _navGo.AddComponent<NavMeshSurface>();
            _surface.collectObjects = CollectObjects.All;
            _surface.useGeometry = UnityEngine.AI.NavMeshCollectGeometry.RenderMeshes;
            _surface.layerMask = ~(1 << 5); // sin UI
            // Textos y conos de visión no son geometría (y el baker rechaza sus mallas con un error).
            var _notGeometry = new List<GameObject>();
            _notGeometry.AddRange(Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None).Select(c => c.gameObject));
            _notGeometry.AddRange(Object.FindObjectsByType<FOV>(FindObjectsInactive.Include, FindObjectsSortMode.None).Select(c => c.gameObject));
            foreach (GameObject _go in _notGeometry)
            {
                if (_go.GetComponent<NavMeshModifier>() == null) _go.AddComponent<NavMeshModifier>().ignoreFromBuild = true;
            }
            _surface.BuildNavMesh();
            string _navPath = k_outputFolder + "/NavMesh-SensesTestRoom.asset";
            AssetDatabase.DeleteAsset(_navPath);
            if (_surface.navMeshData != null) AssetDatabase.CreateAsset(_surface.navMeshData, _navPath);

            EditorSceneManager.SaveScene(_scene, k_outputScene);
            addToBuildSettings(k_outputScene);
            AssetDatabase.SaveAssets();

            _log.AppendLine("Saved " + k_outputScene);
            Debug.Log("[SensesTestRoom]\n" + _log);
            return _log.ToString();
        }

        // ------------------------------------------------------------------

        static GameObject extractPlayer(Scene _target)
        {
            Scene _demo = EditorSceneManager.OpenScene(k_demoFolder + k_playerSourceDemo + ".unity", OpenSceneMode.Additive);
            GameObject _player = _demo.GetRootGameObjects().First(g => g.name == "(P) Player");
            SceneManager.MoveGameObjectToScene(_player, _target);
            EditorSceneManager.CloseScene(_demo, true);
            _player.name = "Player";
            return _player;
        }

        static void configurePlayer(GameObject _player, Transform _camera)
        {
            // Facción 1 (como en las demos 3 y 5) para que los centinelas MK2 lo traten como amenaza y
            // los distractores (facción None) como distracción. Las demás demos buscan None y Faction1.
            setProperty(_player.GetComponent<TargetSenses>(), "m_factionName", p => p.enumValueIndex = 1);

            // Stealth con los dos AddOns activos (Lumen = demo 3, Zoner = demo 4) y los valores de la demo 3.
            Stealth _stealth = _player.GetComponent<Stealth>();
            setProperty(_stealth, "<UseLumenStealthAddon>k__BackingField", p => p.boolValue = true);
            setProperty(_stealth, "<UseZonerStealthAddon>k__BackingField", p => p.boolValue = true);
            setProperty(_stealth, "<StealthBuff>k__BackingField", p => p.floatValue = 1f);
            setProperty(_stealth, "<OffsetAxisY>k__BackingField", p => p.floatValue = 2.395757f);

            setProperty(_player.GetComponent<PlayerMover>(), "m_cameraRotationTransform", p => p.objectReferenceValue = _camera);
        }

        static void setProperty(Object _target, string _name, System.Action<SerializedProperty> _set)
        {
            if (_target == null) return;
            var _so = new SerializedObject(_target);
            SerializedProperty _p = _so.FindProperty(_name);
            if (_p == null)
            {
                Debug.LogWarning($"[SensesTestRoom] {_target.GetType().Name}.{_name} no encontrado");
                return;
            }
            _set(_p);
            _so.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>
        /// Las referencias de los objetos que se quedan hacia el jugador de la demo (que se descarta)
        /// pasan a apuntar al jugador único de la test room (mismo hijo, mismo tipo de componente).
        /// </summary>
        static void remapReferences(List<GameObject> _kept, List<GameObject> _discarded, GameObject _newPlayer)
        {
            var _map = new Dictionary<Object, Object>();
            foreach (GameObject _old in _discarded.Where(g => g.name == "(P) Player"))
            {
                foreach (Transform _t in _old.GetComponentsInChildren<Transform>(true))
                {
                    string _rel = relativePath(_old.transform, _t);
                    Transform _match = string.IsNullOrEmpty(_rel) ? _newPlayer.transform : _newPlayer.transform.Find(_rel);
                    if (_match == null) continue;

                    _map[_t.gameObject] = _match.gameObject;
                    foreach (Component _c in _t.GetComponents<Component>())
                    {
                        if (_c == null) continue;
                        Component[] _same = _t.GetComponents(_c.GetType());
                        Component[] _sameNew = _match.GetComponents(_c.GetType());
                        int _index = System.Array.IndexOf(_same, _c);
                        if (_index >= 0 && _index < _sameNew.Length) _map[_c] = _sameNew[_index];
                    }
                }
            }

            if (_map.Count == 0) return;

            foreach (GameObject _root in _kept)
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
                        if (_value != null && _map.TryGetValue(_value, out Object _replacement))
                        {
                            _it.objectReferenceValue = _replacement;
                            _changed = true;
                        }
                    }
                    if (_changed) _so.ApplyModifiedPropertiesWithoutUndo();
                }
            }
        }

        static string relativePath(Transform _root, Transform _t)
        {
            var _parts = new List<string>();
            while (_t != null && _t != _root)
            {
                _parts.Insert(0, _t.name);
                _t = _t.parent;
            }
            return string.Join("/", _parts);
        }

        static Bounds computeBounds(GameObject _root)
        {
            bool _has = false;
            Bounds _b = new Bounds(_root.transform.position, Vector3.zero);
            foreach (Renderer _r in _root.GetComponentsInChildren<Renderer>(false))
            {
                if (_r.gameObject.layer == 5 || _r is ParticleSystemRenderer) continue;
                if (_r.GetComponent<TMP_Text>() != null) continue;
                Bounds _rb = _r.bounds;
                if (_rb.size.x > 60f || _rb.size.z > 60f) continue; // río de 100 m de largo, etc.
                if (!_has) { _b = _rb; _has = true; }
                else _b.Encapsulate(_rb);
            }
            if (!_has) _b = new Bounds(_root.transform.position, new Vector3(20f, 2f, 10f));
            return _b;
        }

        static void createSign(Transform _parent, string _text, Vector3 _localPos)
        {
            var _go = new GameObject("Sign");
            _go.transform.SetParent(_parent, false);
            _go.transform.localPosition = _localPos;
            _go.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            var _tmp = _go.AddComponent<TextMeshPro>();
            _tmp.text = _text;
            _tmp.fontSize = 8f;
            _tmp.alignment = TextAlignmentOptions.Center;
            _tmp.color = new Color(1f, 0.85f, 0.35f);
            _tmp.rectTransform.sizeDelta = new Vector2(24f, 3f);
            _tmp.textWrappingMode = TextWrappingModes.NoWrap;
        }

        static void ensureTags()
        {
            var _tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty _tags = _tagManager.FindProperty("tags");
            foreach (string _tag in k_requiredTags)
            {
                bool _exists = false;
                for (int i = 0; i < _tags.arraySize; i++)
                {
                    if (_tags.GetArrayElementAtIndex(i).stringValue == _tag) _exists = true;
                }
                if (_exists) continue;
                _tags.InsertArrayElementAtIndex(_tags.arraySize);
                _tags.GetArrayElementAtIndex(_tags.arraySize - 1).stringValue = _tag;
            }
            _tagManager.ApplyModifiedPropertiesWithoutUndo();
        }

        static void addToBuildSettings(string _path)
        {
            var _scenes = EditorBuildSettings.scenes.ToList();
            if (_scenes.Any(s => s.path == _path)) return;
            _scenes.Add(new EditorBuildSettingsScene(_path, true));
            EditorBuildSettings.scenes = _scenes.ToArray();
        }

        static T findAsset<T>(string _filter, string _pathContains = null) where T : Object
        {
            foreach (string _guid in AssetDatabase.FindAssets(_filter))
            {
                string _path = AssetDatabase.GUIDToAssetPath(_guid);
                if (_pathContains != null && !_path.Contains(_pathContains)) continue;
                T _asset = AssetDatabase.LoadAssetAtPath<T>(_path);
                if (_asset != null) return _asset;
            }
            return null;
        }
    }
}
