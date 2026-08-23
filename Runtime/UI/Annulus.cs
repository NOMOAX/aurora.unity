using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Aurora.Unity.UI
{
    /// <summary>
    /// An annulus.
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
        [Range(0, 1)]
        internal float thickness = ThicknessDefaultValue;

        [SerializeField]
        internal bool useExactRaycastLocation;

        private readonly List<Vector2> _innerPositions = new();

        private readonly List<Vector2> _outerPositions = new();

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
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is outside the range [3, 32499].</exception>
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
        /// The thickness.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is outside the range [0, 1].</exception>
        public float Thickness
        {
            get => thickness;
            set
            {
                if (value is float.NaN or < 0 or > 1)
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

            // When the thickness is 0, the graphic disappears
            if (thickness == 0)
            {
                TrimExcess();
                return;
            }

            var color32           = (Color32)color;
            var pixelAdjustedRect = GetPixelAdjustedRect();
            var center            = pixelAdjustedRect.center;
            var halfSize          = pixelAdjustedRect.size * 0.5f;

            var stepAngle = 2 * Mathf.PI / segments;

            // Add the points on the inner circle's circumference
            for (var i = 0; i < segments; i++)
            {
                var angle    = stepAngle * i;
                var position = center + UnityMath.CosSin(angle) * halfSize * (1 - thickness);
                if (useExactRaycastLocation)
                {
                    _innerPositions.Add(position);
                }
                vh.AddVert(position, color32, UnityMath.GetUV(pixelAdjustedRect, position));
            }
            // Add the points on the outer circle's circumference
            for (var i = 0; i < segments; i++)
            {
                var angle    = stepAngle * i;
                var position = center + UnityMath.CosSin(angle) * halfSize;
                if (useExactRaycastLocation)
                {
                    _outerPositions.Add(position);
                }
                vh.AddVert(position, color32, UnityMath.GetUV(pixelAdjustedRect, position));
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
            segments  = Mathf.Clamp(segments, SegmentsMinValue, SegmentsMaxValue);
            thickness = thickness is not float.NaN ? Mathf.Clamp01(thickness) : ThicknessDefaultValue;
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
