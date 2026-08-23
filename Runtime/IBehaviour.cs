using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Aurora.Unity
{
    /// <summary>
    /// 提供一组成员，它们与 <see cref="Behaviour"/> 的一些公开的实例成员具有相同的签名。
    /// </summary>
    public interface IBehaviour : IComponent
    {
        /// <seealso cref="Behaviour.enabled"/>
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        bool enabled { get; set; }

        /// <seealso cref="Behaviour.isActiveAndEnabled"/>
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        bool isActiveAndEnabled { get; }
    }
}
