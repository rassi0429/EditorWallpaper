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

        private static bool _enabled = true;
        private static string _imagePath = "";
        private static float _opacity = 0.08f;
        private static ScaleMode _scaleMode = ScaleMode.ScaleAndCrop;
        private static Color _tintColor = Color.white;
        private static bool _globalMode = true;

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
        }

        public static void Load()
        {
            _enabled = EditorPrefs.GetBool(KEY_ENABLED, true);
            _imagePath = EditorPrefs.GetString(KEY_IMAGE_PATH, "");
            _opacity = EditorPrefs.GetFloat(KEY_OPACITY, 0.08f);
            _scaleMode = (ScaleMode)EditorPrefs.GetInt(KEY_SCALE_MODE, (int)ScaleMode.ScaleAndCrop);
            _globalMode = EditorPrefs.GetBool(KEY_GLOBAL_MODE, true);

            var colorString = EditorPrefs.GetString(KEY_TINT_COLOR, "FFFFFFFF");
            if (ColorUtility.TryParseHtmlString("#" + colorString, out var color))
            {
                _tintColor = color;
            }
            else
            {
                _tintColor = Color.white;
            }
        }

        public static void ResetToDefault()
        {
            _enabled = true;
            _imagePath = "";
            _opacity = 0.08f;
            _scaleMode = ScaleMode.ScaleAndCrop;
            _tintColor = Color.white;
            _globalMode = true;
            Save();
            NotifySettingsChanged();
        }

        private static void NotifySettingsChanged()
        {
            OnSettingsChanged?.Invoke();
        }
    }
}
