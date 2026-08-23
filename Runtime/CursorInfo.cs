using System;
using UnityEngine;

namespace Aurora.Unity
{
    /// <summary>
    /// Cursor information.
    /// </summary>
    [Serializable]
    public struct CursorInfo
    {
        /// <summary>
        /// The cursor texture.
        /// </summary>
        public Texture2D texture;

        /// <summary>
        /// The offset from the top-left corner of the cursor texture used to determine the target point.
        /// </summary>
        /// <remarks>Must be within the bounds of the cursor texture.</remarks>
        public Vector2 hotspot;

        /// <summary>
        /// The cursor mode.
        /// </summary>
        public CursorMode cursorMode;
    }
}
