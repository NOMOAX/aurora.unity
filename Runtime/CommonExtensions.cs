using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Aurora.Unity
{
    /// <summary>
    /// 通用扩展方法。
    /// </summary>
    public static class CommonExtensions
    {
        /// <summary>
        /// 销毁当前 <see cref="List{T}"/> 中的所有对象，然后清空列表。如果列表中的成员是 <see cref="UnityEngine.Component"/> 类型，则改为销毁它所在的游戏物体。
        /// </summary>
        /// <param name="list">列表、</param>
        /// <typeparam name="T">列表中成员的类型。</typeparam>
        /// <exception cref="System.ArgumentNullException"><paramref name="list"/> 为 <see langword="null"/>。</exception>
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
        /// 立即销毁当前 <see cref="List{T}"/> 中的所有对象，然后清空列表。如果列表中的成员是 <see cref="UnityEngine.Component"/> 类型，则改为立即销毁它所在的游戏物体。
        /// </summary>
        /// <param name="list">列表、</param>
        /// <typeparam name="T">列表中成员的类型。</typeparam>
        /// <exception cref="System.ArgumentNullException"><paramref name="list"/> 为 <see langword="null"/>。</exception>
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
