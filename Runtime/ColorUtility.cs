using System;
using UnityEngine;

namespace Aurora.Unity
{
    /// <summary>
    /// Provides utility methods for the <see cref="Color"/> struct.
    /// </summary>
    public static class ColorUtility
    {
        /// <summary>
        /// Converts an HTML color string to a color value.
        /// </summary>
        /// <param name="htmlString">The HTML color string.</param>
        /// <returns>The color value converted from <paramref name="htmlString"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="htmlString"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="htmlString"/> is not a valid HTML color string.</exception>
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
