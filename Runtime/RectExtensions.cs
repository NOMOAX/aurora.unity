using UnityEngine;

namespace Aurora.Unity
{
    /// <summary>
    /// 为 <see cref="Rect"/> 结构提供扩展方法。
    /// </summary>
    public static class RectExtensions
    {
        /// <summary>
        /// 判断当前 <see cref="Rect"/> 是否包含另一个矩形。
        /// </summary>
        /// <param name="rect">此矩形。</param>
        /// <param name="other">另一个矩形。</param>
        /// <returns>如果 <paramref name="rect"/> 包含 <paramref name="other"/>，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
        public static bool Contains(this Rect rect, Rect other)
        {
            return rect.xMin <= other.xMin && rect.xMax >= other.xMax && rect.yMin <= other.yMin &&
                   rect.yMax >= other.yMax;
        }
    }
}
