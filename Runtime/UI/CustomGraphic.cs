using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Aurora.Unity.UI
{
    /// <summary>
    /// 自定义图形。
    /// </summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class CustomGraphic : MaskableGraphic, ILayoutElement
    {
        [SerializeField]
        internal Texture texture;

        [SerializeField]
        internal List<NormalizedPositionAndColor> vertices = new List<NormalizedPositionAndColor>();

        [SerializeField]
        internal List<Vector3Int> triangles = new List<Vector3Int>();

        private CustomGraphic()
        {
            useLegacyMeshGeneration = false;
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
        /// 顶点。
        /// </summary>
        public List<NormalizedPositionAndColor> Vertices => vertices;

        /// <summary>
        /// 三角形。
        /// </summary>
        public List<Vector3Int> Triangles => triangles;

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

        private bool IsValid =>
            vertices.Count <= UnityUtility.VertexCountPerMeshMaxValue && AreVerticesAndTrianglesValid;

        private bool AreVerticesAndTrianglesValid
        {
            get
            {
                var vertexCount = vertices.Count;
                foreach (var triangle in triangles)
                {
                    for (var i = 0; i < 3; i++)
                    {
                        if (!Between(triangle[i], 0, vertexCount))
                        {
                            return false;
                        }
                    }
                }
                return true;
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
            if (!IsValid)
            {
                base.OnPopulateMesh(vh);
                return;
            }

            vh.Clear();

            if (triangles.Count == 0)
            {
                return;
            }

            var pixelAdjustedRect = GetPixelAdjustedRect();
            foreach (var vertex in vertices)
            {
                var normalizedPosition = vertex.normalizedPosition;
                var position           = UnityMath.NormalizedToPointUnclamped(pixelAdjustedRect, normalizedPosition);
                vh.AddVert(position, color * vertex.color, normalizedPosition);
            }
            foreach (var triangle in triangles)
            {
                vh.AddTriangle(triangle.x, triangle.y, triangle.z);
            }
        }

        private static bool Between(int value, int min, int max)
        {
            return min <= value && value < max;
        }

        /// <inheritdoc />
        protected override void OnDidApplyAnimationProperties()
        {
            SetMaterialDirty();
            SetVerticesDirty();
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
    }
}
