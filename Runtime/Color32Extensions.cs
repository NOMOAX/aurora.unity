using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Aurora.Unity
{
    /// <summary>
    /// 为 <see cref="Color32"/> 结构提供扩展方法。
    /// </summary>
    public static class Color32Extensions
    {
        /// <summary>
        /// 析构此 <see cref="Color32"/>。
        /// </summary>
        /// <param name="color">颜色。</param>
        /// <param name="r">红色。</param>
        /// <param name="g">绿色。</param>
        /// <param name="b">蓝色。</param>
        /// <param name="a">不透明度。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Deconstruct(this Color32 color, out byte r, out byte g, out byte b, out byte a)
        {
            r = color.r;
            g = color.g;
            b = color.b;
            a = color.a;
        }

        /// <summary>
        /// 析构此 <see cref="Color32"/>。
        /// </summary>
        /// <param name="color">颜色。</param>
        /// <param name="h">色相。</param>
        /// <param name="s">饱和度。</param>
        /// <param name="v">明度。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Deconstruct(this Color32 color, out float h, out float s, out float v)
        {
            Color.RGBToHSV(color, out h, out s, out v);
        }

        /// <summary>
        /// 判断 <see cref="Color32"/> 的色相是否是未定义的。
        /// </summary>
        /// <param name="color">颜色。</param>
        /// <returns>如果 <paramref name="color"/> 的 R、G、B 值都相等，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsHUndefined(this Color32 color)
        {
            return color.r.Equals(color.g) && color.r.Equals(color.b);
        }

        /// <summary>
        /// 复制当前 <see cref="Color32"/>，然后将色相设置为新的值。
        /// </summary>
        /// <param name="color">颜色。</param>
        /// <param name="h">色相。</param>
        /// <returns>新的颜色。</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="h"/> 不在 [0, 1] 范围内。</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Color32 WithH(this Color32 color, float h)
        {
            if (h is < 0 or > 1 or float.NaN)
            {
                throw new ArgumentOutOfRangeException(nameof(h), h, null);
            }
            return InternalWithH(color, h);
        }

        /// <summary>
        /// 复制当前 <see cref="Color32"/>，然后尝试将色相设置为新的值。
        /// </summary>
        /// <param name="color">颜色。</param>
        /// <param name="h">色相。</param>
        /// <param name="result">新的颜色。</param>
        /// <returns>如果新的颜色与 <paramref name="color"/> 不相同，则为 <see langword="true"/> ；否则为 <see langword="false"/>。</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="h"/> 不在 [0, 1] 范围内。</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryWithH(this Color32 color, float h, out Color32 result)
        {
            if (h is < 0 or > 1 or float.NaN)
            {
                throw new ArgumentOutOfRangeException(nameof(h), h, null);
            }
            result = InternalWithH(color, h);
            return color.r != result.r || color.g != result.g || color.b != result.b;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Color32 InternalWithH(Color32 color, float h)
        {
            if (IsHUndefined(color))
            {
                return color;
            }
            Color.RGBToHSV(color, out _, out var s, out var v);
            var result = (Color32) Color.HSVToRGB(h, s, v);
            result.a = color.a;
            return result;
        }
    }
}
