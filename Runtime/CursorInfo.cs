using System;
using UnityEngine;

namespace Aurora.Unity
{
    /// <summary>
    /// 光标信息。
    /// </summary>
    [Serializable]
    public struct CursorInfo
    {
        /// <summary>
        /// 光标纹理。
        /// </summary>
        public Texture2D texture;

        /// <summary>
        /// 从光标纹理的左上角开始的偏移量，用于确定目标点的位置。
        /// </summary>
        /// <remarks>必须在光标纹理的边界内。</remarks>
        public Vector2 hotspot;

        /// <summary>
        /// 光标模式。
        /// </summary>
        public CursorMode cursorMode;
    }
}
