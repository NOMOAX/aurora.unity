using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Aurora.Unity
{
    /// <summary>
    /// 提供一组成员，它们与 <see cref="Component"/> 的一些公开的实例成员具有相同的签名。
    /// </summary>
    public interface IComponent
    {
        /// <seealso cref="Component.gameObject"/>
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        GameObject gameObject { get; }

        /// <seealso cref="Component.transform"/>
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        Transform transform { get; }
    }
}
