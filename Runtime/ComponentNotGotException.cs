using System;
using UnityEngine;

namespace Aurora.Unity
{
    /// <summary>
    /// 在无法获取到指定类型的组件时引发的异常。
    /// </summary>
    public class ComponentNotGotException : UnityException
    {
        /// <summary>
        /// 游戏物体。
        /// </summary>
        public GameObject GameObject { get; }

        /// <summary>
        /// 从游戏物体获取组件的方法。
        /// </summary>
        public GetComponentMethod Method { get; }

        /// <summary>
        /// 组件类型。
        /// </summary>
        public Type ComponentType { get; }

        /// <summary>
        /// 使用指定的游戏物体、从游戏物体获取组件的方法和组件类型初始化 <see cref="GameObjectInactiveException"/> 类的新实例。
        /// </summary>
        /// <param name="gameObject">游戏物体。</param>
        /// <param name="method">从游戏物体获取组件的方法。</param>
        /// <param name="componentType">组件类型。</param>
        /// <exception cref="ArgumentNullException"><paramref name="gameObject"/> 或 <paramref name="componentType"/> 为 <see langword="null"/>。</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="componentType"/> 既不是接口类型，又不是 <seealso cref="Component"/> 类型或其子类型。</exception>
        public ComponentNotGotException(GameObject gameObject, GetComponentMethod method, Type componentType)
        {
            if (gameObject is null)
            {
                throw new ArgumentNullException(nameof(gameObject));
            }
            if (componentType is null)
            {
                throw new ArgumentNullException(nameof(componentType));
            }
            if (!componentType.IsInterface || componentType != typeof(Component) ||
                componentType.IsSubclassOf(typeof(Component)))
            {
                throw new ArgumentOutOfRangeException(nameof(componentType), componentType, null);
            }
            GameObject    = gameObject;
            Method        = method;
            ComponentType = componentType;
        }

        /// <summary>
        /// 使用指定的游戏物体、从游戏物体获取组件的方法、组件类型和错误消息初始化 <see cref="GameObjectInactiveException"/> 类的新实例。
        /// </summary>
        /// <param name="gameObject">游戏物体。</param>
        /// <param name="method">从游戏物体获取组件的方法。</param>
        /// <param name="message">描述错误的消息。</param>
        /// <param name="componentType">组件类型。</param>
        /// <exception cref="ArgumentNullException"><paramref name="gameObject"/> 或 <paramref name="componentType"/> 为 <see langword="null"/>。</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="componentType"/> 既不是接口类型，又不是 <seealso cref="Component"/> 类型或其子类型。</exception>
        public ComponentNotGotException(
            GameObject         gameObject,
            GetComponentMethod method,
            string             message,
            Type               componentType) : base(message)
        {
            if (gameObject is null)
            {
                throw new ArgumentNullException(nameof(gameObject));
            }
            if (componentType is null)
            {
                throw new ArgumentNullException(nameof(componentType));
            }
            if (!componentType.IsInterface || componentType != typeof(Component) ||
                componentType.IsSubclassOf(typeof(Component)))
            {
                throw new ArgumentOutOfRangeException(nameof(componentType), componentType, null);
            }
            GameObject    = gameObject;
            Method        = method;
            ComponentType = componentType;
        }
    }
}
