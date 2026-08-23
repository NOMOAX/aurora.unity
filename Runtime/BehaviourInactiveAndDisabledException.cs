using System;
using UnityEngine;

namespace Aurora.Unity
{
    /// <summary>
    /// 在行为所关联的游戏物体处于未激活状态并且行为禁用时引发的异常。
    /// </summary>
    public class BehaviourInactiveAndDisabledException : UnityException
    {
        /// <summary>
        /// 行为。
        /// </summary>
        public Behaviour Behaviour { get; }

        /// <summary>
        /// 使用指定的行为初始化 <see cref="BehaviourInactiveAndDisabledException"/> 类的新实例。
        /// </summary>
        /// <param name="behaviour">行为。</param>
        /// <exception cref="ArgumentNullException"><paramref name="behaviour"/> 为 <see langword="null"/>。</exception>
        public BehaviourInactiveAndDisabledException(Behaviour behaviour)
        {
            if (behaviour is null)
            {
                throw new ArgumentNullException(nameof(behaviour));
            }
            Behaviour = behaviour;
        }

        /// <summary>
        /// 使用指定的行为和错误消息初始化 <see cref="BehaviourInactiveAndDisabledException"/> 类的新实例。
        /// </summary>
        /// <param name="behaviour">行为。</param>
        /// <param name="message">描述错误的消息。</param>
        /// <exception cref="ArgumentNullException"><paramref name="behaviour"/> 为 <see langword="null"/>。</exception>
        public BehaviourInactiveAndDisabledException(Behaviour behaviour, string message) : base(message)
        {
            if (behaviour is null)
            {
                throw new ArgumentNullException(nameof(behaviour));
            }
            Behaviour = behaviour;
        }

        /// <summary>
        /// 使用指定的行为、错误消息和内部异常初始化 <see cref="BehaviourInactiveAndDisabledException"/> 类的新实例。
        /// </summary>
        /// <param name="behaviour">行为。</param>
        /// <param name="message">描述错误的消息。</param>
        /// <param name="innerException">造成此异常的异常。</param>
        public BehaviourInactiveAndDisabledException(Behaviour behaviour, string message, Exception innerException) :
            base(message, innerException)
        {
            if (behaviour is null)
            {
                throw new ArgumentNullException(nameof(behaviour));
            }
            Behaviour = behaviour;
        }
    }
}
