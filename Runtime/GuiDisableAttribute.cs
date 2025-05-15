using System;
using UnityEngine;

namespace Aurora.Unity
{
    /// <summary>
    /// 被默认绘制器绘制时，呈禁用状态。
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class GuiDisableAttribute : PropertyAttribute
    {
    }
}
