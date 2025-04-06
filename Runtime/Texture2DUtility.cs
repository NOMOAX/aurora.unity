using System;
using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Aurora.Unity
{
    /// <summary>
    /// 为 <see cref="Texture2D"/> 类提供工具方法。
    /// </summary>
    public static class Texture2DUtility
    {
        private static readonly byte[] RawRedQuestionMarkTextureData = GetRawRedQuestionMarkTextureData();

        /// <summary>
        /// 判断指定的二维纹理是否是默认的红色问号8×8二维纹理。
        /// </summary>
        /// <param name="texture">二维纹理。</param>
        /// <returns><paramref name="texture"/> 是否是默认的红色问号8×8二维纹理。</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="texture"/> 为 <see langword="null"/>。</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe bool IsRedQuestionMarkTexture(Texture2D texture)
        {
            if (texture == null)
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
