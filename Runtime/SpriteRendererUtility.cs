using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Aurora.Unity
{
    /// <summary>
    /// Provides utility methods for the <see cref="SpriteRenderer"/> class.
    /// </summary>
    public static class SpriteRendererUtility
    {
        /// <summary>
        /// Converts a normalized position in a sprite renderer to a world position.
        /// </summary>
        /// <param name="spriteRenderer">The sprite renderer.</param>
        /// <param name="normalizedPosition">The normalized position.</param>
        /// <returns>The position in world space of the point whose normalized position in <paramref name="spriteRenderer"/> is <paramref name="normalizedPosition"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="spriteRenderer"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">The <see cref="SpriteRenderer.sprite"/> value of <paramref name="spriteRenderer"/> is <see langword="null"/>.</exception>
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
        /// Converts a normalized position in a sprite renderer to a local position.
        /// </summary>
        /// <param name="spriteRenderer">The sprite renderer.</param>
        /// <param name="normalizedPosition">The normalized position.</param>
        /// <returns>The position in <paramref name="spriteRenderer"/>'s local coordinate system of the point whose normalized position in <paramref name="spriteRenderer"/> is <paramref name="normalizedPosition"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="spriteRenderer"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">The <see cref="SpriteRenderer.sprite"/> value of <paramref name="spriteRenderer"/> is <see langword="null"/>.</exception>
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
        /// Gets the factor related to the <see cref="SpriteRenderer.flipX"/> and <see cref="SpriteRenderer.flipY"/> of the sprite renderer, which can be used in a specific multiplication calculation.
        /// </summary>
        /// <param name="spriteRenderer">The sprite renderer.</param>
        /// <returns>
        /// A <see cref="Vector2"/> value; see the table below.
        /// <list type="table">
        /// <listheader><term>Case</term><description>Description</description></listheader>
        /// <item><term>the <see cref="SpriteRenderer.flipX"/> of <paramref name="spriteRenderer"/> is <see langword="true"/></term><description>the <see cref="Vector2.x"/> component of the return value is 1</description></item>
        /// <item><term>the <see cref="SpriteRenderer.flipX"/> of <paramref name="spriteRenderer"/> is <see langword="false"/></term><description>the <see cref="Vector2.x"/> component of the return value is -1</description></item>
        /// <item><term>the <see cref="SpriteRenderer.flipY"/> of <paramref name="spriteRenderer"/> is <see langword="true"/></term><description>the <see cref="Vector2.y"/> component of the return value is 1</description></item>
        /// <item><term>the <see cref="SpriteRenderer.flipY"/> of <paramref name="spriteRenderer"/> is <see langword="false"/></term><description>the <see cref="Vector2.y"/> component of the return value is -1</description></item>
        /// </list>
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="spriteRenderer"/> is <see langword="null"/>.</exception>
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
