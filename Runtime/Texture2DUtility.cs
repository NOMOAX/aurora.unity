using System;
using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Aurora.Unity
{
    /// <summary>
    /// Provides utility methods for the <see cref="Texture2D"/> class.
    /// </summary>
    public static class Texture2DUtility
    {
        private static readonly byte[] RawRedQuestionMarkTextureData = GetRawRedQuestionMarkTextureData();

        /// <summary>
        /// Determines whether the specified 2D texture is the default red question-mark 8x8 texture.
        /// </summary>
        /// <param name="texture">The 2D texture.</param>
        /// <returns>Whether <paramref name="texture"/> is the default red question-mark 8x8 texture.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="texture"/> is <see langword="null"/>.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe bool IsRedQuestionMarkTexture(Texture2D texture)
        {
            if (!texture)
            {
                throw new ArgumentNullException(nameof(texture));
            }
            var textureRawData                = texture.GetRawTextureData<byte>();
            var length                        = textureRawData.Length;
            var rawRedQuestionMarkTextureData = RawRedQuestionMarkTextureData;
            if (length != rawRedQuestionMarkTextureData.Length)
            {
                return false;
            }
            fixed (byte* pointer = rawRedQuestionMarkTextureData)
            {
                return UnsafeUtility.MemCmp(textureRawData.GetUnsafeReadOnlyPtr(), pointer, length) == 0;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static byte[] GetRawRedQuestionMarkTextureData()
        {
            var redQuestionMarkTexture = GetRedQuestionMarkTexture();
            try
            {
                return redQuestionMarkTexture.GetRawTextureData();
            }
            finally
            {
                Object.DestroyImmediate(redQuestionMarkTexture);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Texture2D GetRedQuestionMarkTexture()
        {
            var texture2D = new Texture2D(0, 0);
            texture2D.LoadImage(Array.Empty<byte>());
            return texture2D;
        }
    }
}
