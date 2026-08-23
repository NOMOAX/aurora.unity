using System;
using UnityEngine;

namespace Aurora.Unity
{
    /// <summary>
    /// Selects a tag from a dropdown.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class TagAttribute : PropertyAttribute
    {
    }
}
