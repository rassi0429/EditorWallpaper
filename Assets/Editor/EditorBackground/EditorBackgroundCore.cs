using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace EditorBackground
{
    /// <summary>
    /// 背景描画のコアロジック - グローバル背景対応版
    /// </summary>
    [InitializeOnLoad]
    public static class EditorBackgroundCore
    {
        private const string BACKGROUND_ELEMENT_NAME = "editor-background-image";

        private static HashSet<EditorWindow> processedWindows = new HashSet<EditorWindow>();
        private static Dictionary<EditorWindow, VisualElement> backgroundElements = new Dictionary<EditorWindow, VisualElement>();

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

            // グローバル境界を更新
            UpdateGlobalBounds();

            var allWindows = Resources.FindObjectsOfTypeAll<EditorWindow>();
            foreach (var window in allWindows)
            {
                if (window == null)
                    continue;

                if (!processedWindows.Contains(window))
                {
                    ApplyBackground(window);
                    processedWindows.Add(window);
                }
                else
                {
                    // ウィンドウ位置が変わった場合に背景位置を更新
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

            var keysToRemove = backgroundElements.Keys.Where(k => k == null).ToList();
            foreach (var key in keysToRemove)
            {
                backgroundElements.Remove(key);
            }
        }

        private static void ApplyBackground(EditorWindow window)
        {
            if (window == null || window.rootVisualElement == null)
                return;

            var texture = EditorBackgroundSettings.GetTexture();
            if (texture == null)
                return;

            RemoveBackground(window);

            var bg = CreateBackgroundElement(window, texture);
            window.rootVisualElement.Insert(0, bg);
            backgroundElements[window] = bg;
        }

        private static VisualElement CreateBackgroundElement(EditorWindow window, Texture2D texture)
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
            innerBg.style.backgroundRepeat = new BackgroundRepeat(Repeat.NoRepeat, Repeat.NoRepeat);
            innerBg.style.opacity = EditorBackgroundSettings.Opacity;
            innerBg.style.unityBackgroundImageTintColor = EditorBackgroundSettings.TintColor;
            innerBg.pickingMode = PickingMode.Ignore;

            ApplyBackgroundSize(innerBg, EditorBackgroundSettings.ScaleMode);
            UpdateInnerBackgroundTransform(window, innerBg);

            bg.Add(innerBg);
            return bg;
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

        private static void ApplyBackgroundSize(VisualElement element, ScaleMode scaleMode)
        {
            switch (scaleMode)
            {
                case ScaleMode.ScaleAndCrop:
                    element.style.backgroundSize = new BackgroundSize(BackgroundSizeType.Cover);
                    break;
                case ScaleMode.ScaleToFit:
                    element.style.backgroundSize = new BackgroundSize(BackgroundSizeType.Contain);
                    break;
                case ScaleMode.StretchToFill:
                    element.style.backgroundSize = new BackgroundSize(Length.Percent(100), Length.Percent(100));
                    break;
                default:
                    element.style.backgroundSize = new BackgroundSize(BackgroundSizeType.Cover);
                    break;
            }
        }

        private static void RemoveBackground(EditorWindow window)
        {
            if (window == null || window.rootVisualElement == null)
                return;

            var existingBg = window.rootVisualElement.Q(BACKGROUND_ELEMENT_NAME);
            if (existingBg != null)
            {
                existingBg.RemoveFromHierarchy();
            }

            backgroundElements.Remove(window);
        }

        private static void RefreshAllBackgrounds()
        {
            if (!EditorBackgroundSettings.Enabled)
            {
                RemoveAllBackgrounds();
                return;
            }

            var texture = EditorBackgroundSettings.GetTexture();

            // 一度全部削除して再適用（設定変更時）
            foreach (var window in backgroundElements.Keys.ToList())
            {
                if (window != null)
                {
                    RemoveBackground(window);
                    processedWindows.Remove(window);
                }
            }

            if (texture != null)
            {
                UpdateGlobalBounds();
                var allWindows = Resources.FindObjectsOfTypeAll<EditorWindow>();
                foreach (var window in allWindows)
                {
                    if (window != null)
                    {
                        ApplyBackground(window);
                        processedWindows.Add(window);
                    }
                }
            }
        }

        private static void RemoveAllBackgrounds()
        {
            foreach (var kvp in backgroundElements.ToList())
            {
                var window = kvp.Key;
                if (window != null)
                {
                    RemoveBackground(window);
                }
            }

            backgroundElements.Clear();
            processedWindows.Clear();
        }
    }
}
