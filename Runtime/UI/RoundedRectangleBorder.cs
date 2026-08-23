using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Aurora.Unity.UI
{
    /// <summary>
    /// A rounded rectangle border.
    /// </summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class RoundedRectangleBorder : MaskableGraphic, ILayoutElement, ICanvasRaycastFilter
    {
        [SerializeField]
        internal Texture texture;

        [SerializeField]
        [Range(SegmentsMinValue, SegmentsMaxValue)]
        internal int segments = 7;

        [SerializeField]
        internal bool topLeftCornerRadiusNormalized;

        [SerializeField]
        [Min(0)]
        internal float topLeftCornerRadius = 8;

        [SerializeField]
        internal bool topRightCornerRadiusNormalized;

        [SerializeField]
        [Min(0)]
        internal float topRightCornerRadius = 8;

        [SerializeField]
        internal bool bottomLeftCornerRadiusNormalized;

        [SerializeField]
        [Min(0)]
        internal float bottomLeftCornerRadius = 8;

        [SerializeField]
        internal bool bottomRightCornerRadiusNormalized;

        [SerializeField]
        [Min(0)]
        internal float bottomRightCornerRadius = 8;

        [SerializeField]
        internal bool thicknessNormalized;

        [SerializeField]
        [Min(0)]
        internal float thickness = 4;

        [SerializeField]
        internal bool useExactRaycastLocation;

        private readonly List<Vector2> _outerPositions = new();

        private readonly List<Vector2> _innerPositions = new();

        private const int SegmentsMinValue = 1;

        private const int SegmentsMaxValue = UnityUtility.VertexCountPerMeshMaxValue / 8 - 1;

        private const int OuterPositionCountMinValue = (SegmentsMinValue + 1) * 4;

        private const int InnerPositionCountMinValue = (SegmentsMinValue + 1) * 4;

        private RoundedRectangleBorder()
        {
            useLegacyMeshGeneration = false;
        }

        /// <inheritdoc />
        public override Texture mainTexture
        {
            get
            {
                if (texture)
                {
                    return texture;
                }
                return material && material.mainTexture ? material.mainTexture : s_WhiteTexture;
            }
        }

        /// <summary>
        /// The texture.
        /// </summary>
        public Texture Texture
        {
            get => texture;
            set
            {
                if (texture == value)
                {
                    return;
                }
                texture = value;
                SetVerticesDirty();
                SetMaterialDirty();
            }
        }

        /// <summary>
        /// The segment count.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is outside the range [1, 8123].</exception>
        public int Segments
        {
            get => segments;
            set
            {
                if (value is < SegmentsMinValue or > SegmentsMaxValue)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
                }
                if (segments == value)
                {
                    return;
                }
                segments = value;
                SetVerticesDirty();
            }
        }

        /// <summary>
        /// Whether to use a normalized length to represent the top-left corner radius.
        /// </summary>
        public bool TopLeftCornerRadiusNormalized
        {
            get => topLeftCornerRadiusNormalized;
            set
            {
                if (topLeftCornerRadiusNormalized == value)
                {
                    return;
                }
                SetNormalized(ref topLeftCornerRadiusNormalized, ref topLeftCornerRadius);
            }
        }

        /// <summary>
        /// The top-left corner radius.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is not a number, or <paramref name="value"/> is less than 0, or <paramref name="value"/> is greater than 1 when <see cref="TopLeftCornerRadiusNormalized"/> is <see langword="true"/>.</exception>
        public float TopLeftCornerRadius
        {
            get => topLeftCornerRadius;
            set
            {
                if (value is float.NaN or < 0 || topLeftCornerRadiusNormalized && value > 1)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
                }
                if (topLeftCornerRadius.Equals(value))
                {
                    return;
                }
                topLeftCornerRadius = value;
                SetVerticesDirty();
            }
        }

        /// <summary>
        /// Whether to use a normalized length to represent the top-right corner radius.
        /// </summary>
        public bool TopRightCornerRadiusNormalized
        {
            get => topRightCornerRadiusNormalized;
            set
            {
                if (topRightCornerRadiusNormalized == value)
                {
                    return;
                }
                SetNormalized(ref topRightCornerRadiusNormalized, ref topRightCornerRadius);
            }
        }

        /// <summary>
        /// The top-right corner radius.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is not a number, or <paramref name="value"/> is less than 0, or <paramref name="value"/> is greater than 1 when <see cref="TopRightCornerRadiusNormalized"/> is <see langword="true"/>.</exception>
        public float TopRightCornerRadius
        {
            get => topRightCornerRadius;
            set
            {
                if (value is float.NaN or < 0 || topRightCornerRadiusNormalized && value > 1)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
                }
                if (topRightCornerRadius.Equals(value))
                {
                    return;
                }
                topRightCornerRadius = value;
                SetVerticesDirty();
            }
        }

        /// <summary>
        /// Whether to use a normalized length to represent the bottom-left corner radius.
        /// </summary>
        public bool BottomLeftCornerRadiusNormalized
        {
            get => bottomLeftCornerRadiusNormalized;
            set
            {
                if (bottomLeftCornerRadiusNormalized == value)
                {
                    return;
                }
                SetNormalized(ref bottomLeftCornerRadiusNormalized, ref bottomLeftCornerRadius);
            }
        }

        /// <summary>
        /// The bottom-left corner radius.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is not a number, or <paramref name="value"/> is less than 0, or <paramref name="value"/> is greater than 1 when <see cref="BottomLeftCornerRadiusNormalized"/> is <see langword="true"/>.</exception>
        public float BottomLeftCornerRadius
        {
            get => bottomLeftCornerRadius;
            set
            {
                if (value is float.NaN or < 0 || bottomLeftCornerRadiusNormalized && value > 1)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
                }
                if (bottomLeftCornerRadius.Equals(value))
                {
                    return;
                }
                bottomLeftCornerRadius = value;
                SetVerticesDirty();
            }
        }

        /// <summary>
        /// Whether to use a normalized length to represent the bottom-right corner radius.
        /// </summary>
        public bool BottomRightCornerRadiusNormalized
        {
            get => bottomRightCornerRadiusNormalized;
            set
            {
                if (bottomRightCornerRadiusNormalized == value)
                {
                    return;
                }
                SetNormalized(ref bottomRightCornerRadiusNormalized, ref bottomRightCornerRadius);
            }
        }

        /// <summary>
        /// The bottom-right corner radius.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is not a number, or <paramref name="value"/> is less than 0, or <paramref name="value"/> is greater than 1 when <see cref="BottomRightCornerRadiusNormalized"/> is <see langword="true"/>.</exception>
        public float BottomRightCornerRadius
        {
            get => bottomRightCornerRadius;
            set
            {
                if (value is float.NaN or < 0 || bottomRightCornerRadiusNormalized && value > 1)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
                }
                if (bottomRightCornerRadius.Equals(value))
                {
                    return;
                }
                bottomRightCornerRadius = value;
                SetVerticesDirty();
            }
        }

        /// <summary>
        /// Whether to use a normalized length to represent the thickness.
        /// </summary>
        public bool ThicknessNormalized
        {
            get => thicknessNormalized;
            set
            {
                if (thicknessNormalized == value)
                {
                    return;
                }
                SetNormalized(ref thicknessNormalized, ref thickness);
            }
        }

        /// <summary>
        /// The thickness.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is not a number, or <paramref name="value"/> is less than 0, or <paramref name="value"/> is greater than 1 when <see cref="ThicknessNormalized"/> is <see langword="true"/>.</exception>
        public float Thickness
        {
            get => thickness;
            set
            {
                if (value is float.NaN or < 0 || thicknessNormalized && value > 1)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
                }
                if (thickness.Equals(value))
                {
                    return;
                }
                thickness = value;
                SetVerticesDirty();
            }
        }

        /// <summary>
        /// Whether to use an exact click area.
        /// </summary>
        public bool UseExactRaycastLocation
        {
            get => useExactRaycastLocation;
            set
            {
                if (useExactRaycastLocation == value)
                {
                    return;
                }
                useExactRaycastLocation = value;
                SetVerticesDirty();
            }
        }

        private bool IsValid => Mathf.Clamp(segments, SegmentsMinValue, SegmentsMaxValue) == segments &&
                                IsValueValid(topLeftCornerRadiusNormalized,    topLeftCornerRadius) &&
                                IsValueValid(topRightCornerRadiusNormalized,   topRightCornerRadius) &&
                                IsValueValid(bottomLeftCornerRadiusNormalized, bottomLeftCornerRadius) && IsValueValid(
                                    bottomRightCornerRadiusNormalized,
                                    bottomRightCornerRadius
                                ) && IsValueValid(thicknessNormalized, thickness);

        private void SetNormalized(ref bool normalized, ref float value)
        {
            var pixelAdjustedRect = GetPixelAdjustedRect();
            var halfMinSide       = Mathf.Min(pixelAdjustedRect.width, pixelAdjustedRect.height) * 0.5f;
            var newNormalized     = !normalized;
            value = newNormalized switch
            {
                false => Mathf.Lerp(0, halfMinSide, value),
                true  => Mathf.InverseLerp(0, halfMinSide, value)
            };
            normalized = newNormalized;
            SetVerticesDirty();
        }

        private static bool IsValueValid(bool normalized, float value)
        {
            return normalized switch
            {
                false => value >= 0,
                true  => value is >= 0 and <= 1
            };
        }

        private static float GetActualValue(bool normalized, float value, float halfMinSide)
        {
            return normalized switch
            {
                false => Mathf.Min(value, halfMinSide),
                true  => value * halfMinSide
            };
        }

        /// <inheritdoc />
        public override void SetNativeSize()
        {
            rectTransform.anchorMin = rectTransform.anchorMax = Vector2.one * 0.5f;
            var t                                             = mainTexture;
            rectTransform.sizeDelta = new Vector2(t.width, t.height);
        }

        /// <inheritdoc />
        protected override unsafe void OnPopulateMesh(VertexHelper vh)
        {
            if (!IsValid)
            {
                base.OnPopulateMesh(vh);
                return;
            }

            _outerPositions.Clear();
            _innerPositions.Clear();

            // When the thickness is 0, the graphic disappears
            if (thickness == 0)
            {
                vh.Clear();
                TrimExcess();
                return;
            }

            var color32           = (Color32)color;
            var pixelAdjustedRect = GetPixelAdjustedRect();
            var halfMinSide       = Mathf.Min(pixelAdjustedRect.width, pixelAdjustedRect.height) * 0.5f;

            // Each corner radius
            var cornerRadii = new Vector4(
                GetActualValue(topRightCornerRadiusNormalized,    topRightCornerRadius,    halfMinSide),
                GetActualValue(topLeftCornerRadiusNormalized,     topLeftCornerRadius,     halfMinSide),
                GetActualValue(bottomLeftCornerRadiusNormalized,  bottomLeftCornerRadius,  halfMinSide),
                GetActualValue(bottomRightCornerRadiusNormalized, bottomRightCornerRadius, halfMinSide)
            );
            // The thickness
            var thickness1 = GetActualValue(thicknessNormalized, thickness, halfMinSide);

            vh.Clear();

            var center   = pixelAdjustedRect.center;
            var halfSize = pixelAdjustedRect.size * 0.5f;

            /*
             * The unit vectors of each quadrant, used in multiplication operations
             * The stackalloc keyword is used to ensure allocation only on the stack, improving performance
             */
            var multipliers = stackalloc Vector2[4]
            {
                Vector2.right + Vector2.up,  // top-right (quadrant I)
                Vector2.left + Vector2.up,   // top-left (quadrant II)
                Vector2.left + Vector2.down, // bottom-left (quadrant III)
                Vector2.right + Vector2.down // bottom-right (quadrant IV)
            };

            var stepAngle = 0.5f * Mathf.PI / segments;
            for (var i = 0; i < 4; i++)
            {
                var firstVertexIndex = segments * i;

                #region Outer layer

                var cornerRadius         = cornerRadii[i];
                var cornerCenterToCenter = (halfSize - Vector2.one * cornerRadius) * multipliers[i];

                #endregion

                #region Inner layer

                var innerCornerRadius = Mathf.Max(cornerRadius - thickness1, 0);
                var innerCornerCenterToCenter =
                    (halfSize - Vector2.one * Mathf.Max(cornerRadius, thickness1)) * multipliers[i];

                #endregion

                for (var j = 0; j < segments + 1; j++)
                {
                    var angle  = stepAngle * (firstVertexIndex + j);
                    var cosSin = UnityMath.CosSin(angle);

                    #region Outer layer

                    var positionToCornerCenter = cosSin * cornerRadius;
                    var position               = center + cornerCenterToCenter + positionToCornerCenter;
                    if (useExactRaycastLocation)
                    {
                        _outerPositions.Add(position);
                    }
                    vh.AddVert(position, color32, UnityMath.GetUV(pixelAdjustedRect, position));

                    #endregion

                    #region Inner layer

                    var innerPositionToInnerCornerCenter = cosSin * innerCornerRadius;
                    var innerPosition = center + innerCornerCenterToCenter + innerPositionToInnerCornerCenter;
                    if (useExactRaycastLocation)
                    {
                        _innerPositions.Add(innerPosition);
                    }
                    vh.AddVert(innerPosition, color32, UnityMath.GetUV(pixelAdjustedRect, innerPosition));

                    #endregion
                }
            }

            for (var index = 0; index < segments * 4 + 3; index++)
            {
                vh.AddTriangle(index * 2,     index * 2 + 1, index * 2 + 3);
                vh.AddTriangle(index * 2 + 3, index * 2 + 2, index * 2);
            }
            vh.AddTriangle(segments * 8 + 6, segments * 8 + 7, 1);
            vh.AddTriangle(1,                0,                segments * 8 + 6);

            TrimExcess();
        }

        private void TrimExcess()
        {
            _outerPositions.TrimExcess();
            _innerPositions.TrimExcess();
        }

        void ILayoutElement.CalculateLayoutInputHorizontal()
        {
        }

        void ILayoutElement.CalculateLayoutInputVertical()
        {
        }

        float ILayoutElement.minWidth => 0;

        float ILayoutElement.preferredWidth => texture ? texture.width : 0;

        float ILayoutElement.flexibleWidth => -1;

        float ILayoutElement.minHeight => 0;

        float ILayoutElement.preferredHeight => texture ? texture.height : 0;

        float ILayoutElement.flexibleHeight => -1;

        int ILayoutElement.layoutPriority => 0;

        bool ICanvasRaycastFilter.IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
        {
            if (!UnityEngine.RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    rectTransform,
                    screenPoint,
                    eventCamera,
                    out var localPoint
                ))
            {
                return false;
            }
            if (!useExactRaycastLocation)
            {
                return true;
            }
            if (thickness == 0)
            {
                return false;
            }
            if (_outerPositions.Count < OuterPositionCountMinValue ||
                _innerPositions.Count < InnerPositionCountMinValue)
            {
                return true;
            }
            return UnityMath.IsPointInsidePolygon(localPoint, _outerPositions) &&
                   !UnityMath.IsPointInsidePolygon(localPoint, _innerPositions);
        }

        /// <inheritdoc />
        protected override void OnDidApplyAnimationProperties()
        {
            SetMaterialDirty();
            SetVerticesDirty();
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            segments = Mathf.Clamp(segments, SegmentsMinValue, SegmentsMaxValue);
            CorrectValue(topLeftCornerRadiusNormalized,     ref topLeftCornerRadius);
            CorrectValue(topRightCornerRadiusNormalized,    ref topRightCornerRadius);
            CorrectValue(bottomLeftCornerRadiusNormalized,  ref bottomLeftCornerRadius);
            CorrectValue(bottomRightCornerRadiusNormalized, ref bottomRightCornerRadius);
            CorrectValue(thicknessNormalized,               ref thickness);
        }

        private static void CorrectValue(bool normalized, ref float value)
        {
            if (value is float.NaN)
            {
                value = 0;
                return;
            }
            if (IsValueValid(normalized, value))
            {
                return;
            }
            value = normalized switch
            {
                false => Mathf.Max(value, 0),
                true  => Mathf.Clamp01(value)
            };
        }
#endif

        protected override void OnDisable()
        {
            _outerPositions.Clear();
            _innerPositions.Clear();
            base.OnDisable();
        }
    }
}
