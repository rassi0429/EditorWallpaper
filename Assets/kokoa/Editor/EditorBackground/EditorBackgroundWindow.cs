using System.IO;
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
        private BackgroundScaleMode scaleMode;
        private float tileScale;
        private CornerPosition cornerPosition;
        private float offsetX;
        private float offsetY;
        private Color tintColor;
        private bool globalMode;
        private bool overlayEnabled;
        private Color overlayColor;
        private bool borderEnabled;
        private Color borderColor;
        private float borderWidth;

        private Vector2 scrollPosition;

        // UI Styles
        private GUIStyle headerStyle;
        private GUIStyle sectionBoxStyle;
        private GUIStyle languageButtonStyle;
        private GUIStyle languageButtonActiveStyle;
        private bool stylesInitialized;

        [MenuItem("Tools/Editor Background/Settings")]
        public static void ShowWindow()
        {
            var window = GetWindow<EditorBackgroundWindow>("Editor Background");
            window.minSize = new Vector2(380, 500);
            window.LoadCurrentSettings();
        }

        private void OnEnable()
        {
            LoadCurrentSettings();
            stylesInitialized = false;
        }

        private void InitStyles()
        {
            if (stylesInitialized) return;

            headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 14,
                margin = new RectOffset(0, 0, 8, 8)
            };

            sectionBoxStyle = new GUIStyle("box")
            {
                padding = new RectOffset(12, 12, 10, 10),
                margin = new RectOffset(0, 0, 4, 8)
            };

            languageButtonStyle = new GUIStyle(EditorStyles.miniButton)
            {
                fixedWidth = 70,
                fixedHeight = 22
            };

            languageButtonActiveStyle = new GUIStyle(EditorStyles.miniButton)
            {
                fixedWidth = 70,
                fixedHeight = 22,
                fontStyle = FontStyle.Bold
            };

            stylesInitialized = true;
        }

        private void LoadCurrentSettings()
        {
            EditorBackgroundSettings.Load();
            enabled = EditorBackgroundSettings.Enabled;
            opacity = EditorBackgroundSettings.Opacity;
            scaleMode = EditorBackgroundSettings.ScaleMode;
            tileScale = EditorBackgroundSettings.TileScale;
            cornerPosition = EditorBackgroundSettings.CornerPosition;
            offsetX = EditorBackgroundSettings.OffsetX;
            offsetY = EditorBackgroundSettings.OffsetY;
            tintColor = EditorBackgroundSettings.TintColor;
            globalMode = EditorBackgroundSettings.GlobalMode;
            overlayEnabled = EditorBackgroundSettings.OverlayEnabled;
            overlayColor = EditorBackgroundSettings.OverlayColor;
            borderEnabled = EditorBackgroundSettings.BorderEnabled;
            borderColor = EditorBackgroundSettings.BorderColor;
            borderWidth = EditorBackgroundSettings.BorderWidth;

            // GetTexture() は外部ファイル・アセット両方に対応
            selectedTexture = EditorBackgroundSettings.GetTexture();
        }

        private void OnGUI()
        {
            InitStyles();

            // ウィンドウタイトルを更新
            titleContent = new GUIContent(Localization.WindowTitle);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            EditorGUILayout.Space(12);

            // ヘッダー部分（タイトル + 言語切り替え）
            DrawHeader();

            EditorGUILayout.Space(8);

            // メイン有効化トグル
            DrawMainToggle();

            EditorGUILayout.Space(12);

            // 背景画像セクション
            DrawBackgroundImageSection();

            EditorGUILayout.Space(8);

            // カラーオーバーレイセクション
            DrawOverlaySection();

            EditorGUILayout.Space(8);

            // ボーダーセクション
            DrawBorderSection();

            EditorGUILayout.Space(16);

            // リセットボタン
            DrawResetButton();

            // 画像プレビュー情報
            if (selectedTexture != null)
            {
                EditorGUILayout.Space(12);
                DrawImageInfo();
            }

            EditorGUILayout.Space(12);
            EditorGUILayout.EndScrollView();
        }

        private void DrawHeader()
        {
            EditorGUILayout.BeginHorizontal();

            // タイトル
            EditorGUILayout.LabelField(Localization.MainTitle, headerStyle);

            GUILayout.FlexibleSpace();

            // 言語切り替えボタン
            var isJapanese = EditorBackgroundSettings.CurrentLanguage == EditorBackgroundSettings.Language.Japanese;

            if (GUILayout.Button(Localization.LanguageJapanese,
                isJapanese ? languageButtonActiveStyle : languageButtonStyle))
            {
                EditorBackgroundSettings.CurrentLanguage = EditorBackgroundSettings.Language.Japanese;
                Repaint();
            }

            if (GUILayout.Button(Localization.LanguageEnglish,
                !isJapanese ? languageButtonActiveStyle : languageButtonStyle))
            {
                EditorBackgroundSettings.CurrentLanguage = EditorBackgroundSettings.Language.English;
                Repaint();
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawMainToggle()
        {
            EditorGUILayout.BeginVertical(sectionBoxStyle);

            var newEnabled = EditorGUILayout.ToggleLeft(Localization.EnableBackground, enabled, EditorStyles.boldLabel);
            if (newEnabled != enabled)
            {
                enabled = newEnabled;
                EditorBackgroundSettings.Enabled = enabled;
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawBackgroundImageSection()
        {
            EditorGUILayout.BeginVertical(sectionBoxStyle);

            EditorGUILayout.LabelField(Localization.BackgroundImageSection, EditorStyles.boldLabel);
            EditorGUILayout.Space(6);

            // 画像選択
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(Localization.Image);

            // ファイル選択ボタン
            if (GUILayout.Button(Localization.SelectFile, GUILayout.Width(100)))
            {
                var path = EditorUtility.OpenFilePanel(
                    Localization.SelectImageFile,
                    "",
                    "png,jpg,jpeg,gif,bmp,tga");

                if (!string.IsNullOrEmpty(path))
                {
                    EditorBackgroundSettings.ImagePath = path;
                    selectedTexture = EditorBackgroundSettings.GetTexture();
                }
            }

            // クリアボタン
            using (new EditorGUI.DisabledGroupScope(selectedTexture == null))
            {
                if (GUILayout.Button(Localization.ClearImage, GUILayout.Width(50)))
                {
                    EditorBackgroundSettings.ImagePath = "";
                    selectedTexture = null;
                }
            }
            EditorGUILayout.EndHorizontal();

            // 現在選択中のファイル名を表示
            if (!string.IsNullOrEmpty(EditorBackgroundSettings.ImagePath))
            {
                var fileName = Path.GetFileName(EditorBackgroundSettings.ImagePath);
                EditorGUILayout.LabelField("  " + fileName, EditorStyles.miniLabel);
            }

            EditorGUILayout.Space(4);

            // 不透明度
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(new GUIContent(Localization.Opacity, Localization.OpacityTooltip));
            var newOpacity = EditorGUILayout.Slider(opacity, 0f, 1f);
            EditorGUILayout.EndHorizontal();

            if (!Mathf.Approximately(newOpacity, opacity))
            {
                opacity = newOpacity;
                EditorBackgroundSettings.Opacity = opacity;
            }

            EditorGUILayout.Space(4);

            // スケールモード
            var newScaleModeIndex = EditorGUILayout.Popup(
                new GUIContent(Localization.ScaleMode, Localization.ScaleModeTooltip),
                (int)scaleMode,
                Localization.ScaleModeOptions);
            if (newScaleModeIndex != (int)scaleMode)
            {
                scaleMode = (BackgroundScaleMode)newScaleModeIndex;
                EditorBackgroundSettings.ScaleMode = scaleMode;
            }

            // 画像倍率（タイル/コーナーモードのときのみ表示）
            if (scaleMode == BackgroundScaleMode.Tile || scaleMode == BackgroundScaleMode.Corner)
            {
                EditorGUILayout.Space(4);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel(new GUIContent(Localization.TileScale, Localization.TileScaleTooltip));
                var newTileScale = EditorGUILayout.Slider(tileScale, 0.01f, 5f);
                EditorGUILayout.EndHorizontal();

                if (!Mathf.Approximately(newTileScale, tileScale))
                {
                    tileScale = newTileScale;
                    EditorBackgroundSettings.TileScale = tileScale;
                }
            }

            // 配置位置（コーナーモードのときのみ表示）
            if (scaleMode == BackgroundScaleMode.Corner)
            {
                EditorGUILayout.Space(4);

                var newCornerPositionIndex = EditorGUILayout.Popup(
                    new GUIContent(Localization.CornerPositionLabel, Localization.CornerPositionTooltip),
                    (int)cornerPosition,
                    Localization.CornerPositionOptions);
                if (newCornerPositionIndex != (int)cornerPosition)
                {
                    cornerPosition = (CornerPosition)newCornerPositionIndex;
                    EditorBackgroundSettings.CornerPosition = cornerPosition;
                }

                EditorGUILayout.Space(4);

                // オフセットX
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel(new GUIContent(Localization.OffsetX, Localization.OffsetTooltip));
                var newOffsetX = EditorGUILayout.Slider(offsetX, -500f, 500f);
                EditorGUILayout.EndHorizontal();

                if (!Mathf.Approximately(newOffsetX, offsetX))
                {
                    offsetX = newOffsetX;
                    EditorBackgroundSettings.OffsetX = offsetX;
                }

                // オフセットY
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel(new GUIContent(Localization.OffsetY, Localization.OffsetTooltip));
                var newOffsetY = EditorGUILayout.Slider(offsetY, -500f, 500f);
                EditorGUILayout.EndHorizontal();

                if (!Mathf.Approximately(newOffsetY, offsetY))
                {
                    offsetY = newOffsetY;
                    EditorBackgroundSettings.OffsetY = offsetY;
                }
            }

            EditorGUILayout.Space(4);

            // 色調
            var newTintColor = EditorGUILayout.ColorField(
                new GUIContent(Localization.TintColor, Localization.TintColorTooltip),
                tintColor);
            if (newTintColor != tintColor)
            {
                tintColor = newTintColor;
                EditorBackgroundSettings.TintColor = tintColor;
            }

            EditorGUILayout.Space(4);

            // グローバルモード
            var newGlobalMode = EditorGUILayout.Toggle(
                new GUIContent(Localization.GlobalMode, Localization.GlobalModeTooltip),
                globalMode);
            if (newGlobalMode != globalMode)
            {
                globalMode = newGlobalMode;
                EditorBackgroundSettings.GlobalMode = globalMode;
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawOverlaySection()
        {
            EditorGUILayout.BeginVertical(sectionBoxStyle);

            EditorGUILayout.LabelField(Localization.ColorOverlaySection, EditorStyles.boldLabel);
            EditorGUILayout.Space(6);

            // オーバーレイ有効化
            var newOverlayEnabled = EditorGUILayout.Toggle(
                new GUIContent(Localization.EnableOverlay, Localization.EnableOverlayTooltip),
                overlayEnabled);
            if (newOverlayEnabled != overlayEnabled)
            {
                overlayEnabled = newOverlayEnabled;
                EditorBackgroundSettings.OverlayEnabled = overlayEnabled;
            }

            using (new EditorGUI.DisabledGroupScope(!overlayEnabled))
            {
                EditorGUILayout.Space(4);

                // オーバーレイカラー
                var newOverlayColor = EditorGUILayout.ColorField(
                    new GUIContent(Localization.OverlayColor, Localization.OverlayColorTooltip),
                    overlayColor, true, true, false);
                if (newOverlayColor != overlayColor)
                {
                    overlayColor = newOverlayColor;
                    EditorBackgroundSettings.OverlayColor = overlayColor;
                }
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawBorderSection()
        {
            EditorGUILayout.BeginVertical(sectionBoxStyle);

            EditorGUILayout.LabelField(Localization.BorderSection, EditorStyles.boldLabel);
            EditorGUILayout.Space(6);

            // ボーダー有効化
            var newBorderEnabled = EditorGUILayout.Toggle(
                new GUIContent(Localization.EnableBorder, Localization.EnableBorderTooltip),
                borderEnabled);
            if (newBorderEnabled != borderEnabled)
            {
                borderEnabled = newBorderEnabled;
                EditorBackgroundSettings.BorderEnabled = borderEnabled;
            }

            using (new EditorGUI.DisabledGroupScope(!borderEnabled))
            {
                EditorGUILayout.Space(4);

                // ボーダーカラー
                var newBorderColor = EditorGUILayout.ColorField(
                    new GUIContent(Localization.BorderColor, Localization.BorderColorTooltip),
                    borderColor, true, true, false);
                if (newBorderColor != borderColor)
                {
                    borderColor = newBorderColor;
                    EditorBackgroundSettings.BorderColor = borderColor;
                }

                EditorGUILayout.Space(4);

                // ボーダー幅
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel(new GUIContent(Localization.BorderWidth, Localization.BorderWidthTooltip));
                var newBorderWidth = EditorGUILayout.Slider(borderWidth, 1f, 10f);
                EditorGUILayout.EndHorizontal();

                if (!Mathf.Approximately(newBorderWidth, borderWidth))
                {
                    borderWidth = newBorderWidth;
                    EditorBackgroundSettings.BorderWidth = borderWidth;
                }
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawResetButton()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button(Localization.ResetToDefault, GUILayout.Width(140), GUILayout.Height(28)))
            {
                if (EditorUtility.DisplayDialog(
                    Localization.ResetConfirmTitle,
                    Localization.ResetConfirmMessage,
                    Localization.Yes,
                    Localization.No))
                {
                    EditorBackgroundSettings.ResetToDefault();
                    LoadCurrentSettings();
                }
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawImageInfo()
        {
            EditorGUILayout.BeginVertical(sectionBoxStyle);

            EditorGUILayout.LabelField(Localization.ImageInfo, EditorStyles.boldLabel);
            EditorGUILayout.Space(4);

            // パスからファイル名を取得（外部ファイル対応）
            var fileName = Path.GetFileName(EditorBackgroundSettings.ImagePath);
            EditorGUILayout.LabelField($"{Localization.ImageName}: {fileName}");
            EditorGUILayout.LabelField($"{Localization.ImageSize}: {selectedTexture.width} x {selectedTexture.height}");

            EditorGUILayout.EndVertical();
        }
    }
}
