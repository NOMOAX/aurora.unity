using System;
using UnityEngine;

namespace Aurora.Unity
{
    /// <summary>
    /// 用于修饰继承 <see cref="SingletonBehaviour{T}"/> 类的类型，以在首次获取或创建 <see cref="SingletonBehaviour{T}.Instance"/> 时对该单一实例设置 <see cref="UnityEngine.Object.hideFlags"/>。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class WithHideFlagsAttribute : Attribute
    {
        /// <summary>
        /// 隐藏标记。
        /// </summary>
        public readonly HideFlags HideFlags;

        /// <summary>
        /// 初始化 <see cref="WithHideFlagsAttribute"/> 类的新示例。
        /// </summary>
        /// <param name="hideFlags">隐藏标记。</param>
        public WithHideFlagsAttribute(HideFlags hideFlags)
        {
            HideFlags = hideFlags;
        }
    }
}
