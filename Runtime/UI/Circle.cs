using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Aurora.Unity.UI
{
    /// <summary>
    /// A circle.
    /// </summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class Circle : MaskableGraphic, ILayoutElement, ICanvasRaycastFilter
    {
        [SerializeField]
        internal Texture texture;

        [SerializeField]
        [Range(SegmentsMinValue, SegmentsMaxValue)]
        internal int segments = 32;

        [SerializeField]
        internal bool useExactRaycastLocation;

        private readonly List<Vector2> _positions = new();

        private const int SegmentsMinValue = 3;

        private const int SegmentsMaxValue = UnityUtility.VertexCountPerMeshMaxValue - 1;

        private const int PositionCountMinValue = SegmentsMinValue;

        private Circle()
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
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is outside the range [3, 64998].</exception>
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
            _positions.Clear();

            vh.Clear();

            var color32           = (Color32)color;
            var pixelAdjustedRect = GetPixelAdjustedRect();
            var center            = pixelAdjustedRect.center;
            var halfSize          = pixelAdjustedRect.size * 0.5f;

            // Add the circle center
            vh.AddVert(center, color32, new Vector2(0.5f, 0.5f));

            var stepAngle = 2 * Mathf.PI / segments;

            // Add the points on the circumference
            for (var i = 0; i < segments; i++)
            {
                var angle    = stepAngle * i;
                var position = center + UnityMath.CosSin(angle) * halfSize;
                if (useExactRaycastLocation)
                {
                    _positions.Add(position);
                }
                vh.AddVert(position, color32, UnityMath.GetUV(pixelAdjustedRect, position));
            }

            for (var index = 1; index < segments; index++)
            {
                vh.AddTriangle(index, 0, index + 1);
            }
            vh.AddTriangle(segments, 0, 1);

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
            if (!useExactRaycastLocation || _positions.Count < PositionCountMinValue)
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
        }
#endif

        protected override void OnDisable()
        {
            _positions.Clear();
            base.OnDisable();
        }
    }
}
