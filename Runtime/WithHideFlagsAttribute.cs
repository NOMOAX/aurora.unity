using System;
using UnityEngine;

namespace Aurora.Unity
{
    /// <summary>
    /// Marks a type that inherits from <see cref="SingletonBehaviour{T}"/> so that <see cref="UnityEngine.Object.hideFlags"/> is set on the single instance when <see cref="SingletonBehaviour{T}.Instance"/> is first retrieved or created.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class WithHideFlagsAttribute : Attribute
    {
        /// <summary>
        /// The hide flags.
        /// </summary>
        public readonly HideFlags HideFlags;

        /// <summary>
        /// Initializes a new instance of the <see cref="WithHideFlagsAttribute"/> class.
        /// </summary>
        /// <param name="hideFlags">The hide flags.</param>
        public WithHideFlagsAttribute(HideFlags hideFlags)
        {
            HideFlags = hideFlags;
        }
    }
}
