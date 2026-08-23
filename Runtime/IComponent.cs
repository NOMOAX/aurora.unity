using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Aurora.Unity
{
    /// <summary>
    /// Provides a set of members that have the same signatures as some public instance members of <see cref="Component"/>.
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
