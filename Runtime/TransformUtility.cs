using System;
using UnityEngine;

namespace Aurora.Unity
{
    /// <summary>
    /// Provides utility methods for the <see cref="Transform"/> class.
    /// </summary>
    public static class TransformUtility
    {
        /// <summary>
        /// Determines whether two transforms share the same parent transform.
        /// </summary>
        /// <param name="a">The first transform.</param>
        /// <param name="b">The second transform.</param>
        /// <returns><see langword="true"/> if <paramref name="a"/> and <paramref name="b"/> share the same parent transform; otherwise <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="a"/> or <paramref name="b"/> is <see langword="null"/>.</exception>
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
        /// Determines whether three transforms share the same parent transform.
        /// </summary>
        /// <param name="a">The first transform.</param>
        /// <param name="b">The second transform.</param>
        /// <param name="c">The third transform.</param>
        /// <returns><see langword="true"/> if <paramref name="a"/>, <paramref name="b"/>, and <paramref name="c"/> share the same parent transform; otherwise <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="a"/>, <paramref name="b"/>, or <paramref name="c"/> is <see langword="null"/>.</exception>
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
        /// Determines whether four transforms share the same parent transform.
        /// </summary>
        /// <param name="a">The first transform.</param>
        /// <param name="b">The second transform.</param>
        /// <param name="c">The third transform.</param>
        /// <param name="d">The fourth transform.</param>
        /// <returns><see langword="true"/> if <paramref name="a"/>, <paramref name="b"/>, <paramref name="c"/>, and <paramref name="d"/> share the same parent transform; otherwise <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="a"/>, <paramref name="b"/>, <paramref name="c"/>, or <paramref name="d"/> is <see langword="null"/>.</exception>
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
        /// Determines whether multiple transforms share the same parent transform.
        /// </summary>
        /// <param name="transforms">Multiple transforms.</param>
        /// <returns><see langword="true"/> if every member of <paramref name="transforms"/> shares the same parent transform; otherwise <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="transforms"/> is <see langword="null"/>, or <paramref name="transforms"/> contains a <see langword="null"/> member.</exception>
        /// <exception cref="ArgumentException">The member count of <paramref name="transforms"/> is less than 2.</exception>
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
