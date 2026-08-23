using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Aurora.Unity
{
    /// <summary>
    /// Common extension methods.
    /// </summary>
    public static class CommonExtensions
    {
        /// <summary>
        /// Destroys all objects in the current <see cref="List{T}"/> and then clears the list. If a member is a <see cref="UnityEngine.Component"/> type, destroys the game object it belongs to instead.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <typeparam name="T">The type of the list members.</typeparam>
        /// <exception cref="System.ArgumentNullException"><paramref name="list"/> is <see langword="null"/>.</exception>
        public static void ClearAndDestroy<T>(this List<T> list) where T : Object
        {
            if (list is null)
            {
                throw new ArgumentNullException(nameof(list));
            }
            foreach (var element in list)
            {
                switch (element)
                {
                    case null:
                        break;
                    case Component component:
                        Object.Destroy(component.gameObject);
                        break;
                    default:
                        Object.Destroy(element);
                        break;
                }
            }
            list.Clear();
        }

        /// <summary>
        /// Immediately destroys all objects in the current <see cref="List{T}"/> and then clears the list. If a member is a <see cref="UnityEngine.Component"/> type, immediately destroys the game object it belongs to instead.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <typeparam name="T">The type of the list members.</typeparam>
        /// <exception cref="System.ArgumentNullException"><paramref name="list"/> is <see langword="null"/>.</exception>
        public static void ClearAndDestroyImmediate<T>(this List<T> list) where T : Object
        {
            if (list is null)
            {
                throw new ArgumentNullException(nameof(list));
            }
            foreach (var element in list)
            {
                switch (element)
                {
                    case null:
                        break;
                    case Component component:
                        Object.DestroyImmediate(component.gameObject);
                        break;
                    default:
                        Object.DestroyImmediate(element);
                        break;
                }
            }
            list.Clear();
        }
    }
}
