using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace EditorBackground
{
    /// <summary>
    /// 背景描画のコアロジック
    /// </summary>
    [InitializeOnLoad]
    public static class EditorBackgroundCore
    {
        private const string BACKGROUND_ELEMENT_NAME = "editor-background-image";

        private static HashSet<EditorWindow> processedWindows = new HashSet<EditorWindow>();
        private static Dictionary<EditorWindow, VisualElement> backgroundElements = new Dictionary<EditorWindow, VisualElement>();

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

            var allWindows = Resources.FindObjectsOfTypeAll<EditorWindow>();
            foreach (var window in allWindows)
            {
                if (window != null && !processedWindows.Contains(window))
                {
                    ApplyBackground(window);
                    processedWindows.Add(window);
                }
            }
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

            var bg = CreateBackgroundElement(texture);
            window.rootVisualElement.Insert(0, bg);
            backgroundElements[window] = bg;
        }

        private static VisualElement CreateBackgroundElement(Texture2D texture)
        {
            var bg = new VisualElement();
            bg.name = BACKGROUND_ELEMENT_NAME;
            bg.style.position = Position.Absolute;
            bg.style.left = 0;
            bg.style.top = 0;
            bg.style.right = 0;
            bg.style.bottom = 0;
            bg.style.backgroundImage = texture;
            ApplyBackgroundSize(bg, EditorBackgroundSettings.ScaleMode);
            bg.style.backgroundRepeat = new BackgroundRepeat(Repeat.NoRepeat, Repeat.NoRepeat);
            bg.style.opacity = EditorBackgroundSettings.Opacity;
            bg.style.unityBackgroundImageTintColor = EditorBackgroundSettings.TintColor;
            bg.pickingMode = PickingMode.Ignore;

            return bg;
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

            foreach (var kvp in backgroundElements.ToList())
            {
                var window = kvp.Key;
                var bg = kvp.Value;

                if (window == null || bg == null)
                    continue;

                if (texture == null)
                {
                    RemoveBackground(window);
                }
                else
                {
                    bg.style.backgroundImage = texture;
                    ApplyBackgroundSize(bg, EditorBackgroundSettings.ScaleMode);
                    bg.style.opacity = EditorBackgroundSettings.Opacity;
                    bg.style.unityBackgroundImageTintColor = EditorBackgroundSettings.TintColor;
                }
            }

            // 背景がまだ適用されていないウィンドウに適用
            if (texture != null)
            {
                var allWindows = Resources.FindObjectsOfTypeAll<EditorWindow>();
                foreach (var window in allWindows)
                {
                    if (window != null && !backgroundElements.ContainsKey(window))
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
