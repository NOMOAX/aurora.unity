using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Aurora.Unity
{
    /// <summary>
    /// 为 <see cref="Component"/> 类提供扩展方法。
    /// </summary>
    public static class ComponentExtensions
    {
        /// <summary>
        /// 获取或添加组件。
        /// </summary>
        /// <param name="component">组件。</param>
        /// <typeparam name="T">要获取或添加的组件的类型。</typeparam>
        /// <returns>获取到的或添加的组件。</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentNullException"><paramref name="component"/> 为 <see langword="null"/>。</exception>
        public static T GetOrAddComponent<T>(this Component component) where T : Component
        {
            if (!component)
            {
                throw new ArgumentNullException(nameof(component));
            }
            return component.gameObject.TryGetComponent<T>(out var result)
                       ? result
                       : component.gameObject.AddComponent<T>();
        }

        /// <summary>
        /// 移除组件。
        /// </summary>
        /// <param name="component">组件。</param>
        /// <typeparam name="T">要移除的组件的类型。</typeparam>
        /// <exception cref="ArgumentNullException"><paramref name="component"/> 为 <see langword="null"/>。</exception>
        public static void RemoveComponent<T>(this Component component) where T : Component
        {
            if (!component)
            {
                throw new ArgumentNullException(nameof(component));
            }
            if (component.gameObject.TryGetComponent<T>(out var result))
            {
                Object.Destroy(result);
            }
        }
    }
}
