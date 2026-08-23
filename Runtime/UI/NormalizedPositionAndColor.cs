using System;
using UnityEngine;

namespace Aurora.Unity.UI
{
    /// <summary>
    /// Defines a normalized position and a color.
    /// </summary>
    [Serializable]
    public struct NormalizedPositionAndColor
    {
        /// <summary>
        /// The normalized position.
        /// </summary>
        public Vector2 normalizedPosition;

        /// <summary>
        /// The color.
        /// </summary>
        public Color32 color;
    }
}
