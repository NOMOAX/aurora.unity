using System;
using UnityEngine;

namespace Aurora.Unity
{
    /// <summary>
    /// 使用下拉框选择标签。
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class TagAttribute : PropertyAttribute
    {
    }
}
