using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Aurora.Unity
{
    /// <summary>
    /// Provides extension methods for the <see cref="Color32"/> struct.
    /// </summary>
    public static class Color32Extensions
    {
        /// <summary>
        /// Deconstructs this <see cref="Color32"/>.
        /// </summary>
        /// <param name="color">The color.</param>
        /// <param name="r">Red.</param>
        /// <param name="g">Green.</param>
        /// <param name="b">Blue.</param>
        /// <param name="a">Alpha.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Deconstruct(this Color32 color, out byte r, out byte g, out byte b, out byte a)
        {
            r = color.r;
            g = color.g;
            b = color.b;
            a = color.a;
        }

        /// <summary>
        /// Deconstructs this <see cref="Color32"/>.
        /// </summary>
        /// <param name="color">The color.</param>
        /// <param name="h">Hue.</param>
        /// <param name="s">Saturation.</param>
        /// <param name="v">Value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Deconstruct(this Color32 color, out float h, out float s, out float v)
        {
            Color.RGBToHSV(color, out h, out s, out v);
        }

        /// <summary>
        /// Determines whether the hue of the <see cref="Color32"/> is undefined.
        /// </summary>
        /// <param name="color">The color.</param>
        /// <returns><see langword="true"/> if the R, G, and B values of <paramref name="color"/> are all equal; otherwise <see langword="false"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsHUndefined(this Color32 color)
        {
            return color.r.Equals(color.g) && color.r.Equals(color.b);
        }

        /// <summary>
        /// Copies the current <see cref="Color32"/> and then sets the hue to a new value.
        /// </summary>
        /// <param name="color">The color.</param>
        /// <param name="h">The hue.</param>
        /// <returns>The new color.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="h"/> is not in the [0, 1] range.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Color32 WithH(this Color32 color, float h)
        {
            if (h is float.NaN or < 0 or > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(h), h, null);
            }
            return InternalWithH(color, h);
        }

        /// <summary>
        /// Copies the current <see cref="Color32"/> and then attempts to set the hue to a new value.
        /// </summary>
        /// <param name="color">The color.</param>
        /// <param name="h">The hue.</param>
        /// <param name="result">The new color.</param>
        /// <returns><see langword="true"/> if the new color is different from <paramref name="color"/>; otherwise <see langword="false"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="h"/> is not in the [0, 1] range.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryWithH(this Color32 color, float h, out Color32 result)
        {
            if (h is float.NaN or < 0 or > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(h), h, null);
            }
            result = InternalWithH(color, h);
            return color.r != result.r || color.g != result.g || color.b != result.b;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Color32 InternalWithH(Color32 color, float h)
        {
            if (color.IsHUndefined())
            {
                return color;
            }
            Color.RGBToHSV(color, out _, out var s, out var v);
            var result = (Color32)Color.HSVToRGB(h, s, v);
            result.a = color.a;
            return result;
        }
    }
}
