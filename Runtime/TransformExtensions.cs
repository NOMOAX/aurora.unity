using System;
using System.Collections.Generic;
using Aurora.Pooling;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Aurora.Unity
{
    /// <summary>
    /// 为 <see cref="Transform"/> 类提供扩展方法。
    /// </summary>
    public static class TransformExtensions
    {
        /// <summary>
        /// 交换当前 <see cref="Transform"/> 与指定的兄弟变换的索引。
        /// </summary>
        /// <param name="transform">此变换.</param>
        /// <param name="sibling">兄弟变换。</param>
        /// <exception cref="ArgumentNullException"><paramref name="transform"/> 或 <paramref name="sibling"/> 为 <see langword="null"/>。</exception>
        /// <exception cref="ArgumentException"><paramref name="transform"/> 与 <paramref name="sibling"/> 不是兄弟关系。</exception>
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
        /// 设置当前 <see cref="Transform"/> 的索引，使得它的索引刚好位于指定的兄弟变换前。
        /// </summary>
        /// <param name="transform">此变换.</param>
        /// <param name="sibling">兄弟变换。</param>
        /// <exception cref="ArgumentNullException"><paramref name="transform"/> 或 <paramref name="sibling"/> 为 <see langword="null"/>。</exception>
        /// <exception cref="ArgumentException"><paramref name="transform"/> 与 <paramref name="sibling"/> 不是兄弟关系，或者 <paramref name="transform"/> 与 <paramref name="sibling"/> 为相同实例。</exception>
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
        /// 设置当前 <see cref="Transform"/> 的索引，使得它的索引刚好位于指定的兄弟变换后。
        /// </summary>
        /// <param name="transform">此变换.</param>
        /// <param name="sibling">兄弟变换。</param>
        /// <exception cref="ArgumentNullException"><paramref name="transform"/> 或 <paramref name="sibling"/> 为 <see langword="null"/>。</exception>
        /// <exception cref="ArgumentException"><paramref name="transform"/> 与 <paramref name="sibling"/> 不是兄弟关系，或者 <paramref name="transform"/> 与 <paramref name="sibling"/> 为相同实例。</exception>
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
        /// 获取当前 <see cref="Transform"/> 的层级，并将结果存入指定的列表。
        /// </summary>
        /// <param name="transform">此变换.</param>
        /// <param name="result">用于存放结果的列表。</param>
        /// <exception cref="ArgumentNullException"><paramref name="transform"/> 或 <paramref name="result"/> 为 <see langword="null"/>。</exception>
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
        /// 获取当前 <see cref="Transform"/> 的层级中各层变换的名称，并将结果存入指定的列表。
        /// </summary>
        /// <param name="transform">此变换.</param>
        /// <param name="result">用于存放结果的列表。</param>
        /// <exception cref="ArgumentNullException"><paramref name="transform"/> 或 <paramref name="result"/> 为 <see langword="null"/>。</exception>
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
        /// 获取当前 <see cref="Transform"/> 的全路径名称。
        /// </summary>
        /// <param name="transform">此变换。</param>
        /// <returns>这个变换的全路径名称。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="transform"/> 为 <see langword="null"/>。</exception>
        public static string GetFullName(this Transform transform)
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
        /// 销毁当前 <see cref="Transform"/> 的所有子变换。
        /// </summary>
        /// <param name="transform">此变换。</param>
        /// <exception cref="ArgumentNullException"><paramref name="transform"/> 为 <see langword="null"/>。</exception>
        /// <remarks>销毁按倒序进行，即首先销毁最后的子变换，最后销毁第一个子变换。</remarks>
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
        /// 立即销毁当前 <see cref="Transform"/> 的所有子变换。
        /// </summary>
        /// <param name="transform">此变换。</param>
        /// <exception cref="ArgumentNullException"><paramref name="transform"/> 为 <see langword="null"/>。</exception>
        /// <remarks>销毁按倒序进行，即首先销毁最后的子变换，最后销毁第一个子变换。</remarks>
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
