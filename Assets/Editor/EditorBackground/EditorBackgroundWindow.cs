using UnityEditor;
using UnityEngine;

namespace EditorBackground
{
    /// <summary>
    /// 設定UI
    /// </summary>
    public class EditorBackgroundWindow : EditorWindow
    {
        private Texture2D selectedTexture;
        private bool enabled;
        private float opacity;
        private ScaleMode scaleMode;
        private Color tintColor;
        private bool globalMode;
        private bool overlayEnabled;
        private Color overlayColor;
        private bool borderEnabled;
        private Color borderColor;
        private float borderWidth;

        private Vector2 scrollPosition;

        [MenuItem("Tools/Editor Background/Settings")]
        public static void ShowWindow()
        {
            var window = GetWindow<EditorBackgroundWindow>("Editor Background");
            window.minSize = new Vector2(350, 400);
            window.LoadCurrentSettings();
        }

        private void OnEnable()
        {
            LoadCurrentSettings();
        }

        private void LoadCurrentSettings()
        {
            EditorBackgroundSettings.Load();
            enabled = EditorBackgroundSettings.Enabled;
            opacity = EditorBackgroundSettings.Opacity;
            scaleMode = EditorBackgroundSettings.ScaleMode;
            tintColor = EditorBackgroundSettings.TintColor;
            globalMode = EditorBackgroundSettings.GlobalMode;
            overlayEnabled = EditorBackgroundSettings.OverlayEnabled;
            overlayColor = EditorBackgroundSettings.OverlayColor;
            borderEnabled = EditorBackgroundSettings.BorderEnabled;
            borderColor = EditorBackgroundSettings.BorderColor;
            borderWidth = EditorBackgroundSettings.BorderWidth;

            if (!string.IsNullOrEmpty(EditorBackgroundSettings.ImagePath))
            {
                selectedTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(EditorBackgroundSettings.ImagePath);
            }
            else
            {
                selectedTexture = null;
            }
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Editor Background Settings", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);

            DrawSeparator();
            EditorGUILayout.Space(5);

            // Enabled Toggle
            var newEnabled = EditorGUILayout.Toggle("Enable Background", enabled);
            if (newEnabled != enabled)
            {
                enabled = newEnabled;
                EditorBackgroundSettings.Enabled = enabled;
            }

            EditorGUILayout.Space(15);

            // === Background Image Section ===
            EditorGUILayout.LabelField("Background Image", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            // Image Selection
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Image");
            var newTexture = (Texture2D)EditorGUILayout.ObjectField(selectedTexture, typeof(Texture2D), false);
            EditorGUILayout.EndHorizontal();

            if (newTexture != selectedTexture)
            {
                selectedTexture = newTexture;
                if (selectedTexture != null)
                {
                    EditorBackgroundSettings.ImagePath = AssetDatabase.GetAssetPath(selectedTexture);
                }
                else
                {
                    EditorBackgroundSettings.ImagePath = "";
                }
            }

            EditorGUILayout.Space(5);

            // Opacity Slider
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Opacity");
            var newOpacity = EditorGUILayout.Slider(opacity, 0f, 1f);
            EditorGUILayout.EndHorizontal();

            if (!Mathf.Approximately(newOpacity, opacity))
            {
                opacity = newOpacity;
                EditorBackgroundSettings.Opacity = opacity;
            }

            EditorGUILayout.Space(5);

            // Scale Mode
            var newScaleMode = (ScaleMode)EditorGUILayout.EnumPopup("Scale Mode", scaleMode);
            if (newScaleMode != scaleMode)
            {
                scaleMode = newScaleMode;
                EditorBackgroundSettings.ScaleMode = scaleMode;
            }

            EditorGUILayout.Space(5);

            // Tint Color
            var newTintColor = EditorGUILayout.ColorField("Tint Color", tintColor);
            if (newTintColor != tintColor)
            {
                tintColor = newTintColor;
                EditorBackgroundSettings.TintColor = tintColor;
            }

            EditorGUILayout.Space(5);

            // Global Mode Toggle
            var newGlobalMode = EditorGUILayout.Toggle(
                new GUIContent("Global Mode", "ON: 全ウィンドウで1枚の背景を共有\nOFF: 各ウィンドウに個別の背景"),
                globalMode);
            if (newGlobalMode != globalMode)
            {
                globalMode = newGlobalMode;
                EditorBackgroundSettings.GlobalMode = globalMode;
            }

            EditorGUILayout.Space(15);
            DrawSeparator();
            EditorGUILayout.Space(10);

            // === Color Overlay Section ===
            EditorGUILayout.LabelField("Color Overlay", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            // Overlay Enabled
            var newOverlayEnabled = EditorGUILayout.Toggle(
                new GUIContent("Enable Overlay", "ウィンドウ全体に半透明のカラーを重ねる"),
                overlayEnabled);
            if (newOverlayEnabled != overlayEnabled)
            {
                overlayEnabled = newOverlayEnabled;
                EditorBackgroundSettings.OverlayEnabled = overlayEnabled;
            }

            using (new EditorGUI.DisabledGroupScope(!overlayEnabled))
            {
                EditorGUILayout.Space(5);

                // Overlay Color
                var newOverlayColor = EditorGUILayout.ColorField(
                    new GUIContent("Overlay Color", "オーバーレイの色（アルファ値で透明度を調整）"),
                    overlayColor, true, true, false);
                if (newOverlayColor != overlayColor)
                {
                    overlayColor = newOverlayColor;
                    EditorBackgroundSettings.OverlayColor = overlayColor;
                }
            }

            EditorGUILayout.Space(15);
            DrawSeparator();
            EditorGUILayout.Space(10);

            // === Border Section ===
            EditorGUILayout.LabelField("Window Border", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            // Border Enabled
            var newBorderEnabled = EditorGUILayout.Toggle(
                new GUIContent("Enable Border", "ウィンドウの縁にカラーボーダーを追加"),
                borderEnabled);
            if (newBorderEnabled != borderEnabled)
            {
                borderEnabled = newBorderEnabled;
                EditorBackgroundSettings.BorderEnabled = borderEnabled;
            }

            using (new EditorGUI.DisabledGroupScope(!borderEnabled))
            {
                EditorGUILayout.Space(5);

                // Border Color
                var newBorderColor = EditorGUILayout.ColorField(
                    new GUIContent("Border Color", "ボーダーの色"),
                    borderColor, true, true, false);
                if (newBorderColor != borderColor)
                {
                    borderColor = newBorderColor;
                    EditorBackgroundSettings.BorderColor = borderColor;
                }

                EditorGUILayout.Space(5);

                // Border Width
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel(new GUIContent("Border Width", "ボーダーの太さ (1-10px)"));
                var newBorderWidth = EditorGUILayout.Slider(borderWidth, 1f, 10f);
                EditorGUILayout.EndHorizontal();

                if (!Mathf.Approximately(newBorderWidth, borderWidth))
                {
                    borderWidth = newBorderWidth;
                    EditorBackgroundSettings.BorderWidth = borderWidth;
                }
            }

            EditorGUILayout.Space(20);
            DrawSeparator();
            EditorGUILayout.Space(10);

            // Buttons
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Reset to Default", GUILayout.Width(120)))
            {
                if (EditorUtility.DisplayDialog("Reset Settings",
                    "Are you sure you want to reset all settings to default?", "Yes", "No"))
                {
                    EditorBackgroundSettings.ResetToDefault();
                    LoadCurrentSettings();
                }
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);

            // Preview Info
            if (selectedTexture != null)
            {
                DrawSeparator();
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Image Info", EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"Name: {selectedTexture.name}");
                EditorGUILayout.LabelField($"Size: {selectedTexture.width} x {selectedTexture.height}");
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.EndScrollView();
        }

        private void DrawSeparator()
        {
            var rect = EditorGUILayout.GetControlRect(false, 1);
            rect.height = 1;
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 0.5f));
        }
    }
}
