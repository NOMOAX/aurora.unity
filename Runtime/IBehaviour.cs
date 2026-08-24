using UnityEngine;

namespace Aurora.Unity
{
    /// <summary>
    /// Provides a set of members that have the same signatures as some public instance members of <see cref="Behaviour"/>.
    /// </summary>
    public interface IBehaviour : IComponent
    {
        /// <seealso cref="Behaviour.enabled"/>
        // ReSharper disable InconsistentNaming
        bool enabled { get; set; }
        // ReSharper restore InconsistentNaming

        /// <seealso cref="Behaviour.isActiveAndEnabled"/>
        // ReSharper disable InconsistentNaming
        bool isActiveAndEnabled { get; }
        // ReSharper restore InconsistentNaming
    }
}
