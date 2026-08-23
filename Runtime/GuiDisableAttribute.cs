using System;
using UnityEngine;

namespace Aurora.Unity
{
    /// <summary>
    /// Draws a serialized property in a disabled state.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class GuiDisableAttribute : PropertyAttribute
    {
        /// <summary>
        /// When to draw the serialized property in a disabled state.
        /// </summary>
        public readonly When When;

        /// <summary>
        /// Initializes a new instance of the <see cref="GuiDisableAttribute"/> class.
        /// </summary>
        public GuiDisableAttribute()
        {
            When = When.Always;
        }

        /// <summary>
        /// Specifies when to draw the serialized property in a disabled state, initializing a new instance of the <see cref="GuiDisableAttribute"/> class.
        /// </summary>
        /// <param name="when">When to draw the serialized property in a disabled state.</param>
        public GuiDisableAttribute(When when)
        {
            When = when;
        }
    }
}
