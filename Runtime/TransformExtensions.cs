using System;
using System.Collections.Generic;
using Aurora.Pooling;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Aurora.Unity
{
    /// <summary>
    /// Provides extension methods for the <see cref="Transform"/> class.
    /// </summary>
    public static class TransformExtensions
    {
        /// <summary>
        /// Swaps the index of the current <see cref="Transform"/> with the specified sibling transform.
        /// </summary>
        /// <param name="transform">This transform.</param>
        /// <param name="sibling">The sibling transform.</param>
        /// <exception cref="ArgumentNullException"><paramref name="transform"/> or <paramref name="sibling"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="transform"/> and <paramref name="sibling"/> are not siblings.</exception>
        public static void SwapSibling(this Transform transform, Transform sibling)
        {
            if (!transform)
            {
                throw new ArgumentNullException(nameof(transform));
            }
            if (!sibling)
            {
                throw new ArgumentNullException(nameof(sibling));
            }
            if (!TransformUtility.AreTransformsShareSameParent(transform, sibling))
            {
                throw new ArgumentException(null, nameof(sibling));
            }
            if (transform == sibling)
            {
                return;
            }
            var index        = transform.GetSiblingIndex();
            var siblingIndex = sibling.GetSiblingIndex();
            if (index < siblingIndex)
            {
                sibling.SetSiblingIndex(index);
                transform.SetSiblingIndex(siblingIndex);
            }
            else
            {
                transform.SetSiblingIndex(siblingIndex);
                sibling.SetSiblingIndex(index);
            }
        }

        /// <summary>
        /// Sets the index of the current <see cref="Transform"/> so that its index is just before the specified sibling transform.
        /// </summary>
        /// <param name="transform">This transform.</param>
        /// <param name="sibling">The sibling transform.</param>
        /// <exception cref="ArgumentNullException"><paramref name="transform"/> or <paramref name="sibling"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="transform"/> and <paramref name="sibling"/> are not siblings, or <paramref name="transform"/> and <paramref name="sibling"/> are the same instance.</exception>
        public static void SetBeforeSibling(this Transform transform, Transform sibling)
        {
            if (!transform)
            {
                throw new ArgumentNullException(nameof(transform));
            }
            if (!sibling)
            {
                throw new ArgumentNullException(nameof(sibling));
            }
            if (!TransformUtility.AreTransformsShareSameParent(transform, sibling))
            {
                throw new ArgumentException(null, nameof(sibling));
            }
            if (transform == sibling)
            {
                throw new ArgumentException(null, nameof(sibling));
            }
            var index        = transform.GetSiblingIndex();
            var siblingIndex = sibling.GetSiblingIndex();
            transform.SetSiblingIndex(index > siblingIndex ? siblingIndex : siblingIndex - 1);
        }

        /// <summary>
        /// Sets the index of the current <see cref="Transform"/> so that its index is just after the specified sibling transform.
        /// </summary>
        /// <param name="transform">This transform.</param>
        /// <param name="sibling">The sibling transform.</param>
        /// <exception cref="ArgumentNullException"><paramref name="transform"/> or <paramref name="sibling"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="transform"/> and <paramref name="sibling"/> are not siblings, or <paramref name="transform"/> and <paramref name="sibling"/> are the same instance.</exception>
        public static void SetAfterSibling(this Transform transform, Transform sibling)
        {
            if (!transform)
            {
                throw new ArgumentNullException(nameof(transform));
            }
            if (!sibling)
            {
                throw new ArgumentNullException(nameof(sibling));
            }
            if (!TransformUtility.AreTransformsShareSameParent(transform, sibling))
            {
                throw new ArgumentException(null, nameof(sibling));
            }
            if (transform == sibling)
            {
                throw new ArgumentException(null, nameof(sibling));
            }
            var index        = transform.GetSiblingIndex();
            var siblingIndex = sibling.GetSiblingIndex();
            transform.SetSiblingIndex(index < siblingIndex ? siblingIndex : siblingIndex + 1);
        }

        /// <summary>
        /// Gets the hierarchy of the current <see cref="Transform"/> and stores the result in the specified list.
        /// </summary>
        /// <param name="transform">This transform.</param>
        /// <param name="result">The list used to hold the results.</param>
        /// <exception cref="ArgumentNullException"><paramref name="transform"/> or <paramref name="result"/> is <see langword="null"/>.</exception>
        public static void GetHierarchies(this Transform transform, List<Transform> result)
        {
            if (!transform)
            {
                throw new ArgumentNullException(nameof(transform));
            }
            if (result is null)
            {
                throw new ArgumentNullException(nameof(result));
            }
            var list = PredefinedPools<Transform>.List.Get();
            try
            {
                do
                {
                    list.Add(transform);
                    transform = transform.parent;
                } while (transform);
                list.Reverse();
                result.AddRange(list);
            }
            finally
            {
                PredefinedPools<Transform>.List.Return(list);
            }
        }

        /// <summary>
        /// Gets the names of each transform level in the hierarchy of the current <see cref="Transform"/> and stores the result in the specified list.
        /// </summary>
        /// <param name="transform">This transform.</param>
        /// <param name="result">The list used to hold the results.</param>
        /// <exception cref="ArgumentNullException"><paramref name="transform"/> or <paramref name="result"/> is <see langword="null"/>.</exception>
        public static void GetHierarchyNames(this Transform transform, List<string> result)
        {
            if (!transform)
            {
                throw new ArgumentNullException(nameof(transform));
            }
            if (result is null)
            {
                throw new ArgumentNullException(nameof(result));
            }
            var list = PredefinedPools<string>.List.Get();
            try
            {
                do
                {
                    list.Add(transform.name);
                    transform = transform.parent;
                } while (transform);
                list.Reverse();
                result.AddRange(list);
            }
            finally
            {
                PredefinedPools<string>.List.Return(list);
            }
        }

        /// <summary>
        /// Gets the path of the current <see cref="Transform"/> in the scene.
        /// </summary>
        /// <param name="transform">This transform.</param>
        /// <returns>The path of this transform in the scene.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="transform"/> is <see langword="null"/>.</exception>
        public static string GetScenePath(this Transform transform)
        {
            if (!transform)
            {
                throw new ArgumentNullException(nameof(transform));
            }
            var list = PredefinedPools<string>.List.Get();
            try
            {
                do
                {
                    list.Add(transform.name);
                    transform = transform.parent;
                } while (transform);
                list.Reverse();
                return string.Join("/", list);
            }
            finally
            {
                PredefinedPools<string>.List.Return(list);
            }
        }

        /// <summary>
        /// Destroys all child transforms of the current <see cref="Transform"/>.
        /// </summary>
        /// <param name="transform">This transform.</param>
        /// <exception cref="ArgumentNullException"><paramref name="transform"/> is <see langword="null"/>.</exception>
        /// <remarks>Destruction is in reverse order, i.e. the last child transform is destroyed first and the first child transform is destroyed last.</remarks>
        public static void DestroyChildren(this Transform transform)
        {
            if (!transform)
            {
                throw new ArgumentNullException(nameof(transform));
            }
            for (var i = transform.childCount - 1; i >= 0; i--)
            {
                var childTransform  = transform.GetChild(i);
                var childGameObject = childTransform.gameObject;
                Object.Destroy(childGameObject);
            }
        }

        /// <summary>
        /// Immediately destroys all child transforms of the current <see cref="Transform"/>.
        /// </summary>
        /// <param name="transform">This transform.</param>
        /// <exception cref="ArgumentNullException"><paramref name="transform"/> is <see langword="null"/>.</exception>
        /// <remarks>Destruction is in reverse order, i.e. the last child transform is destroyed first and the first child transform is destroyed last.</remarks>
        public static void DestroyChildrenImmediate(this Transform transform)
        {
            if (!transform)
            {
                throw new ArgumentNullException(nameof(transform));
            }
            for (var i = transform.childCount - 1; i >= 0; i--)
            {
                var childTransform  = transform.GetChild(i);
                var childGameObject = childTransform.gameObject;
                Object.DestroyImmediate(childGameObject);
            }
        }
    }
}
