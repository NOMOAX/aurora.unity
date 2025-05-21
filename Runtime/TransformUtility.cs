using System;
using UnityEngine;

namespace Aurora.Unity
{
    /// <summary>
    /// 为 <see cref="Transform"/> 类提供工具方法。
    /// </summary>
    public static class TransformUtility
    {
        /// <summary>
        /// 判断两个变换是否具有同样的父变换。
        /// </summary>
        /// <param name="a">第一个变换。</param>
        /// <param name="b">第二个变换。</param>
        /// <returns>如果 <paramref name="a"/> 和 <paramref name="b"/> 具有同样的父变换，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="a"/> 或 <paramref name="b"/> 为 <see langword="null"/>。</exception>
        public static bool AreTransformsShareSameParent(Transform a, Transform b)
        {
            if (!a)
            {
                throw new ArgumentNullException(nameof(a));
            }
            if (!b)
            {
                throw new ArgumentNullException(nameof(b));
            }
            return a.parent == b.parent;
        }

        /// <summary>
        /// 判断三个变换是否具有同样的父变换。
        /// </summary>
        /// <param name="a">第一个变换。</param>
        /// <param name="b">第二个变换。</param>
        /// <param name="c">第三个变换。</param>
        /// <returns>如果 <paramref name="a"/>、<paramref name="b"/> 和 <paramref name="c"/> 具有同样的父变换，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="a"/>、<paramref name="b"/> 或 <paramref name="c"/> 为 <see langword="null"/>。</exception>
        public static bool AreTransformsShareSameParent(Transform a, Transform b, Transform c)
        {
            if (!a)
            {
                throw new ArgumentNullException(nameof(a));
            }
            if (!b)
            {
                throw new ArgumentNullException(nameof(b));
            }
            if (!c)
            {
                throw new ArgumentNullException(nameof(c));
            }
            var parent = a.parent;
            return parent == b.parent && parent == c.parent;
        }

        /// <summary>
        /// 判断四个变换是否具有同样的父变换。
        /// </summary>
        /// <param name="a">第一个变换。</param>
        /// <param name="b">第二个变换。</param>
        /// <param name="c">第三个变换。</param>
        /// <param name="d">第四个变换。</param>
        /// <returns>如果 <paramref name="a"/>、<paramref name="b"/>、<paramref name="c"/> 和 <paramref name="d"/> 具有同样的父变换，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="a"/>、<paramref name="b"/>、<paramref name="c"/> 或 <paramref name="d"/> 为 <see langword="null"/>。</exception>
        public static bool AreTransformsShareSameParent(Transform a, Transform b, Transform c, Transform d)
        {
            if (!a)
            {
                throw new ArgumentNullException(nameof(a));
            }
            if (!b)
            {
                throw new ArgumentNullException(nameof(b));
            }
            if (!c)
            {
                throw new ArgumentNullException(nameof(c));
            }
            if (!d)
            {
                throw new ArgumentNullException(nameof(d));
            }
            var parent = a.parent;
            return parent == b.parent && parent == c.parent && parent == d.parent;
        }

        /// <summary>
        /// 判断多个变换是否具有同样的父变换。
        /// </summary>
        /// <param name="transforms">多个变换。</param>
        /// <returns>如果 <paramref name="transforms"/> 中的每个成员都具有同样的父变换，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="transforms"/> 为 <see langword="null"/>，或 <paramref name="transforms"/> 中含有为 <see langword="null"/> 的成员。</exception>
        /// <exception cref="ArgumentException"><paramref name="transforms"/> 的成员数小于 2。</exception>
        public static bool AreTransformsShareSameParent(params Transform[] transforms)
        {
            if (transforms is null)
            {
                throw new ArgumentNullException(nameof(transforms));
            }
            var length = transforms.Length;
            if (length < 2)
            {
                throw new ArgumentException(null, nameof(transforms));
            }
            var firstTransform = transforms[0];
            if (!firstTransform)
            {
                throw new ArgumentNullException();
            }
            var parent = firstTransform.parent;
            for (var i = 1; i < length; i++)
            {
                var otherTransform = transforms[i];
                if (!otherTransform)
                {
                    throw new ArgumentNullException();
                }
                if (parent != otherTransform.parent)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
