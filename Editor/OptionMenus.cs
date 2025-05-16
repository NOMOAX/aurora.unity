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
        private const int CreateHorizontalScrollViewPriority = 19950321;

        [MenuItem("GameObject/UI/Horizontal Scroll View", priority = CreateHorizontalScrollViewPriority)]
        public static void CreateHorizontalScrollView(MenuCommand menuCommand)
        {
            var parentGameObject = menuCommand.context as GameObject;
            if (!parentGameObject)
            {
                Log.W($"You can only do this operation under a {nameof(Canvas)}.");
                return;
            }
            var canvas = parentGameObject.GetComponentInParent<Canvas>(true);
            if (!canvas)
            {
                Log.W($"You can only do this operation under a {nameof(Canvas)}.");
                return;
            }
            var parent = parentGameObject.transform;

            CreateScrollView(
                "Horizontal Scroll View",
                parent,
                new Vector2(200f, 200f),
                20f,
                300f,
                RectTransform.Axis.Horizontal
            );
        }

        private const int CreateVerticalScrollViewPriority = CreateHorizontalScrollViewPriority + 1;

        [MenuItem("GameObject/UI/Vertical Scroll View", priority = CreateVerticalScrollViewPriority)]
        public static void CreateVerticalScrollView(MenuCommand menuCommand)
        {
            var parentGameObject = menuCommand.context as GameObject;
            if (!parentGameObject)
            {
                Log.W($"You can only do this operation under a {nameof(Canvas)}.");
                return;
            }
            var canvas = parentGameObject.GetComponentInParent<Canvas>(true);
            if (!canvas)
            {
                Log.W($"You can only do this operation under a {nameof(Canvas)}.");
                return;
            }
            var parent = parentGameObject.transform;

            CreateScrollView(
                "Vertical Scroll View",
                parent,
                new Vector2(200f, 200f),
                20f,
                300f,
                RectTransform.Axis.Vertical
            );
        }

        private static void CreateScrollView(
            string             name,
            Transform          parent,
            Vector2            size,
            float              scrollbarThickness,
            float              contentSize,
            RectTransform.Axis axis)
        {
            var gameObject = new GameObject(name);
            var transform  = gameObject.AddComponent<RectTransform>();
            transform.SetParent(parent, false);
            transform.localPosition    = Vector3.zero;
            transform.localRotation    = Quaternion.identity;
            transform.localScale       = Vector3.one;
            transform.anchorMin        = new Vector2(0.5f, 0.5f);
            transform.anchorMax        = new Vector2(0.5f, 0.5f);
            transform.anchoredPosition = Vector2.zero;
            transform.sizeDelta        = size;
            transform.pivot            = new Vector2(0.5f, 0.5f);

            Graphic graphic = gameObject.AddComponent<Block>();
            graphic.color = new Color(1f, 1f, 1f, 0.125f);

            var inactiveContainer = CreateInactiveContainer(transform);

            var viewportTransform = CreateViewport(transform, scrollbarThickness, axis);

            var (contentTransform, contentLayoutGroup) = CreateContent(viewportTransform, contentSize, axis);

            var leadingPlaceholder = CreatePlaceholder(
                "Leading Placeholder",
                contentTransform,
                RectTransform.Axis.Vertical
            );

            var trailingPlaceholder = CreatePlaceholder(
                "Trailing Placeholder",
                contentTransform,
                RectTransform.Axis.Vertical
            );

            var scrollbar = CreateScrollbar(transform, scrollbarThickness, axis);
            scrollbar.gameObject.SetActive(false);

            var scrollRect = gameObject.AddComponent<ScrollRect>();
            scrollRect.content = contentTransform;
            switch (axis)
            {
                case RectTransform.Axis.Horizontal:
                    scrollRect.horizontal        = true;
                    scrollRect.vertical          = false;
                    scrollRect.scrollSensitivity = -32f;
                    break;
                case RectTransform.Axis.Vertical:
                    scrollRect.horizontal        = false;
                    scrollRect.vertical          = true;
                    scrollRect.scrollSensitivity = 32f;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(axis), axis, null);
            }
            scrollRect.viewport = viewportTransform;
            // switch (axis)
            // {
            //     case RectTransform.Axis.Horizontal:
            //         scrollRect.horizontalScrollbar = scrollbar;
            //         break;
            //     case RectTransform.Axis.Vertical:
            //         scrollRect.verticalScrollbar = scrollbar;
            //         break;
            //     default:
            //         throw new ArgumentOutOfRangeException(nameof(axis), axis, null);
            // }

            ScrollView scrollView = axis switch
            {
                RectTransform.Axis.Horizontal => gameObject.AddComponent<HorizontalScrollView>(),
                RectTransform.Axis.Vertical   => gameObject.AddComponent<VerticalScrollView>(),
                _                             => throw new ArgumentOutOfRangeException(nameof(axis), axis, null)
            };
            scrollView.scrollRect          = scrollRect;
            scrollView.viewport            = viewportTransform;
            scrollView.inactiveContainer   = inactiveContainer;
            scrollView.content             = contentTransform;
            scrollView.leadingPlaceholder  = leadingPlaceholder;
            scrollView.trailingPlaceholder = trailingPlaceholder;
            scrollView.contentLayoutGroup  = contentLayoutGroup;
            scrollView.scrollbar           = scrollbar;
        }

        private static Transform CreateInactiveContainer(Transform parent)
        {
            var gameObject = new GameObject("Inactive Container");
            gameObject.SetActive(false);
            var transform = gameObject.AddComponent<RectTransform>();
            transform.SetParent(parent, false);
            transform.localPosition    = Vector3.zero;
            transform.localRotation    = Quaternion.identity;
            transform.localScale       = Vector3.one;
            transform.anchorMin        = Vector2.zero;
            transform.anchorMax        = Vector2.one;
            transform.anchoredPosition = Vector2.zero;
            transform.sizeDelta        = Vector2.zero;
            transform.pivot            = new Vector2(0.5f, 0.5f);

            return transform;
        }

        private static RectTransform CreateViewport(Transform parent, float padding, RectTransform.Axis axis)
        {
            var gameObject = new GameObject("Viewport");
            var transform  = gameObject.AddComponent<RectTransform>();
            transform.SetParent(parent, false);
            transform.localPosition    = Vector3.zero;
            transform.localRotation    = Quaternion.identity;
            transform.localScale       = Vector3.one;
            transform.anchorMin        = Vector2.zero;
            transform.anchorMax        = Vector2.one;
            transform.anchoredPosition = Vector2.zero;
            transform.sizeDelta = axis switch
            {
                RectTransform.Axis.Horizontal => new Vector2(0f,       -padding),
                RectTransform.Axis.Vertical   => new Vector2(-padding, 0f),
                _                             => throw new ArgumentOutOfRangeException(nameof(axis), axis, null)
            };
            transform.pivot = new Vector2(0f, 1f);

            gameObject.AddComponent<Block>();

            var mask = gameObject.AddComponent<Mask>();
            mask.showMaskGraphic = false;

            return transform;
        }

        private static (RectTransform, HorizontalOrVerticalLayoutGroup) CreateContent(
            Transform          parent,
            float              size,
            RectTransform.Axis axis)
        {
            var gameObject = new GameObject("Content");
            var transform  = gameObject.AddComponent<RectTransform>();
            transform.SetParent(parent, false);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale    = Vector3.one;
            switch (axis)
            {
                case RectTransform.Axis.Horizontal:
                    transform.anchorMin = new Vector2(0f, 0f);
                    transform.anchorMax = new Vector2(0f, 1f);
                    break;
                case RectTransform.Axis.Vertical:
                    transform.anchorMin = new Vector2(0f, 1f);
                    transform.anchorMax = new Vector2(1f, 1f);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(axis), axis, null);
            }
            transform.anchoredPosition = Vector2.zero;
            transform.sizeDelta = axis switch
            {
                RectTransform.Axis.Horizontal => new Vector2(size, 0f),
                RectTransform.Axis.Vertical   => new Vector2(0f,   size),
                _                             => throw new ArgumentOutOfRangeException(nameof(axis), axis, null)
            };
            transform.pivot = new Vector2(0f, 1f);

            HorizontalOrVerticalLayoutGroup horizontalOrVerticalLayoutGroup = axis switch
            {
                RectTransform.Axis.Horizontal => gameObject.AddComponent<HorizontalLayoutGroup>(),
                RectTransform.Axis.Vertical   => gameObject.AddComponent<VerticalLayoutGroup>(),
                _                             => throw new ArgumentOutOfRangeException(nameof(axis), axis, null)
            };
            horizontalOrVerticalLayoutGroup.childControlWidth      = true;
            horizontalOrVerticalLayoutGroup.childControlHeight     = true;
            horizontalOrVerticalLayoutGroup.childForceExpandWidth  = false;
            horizontalOrVerticalLayoutGroup.childForceExpandHeight = false;

            return (transform, horizontalOrVerticalLayoutGroup);
        }

        private static LayoutElement CreatePlaceholder(string name, Transform parent, RectTransform.Axis axis)
        {
            var gameObject = new GameObject(name);
            gameObject.SetActive(false);
            var transform = gameObject.AddComponent<RectTransform>();
            transform.SetParent(parent, false);
            transform.localPosition    = Vector3.zero;
            transform.localRotation    = Quaternion.identity;
            transform.localScale       = Vector3.one;
            transform.anchorMin        = new Vector2(0f, 1f);
            transform.anchorMax        = new Vector2(0f, 1f);
            transform.anchoredPosition = Vector2.zero;
            transform.sizeDelta        = Vector2.zero;
            transform.pivot            = new Vector2(0f, 1f);

            var layoutElement = gameObject.AddComponent<LayoutElement>();
            switch (axis)
            {
                case RectTransform.Axis.Horizontal:
                    layoutElement.minWidth = 0f;
                    break;
                case RectTransform.Axis.Vertical:
                    layoutElement.minHeight = 0f;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(axis), axis, null);
            }

            return layoutElement;
        }

        private static Scrollbar CreateScrollbar(Transform parent, float thickness, RectTransform.Axis axis)
        {
            var gameObject = new GameObject("Scrollbar");
            var transform  = gameObject.AddComponent<RectTransform>();
            transform.SetParent(parent, false);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale    = Vector3.one;
            switch (axis)
            {
                case RectTransform.Axis.Horizontal:
                    transform.anchorMin        = new Vector2(0f, 0f);
                    transform.anchorMax        = new Vector2(1f, 0f);
                    transform.anchoredPosition = Vector2.zero;
                    transform.sizeDelta        = new Vector2(0f, thickness);
                    transform.pivot            = new Vector2(0f, 0f);
                    break;
                case RectTransform.Axis.Vertical:
                    transform.anchorMin        = new Vector2(1f, 0f);
                    transform.anchorMax        = new Vector2(1f, 1f);
                    transform.anchoredPosition = Vector2.zero;
                    transform.sizeDelta        = new Vector2(thickness, 0f);
                    transform.pivot            = new Vector2(1f,        1f);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(axis), axis, null);
            }

            var roundedRectangle = gameObject.AddComponent<RoundedRectangle>();
            roundedRectangle.color                             = new Color(1f, 1f, 1f, 0.125f);
            roundedRectangle.topLeftCornerRadiusNormalized     = true;
            roundedRectangle.topLeftCornerRadius               = 1f;
            roundedRectangle.topRightCornerRadiusNormalized    = true;
            roundedRectangle.topRightCornerRadius              = 1f;
            roundedRectangle.bottomLeftCornerRadiusNormalized  = true;
            roundedRectangle.bottomLeftCornerRadius            = 1f;
            roundedRectangle.bottomRightCornerRadiusNormalized = true;
            roundedRectangle.bottomRightCornerRadius           = 1f;

            var scrollbar = gameObject.AddComponent<Scrollbar>();
            scrollbar.direction = axis switch
            {
                RectTransform.Axis.Horizontal => Scrollbar.Direction.LeftToRight,
                RectTransform.Axis.Vertical   => Scrollbar.Direction.BottomToTop,
                _                             => throw new ArgumentOutOfRangeException(nameof(axis), axis, null)
            };

            var slidingArea = CreateScrollbarSlidingArea(transform, thickness, axis);
            var (handleImage, handleTransform) = CreateScrollbarHandle(slidingArea, thickness, axis);

            scrollbar.targetGraphic = handleImage;
            scrollbar.handleRect    = handleTransform;

            scrollbar.SetValueWithoutNotify(
                axis switch
                {
                    RectTransform.Axis.Horizontal => 0f,
                    RectTransform.Axis.Vertical   => 1f,
                    _                             => throw new ArgumentOutOfRangeException(nameof(axis), axis, null)
                }
            );

            return scrollbar;

            static Transform CreateScrollbarSlidingArea(
                Transform          parent,
                float              scrollbarThickness,
                RectTransform.Axis axis)
            {
                var gameObject = new GameObject("Sliding Area");
                var transform  = gameObject.AddComponent<RectTransform>();
                transform.SetParent(parent, false);
                transform.localPosition    = Vector3.zero;
                transform.localRotation    = Quaternion.identity;
                transform.localScale       = Vector3.one;
                transform.anchorMin        = Vector2.zero;
                transform.anchorMax        = Vector2.one;
                transform.anchoredPosition = Vector2.zero;
                transform.sizeDelta = axis switch
                {
                    RectTransform.Axis.Horizontal => new Vector2(-scrollbarThickness, 0f),
                    RectTransform.Axis.Vertical   => new Vector2(0f,                  -scrollbarThickness),
                    _                             => throw new ArgumentOutOfRangeException(nameof(axis), axis, null)
                };

                return transform;
            }

            static (Graphic, RectTransform) CreateScrollbarHandle(
                Transform          parent,
                float              scrollbarThickness,
                RectTransform.Axis axis)
            {
                var gameObject = new GameObject("Handle");
                var transform  = gameObject.AddComponent<RectTransform>();
                transform.SetParent(parent, false);
                transform.localPosition    = Vector3.zero;
                transform.localRotation    = Quaternion.identity;
                transform.localScale       = Vector3.one;
                transform.anchorMin        = Vector2.zero;
                transform.anchorMax        = Vector2.one;
                transform.anchoredPosition = Vector2.zero;
                transform.sizeDelta = axis switch
                {
                    RectTransform.Axis.Horizontal => new Vector2(scrollbarThickness, 0f),
                    RectTransform.Axis.Vertical   => new Vector2(0f,                 scrollbarThickness),
                    _                             => throw new ArgumentOutOfRangeException(nameof(axis), axis, null)
                };

                var roundedRectangle = gameObject.AddComponent<RoundedRectangle>();
                roundedRectangle.topLeftCornerRadiusNormalized     = true;
                roundedRectangle.topLeftCornerRadius               = 1f;
                roundedRectangle.topRightCornerRadiusNormalized    = true;
                roundedRectangle.topRightCornerRadius              = 1f;
                roundedRectangle.bottomLeftCornerRadiusNormalized  = true;
                roundedRectangle.bottomLeftCornerRadius            = 1f;
                roundedRectangle.bottomRightCornerRadiusNormalized = true;
                roundedRectangle.bottomRightCornerRadius           = 1f;

                return (roundedRectangle, transform);
            }
        }
    }
}
