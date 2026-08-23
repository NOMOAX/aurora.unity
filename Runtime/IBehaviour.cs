using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Aurora.Unity
{
    /// <summary>
    /// Provides a set of members that have the same signatures as some public instance members of <see cref="Behaviour"/>.
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
