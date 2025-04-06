using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Aurora.Unity.UI
{
    /// <summary>
    /// 环。
    /// </summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class Annulus : MaskableGraphic, ILayoutElement, ICanvasRaycastFilter
    {
        [SerializeField]
        internal Texture texture;

        [SerializeField]
        [Range(SegmentsMinValue, SegmentsMaxValue)]
        internal int segments = SegmentsDefaultValue;

        [SerializeField]
        [Range(0f, 1f)]
        internal float thickness = ThicknessDefaultValue;

        [SerializeField]
        internal bool useExactRaycastLocation;

        private readonly List<Vector2> _innerPositions = new List<Vector2>();

        private readonly List<Vector2> _outerPositions = new List<Vector2>();

        private const int SegmentsMinValue = 3;

        private const int SegmentsMaxValue = UnityUtility.VertexCountPerMeshMaxValue / 2;

        private const int SegmentsDefaultValue = 32;

        private const float ThicknessDefaultValue = 0.5f;

        private const int OuterPositionCountMinValue = SegmentsMinValue;

        private const int InnerPositionCountMinValue = SegmentsMinValue;

        private Annulus()
        {
            useLegacyMeshGeneration = false;
        }

        /// <inheritdoc />
        public override Texture mainTexture
        {
            get
            {
                if (texture != null)
                {
                    return texture;
                }
                return material != null && material.mainTexture != null ? material.mainTexture : s_WhiteTexture;
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
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> 不在 [3, 32499] 范围内。</exception>
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
        /// 粗细。
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> 不在 [0, 1] 范围内。</exception>
        public float Thickness
        {
            get => thickness;
            set
            {
                if (!(value >= 0f) || !(value <= 1f))
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
                }
                if (thickness == value)
                {
                    return;
                }
                thickness = value;
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

        /// <inheritdoc />
        public override void SetNativeSize()
        {
            rectTransform.anchorMin = rectTransform.anchorMax = Vector2.one * 0.5f;
            var t                                             = mainTexture;
            rectTransform.sizeDelta = new Vector2(t.width, t.height);
        }

        /// <inheritdoc />
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            _outerPositions.Clear();
            _innerPositions.Clear();

            vh.Clear();

            // 粗细为 0，图形消失
            if (thickness == 0f)
            {
                TrimExcess();
                return;
            }

            var color32           = (Color32) color;
            var pixelAdjustedRect = GetPixelAdjustedRect();
            var center            = pixelAdjustedRect.center;
            var halfSize          = pixelAdjustedRect.size * 0.5f;

            var stepAngle = 2f * Mathf.PI / segments;

            // 添加内圆圆周上各点
            for (var i = 0; i < segments; i++)
            {
                var angle    = stepAngle * i;
                var position = center + AuroraUnityMath.CosSin(angle) * halfSize * (1f - thickness);
                if (useExactRaycastLocation)
                {
                    _innerPositions.Add(position);
                }
                vh.AddVert(position, color32, AuroraUnityMath.GetUV(pixelAdjustedRect, position));
            }
            // 添加外圆圆周上各点
            for (var i = 0; i < segments; i++)
            {
                var angle    = stepAngle * i;
                var position = center + AuroraUnityMath.CosSin(angle) * halfSize;
                if (useExactRaycastLocation)
                {
                    _outerPositions.Add(position);
                }
                vh.AddVert(position, color32, AuroraUnityMath.GetUV(pixelAdjustedRect, position));
            }

            for (var index = 0; index < segments - 1; index++)
            {
                vh.AddTriangle(index,                index + 1,        index + segments + 1);
                vh.AddTriangle(index + segments + 1, index + segments, index);
            }
            vh.AddTriangle(segments - 1, 0,                segments);
            vh.AddTriangle(segments,     segments * 2 - 1, segments - 1);

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

        float ILayoutElement.minWidth => 0f;

        float ILayoutElement.preferredWidth => texture == null ? 0f : texture.width;

        float ILayoutElement.flexibleWidth => -1f;

        float ILayoutElement.minHeight => 0f;

        float ILayoutElement.preferredHeight => texture == null ? 0f : texture.height;

        float ILayoutElement.flexibleHeight => -1f;

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
            if (thickness == 0f)
            {
                return false;
            }
            if (_outerPositions.Count < OuterPositionCountMinValue ||
                _innerPositions.Count < InnerPositionCountMinValue)
            {
                return true;
            }
            return AuroraUnityMath.IsPointInsidePolygon(localPoint, _outerPositions) &&
                   !AuroraUnityMath.IsPointInsidePolygon(localPoint, _innerPositions);
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
            segments  = Mathf.Clamp(segments, SegmentsMinValue, SegmentsMaxValue);
            thickness = !float.IsNaN(thickness) ? Mathf.Clamp01(thickness) : ThicknessDefaultValue;
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
