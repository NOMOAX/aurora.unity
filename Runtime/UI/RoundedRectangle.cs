using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Aurora.Unity.UI
{
    /// <summary>
    /// 圆角矩形。
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
        /// 纹理。
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
        /// 边数。
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> 不在 [1, 16248] 范围内。</exception>
        public int Segments
        {
            get => segments;
            set
            {
                if (value < SegmentsMinValue || value > SegmentsMaxValue)
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
        /// 是否使用标准化长度表示左上圆角半径。
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
        /// 左上圆角半径。
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> 为非数字，或者 <paramref name="value"/> 小于 0，或者当 <see cref="TopLeftCornerRadiusNormalized"/> 为 <see langword="true"/> 时 <paramref name="value"/> 大于 1。</exception>
        public float TopLeftCornerRadius
        {
            get => topLeftCornerRadius;
            set
            {
                if (!(value >= 0) || topLeftCornerRadiusNormalized && !(value <= 1))
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
        /// 是否使用标准化长度表示右上圆角半径。
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
        /// 右上圆角半径。
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> 为非数字，或者 <paramref name="value"/> 小于 0，或者当 <see cref="TopRightCornerRadiusNormalized"/> 为 <see langword="true"/> 时 <paramref name="value"/> 大于 1。</exception>
        public float TopRightCornerRadius
        {
            get => topRightCornerRadius;
            set
            {
                if (!(value >= 0) || topRightCornerRadiusNormalized && !(value <= 1))
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
        /// 是否使用标准化长度表示左下圆角半径。
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
        /// 左下圆角半径。
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> 为非数字，或者 <paramref name="value"/> 小于 0，或者当 <see cref="BottomLeftCornerRadiusNormalized"/> 为 <see langword="true"/> 时 <paramref name="value"/> 大于 1。</exception>
        public float BottomLeftCornerRadius
        {
            get => bottomLeftCornerRadius;
            set
            {
                if (!(value >= 0) || bottomLeftCornerRadiusNormalized && !(value <= 1))
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
        /// 是否使用标准化长度表示右下圆角半径。
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
        /// 右下圆角半径。
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> 为非数字，或者 <paramref name="value"/> 小于 0，或者当 <see cref="BottomRightCornerRadiusNormalized"/> 为 <see langword="true"/> 时 <paramref name="value"/> 大于 1。</exception>
        public float BottomRightCornerRadius
        {
            get => bottomRightCornerRadius;
            set
            {
                if (!(value >= 0) || bottomRightCornerRadiusNormalized && !(value <= 1))
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
        /// 是否使用精确点击区域。
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
                true  => Mathf.Clamp01(Mathf.InverseLerp(0, halfMinSide, value))
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
        /// 获取非标准化、实际有效的值。
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

            var color32           = (Color32) color;
            var pixelAdjustedRect = GetPixelAdjustedRect();
            var halfMinSide       = Mathf.Min(pixelAdjustedRect.width, pixelAdjustedRect.height) * 0.5f;

            // 各圆角半径
            var cornerRadii = new Vector4(
                GetActualValue(topRightCornerRadiusNormalized,    topRightCornerRadius,    halfMinSide),
                GetActualValue(topLeftCornerRadiusNormalized,     topLeftCornerRadius,     halfMinSide),
                GetActualValue(bottomLeftCornerRadiusNormalized,  bottomLeftCornerRadius,  halfMinSide),
                GetActualValue(bottomRightCornerRadiusNormalized, bottomRightCornerRadius, halfMinSide)
            );

            vh.Clear();

            var center   = pixelAdjustedRect.center;
            var halfSize = pixelAdjustedRect.size * 0.5f;

            // 添加中心点
            vh.AddVert(center, color32, new Vector2(0.5f, 0.5f));

            /*
             * 各象限的单位向量，用于参与乘法运算
             * 使用 stackalloc 关键字确保仅分配在栈内存上，提高性能
             */
            var multipliers = stackalloc Vector2[4]
            {
                Vector2.right + Vector2.up,  // 右上（第Ⅰ象限）
                Vector2.left + Vector2.up,   // 左上（第Ⅱ象限）
                Vector2.left + Vector2.down, // 左下（第Ⅲ象限）
                Vector2.right + Vector2.down // 右下（第Ⅳ象限）
            };

            // 每个象限使用 0.5π
            var stepAngle = 0.5f * Mathf.PI / segments;
            // 添加四个象限的各点
            for (var i = 0; i < 4; i++)
            {
                // 此象限的首个顶点的索引（注意，每个象限的最后一个顶点与它的下个象限的首个顶点具有相同的角度，此时将这两个顶点当作具有相同的索引）
                var firstVertexIndex = segments * i;
                // 圆角半径
                var cornerRadius = cornerRadii[i];
                // 圆角圆心到中心的相对位置
                var cornerCenterToCenter = (halfSize - Vector2.one * cornerRadius) * multipliers[i];
                // 因为每个象限有 segments 条边，所以有 segments + 1 个顶点
                for (var j = 0; j < segments + 1; j++)
                {
                    var angle = stepAngle * (firstVertexIndex + j);
                    // 此顶点到圆角圆心的相对位置
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
            if (float.IsNaN(value))
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
