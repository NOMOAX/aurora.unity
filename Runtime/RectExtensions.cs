using UnityEngine;

namespace Aurora.Unity
{
    /// <summary>
    /// Provides extension methods for the <see cref="Rect"/> struct.
    /// </summary>
    public static class RectExtensions
    {
        /// <summary>
        /// Determines whether the current <see cref="Rect"/> contains another rectangle.
        /// </summary>
        /// <param name="rect">This rectangle.</param>
        /// <param name="other">Another rectangle.</param>
        /// <returns><see langword="true"/> if <paramref name="rect"/> contains <paramref name="other"/>; otherwise, <see langword="false"/>.</returns>
        public static bool Contains(this Rect rect, Rect other)
        {
            return rect.xMin <= other.xMin && rect.xMax >= other.xMax && rect.yMin <= other.yMin &&
                   rect.yMax >= other.yMax;
        }
    }
}
