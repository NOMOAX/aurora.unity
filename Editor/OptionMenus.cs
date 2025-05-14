using System;
using Aurora.Diagnostics;
using Aurora.Unity.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Aurora.UnityEditor
{
    internal static class OptionMenus
    {
        private const int AddHorizontalScrollViewPriority = 19950321;

        [MenuItem("GameObject/UI/Horizontal Scroll View", priority = AddHorizontalScrollViewPriority)]
        public static void AddHorizontalScrollView(MenuCommand menuCommand)
        {
            var parentGameObject = menuCommand.context as GameObject;
            if (parentGameObject == null)
            {
                Log.W($"Operate under {nameof(Canvas)}.");
                return;
            }
            var canvas = parentGameObject.GetComponentInParent<Canvas>(true);
            if (canvas == null)
            {
                Log.W($"Operate under {nameof(Canvas)}.");
                return;
            }
            var parent = parentGameObject.transform;

            var gameObject = new GameObject("Horizontal Scroll View");
            var transform  = gameObject.AddComponent<RectTransform>();
            transform.sizeDelta = Vector2.one * 200;
            {
                var image = gameObject.AddComponent<Image>();
                image.color = new Color(0.125f, 0.125f, 0.125f);
            }
            ScrollRect scrollRect;
            ScrollView scrollView;
            {
                scrollRect = gameObject.AddComponent<ScrollRect>();
                scrollView = gameObject.AddComponent<ScrollView>();
            }
            {
                var layoutElement = gameObject.AddComponent<LayoutElement>();
                layoutElement.minWidth        = 200;
                layoutElement.minHeight       = 200;
                layoutElement.preferredWidth  = 200;
                layoutElement.preferredHeight = 200;
            }
        }

        private const int AddVerticalScrollViewPriority = AddHorizontalScrollViewPriority + 1;

        [MenuItem("GameObject/UI/Vertical Scroll View", priority = AddVerticalScrollViewPriority)]
        public static void AddVerticalScrollView(MenuCommand menuCommand)
        {
            var parentGameObject = menuCommand.context as GameObject;
            if (parentGameObject == null)
            {
                Log.W($"Operate under {nameof(Canvas)}.");
                return;
            }
            var canvas = parentGameObject.GetComponentInParent<Canvas>(true);
            if (canvas == null)
            {
                Log.W($"Operate under {nameof(Canvas)}.");
                return;
            }
            var parent = parentGameObject.transform;

            var gameObject = new GameObject("Vertical Scroll View");

            var transform = gameObject.AddComponent<RectTransform>();
            transform.SetParent(parent, false);
            transform.localPosition    = Vector3.zero;
            transform.localRotation    = Quaternion.identity;
            transform.localScale       = Vector3.one;
            transform.anchorMin        = new Vector2(0.5f, 0.5f);
            transform.anchorMax        = new Vector2(0.5f, 0.5f);
            transform.anchoredPosition = Vector2.zero;
            transform.sizeDelta        = new Vector2(200f, 200f);
            transform.pivot            = new Vector2(0.5f, 0.5f);

            var image = gameObject.AddComponent<Image>();
            image.color         = new Color(1f, 1f, 1f, 0.125f);
            image.raycastTarget = false;

            var scrollRect = gameObject.AddComponent<ScrollRect>();
            scrollRect.horizontal        = false;
            scrollRect.vertical          = true;
            scrollRect.scrollSensitivity = 32f;

            var viewportGameObject = new GameObject("Viewport");
            var viewportTransform  = viewportGameObject.AddComponent<RectTransform>();
            viewportTransform.SetParent(transform, false);
            viewportTransform.localPosition    = Vector3.zero;
            viewportTransform.localRotation    = Quaternion.identity;
            viewportTransform.localScale       = Vector3.one;
            viewportTransform.anchorMin        = Vector2.zero;
            viewportTransform.anchorMax        = Vector2.one;
            viewportTransform.anchoredPosition = Vector2.zero;
            viewportTransform.sizeDelta        = new Vector2(-10f, 0f);
            viewportTransform.pivot            = new Vector2(0f,   1f);
            viewportGameObject.AddComponent<Image>();
            var viewportMask = viewportGameObject.AddComponent<Mask>();
            viewportMask.showMaskGraphic = false;

            var contentGameObject = new GameObject("Content");
            var contentTransform  = contentGameObject.AddComponent<RectTransform>();
            contentTransform.SetParent(viewportTransform);
            contentTransform.localPosition    = Vector3.zero;
            contentTransform.localRotation    = Quaternion.identity;
            contentTransform.localScale       = Vector3.one;
            contentTransform.anchorMin        = new Vector2(0f, 1f);
            contentTransform.anchorMax        = new Vector2(1f, 1f);
            contentTransform.anchoredPosition = Vector2.zero;
            contentTransform.sizeDelta        = new Vector2(0f, 300f);
            contentTransform.pivot            = new Vector2(0f, 1f);

            var scrollBarGameObject = new GameObject("Scrollbar");
            var scrollbarTransform  = scrollBarGameObject.AddComponent<RectTransform>();
            scrollbarTransform.SetParent(transform, false);
            scrollbarTransform.localPosition    = Vector3.zero;
            scrollbarTransform.localRotation    = Quaternion.identity;
            scrollbarTransform.localScale       = Vector3.one;
            scrollbarTransform.anchorMin        = new Vector2(1f, 0f);
            scrollbarTransform.anchorMax        = new Vector2(1f, 1f);
            scrollbarTransform.anchoredPosition = Vector2.zero;
            scrollbarTransform.sizeDelta        = new Vector2(10f, 0f);
            scrollbarTransform.pivot            = new Vector2(1f,  1f);

            var scrollBarImage = scrollBarGameObject.AddComponent<Image>();
            scrollBarImage.color = new Color(0.125f, 0.125f, 0.125f);

            var scrollbar = scrollBarGameObject.AddComponent<Scrollbar>();
            scrollbar.direction = Scrollbar.Direction.BottomToTop;

            var slidingAreaGameObject = new GameObject("Sliding Area");
            var slidingAreaTransform  = slidingAreaGameObject.AddComponent<RectTransform>();
            slidingAreaTransform.SetParent(scrollbarTransform, false);
            slidingAreaTransform.localPosition    = Vector3.zero;
            slidingAreaTransform.localRotation    = Quaternion.identity;
            slidingAreaTransform.localScale       = Vector3.one;
            slidingAreaTransform.anchorMin        = Vector2.zero;
            slidingAreaTransform.anchorMax        = Vector2.one;
            slidingAreaTransform.anchoredPosition = Vector2.zero;
            slidingAreaTransform.sizeDelta        = new Vector2(0f, -10f);

            var handleObject    = new GameObject("Handle");
            var handleTransform = handleObject.AddComponent<RectTransform>();
            handleTransform.SetParent(slidingAreaTransform, false);
            handleTransform.localPosition    = Vector3.zero;
            handleTransform.localRotation    = Quaternion.identity;
            handleTransform.localScale       = Vector3.one;
            handleTransform.anchoredPosition = Vector2.zero;
            handleTransform.sizeDelta        = new Vector2(0f, 10f);

            var handleImage = handleObject.AddComponent<Image>();
            scrollbar.targetGraphic = handleImage;
            scrollbar.handleRect    = handleTransform;

            var inactiveContainerGameObject = new GameObject("Inactive Container");
            inactiveContainerGameObject.SetActive(false);
            var inactiveContainer = inactiveContainerGameObject.AddComponent<RectTransform>();
            inactiveContainer.SetParent(transform, false);
            inactiveContainer.localPosition    = Vector3.zero;
            inactiveContainer.localRotation    = Quaternion.identity;
            inactiveContainer.localScale       = Vector3.one;
            inactiveContainer.anchorMin        = Vector2.zero;
            inactiveContainer.anchorMax        = Vector2.one;
            inactiveContainer.anchoredPosition = Vector2.zero;
            inactiveContainer.sizeDelta        = Vector2.zero;
            inactiveContainer.pivot            = new Vector2(0.5f, 0.5f);

            var verticalLayoutGroup = contentGameObject.AddComponent<VerticalLayoutGroup>();
            verticalLayoutGroup.childForceExpandWidth  = false;
            verticalLayoutGroup.childForceExpandHeight = false;

            var leadingPlaceholderGameObject = new GameObject("Leading Placeholder");
            leadingPlaceholderGameObject.SetActive(false);
            var leadingPlaceholderTransform = leadingPlaceholderGameObject.AddComponent<RectTransform>();
            leadingPlaceholderTransform.SetParent(contentTransform, false);
            leadingPlaceholderTransform.localPosition    = Vector3.zero;
            leadingPlaceholderTransform.localRotation    = Quaternion.identity;
            leadingPlaceholderTransform.localScale       = Vector3.one;
            leadingPlaceholderTransform.anchorMin        = new Vector2(0f, 1f);
            leadingPlaceholderTransform.anchorMax        = new Vector2(0f, 1f);
            leadingPlaceholderTransform.anchoredPosition = Vector2.zero;
            leadingPlaceholderTransform.sizeDelta        = Vector2.zero;
            leadingPlaceholderTransform.pivot            = new Vector2(0f, 1f);

            var leadingPlaceholder = leadingPlaceholderGameObject.AddComponent<LayoutElement>();
            leadingPlaceholder.minHeight = 0;

            var trailingPlaceholderGameObject = new GameObject("Trailing Placeholder");
            trailingPlaceholderGameObject.SetActive(false);
            var trailingPlaceholderTransform = trailingPlaceholderGameObject.AddComponent<RectTransform>();
            trailingPlaceholderTransform.SetParent(contentTransform, false);
            trailingPlaceholderTransform.localPosition    = Vector3.zero;
            trailingPlaceholderTransform.localRotation    = Quaternion.identity;
            trailingPlaceholderTransform.localScale       = Vector3.one;
            trailingPlaceholderTransform.anchorMin        = new Vector2(0f, 1f);
            trailingPlaceholderTransform.anchorMax        = new Vector2(0f, 1f);
            trailingPlaceholderTransform.anchoredPosition = Vector2.zero;
            trailingPlaceholderTransform.sizeDelta        = Vector2.zero;
            trailingPlaceholderTransform.pivot            = new Vector2(0f, 1f);

            var trailingPlaceholder = trailingPlaceholderGameObject.AddComponent<LayoutElement>();
            trailingPlaceholder.minHeight = 0;

            var scrollView = gameObject.AddComponent<VerticalScrollView>();
        }
    }
}
