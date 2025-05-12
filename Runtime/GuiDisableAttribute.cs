using System;
using UnityEngine;

namespace Aurora.Unity
{
    /// <summary>
    /// 禁用。
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class GuiDisableAttribute : PropertyAttribute
    {
    }
}
