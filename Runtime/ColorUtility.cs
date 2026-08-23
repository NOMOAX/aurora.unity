using System;
using UnityEngine;

namespace Aurora.Unity
{
    /// <summary>
    /// 为 <see cref="Color"/> 结构提供工具方法。
    /// </summary>
    public static class ColorUtility
    {
        /// <summary>
        /// 将 HTML 颜色字符串转换为颜色值。
        /// </summary>
        /// <param name="htmlString">HTML 颜色字符串。</param>
        /// <returns>将 <paramref name="htmlString"/> 转换得到的颜色值。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="htmlString"/> 为 <see langword="null"/>。</exception>
        /// <exception cref="ArgumentException"><paramref name="htmlString"/> 不是有效的 HTML 颜色字符串。</exception>
        /// <seealso cref="UnityEngine.ColorUtility.TryParseHtmlString"/>
        public static Color ParseHtmlString(string htmlString)
        {
            if (UnityEngine.ColorUtility.TryParseHtmlString(htmlString, out var color))
            {
                return color;
            }
            throw htmlString switch
            {
                null     => new ArgumentNullException(nameof(htmlString)),
                not null => new ArgumentException()
            };
        }
    }
}
