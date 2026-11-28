using System.Collections.Generic;
using System.IO;
using System.Linq;
using FlatKit;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace LastFrame.EditorTools
{
    /// <summary>
    /// Project art setup, kept as code so it can be re-run and reviewed:
    /// - Rendering: Last Frame's own URP asset, made from the Flat Kit "Room" demo asset the project was using,
    ///   without the demo's height fog (its green-to-red gradient tinted every scene). Renderer 0 is the default
    ///   one; renderers 1-3 ("Look" ink, "Pencil" graphite, "FlatTV" thick black) add Flat Kit's screen-space
    ///   outline with their own line style and use Forward+. Each prototype camera picks one.
    /// - Placeholder sprites: Ollie and the AI otter / capybara cut out from the reference art in Docs/Referencias,
    ///   until the final sprites exist.
    /// </summary>
    public static class LastFrameArtSetup
    {
        const string k_renderingFolder = "Assets/LastFrame/Settings/Rendering";
        const string k_pipelinePath = k_renderingFolder + "/URP_LastFrame.asset";
        const string k_rendererPath = k_renderingFolder + "/URP_LastFrame_Renderer.asset";
        const string k_lookRendererPath = k_renderingFolder + "/URP_LastFrame_Renderer_Look.asset";
        const string k_outlinePath = k_renderingFolder + "/OUT_LastFrame_Ink.asset";
        const string k_demoPipeline = "Assets/FlatKit/Demos/[Demo] Room/Room-URP Asset.asset";
        const string k_demoRenderer = "Assets/FlatKit/Demos/[Demo] Room/Room-Renderer.asset";
        const string k_demoOutline = "Assets/FlatKit/Demos/Common/URP Configs/[FlatKit] Example Outline Settings.asset";

        public const int k_lookRendererIndex = 1;

        const string k_spritesFolder = "Assets/LastFrame/Art/Sprites/Placeholders";
        public const string k_spriteOllie = k_spritesFolder + "/SPR_Ollie_Placeholder.png";
        public const string k_spriteOtter = k_spritesFolder + "/SPR_EnemyOtter_Placeholder.png";
        public const string k_spriteCapybara = k_spritesFolder + "/SPR_EnemyCapybara_Placeholder.png";
        public const string k_spriteOllieOutlined = k_spritesFolder + "/SPR_Ollie_Placeholder_Outlined.png";
        public const string k_spriteOtterOutlined = k_spritesFolder + "/SPR_EnemyOtter_Placeholder_Outlined.png";
        public const string k_spriteCapybaraOutlined = k_spritesFolder + "/SPR_EnemyCapybara_Placeholder_Outlined.png";
        public const string k_paperGrain = "Assets/LastFrame/Art/Textures/TEX_PaperGrain.png";
        public const string k_lightPool = "Assets/LastFrame/Art/Textures/TEX_LightPool.png";
        public const string k_woodPlanks = "Assets/LastFrame/Art/Textures/TEX_WoodPlanks.png";
        public const string k_windowCookie = "Assets/LastFrame/Art/Textures/TEX_WindowCookie.png";

        /// <summary>Look renderers after renderer 0, in pipeline order (index 1, 2, 3).</summary>
        static readonly string[] k_lookRenderers = { "Look", "Pencil", "FlatTV" };
        public static int LookRendererIndex(string _name) { int i = System.Array.IndexOf(k_lookRenderers, _name); return i < 0 ? 1 : i + 1; }
        static string lookRendererPath(string _name) => _name == "Look" ? k_lookRendererPath : $"{k_renderingFolder}/URP_LastFrame_Renderer_{_name}.asset";
        static string outlinePath(string _name) => _name == "Look" ? k_outlinePath : $"{k_renderingFolder}/OUT_LastFrame_{_name}.asset";

        // =====================================================================
        // Rendering
        // =====================================================================

        [MenuItem("Tools/Last Frame/Setup Rendering (URP Last Frame)")]
        public static void SetupRenderingFromMenu() => EditorUtility.DisplayDialog("Last Frame", SetupRendering(), "OK");

        /// <summary>Creates (once) and activates the Last Frame URP asset. Safe to run again.</summary>
        public static string SetupRendering()
        {
            var _log = new System.Text.StringBuilder();
            ensureFolder(k_renderingFolder);

            // Renderer 0: the demo renderer without its fog (SSAO and per-object outlines stay)
            if (AssetDatabase.LoadAssetAtPath<ScriptableRendererData>(k_rendererPath) == null)
            {
                AssetDatabase.CopyAsset(k_demoRenderer, k_rendererPath);
                var _renderer = AssetDatabase.LoadAssetAtPath<ScriptableRendererData>(k_rendererPath);
                _renderer.name = "URP_LastFrame_Renderer";
                removeFeatures(_renderer, "FlatKitFog");
                _log.AppendLine("Created " + k_rendererPath + " (no fog)");
            }

            // Renderers 1-3: renderer 0 + Flat Kit's screen-space outline, each with its own line style, in Forward+
            // (no 4-lights-per-object limit, so every practical lamp lights big floors)
            foreach (string _name in k_lookRenderers)
            {
                string _path = lookRendererPath(_name);
                if (AssetDatabase.LoadAssetAtPath<ScriptableRendererData>(_path) == null)
                {
                    AssetDatabase.SaveAssets();
                    AssetDatabase.CopyAsset(k_rendererPath, _path);
                    var _data = AssetDatabase.LoadAssetAtPath<ScriptableRendererData>(_path);
                    _data.name = Path.GetFileNameWithoutExtension(_path);
                    if (AssetDatabase.LoadAssetAtPath<OutlineSettings>(outlinePath(_name)) == null) AssetDatabase.CopyAsset(k_demoOutline, outlinePath(_name));
                    var _outline = ScriptableObject.CreateInstance<FlatKitOutline>();
                    _outline.name = $"Flat Kit Outline ({_name})";
                    _outline.settings = AssetDatabase.LoadAssetAtPath<OutlineSettings>(outlinePath(_name));
                    addFeature(_data, _outline);
                    _log.AppendLine("Created " + _path);
                }
                var _renderer = AssetDatabase.LoadAssetAtPath<ScriptableRendererData>(_path);
                var _settings = AssetDatabase.LoadAssetAtPath<OutlineSettings>(outlinePath(_name));
                if (_settings != null) configureOutline(_name, _settings);
                foreach (ScriptableRendererFeature _f in _renderer.rendererFeatures)
                    if (_f is FlatKitOutline) _f.Create(); // edits by script do not fire OnValidate: push them to the effect
                var _rso = new SerializedObject(_renderer);
                SerializedProperty _mode = _rso.FindProperty("m_RenderingMode");
                if (_mode != null && _mode.intValue != 2)
                {
                    _mode.intValue = 2; // Forward+
                    _rso.ApplyModifiedPropertiesWithoutUndo();
                    EditorUtility.SetDirty(_renderer);
                    _log.AppendLine(_name + " renderer -> Forward+");
                }
            }

            // The pipeline asset: the demo's quality settings, with all the renderers
            if (AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(k_pipelinePath) == null)
            {
                AssetDatabase.CopyAsset(k_demoPipeline, k_pipelinePath);
                _log.AppendLine("Created " + k_pipelinePath);
            }
            var _pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(k_pipelinePath);
            var _so = new SerializedObject(_pipeline);
            SerializedProperty _list = _so.FindProperty("m_RendererDataList");
            _list.arraySize = 1 + k_lookRenderers.Length;
            _list.GetArrayElementAtIndex(0).objectReferenceValue = AssetDatabase.LoadAssetAtPath<ScriptableRendererData>(k_rendererPath);
            for (int i = 0; i < k_lookRenderers.Length; i++)
                _list.GetArrayElementAtIndex(i + 1).objectReferenceValue = AssetDatabase.LoadAssetAtPath<ScriptableRendererData>(lookRendererPath(k_lookRenderers[i]));
            _so.FindProperty("m_DefaultRendererIndex").intValue = 0;
            // Spot and point light shadows (v8 motivated lighting): a bigger atlas so several lamps can cast shadows
            _so.FindProperty("m_AdditionalLightShadowsSupported").boolValue = true;
            _so.FindProperty("m_AdditionalLightsShadowmapResolution").intValue = 2048;
            _so.ApplyModifiedPropertiesWithoutUndo();

            // Use it wherever the project used the demo asset (graphics settings and quality levels)
            var _demo = AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>(k_demoPipeline);
            if (GraphicsSettings.defaultRenderPipeline == _demo || GraphicsSettings.defaultRenderPipeline == null)
            {
                GraphicsSettings.defaultRenderPipeline = _pipeline;
                _log.AppendLine("Graphics settings -> URP_LastFrame");
            }
            Object _quality = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/QualitySettings.asset").FirstOrDefault();
            if (_quality != null)
            {
                var _qso = new SerializedObject(_quality);
                SerializedProperty _levels = _qso.FindProperty("m_QualitySettings");
                for (int i = 0; i < _levels.arraySize; i++)
                {
                    SerializedProperty _rp = _levels.GetArrayElementAtIndex(i).FindPropertyRelative("customRenderPipeline");
                    if (_rp != null && _rp.objectReferenceValue == _demo)
                    {
                        _rp.objectReferenceValue = _pipeline;
                        _log.AppendLine($"Quality level {i} -> URP_LastFrame");
                    }
                }
                _qso.ApplyModifiedPropertiesWithoutUndo();
            }

            AssetDatabase.SaveAssets();
            return _log.Length > 0 ? _log.ToString() : "Rendering already set up.";
        }

        static void configureOutline(string _name, OutlineSettings _settings)
        {
            configureInkOutline(_settings);
            if (_name == "Pencil")
            {
                // thin graphite lines, also between color areas, like a pencil drawing under the paint
                _settings.edgeColor = new Color32(0x4A, 0x3E, 0x48, 0xE6);
                _settings.thickness = 1;
                _settings.useColor = true;
                _settings.minColorThreshold = 0.12f;
                _settings.maxColorThreshold = 0.3f;
            }
            else if (_name == "FlatTV")
            {
                // thick black ink, the only thing that separates the flat color shapes
                _settings.edgeColor = new Color32(0x10, 0x0C, 0x12, 0xFF);
                _settings.thickness = 3;
                _settings.minNormalsThreshold = 0.2f;
                _settings.maxNormalsThreshold = 0.5f;
                _settings.useColor = false; // color edges would ink every floor tile
            }
            EditorUtility.SetDirty(_settings);
        }

        /// <summary>Thick ink lines from depth and normals, like a pencil-and-ink cartoon background.</summary>
        static void configureInkOutline(OutlineSettings _settings)
        {
            _settings.edgeColor = new Color32(0x1C, 0x14, 0x1A, 0xFF);
            _settings.thickness = 2;
            _settings.resolutionInvariant = true;
            _settings.fadeWithDistance = false;
            _settings.useDepth = true;
            // Depth edges only for real gaps: low thresholds fill walls seen at a grazing angle from far away
            _settings.minDepthThreshold = 0.6f;
            _settings.maxDepthThreshold = 1.2f;
            _settings.useNormals = true;
            _settings.minNormalsThreshold = 0.25f;
            _settings.maxNormalsThreshold = 0.6f;
            _settings.useColor = false;
            _settings.renderEvent = RenderPassEvent.BeforeRenderingTransparents; // sprites and plan labels stay clean
            _settings.outlineOnly = false;
            _settings.applyInSceneView = false;
            EditorUtility.SetDirty(_settings);
        }

        static void removeFeatures(ScriptableRendererData _data, string _typeName)
        {
            var _so = new SerializedObject(_data);
            SerializedProperty _features = _so.FindProperty("m_RendererFeatures");
            SerializedProperty _map = _so.FindProperty("m_RendererFeatureMap");
            for (int i = _features.arraySize - 1; i >= 0; i--)
            {
                Object _feature = _features.GetArrayElementAtIndex(i).objectReferenceValue;
                if (_feature == null || _feature.GetType().Name != _typeName) continue;
                _features.GetArrayElementAtIndex(i).objectReferenceValue = null;
                _features.DeleteArrayElementAtIndex(i);
                if (i < _map.arraySize) _map.DeleteArrayElementAtIndex(i);
                AssetDatabase.RemoveObjectFromAsset(_feature);
                Object.DestroyImmediate(_feature, true);
            }
            _so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(_data);
        }

        static void addFeature(ScriptableRendererData _data, ScriptableRendererFeature _feature)
        {
            AssetDatabase.AddObjectToAsset(_feature, _data);
            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(_feature, out string _, out long _id);
            var _so = new SerializedObject(_data);
            SerializedProperty _features = _so.FindProperty("m_RendererFeatures");
            SerializedProperty _map = _so.FindProperty("m_RendererFeatureMap");
            _features.arraySize++;
            _features.GetArrayElementAtIndex(_features.arraySize - 1).objectReferenceValue = _feature;
            _map.arraySize++;
            _map.GetArrayElementAtIndex(_map.arraySize - 1).longValue = _id;
            _so.ApplyModifiedPropertiesWithoutUndo();
            _feature.Create();
            EditorUtility.SetDirty(_data);
        }

        // =====================================================================
        // Placeholder sprites
        // =====================================================================

        [MenuItem("Tools/Last Frame/Build Placeholder Sprites")]
        public static void BuildPlaceholderSpritesFromMenu() => EditorUtility.DisplayDialog("Last Frame", BuildPlaceholderSprites(), "OK");

        /// <summary>Cuts the reference art (flat background removed, cropped) into sprite assets.</summary>
        public static string BuildPlaceholderSprites()
        {
            ensureFolder(k_spritesFolder);
            var _log = new System.Text.StringBuilder();
            Texture2D _ollie = loadPng("Docs/Referencias/ollie.png");
            Texture2D _enemies = loadPng("Docs/Referencias/enemigos_nutria_capibara.png");
            if (_ollie == null || _enemies == null) return "Missing Docs/Referencias/ollie.png or enemigos_nutria_capibara.png";

            int _half = _enemies.width / 2;
            Texture2D _ollieCut = cutOut(_ollie, 0, _ollie.width), _otterCut = cutOut(_enemies, 0, _half), _capyCut = cutOut(_enemies, _half, _enemies.width);
            _log.AppendLine(saveSprite(_ollieCut, k_spriteOllie));
            _log.AppendLine(saveSprite(_otterCut, k_spriteOtter));
            _log.AppendLine(saveSprite(_capyCut, k_spriteCapybara));
            // copies with an ink outline as thick as the scene's lines, so the characters look part of the same drawing
            Color32 _ink = new Color32(0x1C, 0x14, 0x1A, 0xFF);
            _log.AppendLine(saveSprite(withOutline(_ollieCut, 10, _ink), k_spriteOllieOutlined));
            _log.AppendLine(saveSprite(withOutline(_otterCut, 10, _ink), k_spriteOtterOutlined));
            _log.AppendLine(saveSprite(withOutline(_capyCut, 10, _ink), k_spriteCapybaraOutlined));
            return _log.ToString();
        }

        /// <summary>Adds a solid outline of _radius pixels around the opaque pixels.</summary>
        static Texture2D withOutline(Texture2D _src, int _radius, Color32 _ink)
        {
            int _sw = _src.width, _sh = _src.height, _w = _sw + _radius * 2, _h = _sh + _radius * 2;
            Color32[] _in = _src.GetPixels32();
            var _out = new Color32[_w * _h];
            int _r2 = _radius * _radius;
            for (int y = 0; y < _h; y++)
                for (int x = 0; x < _w; x++)
                {
                    bool _hit = false;
                    for (int dy = -_radius; dy <= _radius && !_hit; dy++)
                        for (int dx = -_radius; dx <= _radius && !_hit; dx++)
                        {
                            if (dx * dx + dy * dy > _r2) continue;
                            int sx = x - _radius + dx, sy = y - _radius + dy;
                            if (sx >= 0 && sy >= 0 && sx < _sw && sy < _sh && _in[sy * _sw + sx].a > 100) _hit = true;
                        }
                    _out[y * _w + x] = _hit ? _ink : new Color32(0, 0, 0, 0);
                }
            for (int y = 0; y < _sh; y++)
                for (int x = 0; x < _sw; x++)
                {
                    Color32 c = _in[y * _sw + x];
                    if (c.a == 0) continue;
                    int i = (y + _radius) * _w + x + _radius;
                    _out[i] = Color32.Lerp(_out[i], new Color32(c.r, c.g, c.b, 255), c.a / 255f);
                }
            var _tex = new Texture2D(_w, _h, TextureFormat.RGBA32, false);
            _tex.SetPixels32(_out);
            _tex.Apply();
            return _tex;
        }

        // =====================================================================
        // Paper grain (film grain texture for the pencil-and-paper look)
        // =====================================================================

        /// <summary>A tileable gray paper texture (soft noise plus fibers) used as custom film grain.</summary>
        public static string BuildPaperGrain()
        {
            ensureFolder(Path.GetDirectoryName(k_paperGrain).Replace('\\', '/'));
            const int n = 256;
            var _rnd = new System.Random(7);
            var _v = new float[n * n];
            for (int i = 0; i < _v.Length; i++) _v[i] = (float)_rnd.NextDouble();
            var _b = new float[n * n];
            for (int pass = 0; pass < 2; pass++)
            {
                for (int y = 0; y < n; y++)
                    for (int x = 0; x < n; x++)
                    {
                        float _sum = 0f;
                        for (int dy = -1; dy <= 1; dy++)
                            for (int dx = -1; dx <= 1; dx++)
                                _sum += _v[((y + dy + n) % n) * n + (x + dx + n) % n];
                        _b[y * n + x] = _sum / 9f;
                    }
                System.Array.Copy(_b, _v, _v.Length);
            }
            var _fiber = new float[n * n];
            for (int f = 0; f < 140; f++)
            {
                float _x = (float)_rnd.NextDouble() * n, _y = (float)_rnd.NextDouble() * n;
                float _a = (float)(_rnd.NextDouble() * System.Math.PI * 2), _len = 6f + (float)_rnd.NextDouble() * 18f;
                float _tone = _rnd.NextDouble() < 0.5 ? -0.12f : 0.1f;
                for (float t = 0; t < _len; t += 0.5f)
                {
                    int px = ((int)(_x + Mathf.Cos(_a) * t) % n + n) % n, py = ((int)(_y + Mathf.Sin(_a) * t) % n + n) % n;
                    _fiber[py * n + px] = _tone;
                }
            }
            var _px = new Color32[n * n];
            for (int i = 0; i < _px.Length; i++)
            {
                float _g = Mathf.Clamp01(0.5f + (_v[i] - 0.5f) * 1.6f + _fiber[i]);
                byte _c = (byte)(_g * 255f);
                _px[i] = new Color32(_c, _c, _c, 255);
            }
            var _tex = new Texture2D(n, n, TextureFormat.RGBA32, false);
            _tex.SetPixels32(_px);
            _tex.Apply();
            File.WriteAllBytes(k_paperGrain, _tex.EncodeToPNG());
            AssetDatabase.ImportAsset(k_paperGrain);
            var _importer = (TextureImporter)AssetImporter.GetAtPath(k_paperGrain);
            _importer.textureType = TextureImporterType.Default;
            _importer.sRGBTexture = false;
            _importer.wrapMode = TextureWrapMode.Repeat;
            _importer.mipmapEnabled = false;
            _importer.filterMode = FilterMode.Bilinear;
            _importer.SaveAndReimport();
            return k_paperGrain;
        }

        /// <summary>
        /// Floor pool of a lamp, drawn like a cartoon light: a solid core, a softer ring and a thin fringe
        /// (stepped, not a smooth gradient), white so each pool takes its lamp's color.
        /// </summary>
        public static string BuildLightPoolTexture()
        {
            ensureFolder(Path.GetDirectoryName(k_lightPool).Replace('\\', '/'));
            const int n = 128;
            var _px = new Color32[n * n];
            for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                {
                    float _dx = (x + 0.5f) / n * 2f - 1f, _dy = (y + 0.5f) / n * 2f - 1f;
                    float _r = Mathf.Sqrt(_dx * _dx + _dy * _dy);
                    float _a = _r < 0.55f ? 1f : _r < 0.8f ? 0.55f : _r < 0.96f ? 0.22f : 0f;
                    // one pixel of anti-aliasing at each step
                    float _edge = 1.5f / n;
                    if (Mathf.Abs(_r - 0.55f) < _edge) _a = 0.78f;
                    else if (Mathf.Abs(_r - 0.8f) < _edge) _a = 0.38f;
                    else if (Mathf.Abs(_r - 0.96f) < _edge) _a = 0.11f;
                    _px[y * n + x] = new Color32(255, 255, 255, (byte)(_a * 255f));
                }
            var _tex = new Texture2D(n, n, TextureFormat.RGBA32, false);
            _tex.SetPixels32(_px);
            _tex.Apply();
            File.WriteAllBytes(k_lightPool, _tex.EncodeToPNG());
            AssetDatabase.ImportAsset(k_lightPool);
            var _importer = (TextureImporter)AssetImporter.GetAtPath(k_lightPool);
            _importer.textureType = TextureImporterType.Default;
            _importer.alphaIsTransparency = true;
            _importer.wrapMode = TextureWrapMode.Clamp;
            _importer.mipmapEnabled = true;
            _importer.SaveAndReimport();
            return k_lightPool;
        }

        /// <summary>
        /// Walnut plank floor, 1 m per tile: 4 planks with a dark seam, a tone per plank, a few staggered end joints
        /// and soft grain streaks. Painted flat, like the concept art.
        /// </summary>
        /// <summary>
        /// Light cookie of a 4-pane window for the sun spots: white panes, black mullions and frame, the panes inside
        /// the spot's inscribed circle (|x| <= 0.84, |y| <= 0.546 in -1..1) so the cone never cuts them.
        /// </summary>
        public static string BuildWindowCookie()
        {
            ensureFolder("Assets/LastFrame/Art/Textures");
            const int n = 256;
            var _px = new Color32[n * n];
            for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                {
                    float u = (x + 0.5f) / n * 2f - 1f, v = (y + 0.5f) / n * 2f - 1f;
                    bool _in = Mathf.Abs(u) <= 0.84f && Mathf.Abs(v) <= 0.546f;
                    bool _mullion = Mathf.Abs(u) < 0.042f || Mathf.Abs(v) < 0.042f;
                    byte c = (byte)(_in && !_mullion ? 255 : 0);
                    _px[y * n + x] = new Color32(c, c, c, c);
                }
            var _tex = new Texture2D(n, n, TextureFormat.RGBA32, false);
            _tex.SetPixels32(_px);
            _tex.Apply();
            File.WriteAllBytes(k_windowCookie, _tex.EncodeToPNG());
            AssetDatabase.ImportAsset(k_windowCookie);
            var _importer = (TextureImporter)AssetImporter.GetAtPath(k_windowCookie);
            _importer.textureType = TextureImporterType.Cookie;
            _importer.wrapMode = TextureWrapMode.Clamp;
            _importer.mipmapEnabled = false;
            _importer.alphaSource = TextureImporterAlphaSource.FromInput;
            _importer.SaveAndReimport();
            return k_windowCookie;
        }

        public static string BuildWoodPlanks()
        {
            ensureFolder("Assets/LastFrame/Art/Textures");
            const int n = 256, planks = 4, pw = n / planks;
            var _rnd = new System.Random(11);
            Color _base = new Color32(0x6E, 0x4E, 0x40, 0xFF), _seam = new Color32(0x2E, 0x22, 0x22, 0xFF);
            var _tone = new float[planks];
            var _joint = new int[planks];
            for (int p = 0; p < planks; p++) { _tone[p] = 0.88f + (float)_rnd.NextDouble() * 0.22f; _joint[p] = _rnd.Next(0, n); }
            var _grain = new float[n];
            for (int y = 0; y < n; y++) _grain[y] = (float)_rnd.NextDouble();
            var _px = new Color32[n * n];
            for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                {
                    int p = x / pw, lx = x % pw;
                    Color c = _base * _tone[p];
                    float g = Mathf.Sin((x * 0.9f + _grain[(y / 9) % n] * 6f) * 0.35f) * 0.04f;
                    c *= 1f + g;
                    if (lx < 3 || Mathf.Abs(y - _joint[p]) < 2) c = _seam;
                    c.a = 1f;
                    _px[y * n + x] = c;
                }
            var _tex = new Texture2D(n, n, TextureFormat.RGBA32, false);
            _tex.SetPixels32(_px);
            _tex.Apply();
            File.WriteAllBytes(k_woodPlanks, _tex.EncodeToPNG());
            AssetDatabase.ImportAsset(k_woodPlanks);
            var _importer = (TextureImporter)AssetImporter.GetAtPath(k_woodPlanks);
            _importer.wrapMode = TextureWrapMode.Repeat;
            _importer.mipmapEnabled = true;
            _importer.SaveAndReimport();
            return k_woodPlanks;
        }

        static Texture2D loadPng(string _path)
        {
            if (!File.Exists(_path)) return null;
            var _tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            _tex.LoadImage(File.ReadAllBytes(_path));
            return _tex;
        }

        /// <summary>
        /// Flood-fills the flat background from the borders of the column range [_x0, _x1) to transparent,
        /// softens the edge by one pixel and crops to the remaining pixels.
        /// </summary>
        static Texture2D cutOut(Texture2D _src, int _x0, int _x1)
        {
            int _w = _x1 - _x0, _h = _src.height;
            Color32[] _all = _src.GetPixels32();
            var _px = new Color32[_w * _h];
            for (int y = 0; y < _h; y++)
                for (int x = 0; x < _w; x++)
                    _px[y * _w + x] = _all[y * _src.width + x + _x0];

            Color32 _bg = _px[0];
            bool isBg(Color32 c) => c.a < 20 || (Mathf.Abs(c.r - _bg.r) + Mathf.Abs(c.g - _bg.g) + Mathf.Abs(c.b - _bg.b)) < 60;

            var _clear = new bool[_px.Length];
            var _stack = new Stack<int>();
            for (int x = 0; x < _w; x++) { _stack.Push(x); _stack.Push((_h - 1) * _w + x); }
            for (int y = 0; y < _h; y++) { _stack.Push(y * _w); _stack.Push(y * _w + _w - 1); }
            while (_stack.Count > 0)
            {
                int i = _stack.Pop();
                if (_clear[i] || !isBg(_px[i])) continue;
                _clear[i] = true;
                int x = i % _w, y = i / _w;
                if (x > 0) _stack.Push(i - 1);
                if (x < _w - 1) _stack.Push(i + 1);
                if (y > 0) _stack.Push(i - _w);
                if (y < _h - 1) _stack.Push(i + _w);
            }

            int _minX = _w, _minY = _h, _maxX = -1, _maxY = -1;
            for (int i = 0; i < _px.Length; i++)
            {
                int x = i % _w, y = i / _w;
                if (_clear[i]) { _px[i].a = 0; continue; }
                // one-pixel soft edge next to the removed background
                bool _edge = (x > 0 && _clear[i - 1]) || (x < _w - 1 && _clear[i + 1]) || (y > 0 && _clear[i - _w]) || (y < _h - 1 && _clear[i + _w]);
                if (_edge) _px[i].a = (byte)(_px[i].a / 2);
                _minX = Mathf.Min(_minX, x); _maxX = Mathf.Max(_maxX, x);
                _minY = Mathf.Min(_minY, y); _maxY = Mathf.Max(_maxY, y);
            }

            int _cw = _maxX - _minX + 1, _ch = _maxY - _minY + 1;
            var _out = new Texture2D(_cw, _ch, TextureFormat.RGBA32, false);
            var _crop = new Color32[_cw * _ch];
            for (int y = 0; y < _ch; y++)
                for (int x = 0; x < _cw; x++)
                    _crop[y * _cw + x] = _px[(y + _minY) * _w + x + _minX];
            _out.SetPixels32(_crop);
            _out.Apply();
            return _out;
        }

        static string saveSprite(Texture2D _tex, string _path)
        {
            File.WriteAllBytes(_path, _tex.EncodeToPNG());
            AssetDatabase.ImportAsset(_path);
            var _importer = (TextureImporter)AssetImporter.GetAtPath(_path);
            _importer.textureType = TextureImporterType.Sprite;
            _importer.spriteImportMode = SpriteImportMode.Single;
            _importer.spritePivot = new Vector2(0.5f, 0f);
            var _settings = new TextureImporterSettings();
            _importer.ReadTextureSettings(_settings);
            _settings.spriteAlignment = (int)SpriteAlignment.BottomCenter;
            _importer.SetTextureSettings(_settings);
            _importer.spritePixelsPerUnit = _tex.height; // 1 unit tall; the scene scales it to the character height
            _importer.alphaIsTransparency = true;
            _importer.mipmapEnabled = true;
            _importer.SaveAndReimport();
            return $"{_path} ({_tex.width}x{_tex.height})";
        }

        static void ensureFolder(string _path)
        {
            string[] _parts = _path.Split('/');
            string _current = _parts[0];
            for (int i = 1; i < _parts.Length; i++)
            {
                string _next = _current + "/" + _parts[i];
                if (!AssetDatabase.IsValidFolder(_next)) AssetDatabase.CreateFolder(_current, _parts[i]);
                _current = _next;
            }
        }
    }
}
