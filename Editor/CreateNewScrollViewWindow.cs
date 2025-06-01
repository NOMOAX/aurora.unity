using System;
using Aurora.Unity;
using Aurora.Unity.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Aurora.UnityEditor
{
    internal sealed class CreateNewScrollViewWindow : EditorWindow
    {
        private const string WindowTitle = "Create New ScrollView";

        private const float WindowWidth = 400;

        private static readonly Vector2 DefaultSize = new(200, 200);

        private const float ContentSizeMultiplier = 1.5f;

        private const float DefaultScrollbarThickness = 200 * DefaultScrollbarThicknessMultiplier;

        private const float DefaultScrollbarThicknessMultiplier = 0.1f;

        private const float MinScrollbarThicknessMultiplier = 0f;

        private const float MaxScrollbarThicknessMultiplier = 0.25f;

        private static readonly GUIContent[] SizeSubLabels = { new("X"), new("Y") };

        private static readonly GUIContent[] ScrollbarPositionLabelsForHorizontalScrollView =
        {
            new("No Scrollbar"), new("Top"), new("Bottom")
        };

        private static readonly GUIContent[] ScrollbarPositionLabelsForVerticalScrollView =
        {
            new("No Scrollbar"), new("Left"), new("Right")
        };

        private RectTransform _parent;

        private RectTransform.Axis _axis = RectTransform.Axis.Vertical;

        private Vector2 _size = DefaultSize;

        private float _scrollbarThickness = DefaultScrollbarThickness;

        private ScrollbarPosition _scrollbarPosition = ScrollbarPosition.RightOrBottom;

        private readonly float[] _sizeValues = new float[2];

        private readonly Func<ScrollbarPosition> _scrollbarPositionGetter;

        private readonly Action<ScrollbarPosition> _scrollbarPositionSetter;

        public CreateNewScrollViewWindow()
        {
            _scrollbarPositionGetter = () => _scrollbarPosition;
            _scrollbarPositionSetter = scrollbarPosition => _scrollbarPosition = scrollbarPosition;
        }

        internal static void OpenWindow(GameObject parentGameObject)
        {
            var scrollViewCreator = GetWindow<CreateNewScrollViewWindow>(true, WindowTitle);
            scrollViewCreator._parent = parentGameObject ? parentGameObject.transform as RectTransform : null;
            var windowHeight = IsParentValid(scrollViewCreator._parent) ? 123 : 163;
            var windowSize   = new Vector2(WindowWidth, windowHeight);
            scrollViewCreator.minSize = windowSize;
            scrollViewCreator.maxSize = windowSize;
        }

        private static bool IsParentValid(RectTransform parent)
        {
            return parent && parent.gameObject.GetComponentInParent<Canvas>();
        }

        private void OnGUI()
        {
            using (var verticalScope = new EditorGUILayout.VerticalScope())
            {
                DrawParent(ref _parent);
                DrawAxis(ref _axis);
                DrawScrollViewSize(ref _size, _sizeValues);
                DrawScrollbar(_scrollbarPositionGetter, _scrollbarPositionSetter, _axis);
                DrawScrollbarThickness(ref _scrollbarThickness, _scrollbarPosition, _size, _axis);

                using (new EditorGUILayout.HorizontalScope())
                {
                    using (new EditorGUI.DisabledScope(!IsParentValid(_parent)))
                    {
                        if (GUILayout.Button("Create and Continue"))
                        {
                            var scrollView = CreateScrollView();
                            Selection.activeGameObject = scrollView.gameObject;
                        }
                        if (GUILayout.Button("Create"))
                        {
                            var scrollView = CreateScrollView();
                            Selection.activeGameObject = scrollView.gameObject;
                            Close();
                            return;
                        }
                    }
                }

                // 重设窗口的高度
                if (Event.current.type == EventType.Repaint)
                {
                    var marginTop     = verticalScope.rect.y; // 顶部外边距
                    var contentHeight = verticalScope.rect.height;
                    var windowHeight  = marginTop + contentHeight + marginTop /* 底部外边距（很可能与顶部外边距相等） */;
                    var windowSize    = new Vector2(WindowWidth, windowHeight);
                    minSize = maxSize = windowSize;
                }
            }
        }

        private static void DrawParent(ref RectTransform parent)
        {
            parent = (RectTransform) EditorGUILayout.ObjectField("Parent", parent, typeof(RectTransform), true);
            if (!IsParentValid(parent))
            {
                EditorGUILayout.HelpBox("Select a canvas element as parent!", MessageType.Error);
            }
            // parent 已被销毁，释放引用
            if (parent is not null && !UnityEngineObjectUtility.IsAlive(parent))
            {
                parent = null;
            }
        }

        private static void DrawAxis(ref RectTransform.Axis axis)
        {
            axis = (RectTransform.Axis) EditorGUILayout.EnumPopup("Axis", axis);
        }

        private static void DrawScrollViewSize(ref Vector2 size, float[] sizeValues)
        {
            for (var i = 0; i < 2; i++)
            {
                sizeValues[i] = size[i];
            }
            var rect = EditorGUI.PrefixLabel(EditorGUILayout.GetControlRect(), EditorGUIUtility.TrTextContent("Size"));
            EditorGUI.MultiFloatField(rect, SizeSubLabels, sizeValues);
            for (var i = 0; i < 2; i++)
            {
                size[i] = sizeValues[i] switch
                {
                    float.NaN              => DefaultSize[i],
                    < 0                    => 0,
                    float.PositiveInfinity => float.MaxValue,
                    _                      => sizeValues[i]
                };
            }
        }

        private static void DrawScrollbar(
            Func<ScrollbarPosition>   scrollbarPositionGetter,
            Action<ScrollbarPosition> scrollbarPositionSetter,
            RectTransform.Axis        axis)
        {
            var rect = EditorGUI.PrefixLabel(
                EditorGUILayout.GetControlRect(),
                EditorGUIUtility.TrTextContent("Scrollbar Position")
            );

            var guiContents = axis switch
            {
                RectTransform.Axis.Horizontal => ScrollbarPositionLabelsForHorizontalScrollView,
                RectTransform.Axis.Vertical   => ScrollbarPositionLabelsForVerticalScrollView,
                _                             => throw new ArgumentOutOfRangeException(nameof(axis), axis, null)
            };

            var scrollbarPosition = scrollbarPositionGetter();
            if (EditorGUI.DropdownButton(rect, guiContents[(int) scrollbarPosition], FocusType.Keyboard))
            {
                var genericMenu = new GenericMenu();
                foreach (var value in EnumUtility<ScrollbarPosition>.Values)
                {
                    genericMenu.AddItem(
                        guiContents[(int) value],
                        value == scrollbarPosition,
                        OnScrollPositionDropdownItemSelected,
                        Tuple.Create(value, scrollbarPositionGetter, scrollbarPositionSetter)
                    );
                }
                genericMenu.DropDown(rect);

                static void OnScrollPositionDropdownItemSelected(object userData)
                {
                    var (scrollbarPosition, scrollbarPositionGetter, scrollbarPositionSetter) =
                        (Tuple<ScrollbarPosition, Func<ScrollbarPosition>, Action<ScrollbarPosition>>) userData;
                    var oldScrollbarPosition = scrollbarPositionGetter();
                    if (oldScrollbarPosition == scrollbarPosition)
                    {
                        return;
                    }
                    scrollbarPositionSetter(scrollbarPosition);
                }
            }
        }

        private static void DrawScrollbarThickness(
            ref float          scrollbarThickness,
            ScrollbarPosition  scrollbarPosition,
            Vector2            size,
            RectTransform.Axis axis)
        {
            if (scrollbarPosition is ScrollbarPosition.NoScrollbar)
            {
                scrollbarThickness = 0;
            }
            else
            {
                var sizeAloneOtherAxis        = size[1 - (int) axis];
                var defaultScrollbarThickness = sizeAloneOtherAxis * DefaultScrollbarThicknessMultiplier;
                var minScrollbarThickness     = sizeAloneOtherAxis * MinScrollbarThicknessMultiplier;
                var maxScrollbarThickness     = sizeAloneOtherAxis * MaxScrollbarThicknessMultiplier;
                scrollbarThickness = scrollbarThickness is float.NaN
                                         ? defaultScrollbarThickness
                                         : Mathf.Clamp(
                                             scrollbarThickness,
                                             minScrollbarThickness,
                                             maxScrollbarThickness
                                         );
                scrollbarThickness = EditorGUILayout.Slider(
                    "Slider Thickness",
                    scrollbarThickness,
                    minScrollbarThickness,
                    maxScrollbarThickness
                );
                scrollbarThickness = scrollbarThickness is float.NaN
                                         ? defaultScrollbarThickness
                                         : Mathf.Clamp(
                                             scrollbarThickness,
                                             minScrollbarThickness,
                                             maxScrollbarThickness
                                         );
            }
        }

        private ScrollView CreateScrollView()
        {
            return CreateScrollView(
                _parent,
                _size,
                _size[(int) _axis] * ContentSizeMultiplier,
                _scrollbarPosition,
                _scrollbarThickness,
                _axis
            );
        }

        private static ScrollView CreateScrollView(
            RectTransform      parent,
            Vector2            size,
            float              contentSize,
            ScrollbarPosition  scrollbarPosition,
            float              scrollbarThickness,
            RectTransform.Axis axis)
        {
            return CreateScrollView(
                "Scroll View",
                parent,
                size,
                contentSize,
                scrollbarPosition,
                scrollbarThickness,
                axis
            );
        }

        private static ScrollView CreateScrollView(
            string             name,
            RectTransform      parent,
            Vector2            size,
            float              contentSize,
            ScrollbarPosition  scrollbarPosition,
            float              scrollbarThickness,
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
            graphic.color = new Color(1, 1, 1, 0.125f);

            var inactiveContainer = CreateInactiveContainer(transform);

            var viewportTransform = CreateViewport(transform, scrollbarPosition, scrollbarThickness, axis);

            var (contentTransform, contentLayoutGroup) = CreateContent(
                viewportTransform,
                contentSize,
                scrollbarPosition,
                axis
            );

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

            var scrollbar = CreateScrollbar(transform, scrollbarPosition, scrollbarThickness, axis);
            if (scrollbar)
            {
                scrollbar.gameObject.SetActive(false);
            }

            var scrollRect = gameObject.AddComponent<ScrollRect>();
            scrollRect.content = contentTransform;
            switch (axis)
            {
                case RectTransform.Axis.Horizontal:
                    scrollRect.horizontal        = true;
                    scrollRect.vertical          = false;
                    scrollRect.scrollSensitivity = -32;
                    break;
                case RectTransform.Axis.Vertical:
                    scrollRect.horizontal        = false;
                    scrollRect.vertical          = true;
                    scrollRect.scrollSensitivity = 32;
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

            return scrollView;
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

        private static RectTransform CreateViewport(
            Transform          parent,
            ScrollbarPosition  scrollbarPosition,
            float              scrollbarThickness,
            RectTransform.Axis axis)
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
                RectTransform.Axis.Horizontal => new Vector2(0,                   -scrollbarThickness),
                RectTransform.Axis.Vertical   => new Vector2(-scrollbarThickness, 0),
                _                             => throw new ArgumentOutOfRangeException(nameof(axis), axis, null)
            };
            transform.pivot = scrollbarPosition switch
            {
                ScrollbarPosition.NoScrollbar or ScrollbarPosition.RightOrBottom => new Vector2(0, 1),
                ScrollbarPosition.LeftOrTop => axis switch
                {
                    RectTransform.Axis.Horizontal => new Vector2(0, 0),
                    RectTransform.Axis.Vertical   => new Vector2(1, 1),
                    _                             => throw new ArgumentOutOfRangeException(nameof(axis), axis, null)
                },
                _ => throw new ArgumentOutOfRangeException(nameof(scrollbarPosition), scrollbarPosition, null)
            };

            gameObject.AddComponent<Block>();

            var mask = gameObject.AddComponent<Mask>();
            mask.showMaskGraphic = false;

            return transform;
        }

        private static (RectTransform, HorizontalOrVerticalLayoutGroup) CreateContent(
            Transform          parent,
            float              size,
            ScrollbarPosition  scrollbarPosition,
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
                    transform.anchorMin = new Vector2(0, 0);
                    transform.anchorMax = new Vector2(0, 1);
                    break;
                case RectTransform.Axis.Vertical:
                    transform.anchorMin = new Vector2(0, 1);
                    transform.anchorMax = new Vector2(1, 1);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(axis), axis, null);
            }
            transform.anchoredPosition = Vector2.zero;
            transform.sizeDelta = axis switch
            {
                RectTransform.Axis.Horizontal => new Vector2(size, 0),
                RectTransform.Axis.Vertical   => new Vector2(0,    size),
                _                             => throw new ArgumentOutOfRangeException(nameof(axis), axis, null)
            };
            transform.pivot = new Vector2(0, 1);

            HorizontalOrVerticalLayoutGroup horizontalOrVerticalLayoutGroup = axis switch
            {
                RectTransform.Axis.Horizontal => gameObject.AddComponent<HorizontalLayoutGroup>(),
                RectTransform.Axis.Vertical   => gameObject.AddComponent<VerticalLayoutGroup>(),
                _                             => throw new ArgumentOutOfRangeException(nameof(axis), axis, null)
            };
            horizontalOrVerticalLayoutGroup.childAlignment = scrollbarPosition switch
            {
                ScrollbarPosition.NoScrollbar or ScrollbarPosition.RightOrBottom => TextAnchor.UpperLeft,
                ScrollbarPosition.LeftOrTop => axis switch
                {
                    RectTransform.Axis.Horizontal => TextAnchor.LowerLeft,
                    RectTransform.Axis.Vertical   => TextAnchor.UpperRight,
                    _                             => throw new ArgumentOutOfRangeException(nameof(axis), axis, null)
                },
                _ => throw new ArgumentOutOfRangeException(nameof(scrollbarPosition), scrollbarPosition, null)
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
            transform.anchorMin        = new Vector2(0, 1);
            transform.anchorMax        = new Vector2(0, 1);
            transform.anchoredPosition = Vector2.zero;
            transform.sizeDelta        = Vector2.zero;
            transform.pivot            = new Vector2(0, 1);

            var layoutElement = gameObject.AddComponent<LayoutElement>();
            switch (axis)
            {
                case RectTransform.Axis.Horizontal:
                    layoutElement.minWidth = 0;
                    break;
                case RectTransform.Axis.Vertical:
                    layoutElement.minHeight = 0;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(axis), axis, null);
            }

            return layoutElement;
        }

        private static Scrollbar CreateScrollbar(
            Transform          parent,
            ScrollbarPosition  position,
            float              thickness,
            RectTransform.Axis axis)
        {
            if (position is ScrollbarPosition.NoScrollbar)
            {
                return null;
            }

            var gameObject = new GameObject("Scrollbar");
            var transform  = gameObject.AddComponent<RectTransform>();
            transform.SetParent(parent, false);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale    = Vector3.one;
            switch (position)
            {
                case ScrollbarPosition.LeftOrTop:
                    switch (axis)
                    {
                        case RectTransform.Axis.Horizontal:
                            transform.anchorMin        = new Vector2(0, 1);
                            transform.anchorMax        = new Vector2(1, 1);
                            transform.anchoredPosition = Vector2.zero;
                            transform.sizeDelta        = new Vector2(0, thickness);
                            transform.pivot            = new Vector2(0, 1);
                            break;
                        case RectTransform.Axis.Vertical:
                            transform.anchorMin        = new Vector2(0, 0);
                            transform.anchorMax        = new Vector2(0, 1);
                            transform.anchoredPosition = Vector2.zero;
                            transform.sizeDelta        = new Vector2(thickness, 0);
                            transform.pivot            = new Vector2(0,         1);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(axis), axis, null);
                    }
                    break;
                case ScrollbarPosition.RightOrBottom:
                    switch (axis)
                    {
                        case RectTransform.Axis.Horizontal:
                            transform.anchorMin        = new Vector2(0, 0);
                            transform.anchorMax        = new Vector2(1, 0);
                            transform.anchoredPosition = Vector2.zero;
                            transform.sizeDelta        = new Vector2(0, thickness);
                            transform.pivot            = new Vector2(0, 0);
                            break;
                        case RectTransform.Axis.Vertical:
                            transform.anchorMin        = new Vector2(1, 0);
                            transform.anchorMax        = new Vector2(1, 1);
                            transform.anchoredPosition = Vector2.zero;
                            transform.sizeDelta        = new Vector2(thickness, 0);
                            transform.pivot            = new Vector2(1,         1);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(axis), axis, null);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(position), position, null);
            }

            var roundedRectangle = gameObject.AddComponent<RoundedRectangle>();
            roundedRectangle.color                             = new Color(1, 1, 1, 0.125f);
            roundedRectangle.topLeftCornerRadiusNormalized     = true;
            roundedRectangle.topLeftCornerRadius               = 1;
            roundedRectangle.topRightCornerRadiusNormalized    = true;
            roundedRectangle.topRightCornerRadius              = 1;
            roundedRectangle.bottomLeftCornerRadiusNormalized  = true;
            roundedRectangle.bottomLeftCornerRadius            = 1;
            roundedRectangle.bottomRightCornerRadiusNormalized = true;
            roundedRectangle.bottomRightCornerRadius           = 1;

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
                    RectTransform.Axis.Horizontal => 0,
                    RectTransform.Axis.Vertical   => 1,
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
                    RectTransform.Axis.Horizontal => new Vector2(-scrollbarThickness, 0),
                    RectTransform.Axis.Vertical   => new Vector2(0,                   -scrollbarThickness),
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
                    RectTransform.Axis.Horizontal => new Vector2(scrollbarThickness, 0),
                    RectTransform.Axis.Vertical   => new Vector2(0,                  scrollbarThickness),
                    _                             => throw new ArgumentOutOfRangeException(nameof(axis), axis, null)
                };

                var roundedRectangle = gameObject.AddComponent<RoundedRectangle>();
                roundedRectangle.topLeftCornerRadiusNormalized     = true;
                roundedRectangle.topLeftCornerRadius               = 1;
                roundedRectangle.topRightCornerRadiusNormalized    = true;
                roundedRectangle.topRightCornerRadius              = 1;
                roundedRectangle.bottomLeftCornerRadiusNormalized  = true;
                roundedRectangle.bottomLeftCornerRadius            = 1;
                roundedRectangle.bottomRightCornerRadiusNormalized = true;
                roundedRectangle.bottomRightCornerRadius           = 1;

                return (roundedRectangle, transform);
            }
        }

        private enum ScrollbarPosition
        {
            NoScrollbar,

            LeftOrTop,

            RightOrBottom
        }
    }
}
