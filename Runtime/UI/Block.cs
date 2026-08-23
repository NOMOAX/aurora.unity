using UnityEngine;
using UnityEngine.UI;

namespace Aurora.Unity.UI
{
    /// <summary>
    /// A color block.
    /// </summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class Block : MaskableGraphic
    {
        private Block()
        {
            useLegacyMeshGeneration = false;
        }

        /// <inheritdoc />
        protected override void OnDidApplyAnimationProperties()
        {
            SetMaterialDirty();
            SetVerticesDirty();
        }
    }
}
