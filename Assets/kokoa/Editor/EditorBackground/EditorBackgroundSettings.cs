using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace EditorBackground
{
    /// <summary>
    /// 背景のスケールモード
    /// </summary>
    public enum BackgroundScaleMode
    {
        ScaleAndCrop,   // 画面を覆うようにスケール（はみ出し部分はクリップ）
        ScaleToFit,     // 画面に収まるようにスケール（余白あり）
        StretchToFill,  // 画面に合わせて引き伸ばし
        Tile,           // タイル状に繰り返し
        Corner          // 角基準で1枚配置
    }

    public enum CornerPosition
    {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }

    /// <summary>
    /// JSON保存用のデータクラス
    /// </summary>
    [Serializable]
    public class EditorBackgroundSettingsData
    {
        public bool enabled = true;
        public string imagePath = "";
        public float opacity = 0.08f;
        public int scaleMode = 0;
        public float tileScale = 1f;
        public int cornerPosition = 3;
        public float offsetX = 0f;
        public float offsetY = 0f;
        public string tintColor = "FFFFFFFF";
        public bool globalMode = true;
        public bool overlayEnabled = false;
        public string overlayColor = "334CCC1A";
        public bool borderEnabled = false;
        public string borderColor = "6699FF80";
        public float borderWidth = 2f;
        public int language = 0;
    }

    /// <summary>
    /// 設定データを管理する静的クラス
    /// </summary>
    public static class EditorBackgroundSettings
    {
        private static readonly string SettingsPath = Path.Combine("ProjectSettings", "EditorBackgroundSettings.json");

        public enum Language { Japanese, English }

        private static bool _enabled = true;
        private static string _imagePath = "";
        private static float _opacity = 0.08f;
        private static BackgroundScaleMode _scaleMode = BackgroundScaleMode.ScaleAndCrop;
        private static float _tileScale = 1f;
        private static CornerPosition _cornerPosition = CornerPosition.BottomRight;
        private static float _offsetX = 0f;
        private static float _offsetY = 0f;
        private static Color _tintColor = Color.white;
        private static bool _globalMode = true;
        private static bool _overlayEnabled = false;
        private static Color _overlayColor = new Color(0.2f, 0.4f, 0.8f, 0.1f);
        private static bool _borderEnabled = false;
        private static Color _borderColor = new Color(0.4f, 0.6f, 1f, 0.5f);
        private static float _borderWidth = 2f;
        private static Language _language = Language.Japanese;

        public static event Action OnSettingsChanged;

        // テクスチャキャッシュ
        private static Texture2D _cachedTexture;
        private static string _cachedTexturePath;

        public static bool Enabled
        {
            get => _enabled;
            set
            {
                if (_enabled != value)
                {
                    _enabled = value;
                    Save();
                    NotifySettingsChanged();
                }
            }
        }

        public static string ImagePath
        {
            get => _imagePath;
            set
            {
                if (_imagePath != value)
                {
                    _imagePath = value ?? "";
                    // キャッシュをクリア
                    ClearTextureCache();
                    Save();
                    NotifySettingsChanged();
                }
            }
        }

        public static float Opacity
        {
            get => _opacity;
            set
            {
                value = Mathf.Clamp01(value);
                if (!Mathf.Approximately(_opacity, value))
                {
                    _opacity = value;
                    Save();
                    NotifySettingsChanged();
                }
            }
        }

        public static BackgroundScaleMode ScaleMode
        {
            get => _scaleMode;
            set
            {
                if (_scaleMode != value)
                {
                    _scaleMode = value;
                    Save();
                    NotifySettingsChanged();
                }
            }
        }

        public static float TileScale
        {
            get => _tileScale;
            set
            {
                value = Mathf.Clamp(value, 0.01f, 5f);
                if (!Mathf.Approximately(_tileScale, value))
                {
                    _tileScale = value;
                    Save();
                    NotifySettingsChanged();
                }
            }
        }

        public static CornerPosition CornerPosition
        {
            get => _cornerPosition;
            set
            {
                if (_cornerPosition != value)
                {
                    _cornerPosition = value;
                    Save();
                    NotifySettingsChanged();
                }
            }
        }

        public static float OffsetX
        {
            get => _offsetX;
            set
            {
                value = Mathf.Clamp(value, -500f, 500f);
                if (!Mathf.Approximately(_offsetX, value))
                {
                    _offsetX = value;
                    Save();
                    NotifySettingsChanged();
                }
            }
        }

        public static float OffsetY
        {
            get => _offsetY;
            set
            {
                value = Mathf.Clamp(value, -500f, 500f);
                if (!Mathf.Approximately(_offsetY, value))
                {
                    _offsetY = value;
                    Save();
                    NotifySettingsChanged();
                }
            }
        }

        public static Color TintColor
        {
            get => _tintColor;
            set
            {
                if (_tintColor != value)
                {
                    _tintColor = value;
                    Save();
                    NotifySettingsChanged();
                }
            }
        }

        public static bool GlobalMode
        {
            get => _globalMode;
            set
            {
                if (_globalMode != value)
                {
                    _globalMode = value;
                    Save();
                    NotifySettingsChanged();
                }
            }
        }

        public static bool OverlayEnabled
        {
            get => _overlayEnabled;
            set
            {
                if (_overlayEnabled != value)
                {
                    _overlayEnabled = value;
                    Save();
                    NotifySettingsChanged();
                }
            }
        }

        public static Color OverlayColor
        {
            get => _overlayColor;
            set
            {
                if (_overlayColor != value)
                {
                    _overlayColor = value;
                    Save();
                    NotifySettingsChanged();
                }
            }
        }

        public static bool BorderEnabled
        {
            get => _borderEnabled;
            set
            {
                if (_borderEnabled != value)
                {
                    _borderEnabled = value;
                    Save();
                    NotifySettingsChanged();
                }
            }
        }

        public static Color BorderColor
        {
            get => _borderColor;
            set
            {
                if (_borderColor != value)
                {
                    _borderColor = value;
                    Save();
                    NotifySettingsChanged();
                }
            }
        }

        public static float BorderWidth
        {
            get => _borderWidth;
            set
            {
                value = Mathf.Clamp(value, 1f, 10f);
                if (!Mathf.Approximately(_borderWidth, value))
                {
                    _borderWidth = value;
                    Save();
                    NotifySettingsChanged();
                }
            }
        }

        public static Language CurrentLanguage
        {
            get => _language;
            set
            {
                if (_language != value)
                {
                    _language = value;
                    Save();
                }
            }
        }

        public static Texture2D GetTexture()
        {
            if (string.IsNullOrEmpty(_imagePath))
                return null;

            // キャッシュがあればそれを返す
            if (_cachedTexture != null && _cachedTexturePath == _imagePath)
                return _cachedTexture;

            Texture2D texture = null;

            // 絶対パスの場合（外部ファイル）
            if (Path.IsPathRooted(_imagePath))
            {
                texture = LoadExternalTexture(_imagePath);
            }
            // 相対パスの場合（Unityアセット）
            else
            {
                texture = AssetDatabase.LoadAssetAtPath<Texture2D>(_imagePath);
            }

            if (texture == null)
            {
                return null;
            }

            // キャッシュに保存
            _cachedTexture = texture;
            _cachedTexturePath = _imagePath;

            return texture;
        }

        /// <summary>
        /// 外部ファイルからテクスチャを読み込む
        /// </summary>
        private static Texture2D LoadExternalTexture(string absolutePath)
        {
            if (!File.Exists(absolutePath))
                return null;

            try
            {
                var bytes = File.ReadAllBytes(absolutePath);
                var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                texture.filterMode = FilterMode.Bilinear;
                texture.wrapMode = TextureWrapMode.Clamp;

                if (texture.LoadImage(bytes))
                {
                    return texture;
                }
                else
                {
                    UnityEngine.Object.DestroyImmediate(texture);
                    return null;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[EditorBackground] Failed to load image: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// テクスチャキャッシュをクリア
        /// </summary>
        public static void ClearTextureCache()
        {
            if (_cachedTexture != null && Path.IsPathRooted(_cachedTexturePath))
            {
                // 外部ファイルから作成したテクスチャは破棄
                UnityEngine.Object.DestroyImmediate(_cachedTexture);
            }
            _cachedTexture = null;
            _cachedTexturePath = null;
        }

        public static void Save()
        {
            try
            {
                var data = new EditorBackgroundSettingsData
                {
                    enabled = _enabled,
                    imagePath = _imagePath,
                    opacity = _opacity,
                    scaleMode = (int)_scaleMode,
                    tileScale = _tileScale,
                    cornerPosition = (int)_cornerPosition,
                    offsetX = _offsetX,
                    offsetY = _offsetY,
                    tintColor = ColorUtility.ToHtmlStringRGBA(_tintColor),
                    globalMode = _globalMode,
                    overlayEnabled = _overlayEnabled,
                    overlayColor = ColorUtility.ToHtmlStringRGBA(_overlayColor),
                    borderEnabled = _borderEnabled,
                    borderColor = ColorUtility.ToHtmlStringRGBA(_borderColor),
                    borderWidth = _borderWidth,
                    language = (int)_language
                };

                var json = JsonUtility.ToJson(data, true);
                File.WriteAllText(SettingsPath, json);
            }
            catch (Exception e)
            {
                Debug.LogError($"[EditorBackground] Failed to save settings: {e.Message}");
            }
        }

        public static void Load()
        {
            try
            {
                if (File.Exists(SettingsPath))
                {
                    var json = File.ReadAllText(SettingsPath);
                    var data = JsonUtility.FromJson<EditorBackgroundSettingsData>(json);

                    _enabled = data.enabled;
                    _imagePath = data.imagePath ?? "";
                    _opacity = data.opacity;
                    _scaleMode = (BackgroundScaleMode)data.scaleMode;
                    _tileScale = data.tileScale;
                    _cornerPosition = (CornerPosition)data.cornerPosition;
                    _offsetX = data.offsetX;
                    _offsetY = data.offsetY;
                    _globalMode = data.globalMode;
                    _overlayEnabled = data.overlayEnabled;
                    _borderEnabled = data.borderEnabled;
                    _borderWidth = data.borderWidth;
                    _language = (Language)data.language;

                    // 色の読み込み
                    if (ColorUtility.TryParseHtmlString("#" + data.tintColor, out var tint))
                        _tintColor = tint;
                    if (ColorUtility.TryParseHtmlString("#" + data.overlayColor, out var overlay))
                        _overlayColor = overlay;
                    if (ColorUtility.TryParseHtmlString("#" + data.borderColor, out var border))
                        _borderColor = border;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[EditorBackground] Failed to load settings, using defaults: {e.Message}");
                ResetToDefault();
            }
        }

        public static void ResetToDefault()
        {
            _enabled = true;
            _imagePath = "";
            _opacity = 0.08f;
            _scaleMode = BackgroundScaleMode.ScaleAndCrop;
            _tileScale = 1f;
            _cornerPosition = CornerPosition.BottomRight;
            _offsetX = 0f;
            _offsetY = 0f;
            _tintColor = Color.white;
            _globalMode = true;
            _overlayEnabled = false;
            _overlayColor = new Color(0.2f, 0.4f, 0.8f, 0.1f);
            _borderEnabled = false;
            _borderColor = new Color(0.4f, 0.6f, 1f, 0.5f);
            _borderWidth = 2f;
            _language = Language.Japanese;
            ClearTextureCache();
            Save();
            NotifySettingsChanged();
        }

        private static void NotifySettingsChanged()
        {
            OnSettingsChanged?.Invoke();
        }
    }
}
