using System;
using UnityEngine;

namespace Aurora.Unity
{
    /// <summary>
    /// 在游戏物体处于非激活状态时引发的异常。
    /// </summary>
    public class GameObjectInactiveException : UnityException
    {
        /// <summary>
        /// 游戏物体。
        /// </summary>
        public GameObject GameObject { get; }

        /// <summary>
        /// 使用指定的游戏物体初始化 <see cref="GameObjectInactiveException"/> 类的新实例。
        /// </summary>
        /// <param name="gameObject">游戏物体。</param>
        /// <exception cref="ArgumentNullException"><paramref name="gameObject"/> 为 <see langword="null"/>。</exception>
        public GameObjectInactiveException(GameObject gameObject)
        {
            if (gameObject is null)
            {
                throw new ArgumentNullException(nameof(gameObject));
            }
            GameObject = gameObject;
        }

        /// <summary>
        /// 使用指定的游戏物体和错误消息初始化 <see cref="GameObjectInactiveException"/> 类的新实例。
        /// </summary>
        /// <param name="gameObject">游戏物体。</param>
        /// <param name="message">描述错误的消息。</param>
        /// <exception cref="ArgumentNullException"><paramref name="gameObject"/> 为 <see langword="null"/>。</exception>
        public GameObjectInactiveException(GameObject gameObject, string message) : base(message)
        {
            if (gameObject is null)
            {
                throw new ArgumentNullException(nameof(gameObject));
            }
            GameObject = gameObject;
        }

        /// <summary>
        /// 使用指定的游戏物体、错误消息和内部异常初始化 <see cref="GameObjectInactiveException"/> 类的新实例。
        /// </summary>
        /// <param name="gameObject">游戏物体。</param>
        /// <param name="message">描述错误的消息。</param>
        /// <param name="innerException">造成此异常的异常。</param>
        /// <exception cref="ArgumentNullException"><paramref name="gameObject"/> 为 <see langword="null"/>。</exception>
        public GameObjectInactiveException(GameObject gameObject, string message, Exception innerException) : base(
            message,
            innerException
        )
        {
            if (gameObject is null)
            {
                throw new ArgumentNullException(nameof(gameObject));
            }
            GameObject = gameObject;
        }
    }
}
