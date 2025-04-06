using System.Runtime.CompilerServices;
using UnityEngine;

namespace Aurora.Unity
{
    /// <summary>
    /// 为 <see cref="Color"/> 结构提供扩展方法。
    /// </summary>
    public static class ColorExtensions
    {
        /// <summary>
        /// 析构此 <see cref="Color"/>。
        /// </summary>
        /// <param name="color">颜色。</param>
        /// <param name="r">红色。</param>
        /// <param name="g">绿色。</param>
        /// <param name="b">蓝色。</param>
        /// <param name="a">不透明度。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Deconstruct(this Color color, out float r, out float g, out float b, out float a)
        {
            r = color.r;
            g = color.g;
            b = color.b;
            a = color.a;
        }

        /// <summary>
        /// 析构此 <see cref="Color"/>。
        /// </summary>
        /// <param name="color">颜色。</param>
        /// <param name="h">色相。</param>
        /// <param name="s">饱和度。</param>
        /// <param name="v">明度。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Deconstruct(this Color color, out float h, out float s, out float v)
        {
            Color.RGBToHSV(color, out h, out s, out v);
        }

        /// <summary>
        /// 判断 <see cref="Color"/> 的色相是否是未定义的。
        /// </summary>
        /// <param name="color">颜色。</param>
        /// <returns>如果 <paramref name="color"/> 的 R、G、B 值都相等，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsHUndefined(this Color color)
        {
            return color.r.Equals(color.g) && color.r.Equals(color.b);
        }
    }
}
