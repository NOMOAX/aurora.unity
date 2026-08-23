using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Aurora.Unity.UI
{
    /// <summary>
    /// A rounded rectangle.
    /// </summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class RoundedRectangle : MaskableGraphic, ILayoutElement, ICanvasRaycastFilter
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
        internal bool useExactRaycastLocation;

        private readonly List<Vector2> _positions = new();

        private const int SegmentsMinValue = 1;

        private const int SegmentsMaxValue = (UnityUtility.VertexCountPerMeshMaxValue - 1) / 4 - 1;

        private const int PositionCountMinValue = (SegmentsMinValue + 1) * 4;

        private RoundedRectangle()
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
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is outside the range [1, 16248].</exception>
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
                                );

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

        /// <summary>
        /// Gets the non-normalized, actually effective value.
        /// </summary>
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

            _positions.Clear();

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

            vh.Clear();

            var center   = pixelAdjustedRect.center;
            var halfSize = pixelAdjustedRect.size * 0.5f;

            // Add the center point
            vh.AddVert(center, color32, new Vector2(0.5f, 0.5f));

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

            // Each quadrant uses 0.5π
            var stepAngle = 0.5f * Mathf.PI / segments;
            // Add the points of the four quadrants
            for (var i = 0; i < 4; i++)
            {
                // The index of the first vertex of this quadrant (note that the last vertex of each quadrant and the first vertex of the next quadrant have the same angle, so these two vertices are treated as having the same index)
                var firstVertexIndex     = segments * i;
                // The corner radius
                var cornerRadius         = cornerRadii[i];
                // The relative position from the corner center to the center
                var cornerCenterToCenter = (halfSize - Vector2.one * cornerRadius) * multipliers[i];
                // Because each quadrant has segments edges, there are segments + 1 vertices
                for (var j = 0; j < segments + 1; j++)
                {
                    var angle                  = stepAngle * (firstVertexIndex + j);
                    // The relative position from this vertex to the corner center
                    var positionToCornerCenter = UnityMath.CosSin(angle) * cornerRadius;
                    var position               = center + cornerCenterToCenter + positionToCornerCenter;
                    if (useExactRaycastLocation)
                    {
                        _positions.Add(position);
                    }
                    vh.AddVert(position, color32, UnityMath.GetUV(pixelAdjustedRect, position));
                }
            }

            for (var index = 1; index < segments * 4 + 4; index++)
            {
                vh.AddTriangle(index, 0, index + 1);
            }
            vh.AddTriangle(segments * 4 + 4, 0, 1);

            TrimExcess();
        }

        private void TrimExcess()
        {
            _positions.TrimExcess();
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
            if (!useExactRaycastLocation || !IsValid || _positions.Count < PositionCountMinValue)
            {
                return true;
            }
            return UnityMath.IsPointInsidePolygon(localPoint, _positions);
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
            _positions.Clear();
            base.OnDisable();
        }
    }
}
