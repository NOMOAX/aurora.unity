using System;
using UnityEngine;

namespace Aurora.Unity
{
    /// <summary>
    /// 以禁用状态绘制序列化属性。
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class GuiDisableAttribute : PropertyAttribute
    {
        /// <summary>
        /// 何时以禁用状态绘制序列化属性。
        /// </summary>
        public readonly When When;

        /// <summary>
        /// 初始化 <see cref="GuiDisableAttribute"/> 类的新实例。
        /// </summary>
        public GuiDisableAttribute()
        {
            When = When.Always;
        }

        /// <summary>
        /// 指定何时以禁用状态绘制序列化属性，初始化 <see cref="GuiDisableAttribute"/> 类的新实例。
        /// </summary>
        /// <param name="when">何时以禁用状态绘制序列化属性。</param>
        public GuiDisableAttribute(When when)
        {
            When = when;
        }
    }
}
