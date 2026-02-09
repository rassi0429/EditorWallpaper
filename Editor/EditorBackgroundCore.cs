using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace EditorBackground
{
    /// <summary>
    /// 背景描画のコアロジック - グローバル/個別モード対応版
    /// </summary>
    [InitializeOnLoad]
    public static class EditorBackgroundCore
    {
        private const string BACKGROUND_ELEMENT_NAME = "editor-background-image";
        private const string OVERLAY_ELEMENT_NAME = "editor-background-overlay";
        private const string BORDER_ELEMENT_NAME = "editor-background-border";

        private static HashSet<EditorWindow> processedWindows = new HashSet<EditorWindow>();
        private static Dictionary<EditorWindow, VisualElement> backgroundElements = new Dictionary<EditorWindow, VisualElement>();
        private static Dictionary<EditorWindow, VisualElement> overlayElements = new Dictionary<EditorWindow, VisualElement>();
        private static Dictionary<EditorWindow, VisualElement> borderElements = new Dictionary<EditorWindow, VisualElement>();

        // ランダム画像用：ウィンドウごとのテクスチャ
        private static Dictionary<EditorWindow, Texture2D> windowTextures = new Dictionary<EditorWindow, Texture2D>();

        // グローバル背景の基準となる境界
        private static Rect globalBounds;

        static EditorBackgroundCore()
        {
            EditorBackgroundSettings.Load();
            EditorApplication.update += OnUpdate;
            EditorBackgroundSettings.OnSettingsChanged += RefreshAllBackgrounds;
        }

        private static void OnUpdate()
        {
            CleanupDestroyedWindows();

            if (!EditorBackgroundSettings.Enabled)
                return;

            // スライドショー更新（プレミアム機能）
            EditorBackgroundSettings.UpdateSlideshow();

            // グローバルモードの場合のみ境界を更新
            if (EditorBackgroundSettings.GlobalMode)
            {
                UpdateGlobalBounds();
            }

            var allWindows = Resources.FindObjectsOfTypeAll<EditorWindow>();
            foreach (var window in allWindows)
            {
                if (window == null)
                    continue;

                if (!processedWindows.Contains(window))
                {
                    ApplyAllElements(window);
                    processedWindows.Add(window);
                }
                else if (EditorBackgroundSettings.GlobalMode)
                {
                    // グローバルモードの場合のみ位置更新
                    UpdateBackgroundPosition(window);
                }
            }
        }

        private static void UpdateGlobalBounds()
        {
            var allWindows = Resources.FindObjectsOfTypeAll<EditorWindow>();
            if (allWindows.Length == 0)
                return;

            float minX = float.MaxValue;
            float minY = float.MaxValue;
            float maxX = float.MinValue;
            float maxY = float.MinValue;

            foreach (var window in allWindows)
            {
                if (window == null)
                    continue;

                var pos = window.position;
                minX = Mathf.Min(minX, pos.x);
                minY = Mathf.Min(minY, pos.y);
                maxX = Mathf.Max(maxX, pos.x + pos.width);
                maxY = Mathf.Max(maxY, pos.y + pos.height);
            }

            globalBounds = new Rect(minX, minY, maxX - minX, maxY - minY);
        }

        private static void CleanupDestroyedWindows()
        {
            processedWindows.RemoveWhere(w => w == null);

            CleanupDictionary(backgroundElements);
            CleanupDictionary(overlayElements);
            CleanupDictionary(borderElements);
            CleanupTextureDictionary(windowTextures);
        }

        private static void CleanupTextureDictionary(Dictionary<EditorWindow, Texture2D> dict)
        {
            var keysToRemove = dict.Keys.Where(k => k == null).ToList();
            foreach (var key in keysToRemove)
            {
                if (dict[key] != null)
                {
                    Object.DestroyImmediate(dict[key]);
                }
                dict.Remove(key);
            }
        }

        private static void CleanupDictionary(Dictionary<EditorWindow, VisualElement> dict)
        {
            var keysToRemove = dict.Keys.Where(k => k == null).ToList();
            foreach (var key in keysToRemove)
            {
                dict.Remove(key);
            }
        }

        private static void ApplyAllElements(EditorWindow window)
        {
            if (window == null || window.rootVisualElement == null)
                return;

            // 特定のウィンドウには適用しない
            var typeName = window.GetType().Name;
            if (typeName == "EditorBackgroundWindow" ||  // 設定ウィンドウ
                typeName == "SceneView" ||               // Sceneビュー
                typeName == "GameView")                  // Gameビュー
                return;

            // 背景画像
            Texture2D texture;

            // プレミアム機能：フォルダモード + グローバルモードOFF + ランダム画像有効時
            if (!EditorBackgroundSettings.GlobalMode &&
                EditorBackgroundSettings.IsPremium &&
                EditorBackgroundSettings.ImageSourceMode == ImageSourceMode.Folder &&
                EditorBackgroundSettings.RandomPerWindow &&
                EditorBackgroundSettings.GetFolderImages().Length > 0)
            {
                // ウィンドウごとにランダムなテクスチャを取得（キャッシュ）
                if (!windowTextures.TryGetValue(window, out texture) || texture == null)
                {
                    texture = EditorBackgroundSettings.GetRandomTexture();
                    if (texture != null)
                    {
                        windowTextures[window] = texture;
                    }
                }
            }
            else
            {
                texture = EditorBackgroundSettings.GetTexture();
            }

            if (texture != null)
            {
                ApplyBackground(window, texture);
            }

            // オーバーレイ
            if (EditorBackgroundSettings.OverlayEnabled)
            {
                ApplyOverlay(window);
            }

            // ボーダー
            if (EditorBackgroundSettings.BorderEnabled)
            {
                ApplyBorder(window);
            }
        }

        private static void ApplyBackground(EditorWindow window, Texture2D texture)
        {
            RemoveElement(window, BACKGROUND_ELEMENT_NAME, backgroundElements);

            VisualElement bg;
            if (EditorBackgroundSettings.GlobalMode)
            {
                bg = CreateGlobalBackgroundElement(window, texture);
            }
            else
            {
                bg = CreateLocalBackgroundElement(texture);
            }

            window.rootVisualElement.Insert(0, bg);
            backgroundElements[window] = bg;
        }

        private static void ApplyOverlay(EditorWindow window)
        {
            RemoveElement(window, OVERLAY_ELEMENT_NAME, overlayElements);

            var overlay = CreateOverlayElement();
            window.rootVisualElement.Insert(0, overlay);
            overlayElements[window] = overlay;
        }

        private static void ApplyBorder(EditorWindow window)
        {
            RemoveElement(window, BORDER_ELEMENT_NAME, borderElements);

            var border = CreateBorderElement();
            window.rootVisualElement.Add(border);
            borderElements[window] = border;
        }

        /// <summary>
        /// グローバルモード用の背景要素を作成
        /// </summary>
        private static VisualElement CreateGlobalBackgroundElement(EditorWindow window, Texture2D texture)
        {
            var bg = new VisualElement();
            bg.name = BACKGROUND_ELEMENT_NAME;
            bg.style.position = Position.Absolute;
            bg.style.overflow = Overflow.Hidden;
            bg.style.left = 0;
            bg.style.top = 0;
            bg.style.right = 0;
            bg.style.bottom = 0;
            bg.pickingMode = PickingMode.Ignore;

            // 内側に実際の背景画像を配置（グローバルサイズ）
            var innerBg = new VisualElement();
            innerBg.name = "editor-background-inner";
            innerBg.style.position = Position.Absolute;
            innerBg.style.backgroundImage = texture;
            // TintColorのアルファ値にOpacityを乗算して適用
            var tintWithOpacity = EditorBackgroundSettings.TintColor;
            tintWithOpacity.a *= EditorBackgroundSettings.Opacity;
            innerBg.style.unityBackgroundImageTintColor = tintWithOpacity;
            innerBg.pickingMode = PickingMode.Ignore;

            ApplyBackgroundSize(innerBg, EditorBackgroundSettings.ScaleMode, texture);
            UpdateInnerBackgroundTransform(window, innerBg);

            bg.Add(innerBg);
            return bg;
        }

        /// <summary>
        /// 個別モード用の背景要素を作成（従来の方式）
        /// </summary>
        private static VisualElement CreateLocalBackgroundElement(Texture2D texture)
        {
            var bg = new VisualElement();
            bg.name = BACKGROUND_ELEMENT_NAME;
            bg.style.position = Position.Absolute;
            bg.style.left = 0;
            bg.style.top = 0;
            bg.style.right = 0;
            bg.style.bottom = 0;
            bg.style.backgroundImage = texture;
            // TintColorのアルファ値にOpacityを乗算して適用
            var tintWithOpacity = EditorBackgroundSettings.TintColor;
            tintWithOpacity.a *= EditorBackgroundSettings.Opacity;
            bg.style.unityBackgroundImageTintColor = tintWithOpacity;
            bg.pickingMode = PickingMode.Ignore;

            ApplyBackgroundSize(bg, EditorBackgroundSettings.ScaleMode, texture);

            return bg;
        }

        /// <summary>
        /// オーバーレイ要素を作成
        /// </summary>
        private static VisualElement CreateOverlayElement()
        {
            var overlay = new VisualElement();
            overlay.name = OVERLAY_ELEMENT_NAME;
            overlay.style.position = Position.Absolute;
            overlay.style.left = 0;
            overlay.style.top = 0;
            overlay.style.right = 0;
            overlay.style.bottom = 0;
            overlay.style.backgroundColor = EditorBackgroundSettings.OverlayColor;
            overlay.pickingMode = PickingMode.Ignore;

            return overlay;
        }

        /// <summary>
        /// ボーダー要素を作成
        /// </summary>
        private static VisualElement CreateBorderElement()
        {
            var border = new VisualElement();
            border.name = BORDER_ELEMENT_NAME;
            border.style.position = Position.Absolute;
            border.style.left = 0;
            border.style.top = 0;
            border.style.right = 0;
            border.style.bottom = 0;
            border.style.borderTopWidth = EditorBackgroundSettings.BorderWidth;
            border.style.borderBottomWidth = EditorBackgroundSettings.BorderWidth;
            border.style.borderLeftWidth = EditorBackgroundSettings.BorderWidth;
            border.style.borderRightWidth = EditorBackgroundSettings.BorderWidth;
            border.style.borderTopColor = EditorBackgroundSettings.BorderColor;
            border.style.borderBottomColor = EditorBackgroundSettings.BorderColor;
            border.style.borderLeftColor = EditorBackgroundSettings.BorderColor;
            border.style.borderRightColor = EditorBackgroundSettings.BorderColor;
            border.pickingMode = PickingMode.Ignore;

            return border;
        }

        private static void UpdateInnerBackgroundTransform(EditorWindow window, VisualElement innerBg)
        {
            if (window == null || innerBg == null)
                return;

            var windowPos = window.position;

            // ウィンドウ位置からグローバル基準位置へのオフセット
            float offsetX = globalBounds.x - windowPos.x;
            float offsetY = globalBounds.y - windowPos.y;

            // 内側の背景をグローバルサイズに設定
            innerBg.style.left = offsetX;
            innerBg.style.top = offsetY;
            innerBg.style.width = globalBounds.width;
            innerBg.style.height = globalBounds.height;
        }

        private static void UpdateBackgroundPosition(EditorWindow window)
        {
            if (!backgroundElements.TryGetValue(window, out var bg))
                return;

            if (bg == null)
                return;

            var innerBg = bg.Q("editor-background-inner");
            if (innerBg == null)
                return;

            UpdateInnerBackgroundTransform(window, innerBg);
        }

        private static void ApplyBackgroundSize(VisualElement element, BackgroundScaleMode scaleMode, Texture2D texture = null)
        {
            switch (scaleMode)
            {
                case BackgroundScaleMode.ScaleAndCrop:
                    element.style.backgroundSize = new BackgroundSize(BackgroundSizeType.Cover);
                    element.style.backgroundRepeat = new BackgroundRepeat(Repeat.NoRepeat, Repeat.NoRepeat);
                    break;
                case BackgroundScaleMode.ScaleToFit:
                    element.style.backgroundSize = new BackgroundSize(BackgroundSizeType.Contain);
                    element.style.backgroundRepeat = new BackgroundRepeat(Repeat.NoRepeat, Repeat.NoRepeat);
                    break;
                case BackgroundScaleMode.StretchToFill:
                    element.style.backgroundSize = new BackgroundSize(Length.Percent(100), Length.Percent(100));
                    element.style.backgroundRepeat = new BackgroundRepeat(Repeat.NoRepeat, Repeat.NoRepeat);
                    break;
                case BackgroundScaleMode.Tile:
                    if (texture != null)
                    {
                        float scale = EditorBackgroundSettings.TileScale;
                        float w = texture.width * scale;
                        float h = texture.height * scale;
                        // 頂点数オーバーフロー防止: 最小16pxを保証
                        const float minTileSize = 16f;
                        if (w < minTileSize || h < minTileSize)
                        {
                            float ratio = Mathf.Max(minTileSize / w, minTileSize / h);
                            w *= ratio;
                            h *= ratio;
                        }
                        var widthLen = new Length(w, LengthUnit.Pixel);
                        var heightLen = new Length(h, LengthUnit.Pixel);
                        element.style.backgroundSize = new BackgroundSize(widthLen, heightLen);
                    }
                    element.style.backgroundRepeat = new BackgroundRepeat(Repeat.Repeat, Repeat.Repeat);
                    break;
                case BackgroundScaleMode.Corner:
                    if (texture != null)
                    {
                        float scale = EditorBackgroundSettings.TileScale;
                        float w = texture.width * scale;
                        float h = texture.height * scale;
                        var widthLen = new Length(w, LengthUnit.Pixel);
                        var heightLen = new Length(h, LengthUnit.Pixel);
                        element.style.backgroundSize = new BackgroundSize(widthLen, heightLen);
                    }
                    element.style.backgroundRepeat = new BackgroundRepeat(Repeat.NoRepeat, Repeat.NoRepeat);
                    ApplyCornerPosition(element, EditorBackgroundSettings.CornerPosition);
                    break;
                default:
                    element.style.backgroundSize = new BackgroundSize(BackgroundSizeType.Cover);
                    element.style.backgroundRepeat = new BackgroundRepeat(Repeat.NoRepeat, Repeat.NoRepeat);
                    break;
            }
        }

        private static void ApplyCornerPosition(VisualElement element, CornerPosition position)
        {
            float offsetX = EditorBackgroundSettings.OffsetX;
            float offsetY = EditorBackgroundSettings.OffsetY;

            switch (position)
            {
                case CornerPosition.TopLeft:
                    element.style.backgroundPositionX = new BackgroundPosition(BackgroundPositionKeyword.Left, offsetX);
                    element.style.backgroundPositionY = new BackgroundPosition(BackgroundPositionKeyword.Top, offsetY);
                    break;
                case CornerPosition.TopRight:
                    element.style.backgroundPositionX = new BackgroundPosition(BackgroundPositionKeyword.Right, -offsetX);
                    element.style.backgroundPositionY = new BackgroundPosition(BackgroundPositionKeyword.Top, offsetY);
                    break;
                case CornerPosition.BottomLeft:
                    element.style.backgroundPositionX = new BackgroundPosition(BackgroundPositionKeyword.Left, offsetX);
                    element.style.backgroundPositionY = new BackgroundPosition(BackgroundPositionKeyword.Bottom, -offsetY);
                    break;
                case CornerPosition.BottomRight:
                    element.style.backgroundPositionX = new BackgroundPosition(BackgroundPositionKeyword.Right, -offsetX);
                    element.style.backgroundPositionY = new BackgroundPosition(BackgroundPositionKeyword.Bottom, -offsetY);
                    break;
            }
        }

        private static void RemoveElement(EditorWindow window, string elementName, Dictionary<EditorWindow, VisualElement> dict)
        {
            if (window == null || window.rootVisualElement == null)
                return;

            var existing = window.rootVisualElement.Q(elementName);
            if (existing != null)
            {
                existing.RemoveFromHierarchy();
            }

            dict.Remove(window);
        }

        private static void RemoveAllFromWindow(EditorWindow window)
        {
            RemoveElement(window, BACKGROUND_ELEMENT_NAME, backgroundElements);
            RemoveElement(window, OVERLAY_ELEMENT_NAME, overlayElements);
            RemoveElement(window, BORDER_ELEMENT_NAME, borderElements);
        }

        // 前回の設定値をキャッシュ（テクスチャ再読み込みが必要か判定用）
        private static string _lastImagePath;
        private static ImageSourceMode _lastImageSourceMode;
        private static string _lastImageFolderPath;
        private static bool _lastRandomPerWindow;
        private static bool _lastGlobalMode;

        private static void RefreshAllBackgrounds()
        {
            // テクスチャの再取得が必要かどうか判定
            bool needsTextureRefresh =
                _lastImagePath != EditorBackgroundSettings.ImagePath ||
                _lastImageSourceMode != EditorBackgroundSettings.ImageSourceMode ||
                _lastImageFolderPath != EditorBackgroundSettings.ImageFolderPath ||
                _lastRandomPerWindow != EditorBackgroundSettings.RandomPerWindow ||
                _lastGlobalMode != EditorBackgroundSettings.GlobalMode;

            // 設定値を更新
            _lastImagePath = EditorBackgroundSettings.ImagePath;
            _lastImageSourceMode = EditorBackgroundSettings.ImageSourceMode;
            _lastImageFolderPath = EditorBackgroundSettings.ImageFolderPath;
            _lastRandomPerWindow = EditorBackgroundSettings.RandomPerWindow;
            _lastGlobalMode = EditorBackgroundSettings.GlobalMode;

            // 一度全部削除
            foreach (var window in processedWindows.ToList())
            {
                if (window != null)
                {
                    RemoveAllFromWindow(window);
                }
            }
            processedWindows.Clear();

            // ウィンドウごとのテクスチャは必要な時だけクリア
            if (needsTextureRefresh)
            {
                foreach (var tex in windowTextures.Values)
                {
                    if (tex != null)
                    {
                        Object.DestroyImmediate(tex);
                    }
                }
                windowTextures.Clear();
            }

            if (!EditorBackgroundSettings.Enabled)
                return;

            if (EditorBackgroundSettings.GlobalMode)
            {
                UpdateGlobalBounds();
            }

            var allWindows = Resources.FindObjectsOfTypeAll<EditorWindow>();
            foreach (var window in allWindows)
            {
                if (window != null)
                {
                    ApplyAllElements(window);
                    processedWindows.Add(window);
                }
            }
        }

        private static void RemoveAllBackgrounds()
        {
            foreach (var window in processedWindows.ToList())
            {
                if (window != null)
                {
                    RemoveAllFromWindow(window);
                }
            }

            backgroundElements.Clear();
            overlayElements.Clear();
            borderElements.Clear();
            processedWindows.Clear();
        }
    }
}
