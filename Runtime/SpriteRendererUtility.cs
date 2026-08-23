using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Aurora.Unity
{
    /// <summary>
    /// 为 <see cref="SpriteRenderer"/> 类提供工具方法。
    /// </summary>
    public static class SpriteRendererUtility
    {
        /// <summary>
        /// 将精灵渲染器中的标准化位置转换为世界位置。
        /// </summary>
        /// <param name="spriteRenderer">精灵渲染器。</param>
        /// <param name="normalizedPosition">标准化位置。</param>
        /// <returns><paramref name="spriteRenderer"/> 中标准化位置为 <paramref name="normalizedPosition"/> 的点在世界坐标系中的位置。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="spriteRenderer"/> 为 <see langword="null"/>。</exception>
        /// <exception cref="ArgumentException"><paramref name="spriteRenderer"/> 的 <see cref="SpriteRenderer.sprite"/> 值为 <see langword="null"/>。</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 NormalizedToWorldPosition(SpriteRenderer spriteRenderer, Vector2 normalizedPosition)
        {
            if (!spriteRenderer)
            {
                throw new ArgumentNullException(nameof(spriteRenderer));
            }
            return InternalNormalizedToWorldPosition(spriteRenderer, normalizedPosition);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector3 InternalNormalizedToWorldPosition(
            SpriteRenderer spriteRenderer,
            Vector2        normalizedPosition)
        {
            var localPosition = InternalNormalizedToLocalPosition(spriteRenderer, normalizedPosition);
            return spriteRenderer.transform.TransformPoint(localPosition);
        }

        /// <summary>
        /// 将精灵渲染器中的标准化位置转换为本地位置。
        /// </summary>
        /// <param name="spriteRenderer">精灵渲染器。</param>
        /// <param name="normalizedPosition">标准化位置。</param>
        /// <returns><paramref name="spriteRenderer"/> 中标准化位置为 <paramref name="normalizedPosition"/> 的点在 <paramref name="spriteRenderer"/> 本地坐标系中的位置。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="spriteRenderer"/> 为 <see langword="null"/>。</exception>
        /// <exception cref="ArgumentException"><paramref name="spriteRenderer"/> 的 <see cref="SpriteRenderer.sprite"/> 值为 <see langword="null"/>。</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 NormalizedToLocalPosition(SpriteRenderer spriteRenderer, Vector2 normalizedPosition)
        {
            if (!spriteRenderer)
            {
                throw new ArgumentNullException(nameof(spriteRenderer));
            }
            return InternalNormalizedToLocalPosition(spriteRenderer, normalizedPosition);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector2 InternalNormalizedToLocalPosition(
            SpriteRenderer spriteRenderer,
            Vector2        normalizedPosition)
        {
            var sprite = spriteRenderer.sprite;
            if (!sprite)
            {
                throw new ArgumentException(null, nameof(sprite));
            }
            var localPosition = SpriteUtility.InternalNormalizedToLocalPosition(sprite, normalizedPosition);
            var multiplier    = InternalGetFlipMultiplier(spriteRenderer);
            return localPosition * multiplier;
        }

        /// <summary>
        /// 获取与精灵渲染器的 <see cref="SpriteRenderer.flipX"/> 和 <see cref="SpriteRenderer.flipY"/> 有关的因数，该因数可用于参与特定的乘法计算。
        /// </summary>
        /// <param name="spriteRenderer">精灵渲染器。</param>
        /// <returns>
        /// 一个 <see cref="Vector2"/> 值，它的值见下表。
        /// <list type="table">
        /// <listheader><term>情形</term><description>描述</description></listheader>
        /// <item><term><paramref name="spriteRenderer"/> 的 <see cref="SpriteRenderer.flipX"/> 为 <see langword="true"/></term><description>返回值的 <see cref="Vector2.x"/> 分量为 1</description></item>
        /// <item><term><paramref name="spriteRenderer"/> 的 <see cref="SpriteRenderer.flipX"/> 为 <see langword="false"/></term><description>返回值的 <see cref="Vector2.x"/> 分量为 -1</description></item>
        /// <item><term><paramref name="spriteRenderer"/> 的 <see cref="SpriteRenderer.flipY"/> 为 <see langword="true"/></term><description>返回值的 <see cref="Vector2.y"/> 分量为 1</description></item>
        /// <item><term><paramref name="spriteRenderer"/> 的 <see cref="SpriteRenderer.flipY"/> 为 <see langword="false"/></term><description>返回值的 <see cref="Vector2.y"/> 分量为 -1</description></item>
        /// </list>
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="spriteRenderer"/> 为 <see langword="null"/>。</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 GetFlipMultiplier(SpriteRenderer spriteRenderer)
        {
            if (!spriteRenderer)
            {
                throw new ArgumentNullException(nameof(spriteRenderer));
            }
            return InternalGetFlipMultiplier(spriteRenderer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector2 InternalGetFlipMultiplier(SpriteRenderer spriteRenderer)
        {
            return new Vector2(spriteRenderer.flipX ? -1 : 1, spriteRenderer.flipY ? -1 : 1);
        }
    }
}
