using System.Runtime.CompilerServices;
using UnityEngine;

namespace Aurora.Unity
{
    /// <summary>
    /// Provides extension methods for the <see cref="Color"/> struct.
    /// </summary>
    public static class ColorExtensions
    {
        /// <summary>
        /// Deconstructs this <see cref="Color"/>.
        /// </summary>
        /// <param name="color">The color.</param>
        /// <param name="r">Red.</param>
        /// <param name="g">Green.</param>
        /// <param name="b">Blue.</param>
        /// <param name="a">Alpha.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Deconstruct(this Color color, out float r, out float g, out float b, out float a)
        {
            r = color.r;
            g = color.g;
            b = color.b;
            a = color.a;
        }

        /// <summary>
        /// Deconstructs this <see cref="Color"/>.
        /// </summary>
        /// <param name="color">The color.</param>
        /// <param name="h">Hue.</param>
        /// <param name="s">Saturation.</param>
        /// <param name="v">Value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Deconstruct(this Color color, out float h, out float s, out float v)
        {
            Color.RGBToHSV(color, out h, out s, out v);
        }

        /// <summary>
        /// Determines whether the hue of the <see cref="Color"/> is undefined.
        /// </summary>
        /// <param name="color">The color.</param>
        /// <returns><see langword="true"/> if the R, G, and B values of <paramref name="color"/> are all equal; otherwise <see langword="false"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsHUndefined(this Color color)
        {
            return color.r.Equals(color.g) && color.r.Equals(color.b);
        }
    }
}
