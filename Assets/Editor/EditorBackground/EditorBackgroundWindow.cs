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

        [MenuItem("Tools/Editor Background/Settings")]
        public static void ShowWindow()
        {
            var window = GetWindow<EditorBackgroundWindow>("Editor Background");
            window.minSize = new Vector2(350, 280);
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

            EditorGUILayout.Space(10);

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

            EditorGUILayout.Space(10);

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

            EditorGUILayout.Space(10);

            // Scale Mode
            var newScaleMode = (ScaleMode)EditorGUILayout.EnumPopup("Scale Mode", scaleMode);
            if (newScaleMode != scaleMode)
            {
                scaleMode = newScaleMode;
                EditorBackgroundSettings.ScaleMode = scaleMode;
            }

            EditorGUILayout.Space(10);

            // Tint Color
            var newTintColor = EditorGUILayout.ColorField("Tint Color", tintColor);
            if (newTintColor != tintColor)
            {
                tintColor = newTintColor;
                EditorBackgroundSettings.TintColor = tintColor;
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
                EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"Image: {selectedTexture.name}");
                EditorGUILayout.LabelField($"Size: {selectedTexture.width} x {selectedTexture.height}");
            }
        }

        private void DrawSeparator()
        {
            var rect = EditorGUILayout.GetControlRect(false, 1);
            rect.height = 1;
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 0.5f));
        }
    }
}
