using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Aurora.Unity
{
    /// <summary>
    /// Provides extension methods for the <see cref="Component"/> class.
    /// </summary>
    public static class ComponentExtensions
    {
        /// <summary>
        /// Gets or adds a component.
        /// </summary>
        /// <param name="component">The component.</param>
        /// <typeparam name="T">The type of the component to get or add.</typeparam>
        /// <returns>The component that was gotten or added.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentNullException"><paramref name="component"/> is <see langword="null"/>.</exception>
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
        /// Removes a component.
        /// </summary>
        /// <param name="component">The component.</param>
        /// <typeparam name="T">The type of the component to remove.</typeparam>
        /// <exception cref="ArgumentNullException"><paramref name="component"/> is <see langword="null"/>.</exception>
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
