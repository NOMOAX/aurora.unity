using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Aurora.Unity
{
    /// <summary>
    /// Provides utility methods for the <see cref="Sprite"/> class.
    /// </summary>
    public static class SpriteUtility
    {
        /// <summary>
        /// Converts a normalized position in a sprite to a local position.
        /// </summary>
        /// <param name="sprite">The sprite.</param>
        /// <param name="normalizedPosition">The normalized position.</param>
        /// <returns>The position in <paramref name="sprite"/>'s local coordinate system of the point whose normalized position in <paramref name="sprite"/> is <paramref name="normalizedPosition"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="sprite"/> is <see langword="null"/>.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 NormalizedToLocalPosition(Sprite sprite, Vector2 normalizedPosition)
        {
            if (!sprite)
            {
                throw new ArgumentNullException(nameof(sprite));
            }
            return InternalNormalizedToLocalPosition(sprite, normalizedPosition);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Vector2 InternalNormalizedToLocalPosition(Sprite sprite, Vector2 normalizedPosition)
        {
            var rect                  = new Rect(-sprite.pivot, sprite.rect.size);
            var localPositionInPixels = UnityMath.NormalizedToPointUnclamped(rect, normalizedPosition);
            return localPositionInPixels / sprite.pixelsPerUnit;
        }
    }
}
