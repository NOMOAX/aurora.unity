using System;
using Aurora.Unity.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Aurora.UnityEditor
{
    internal static class OptionMenus
    {
        private const int Priority = 19950321;

        [MenuItem("GameObject/UI/Horizontal Scroll View", priority = Priority)]
        public static void AddEnhancedScroller(MenuCommand menuCommand)
        {
            throw new NotImplementedException();

            var parentGameObject = menuCommand.context as GameObject;
            if (parentGameObject == null)
            {
                return;
            }
            var canvas = parentGameObject.GetComponentInParent<Canvas>(true);
            if (canvas == null)
            {
                return;
            }
            var parent = parentGameObject.transform;

            var gameObject = new GameObject(nameof(ScrollView));
            var transform  = gameObject.AddComponent<RectTransform>();
            transform.sizeDelta = Vector2.one * 200;
            {
                var image = gameObject.AddComponent<Image>();
                image.color = new Color(0.125f, 0.125f, 0.125f);
            }
            ScrollRect scrollRect;
            ScrollView   scroller;
            {
                scrollRect = gameObject.AddComponent<ScrollRect>();
                scroller   = gameObject.AddComponent<ScrollView>();
            }
            {
                var layoutElement = gameObject.AddComponent<LayoutElement>();
                layoutElement.minWidth        = 200;
                layoutElement.minHeight       = 200;
                layoutElement.preferredWidth  = 200;
                layoutElement.preferredHeight = 200;
            }
        }
    }
}
