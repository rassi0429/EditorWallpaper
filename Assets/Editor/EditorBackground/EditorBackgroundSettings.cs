using System;
using UnityEditor;
using UnityEngine;

namespace EditorBackground
{
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

        private static bool _enabled = true;
        private static string _imagePath = "";
        private static float _opacity = 0.08f;
        private static ScaleMode _scaleMode = ScaleMode.ScaleAndCrop;
        private static Color _tintColor = Color.white;
        private static bool _globalMode = true;
        private static bool _overlayEnabled = false;
        private static Color _overlayColor = new Color(0.2f, 0.4f, 0.8f, 0.1f);
        private static bool _borderEnabled = false;
        private static Color _borderColor = new Color(0.4f, 0.6f, 1f, 0.5f);
        private static float _borderWidth = 2f;

        public static event Action OnSettingsChanged;

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

        public static ScaleMode ScaleMode
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

        public static Texture2D GetTexture()
        {
            if (string.IsNullOrEmpty(_imagePath))
                return null;

            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(_imagePath);
            if (texture == null)
            {
                Debug.LogWarning($"[EditorBackground] Image not found: {_imagePath}");
            }
            return texture;
        }

        public static void Save()
        {
            EditorPrefs.SetBool(KEY_ENABLED, _enabled);
            EditorPrefs.SetString(KEY_IMAGE_PATH, _imagePath);
            EditorPrefs.SetFloat(KEY_OPACITY, _opacity);
            EditorPrefs.SetInt(KEY_SCALE_MODE, (int)_scaleMode);
            EditorPrefs.SetString(KEY_TINT_COLOR, ColorUtility.ToHtmlStringRGBA(_tintColor));
            EditorPrefs.SetBool(KEY_GLOBAL_MODE, _globalMode);
            EditorPrefs.SetBool(KEY_OVERLAY_ENABLED, _overlayEnabled);
            EditorPrefs.SetString(KEY_OVERLAY_COLOR, ColorUtility.ToHtmlStringRGBA(_overlayColor));
            EditorPrefs.SetBool(KEY_BORDER_ENABLED, _borderEnabled);
            EditorPrefs.SetString(KEY_BORDER_COLOR, ColorUtility.ToHtmlStringRGBA(_borderColor));
            EditorPrefs.SetFloat(KEY_BORDER_WIDTH, _borderWidth);
        }

        public static void Load()
        {
            _enabled = EditorPrefs.GetBool(KEY_ENABLED, true);
            _imagePath = EditorPrefs.GetString(KEY_IMAGE_PATH, "");
            _opacity = EditorPrefs.GetFloat(KEY_OPACITY, 0.08f);
            _scaleMode = (ScaleMode)EditorPrefs.GetInt(KEY_SCALE_MODE, (int)ScaleMode.ScaleAndCrop);
            _globalMode = EditorPrefs.GetBool(KEY_GLOBAL_MODE, true);
            _overlayEnabled = EditorPrefs.GetBool(KEY_OVERLAY_ENABLED, false);
            _borderEnabled = EditorPrefs.GetBool(KEY_BORDER_ENABLED, false);
            _borderWidth = EditorPrefs.GetFloat(KEY_BORDER_WIDTH, 2f);

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
            _scaleMode = ScaleMode.ScaleAndCrop;
            _tintColor = Color.white;
            _globalMode = true;
            _overlayEnabled = false;
            _overlayColor = new Color(0.2f, 0.4f, 0.8f, 0.1f);
            _borderEnabled = false;
            _borderColor = new Color(0.4f, 0.6f, 1f, 0.5f);
            _borderWidth = 2f;
            Save();
            NotifySettingsChanged();
        }

        private static void NotifySettingsChanged()
        {
            OnSettingsChanged?.Invoke();
        }
    }
}
