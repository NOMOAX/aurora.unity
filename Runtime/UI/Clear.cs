using UnityEngine;
using UnityEngine.UI;

namespace Aurora.Unity.UI
{
    /// <summary>
    /// Transparent.
    /// </summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class Clear : MaskableGraphic
    {
        private Clear()
        {
            useLegacyMeshGeneration = false;
        }

        /// <inheritdoc />
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
        }
    }
}
