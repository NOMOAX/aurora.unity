using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Aurora.Unity
{
    /// <summary>
    /// 为 <see cref="Sprite"/> 类提供工具方法。
    /// </summary>
    public static class SpriteUtility
    {
        /// <summary>
        /// 将精灵中的标准化位置转换为本地位置。
        /// </summary>
        /// <param name="sprite">精灵。</param>
        /// <param name="normalizedPosition">标准化位置。</param>
        /// <returns><paramref name="sprite"/> 中标准化位置为 <paramref name="normalizedPosition"/> 的点在 <paramref name="sprite"/> 本地坐标系中的位置。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="sprite"/> 为 <see langword="null"/>。</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 NormalizedToLocalPosition(Sprite sprite, Vector2 normalizedPosition)
        {
            if (sprite == null)
            {
                throw new ArgumentNullException(nameof(sprite));
            }
            return InternalNormalizedToLocalPosition(sprite, normalizedPosition);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Vector2 InternalNormalizedToLocalPosition(Sprite sprite, Vector2 normalizedPosition)
        {
            var rect                  = new Rect(-sprite.pivot, sprite.rect.size);
            var localPositionInPixels = AuroraUnityMath.NormalizedToPointUnclamped(rect, normalizedPosition);
            return localPositionInPixels / sprite.pixelsPerUnit;
        }
    }
}
