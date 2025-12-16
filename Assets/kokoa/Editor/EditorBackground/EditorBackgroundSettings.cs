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
    /// 設定データを管理する静的クラス
    /// </summary>
    public static class EditorBackgroundSettings
    {
        private const string KEY_ENABLED = "EditorBackground_Enabled";
        private const string KEY_IMAGE_PATH = "EditorBackground_ImagePath";
        private const string KEY_OPACITY = "EditorBackground_Opacity";
        private const string KEY_SCALE_MODE = "EditorBackground_ScaleMode";
        private const string KEY_TINT_COLOR = "EditorBackground_TintColor";
        private const string KEY_GLOBAL_MODE = "EditorBackground_GlobalMode";
        private const string KEY_OVERLAY_ENABLED = "EditorBackground_OverlayEnabled";
        private const string KEY_OVERLAY_COLOR = "EditorBackground_OverlayColor";
        private const string KEY_BORDER_ENABLED = "EditorBackground_BorderEnabled";
        private const string KEY_BORDER_COLOR = "EditorBackground_BorderColor";
        private const string KEY_BORDER_WIDTH = "EditorBackground_BorderWidth";
        private const string KEY_LANGUAGE = "EditorBackground_Language";
        private const string KEY_TILE_SCALE = "EditorBackground_TileScale";
        private const string KEY_CORNER_POSITION = "EditorBackground_CornerPosition";

        public enum Language { Japanese, English }

        private static bool _enabled = true;
        private static string _imagePath = "";
        private static float _opacity = 0.08f;
        private static BackgroundScaleMode _scaleMode = BackgroundScaleMode.ScaleAndCrop;
        private static float _tileScale = 1f;
        private static CornerPosition _cornerPosition = CornerPosition.BottomRight;
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
                value = Mathf.Clamp(value, 0.1f, 5f);
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
                Debug.LogWarning($"[EditorBackground] Image not found: {_imagePath}");
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
            EditorPrefs.SetBool(KEY_ENABLED, _enabled);
            EditorPrefs.SetString(KEY_IMAGE_PATH, _imagePath);
            EditorPrefs.SetFloat(KEY_OPACITY, _opacity);
            EditorPrefs.SetInt(KEY_SCALE_MODE, (int)_scaleMode);
            EditorPrefs.SetFloat(KEY_TILE_SCALE, _tileScale);
            EditorPrefs.SetInt(KEY_CORNER_POSITION, (int)_cornerPosition);
            EditorPrefs.SetString(KEY_TINT_COLOR, ColorUtility.ToHtmlStringRGBA(_tintColor));
            EditorPrefs.SetBool(KEY_GLOBAL_MODE, _globalMode);
            EditorPrefs.SetBool(KEY_OVERLAY_ENABLED, _overlayEnabled);
            EditorPrefs.SetString(KEY_OVERLAY_COLOR, ColorUtility.ToHtmlStringRGBA(_overlayColor));
            EditorPrefs.SetBool(KEY_BORDER_ENABLED, _borderEnabled);
            EditorPrefs.SetString(KEY_BORDER_COLOR, ColorUtility.ToHtmlStringRGBA(_borderColor));
            EditorPrefs.SetFloat(KEY_BORDER_WIDTH, _borderWidth);
            EditorPrefs.SetInt(KEY_LANGUAGE, (int)_language);
        }

        public static void Load()
        {
            _enabled = EditorPrefs.GetBool(KEY_ENABLED, true);
            _imagePath = EditorPrefs.GetString(KEY_IMAGE_PATH, "");
            _opacity = EditorPrefs.GetFloat(KEY_OPACITY, 0.08f);
            _scaleMode = (BackgroundScaleMode)EditorPrefs.GetInt(KEY_SCALE_MODE, (int)BackgroundScaleMode.ScaleAndCrop);
            _tileScale = EditorPrefs.GetFloat(KEY_TILE_SCALE, 1f);
            _cornerPosition = (CornerPosition)EditorPrefs.GetInt(KEY_CORNER_POSITION, (int)CornerPosition.BottomRight);
            _globalMode = EditorPrefs.GetBool(KEY_GLOBAL_MODE, true);
            _overlayEnabled = EditorPrefs.GetBool(KEY_OVERLAY_ENABLED, false);
            _borderEnabled = EditorPrefs.GetBool(KEY_BORDER_ENABLED, false);
            _borderWidth = EditorPrefs.GetFloat(KEY_BORDER_WIDTH, 2f);
            _language = (Language)EditorPrefs.GetInt(KEY_LANGUAGE, (int)Language.Japanese);

            _tintColor = LoadColor(KEY_TINT_COLOR, Color.white);
            _overlayColor = LoadColor(KEY_OVERLAY_COLOR, new Color(0.2f, 0.4f, 0.8f, 0.1f));
            _borderColor = LoadColor(KEY_BORDER_COLOR, new Color(0.4f, 0.6f, 1f, 0.5f));
        }

        private static Color LoadColor(string key, Color defaultColor)
        {
            var colorString = EditorPrefs.GetString(key, "");
            if (!string.IsNullOrEmpty(colorString) && ColorUtility.TryParseHtmlString("#" + colorString, out var color))
            {
                return color;
            }
            return defaultColor;
        }

        public static void ResetToDefault()
        {
            _enabled = true;
            _imagePath = "";
            _opacity = 0.08f;
            _scaleMode = BackgroundScaleMode.ScaleAndCrop;
            _tileScale = 1f;
            _cornerPosition = CornerPosition.BottomRight;
            _tintColor = Color.white;
            _globalMode = true;
            _overlayEnabled = false;
            _overlayColor = new Color(0.2f, 0.4f, 0.8f, 0.1f);
            _borderEnabled = false;
            _borderColor = new Color(0.4f, 0.6f, 1f, 0.5f);
            _borderWidth = 2f;
            _language = Language.Japanese;
            Save();
            NotifySettingsChanged();
        }

        private static void NotifySettingsChanged()
        {
            OnSettingsChanged?.Invoke();
        }
    }
}
