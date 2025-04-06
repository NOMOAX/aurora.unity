using System;
using UnityEngine;

namespace Aurora.Unity.UI
{
    /// <summary>
    /// 定义标准化位置和颜色。
    /// </summary>
    [Serializable]
    public struct NormalizedPositionAndColor
    {
        /// <summary>
        /// 标准化位置。
        /// </summary>
        public Vector2 normalizedPosition;

        /// <summary>
        /// 颜色。
        /// </summary>
        public Color32 color;
    }
}
