using UnityEngine;

namespace Aurora.Unity
{
    /// <summary>
    /// Provides a set of members that have the same signatures as some public instance members of <see cref="Component"/>.
    /// </summary>
    public interface IComponent
    {
        /// <seealso cref="Component.gameObject"/>
        // ReSharper disable InconsistentNaming
        GameObject gameObject { get; }
        // ReSharper restore InconsistentNaming

        /// <seealso cref="Component.transform"/>
        // ReSharper disable InconsistentNaming
        Transform transform { get; }
        // ReSharper restore InconsistentNaming
    }
}
